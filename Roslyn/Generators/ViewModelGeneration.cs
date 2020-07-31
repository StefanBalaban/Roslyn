using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
namespace Roslyn
{
    public static class ViewModelGeneration
    {
        public static SyntaxNode GenerateViewModel(SyntaxNode node)
        {
            // Find the first class in the syntax node
            var classNode =
              node.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classNode != null)
            {
                var codeIssues = node.GetDiagnostics();
                if (!codeIssues.Any())
                {
                    // Get the name of the model class
                    var modelClassName = classNode.Identifier.Text;
                    // The name of the ViewModel class
                    var viewModelClassName = $"{modelClassName}ViewModel";
                    // Only for demo purposes, pluralizing an object is done by
                    // simply adding the "s" letter. Consider proper algorithms
                    string newImplementation =
                      $@"public class {viewModelClassName} : INotifyPropertyChanged
                            {{
                                public event PropertyChangedEventHandler PropertyChanged;
                                // Raise a property change notification
                                protected virtual void OnPropertyChanged(string propname)
                                {{
                                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
                                }}
                                private ObservableCollection<{modelClassName}> _{modelClassName}s;
                                public ObservableCollection<{modelClassName}> {modelClassName}s
                                {{
                                    get {{ return _{modelClassName}s; }}
                                    set
                                    {{
                                        _{modelClassName}s = value;
                                        OnPropertyChanged(nameof({modelClassName}s));
                                    }}
                                }}
                                public {viewModelClassName}() 
                                    {{
                                        // Implement your logic to load a collection of items
                                    }}
                            }}
                            ";
                    var newClassNode =
                      SyntaxFactory.ParseSyntaxTree(newImplementation).GetRoot()
                      .DescendantNodes().OfType<ClassDeclarationSyntax>()
                      .FirstOrDefault();
                    // Retrieve the parent namespace declaration
                    if (!(classNode.Parent is NamespaceDeclarationSyntax)) return null;
                    var parentNamespace = (NamespaceDeclarationSyntax)classNode.Parent;
                    // Add the new class to the namespace
                    var newParentNamespace =
                      parentNamespace.AddMembers(newClassNode).NormalizeWhitespace();
                    return newParentNamespace;
                }
                foreach (Diagnostic codeIssue in codeIssues)
                {
                    string issue = $"ID: {codeIssue.Id}, Message: {codeIssue.GetMessage()}, Location: { codeIssue.Location.GetLineSpan()}, Severity: { codeIssue.Severity}";
                    Console.WriteLine(issue);
                }
                return null;
            }
            return null;
        }
    }
}