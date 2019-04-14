using Storage.Core.Abstractions;
using Storage.Core.Configuration;
using Storage.Core.FileNamingStrategies;
using Storage.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Storage.Core
{
    /*
	 * Зона ответственности: управление списком DataPage, обновление индекса при вставке, управление созданием новых страничек, метод для очистки старых.
	 * Загрузка из диска индексов и имеющихся страниц данных (карта)
	 */
    // TODO: DataPageManager должен сам менеджить RecordId и инкрементить, а сверху прилетать только массивы байт.
    /// <summary>
    /// Менеджер страниц данных.
    /// </summary>
    public sealed class DataPageManager : IDisposable
    {
        #region Поля

        /// <summary>
        /// Список страниц данных.
        /// </summary>
        private readonly ConcurrentDictionary<int, DataPage> _dataPages;

        /// <summary>
        /// Индекс по <see cref="DataRecord.Id" />
        /// </summary>
        private readonly IDataRecordIndexStore _dataRecordIndexStore;

        /// <summary>
        /// Конфигурация менеджера страниц.
        /// </summary>
        private readonly DataPageManagerConfig _config;

        /// <summary>
        /// Стратегия формирования названия файлов.
        /// </summary>
        private readonly IFileNamingStrategy _dataPageFileNamingStrategy;

        /// <summary>
        /// Лок объект для создания страницы.
        /// </summary>
        private readonly object _createDataPageLock = new object();

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Название менеджера страниц.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Количество страниц.
        /// </summary>
        public int DataPagesCount => _dataPages.Count;

        /// <summary>
        /// Номер следующей страницы.
        /// </summary>
        private int NextDataPageNumber => _dataPages.IsEmpty
            ? 1
            : _dataPages.Keys.Max() + 1;

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создает менеджера страниц данных.
        /// </summary>
        public DataPageManager(DataPageManagerConfig config)
        {
            _config = config;
            if (!Directory.Exists(_config.Directory))
            {
                Directory.CreateDirectory(_config.Directory);
            }

            Name = config.Name;
            _dataPageFileNamingStrategy = new HierarchyFileNamingStrategy(_config.Directory);
            _dataPages = new ConcurrentDictionary<int, DataPage>();
            _dataRecordIndexStore = new DataRecordIndexStore(_config.Directory);
            LoadMetaData();
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Сохранить данные на диск.
        /// </summary>
        /// <param name="record">Модель данных.</param>
        public void Save(DataRecord record)
        {
            var data = record.GetBytes();
            var dataLength = data.Length;

            // пишем последовательно, поэтому получаем всегда для записи последнюю страницу.
            var currentDataPage = GetLastPage();
            // узнаем, сколько можно записать в текущую страницу:
            var freeSpaceLength = currentDataPage.GetFreeSpaceLength();

            // если хватает места для записи на одной странице
            if (freeSpaceLength > dataLength)
            {
                // записали на диск.
                currentDataPage.TrySaveData(data, out var dataOffset);
                // пишем в индекс
                _dataRecordIndexStore.AddToIndex(new DataRecordIndexPointer(
                        record.Id,
                        currentDataPage.PageId,
                        dataOffset, dataLength
                    )
                );

                return;
            }

            // если же места не хватило на одной странице, то уже пишем в несколько:
            var dataRecordIndexPointers = new List<DataRecordIndexPointer>();
            var writtenBytes = 0;
            var dataSpan = data.AsSpan();

            // заполним оставшееся место в текущей странице 
            if (currentDataPage.TrySaveData(dataSpan.Slice(0, freeSpaceLength).ToArray(), out var offset))
            {
                dataRecordIndexPointers.Add(new DataRecordIndexPointer(
                        record.Id,
                        currentDataPage.PageId,
                        offset,
                        freeSpaceLength
                    )
                );
            }

            writtenBytes += freeSpaceLength;

            // и до тех пор, пока все не будет записано...
            while (writtenBytes < dataLength)
            {
                // создадим новую страницу.
                currentDataPage = CreateNew();

                var bytesToWrite = dataLength - writtenBytes > _config.PageSize
                    ? _config.PageSize
                    : dataLength - writtenBytes;

                // создаем разрез для записи.
                var slice = dataSpan.Slice(writtenBytes, bytesToWrite);

                // сохраняем.
                currentDataPage.TrySaveData(slice.ToArray(), out offset);
                dataRecordIndexPointers.Add(new DataRecordIndexPointer(
                        record.Id,
                        currentDataPage.PageId,
                        offset,
                        slice.Length
                    )
                );
                writtenBytes += slice.Length;
            }

            var index = dataRecordIndexPointers.First();
            _dataRecordIndexStore.AddToIndex(new DataRecordIndexPointer(
                    index.DataRecordId,
                    index.DataPageNumber,
                    index.Offset,
                    index.Length,
                    dataRecordIndexPointers.Skip(1).ToArray()
                )
            );
        }

        /// <summary>
        /// Получить контейнер по идентификатору.
        /// </summary>
        /// <param name="recordId">Идентификатор записи.</param>
        /// <returns>Контейнер данных.</returns>
        public DataRecord Read(long recordId)
        {
            if (!_dataRecordIndexStore.TryGetIndex(recordId, out var index))
            {
                // TODO: implement FULL SCAN (для мультистраничников читать до тех пор, пока номер записи не изменится.
                // можно сделать счетчик индекс миссов, и если по-какому то из dataPage слишком много миссов, то нужно перестроить индекс, побыстрому в фоне.
                // DataPageHeader.DataRecordStartId добавить в заголовок и поиск упрощается в разы:
                // _dataPages.FirstOrDefault(dp => dp.PageId.DataRecordStartId >= dataRecordId) и сканить уже по найденной странице. Если не удалось и так найти.. то тут косяк, надо бежать по всем страницам...)

                return null;
            }

            return new DataRecord(GetDataRecordBytes(index));
        }

        // TODO: тесты.
        /// <summary>
        /// Получить <see cref="IEnumerable{DataRecord}"/> с указанного номера записи.
        /// </summary>
        /// <param name="fromRecordId">Идентификатор записи, с которого нужно получить итератор.</param>
        /// <returns><see cref="IEnumerable{TDataRecord}"/> с указанного номера записи.</returns>
        public IEnumerable<DataRecord> AsEnumerable(long fromRecordId)
        {
            return _dataRecordIndexStore.AsEnumerable(fromRecordId)
                .Select(recordIndexPointer => new DataRecord(GetDataRecordBytes(recordIndexPointer)));
        }

        /// <summary>
        /// Высвобождает неуправляемые ресурсы.
        /// </summary>
        public void Dispose()
        {
            _dataRecordIndexStore?.Dispose();
            foreach (var dataPage in _dataPages)
            {
                dataPage.Value?.Dispose();
            }
        }

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Прочитать (собрать) массив байт записи по индексу.
        /// </summary>
        /// <param name="recordIndexPointer">Указатель на данные на странице.</param>
        /// <returns>Массив байт записи.</returns>
        private byte[] GetDataRecordBytes(in DataRecordIndexPointer recordIndexPointer)
        {
            var dataPage = GetDataPage(recordIndexPointer.DataPageNumber);
            if (!recordIndexPointer.AdditionalDataRecordIndexPointers.Any())
            {
                return dataPage.ReadBytes(recordIndexPointer.Offset, recordIndexPointer.Length);
            }

            using (var memoryStream = new MemoryStream())
            {
                var bytes = dataPage.ReadBytes(recordIndexPointer.Offset, recordIndexPointer.Length);
                memoryStream.Write(bytes, 0, bytes.Length);

                // пробегаемся по всем указателям, собираем тело записи.
                foreach (var pointer in recordIndexPointer.AdditionalDataRecordIndexPointers.OrderBy(p => p.DataPageNumber))
                {
                    // получаем страницу.
                    var dp = GetDataPage(pointer.DataPageNumber);
                    // читаем данные.
                    var recordPiece = dp.ReadBytes(pointer.Offset, pointer.Length);
                    // пишем в memoryStream для аггрегации.
                    memoryStream.Write(recordPiece, 0, recordPiece.Length);
                }

                // возвращаем агреггированный массив байт.
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Создать новую страницу.
        /// </summary>
        /// <returns>Новая страница.</returns>
        private DataPage CreateNew()
        {
            lock (_createDataPageLock)
            {
                var dataPageNumber = NextDataPageNumber;
                var fileName = _dataPageFileNamingStrategy.GetFileNameFor(dataPageNumber);
                var dataPage = new DataPage(_config.DataPageConfig, dataPageNumber, fileName, false);

                AddDataPage(dataPage);

                return dataPage;
            }
        }

        /// <summary>
        /// Добавить страницу в словарь.
        /// </summary>
        /// <param name="page">Страница данных.</param>
        private void AddDataPage(DataPage page)
        {
            while (!_dataPages.TryAdd(page.PageId, page))
            {
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Загрузить в память метаданные страниц.
        /// </summary>
        private void LoadMetaData()
        {
            lock (_createDataPageLock)
            {
                var files = _dataPageFileNamingStrategy.GetFiles();

                foreach (var file in files.Reverse().Skip(1))
                {
                    AddDataPage(new DataPage(_config.DataPageConfig, file.Key, file.Value, true));
                }

                if (files.Any())
                {
                    var lastDataPage = files.Last();
                    AddDataPage(new DataPage(_config.DataPageConfig, lastDataPage.Key, lastDataPage.Value, false));
                }
            }
        }

        /// <summary>
        /// Получить страницу данных по номеру.
        /// </summary>
        /// <param name="pageId">Номер страницы.</param>
        /// <returns>Страница данных.</returns>
        private DataPage GetDataPage(int pageId)
        {
            if (_dataPages.TryGetValue(pageId, out var dataPage))
            {
                return dataPage;
            }

            return TryLoadPage(pageId, out dataPage)
                ? dataPage
                : throw new Exception(); // TODO: feels bad man.. такой ситуации в принципе быть не должно.
        }

        /// <summary>
        /// Попытаться найти указанную страницу и загрузить в память мета информацию о ней.
        /// </summary>
        /// <param name="pageId">Номер страницы.</param>
        /// <param name="page">Страница данных.</param>
        /// <returns>True, если удалось найти файл и загрузить мета информацию.</returns>
        private bool TryLoadPage(int pageId, out DataPage page)
        {
            page = default;

            var fileName = _dataPageFileNamingStrategy.GetFileNameFor(pageId);
            if (!File.Exists(fileName))
            {
                return false;
            }

            // считаем что страница завершена, так как не завершенная всегда одна и её грузим в память при инициализации менеджера.
            page = new DataPage(_config.DataPageConfig, pageId, fileName, true);

            AddDataPage(page);

            return true;
        }

        /// <summary>
        /// Получить последнюю страницу данных.
        /// </summary>
        /// <returns>Страница данных.</returns>
        private DataPage GetLastPage()
        {
            return _dataPages.IsEmpty
                ? CreateNew()
                : _dataPages.LastOrDefault().Value;
        }

        #endregion Методы (private)
    }
}