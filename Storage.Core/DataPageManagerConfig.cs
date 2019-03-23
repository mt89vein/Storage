using System;
using Storage.Core.Configuration;

namespace Storage.Core
{
    /// <summary>
    /// Настройки менеджера страниц.
    /// </summary>
    public class DataPageManagerConfig
    {
        /// <summary>
        /// Название менеджера страниц.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Базовая директория, для хранения страниц.
        /// </summary>
        public string Directory { get; }

        /// <summary>
        /// Временной интервал, между автоматическим сохранением на диск.
        /// </summary>
        public TimeSpan AutoFlushInterval { get; }

        /// <summary>
        /// Размер страницы в байтах.
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// Конфигурация страницы, на основе конфигурации менеджера страниц.
        /// </summary>
        public DataPageConfig DataPageConfig => DataPageConfigLazy.Value;

        /// <summary>
        /// Ленивая инициализация конфигурация страницы.
        /// </summary>
        /// <returns>Инициализатор конфигурации страницы.</returns>
        private Lazy<DataPageConfig> DataPageConfigLazy => new Lazy<DataPageConfig>(() => new DataPageConfig(Name, PageSize, AutoFlushInterval));

        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="name">Название менеджера страниц.</param>
        /// <param name="pageSize">Размер страницы.</param>
        /// <param name="directory">Базовая директория, для хранения страниц.</param>
        /// <param name="autoFlushInterval">Временной интервал, между автоматическим сохранением на диск.</param>
        public DataPageManagerConfig(string name, int pageSize, string directory, TimeSpan? autoFlushInterval = null)
        {
            Name = name;
            PageSize = pageSize;
            Directory = directory;
            AutoFlushInterval = autoFlushInterval ?? TimeSpan.FromMilliseconds(500);
        }
    }
}