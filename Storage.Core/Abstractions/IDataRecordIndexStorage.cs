﻿using System;
using System.Collections.Generic;
using Storage.Core.Models;

namespace Storage.Core.Abstractions
{
    /// <summary>
    /// Индекс по <see cref="DataRecord" />
    /// </summary>
    public interface IDataRecordIndexStorage : IDisposable
    {
        /// <summary>
        /// Попытаться найти из индекса.
        /// </summary>
        /// <param name="recordId">Идентификатор записи.</param>
        /// <param name="recordIndexPointer">Указатель на данные.</param>
        /// <returns>True, если удалось найти данные.</returns>
        bool TryGetIndex(long recordId, out DataRecordIndexPointer recordIndexPointer);

        /// <summary>
        /// Добавить указатель в индекс.
        /// </summary>
        /// <param name="recordIndexPointer">Указатель для добавления в индекс.</param>
        void AddToIndex(DataRecordIndexPointer recordIndexPointer);

        /// <summary>
        /// Получить индекс для итерирования с указанного идентификатора записи.
        /// </summary>
        /// <param name="fromRecordId">Идентификатор записи, начиная с которого нужен итератор.</param>
        /// <returns>Итератор.</returns>
        IEnumerable<DataRecordIndexPointer> AsEnumerable(long fromRecordId = 0);

        /// <summary>
        /// Почистить все данные. Используется для перестройки индекса.
        /// </summary>
        void Clear();
    }
}