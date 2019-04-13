using Storage.Core.Helpers;
using System;

namespace Storage.Core.Models
{
    /// <summary>
    /// Модель-контейнер данных для хранения в <see cref="DataPage" />.
    /// </summary>
    public sealed class DataRecord : IEquatable<DataRecord>
    {
        #region Поля, свойства

        /// <summary>
        /// Размер метаданных в байтах.
        /// </summary>
        private const int MetaInfoSize = sizeof(long) + sizeof(int);

        /// <summary>
        /// Идентификатор записи в хранилище.
        /// </summary>
        public long Id { get; }

        /// <summary>
        /// Длина данных, включая метаданные (Id и Length).
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Тело данных.
        /// </summary>
        public byte[] Body { get; }

        #endregion Поля, свойства

        #region Конструкторы

        /// <summary>
        /// Инстанцирует новую модель с указанными данными.
        /// </summary>
        /// <param name="recordId">Идентификатор записи.</param>
        /// <param name="body">Данные для хранения.</param>
        public DataRecord(long recordId, byte[] body)
        {
            Body = body;
            Id = recordId;
            Length = body.Length + MetaInfoSize;
        }

        /// <summary>
        /// Создает новый инстанс из указанного массива байт.
        /// </summary>
        /// <param name="bytes"></param>
        public DataRecord(byte[] bytes)
        {
            if (bytes.Length < MetaInfoSize)
            {
                throw new InvalidOperationException("Передано недостаточное количество байт для чтения.");
            }
            var span = bytes.AsSpan();
            Length = span.DecodeInt(0, out var nextStartOffset);
            Id = span.DecodeLong(nextStartOffset, out nextStartOffset);
            Body = span.Slice(nextStartOffset, span.Length - nextStartOffset).ToArray();
        }

        #endregion Конструкторы

        #region Методы (public)

        /// <summary>
        /// Получить в виде массива байт.
        /// </summary>
        public byte[] GetBytes()
        {
            return ByteUtils.Flatten(
                BitConverter.GetBytes(Length),
                BitConverter.GetBytes(Id),
                Body
            );
        }

        #endregion Методы (public)

        #region Equality

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
        public bool Equals(DataRecord other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id == other.Id && Length == other.Length;
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is DataRecord other && Equals(other);
        }

        /// <summary>Serves as the default hash function.</summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            const int offset = 400;
            const int multiplier = 552;

            unchecked
            {
                var hashCode = offset;
                hashCode = (hashCode * multiplier) ^ Id.GetHashCode();
                hashCode = (hashCode * multiplier) ^ Length;

                return hashCode;
            }
        }

        /// <summary>Returns a value that indicates whether the values of two <see cref="T:Storage.Core.Models.DataRecord" /> objects are equal.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(DataRecord left, DataRecord right)
        {
            return Equals(left, right);
        }

        /// <summary>Returns a value that indicates whether two <see cref="T:Storage.Core.Models.DataRecord" /> objects have different values.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(DataRecord left, DataRecord right)
        {
            return !Equals(left, right);
        }

        #endregion Equality
    }
}