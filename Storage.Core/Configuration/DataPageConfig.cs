using System;

namespace Storage.Core.Configuration
{
	public class DataPageConfig
	{
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
        ///  default: 30% от размера страницы.
        /// </para>
        /// </summary>
        public int BufferSize { get; }

        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="managerName">Название менеджера страниц, который управляет данной страницей.</param>
        /// <param name="pageSize">Размер страницы.</param>
        /// <param name="autoFlushInterval">Временной интервал, между автоматическим сохранением на диск. </param>
        /// <param name="bufferSize">Размер буфера для автоматической записи на диск.</param>
        public DataPageConfig(string managerName, int pageSize, TimeSpan autoFlushInterval, int? bufferSize = null)
		{
            DataManagerName = managerName;
            PageSize = pageSize;
            BufferSize = bufferSize ?? (int)Math.Round(pageSize * 0.3m, MidpointRounding.AwayFromZero);
			AutoFlushInterval = autoFlushInterval;
		}
	}
}
