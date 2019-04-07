using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Storage.Core.Abstractions;

namespace Storage.Core.FileNamingStrategies
{
    // TODO: тесты
    /// <summary>
    /// Стратегия формирования названия файлов по-умолчанию.
    /// </summary>
    public class DefaultFileNamingStrategy : IFileNamingStrategy
    {
        #region Поля

        /// <summary>
        /// Паттерн наименования файла.
        /// </summary>
        private readonly Regex _fileNamePattern;

        /// <summary>
        /// Корневая директория.
        /// </summary>
        private readonly string _rootDirectory;

        /// <summary>
        /// Формат.
        /// </summary>
        private readonly string _format;

        /// <summary>
        /// Расширение файла.
        /// </summary>
        private readonly string _extension;

        /// <summary>
        /// Префикс для названия файла.
        /// </summary>
        private readonly string _prefix;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="rootDirectory">Корневая директория.</param>
        /// <param name="prefix">Префикс для названия файла.</param>
        /// <param name="pattern">Паттерн. По-умолчанию 6 цифр.</param>
        /// <param name="format">Формат.</param>
        /// <param name="extension">Расширение файла.</param>
        public DefaultFileNamingStrategy(string rootDirectory, string prefix, string pattern = @"\d{6}",
            string format = "{0}{1:000000000}", string extension = "bin")
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            _prefix = prefix + "-";
            _rootDirectory = rootDirectory;
            _format = format ?? throw new ArgumentNullException(nameof(format));
            _extension = extension;
            _fileNamePattern = new Regex("^" + _prefix + pattern);
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

            return Path.Combine(_rootDirectory, string.Format(_format, _prefix, index, ".", _extension));
        }

        /// <summary>
        /// Получить индекс по названию файла.
        /// </summary>
        /// <param name="fileName">Название файла.</param>
        /// <returns>Индекс.</returns>
        public int GetIndexFor(string fileName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Получить фалйы по пути.
        /// </summary>
        /// <returns>Список названий файлов.</returns>
        public IReadOnlyDictionary<int, string> GetFiles()
        {
            return Directory
                .EnumerateFiles(_rootDirectory)
                .Where(x => !string.IsNullOrWhiteSpace(x) && _fileNamePattern.IsMatch(Path.GetFileName(x)))
                .OrderBy(GetIndexFor)
                .ToDictionary(GetIndexFor);
        }

        #endregion Методы (public)
    }
}