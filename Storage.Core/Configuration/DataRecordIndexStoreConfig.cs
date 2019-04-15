using System;

namespace Storage.Core.Configuration
{
    /// <summary>
    /// Конфигуррация хранилища индексов.
    /// </summary>
    public class DataRecordIndexStoreConfig
    {
        /// <summary>
        /// Временной интервал, между автоматическим сохранением на диск.
        /// </summary>
        public TimeSpan AutoFlushInterval { get; }

        /// <summary>
        /// Размер буфера для автоматической записи на диск.
        /// <para>
        /// default: 50 записей в индекс.
        /// </para>
        /// </summary>
        public int BufferSize { get; }

        /// <summary>
        /// Инициализирует конфигурацию хранилища индексов.
        /// </summary>
        /// <param name="autoFlushInterval">Временной интервал, между автоматическим сохранением на диск. </param>
        /// <param name="bufferSize">Размер буфера для автоматической записи на диск.</param>
        public DataRecordIndexStoreConfig(TimeSpan autoFlushInterval, int? bufferSize = null)
        {
            AutoFlushInterval = autoFlushInterval;
            BufferSize = bufferSize ?? 4096;
        }
    }
}