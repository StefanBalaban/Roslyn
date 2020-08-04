using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Helpers;

namespace Roslyn.Generators
{
    public static class Generator
    {
        
        public static void Generate(string models)
        {
            
            var node = CSharpSyntaxTree.ParseText(models).GetRoot();
            var codeIssues = node.GetDiagnostics();
            if (!codeIssues.Any())
            {
                var classNode = node.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();

                var serviceImplementation = ServiceTemplate.GenerateTemplate(classNode);
                var apiImplementation = ApiTemplate.GenerateTemplate(classNode);
                if (serviceImplementation != null)
                    Console.WriteLine(ParseClass(serviceImplementation));
                if (apiImplementation != null)
                    Console.WriteLine(apiImplementation);
                Console.ReadLine();
            }
            else
            {
                DiagnosticsHelper.ShowDiagnostics(codeIssues);
                Console.ReadLine();
            }


        }

        private static void GenerateCrudApiClasses(ClassDeclarationSyntax classNode)
        {
            
        }
        private static SyntaxNode ParseClass(string implementation)
        {
            var classImplementation = SyntaxFactory.ParseSyntaxTree(implementation).GetRoot();
            var codeIssues = classImplementation.GetDiagnostics();
            if (!codeIssues.Any())
                return classImplementation;
            DiagnosticsHelper.ShowDiagnostics(codeIssues);
            return null;
        }
    }
}
