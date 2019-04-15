using Microsoft.Extensions.ObjectPool;
using System.IO;

namespace Storage.Core.Helpers
{
    /// <summary>
    /// Политика пулинга читателя потока.
    /// </summary>
    internal class FileReaderUnitPooledObjectPolicy : IPooledObjectPolicy<FileReaderUnit>
    {
        #region Поля

        /// <summary>
        /// Название файла.
        /// </summary>
        private readonly string _fileName;

        /// <summary>
        /// Размер буффера для чтения.
        /// </summary>
        private readonly int _readBufferSize;

        #endregion Поля

        #region Конструктор

        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="fileName">Название файла.</param>
        /// <param name="readBufferSize">Размер буффера для чтения.</param>
        public FileReaderUnitPooledObjectPolicy(string fileName, int readBufferSize)
        {
            _fileName = fileName;
            _readBufferSize = readBufferSize;
        }

        #endregion Конструктор

        #region Методы

        /// <summary>
        /// Фабричный метод для создания читателя потока в пуле.
        /// </summary>
        /// <returns>Читатель потока.</returns>
        public FileReaderUnit Create()
        {
            return new FileReaderUnit(new FileStream(
                _fileName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,
                _readBufferSize,
                FileOptions.RandomAccess
            ));
        }

        /// <summary>
        /// Вернуть читателя в пул.
        /// </summary>
        /// <param name="obj">Читатель потока.</param>
        /// <returns>True, если элемент возвращаем в поток.</returns>
        public bool Return(FileReaderUnit obj)
        {
            // перед возвращением ничего делать не нужно, в том числе и
            // перевод позиции в начало, так как при чтении мы сдвигаем всё равно.

            return true;
        }

        #endregion Методы
    }
}