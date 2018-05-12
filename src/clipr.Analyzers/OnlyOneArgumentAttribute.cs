using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Collections.Generic;
using clipr.Core;

namespace clipr.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OnlyOneArgumentAttribute : DiagnosticAnalyzer
    {

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            "CLPR001",
            "Cannot have multiple ArgumentAttribute subclasses on one property",
            "Multiple ArgumentAttributes applied to the property {0}. Only one is allowed.",
            "clipr.Integrity",
            DiagnosticSeverity.Error,
            true,
            customTags: WellKnownDiagnosticTags.Build
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            //context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            var issues = new HashSet<IPropertySymbol>();
            
            context.RegisterCompilationStartAction(compilationContext => {
                compilationContext.RegisterSymbolAction(
                    (symbolContext) =>
                    {
                        var prop = (IPropertySymbol)symbolContext.Symbol;
                        var attrs = prop.GetAttributes();
                        var argAttrCount = 0;

                        foreach(var attr in attrs)
                        {
                            if((Type)attr.AttributeClass.BaseType == typeof(ArgumentAttribute))
                            {
                                argAttrCount++;
                            }
                        }

                        if(argAttrCount > 1)
                        {
                            issues.Add(prop);
                        }

                    }, SymbolKind.Property);

                compilationContext.RegisterCompilationEndAction(
                    (compilationEndContext) =>
                    {
                        foreach(var prop in issues)
                        {
                            compilationEndContext.ReportDiagnostic(Diagnostic.Create(Rule,
                                 prop.Locations[0], prop.Name));
                        }
                    });
            });
        }


    }
}
