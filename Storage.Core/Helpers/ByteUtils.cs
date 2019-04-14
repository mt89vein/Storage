using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Storage.Core.Helpers
{
    /// <summary>
    /// Утилита для работы с байтами. 
    /// </summary>
    internal static class ByteUtils
    {
        #region Поля

        /// <summary>
        /// Пустой массив байт.
        /// </summary>
        private static readonly byte[] EmptyBytes = new byte[0];

        #endregion Поля

        #region Конвертация в массив байт

        /// <summary>
        /// Конвертировать строку в массив байт.
        /// </summary>
        /// <param name="data">Строка.</param>
        public static byte[] ToBytes(this string data)
        {
            return data == null
                ? EmptyBytes
                : Encoding.UTF8.GetBytes(data);
        }

        /// <summary>
        /// Конвертировать дата-время в массив байт.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this DateTime data)
        {
            return BitConverter.GetBytes(data.Ticks);
        }

        #endregion Конвертация в массив байт

        #region Стандартные реализации

        /// <summary>
        /// Получить строку из переданного массива байт.
        /// </summary>
        /// <param name="sourceBuffer">Массив байт для конвертации.</param>
        /// <param name="startOffset">Начальный сдвиг.</param>
        /// <param name="nextStartOffset">Оффсет после чтения.</param>
        /// <returns>Строка.</returns>
        public static string DecodeString(this byte[] sourceBuffer, int startOffset, out int nextStartOffset)
        {
            nextStartOffset = startOffset + sourceBuffer.Length;

            return Encoding.UTF8.GetString(sourceBuffer);
        }

        /// <summary>
        /// Получить <see cref="short"/> из массива байт.
        /// </summary>
        /// <param name="sourceBuffer">Массив байт для конвертации.</param>
        /// <param name="startOffset">Начальный сдвиг.</param>
        /// <param name="nextStartOffset">Оффсет после чтения.</param>
        /// <returns>Short.</returns>
        public static short DecodeShort(this byte[] sourceBuffer, int startOffset, out int nextStartOffset)
        {
            var shortBytes = sourceBuffer.Slice(startOffset, sizeof(short), out nextStartOffset);

            return BitConverter.ToInt16(shortBytes, 0);
        }

        /// <summary>
        /// Получить <see cref="int"/> из массива байт. 
        /// </summary>
        /// <param name="sourceBuffer">Массив байт для конвертации.</param>
        /// <param name="startOffset">Начальный сдвиг.</param>
        /// <param name="nextStartOffset">Оффсет после чтения.</param>
        /// <returns>Int.</returns>
        public static int DecodeInt(this byte[] sourceBuffer, int startOffset, out int nextStartOffset)
        {
            var intBytes = sourceBuffer.Slice(startOffset, sizeof(int), out nextStartOffset);

            return BitConverter.ToInt32(intBytes, 0);
        }

        /// <summary>
        /// Получить <see cref="long"/> из массива байт.
        /// </summary>
        /// <param name="sourceBuffer">Массив байт для конвертации.</param>
        /// <param name="startOffset">Начальный сдвиг.</param>
        /// <param name="nextStartOffset">Оффсет после чтения.</param>
        /// <returns>Long.</returns>
        public static long DecodeLong(this byte[] sourceBuffer, int startOffset, out int nextStartOffset)
        {
            var longBytes = sourceBuffer.Slice(startOffset, sizeof(long), out nextStartOffset);

            return BitConverter.ToInt64(longBytes, 0);
        }

        /// <summary>
        /// Получить <see cref="DateTime"/> из массива байт.
        /// </summary>
        /// <param name="sourceBuffer">Массив байт для конвертации.</param>
        /// <param name="startOffset">Начальный сдвиг.</param>
        /// <param name="nextStartOffset">Оффсет после чтения.</param>
        /// <returns>DateTime.</returns>
        public static DateTime DecodeDateTime(this byte[] sourceBuffer, int startOffset, out int nextStartOffset)
        {
            // DateTime -> 8 Byte, as Long.
            var bytes = sourceBuffer.Slice(startOffset, sizeof(long), out nextStartOffset);

            return new DateTime(BitConverter.ToInt64(bytes, 0));
        }

        /// <summary>
        /// Получить срез массива.
        /// </summary>
        /// <param name="sourceBuffer">Массив байт для среза.</param>
        /// <param name="length">Длина среза.</param>
        /// <param name="startOffset">Начальный сдвиг.</param>
        /// <param name="nextStartOffset">Оффсет после среза.</param>
        /// <returns>Срезанный массив байт указанной длины.</returns>
        public static byte[] Slice(this byte[] sourceBuffer, int startOffset, int length, out int nextStartOffset)
        {
            var bytes = new byte[length];
            Buffer.BlockCopy(sourceBuffer, startOffset, bytes, 0, length);
            nextStartOffset = startOffset + length;

            return bytes;
        }

        #endregion Стандартные реализации

        #region Span реализации

        /// <summary>
        /// Получить <see cref="short"/> из среза массива байт.
        /// </summary>
        /// <param name="sourceBuffer">Срез массива байт для конвертации.</param>
        /// <param name="startOffset">Начальный сдвиг.</param>
        /// <param name="nextStartOffset">Оффсет после чтения.</param>
        /// <returns>Short.</returns>
        public static short DecodeShort(this Span<byte> sourceBuffer, int startOffset, out int nextStartOffset)
        {
            var shortBytes = sourceBuffer.Slice(startOffset, sizeof(short), out nextStartOffset);
            return MemoryMarshal.Read<short>(shortBytes);
        }

        /// <summary>
        /// Получить <see cref="int"/> из среза массива байт. 
        /// </summary>
        /// <param name="sourceBuffer">Срез массива байт для конвертации.</param>
        /// <param name="startOffset">Начальный сдвиг.</param>
        /// <param name="nextStartOffset">Оффсет после чтения.</param>
        /// <returns>Int.</returns>
        public static int DecodeInt(this Span<byte> sourceBuffer, int startOffset, out int nextStartOffset)
        {
            var intBytes = sourceBuffer.Slice(startOffset, sizeof(int), out nextStartOffset);

            return MemoryMarshal.Read<int>(intBytes);
        }

        /// <summary>
        /// Получить <see cref="long"/> из среза массива байт.
        /// </summary>
        /// <param name="sourceBuffer">Срез массива байт для конвертации.</param>
        /// <param name="startOffset">Начальный сдвиг.</param>
        /// <param name="nextStartOffset">Оффсет после чтения.</param>
        /// <returns>Long.</returns>
        public static long DecodeLong(this Span<byte> sourceBuffer, int startOffset, out int nextStartOffset)
        {
            var longBytes = sourceBuffer.Slice(startOffset, sizeof(long), out nextStartOffset);

            return MemoryMarshal.Read<long>(longBytes);
        }

        /// <summary>
        /// Получить <see cref="DateTime"/> из среза массива байт.
        /// </summary>
        /// <param name="sourceBuffer">Срез массива байт для конвертации.</param>
        /// <param name="startOffset">Начальный сдвиг.</param>
        /// <param name="nextStartOffset">Оффсет после чтения.</param>
        /// <returns>DateTime.</returns>
        public static DateTime DecodeDateTime(this Span<byte> sourceBuffer, int startOffset, out int nextStartOffset)
        {
            // DateTime -> 8 Byte, as Long.
            var bytes = sourceBuffer.Slice(startOffset, sizeof(long), out nextStartOffset);

            return MemoryMarshal.Read<DateTime>(bytes);
        }

        /// <summary>
        /// Получить срез массива.
        /// </summary>
        /// <param name="sourceBuffer">Срез массива байт для среза.</param>
        /// <param name="length">Длина среза.</param>
        /// <param name="startOffset">Начальный сдвиг.</param>
        /// <param name="nextStartOffset">Оффсет после среза.</param>
        /// <returns>Срезанный массив байт указанной длины.</returns>
        public static Span<byte> Slice(this Span<byte> sourceBuffer, int startOffset, int length, out int nextStartOffset)
        {
            if (sourceBuffer.Length == 0)
            {
                nextStartOffset = startOffset;

                return new byte[length];
            }
            nextStartOffset = startOffset + length;

            return sourceBuffer.Slice(startOffset, length);
        }

        #endregion Span реализации

        /// <summary>
        /// Собрать переданный массив массивов байт в один массив.
        /// </summary>
        /// <param name="arrays">Массив массивов байт.</param>
        /// <returns>Новый массив собранный из переданных.</returns>
        public static byte[] Flatten(params byte[][] arrays)
        {
            var destination = new byte[arrays.Sum(x => x.Length)];
            var offset = 0;
            foreach (var data in arrays)
            {
                Buffer.BlockCopy(data, 0, destination, offset, data.Length);
                offset += data.Length;
            }

            return destination;
        }

        /// <summary>
        /// Собрать переданное перечисление массивов байт в один массив.
        /// </summary>
        /// <param name="arrays">Перечисление массивов байт.</param>
        /// <returns>Новый массив собранный из переданных.</returns>
        public static byte[] Flatten(IEnumerable<byte[]> arrays)
        {
            var destination = new byte[arrays.Sum(x => x.Length)];
            var offset = 0;
            foreach (var data in arrays)
            {
                Buffer.BlockCopy(data, 0, destination, offset, data.Length);
                offset += data.Length;
            }

            return destination;
        }
    }
}
