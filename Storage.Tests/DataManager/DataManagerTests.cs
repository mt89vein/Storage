﻿using NUnit.Framework;
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

        /// <summary>
        /// Менеджер страниц.
        /// </summary>
        private DataPageManager _dataPageManager;

        /// <summary>
        /// Конфигурация менеджера страниц данных.
        /// </summary>
        private readonly DataPageManagerConfig _config = new DataPageManagerConfig("DataPageManager-1", PageSize, TempFilesDirectory);
        
        #endregion Поля

        #region Clean/Prepare management

        [TearDown]
        public void ClearTempFilesDirectory()
        {
            _dataPageManager.Dispose();
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

            _dataPageManager = new DataPageManager(_config);
        }

        #endregion Clean/Prepare management

        #region Тесты

        [Test]
        [Description("Корректно записывает данные, которые не влезают на одну страницу целиком.")]
        public async Task MultiPageWriteTest()
        {
            Assert.AreEqual(0, _dataPageManager.DataPagesCount);
            Assert.IsNull(_dataPageManager.Read(1));
            Assert.IsNull(_dataPageManager.Read(2));
            Assert.IsNull(_dataPageManager.Read(3));

            var data = new string('5', 500);
            _dataPageManager.Save(new Core.Models.DataRecord(1, Encoding.UTF8.GetBytes(new string('1', 100))));
            _dataPageManager.Save(new Core.Models.DataRecord(2, Encoding.UTF8.GetBytes(data)));

            await Task.Delay(500); // даём время на автосохранение.

            var dataRecord = _dataPageManager.Read(2);

            Assert.AreEqual(data, Encoding.UTF8.GetString(dataRecord.Body));
        }

        [Test]
        [Description("Корректно записывает данные, которые влезают на одну страницу целиком.")]
        public async Task SinglePageWriteTest()
        {
            Assert.AreEqual(0, _dataPageManager.DataPagesCount);
            Assert.IsNull(_dataPageManager.Read(1));
            Assert.IsNull(_dataPageManager.Read(2));
            Assert.IsNull(_dataPageManager.Read(3));

            var data1 = new string('1', 50);
            var data2 = new string('2', 70);
            _dataPageManager.Save(new Core.Models.DataRecord(1, Encoding.UTF8.GetBytes(data1)));
            _dataPageManager.Save(new Core.Models.DataRecord(2, Encoding.UTF8.GetBytes(data2)));

            await Task.Delay(500); // даём время на автосохранение.

            var dataRecord1 = _dataPageManager.Read(1);
            var dataRecord2 = _dataPageManager.Read(2);

            Assert.AreEqual(data1, Encoding.UTF8.GetString(dataRecord1.Body));
            Assert.AreEqual(data2, Encoding.UTF8.GetString(dataRecord2.Body));
        }

        #endregion Тесты
    }
}
