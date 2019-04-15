using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Storage.Core.Helpers
{
    /// <summary>
    /// Менеджер неуправляемой памяти.
    /// </summary>
    internal class UmsManager : IDisposable
    {
        /// <summary>
        /// Указатель на участок неуправляемой памяти.
        /// </summary>
        private readonly IntPtr _memory;

        /// <summary>
        /// Конструктор с созданием экземпляра <see cref="UmsManager"/> с указанным объёмом памяти, без начальных данных.
        /// </summary>
        /// <param name="capacity">Объём памяти в байтах.</param>
        public unsafe UmsManager(int capacity)
        {
            _memory = Marshal.AllocHGlobal(capacity);
            var bytes = (byte*)_memory.ToPointer();
            var currentByte = bytes;
            for (var index = 0; index < capacity; index++)
            {
                *currentByte = 0;
                currentByte++;
            }

            Stream = new UnmanagedMemoryStream(bytes, capacity, capacity, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Конструктор с созданием экземпляра <see cref="UmsManager"/> с указанным массивом байт.
        /// </summary>
        /// <param name="bytes">Данные.</param>
        public unsafe UmsManager(byte[] bytes)
        {
            var memorySizeInBytes = bytes.Length;
            _memory = Marshal.AllocHGlobal(memorySizeInBytes);
            var destination = (byte*)_memory.ToPointer();
            fixed (byte* source = bytes)
            {
                Buffer.MemoryCopy(source, destination, memorySizeInBytes, memorySizeInBytes);
            }

            Stream = new UnmanagedMemoryStream(destination, memorySizeInBytes, memorySizeInBytes, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Поток из памяти неуправляемых данных.
        /// </summary>
        public UnmanagedMemoryStream Stream { get; }

        /// <summary>
        /// Получить данные в формате массива байт.
        /// </summary>
        /// <returns>Массив байт.</returns>
        public byte[] ToArray()
        {
            var count = Stream.Length;
            var array = new byte[count];
            unsafe
            {
                var source = (byte*)_memory.ToPointer();
                fixed (byte* destination = array)
                {
                    Buffer.MemoryCopy(source, destination, array.Length, count);
                }
            }

            return array;
        }

        #region IDisposable

        /// <summary>
        /// Высвобождает неуправляемые ресурсы.
        /// </summary>
        public void Dispose()
        {
            Stream.Dispose();
            Marshal.FreeHGlobal(_memory);
        }

        #endregion
    }
}