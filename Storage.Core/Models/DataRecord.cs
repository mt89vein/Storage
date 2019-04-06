using System;
using Google.Protobuf;
using System.IO;

namespace Storage.Core.Models
{
    /// <summary>
    /// Модель-контейнер данных для хранения в <see cref="DataPage"/>.
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
            Header = new DataRecordHeader
            {
                Id = recordId,
                Length = body.Length
            };
			Body = ByteString.CopyFrom(body);
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

        #region Методы

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
        public byte[] GetBytes() => this.ToByteArray();

		/// <summary>
		/// Прочитать из массива байт.
		/// </summary>
		/// <param name="bytes">Массив байт.</param>
		public void ReadFrom(byte[] bytes)
		{
			var dataRecord = Parser.ParseFrom(bytes);
			MergeFrom(dataRecord);
		}

		#endregion Методы
	}
}