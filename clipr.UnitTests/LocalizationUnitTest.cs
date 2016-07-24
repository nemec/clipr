using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;
using clipr.Usage;

namespace clipr.UnitTests
{
    [TestClass]
    public class LocalizationUnitTest
    {

        public class LocalizationOptions
        {
            [NamedArgument("turnonthepower", Description = "Set power level to 9001", Action = ParseAction.StoreTrue)]
            public bool TurnOnThePower { get; set; }

            [PositionalArgument(0, Description = "File to add to the thing")]
            public string FileToAdd { get; set; }

            [NamedArgument('s', Description = "Start date")]
            public DateTime StartDate { get; set; }

            [NamedArgument('c', Description = "A cool counter")]
            public double MyCounter { get; set; }
        }

        [TestMethod]
        public void ParseDate_WithAmericanLocaleAndAmericanDateFormat_ParsesDateCorrectly()
        {
            // Arrange
            var expected = new DateTime(2016, 3, 12);
            var oldCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

            var opt = new LocalizationOptions();
            var parser = new CliParser<LocalizationOptions>(opt);
            var help = new AutomaticHelpGenerator<LocalizationOptions>();

            // Act
            parser.Parse("-s 3/12/2016 file.txt".Split());

            // Assert
            Assert.AreEqual(expected, opt.StartDate);

            Thread.CurrentThread.CurrentUICulture = oldCulture;
        }

        [TestMethod]
        public void ParseDate_WithMexicanLocaleAndMexicanDateFormat_ParsesDateCorrectly()
        {
            // Arrange
            var expected = new DateTime(2016, 3, 12);
            var oldCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-MX");

            var opt = new LocalizationOptions();
            var parser = new CliParser<LocalizationOptions>(opt);
            var help = new AutomaticHelpGenerator<LocalizationOptions>();

            // Act
            parser.Parse("-s 12/3/2016 file.txt".Split());

            // Assert
            Assert.AreEqual(expected, opt.StartDate);

            Thread.CurrentThread.CurrentUICulture = oldCulture;
        }

        [TestMethod]
        public void ParseDate_WithSpanishLocaleAndSpanishDateFormat_ParsesDateCorrectly()
        {
            // Arrange
            var expected = new DateTime(2016, 3, 12);
            var oldCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-ES");

            var opt = new LocalizationOptions();
            var parser = new CliParser<LocalizationOptions>(opt);
            var help = new AutomaticHelpGenerator<LocalizationOptions>();

            // Act
            parser.Parse("-s 12/3/2016 file.txt".Split());

            // Assert
            Assert.AreEqual(expected, opt.StartDate);

            Thread.CurrentThread.CurrentUICulture = oldCulture;
        }

        [TestMethod]
        public void ParseNumber_WithAmericanLocaleAndAmericanNumberFormat_ParsesNumberCorrectly()
        {
            // Arrange
            var expected = 2.3;
            var oldCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

            var opt = new LocalizationOptions();
            var parser = new CliParser<LocalizationOptions>(opt);
            var help = new AutomaticHelpGenerator<LocalizationOptions>();

            // Act
            parser.Parse("-c 2.3 file.txt".Split());

            // Assert
            Assert.AreEqual(expected, opt.MyCounter);

            Thread.CurrentThread.CurrentUICulture = oldCulture;
        }

        [TestMethod]
        public void ParseNumber_WithMexicanLocaleAndMexicanNumberFormat_ParsesNumberCorrectly()
        {
            // Arrange
            var expected = 2.3;
            var oldCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-MX");

            var opt = new LocalizationOptions();
            var parser = new CliParser<LocalizationOptions>(opt);
            var help = new AutomaticHelpGenerator<LocalizationOptions>();

            // Act
            parser.Parse("-c 2.3 file.txt".Split());

            // Assert
            Assert.AreEqual(expected, opt.MyCounter);

            Thread.CurrentThread.CurrentUICulture = oldCulture;
        }

        [TestMethod]
        public void ParseNumber_WithSpanishLocaleAndSpanishNumberFormat_ParsesNumberCorrectly()
        {
            // Arrange
            var expected = 2.3;
            var oldCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-ES");

            var opt = new LocalizationOptions();
            var parser = new CliParser<LocalizationOptions>(opt);
            var help = new AutomaticHelpGenerator<LocalizationOptions>();

            // Act
            parser.Parse("-c 2,3 file.txt".Split());

            // Assert
            Assert.AreEqual(expected, opt.MyCounter);

            Thread.CurrentThread.CurrentUICulture = oldCulture;
        }

        [TestMethod]
        public void ShowHelp_WithAmericanLocale_ShowsEnglishHelpText()
        {
            // Arrange
            var expected = @"Usage: clipr [ -h|--help ] [ --version ] [ --turnonthepower ] [ -s S ] [ -c C ] FILETOADD 
Positional Arguments:
 FileToAdd         File to add to the thing

Optional Arguments:
 --turnonthepower  Set power level to 9001
 -c                A cool counter
 -h, --help        Display this help document.
 -s                Start date
 --version         Displays the version of the current executable.";
            var oldCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

            var opt = new LocalizationOptions();
            var parser = new CliParser<LocalizationOptions>(opt);
            var help = new AutomaticHelpGenerator<LocalizationOptions>();  // TODO simplify work required to get help info

            // Act
            var actual = help.GetHelp(parser.Config);

            // Assert
            Assert.AreEqual(expected, actual);

            Thread.CurrentThread.CurrentUICulture = oldCulture;
        }

        [TestMethod]
        public void ShowHelp_WithMexicanLocale_ShowsSpanishHelpText()
        {
            // Arrange
            var expected = @"Uso: clipr [ -h|--help ] [ --version ] [ --turnonthepower ] [ -s S ] [ -c C ] FILETOADD 
Los Argumentos Posicionales:
 FileToAdd         File to add to the thing

Los Argumentos Opcionales:
 --turnonthepower  Set power level to 9001
 -c                A cool counter
 -h, --help        Mostrar este documento de ayuda.
 -s                Start date
 --version         Muestra la versión del ejecutable actual.";
            var oldCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-MX");

            var opt = new LocalizationOptions();
            var parser = new CliParser<LocalizationOptions>(opt);
            var help = new AutomaticHelpGenerator<LocalizationOptions>();

            // Act
            var actual = help.GetHelp(parser.Config);

            // Assert
            Assert.AreEqual(expected, actual);

            Thread.CurrentThread.CurrentUICulture = oldCulture;
        }
    }
}
