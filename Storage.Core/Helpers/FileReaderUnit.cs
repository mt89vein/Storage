using System;
using System.IO;

namespace Storage.Core.Helpers
{
    /// <summary>
    /// Читатель потока.
    /// </summary>
    internal class FileReaderUnit : IDisposable
    {
        /// <summary>
        /// Бинарный читатель.
        /// </summary>
        public BinaryReader Reader { get; }

        /// <summary>
        /// Поток для чтения.
        /// </summary>
        private readonly Stream _stream;

        /// <summary>
        /// Менеджер неуправляемой памяти.
        /// </summary>
        private readonly UmsManager _umsManager;

        /// <summary>
        /// Создает читателя потока из _stream.
        /// </summary>
        /// <param name="stream">Поток.</param>
        public FileReaderUnit(Stream stream)
        {
            _stream = stream;
            Reader = new BinaryReader(stream);
        }

        /// <summary>
        /// Создает читателя потока неуправляемой памяти.
        /// </summary>
        /// <param name="umsManager">Менеджер неуправляемой памяти.</param>
        public FileReaderUnit(UmsManager umsManager)
        {
            _umsManager = umsManager;
            _stream = umsManager.Stream;
            Reader = new BinaryReader(umsManager.Stream);
        }

        /// <summary>
        /// Высвобождает ресурсы.
        /// </summary>
        public void Dispose()
        {
            _umsManager?.Dispose();
            Reader?.Close();
            Reader?.Dispose();
            _stream?.Dispose();
        }
    }
}
