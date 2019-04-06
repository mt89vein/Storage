﻿using FastCollections.Unsafe;
using Storage.Core.Abstractions;
using Storage.Core.Helpers;
using Storage.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
			var fileName = Path.Combine(directory, "data-record-index-pointers.index");
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
		// TODO: тест на корректность записи в файл.
		public void AddToIndex(DataRecordIndexPointer recordIndexPointer)
		{
			lock (_syncWriteLock)
            {
                // добавляем в индекс.
                _tree.Add(recordIndexPointer.DataRecordId, recordIndexPointer);

                // формируем общий список указателей.
                var pointers = new List<DataRecordIndexPointer>(recordIndexPointer.AdditionalDataRecordIndexPointers.Length + 1)
                {
                    recordIndexPointer
                };
                pointers.AddRange(recordIndexPointer.AdditionalDataRecordIndexPointers);

                // сортируем по номерам страниц и последовательно пишем в файл.
                foreach (var pointer in pointers.OrderBy(p => p.DataPageNumber))
                {
                    _bufferedFileWriter.Write(pointer.GetBytes(), 0, DataRecordIndexPointer.Size);
                }
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
        // TODO: написать тест на восстановление индекса из файла (корректность аггрегации многостраничников)
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