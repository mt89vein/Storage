using Storage.Core.Helpers;
using System;
using System.Linq;

namespace Storage.Core.Models
{
    /// <summary>
    /// Указатель на данные на странице данных.
    /// </summary>
    public readonly struct DataRecordIndexPointer : IEquatable<DataRecordIndexPointer>
	{
		#region Константы

		/// <summary>
		/// Размер структуры в байтах.
		/// </summary>
		public const int Size = 20;

		#endregion Константы

		#region Конструкторы

        /// <summary>
        /// Инициализирует указатель на данные на странице данных.
        /// </summary>
        /// <param name="recordId">Идентификатор записи.</param>
        /// <param name="pageNumber">Номер страницы.</param>
        /// <param name="offset">Сдвиг по странице.</param>
        /// <param name="length">Длина данных.</param>
        /// <param name="additionalDataRecordIndexPointers">Ссылки на другие части мультистраничной записи.</param>
        public DataRecordIndexPointer(long recordId, int pageNumber, int offset, int length, params DataRecordIndexPointer[] additionalDataRecordIndexPointers)
			: this()
		{
			DataRecordId = recordId;
			DataPageNumber = pageNumber;
			Offset = offset;
			Length = length;

            if (additionalDataRecordIndexPointers.Any(p => p.DataRecordId != recordId))
            {
                throw new ArgumentException("Для добавление в индекс все указатели должны иметь один и тот же DataRecordId");
            }

            if (additionalDataRecordIndexPointers.Any(p => p.AdditionalDataRecordIndexPointers?.Any() == true))
            {
                throw new ArgumentException("Дополнительный указатель не может иметь свои дополнительные указатели.");
            }

            AdditionalDataRecordIndexPointers = additionalDataRecordIndexPointers;
		}

		/// <summary>
		/// Инициализирует указатель на данные на странице данных из массива байт.
		/// </summary>
		/// <param name="bytes">Массив байт.</param>
		public DataRecordIndexPointer(byte[] bytes)
			: this()
		{
			if (bytes.Length != Size)
			{
				throw new InvalidOperationException($"Переданный массив байт не содержит {Size} байт. Передано: {bytes.Length} байт.");
			}

			DataRecordId = BitConverter.ToInt64(bytes, 0);
			DataPageNumber = BitConverter.ToInt32(bytes, 8);
			Offset = BitConverter.ToInt32(bytes, 12);
			Length = BitConverter.ToInt32(bytes, 16);
		}

		#endregion Конструкторы

		#region Свойства

		/// <summary>
		/// Номер записи.
		/// </summary>
		public long DataRecordId { get; }

		/// <summary>
		/// Номер страницы данных.
		/// </summary>
		public int DataPageNumber { get; }

		/// <summary>
		/// Сдвиг.
		/// </summary>
		public int Offset { get; }

		/// <summary>
		/// Длина данных.
		/// </summary>
		/// <remarks>2 gb максимум</remarks>
		public int Length { get; }

        /// <summary>
        /// Ссылки на другие части мультистраничной записи.
        /// </summary>
        public DataRecordIndexPointer[] AdditionalDataRecordIndexPointers { get; }

		#endregion Свойства

		#region Методы (public)

		/// <summary>
		/// Получить в виде массива байт.
		/// </summary>
		/// <returns>Массив байт.</returns>
		public byte[] GetBytes()
		{
			return ByteArrayExtensions.Flatten(
				BitConverter.GetBytes(DataRecordId),
				BitConverter.GetBytes(DataPageNumber),
				BitConverter.GetBytes(Offset),
				BitConverter.GetBytes(Length)
			);
		}

		#endregion Методы (public)

		#region Equality

		public override bool Equals(object obj)
		{
			return obj is DataRecordIndexPointer pointer && Equals(pointer);
		}

		public bool Equals(DataRecordIndexPointer other)
		{
			return DataRecordId == other.DataRecordId &&
                   DataPageNumber == other.DataPageNumber &&
                   Offset == other.Offset &&
                   Length == other.Length;
        }

		public override int GetHashCode()
        {
            const int offset = 386508603;
            const int multiplier = -1521134295;

            var hashCode = offset;
			hashCode = hashCode * multiplier + DataRecordId.GetHashCode();
			hashCode = hashCode * multiplier + DataPageNumber.GetHashCode();
			hashCode = hashCode * multiplier + Offset.GetHashCode();
			hashCode = hashCode * multiplier + Length.GetHashCode();
			return hashCode;
		}

		#endregion Equality
	}
}