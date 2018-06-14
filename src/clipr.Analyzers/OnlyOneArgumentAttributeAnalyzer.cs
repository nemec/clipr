using System.Collections.Immutable;
using clipr.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace clipr.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OnlyOneArgumentAttributeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "CLPR001";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.OnlyOneArgumentTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.OnlyOneArgumentMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.OnlyOneArgumentDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "clipr.Integrity";

        private static readonly string AttrFullName = typeof(ArgumentAttribute).FullName;


        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId, 
            Title, 
            MessageFormat, 
            Category, 
            DiagnosticSeverity.Error, 
            isEnabledByDefault: true, 
            description: Description,
            customTags: WellKnownDiagnosticTags.Build);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Property);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var prop = (IPropertySymbol)context.Symbol;
            var attrs = prop.GetAttributes();

            var bas = context.Compilation.GetTypeByMetadataName(AttrFullName);
            var argAttrCount = 0;

            foreach(var attr in attrs)
            {
                if (attr.AttributeClass.BaseType != null &&
                    attr.AttributeClass.BaseType == bas)
                {
                    argAttrCount++;
                }
            }

            if(argAttrCount > 1)
            {
                var diag = Diagnostic.Create(Rule, prop.Locations[0], prop.Name);
                context.ReportDiagnostic(diag);
            }
        }
    }
}
