using System;

namespace Storage.Core.Configuration
{
    /// <summary>
    /// Конфигурация страницы данных.
    /// </summary>
    public class DataPageConfig
    {
        #region Свойства

        /// <summary>
        /// Название менеджера страниц, который управляет данной страницей.
        /// </summary>
        public string DataManagerName { get; }

        /// <summary>
        /// Временной интервал, между автоматическим сохранением на диск.
        /// </summary>
        public TimeSpan AutoFlushInterval { get; }

        /// <summary>
        /// Размер страницы в байтах.
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// Размер буфера для автоматической записи на диск.
        /// <para>
        /// default: 16384 байт.
        /// </para>
        /// </summary>
        public int BufferSize { get; }

        /// <summary>
        /// Размер буффера для чтения.
        /// <para>
        /// default: 4096 байт.
        /// </para>
        /// </summary>
        public int ReadBufferSize { get; }

        /// <summary>
        /// Максимальное количество читателей файла.
        /// <para>
        /// default: 5.
        /// </para>
        /// </summary>
        public int MaxReaderCount { get; }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="managerName">Название менеджера страниц, который управляет данной страницей.</param>
        /// <param name="pageSize">Размер страницы.</param>
        /// <param name="autoFlushInterval">Временной интервал, между автоматическим сохранением на диск. </param>
        /// <param name="bufferSize">Размер буфера для автоматической записи на диск.</param>
        /// <param name="readBufferSize">Размер буффера для чтения.</param>
        /// <param name="maxReaderCount">Максимальное количество читателей файла.</param>
        public DataPageConfig(
            string managerName,
            int pageSize,
            TimeSpan autoFlushInterval,
            int bufferSize = 16384,
            int readBufferSize = 4096,
            int maxReaderCount = 5
        )
        {
            DataManagerName = managerName;
            PageSize = pageSize;
            BufferSize = bufferSize;
            ReadBufferSize = readBufferSize;
            AutoFlushInterval = autoFlushInterval;
            MaxReaderCount = maxReaderCount;
        }

        #endregion Конструктор
    }
}