using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Roslyn.BetterGenerators
{
    public class SyntaxGeneratorHelper
    {
        private ServicesSyntaxGenerator servicesSyntaxGenerator = new ServicesSyntaxGenerator();


        public string GenerateSyntaxNode(string model)
        {
            var node = CSharpSyntaxTree.ParseText(model).GetRoot();
            var classNode = node.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            var serviceClassNode = servicesSyntaxGenerator.GenerateServiceClassNode(classNode);

            

            return serviceClassNode.ToFullString();
        }

        
    }
}
