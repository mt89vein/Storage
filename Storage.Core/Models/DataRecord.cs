using Google.Protobuf;
using System.IO;

namespace Storage.Core.Models
{
    /// <summary>
    /// Модель-контейнер данных для хранения в <see cref="DataPage" />.
    /// </summary>
    public sealed partial class DataRecord
    {
        #region Конструкторы

        /// <summary>
        /// Инстанцирует новую модель с указанными данными.
        /// </summary>
        /// <param name="recordId">Идентификатор записи.</param>
        /// <param name="body">Данные для хранения.</param>
        public DataRecord(long recordId, byte[] body)
        {
            Body = ByteString.CopyFrom(body);
            Header = new DataRecordHeader
            {
                Id = recordId,
                Length = Body.Length
            };

            Header.Length = CalculateSize();
        }

        /// <summary>
        /// Создает новый инстанс из указанного массива байт.
        /// </summary>
        /// <param name="bytes"></param>
        public DataRecord(byte[] bytes)
        {
            ReadFrom(bytes);
        }

        #endregion Конструкторы

        #region Методы (public)

        /// <summary>
        /// Прочитать заголовок из потока.
        /// </summary>
        /// <param name="stream">Поток.</param>
        /// <returns>Заголовок записи.</returns>
        public static DataRecordHeader ReadHeader(Stream stream)
        {
            return DataRecordHeader.Parser.ParseFrom(stream);
        }

        /// <summary>
        /// Получить в виде массива байт.
        /// </summary>
        public byte[] GetBytes()
        {
            return this.ToByteArray();
        }

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Прочитать из массива байт.
        /// </summary>
        /// <param name="bytes">Массив байт.</param>
        private void ReadFrom(byte[] bytes)
        {
            var dataRecord = Parser.ParseFrom(bytes);
            MergeFrom(dataRecord);
        }

        #endregion Методы (private)
    }
}