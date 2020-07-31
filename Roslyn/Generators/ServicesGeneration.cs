using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Extensions;

namespace Roslyn.Generators
{
    public class ServicesGeneration
    {
        public static SyntaxNode GenerateService(SyntaxNode node)
        {
            // Find the first class in the syntax node
            var classNode =
              node.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classNode != null)
            {
                var codeIssues = node.GetDiagnostics();
                if (!codeIssues.Any())
                {
                    var modelClassName = classNode.Identifier.Text;
                    var serviceClassName = $"{modelClassName}Services";

                    string implementation =
                    $@"public class {serviceClassName} : I{serviceClassName}
                        {{
                            private AppDbContext _dbContext;
                            private IMapper _mapper;

                            public Services(IMapper mapper, AppDbContext dbContext)
                            {{
                                _mapper = mapper;
                                _dbContext = dbContext;
                            }}

                            public async Task<IEnumerable<{modelClassName}Dto>> GetAsync()
                            {{
                                return _mapper.Map<List<{modelClassName}Dto>>(await _dbContext.{modelClassName}s.ToListAsync());
                            }}

                            public async Task<IEnumerable<{modelClassName}Dto>> GetAsync({modelClassName}Dto t)
                            {{
                                if (t == null) throw new ArgumentNullException(nameof(t));
                                var {modelClassName.UpperToLower()} = _mapper.Map<{modelClassName}>(t);
                                return _mapper.Map<List<{modelClassName}Dto>>(_dbContext.{modelClassName}s.ToListAsync());
                            }}

                            public async Task<{modelClassName}Dto> GetAsync(int id)
                            {{
                                return _mapper.Map<{modelClassName}Dto>(_dbContext.{modelClassName}s.SingleOrDefaultAsync(x => x.Id == id));
                            }}

                            public async Task<{modelClassName}Dto> PostAsync({modelClassName}Dto t)
                            {{
                                if (t == null) throw new ArgumentNullException(nameof(t));
                                var {modelClassName.UpperToLower()} = _mapper.Map<{modelClassName}>(t);
                                await _dbContext.{modelClassName}s.AddAsync({modelClassName.UpperToLower()});
                                await _dbContext.SaveChangesAsync();
                                return _mapper.Map<{modelClassName}Dto>({modelClassName.UpperToLower()});
                            }}

                            public async Task<{modelClassName}Dto> PutAsync({modelClassName}Dto t)
                            {{
                                if (t == null) throw new ArgumentNullException(nameof(t));
                                var {modelClassName.UpperToLower()} = _mapper.Map<{modelClassName}>(t);
                                var {modelClassName.UpperToLower()}InDb = await _dbContext.{modelClassName}.SingleOrDefaultAsync(x => x.Id == {modelClassName.UpperToLower()}.Id);
                                {modelClassName.UpperToLower()}InDb.Name = {modelClassName.UpperToLower()}.Name;
                                await _dbContext.SaveChangesAsync();
                                return _mapper.Map<{modelClassName}Dto>({modelClassName.UpperToLower()});
                            }}
                            public async Task<bool> DeleteAsync(int id)
                            {{
                                var {modelClassName.UpperToLower()}InDb = await _dbContext.{modelClassName}.SingleOrDefaultAsync(x => x.Id == id);
                                if ({modelClassName.UpperToLower()}InDb != null)
                                {{
                                    _dbContext.{modelClassName}s.Remove({modelClassName.UpperToLower()}InDb);
                                    await _dbContext.SaveChangesAsync();
                                    return true;
                                }}

                                return false;
                            }}
                            
                        }}";
                    var newClassNode =
                      SyntaxFactory.ParseSyntaxTree(implementation)
                          .GetRoot()
                          .DescendantNodes()
                          .OfType<ClassDeclarationSyntax>()
                          .FirstOrDefault();
                    // Retrieve the parent namespace declaration
                    if (!(classNode.Parent is NamespaceDeclarationSyntax)) return null;
                    var parentNamespace = (NamespaceDeclarationSyntax)classNode.Parent;
                    // Add the new class to the namespace
                    var newParentNamespace = parentNamespace.AddMembers(newClassNode).NormalizeWhitespace();
                    
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

