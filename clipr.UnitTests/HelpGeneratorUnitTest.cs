using clipr.Usage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace clipr.UnitTests
{
    [TestClass]
    public class HelpGeneratorUnitTest
    {
        [StaticEnumeration]
        internal class MyEnum
        {
            [EnumerationDescription("Some enum one")]
            public static readonly MyEnum First = new MyEnum();

            [EnumerationDescription("Some enum two")]
            public static readonly MyEnum Second = new MyEnum();
        }

        internal class StaticEnumOptions
        {
            public MyEnum Value { get; set; }
        }

        [TestMethod]
        public void GenerateUsage_WithStaticEnum_ListsEnumValues()
        {
            var generator = new AutomaticHelpGenerator<StaticEnumOptions>();
            // TODO
        }
    }
}
