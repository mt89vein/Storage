using Google.Protobuf;
using NUnit.Framework;
using System.IO;

namespace Storage.Tests.DataRecord
{
    [TestFixture]
    public class DataRecordTests
    {
        #region Тесты

        [Test]
        [Description("Корректное восстановление из пустого экзепляра.")]
        public void EmptyBytesReconstituteRecord()
        {
            var left = new Core.Models.DataRecord();
            var right = new Core.Models.DataRecord(left.GetBytes());

            Assert.Multiple(() =>
            {
                Assert.AreEqual(left, right, "Equality");
                Assert.IsTrue(left.Equals(right), "left.Equals(right)");
                Assert.IsTrue(right.Equals(left), "right.Equals(left)");
                Assert.IsTrue(left.Equals((object)right), "left.Equals((object)right)");
                Assert.IsTrue(right.Equals((object)left), "right.Equals((object)left)");
                Assert.IsTrue(Equals(left, right), "Equals(left, right)");
                Assert.AreEqual(left.GetHashCode(), right.GetHashCode(), "HashCodes");
                Assert.IsFalse(right.Equals(null), "right.Equals(null)");
            });
        }

        [Test]
        [Description("Проверка на корректность Equals записи.")]
        public void CorrectRecordEquals()
        {
            var left = new Core.Models.DataRecord(1, new byte[100]);
            var right = new Core.Models.DataRecord(1, new byte[99]);

            Assert.AreNotEqual(left, right);
        }

        [Test]
        [Description("Проверка на корректность Equals заголовков записи.")]
        public void CorrectHeaderEquals()
        {
            var left = new Core.Models.DataRecordHeader
            {
                Id = 100500,
                Length = 125
            };

            using (var memoryStream = new MemoryStream(left.ToByteArray()))
            {
                var right = Core.Models.DataRecord.ReadHeader(memoryStream);

                Assert.AreEqual(left, right);
            }
        }

        #endregion Тесты
    }
}
