using Microsoft.Extensions.ObjectPool;
using Storage.Core.Configuration;
using Storage.Core.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace Storage.Core.Models
{
    /*
	 *  Зона ответственности: чтение с потока, запись в поток.
	 */

    // TODO: сделать Destroy (т.е. уничтожение безвозвратное)

    /// <summary>
    /// Страница данных.
    /// </summary>
    public class DataPage : IDisposable
    {
        #region Поля, свойства

        /// <summary>
        /// Конфигурация страницы.
        /// </summary>
        private readonly DataPageConfig _config;

        /// <summary>
        /// Поток для записи данных в файл.
        /// <para>
        /// Инициализируется, только если в файл можно записать хоть какие-то данные.
        /// </para>
        /// </summary>
        private BufferedFileWriter _bufferedFileWriter;

        /// <summary>
        /// Название файла.
        /// </summary>
        private readonly string _fileName;

        /// <summary>
        /// Пул читателей файла.
        /// </summary>
        private readonly DefaultObjectPool<FileReaderUnit> _fileReaderUnitsPool;

        /// <summary>
        /// Объект синхронизации для записи в страницу.
        /// </summary>
        private readonly object _writeSyncObject = new object();

        /// <summary>
        /// Локальный (в рамках страницы) индекс записей.
        /// </summary>
        private readonly List<DataPageLocalIndex> _localItems = new List<DataPageLocalIndex>();

        /// <summary>
        /// Заголовок страницы.
        /// </summary>
        private DataPageHeader _header;

        // TODO: тесты на корректность записи индексов.
        /// <summary>
        /// Локальный индекс данных на странице.
        /// </summary>
        public IReadOnlyCollection<DataPageLocalIndex> LocalItems => _localItems;

        /// <summary>
        /// Номер страницы.
        /// </summary>
        public int PageId { get; }

        /// <summary>
        /// Дата/время последнего обращения к странице.
        /// </summary>
        /// <remarks>Используется для автоматической выгрузки из памяти, если страница некоторое время не была нужна.</remarks>
        public DateTime LastActiveTime { get; private set; }

        #endregion Поля, свойства

        #region Конструктор

        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="config">Конфигурация страницы.</param>
        /// <param name="pageId">Номер страницы.</param>
        /// <param name="fileName">Название файла.</param>
        /// <param name="isCompleted">Страница завершена?.</param>
        public DataPage(DataPageConfig config, int pageId, string fileName, bool isCompleted)
        {
            LastActiveTime = DateTime.UtcNow;
            _config = config;
            _fileName = fileName;
            PageId = pageId;
            Initialize(isCompleted);
            _fileReaderUnitsPool = new DefaultObjectPool<FileReaderUnit>(
                new FileReaderUnitPooledObjectPolicy(_fileName, config.ReadBufferSize),
                config.MaxReaderCount
            );
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Высвобождает неуправляемые ресурсы.
        /// </summary>
        public void Dispose()
        {
            // TODO: uncache.
            _bufferedFileWriter?.Dispose();
        }

        /// <summary>
        /// Проверить на наличие достаточного места для записи указанного количества байт.
        /// </summary>
        /// <param name="length">Размер байт, который требуется записать.</param>
        /// <returns>True, если достаточно места для записи указанного количества байт.</returns>
        public bool HasEnoughSpaceFor(int length)
        {
            return GetFreeSpaceFor(length) == length;
        }

        /// <summary>
        /// Получить общее кол-во байт, которое доступно для записи.
        /// </summary>
        /// <returns>Количество байт, которое можно записать на данную страницу.</returns>
        public int GetFreeSpaceLength()
        {
            return _header.UpperOffset - _header.LowerOffset;
        }

        /// <summary>
        /// Получить количество байт, которое можно записать на данную страницу.
        /// </summary>
        /// <returns>Количество байт, которое можно записать на данную страницу.</returns>
        public int GetFreeSpaceFor(int length)
        {
            var freeSpace = _header.UpperOffset - _header.LowerOffset;

            // Если места недостаточно даже чтобы записать указатель, возвращаем 0.
            if (freeSpace < DataPageLocalIndex.Size)
            {
                return 0;
            }

            // если свободного места хватает на запись и указателей и данных, возвращаем, всю длину.
            if (freeSpace > DataPageLocalIndex.Size + length)
            {
                return length;
            }

            // в противном случае, резервируем место под указатель, а остальное под запись. 
            return freeSpace - DataPageLocalIndex.Size;
        }

        /// <summary>
        /// Попытаться записать данные на страницу.
        /// </summary>
        /// <param name="recordId">Идентификатор записи.</param>
        /// <param name="data">Массив байт для записи.</param>
        /// <param name="offset">Сдвиг, на котором находятся данные.</param>
        /// <returns>True, если удалось записать данные.</returns>
        public bool TrySaveData(long recordId, byte[] data, out int offset)
        {
            // Обновляем время последней активности
            LastActiveTime = DateTime.UtcNow;
            offset = 0;
            try
            {
                lock (_writeSyncObject)
                {
                    _header.UpperOffset -= data.Length;
                    _header.LowerOffset += DataPageLocalIndex.Size;

                    offset = _header.UpperOffset;

                    _bufferedFileWriter.SetPosition(offset);
                    _bufferedFileWriter.Write(data, 0, data.Length);

                    AddToLocalIndex(recordId, offset, data.Length);
                    UpdateHeader();

                    if (GetFreeSpaceLength() <= DataPageLocalIndex.Size)
                    {
                        SetCompleted();
                    }
                }

                return true;
            }
            catch (Exception)
            {
                // TODO: log error.
                return false;
            }
        }

        /// <summary>
        /// Записать изменения в заголовке в файл.
        /// </summary>
        private void UpdateHeader()
        {
            _bufferedFileWriter.SetPosition(0);
            _bufferedFileWriter.Write(_header.GetBytes(), 0, DataPageHeader.Size);
        }

        /// <summary>
        /// Записать локальный индекс.
        /// </summary>
        /// <param name="recordId">Идентификатор записи.</param>
        /// <param name="offset">Сдвиг.</param>
        /// <param name="length">Длина.</param>
        private void AddToLocalIndex(long recordId, int offset, int length)
        {
            var dataPageLocalIndex = new DataPageLocalIndex(recordId, offset, length);
            _bufferedFileWriter.SetPosition(DataPageHeader.Size + _localItems.Count * DataPageLocalIndex.Size);
            _bufferedFileWriter.Write(dataPageLocalIndex.GetBytes(), 0, DataPageLocalIndex.Size);

            _localItems.Add(dataPageLocalIndex);
        }

        /// <summary>
        /// Прочитать запись.
        /// </summary>
        /// <param name="offset">Сдвиг.</param>
        /// <param name="length">Номер записи.</param>
        /// <returns>Запись.</returns>
        public DataRecord Read(int offset, int length)
        {
            return new DataRecord(ReadBytes(offset, length));
        }

        /// <summary>
        /// Получить массив байт.
        /// </summary>
        /// <param name="offset">Сдвиг.</param>
        /// <param name="length">Длина для чтения.</param>
        /// <returns>Массив байт.</returns>
        public byte[] ReadBytes(int offset, int length)
        {
            LastActiveTime = DateTime.UtcNow;

            var fileReaderUnit = _fileReaderUnitsPool.Get();
            try
            {
                fileReaderUnit.Reader.BaseStream.Position = offset;

                // TODO: cache record
                return fileReaderUnit.Reader.ReadBytes(length);
            }
            finally
            {
                _fileReaderUnitsPool.Return(fileReaderUnit);
            }
        }

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Единый метод для инициализации страницы.
        /// </summary>
        /// <param name="isCompleted">Является ли страница завершенной?</param>
        private void Initialize(bool isCompleted)
        {
            var fileInfo = new FileInfo(_fileName);

            if (!Directory.Exists(_fileName))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_fileName));
            }

            var fileStream = new FileStream(
                _fileName,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.ReadWrite,
                _config.BufferSize,
                FileOptions.SequentialScan
            );

            // Если файл не найден, или файл есть, но он не завершенный
            if (!fileInfo.Exists || !isCompleted)
            {
                // если длина оказалась больше 0, значит файл уже существует и инициализирован.
                if (fileStream.Length > 0)
                {
                    ReadDataPageMetadata(fileStream);
                }
                else
                {
                    // файл новый, поэтому заполняем нулями.
                    fileStream.SetLength(_config.PageSize);

                    _header = new DataPageHeader
                    { 
                        // для нового файла нижняя граница - после заголовка
                        LowerOffset = DataPageHeader.Size,
                        // а верхняя - конец файла.
                        UpperOffset = (int)fileStream.Length
                    };
                }

                _bufferedFileWriter = new BufferedFileWriter(
                    fileStream,
                    _config.BufferSize,
                    _config.AutoFlushInterval
                );

                // записать заголовок в файл.
                UpdateHeader();
            }
            else
            {
                // если файл существует и завершен, то читаем только метаданные.
                ReadDataPageMetadata(fileStream);
            }

            // Выключаем индексацию файла операционной системой, так как в файле массив байт, искать по контенту не получится :)
            fileInfo.Attributes = FileAttributes.NotContentIndexed;
        }

        /// <summary>
        /// Прочитать метаданные страницы.
        /// </summary>
        /// <param name="stream">Поток.</param>
        private void ReadDataPageMetadata(Stream stream)
        {
            ReadHeader();

            // если LowerOffset больше чем размер заголовка, то значит есть данные на странице.
            if (_header.LowerOffset > DataPageHeader.Size)
            {
                // читаем локальные индексы.
                ReadDataPageLocalIndexes(stream);
            }

            void ReadHeader()
            {
                var buffer = new byte[DataPageHeader.Size];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(buffer, 0, DataPageHeader.Size);
                var spanBuffer = buffer.AsSpan();
                _header = new DataPageHeader
                {
                    LowerOffset = spanBuffer.DecodeInt(0, out var nextStartOffset),
                    UpperOffset = spanBuffer.DecodeInt(nextStartOffset, out _)
                };
            }
        }

        /// <summary>
        /// Прочитать из потока файла индексы.
        /// </summary>
        /// <param name="stream">Поток.</param>
        private void ReadDataPageLocalIndexes(Stream stream)
        {
            _localItems.Clear();

            var localIndexSize = _header.LowerOffset - DataPageHeader.Size;
            var localItemsCount = localIndexSize / DataPageLocalIndex.Size;
            var buffer = new byte[localIndexSize];
            stream.Read(buffer, 0, localIndexSize);
            var span = buffer.AsSpan();

            for (var i = 0; i < localItemsCount; i++)
            {
                var slice = span.Slice(i * DataPageLocalIndex.Size, DataPageLocalIndex.Size);
                _localItems.Add(
                    new DataPageLocalIndex(
                        slice.DecodeLong(0, out var nextStartOffset),
                        slice.DecodeInt(nextStartOffset, out nextStartOffset),
                        slice.DecodeInt(nextStartOffset, out _)
                    )
                );
            }
        }

        /// <summary>
        /// Пометить завершенным.
        /// </summary>
        private void SetCompleted()
        {
            // TODO: делаем пометку, что страница завершена. (например делаем файл readonly, уничтожаем fileWriter.. так как он больше не нужен и т.д)

            //_bufferedFileWriter?.Dispose();
        }

        #endregion Методы (private)
    }
}