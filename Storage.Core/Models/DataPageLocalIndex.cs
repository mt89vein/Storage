using Storage.Core.Helpers;
using System;

namespace Storage.Core.Models
{
    /// <summary>
    /// Структура, которая хранит информацию о местонахождении данных на странице.
    /// </summary>
    /// <remarks>Размер 16 байт.</remarks>
    public readonly struct DataPageLocalIndex : IEquatable<DataPageLocalIndex>
    {
        #region Константы

        /// <summary>
        /// Размер структуры в байтах.
        /// </summary>
        public const int Size = sizeof(long) + sizeof(int) * 2;

        #endregion Константы

        #region Свойства

        /// <summary>
        /// Идентификатор записи.
        /// </summary>
        public long Id { get; }

        /// <summary>
        /// Указатель на начало данных.
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// Длина данных.
        /// </summary>
        public int Length { get; }

        #endregion Свойства

        #region Конструкторы

        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="id">Идентификатор записи.</param>
        /// <param name="offset">Указатель на начало данных.</param>
        /// <param name="length">Длина данных.</param>
        public DataPageLocalIndex(long id, int offset, int length)
        {
            Id = id;
            Offset = offset;
            Length = length;
        }

        #endregion Конструкторы

        #region Методы (public)

        /// <summary>
        /// Получить в формате массива байт.
        /// </summary>
        /// <returns>Массив байтю</returns>
        public byte[] GetBytes()
        {
            return ByteUtils.Flatten(
                BitConverter.GetBytes(Id),
                BitConverter.GetBytes(Offset),
                BitConverter.GetBytes(Length)
            );
        }

        #region Equality

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
        public bool Equals(DataPageLocalIndex other)
        {
            return Id == other.Id &&
                   Offset == other.Offset &&
                   Length == other.Length;
        }

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>true if <paramref name="obj">obj</paramref> and this instance are the same type and represent the same value; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return obj is DataPageLocalIndex other && Equals(other);
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            const int offset = 5211251;
            const int multiply = 421;
            unchecked
            {
                var hashCode = offset;
                hashCode = (hashCode * multiply) ^ Id.GetHashCode();
                hashCode = (hashCode * multiply) ^ Offset;
                hashCode = (hashCode * multiply) ^ Length;

                return hashCode;
            }
        }

        /// <summary>Returns a value that indicates whether the values of two <see cref="T:Storage.Core.Models.DataPageLocalIndex" /> objects are equal.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(DataPageLocalIndex left, DataPageLocalIndex right)
        {
            return left.Equals(right);
        }

        /// <summary>Returns a value that indicates whether two <see cref="T:Storage.Core.Models.DataPageLocalIndex" /> objects have different values.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(DataPageLocalIndex left, DataPageLocalIndex right)
        {
            return !left.Equals(right);
        }

        #endregion Equality

        #endregion Методы (public)
    }
}