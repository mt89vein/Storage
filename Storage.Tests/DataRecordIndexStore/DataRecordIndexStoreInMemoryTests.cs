using NUnit.Framework;
using Storage.Core.Abstractions;
using Storage.Core.Models;
using System.IO;

namespace Storage.Tests.DataRecordIndexStore
{
    [TestFixture]
    [Description("Тесты на корректную работу B-tree индекса (без проверок файлов)")]
    public class DataRecordIndexStoreInMemoryTests
    {
        #region Поля

        /// <summary>
        /// Тестируемый сервис.
        /// </summary>
        private IDataRecordIndexStore _dataRecordIndexStore;

        /// <summary>
        /// Директория для хранения временных файлов.
        /// </summary>
        private const string TempFilesDirectory = "./DataRecordIndexStore/TempFiles";

        #endregion Поля

        #region Clean/Prepare management

        [TearDown]
        public void ClearTestFilesDirectory()
        {
            _dataRecordIndexStore.Dispose();
            if (Directory.Exists(TempFilesDirectory))
            {
                Directory.Delete(TempFilesDirectory, true);
            }
        }

        [SetUp]
        public void SetUp()
        {
            if (!Directory.Exists(TempFilesDirectory))
            {
                Directory.CreateDirectory(TempFilesDirectory);
            }
            _dataRecordIndexStore = new Core.DataRecordIndexStore(TempFilesDirectory);
        }

        #endregion Clean/Prepare management

        #region Тесты

        [Test, Description("Корректно добавляет указатель в индекс.")]
        public void CorrectlyAddToIndex()
        {
            var dataRecordIndexPointer = new DataRecordIndexPointer(1, 10, 0, 132);
            _dataRecordIndexStore.AddToIndex(dataRecordIndexPointer);

            var isFound = _dataRecordIndexStore.TryGetIndex(dataRecordIndexPointer.DataRecordId, out var pointer);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(isFound, "Данные по индексу должны найтись.");
                Assert.AreEqual(dataRecordIndexPointer, pointer, "То что положили, то и должны вернуть.");
            });
        }

        [Test, Description("Корректно читает указатель из индекса при добавлении в разном порядке.")]
        public void CorrectlyReadFromIndex()
        {
            var dataRecordIndexPointer1 = new DataRecordIndexPointer(1, 10, 0, 132);
            var dataRecordIndexPointer12 = new DataRecordIndexPointer(12, 10, 0, 132);
            var dataRecordIndexPointer2 = new DataRecordIndexPointer(2, 10, 0, 132);
            _dataRecordIndexStore.AddToIndex(dataRecordIndexPointer1);
            _dataRecordIndexStore.AddToIndex(dataRecordIndexPointer12);
            _dataRecordIndexStore.AddToIndex(dataRecordIndexPointer2);

            var isFound = _dataRecordIndexStore.TryGetIndex(dataRecordIndexPointer12.DataRecordId, out var pointer);
            var notFound13 = _dataRecordIndexStore.TryGetIndex(13, out var defaultPointer);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(isFound, "Данные по индексу должны найтись.");
                Assert.AreEqual(dataRecordIndexPointer12, pointer, "То что положили, то и должны вернуть.");

                Assert.IsFalse(notFound13);
                Assert.AreEqual(default(DataRecordIndexPointer), defaultPointer);
            });
        }

        [Test, Description("Корректно добавляет и читает мультистраничный указатель в индекс.")]
        public void CorrectlyAddMultiPageIndex()
        {
            var dataRecordIndexPointer1 = new DataRecordIndexPointer(1, 1, 0, 512);
            var dataRecordIndexPointer2 = new DataRecordIndexPointer(1, 2, 0, 512);
            var dataRecordIndexPointer3 = new DataRecordIndexPointer(1, 3, 0, 256);

            var multipageIndex = new DataRecordIndexPointer(
                dataRecordIndexPointer1.DataRecordId,
                dataRecordIndexPointer1.DataPageNumber,
                dataRecordIndexPointer1.Offset,
                dataRecordIndexPointer1.Length,
                dataRecordIndexPointer2,
                dataRecordIndexPointer3
           );

            _dataRecordIndexStore.AddToIndex(multipageIndex);

            var isFound = _dataRecordIndexStore.TryGetIndex(multipageIndex.DataRecordId, out var pointer);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(isFound, "Данные по индексу должны найтись.");
                Assert.AreEqual(multipageIndex, pointer, "То что положили, то и должны вернуть.");
                Assert.AreEqual(multipageIndex.AdditionalDataRecordIndexPointers.Length, pointer.AdditionalDataRecordIndexPointers.Length, "То что положили, то и должны вернуть.");
            });
        }

        #endregion Тесты
    }
}