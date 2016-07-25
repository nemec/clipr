using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;
using clipr.Usage;
using clipr.Attributes;

namespace clipr.UnitTests
{
    [TestClass]
    public class LocalizationUnitTest
    {
        [Localize(ResourceType = typeof(Properties.Resources))]
        public class LocalizationOptions
        {
            [Localize]
            [NamedArgument("turnonthepower", Action = ParseAction.StoreTrue)]
            public bool TurnOnThePower { get; set; }

            [Localize("FileToAdd", typeof(Properties.Resources))]
            [PositionalArgument(0)]
            public string FileToAdd { get; set; }

            [Localize("StartDate")]
            [NamedArgument('s')]
            public DateTime StartDate { get; set; }

            [Localize("MyCounter")]
            [NamedArgument('c')]
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
 FileToAdd         File to add to the thing.

Optional Arguments:
 --turnonthepower  Set power level to 9001.
 -c                A cool counter.
 -h, --help        Display this help document.
 -s                Start date.
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
 FileToAdd         Archivo para añadir a la cosa.

Los Argumentos Opcionales:
 --turnonthepower  Establecer el nivel de potencia de 9001.
 -c                Un contador fresco.
 -h, --help        Mostrar este documento de ayuda.
 -s                Fecha de inicio.
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

        // TODO localize verb description

        // TODO localize applicationinfo description

        // TODO test use of Description property when localization does not exist (even in default resource)
    }
}
