using NUnit.Framework;
using Storage.Core;
using Storage.Core.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Storage.Tests.DataManager
{
    // TODO: сделать тесты на чтение уже существующих страниц. (т.е. не с нуля)
    [TestFixture]
    public class DataManagerTests
    {
        #region Поля

        /// <summary>
        /// Директория для хранения временных файлов.
        /// </summary>
        private const string TempFilesDirectory = "./DataManager/tempFiles";

        /// <summary>
        /// Размер страницы данных.
        /// </summary>
        private const int PageSize = 100;
        
        #endregion Поля

        #region Тесты

        [Test]
        [Description("Корректно записывает данные, которые не влезают на одну страницу целиком.")]
        public void MultiPageWriteTest()
        {
            const string pageManagerName = "DataPageManager-1";

            PrepareFor(pageManagerName);

            using (var dataPageManager = new DataPageManager(new DataPageManagerConfig(pageManagerName, PageSize, TempFilesDirectory)))
            {
                Assert.Multiple(() =>
                {
                    Assert.AreEqual(0, dataPageManager.DataPagesCount);
                    Assert.IsNull(dataPageManager.Read(1));
                    Assert.IsNull(dataPageManager.Read(2));
                    Assert.IsNull(dataPageManager.Read(3));
                });

                dataPageManager.Save(Core.Models.DataRecord.Create(Encoding.UTF8.GetBytes(new string('1', 100))));

                var data = new string('5', 500);
                var dataRecord = Core.Models.DataRecord.Create(Encoding.UTF8.GetBytes(data));
                dataPageManager.Save(dataRecord);

                var readedDataRecord = dataPageManager.Read(dataRecord.Id);

                Assert.AreEqual(data, Encoding.UTF8.GetString(readedDataRecord.Body));
            }
        }

        [Test]
        [Description("Корректно записывает данные, которые влезают на одну страницу целиком.")]
        public void SinglePageWriteTest()
        {
            const string pageManagerName = "DataPageManager-2";

            PrepareFor(pageManagerName);

            using (var dataPageManager = new DataPageManager(new DataPageManagerConfig(pageManagerName, PageSize, TempFilesDirectory)))
            {
                Assert.Multiple(() =>
                {
                    Assert.AreEqual(0, dataPageManager.DataPagesCount);
                    Assert.IsNull(dataPageManager.Read(1));
                    Assert.IsNull(dataPageManager.Read(2));
                    Assert.IsNull(dataPageManager.Read(3));
                });

                var data1 = new string('1', 50);
                var data2 = new string('2', 70);
                var dr1 = Core.Models.DataRecord.Create(Encoding.UTF8.GetBytes(data1));
                var dr2 = Core.Models.DataRecord.Create(Encoding.UTF8.GetBytes(data2));
                dataPageManager.Save(dr1);
                dataPageManager.Save(dr2);

                var dataRecord1 = dataPageManager.Read(dr1.Id);
                var dataRecord2 = dataPageManager.Read(dr2.Id);

                Assert.AreEqual(data1, Encoding.UTF8.GetString(dataRecord1.Body));
                Assert.AreEqual(data2, Encoding.UTF8.GetString(dataRecord2.Body));
            }
        }

        #endregion Тесты

        #region Вспомогательные методы

        /// <summary>
        /// Производит подготовку перед тестом.
        /// </summary>
        /// <param name="scopedPath">Путь.</param>
        private static void PrepareFor(string scopedPath)
        {
            var directory = Path.Combine(TempFilesDirectory, scopedPath);
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        #endregion Вспомогательные методы
    }
}
