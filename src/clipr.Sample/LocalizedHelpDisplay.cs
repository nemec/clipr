using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using clipr.Usage;

namespace clipr.Sample
{
    class LocalizedHelpDisplay
    {
        public class LocalizationOptions
        {
            [NamedArgument("turnonthepower", Description = "Set power level to 9001", Action = ParseAction.StoreTrue)]
            public bool TurnOnThePower { get; set; }

            [PositionalArgument(0, Description = "Files to add to the thing", Constraint = NumArgsConstraint.AtLeast, NumArgs = 1)]
            public IList<string> FilesToAdd { get; set; }

            [NamedArgument('s', Description = "Start date")]
            public DateTime StartDate { get; set; }

            [NamedArgument('c', Description = "A cool counter")]
            public double MyCounter { get; set; }
        }

        static void PrintLocalizedHelp()
        {
            var culture = Thread.CurrentThread.CurrentUICulture;

            var opt = new LocalizationOptions();
            var parser = new CliParser<LocalizationOptions>();
            var help = new AutomaticHelpGenerator<LocalizationOptions>();

            parser.Parse("--turnonthepower -s 3/12/2016 -c 2.3 file1.txt".Split(), opt);
            Console.WriteLine(opt.StartDate.Month); // 3
            Console.WriteLine(opt.MyCounter);  // 2.3

            Console.WriteLine(help.GetHelp(parser.BuildConfig()));

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-MX");

            var optMx = new LocalizationOptions();
            var parserMx = new CliParser<LocalizationOptions>();

            parserMx.Parse("--turnonthepower -s 12/3/2016 -c 2.3 file1.txt".Split(), optMx);
            Console.WriteLine(optMx.StartDate.Month);  // 3
            Console.WriteLine(optMx.MyCounter);  // 2.3

            Console.WriteLine(help.GetHelp(parserMx.BuildConfig()));

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-ES");

            var optEs = new LocalizationOptions();
            var parserEs = new CliParser<LocalizationOptions>();

            parserEs.Parse("--turnonthepower -s 12/3/2016 -c 2,3 file1.txt".Split(), optEs);
            Console.WriteLine(optEs.StartDate.Month);  // 3
            Console.WriteLine(optEs.MyCounter);  // 2.3

            Console.WriteLine(help.GetHelp(parserEs.BuildConfig()));

            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}