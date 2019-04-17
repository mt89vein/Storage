using NUnit.Framework;
using System;

namespace Storage.Tests.DataRecord
{
    [TestFixture]
    public class DataRecordTests
    {
        #region Тесты

        [Test]
        [Description("Корректное восстановление из пустого экземпляра.")]
        public void EmptyDataRecordReconstituteValid()
        {
            var left = Core.Models.DataRecord.Create(new byte[0]);
            var right = new Core.Models.DataRecord(left.GetBytes());

            Assert.Multiple(() =>
            {
                Assert.IsTrue(left == right, "Equality left.right");
                Assert.IsTrue(right == left, "Equality right.left");
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
        [Description("IO exception при пустом массиве байт.")]
        public void EmptyBytesThrowsIoException()
        {
            Assert.Throws<InvalidOperationException>(() => new Core.Models.DataRecord(new byte[0]));
        }

        [Test]
        [Description("Проверка на корректность Equals записи.")]
        public void CorrectRecordEquals()
        {
            var left = Core.Models.DataRecord.Create(new byte[100]);
            var right = Core.Models.DataRecord.Create(new byte[99]);

            Assert.AreNotEqual(left, right);
        }

        #endregion Тесты
    }
}
