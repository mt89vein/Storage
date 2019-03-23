using System;

namespace Storage.Core
{
    /// <summary>
    /// Исключение, выбрасывается в случае если переданно данных больше, чем может влезть в одну страницу.
    /// </summary>
    public class DataPageSizeLimitException : Exception
    {
        /// <summary>
        /// Создать исключение о недостатке места на странице.
        /// </summary>
        /// <param name="message">Сообщение об ошибке.</param>
        public DataPageSizeLimitException(string message) : base(message)
        {
        }
    }
}
