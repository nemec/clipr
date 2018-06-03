using System;

namespace clipr.Sample
{
    class PasswordMasking
    {
        public class OptsWithPwMasking
        {
            [PromptIfValueMissing(MaskInput = true)]
            [NamedArgument('p', "password")]
            public string Password { get; set; }
        }

        public static void DoPwMasking()
        {
            var result = CliParser.Parse<OptsWithPwMasking>("-p".Split());
            result.Handle(
                opt => Console.WriteLine(opt.Password),
                t => { },
                e => { });
        }

        public class OptsWithPwMaskingAndPositional
        {
            [PromptIfValueMissing(MaskInput = true)]
            [NamedArgument('p', "password")]
            public string Password { get; set; }

            [PositionalArgument(0)]
            public string Name { get; set; }
        }

        public static void DoPwMaskingAndPositional()
        {
            var result = CliParser.Parse<OptsWithPwMaskingAndPositional>("-p - test".Split());
            result.Handle(
                opt => Console.WriteLine(opt.Password),
                t => { },
                e => { });
        }
    }
}