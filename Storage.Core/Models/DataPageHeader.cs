using Storage.Core.Helpers;
using System;

namespace Storage.Core.Models
{
    /// <summary>
    /// Заголовок страницы данных.
    /// </summary>
    /// <remarks>Размер 8 байт.</remarks>
    public class DataPageHeader
    {
        #region Константы

        /// <summary>
        /// Размер заголовка страницы.
        /// </summary>
        public const int Size = sizeof(int) * 2;

        #endregion Константы

        #region Свойства

        /// <summary>
        /// Указатель на нижнюю границу данных, с которого начинается свободное место.
        /// </summary>
        public int LowerOffset { get; set; }

        /// <summary>
        /// Указатель на верхнюю границу данных, где заканчивается свободное место.
        /// </summary>
        public int UpperOffset { get; set; }

        #endregion Свойства

        #region Методы (public)

        /// <summary>
        /// Получить в формате массива байт.
        /// </summary>
        /// <returns>Массив байтю</returns>
        public byte[] GetBytes()
        {
            return ByteUtils.Flatten(
                BitConverter.GetBytes(LowerOffset),
                BitConverter.GetBytes(UpperOffset)
            );
        }

        #endregion Методы (public)
    }
}