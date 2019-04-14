using NUnit.Framework;
using Storage.Core.Configuration;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Storage.Core.Models;

namespace Storage.Tests.DataPage
{
    [TestFixture]
    public class DataPageFileReadWriteTests
    {
        #region Поля

        /// <summary>
        /// Конфигурация страницы.
        /// </summary>
        private readonly DataPageConfig _config = new DataPageConfig("manager-1", PageSize, TimeSpan.FromMilliseconds(500));

        /// <summary>
        /// Директория для хранения временных файлов.
        /// </summary>
        private const string MockFilesDirectory = "./DataPage/mockFiles";

        /// <summary>
        /// Размер страницы.
        /// </summary>
        private const int PageSize = 8196;

        #endregion Поля

        #region Тесты

        [Test]
        [Description("Проверка корректной записи в файл. Инициализирует страницу с нуля.")]
        public async Task CorrectlyCreatesAndWritesToFile()
        {
            var path = Path.Combine(MockFilesDirectory, "write-to-file-test");

            var dataPageFile = Path.Combine(path, "datapage-1");
            var fileInfo = new FileInfo(dataPageFile);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            const string stringToWrite = "Hello world!";
            var dataRecord = new Core.Models.DataRecord(1, Encoding.UTF8.GetBytes(stringToWrite));
            var bytes = dataRecord.GetBytes();
            int currentOffset;
            int offset;
            Core.Models.DataRecord dataRecordReaded;
            int freeSpaceLength;
            using (var dataPage = new Core.Models.DataPage(_config, 1, fileInfo.FullName, false))
            {
                currentOffset = _config.PageSize - bytes.Length;
                dataPage.TrySaveData(dataRecord.Id, bytes, out offset);
                await Task.Delay(650); // даём время на автосохранение.
                dataRecordReaded = dataPage.Read(offset, bytes.Length);
                freeSpaceLength = dataPage.GetFreeSpaceLength();
            }

            Assert.Multiple(() =>
            {
                Assert.AreEqual(currentOffset, offset, "Оффсет должен быть корректным.");
                Assert.AreEqual(_config.PageSize - bytes.Length - DataPageLocalIndex.Size - DataPageHeader.Size, freeSpaceLength, "Количество свободного места должно уменьшиться на кол-во записанных байт.");
                Assert.AreEqual(stringToWrite, Encoding.UTF8.GetString(dataRecordReaded.Body), "Записанные данные должны корректно восстановиться.");
                Assert.AreEqual(File.ReadAllBytes(Path.Combine(path, "expected-datapage-1")), File.ReadAllBytes(fileInfo.FullName), "Контент должен совпасть.");
            });
        }

        [Test]
        [Description("Проверка корректного чтения из файла. Инициализирует начатую (не завершенную) страницу.")]
        public void CorrectlyReadFromExistingFile()
        {
            var path = Path.Combine(MockFilesDirectory, "read-write-to-file-test");
            var dataPageFile = Path.Combine(path, "datapage-1");

            using (var dataPage = new Core.Models.DataPage(_config, 1, dataPageFile, false))
            {
                const string stringToWrite = "Hello world!";

                var dataRecord = dataPage.Read(_config.PageSize - 24, 24);

                Assert.AreEqual(stringToWrite, Encoding.UTF8.GetString(dataRecord.Body), "Записанные данные должны корректно восстановиться.");
            }
        }

        #endregion Тесты
    }
}