using System;
using System.IO;
using System.Threading;

namespace Storage.Core.Helpers
{
    /// <summary>
    /// Эффективным образом осуществляет запись в файл на диск.
    /// </summary>
    public class BufferedFileWriter : IDisposable
    {
        #region Поля, свойства

        /// <summary>
        /// Таймер для автоматического сохранения данных на диск через определенный интервал.
        /// </summary>
        private readonly Timer _autoFlushTimer;

        /// <summary>
        /// Поток файла с буферным слоем.
        /// </summary>
        private readonly BufferedStream _bufferedStream;

        /// <summary>
        /// Позиция, которая последний раз была записана на диск.
        /// </summary>
        public int LastFlushedOffset { get; private set; }

        #endregion Поля, свойства

        #region Конструктор

        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="stream">Поток данных.</param>
        /// <param name="buferSize">Размер буфера. При заполнении автоматически будет вызван Flush.</param>
        /// <param name="autoFlushInterval">Временной интервал между автоматическими записями на диск.</param>
        public BufferedFileWriter(Stream stream, int buferSize, TimeSpan autoFlushInterval)
        {
            _bufferedStream = new BufferedStream(stream, buferSize);
            _autoFlushTimer = new Timer(state => FlushToDisk(), null, TimeSpan.Zero, autoFlushInterval);
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Записать данные.
        /// </summary>
        /// <param name="data">Массив данных.</param>
        /// <param name="offset">Отступ.</param>
        /// <param name="count">Длина.</param>
        public void Write(byte[] data, int offset, int count)
        {
            _bufferedStream.Write(data, offset, count);
        }

        /// <summary>
        /// Возвращает текущую позицию в потоке.
        /// </summary>
        /// <remarks>Переполнения int не ожидается, так как работаем с макс. 2гб файлами.</remarks>
        public int Position => (int) _bufferedStream.Position;

        /// <summary>
        /// Сохраняет несохраненные данные и освобождает неуправляемые ресурсы.
        /// </summary>
        public void Dispose()
        {
            StopTimer();
            FlushToDisk(true);
            _bufferedStream.Dispose();
            _autoFlushTimer.Dispose();
        }

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Остановить автосохранение.
        /// </summary>
        private void StopTimer()
        {
            _autoFlushTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// Записать на диск, если есть изменения.
        /// </summary>
        private void FlushToDisk(bool force = false)
        {
            if (!force && LastFlushedOffset == _bufferedStream?.Position)
            {
                return;
            }

            if (_bufferedStream != null)
            {
                _bufferedStream.Flush();
                LastFlushedOffset = Position;
            }
        }

        #endregion Методы (private)
    }
}