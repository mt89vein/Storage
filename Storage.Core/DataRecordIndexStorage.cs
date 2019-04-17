using FastCollections.Unsafe;
using Storage.Core.Abstractions;
using Storage.Core.Configuration;
using Storage.Core.Helpers;
using Storage.Core.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Storage.Core
{
    /// <summary>
    /// Хранилище индексов.
    /// </summary>
    public class DataRecordIndexStorage : IDataRecordIndexStorage
    {
        #region Поля

        /// <summary>
        /// Конфигурация хранилища индексов.
        /// </summary>
        private readonly DataRecordIndexStoreConfig _config;

        /// <summary>
        /// BPlusTree дерево.
        /// </summary>
        private readonly BTree<long, DataRecordIndexPointer> _tree;

        /// <summary>
        /// Буферизированый писатель в файл.
        /// </summary>
        private BufferedFileWriter _bufferedFileWriter;

        /// <summary>
        /// Название и путь к файлу - индексу.
        /// </summary>
        private readonly string _fileName;

        /// <summary>
        /// Объект для блокировок на запись в индекс.
        /// </summary>
        private readonly object _syncWriteLock = new object();

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Инициализирует хранилище индекса.
        /// </summary>
        /// <param name="directory">Путь к файлу индекса.</param>
        /// <param name="config">Конфигурация хранилища индексов.</param>
        public DataRecordIndexStorage(string directory, DataRecordIndexStoreConfig config)
        {
            _config = config;
            _tree = new BTree<long, DataRecordIndexPointer>();
            _fileName = Path.Combine(directory, "data-record-index-pointers.index");
            InitializeBufferedFileWriter();
            ReconstituteIndexFromFile();
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Попытаться найти из индекса.
        /// </summary>
        /// <param name="recordId">Идентификатор записи.</param>
        /// <param name="recordIndexPointer">Указатель на данные.</param>
        /// <returns>True, если удалось найти данные.</returns>
        public bool TryGetIndex(long recordId, out DataRecordIndexPointer recordIndexPointer)
        {
            var node = _tree.Find(recordId);
            recordIndexPointer = node.IsValid
                ? node.Value
                : default;

            return node.IsValid;
        }

        /// <summary>
        /// Получить индекс для итерирования с указанного идентификатора записи.
        /// </summary>
        /// <param name="fromRecordId">Идентификатор записи, от которого нужен итератор.</param>
        /// <returns>Итератор.</returns>
        public IEnumerable<DataRecordIndexPointer> AsEnumerable(long fromRecordId = 0)
        {
            return _tree.IsEmpty 
                ? Enumerable.Empty<DataRecordIndexPointer>() 
                : _tree.From(fromRecordId).AsEnumerable().Select(t => t.Value);
        }

        /// <summary>
        /// Добавить указатель в индекс.
        /// </summary>
        /// <param name="recordIndexPointer">Указатель для добавления в индекс.</param>
        public void AddToIndex(DataRecordIndexPointer recordIndexPointer)
        {
            // формируем общий список указателей.
            var pointers =
                new List<DataRecordIndexPointer>(recordIndexPointer.AdditionalDataRecordIndexPointers.Length + 1)
                {
                    recordIndexPointer
                };
            pointers.AddRange(recordIndexPointer.AdditionalDataRecordIndexPointers);

            lock (_syncWriteLock)
            {
                // добавляем в индекс.
                _tree.Add(recordIndexPointer.DataRecordId, recordIndexPointer);

                // сортируем по номерам страниц и последовательно пишем в файл.
                foreach (var pointer in pointers.OrderBy(p => p.DataPageNumber))
                {
                    _bufferedFileWriter.Write(pointer.GetBytes(), 0, DataRecordIndexPointer.Size);
                }
            }
        }

        /// <summary>
        /// Почистить все данные. Используется для перестройки индекса.
        /// </summary>
        public void Clear()
        {
            _tree.Clear();
            var fileInfo = new FileInfo(_fileName);
            if (fileInfo.Exists)
            {
                _bufferedFileWriter.Dispose();
                fileInfo.Delete();
                InitializeBufferedFileWriter();
            }
        }

        /// <summary>
        /// Высвобождаем выделенные неуправляемые ресурсы.
        /// </summary>
        public void Dispose()
        {
            _tree?.Dispose();
            _bufferedFileWriter?.Dispose();
        }

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Инициализировать буферизированный писатель в файл.
        /// </summary>
        private void InitializeBufferedFileWriter()
        {
            _bufferedFileWriter = new BufferedFileWriter(GetFileStream(_fileName), _config.BufferSize, _config.AutoFlushInterval);
        }

        /// <summary>
        /// Создать, если не существует файла для хранения индекса
        /// </summary>
        /// <param name="fileName"></param>
        private FileStream GetFileStream(string fileName)
        {
            var fileInfo = new FileInfo(fileName);

            if (!Directory.Exists(fileName))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            }

            var fileStream = new FileStream(
                fileName,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.ReadWrite,
                _config.BufferSize,
                FileOptions.None
            );

            fileInfo.Attributes = FileAttributes.NotContentIndexed;

            return fileStream;
        }

        /// <summary>
        /// Прочитать индекс из файла.
        /// </summary>
        private void ReconstituteIndexFromFile()
        {
            lock (_syncWriteLock)
            {
                _tree.Clear();

                using (var fileStream = new FileStream(
                    _fileName,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite,
                    _config.BufferSize,
                    FileOptions.SequentialScan
                ))
                {
                    using (var reader = new BinaryReader(fileStream))
                    {
                        var sameDataRecordIdPointers = new List<DataRecordIndexPointer>(); // список для агрегации.

                        var bytes = reader.ReadBytes(DataRecordIndexPointer.Size);
                        if (bytes.Length != DataRecordIndexPointer.Size)
                        {
                            return;
                        }

                        // прочитали первый указатель.
                        var currentDataRecordIndexPointer = new DataRecordIndexPointer(bytes);

                        // если он всего один, то добавляем и выходим.
                        if (reader.BaseStream.Position == reader.BaseStream.Length)
                        {
                            _tree.Add(currentDataRecordIndexPointer.DataRecordId, currentDataRecordIndexPointer);

                            return;
                        }

                        // читаем весь файл.
                        while (reader.BaseStream.Position != reader.BaseStream.Length)
                        {
                            // прочитали следующий указатель.
                            var data = reader.ReadBytes(DataRecordIndexPointer.Size);
                            var dataRecordIndexPointer = data.Length == DataRecordIndexPointer.Size
                                ? new DataRecordIndexPointer(data)
                                : new DataRecordIndexPointer();

                            // если он оказался таким же, какой и ранее, добавляем в список текущих.
                            if (dataRecordIndexPointer.DataRecordId == currentDataRecordIndexPointer.DataRecordId)
                            {
                                sameDataRecordIdPointers.Add(dataRecordIndexPointer);

                                // если это последний элемент, то нам не нужно переходить к следующему циклу.
                                if (reader.BaseStream.Position != reader.BaseStream.Length)
                                {
                                    continue;
                                }
                            }

                            if (sameDataRecordIdPointers.Any())
                            {
                                // создаем агрегированный указатель на основе текущего
                                var aggregated = new DataRecordIndexPointer(
                                    currentDataRecordIndexPointer.DataRecordId,
                                    currentDataRecordIndexPointer.DataPageNumber,
                                    currentDataRecordIndexPointer.Offset,
                                    currentDataRecordIndexPointer.Length,
                                    sameDataRecordIdPointers.ToArray()
                                );

                                _tree.Add(aggregated.DataRecordId, aggregated);
                                sameDataRecordIdPointers.Clear();
                            }
                            else
                            {
                                _tree.Add(currentDataRecordIndexPointer.DataRecordId, currentDataRecordIndexPointer);
                            }

                            // заменяем текущий.
                            currentDataRecordIndexPointer = dataRecordIndexPointer;
                        }
                    }
                }
            }
        }

        #endregion Методы (private)
    }
}