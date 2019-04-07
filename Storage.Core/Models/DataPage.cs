using Storage.Core.Configuration;
using Storage.Core.Helpers;
using System;
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
		/// <summary>
		/// Поток для записи в файл.
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
                    FileAccess.Write,
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

        /// <summary>
        /// Пометить завершенным.
        /// </summary>
        private void SetCompleted()
        {
            // TODO: делаем пометку, что страница завершена. (например делаем файл readonly, уничтожаем fileWriter.. так как он больше не нужен и т.д)
        }

        /// <summary>
        /// Прочитать всю страницу (заголовки записей)
        /// </summary>
        private void SequenceScan()
        {
            // TODO: Можно использовать для поиска без индекса (по id) или перестроить индекс.
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
        /// Получить количество байт, которое можно записать на данную страницу.
        /// </summary>
        /// <returns>Количество байт, которое можно записать на данную страницу.</returns>
        public int GetFreeSpaceLength()
        {
            return _config.PageSize - _dataFreeOffset;
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

                    if (GetFreeSpaceLength() == 0)
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
                    return reader.ReadBytes(length);
                    // TODO: cache record by offset
                }
            }
        }

        /// <summary>
        /// Высвобождает неуправляемые ресурсы.
        /// </summary>
        public void Dispose()
        {
            // TODO: uncache.
            _bufferedFileWriter?.Dispose();
        }
    }
}