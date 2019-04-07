using NUnit.Framework;
using Storage.Core.Models;
using System.IO;

namespace Storage.Tests.DataRecordIndexStore
{
    [TestFixture, Description("Тесты на корректную работу B-tree индекса (проверки чтения/записи в файлы)")]
    public class DataRecordIndexStoreFileTests
    {
        #region Константы

        /// <summary>
        /// Директория с подготовленными файлами для проверки чтения/записи.
        /// </summary>
        private const string MockFilesDirectory = "./DataRecordIndexStore/MockFiles";

        #endregion Константы

        #region Тесты

        [Test, Description("Корректно добавляет одностраничный указатель в индекс (в файл).")]
        public void CorrectlyAddSinglePageIndex()
        {
            var path = Path.Combine(MockFilesDirectory, "singlepage-write");
            var fileInfo = new FileInfo(Path.Combine(path, "data-record-index-pointers.index"));
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            var dataRecordIndexStore = new Core.DataRecordIndexStore(path);

            var dataRecordIndexPointer = new DataRecordIndexPointer(1, 10, 0, 132);
            dataRecordIndexStore.AddToIndex(dataRecordIndexPointer);

            var isFound = dataRecordIndexStore.TryGetIndex(dataRecordIndexPointer.DataRecordId, out var pointer);
            dataRecordIndexStore.Dispose();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(isFound, "Данные по индексу должны найтись.");
                Assert.AreEqual(dataRecordIndexPointer, pointer, "То что положили, то и должны вернуть.");
                Assert.AreEqual(File.ReadAllBytes(Path.Combine(path, "expected.index")), File.ReadAllBytes(fileInfo.FullName));
            });
        }

        [Test, Description("Корректно читает одностраничный указатель в индекс (из файла).")]
        public void CorrectlyReadSinglePageIndex()
        {
            var path = Path.Combine(MockFilesDirectory, "singlepage-read");
            var dataRecordIndexStore = new Core.DataRecordIndexStore(path);

            var dataRecordIndexPointer = new DataRecordIndexPointer(1, 10, 0, 132);

            var isFound = dataRecordIndexStore.TryGetIndex(dataRecordIndexPointer.DataRecordId, out var pointer);
            dataRecordIndexStore.Dispose();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(isFound, "Данные по индексу должны найтись.");
                Assert.AreEqual(dataRecordIndexPointer, pointer, "То что положили, то и должны вернуть.");
            });
        }

        [Test, Description("Корректно добавляет мультистраничный указатель в индекс (в файл).")]
        public void CorrectlyAddMultiPageIndex()
        {
            var path = Path.Combine(MockFilesDirectory, "multipage-write");
            var fileInfo = new FileInfo(Path.Combine(path, "data-record-index-pointers.index"));
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            var dataRecordIndexStore = new Core.DataRecordIndexStore(path);

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

            dataRecordIndexStore.AddToIndex(multipageIndex);

            var isFound = dataRecordIndexStore.TryGetIndex(multipageIndex.DataRecordId, out var pointer);
            dataRecordIndexStore.Dispose();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(isFound, "Данные по индексу должны найтись.");
                Assert.AreEqual(multipageIndex, pointer, "То что положили, то и должны вернуть.");
                Assert.AreEqual(multipageIndex.AdditionalDataRecordIndexPointers.Length, pointer.AdditionalDataRecordIndexPointers.Length, "То что положили, то и должны вернуть.");
                Assert.AreEqual(File.ReadAllBytes(Path.Combine(path, "expected.index")), File.ReadAllBytes(fileInfo.FullName));
            });
        }

        [Test, Description("Корректно читает мультистраничный указатель в индекс (из файла).")]
        public void CorrectlyReadMultiPageIndex()
        {
            var path = Path.Combine(MockFilesDirectory, "multipage-read");
            var dataRecordIndexStore = new Core.DataRecordIndexStore(path);

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

            var isFound = dataRecordIndexStore.TryGetIndex(multipageIndex.DataRecordId, out var pointer);
            dataRecordIndexStore.Dispose();

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