using NUnit.Framework;
using Storage.Core.Configuration;
using Storage.Core.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Storage.Tests.DataRecordIndexStorage
{
    [TestFixture, Description("Тесты на корректную работу B-tree индекса (проверки чтения/записи в файлы)")]
    public class DataRecordIndexStorageFileTests
    {
        #region Константы

        /// <summary>
        /// Директория с подготовленными файлами для проверки чтения/записи.
        /// </summary>
        private const string MockFilesDirectory = "./DataRecordIndexStorage/MockFiles";

        #endregion Константы

        #region Тесты

        [Test, Description("Корректно добавляет одностраничный указатель в индекс (в файл).")]
        public async Task CorrectlyAddSinglePageIndex()
        {
            var path = Path.Combine(MockFilesDirectory, "singlepage-write");
            var fileInfo = new FileInfo(Path.Combine(path, "data-record-index-pointers.index"));
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
                await Task.Delay(100);
            }

            var dataRecordIndexPointer = new DataRecordIndexPointer(1, 10, 0, 132);
            DataRecordIndexPointer foundPointer;
            bool isFound;
            using (var dataRecordIndexStore = new Core.DataRecordIndexStorage(path, new DataRecordIndexStoreConfig(TimeSpan.FromMilliseconds(200))))
            {
                dataRecordIndexStore.AddToIndex(dataRecordIndexPointer);
                isFound = dataRecordIndexStore.TryGetIndex(dataRecordIndexPointer.DataRecordId, out foundPointer);
            }

            Assert.Multiple(() =>
            {
                Assert.IsTrue(isFound, "Данные по индексу должны найтись.");
                Assert.AreEqual(dataRecordIndexPointer, foundPointer, "То что положили, то и должны вернуть.");
                Assert.AreEqual(File.ReadAllBytes(Path.Combine(path, "expected.index")), File.ReadAllBytes(fileInfo.FullName));
            });
        }

        [Test, Description("Корректно читает одностраничный указатель в индекс (из файла).")]
        public void CorrectlyReadSinglePageIndex()
        {
            var path = Path.Combine(MockFilesDirectory, "singlepage-read");

            DataRecordIndexPointer dataRecordIndexPointer;
            DataRecordIndexPointer foundIndexPointer;
            bool isFound;
            using (var dataRecordIndexStore = new Core.DataRecordIndexStorage(path, new DataRecordIndexStoreConfig(TimeSpan.FromMilliseconds(200))))
            {
                dataRecordIndexPointer = new DataRecordIndexPointer(1, 10, 0, 132);

                isFound = dataRecordIndexStore.TryGetIndex(dataRecordIndexPointer.DataRecordId, out foundIndexPointer);
            }

            Assert.Multiple(() =>
            {
                Assert.IsTrue(isFound, "Данные по индексу должны найтись.");
                Assert.AreEqual(dataRecordIndexPointer, foundIndexPointer, "То что положили, то и должны вернуть.");
            });
        }

        [Test, Description("Корректно добавляет мультистраничный указатель в индекс (в файл).")]
        public async Task CorrectlyAddMultiPageIndex()
        {
            var path = Path.Combine(MockFilesDirectory, "multipage-write");
            var fileInfo = new FileInfo(Path.Combine(path, "data-record-index-pointers.index"));
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
                await Task.Delay(100);
            }

            DataRecordIndexPointer multipageIndex;
            DataRecordIndexPointer foundPointer;
            bool isFound;
            using (var dataRecordIndexStore = new Core.DataRecordIndexStorage(path, new DataRecordIndexStoreConfig(TimeSpan.FromMilliseconds(200))))
            {
                var dataRecordIndexPointer1 = new DataRecordIndexPointer(1, 1, 0, 512);
                var dataRecordIndexPointer2 = new DataRecordIndexPointer(1, 2, 0, 512);
                var dataRecordIndexPointer3 = new DataRecordIndexPointer(1, 3, 0, 256);

                multipageIndex = new DataRecordIndexPointer(
                    dataRecordIndexPointer1.DataRecordId,
                    dataRecordIndexPointer1.DataPageNumber,
                    dataRecordIndexPointer1.Offset,
                    dataRecordIndexPointer1.Length,
                    dataRecordIndexPointer2,
                    dataRecordIndexPointer3
                );

                dataRecordIndexStore.AddToIndex(multipageIndex);
                isFound = dataRecordIndexStore.TryGetIndex(multipageIndex.DataRecordId, out foundPointer);
            }

            Assert.Multiple(() =>
            {
                Assert.IsTrue(isFound, "Данные по индексу должны найтись.");
                Assert.AreEqual(multipageIndex, foundPointer, "То что положили, то и должны вернуть.");
                Assert.AreEqual(multipageIndex.AdditionalDataRecordIndexPointers.Length, foundPointer.AdditionalDataRecordIndexPointers.Length, "То что положили, то и должны вернуть.");
                Assert.AreEqual(File.ReadAllBytes(Path.Combine(path, "expected.index")), File.ReadAllBytes(fileInfo.FullName));
            });
        }

        [Test, Description("Корректно читает мультистраничный указатель в индекс (из файла).")]
        public void CorrectlyReadMultiPageIndex()
        {
            var path = Path.Combine(MockFilesDirectory, "multipage-read");

            DataRecordIndexPointer multipageIndex;
            DataRecordIndexPointer foundPointer;
            bool isFound;
            using (var dataRecordIndexStore = new Core.DataRecordIndexStorage(path, new DataRecordIndexStoreConfig(TimeSpan.FromMilliseconds(200))))
            {
                var dataRecordIndexPointer1 = new DataRecordIndexPointer(1, 1, 0, 512);
                var dataRecordIndexPointer2 = new DataRecordIndexPointer(1, 2, 0, 512);
                var dataRecordIndexPointer3 = new DataRecordIndexPointer(1, 3, 0, 256);

                multipageIndex = new DataRecordIndexPointer(
                    dataRecordIndexPointer1.DataRecordId,
                    dataRecordIndexPointer1.DataPageNumber,
                    dataRecordIndexPointer1.Offset,
                    dataRecordIndexPointer1.Length,
                    dataRecordIndexPointer2,
                    dataRecordIndexPointer3
                );

                isFound = dataRecordIndexStore.TryGetIndex(multipageIndex.DataRecordId, out foundPointer);
            }

            Assert.Multiple(() =>
            {
                Assert.IsTrue(isFound, "Данные по индексу должны найтись.");
                Assert.AreEqual(multipageIndex, foundPointer, "То что положили, то и должны вернуть.");
                Assert.AreEqual(multipageIndex.AdditionalDataRecordIndexPointers.Length, foundPointer.AdditionalDataRecordIndexPointers.Length, "То что положили, то и должны вернуть.");
            });
        }
        
        #endregion Тесты
    }
}