using Storage.Core.Models;

namespace Storage.Core.Abstractions
{
	/// <summary>
	/// Индекс по <see cref="DataRecord.Id"/>
	/// </summary>
	public interface IDataRecordIndexStore
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
	}
}