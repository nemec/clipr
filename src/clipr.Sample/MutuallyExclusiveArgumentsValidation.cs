using clipr.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace clipr.Sample
{
    class MutuallyExclusiveArgumentsValidation
    {
        public class MutuallyExclusiveOptions
        {
            [NamedArgument('d')]
            public bool Debug { get; set; }

            [NamedArgument("file")]
            public string File { get; set; }

            [NamedArgument("url")]
            public string Url { get; set; }
        }

        public class MutuallyExclusiveValidator : IParseValidator<MutuallyExclusiveOptions>
        {
            public ValidationResult Validate(MutuallyExclusiveOptions component)
            {
                if(component.File != null && component.Url != null)
                {
                    return new ValidationResult(
                        new ValidationFailure(nameof(component.Url),
                            "Cannot use both --file and --url arguments at once."));
                }
                else if(component.File == null && component.Url == null)
                {
                    return new ValidationResult(
                        new ValidationFailure(nameof(component.Url),
                            "Either --file or --url arguments must be specified."));
                }
                return new ValidationResult();
            }
        }

        public static void Main()
        {
            var args = new[] { "--file", "c:\\users\\me\\file.txt", "--url", "http://example.com" };
            var parser = new CliParser<MutuallyExclusiveOptions>();
            parser.Validator = new MutuallyExclusiveValidator();

            var opt = new MutuallyExclusiveOptions();
            parser.Parse(args, opt)
                .Handle(
                    o => throw new NotImplementedException(),
                    t => throw new NotImplementedException(),
                    errors =>
                    {
                        foreach (var error in errors)
                        {
                            Console.WriteLine(error);
                        }
                    });
        }
    }
}
