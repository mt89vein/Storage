using NUnit.Framework;
using Storage.Core.Models;
using System;

namespace Storage.Tests.DataRecordIndexStorage
{
    [TestFixture]
    public class DataRecordIndexPointerTests
    {
        #region Тесты

        [Test]
        [Description("Проверка на корректность Equals")]
        public void CorrectEquals()
        {
            var left = new DataRecordIndexPointer(1, 0, 0, 256);
            var right = new DataRecordIndexPointer(2, 0, 0, 256);

            Assert.AreNotEqual(left, right);
        }

        [Test]
        [Description("Корректное восстановление из пустого экзепляра.")]
        public void EmptyBytesReconstitute()
        {
            var pointer = new DataRecordIndexPointer();
            var bytes = pointer.GetBytes();
            var point = new DataRecordIndexPointer(bytes);

            Assert.Multiple(() =>
            {
                EqualityAssert(pointer, point);
            });
        }

        [Test]
        [Description("Корректное восстановление из созданного не пустого экземпляра одиночного указателя.")]
        public void SinglePageIndexReconstitute()
        {
            var pointer = new DataRecordIndexPointer(1, 1, 0, 256);
            var bytes = pointer.GetBytes();
            var point = new DataRecordIndexPointer(bytes);

            Assert.Multiple(() =>
            {
                EqualityAssert(pointer, point);
            });
        }

        [Test]
        [Description("Корректное восстановление из созданного не пустого экземпляра многостраничного указателя.")]
        public void MultiPageIndexReconstitute()
        {
            var pointer = new DataRecordIndexPointer(1, 1, 0, 256,
                    new DataRecordIndexPointer(1, 2, 0, 256),
                    new DataRecordIndexPointer(1, 3, 0, 256)
                );
            var bytes = pointer.GetBytes();
            var point = new DataRecordIndexPointer(bytes);

            Assert.Multiple(() =>
            {
                EqualityAssert(pointer, point);
            });
        }

        [Test]
        [Description("Корректное восстановление из массива байт разного размера.")]
        [TestCase(0)]
        [TestCase(19)]
        [TestCase(20)]
        [TestCase(21)]
        public void ThrowsErrorIfNot20SizeByteArray(int bytesCount)
        {
            if (DataRecordIndexPointer.Size != bytesCount)
            {
                Assert.Throws<InvalidOperationException>(() => new DataRecordIndexPointer(new byte[bytesCount]));
            }
            else
            {
                Assert.DoesNotThrow(() => new DataRecordIndexPointer(new byte[bytesCount]));
            }
        }

        [Test]
        [Description("Исключение, в случае если в одном индексе присутствуют разные идентификаторы записи.")]
        public void ThrowsErrorIfMultiPageIndexWithDifferentRecordIds()
        {
            Assert.Throws<ArgumentException>(() => new DataRecordIndexPointer(1, 1, 0, 256,
                    new DataRecordIndexPointer(1, 2, 0, 256),
                    new DataRecordIndexPointer(2, 3, 0, 256)
                )
            );
        }

        [Test]
        [Description("Исключение, в случае если в одном индексе пытаеются создать вложенные индексы.")]
        public void ThrowsErrorIfMultiPageIndexWithDeepIndexes()
        {
            Assert.Throws<ArgumentException>(() => new DataRecordIndexPointer(1, 1, 0, 256,
                    new DataRecordIndexPointer(1, 2, 0, 256),
                    new DataRecordIndexPointer(1, 3, 0, 256,
                        new DataRecordIndexPointer(1, 3, 0, 256)
                    )
                )
            );
        }

        #endregion Тесты

        #region Helper methods

        /// <summary>
        /// Ассерты на идентичность.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        private static void EqualityAssert(DataRecordIndexPointer left, DataRecordIndexPointer right)
        {
            Assert.AreEqual(left, right, "Equality");
            Assert.IsTrue(left.Equals(right), "left.Equals(right)");
            Assert.IsTrue(right.Equals(left), "right.Equals(left)");
            Assert.IsTrue(left.Equals((object)right), "left.Equals((object)right)");
            Assert.IsTrue(right.Equals((object)left), "right.Equals((object)left)");
            Assert.IsTrue(Equals(left, right), "Equals(left, right)");
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode(), "HashCodes");
            Assert.IsFalse(right.Equals(null), "right.Equals(null)");
        }

        #endregion Helper methods
    }
}