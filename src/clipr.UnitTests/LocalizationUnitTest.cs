using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;
using clipr.Usage;

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
        [ApplicationInfo(Name = "clipr")]
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
                Assert.AreEqual(expected.Replace("\r\n", "\n"), actual.Replace("\r\n", "\n"));
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
                Assert.AreEqual(expected.Replace("\r\n", "\n"), actual.Replace("\r\n", "\n"));
            }
        }

        [Localize(ResourceType = typeof(Properties.Resources))]
        [ApplicationInfo(Name = "clipr")]
        public class LocalizationVerbOptions
        {
            [Localize]
            [Verb]
            public LocalizationVerb DoTheThing { get; set; }

            [Localize]
            [Verb]
            public LocalizationVerb DoAnotherThing { get; set; }
        }

        public class LocalizationVerb
        {
            [Localize("MyCounter")]
            [NamedArgument('c')]
            public double MyCounter { get; set; }
        }

        [TestMethod]
        public void ShowVerbHelp_WithAmericanLocale_ShowsEnglishHelpText()
        {
            using (LocalizationExtensions.WithUiCulture(new CultureInfo("en-US")))
            {
                // Arrange
                var expected = @"Usage: clipr [ -h|--help ] [ --version ] <command>
Optional Arguments:
 -h, --help  Display this help document.
 --version   Displays the version of the current executable.

Commands:
 doanotherthingTell me goodbye
 dothething  Tell me hello";

                var parser = new CliParser<LocalizationVerbOptions>();
                var help = new AutomaticHelpGenerator<LocalizationVerbOptions>();  // TODO simplify work required to get help info

                // Act
                var actual = help.GetHelp(parser.BuildConfig());

                // Assert
                Assert.AreEqual(expected.Replace("\r\n", "\n"), actual.Replace("\r\n", "\n"));
            }
        }

        [TestMethod]
        public void ShowVerbHelp_WithMexicanLocale_ShowsSpanishHelpText()
        {
            using (LocalizationExtensions.WithUiCulture(new CultureInfo("es-MX")))
            {
                // Arrange
                var expected = @"Forma de uso: clipr [ -h|--help ] [ --version ] <command>
Argumentos opcionales:
 -h, --help  Muestra esta ayuda
 --version   Muestra la versión del ejecutable

Commands:
 doanotherthingDime adios
 dothething  Dime hola";

                var parser = new CliParser<LocalizationVerbOptions>();
                var help = new AutomaticHelpGenerator<LocalizationVerbOptions>();

                // Act
                var actual = help.GetHelp(parser.BuildConfig());

                // Assert
                Assert.AreEqual(expected.Replace("\r\n", "\n"), actual.Replace("\r\n", "\n"));
            }
        }

        [Localize(ResourceType = typeof(Properties.Resources))]
        [ApplicationInfo(Name = "clipr")]
        public class LocalizationDefaultDescriptionOptions
        {
            [Localize(ResourceName = "DefaultEnglishOnlyDescription")]
            [NamedArgument('n', "name", Description = "Default description")]
            public string Name { get; set; }
        }

        [TestMethod]
        public void ShowHelp_WithAmericanLocaleAndNoOverrideDefaultDescription_ShowsEnglishHelpText()
        {
            using (LocalizationExtensions.WithUiCulture(new CultureInfo("en-US")))
            {
                // Arrange
                var expected = @"Usage: clipr [ -h|--help ] [ --version ] [ -n|--name N ]
Optional Arguments:
 -h, --help  Display this help document.
 -n, --name  English description.
 --version   Displays the version of the current executable.";

                var parser = new CliParser<LocalizationDefaultDescriptionOptions>();
                var help = new AutomaticHelpGenerator<LocalizationDefaultDescriptionOptions>();  // TODO simplify work required to get help info

                // Act
                var actual = help.GetHelp(parser.BuildConfig());

                // Assert
                Assert.AreEqual(expected.Replace("\r\n", "\n"), actual.Replace("\r\n", "\n"));
            }
        }

        [TestMethod]
        public void ShowHelp_WithMexicanLocaleAndMissingLocalizedDescription_ShowsDefaultHelpText()
        {
            using (LocalizationExtensions.WithUiCulture(new CultureInfo("es-MX")))
            {
                // Arrange
                var expected = @"Forma de uso: clipr [ -h|--help ] [ --version ] [ -n|--name N ]
Argumentos opcionales:
 -h, --help  Muestra esta ayuda
 -n, --name  Default description
 --version   Muestra la versión del ejecutable";

                var parser = new CliParser<LocalizationDefaultDescriptionOptions>();
                var help = new AutomaticHelpGenerator<LocalizationDefaultDescriptionOptions>();

                // Act
                var actual = help.GetHelp(parser.BuildConfig());

                // Assert
                Assert.AreEqual(expected.Replace("\r\n", "\n"), actual.Replace("\r\n", "\n"));
            }
        }

        // TODO localize applicationinfo description
    }
}
