using clipr.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace clipr.UnitTests
{
    [TestClass]
    public class AllowedRangeUnitTest
    {
        #region byte comparisons

        [TestMethod]
        public void ValidateAllowedRange_WithByteBelowRange_ReturnsError()
        {
            var expected = "The value 1 is below the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember((byte)1, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithByteOnLowerBound_Validates()
        {
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember((byte)3, out Exception error);
            Assert.IsTrue(actual);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithByteInRange_Validates()
        {
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember((byte)7, out Exception error);
            Assert.IsTrue(actual);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithByteOnUpperBound_ReturnsError()
        {
            var expected = "The value 10 is above the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MaxValue = 10
            };

            var actual = converter.ValidateMember((byte)10, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithByteAboveRange_ReturnsError()
        {
            var expected = "The value 15 is above the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MaxValue = 10
            };

            var actual = converter.ValidateMember((byte)15, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        #endregion

        #region sbyte comparisons

        [TestMethod]
        public void ValidateAllowedRange_WithSByteBelowRange_ReturnsError()
        {
            var expected = "The value 1 is below the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember((sbyte)1, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithSByteOnLowerBound_Validates()
        {
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember((sbyte)3, out Exception error);
            Assert.IsTrue(actual);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithSByteInRange_Validates()
        {
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember((sbyte)7, out Exception error);
            Assert.IsTrue(actual);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithSByteOnUpperBound_ReturnsError()
        {
            var expected = "The value 10 is above the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MaxValue = 10
            };

            var actual = converter.ValidateMember((sbyte)10, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithSByteAboveRange_ReturnsError()
        {
            var expected = "The value 15 is above the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MaxValue = 10
            };

            var actual = converter.ValidateMember((sbyte)15, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        #endregion

        #region int comparisons

        [TestMethod]
        public void ValidateAllowedRange_WithIntegerBelowRange_ReturnsError()
        {
            var expected = "The value 1 is below the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember(1, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithIntegerOnLowerBound_Validates()
        {
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember(3, out Exception error);
            Assert.IsTrue(actual);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithIntegerInRange_Validates()
        {
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember(7, out Exception error);
            Assert.IsTrue(actual);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithIntegerOnUpperBound_ReturnsError()
        {
            var expected = "The value 10 is above the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MaxValue = 10
            };

            var actual = converter.ValidateMember(10, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithIntegerAboveRange_ReturnsError()
        {
            var expected = "The value 15 is above the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MaxValue = 10
            };

            var actual = converter.ValidateMember(15, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        #endregion

        #region uint comparisons

        [TestMethod]
        public void ValidateAllowedRange_WithUIntegerBelowRange_ReturnsError()
        {
            var expected = "The value 1 is below the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember(1U, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithUIntegerOnLowerBound_Validates()
        {
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember(3U, out Exception error);
            Assert.IsTrue(actual);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithUIntegerInRange_Validates()
        {
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember(7U, out Exception error);
            Assert.IsTrue(actual);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithUIntegerOnUpperBound_ReturnsError()
        {
            var expected = "The value 10 is above the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MaxValue = 10
            };

            var actual = converter.ValidateMember(10U, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithUIntegerAboveRange_ReturnsError()
        {
            var expected = "The value 15 is above the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MaxValue = 10
            };

            var actual = converter.ValidateMember(15U, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        #endregion

        #region short comparisons

        [TestMethod]
        public void ValidateAllowedRange_WithShortBelowRange_ReturnsError()
        {
            var expected = "The value 1 is below the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember((short)1, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithShortOnLowerBound_Validates()
        {
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember((short)3, out Exception error);
            Assert.IsTrue(actual);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithShortInRange_Validates()
        {
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember((short)7, out Exception error);
            Assert.IsTrue(actual);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithShortOnUpperBound_ReturnsError()
        {
            var expected = "The value 10 is above the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MaxValue = 10
            };

            var actual = converter.ValidateMember((short)10, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithShortAboveRange_ReturnsError()
        {
            var expected = "The value 15 is above the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MaxValue = 10
            };

            var actual = converter.ValidateMember((short)15, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        #endregion

        #region ushort comparisons

        [TestMethod]
        public void ValidateAllowedRange_WithUShortBelowRange_ReturnsError()
        {
            var expected = "The value 1 is below the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember((ushort)1, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithUShortOnLowerBound_Validates()
        {
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember((ushort)3, out Exception error);
            Assert.IsTrue(actual);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithUShortInRange_Validates()
        {
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember((ushort)7, out Exception error);
            Assert.IsTrue(actual);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithUShortOnUpperBound_ReturnsError()
        {
            var expected = "The value 10 is above the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MaxValue = 10
            };

            var actual = converter.ValidateMember((ushort)10, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithUShortAboveRange_ReturnsError()
        {
            var expected = "The value 15 is above the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MaxValue = 10
            };

            var actual = converter.ValidateMember((ushort)15, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        #endregion

        #region long comparisons

        [TestMethod]
        public void ValidateAllowedRange_WithLongBelowRange_ReturnsError()
        {
            var expected = "The value 1 is below the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember((long)1, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithLongOnLowerBound_Validates()
        {
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember((long)3, out Exception error);
            Assert.IsTrue(actual);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithLongInRange_Validates()
        {
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember((long)7, out Exception error);
            Assert.IsTrue(actual);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithLongOnUpperBound_ReturnsError()
        {
            var expected = "The value 10 is above the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MaxValue = 10
            };

            var actual = converter.ValidateMember((long)10, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithLongAboveRange_ReturnsError()
        {
            var expected = "The value 15 is above the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MaxValue = 10
            };

            var actual = converter.ValidateMember((long)15, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        #endregion

        #region ulong comparisons

        [TestMethod]
        public void ValidateAllowedRange_WithULongBelowRange_ReturnsError()
        {
            var expected = "The value 1 is below the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember((ulong)1, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithULongOnLowerBound_Validates()
        {
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember((ulong)3, out Exception error);
            Assert.IsTrue(actual);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithULongInRange_Validates()
        {
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember((ulong)7, out Exception error);
            Assert.IsTrue(actual);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithULongOnUpperBound_ReturnsError()
        {
            var expected = "The value 10 is above the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MaxValue = 10
            };

            var actual = converter.ValidateMember((ulong)10, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithULongAboveRange_ReturnsError()
        {
            var expected = "The value 15 is above the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MaxValue = 10
            };

            var actual = converter.ValidateMember((ulong)15, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        #endregion

        #region float comparisons

        [TestMethod]
        public void ValidateAllowedRange_WithFloatBelowRange_ReturnsError()
        {
            var expected = "The value 1 is below the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember(1f, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithFloatOnLowerBound_Validates()
        {
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember(3f, out Exception error);
            Assert.IsTrue(actual);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithFloatInRange_Validates()
        {
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember(7f, out Exception error);
            Assert.IsTrue(actual);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithFloatOnUpperBound_ReturnsError()
        {
            var expected = "The value 10 is above the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MaxValue = 10
            };

            var actual = converter.ValidateMember(10f, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithFloatAboveRange_ReturnsError()
        {
            var expected = "The value 15 is above the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MaxValue = 10
            };

            var actual = converter.ValidateMember(15f, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        #endregion

        #region double comparisons

        [TestMethod]
        public void ValidateAllowedRange_WithDoubleBelowRange_ReturnsError()
        {
            var expected = "The value 1 is below the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember(1d, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithDoubleOnLowerBound_Validates()
        {
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember(3d, out Exception error);
            Assert.IsTrue(actual);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithDoubleInRange_Validates()
        {
            var converter = new AllowedRangeAttribute()
            {
                MinValue = 3,
                MaxValue = 10
            };

            var actual = converter.ValidateMember(7d, out Exception error);
            Assert.IsTrue(actual);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithDoubleOnUpperBound_ReturnsError()
        {
            var expected = "The value 10 is above the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MaxValue = 10
            };

            var actual = converter.ValidateMember(10d, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void ValidateAllowedRange_WithDoubleAboveRange_ReturnsError()
        {
            var expected = "The value 15 is above the allowed range.";
            var converter = new AllowedRangeAttribute()
            {
                MaxValue = 10
            };

            var actual = converter.ValidateMember(15d, out Exception error);
            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        #endregion

        public class RangeValidationOptions
        {
            [AllowedRange(MinValue = 3, MaxValue = 10)]
            [NamedArgument("myint")]
            public int MyInt { get; set; }
        }

        [TestMethod]
        public void Parse_WithValueBelowAllowedRange_FailsValidation()
        {
            var expected = "The value 1 is below the allowed range.";

            var obj = new RangeValidationOptions();
            var parser = new CliParser<RangeValidationOptions>();
            var result = parser.Parse("--myint 1".Split(), obj);

            result.Handle(
                opt => Assert.Fail("Parsing validation should throw an error."),
                trigger => Assert.Fail("No triggers should be thrown"),
                errs => Assert.AreEqual(
                    expected,
                    (errs.First() as ValidationFailure).ErrorMessage)
            );
        }
    }
}
