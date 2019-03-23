using System.Collections.Generic;

namespace Storage.Core.Abstractions
{
	/// <summary>
	/// Интерфейс стратегии формирования названия файлов.
	/// </summary>
	public interface IFileNamingStrategy
	{
		/// <summary>
		/// Получить название файла
		/// </summary>
		/// <param name="index">Индекс.</param>
		/// <returns></returns>
		string GetFileNameFor(int index);

		/// <summary>
		/// Получить индекс по названию файла.
		/// </summary>
		/// <param name="fileName">Название файла.</param>
		/// <returns>Индекс.</returns>
		int GetIndexFor(string fileName);

		/// <summary>
		/// Получить файлы по указанному пути.
		/// </summary>
		/// <returns>Список файлов.</returns>
		IReadOnlyDictionary<int, string> GetFiles();
	}
}
