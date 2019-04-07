using System;
using System.Linq;

namespace Storage.Core.Helpers
{
    /// <summary>
    /// Вспомогательные методы для работы с массивами байт.
    /// </summary>
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Собрать переданный массив массивов байт в один массив.
        /// </summary>
        /// <param name="arrays">Массив массивов байт.</param>
        /// <returns>Новый массив собранный из переданных.</returns>
        public static byte[] Flatten(params byte[][] arrays)
        {
            var ret = new byte[arrays.Sum(x => x.Length)];
            var offset = 0;
            foreach (var data in arrays)
            {
                Buffer.BlockCopy(data, 0, ret, offset, data.Length);
                offset += data.Length;
            }

            return ret;
        }
    }
}