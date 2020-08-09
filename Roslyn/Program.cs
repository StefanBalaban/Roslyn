using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Roslyn.BetterGenerators;
using Roslyn.Generators;


namespace Roslyn
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = new StreamReader("..\\..\\..\\Classes\\Person.cs").ReadToEnd();
            //Generator.Generate(code);

            //var syntaxGeneratorHelper = new SyntaxGeneratorSample();
            //var syntaxNode = syntaxGeneratorHelper.Generate();

            var syntaxGeneratorHelper = new SyntaxGeneratorHelper();
            var syntaxNode = syntaxGeneratorHelper.GenerateSyntaxNode(code);

            Console.WriteLine(syntaxNode);
            
        }
    }
}
