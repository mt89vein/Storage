using System;
using System.Collections.Generic;
using System.IO;
using Storage.Core.Configuration;
using Storage.Core.Helpers;

namespace Storage.Core.Models
{
	/*
	 *  Зона ответственности: чтение с файла, запись в файл.
	 */

    // TODO: сделать IDisposable (очищение выделенных ресурсов с сохранением на диск), IDestroyable (т.е. уничтожение безвозвратное)

	/// <summary>
	/// Страница данных.
	/// </summary>
	public class DataPage
	{
		/// <summary>
		/// Поток для записи в файл. TODO: DISPOSE.
		/// <para>
		///  Инициализируется, только если в файл можно записать хоть какие-то данные.
		/// </para>
		/// </summary>
		private BufferedFileWriter _bufferedFileWriter;

		/// <summary>
		/// Объект синхронизации для записи в страницу.
		/// </summary>
		private readonly object _writeSyncObject = new object();

		/// <summary>
		/// Конфигурация страницы.
		/// </summary>
		private readonly DataPageConfig _config;

		/// <summary>
		/// Название файла.
		/// </summary>
		private readonly string _fileName;

		/// <summary>
		/// Позиция, с которой можно писать новые данные.
		/// </summary>
		private int _dataFreeOffset;

		/// <summary>
		/// Номер страницы.
		/// </summary>
		public int PageId { get; }

        /// <summary>
		/// Дата/время последнего обращения к странице.
		/// </summary>
		/// <remarks>Используется для автоматической выгрузки из памяти, если страница некоторое время не была нужна.</remarks>
		public DateTime LastActiveTime { get; private set; }

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
        }

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

            // Если файл не найден, или файл есть, но он не завершенный
            if (!fileInfo.Exists || !isCompleted)
            {
                var fileStream = new FileStream(
                    _fileName,
                    FileMode.Append,
                    FileAccess.ReadWrite,
                    FileShare.Read,
                    _config.BufferSize,
                    FileOptions.SequentialScan
                );

                // для нового 0, для незавершенного - конец файла, откуда можно продолжить запись.
                _dataFreeOffset = (int)fileStream.Position; 

                _bufferedFileWriter = new BufferedFileWriter(
                    fileStream,
                    _config.BufferSize,
                    _config.AutoFlushInterval
                );
            }
            else
            {
                _dataFreeOffset = (int)fileInfo.Length;
            }

            // Выключаем индексацию файла операционной системой, так как в файле массив байт, искать по контенту не получится :)
            fileInfo.Attributes = FileAttributes.NotContentIndexed;
        }

        public void SetCompleted()
        {

        }

        /// <summary>
        /// Прочитать всю страницу (заголовки записей)
        /// </summary>
        /// <remarks>Можно использовать для поиска без индекса (по id) или перестроить индекс.</remarks>
        private void SequenceScan()
        {
            //using (var fileStream = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, _config.BufferSize, FileOptions.SequentialScan))
            //{
            //    // Переходим в начало потока.
            //    fileStream.Seek(0, SeekOrigin.Begin);

            //    // читаем заголовки, пока поток не кончится.
            //    while (fileStream.Position != fileStream.Length)
            //    {
            //        var header = DataRecord.ReadHeader(fileStream);
            //        // тут можно чекнуть индекс.
            //        fileStream.Seek(header.Length, SeekOrigin.Current);
            //    }
            //    _dataFreeOffset = (int)fileStream.Length;
            //}
        }

        /// <summary>
        /// Проверка на вместимость данных в данную страничку.
        /// </summary>
        /// <param name="data">Массив байт.</param>
        /// <exception cref="DataPageSizeLimitException">Исключение, выбрасывается в случае если переданно данных больше, чем может влезть в одну страницу.</exception>
        /// <returns>True, если данные влезут в данную страничку.</returns>
        public bool HasSpaceFor(IReadOnlyCollection<byte> data)
		{
            if (data.Count > _config.PageSize)
            {
                throw new DataPageSizeLimitException($"Переданное количество байт: {data.Count} превышает максимально допустимое: {_config.PageSize}");
            }

			return data.Count + _dataFreeOffset <= _config.PageSize;
		}

		/// <summary>
		/// Попытаться записать данные на страницу.
		/// </summary>
		/// <param name="data">Массив байт для записи.</param>
		/// <param name="offset">Позиция, с которой пойдет запись новых данных.</param>
		/// <returns>Номер записи.</returns>
		public bool TrySaveData(byte[] data, out int offset)
		{
			// Обновляем время последней активности
			LastActiveTime = DateTime.UtcNow;
			offset = 0;

			try
			{
				lock (_writeSyncObject)
				{
					// dataOffset, с которого начинается запись.
					offset = _bufferedFileWriter.Position;

					_bufferedFileWriter.Write(data, 0, data.Length);

					// Сдвигаем оффсет.
                    _dataFreeOffset += data.Length;
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
		/// Прочитать запись.
		/// </summary>
		/// <param name="offset">Сдвиг.</param>
		/// <param name="length">Номер записи.</param>
		/// <returns>Запись.</returns>
		public DataRecord Read(int offset, int length)
		{
			LastActiveTime = DateTime.UtcNow;

			// TODO: use readerPool
			using (var fileStream =
				new FileStream(
					_fileName,
					FileMode.Open,
					FileAccess.Read,
					FileShare.ReadWrite,
					_config.BufferSize,
					FileOptions.RandomAccess
				)
			)
			{
				fileStream.Position = offset;
				using (var reader = new BinaryReader(fileStream))
				{
					return new DataRecord(reader.ReadBytes(length));
					// TODO: cache record by offset
				}
			}
		}
	}
}