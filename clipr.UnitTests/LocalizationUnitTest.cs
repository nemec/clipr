using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;
using clipr.Usage;
using clipr.Attributes;

namespace clipr.UnitTests
{
    internal static class LocalizationExtensions
    {
        private class CultureChanger : IDisposable
        {
            private readonly CultureInfo oldCulture;

            public CultureChanger(CultureInfo culture)
            {
#if NETCORE
                oldCulture = CultureInfo.CurrentUICulture;
                CultureInfo.CurrentUICulture = culture;
#else
                oldCulture = Thread.CurrentThread.CurrentUICulture;
                Thread.CurrentThread.CurrentUICulture = culture;
#endif
            }

            public void Dispose()
            {
#if NETCORE
                CultureInfo.CurrentUICulture = oldCulture;
#else
                Thread.CurrentThread.CurrentUICulture = oldCulture;
#endif
            }
        }

        public static IDisposable WithUiCulture(CultureInfo culture)
        {
            return new CultureChanger(culture);
        }
    }

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
            using (LocalizationExtensions.WithUiCulture(new CultureInfo("en-US")))
            {
                // Arrange
                var expected = new DateTime(2016, 3, 12);

                var opt = new LocalizationOptions();
                var parser = new CliParser<LocalizationOptions>();
                var help = new AutomaticHelpGenerator<LocalizationOptions>();

                // Act
                parser.Parse("-s 3/12/2016 file.txt".Split(), opt);

                // Assert
                Assert.AreEqual(expected, opt.StartDate);
            }
        }

        [TestMethod]
        public void ParseDate_WithMexicanLocaleAndMexicanDateFormat_ParsesDateCorrectly()
        {
            using (LocalizationExtensions.WithUiCulture(new CultureInfo("es-MX")))
            {
                // Arrange
                var expected = new DateTime(2016, 3, 12);

                var opt = new LocalizationOptions();
                var parser = new CliParser<LocalizationOptions>();
                var help = new AutomaticHelpGenerator<LocalizationOptions>();

                // Act
                parser.Parse("-s 12/3/2016 file.txt".Split(), opt);

                // Assert
                Assert.AreEqual(expected, opt.StartDate);
            }
        }

        [TestMethod]
        public void ParseDate_WithSpanishLocaleAndSpanishDateFormat_ParsesDateCorrectly()
        {
            using (LocalizationExtensions.WithUiCulture(new CultureInfo("es-ES")))
            {
                // Arrange
                var expected = new DateTime(2016, 3, 12);

                var opt = new LocalizationOptions();
                var parser = new CliParser<LocalizationOptions>();
                var help = new AutomaticHelpGenerator<LocalizationOptions>();

                // Act
                parser.Parse("-s 12/3/2016 file.txt".Split(), opt);

                // Assert
                Assert.AreEqual(expected, opt.StartDate);
            }
        }

        [TestMethod]
        public void ParseNumber_WithAmericanLocaleAndAmericanNumberFormat_ParsesNumberCorrectly()
        {
            using (LocalizationExtensions.WithUiCulture(new CultureInfo("en-US")))
            {
                // Arrange
                var expected = 2.3;

                var opt = new LocalizationOptions();
                var parser = new CliParser<LocalizationOptions>();
                var help = new AutomaticHelpGenerator<LocalizationOptions>();

                // Act
                parser.Parse("-c 2.3 file.txt".Split(), opt);

                // Assert
                Assert.AreEqual(expected, opt.MyCounter);
            }
        }

        [TestMethod]
        public void ParseNumber_WithMexicanLocaleAndMexicanNumberFormat_ParsesNumberCorrectly()
        {
            using (LocalizationExtensions.WithUiCulture(new CultureInfo("es-MX")))
            {
                // Arrange
                var expected = 2.3;

                var opt = new LocalizationOptions();
                var parser = new CliParser<LocalizationOptions>();
                var help = new AutomaticHelpGenerator<LocalizationOptions>();

                // Act
                parser.Parse("-c 2.3 file.txt".Split(), opt);

                // Assert
                Assert.AreEqual(expected, opt.MyCounter);
            }
        }

        [TestMethod]
        public void ParseNumber_WithSpanishLocaleAndSpanishNumberFormat_ParsesNumberCorrectly()
        {
            using (LocalizationExtensions.WithUiCulture(new CultureInfo("es-ES")))
            {
                // Arrange
                var expected = 2.3;

                var opt = new LocalizationOptions();
                var parser = new CliParser<LocalizationOptions>();
                var help = new AutomaticHelpGenerator<LocalizationOptions>();

                // Act
                parser.Parse("-c 2,3 file.txt".Split(), opt);

                // Assert
                Assert.AreEqual(expected, opt.MyCounter);
            }
        }

        [TestMethod]
        public void ShowHelp_WithAmericanLocale_ShowsEnglishHelpText()
        {
            using (LocalizationExtensions.WithUiCulture(new CultureInfo("en-US")))
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
                
                var parser = new CliParser<LocalizationOptions>();
                var help = new AutomaticHelpGenerator<LocalizationOptions>();  // TODO simplify work required to get help info

                // Act
                var actual = help.GetHelp(parser.BuildConfig());

                // Assert
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void ShowHelp_WithMexicanLocale_ShowsSpanishHelpText()
        {
            using (LocalizationExtensions.WithUiCulture(new CultureInfo("es-MX")))
            {
                // Arrange
                var expected = @"Forma de uso: clipr [ -h|--help ] [ --version ] [ --turnonthepower ] [ -s S ] [ -c C ] FILETOADD
Argumentos dependientes de la posición:
 FileToAdd         Archivo para añadir a la cosa.

Argumentos opcionales:
 --turnonthepower  Establecer el nivel de potencia de 9001.
 -c                Un contador fresco.
 -h, --help        Muestra esta ayuda
 -s                Fecha de inicio.
 --version         Muestra la versión del ejecutable";
                
                var parser = new CliParser<LocalizationOptions>();
                var help = new AutomaticHelpGenerator<LocalizationOptions>();

                // Act
                var actual = help.GetHelp(parser.BuildConfig());

                // Assert
                Assert.AreEqual(expected, actual);
            }
        }

        // TODO localize verb description

        // TODO localize applicationinfo description

        // TODO test use of Description property when localization does not exist (even in default resource)
    }
}
