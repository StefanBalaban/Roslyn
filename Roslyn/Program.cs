using System;
using Microsoft.CodeAnalysis.CSharp;
using Roslyn.Generators;


namespace Roslyn
{
    class Program
    {
        static void Main(string[] args)
        {
            GenerateSampleViewModel();
        }
        static void GenerateSampleViewModel()
        {
            const string models = 
                @"namespace Models
                {
                    public class Item
                    {
                        public string ItemName { get; set; }
                    }
                }";
            var node = CSharpSyntaxTree.ParseText(models).GetRoot();
            var viewModel = ServicesGeneration.GenerateService(node);
            if (viewModel != null)
                Console.WriteLine(viewModel.ToFullString());
            Console.ReadLine();
        }
    }
}
