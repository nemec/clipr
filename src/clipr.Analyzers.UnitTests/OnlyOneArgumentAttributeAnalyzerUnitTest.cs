using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;
using System.Reflection;

namespace clipr.Analyzers.UnitTests
{
    [TestClass]
    public class OnlyOneArgumentAttributeAnalyzerUnitTest : DiagnosticVerifier
    {

        [TestMethod]
        public void Class_WithMultipleArgumentAttributes_TriggersDiagnostic()
        {
            var test = @"
using clipr;
class Options
{
    [NamedArgumentAttribute('n')]
    [PositionalArgumentAttribute(0)]
    public string Name { get; set; }
}";

            var expected = new DiagnosticResult
            {
                Id = "CLPR001",
                Locations = new DiagnosticResultLocation[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 19)
                },
                Message = "Multiple ArgumentAttributes applied to the property Name. Only one is allowed.",
                Severity = DiagnosticSeverity.Error
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new OnlyOneArgumentAttributeAnalyzer();
        }

        protected override MetadataReference[] GetReferences()
        {
            return new[]
            {
                MetadataReference.CreateFromFile(typeof(CliParser).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("system.runtime, Version=4.0.0.0").Location),
                MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location)
            };
        }
    }
}
