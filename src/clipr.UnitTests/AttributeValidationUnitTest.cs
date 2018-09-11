using clipr.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace clipr.UnitTests
{
    [TestClass]
    public class AttributeValidationUnitTest
    {
        [TestMethod]
        public void Validate_WithDirectoryInfoObjectAndDirectoryExists_Validates()
        {
            var input = new DirectoryInfo("testdirectory");

            var validator = new DirectoryExistsAttribute();
            var actual = validator.ValidateMember(input, out Exception error);

            Assert.IsNull(error);
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void Validate_WithDirectoryStringAndDirectoryExists_Validates()
        {
            var input = "testdirectory";

            var validator = new DirectoryExistsAttribute();
            var actual = validator.ValidateMember(input, out Exception error);

            Assert.IsNull(error);
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void Validate_WithDirectoryInfoObjectAndDirectoryDoesNotExist_ReturnsError()
        {
            var expected = "Directory 'testdirectory\\invalid' does not exist.";
            var input = new DirectoryInfo("testdirectory\\invalid");

            var validator = new DirectoryExistsAttribute();
            var actual = validator.ValidateMember(input, out Exception error);

            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void Validate_WithDirectoryStringAndDirectoryDoesNotExist_ReturnsError()
        {
            var expected = "Directory 'testdirectoryinvalid' does not exist.";
            var input = "testdirectoryinvalid";

            var validator = new DirectoryExistsAttribute();
            var actual = validator.ValidateMember(input, out Exception error);

            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void Validate_WithFileInfoObjectAndFileExists_Validates()
        {
            var input = new FileInfo("testdirectory\\testfile.txt");

            var validator = new FileExistsAttribute();
            var actual = validator.ValidateMember(input, out Exception error);

            Assert.IsNull(error);
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void Validate_WithFileStringAndFileExists_Validates()
        {
            var input = "testdirectory\\testfile.txt";

            var validator = new FileExistsAttribute();
            var actual = validator.ValidateMember(input, out Exception error);

            Assert.IsNull(error);
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void Validate_WithFileInfoObjectAndFileDoesNotExist_ReturnsError()
        {
            var expected = "File 'testdirectory\\invalid\\file.txt' does not exist.";
            var input = new FileInfo("testdirectory\\invalid\\file.txt");

            var validator = new FileExistsAttribute();
            var actual = validator.ValidateMember(input, out Exception error);

            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        [TestMethod]
        public void Validate_WithFileStringAndFileDoesNotExist_ReturnsError()
        {
            var expected = "File 'testdirectory\\testfileinvalid.txt' does not exist.";
            var input = "testdirectory\\testfileinvalid.txt";

            var validator = new FileExistsAttribute();
            var actual = validator.ValidateMember(input, out Exception error);

            Assert.IsFalse(actual);
            Assert.AreEqual(expected, error.Message);
        }

        public class Options
        {
            [DirectoryExists]
            [NamedArgument("ds")]
            public string DirectoryString { get; set; }

            [FileExists]
            [NamedArgument("fs")]
            public string FileString { get; set; }
        }

        [TestMethod]
        public void Parse_WithDirectoryStringThatExists_ParsesValue()
        {
            var expected = "testdirectory";
            var obj = new Options();
            var parser = new CliParser<Options>();

            var result = parser.Parse("--ds testdirectory".Split(), obj);
            result.Handle(
                opt => Assert.AreEqual(expected, opt.DirectoryString),
                trigger => Assert.Fail("No triggers should be thrown"),
                errs => Assert.Fail("Parsing should not throw an error:" + errs[0].Message)
            );
        }

        [TestMethod]
        public void Parse_WithDirectoryStringThatDoesNotExist_ReturnsError()
        {
            var expected = "Directory 'testdirectoryinvalid' does not exist.";
            var obj = new Options();
            var parser = new CliParser<Options>();

            var result = parser.Parse("--ds testdirectoryinvalid".Split(), obj);
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
