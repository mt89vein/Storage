using Storage.Core.Models;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Storage.Core.Abstractions;
using Storage.Core.FileNamingStrategies;

namespace Storage.Core
{
	/*
	 * Зона ответственности: управление списком DataPage, обновление индекса при вставке, управление созданием  новых страничек, метод для очистки старых.
	 * Загрузка из диска индексов и имеющихся страниц данных (карта)
	 */
	/// <summary>
	/// Менеджер страниц данных.
	/// </summary>
	public sealed class DataPageManager // TODO: IDisposable
	{
		/// <summary>
		/// Список страниц данных.
		/// </summary>
		private readonly ConcurrentDictionary<int, DataPage> _dataPages;

		/// <summary>
		/// Индекс по <see cref="DataRecord.Id"/>
		/// </summary>
		private readonly IDataRecordIndexStore _dataRecordIndexStore;

        /// <summary>
        /// Конфигурация менеджера страниц.
        /// </summary>
        private readonly DataPageManagerConfig _config;

		/// <summary>
		/// Стратегия формирования названия файлов.
		/// </summary>
		private readonly IFileNamingStrategy _dataPageFileNamingStrategy;

		/// <summary>
		/// Название менеджера страниц.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Количество страниц.
		/// </summary>
		public int DataPagesCount => _dataPages.Count;

		/// <summary>
		/// Номер следующего чанка.
		/// </summary>
		private int NextDataPageNumber => _dataPages.IsEmpty 
			? 1 
			: _dataPages.Keys.Max();

		/// <summary>
		/// Лок объект для создания страницы.
		/// </summary>
		private readonly object _createDataPageLock = new object();

		/// <summary>
		/// Создает менеджера страниц данных.
		/// </summary>
		public DataPageManager(DataPageManagerConfig config)
		{
            _config = config;
			if (!Directory.Exists(_config.Directory))
			{
				Directory.CreateDirectory(_config.Directory);
			}

			_dataPageFileNamingStrategy = new HierarchyFileNamingStrategy(_config.Directory);
			_dataPages = new ConcurrentDictionary<int, DataPage>();
			_dataRecordIndexStore = new DataRecordIndexStore(_config.Directory);
			LoadMetaData();
		}

        /// <summary>
        /// Сохранить данные на диск.
        /// </summary>
        /// <param name="record">Модель данных.</param>
        /// TODO: реализовать multipage хранение.
        public void Save(DataRecord record)
		{
			var data = record.GetBytes();

			var currentDataPage = GetLastPage();
			if (!currentDataPage.HasSpaceFor(data))
			{
				currentDataPage.SetCompleted();
				currentDataPage = CreateNew();
			}

			if (currentDataPage.TrySaveData(data, out var dataOffset))
			{
				_dataRecordIndexStore.AddToIndex(new DataRecordIndexPointer(record.Header.Id, currentDataPage.PageId, dataOffset, data.Length));
			}

			// TODO: throw new NotSavedException();
		}

		/// <summary>
		/// Получить последнюю страницу данных.
		/// </summary>
		/// <returns>Страница данных.</returns>
		public DataPage GetLastPage()
		{
			lock (_createDataPageLock)
			{
				if (_dataPages.IsEmpty)
				{
					CreateNew();
				}

				return _dataPages[NextDataPageNumber];
			}
		}

		/// <summary>
		/// Получить контейнер по идентификатору.
		/// </summary>
		/// <param name="recordId">Идентификатор записи.</param>
		/// <returns>Контейнер данных.</returns>
		public DataRecord Read(long recordId)
		{
			if (!_dataRecordIndexStore.TryGetIndex(recordId, out var index))
			{
				// TODO: implement FULL SCAN
				// можно сделать счетчик индекс миссов, и если по-какому то из dataPage слишком много миссов, то нужно перестроить индекс, побыстрому в фоне.
				// DataPageHeader.DataRecordStartId добавить в заголовок и поиск упрощается в разы:
				// _dataPages.FirstOrDefault(dp => dp.PageId.DataRecordStartId >= dataRecordId) и сканить уже по найденной странице. Если не удалось и так найти.. то тут косяк, надо бежать по всем страницам...)

				// Учесть тот факт, что в файл данные могут писаться не по порядку.
				return null;
			}

			var dataPage = GetDataPage(index.DataPageNumber);
			return dataPage?.Read(index.Offset, index.Length);
		}

		#region Методы (private)

		/// <summary>
		/// Создать новую страницу.
		/// </summary>
		/// <returns>Новая страница.</returns>
		private DataPage CreateNew()
		{
			lock (_createDataPageLock)
			{
				var dataPageNumber = NextDataPageNumber;
				var fileName = _dataPageFileNamingStrategy.GetFileNameFor(dataPageNumber);
				var dataPage = new DataPage(_config.DataPageConfig, dataPageNumber, fileName, false);

				AddDataPage(dataPage);

				return dataPage;
			}
		}

		/// <summary>
		/// Добавить страницу в словарь.
		/// </summary>
		/// <param name="page">Страница данных.</param>
		private void AddDataPage(DataPage page)
		{
			while (!_dataPages.TryAdd(page.PageId, page))
			{
				Thread.Sleep(1);
			}
		}

		/// <summary>
		/// Загрузить в память метаданные страниц.
		/// </summary>
		private void LoadMetaData()
		{
			lock (_createDataPageLock)
            {
                var files = _dataPageFileNamingStrategy.GetFiles();

                foreach (var file in files.Reverse().Skip(1))
				{
					AddDataPage(new DataPage(_config.DataPageConfig, file.Key, file.Value, true));
				}

                var lastDataPage = files.Last();
                AddDataPage(new DataPage(_config.DataPageConfig, lastDataPage.Key, lastDataPage.Value, false));
            }

			// TODO: сделать проверку на индексы (т.е. индекс должен быть корректно восстановлен, например чтобы последний элемент индекса корректно ссылался на последний элемент страницы, сделать перестройку индекса, если проверка не удалась.
		}

		/// <summary>
		/// Получить страницу данных по номеру.
		/// </summary>
		/// <param name="pageNumber">Номер страницы.</param>
		/// <returns>Страница данных.</returns>
		private DataPage GetDataPage(int pageNumber)
		{
			return _dataPages.TryGetValue(pageNumber, out var dataPage)
				? dataPage
				: null;
		}

		#endregion Методы (private)
	}
}
