using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Storage.Core.Abstractions;

namespace Storage.Core.FileNamingStrategies
{
    // TODO: тесты
    /// <summary>
    /// Стратегия формирования названия файлов по иерархии.
    /// </summary>
    public class HierarchyFileNamingStrategy : IFileNamingStrategy
    {
        #region Поля

        /// <summary>
        /// Расширение файла.
        /// </summary>
        private readonly string _extension;

        /// <summary>
        /// Корневая директория для файлов.
        /// </summary>
        private readonly string _rootDirectory;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="rootDirectory">Префикс для названия файла.</param>
        /// <param name="extension">Расширение файла.</param>
        public HierarchyFileNamingStrategy(string rootDirectory, string extension = "bin")
        {
            _rootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
            _extension = extension;
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Получить название файла
        /// </summary>
        /// <param name="index">Индекс.</param>
        /// <returns>Название файла.</returns>
        public string GetFileNameFor(int index)
        {
            if (index <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return Path.Combine(_rootDirectory, GetRelativePathFor(index));
        }

        /// <summary>
        /// Получить фалйы по пути.
        /// </summary>
        /// <returns>Список названий файлов.</returns>
        public IReadOnlyDictionary<int, string> GetFiles()
        {
            return Directory
                .EnumerateFiles(_rootDirectory, $"*.{_extension}", SearchOption.AllDirectories)
                .OrderBy(GetIndexFor)
                .ToDictionary(GetIndexFor);
        }

        /// <summary>
        /// Получить индекс из названия файла.
        /// </summary>
        /// <param name="fileName">Название файла.</param>
        /// <returns>Индекс файла.</returns>
        public int GetIndexFor(string fileName)
        {
            return int.Parse(Path.GetFileNameWithoutExtension(fileName), NumberStyles.HexNumber);
        }

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Вычисляет относительный путь к файлу по индексу.
        /// </summary>
        /// <param name="index">Индекс.</param>
        /// <returns>Относительный путь к файлу.</returns>
        private string GetRelativePathFor(int index)
        {
            var hex = index.ToString("X8");

            return Path.Combine(
                hex.Substring(0, 2),
                hex.Substring(2, 2),
                hex.Substring(4, 2),
                hex + '.' + _extension
            );
        }

        #endregion Методы (private)
    }
}