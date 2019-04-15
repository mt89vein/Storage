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
            _autoFlushTimer = new Timer(state => FlushToDisk(), null, autoFlushInterval, autoFlushInterval);
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Установить на указанную позицию.
        /// </summary>
        /// <param name="position">Позиция.</param>
        public void SetPosition(int position) => _bufferedStream.Position = position;

        /// <summary>
        /// Записать данные.
        /// </summary>
        /// <param name="data">Массив данных.</param>
        /// <param name="offset">Отступ.</param>
        /// <param name="count">Длина.</param>
        public void Write(byte[] data, int offset, int count) => _bufferedStream.Write(data, offset, count);

        /// <summary>
        /// Сохраняет несохраненные данные и освобождает неуправляемые ресурсы.
        /// </summary>
        public void Dispose()
        {
            StopTimer();
            FlushToDisk();
            _bufferedStream?.Dispose();
            _autoFlushTimer?.Dispose();
        }

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Остановить автосохранение.
        /// </summary>
        private void StopTimer() => _autoFlushTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        /// <summary>
        /// Записать на диск.
        /// </summary>
        private void FlushToDisk() => _bufferedStream?.Flush();

        #endregion Методы (private)
    }
}