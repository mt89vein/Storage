using NUnit.Framework;
using Storage.Core.Configuration;
using System;
using System.IO;
using System.Text;
using System.Threading;

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
        public void CorrectlyCreatesAndWritesToFile()
        {
            var path = Path.Combine(MockFilesDirectory, "write-to-file-test");

            var dataPageFile = Path.Combine(path, "datapage-1");
            var fileInfo = new FileInfo(dataPageFile);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            var dataPage = new Core.Models.DataPage(_config, 1, fileInfo.FullName, false);
            var currentOffset = _config.PageSize - dataPage.GetFreeSpaceLength();
            const string stringToWrite = "Hello world!";
            var dataRecord = new Core.Models.DataRecord(1, Encoding.UTF8.GetBytes(stringToWrite));

            var bytes = dataRecord.GetBytes();
            dataPage.TrySaveData(bytes, out var offset);
            Thread.Sleep(1000); // даём время на срабатывание автоматической записи в файл.
            var dataRecordReaded = dataPage.Read(offset, bytes.Length);

            dataPage.Dispose();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(currentOffset, offset, "Оффсет должен быть корректным.");
                Assert.AreEqual(_config.PageSize - bytes.Length, dataPage.GetFreeSpaceLength(), "Количество свободного места должно уменьшиться на кол-во записанных байт.");
                Assert.AreEqual(stringToWrite, Encoding.UTF8.GetString(dataRecordReaded.Body.ToByteArray()), "Записанные данные должны корректно восстановиться.");
                Assert.AreEqual(File.ReadAllBytes(Path.Combine(path, "expected-datapage-1")), File.ReadAllBytes(fileInfo.FullName), "Контент должен совпасть.");
            });
        }

        [Test]
        [Description("Проверка корректного чтения из файла. Инициализирует начатую (не завершенную) страницу.")]
        public void CorrectlyReadFromExistingFile()
        {
            var path = Path.Combine(MockFilesDirectory, "read-write-to-file-test");
            var dataPageFile = Path.Combine(path, "datapage-1");
            var dataPage = new Core.Models.DataPage(_config, 1, dataPageFile, false);

            const string stringToWrite = "Hello world!";

            var dataRecord = dataPage.Read(0, 20);

            dataPage.Dispose();

            Assert.AreEqual(stringToWrite, Encoding.UTF8.GetString(dataRecord.Body.ToByteArray()), "Записанные данные должны корректно восстановиться.");
        }

        #endregion Тесты
    }
}