﻿using NUnit.Framework;
using Storage.Core.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Storage.Tests.DataPage
{
    [TestFixture]
    public class DataPageBasicTests
    {
        #region Поля

        /// <summary>
        /// Тестируемая страница данных.
        /// </summary>
        private Core.Models.DataPage _dataPage;

        /// <summary>
        /// Конфигурация страницы.
        /// </summary>
        private readonly DataPageConfig _config = new DataPageConfig("manager-1", PageSize, TimeSpan.FromMilliseconds(500));

        /// <summary>
        /// Директория для хранения временных файлов.
        /// </summary>
        private const string TempFilesDirectory = "./DataPage/tempFiles";

        /// <summary>
        /// Размер страницы.
        /// </summary>
        private const int PageSize = 8196;

        #endregion Поля

        #region Clean/Prepare management

        [TearDown]
        public void ClearTempFilesDirectory()
        {
            _dataPage.Dispose();
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

            _dataPage = new Core.Models.DataPage(_config, 1, Path.Combine(TempFilesDirectory, "datapage-1"), true);
        }

        #endregion Clean/Prepare management

        #region Тесты

        [Test]
        [Description("Для только что созданной страницы размер свободного места должен равняться размеру страницы в конфиге.")]
        public void CorrectFreeSpaceAtStart()
        {
            Assert.AreEqual(_config.PageSize, _dataPage.GetFreeSpaceLength());
        }

        [Test]
        [Description("Обновляется время последней активности при действиях.")]
        public async Task CorrectlyUpdatesActiveTime()
        {
            var initialActiveTime = _dataPage.LastActiveTime;

            _dataPage.TrySaveData(new byte[12], out var offset);
            var activeTimeAfterSaveData = _dataPage.LastActiveTime;
            Assert.AreNotEqual(initialActiveTime, activeTimeAfterSaveData);
            await Task.Delay(500); // дожидаемся сохранения.

            _dataPage.Read(offset, 12);
            var activeTimeAfterRead = _dataPage.LastActiveTime;
            Assert.AreNotEqual(activeTimeAfterSaveData, activeTimeAfterRead);

            _dataPage.ReadBytes(offset, 12);
            var activeTimeAfterReadBytes = _dataPage.LastActiveTime;
            Assert.AreNotEqual(activeTimeAfterRead, activeTimeAfterReadBytes);
        }

        [Test]
        [Description("Размер оставшейся свободной памяти должен вычисляться корректно.")]
        public void CorrectlyCalculateFreeSpaceLength()
        {
            var bytesToWriteArray = new[] { 500, 200, 400 };

            var initialFreeSpace = _dataPage.GetFreeSpaceLength();

            // убедимся что ничего не записано.
            Assert.AreEqual(_config.PageSize, initialFreeSpace);

            foreach (var bytesToWrite in bytesToWriteArray)
            {
                _dataPage.TrySaveData(new byte[bytesToWrite], out _);
                Assert.AreEqual(initialFreeSpace - bytesToWrite, _dataPage.GetFreeSpaceLength());
                initialFreeSpace -= bytesToWrite;
            }
        }

        #endregion Тесты
    }
}
