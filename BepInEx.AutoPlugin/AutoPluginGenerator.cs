﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BepInEx.AutoPlugin
{
    [Generator]
    public class AutoPluginGenerator : ISourceGenerator
    {
        private const string AttributeCode = @"// <auto-generated />
namespace BepInEx
{
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    [System.Diagnostics.Conditional(""CodeGeneration"")]
    internal sealed class BepInAutoPluginAttribute : System.Attribute
    {
        public BepInAutoPluginAttribute(string id = null, string name = null, string version = null) {}
    }
}
";

        private class SyntaxReceiver : ISyntaxContextReceiver
        {
            public List<ClassDeclarationSyntax> CandidateTypes { get; } = new List<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is ClassDeclarationSyntax classDeclarationSyntax && classDeclarationSyntax.AttributeLists.Any())
                {
                    CandidateTypes.Add(classDeclarationSyntax);
                }
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(i => i.AddSource("BepInAutoPluginAttribute", AttributeCode));
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        private static string? GetAssemblyAttribute(GeneratorExecutionContext context, string name)
        {
            var attribute = context.Compilation.Assembly.GetAttributes().SingleOrDefault(x => x.AttributeClass?.Name == name);
            return (string?)attribute?.ConstructorArguments.Single().Value;
        }

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
                    return;

                var attributeType = context.Compilation.GetTypeByMetadataName("BepInEx.BepInAutoPluginAttribute") ?? throw new NullReferenceException("Couldn't find BepInEx.BepInAutoPluginAttribute");

                foreach (var classDeclarationSyntax in receiver.CandidateTypes)
                {
                    var model = context.Compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
                    var typeSymbol = (INamedTypeSymbol)model.GetDeclaredSymbol(classDeclarationSyntax)!;

                    var attribute = typeSymbol.GetAttributes().SingleOrDefault(a => a.AttributeClass != null && SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeType));

                    if (attribute == null)
                    {
                        continue;
                    }

                    var arguments = attribute.ConstructorArguments.Select(x => x.IsNull ? null : (string)x.Value!).ToArray();
                    var id = arguments[0] ?? context.Compilation.AssemblyName;
                    var name = arguments[1] ?? GetAssemblyAttribute(context, nameof(AssemblyTitleAttribute)) ?? context.Compilation.AssemblyName;
                    var version = arguments[2] ?? GetAssemblyAttribute(context, nameof(AssemblyInformationalVersionAttribute)) ?? GetAssemblyAttribute(context, nameof(AssemblyVersionAttribute));

                    var source = SourceText.From($@"// <auto-generated />
namespace {typeSymbol.ContainingNamespace.ToDisplayString()}
{{
    [BepInEx.BepInPlugin({typeSymbol.Name}.Id, ""{name}"", ""{version}"")]
    public partial class {typeSymbol.Name}
    {{
        public const string Id = ""{id}"";
        public static string Name => ""{name}"";
        public static string Version => ""{version}"";
    }}
}}
", Encoding.UTF8);

                    context.AddSource($"{typeSymbol.Name}_AutoPlugin.cs", source);
                }
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "ERROR",
                        $"An exception was thrown by the {nameof(AutoPluginGenerator)}",
                        $"An exception was thrown by the {nameof(AutoPluginGenerator)} generator: {e.ToString().Replace("\n", ",")}",
                        nameof(AutoPluginGenerator),
                        DiagnosticSeverity.Error,
                        true),
                    Location.None));
            }
        }
    }
}
