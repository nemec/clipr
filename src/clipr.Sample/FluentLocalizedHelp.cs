using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using clipr.Usage;

namespace clipr.Sample
{
    class FluentLocalizedHelp
    {
        public class LocalizationOptions
        {
            public bool TurnOnThePower { get; set; }

            public IList<string> FilesToAdd { get; set; }

            public DateTime StartDate { get; set; }
            
            public double MyCounter { get; set; }
        }

        public static void Main()
        {
            var culture = Thread.CurrentThread.CurrentUICulture;

            var opt = new LocalizationOptions();
            var builder = new CliParserBuilder<LocalizationOptions>();
            builder.Localize(typeof(Properties.Resources));
            builder
                .AddNamedArgument(a => a.TurnOnThePower)
                .WithLongName("turnonthepower")
                .WithLocalizedDescription("Set power level to 9001")
                .StoresTrue();
            builder
                .AddNamedArgument(a => a.StartDate)
                .WithShortName('s')
                .WithLocalizedDescription("Start date");
            builder
                .AddPositionalArgumentList(a => a.FilesToAdd)
                .WithLocalizedDescription("Files to add to the thing")
                .ConsumesAtLeast(1);
            builder
                .AddNamedArgument(a => a.MyCounter)
                .WithShortName('c')
                .WithLocalizedDescription("A cool counter");
            var parser = builder.BuildParser();
            var help = new AutomaticHelpGenerator<LocalizationOptions>();

            parser.Parse("--turnonthepower -s 3/12/2016 -c 2.3 file1.txt".Split(), opt);
            Console.WriteLine(opt.StartDate.Month); // 3
            Console.WriteLine(opt.MyCounter);  // 2.3

            Console.WriteLine(help.GetHelp(parser.BuildConfig()));

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-MX");

            var optMx = new LocalizationOptions();
            var parserMx = builder.BuildParser();

            parserMx.Parse("--turnonthepower -s 12/3/2016 -c 2.3 file1.txt".Split(), optMx);
            Console.WriteLine(optMx.StartDate.Month);  // 3
            Console.WriteLine(optMx.MyCounter);  // 2.3

            Console.WriteLine(help.GetHelp(parserMx.BuildConfig()));

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-ES");

            var optEs = new LocalizationOptions();
            var parserEs = builder.BuildParser();

            parserEs.Parse("--turnonthepower -s 12/3/2016 -c 2,3 file1.txt".Split(), optEs);
            Console.WriteLine(optEs.StartDate.Month);  // 3
            Console.WriteLine(optEs.MyCounter);  // 2.3

            Console.WriteLine(help.GetHelp(parserEs.BuildConfig()));

            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}