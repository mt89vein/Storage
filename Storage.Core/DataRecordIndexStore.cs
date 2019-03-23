using FastCollections.Unsafe;
using Storage.Core.Abstractions;
using Storage.Core.Helpers;
using Storage.Core.Models;
using System;
using System.IO;

namespace Storage.Core
{
    /// <summary>
    /// Индекс по <see cref="DataRecord.Id"/>
    /// </summary>
    public class DataRecordIndexStore : IDataRecordIndexStore
	{
		#region Поля

		/// <summary>
		/// BPlusTree дерево.
		/// </summary>
		private readonly BTree<long, DataRecordIndexPointer> _tree;

        /// <summary>
        /// Буферизированый писатель в файл.
        /// </summary>
        private readonly BufferedFileWriter _bufferedFileWriter;

        /// <summary>
        /// Размер буфера.
        /// </summary>
        private const int BufferSize = 8096;

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
		public DataRecordIndexStore(string directory)
		{
			_tree = new BTree<long, DataRecordIndexPointer>();
			var fileName = Path.Combine(directory, "index.bin");
            _bufferedFileWriter = new BufferedFileWriter(GetFileStream(fileName), BufferSize, TimeSpan.FromMilliseconds(500));
			ReconstituteIndexFromFile(fileName);
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
		/// Добавить указатель в индекс.
		/// </summary>
		/// <param name="recordIndexPointer">Указатель для добавления в индекс.</param>
		public void AddToIndex(DataRecordIndexPointer recordIndexPointer)
		{
			lock (_syncWriteLock)
			{
				_tree.Add(recordIndexPointer.DataRecordId, recordIndexPointer);
                _bufferedFileWriter.Write(recordIndexPointer.GetBytes(), 0, DataRecordIndexPointer.Size);
			}
		}

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Создать, если не существует файла для хранения индекса
        /// </summary>
        /// <param name="fileName"></param>
        private static FileStream GetFileStream(string fileName)
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
                BufferSize,
                FileOptions.None
            );

            fileInfo.Attributes = FileAttributes.NotContentIndexed;

            return fileStream;
        }

        /// <summary>
        /// Прочитать индекс из файла.
        /// </summary>
        /// <param name="fileName">Путь к файлу с индексом.</param>
        private void ReconstituteIndexFromFile(string fileName)
        {
            lock (_syncWriteLock)
            {
                _tree.Clear();

                using (var fileStream = new FileStream(
                    fileName,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite,
                    BufferSize,
                    FileOptions.SequentialScan
                ))
                {
                    using (var reader = new BinaryReader(fileStream))
                    {
                        while (reader.BaseStream.Position != reader.BaseStream.Length)
                        {
                            var dataRecordIndexPointer = new DataRecordIndexPointer(reader.ReadBytes(20));
                            _tree.Add(dataRecordIndexPointer.DataRecordId, dataRecordIndexPointer);
                        }
                    }
                }
            }
        }
        
        #endregion Методы (private)
    }
}