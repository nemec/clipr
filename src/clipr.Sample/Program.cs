using System;
using System.Collections.Generic;
using clipr.Usage;
using clipr.Utils;
using System.Globalization;
using System.Threading;

namespace clipr.Sample
{
    class Program
    {
        static void Main()
        {
            // FluentWithVerb.Main("-n3 add oranges.txt".Split());
            // FluentConditional.Main("http", "-u http://file".Split());
            // DictionaryBackedOptions.DictBackendMethodConfig("-n frank".Split());
            // CustomConverterDateTime.Main("-d 20140730 2013-09-10".Split());
            // RequiredNamedArgument.Main("-c -d 10/13/2010".Split());
            // PasswordMasking.DoPwMaskingAndPositional();
            // StaticEnums.ParseStaticEnumList();
            // OnParseArgumentEvent.ParseArgsWithEvent();
            ReusableCliParser.Main();
        }
    }
}