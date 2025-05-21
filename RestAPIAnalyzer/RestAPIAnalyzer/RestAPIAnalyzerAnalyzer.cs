using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RestAPIAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RestAPIAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            "R001",
            "Route parameter mismatch",
            "Parameter '{0}' in route is not present in method signature",
            "Routing",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
        }

        private void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            // Znajdujemy atrybuty typu Route
            var attributeSyntax = (AttributeSyntax)context.Node;
            var name = attributeSyntax.Name.ToString();

            // Sprawdzamy, czy to nasz atrybut
            if (!name.Contains("Route"))
                return;

            // Pobieramy argument (ścieżkę)
            var arg = attributeSyntax.ArgumentList?.Arguments.FirstOrDefault();
            if (arg == null) return;

            // Sprawdzamy, czy argument jest wartością stałą
            var routeString = context.SemanticModel.GetConstantValue(arg.Expression);
            if (!routeString.HasValue) return;

            var route = routeString.Value?.ToString();
            if (route == null) return;

            // Szukamy parametrów w ścieżce
            var matches = Regex.Matches(route, @"\{(\w+)\}");
            if (matches.Count == 0) return;

            // Szukamy metody, do której przypisany jest atrybut
            var method = attributeSyntax.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (method == null) return;

            // Pobieramy parametry metody
            var parameters = method.ParameterList.Parameters.Select(p => p.Identifier.Text).ToList();

            // Dla każdego parametru w ścieżce, sprawdzamy, czy istnieje w metodzie
            foreach (Match match in matches)
            {
                var paramName = match.Groups[1].Value;
                if (!parameters.Contains(paramName))
                {
                    var diagnostic = Diagnostic.Create(Rule, arg.GetLocation(), paramName);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
        //public const string DiagnosticId = "RestAPIAnalyzer";

        //// You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        //// See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        //private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        //private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        //private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        //private const string Category = "Naming";

        //private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        //public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        //public override void Initialize(AnalysisContext context)
        //{
        //    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        //    context.EnableConcurrentExecution();

        //    // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
        //    // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
        //    context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        //}

        //private static void AnalyzeSymbol(SymbolAnalysisContext context)
        //{
        //    // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
        //    var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        //    // Find just those named type symbols with names containing lowercase letters.
        //    if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
        //    {
        //        // For all such symbols, produce a diagnostic.
        //        var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

        //        context.ReportDiagnostic(diagnostic);
        //    }
        //}
    }
}
