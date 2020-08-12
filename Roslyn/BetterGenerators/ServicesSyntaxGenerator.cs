using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Generic;
using System.Linq;
using Roslyn.Extensions;

namespace Roslyn.BetterGenerators
{
    public class ServicesSyntaxGenerator
    {
        AdhocWorkspace workspace = new AdhocWorkspace();
        private SyntaxGenerator generator;
        private List<KeyValuePair<string, string>> propertiesWithAttributes = new List<KeyValuePair<string, string>>();
        public SyntaxNode GenerateServiceClassNode(ClassDeclarationSyntax classNode)
        {
            GetPropertiesAndAttributes(classNode.DescendantNodes().OfType<MemberDeclarationSyntax>());
            var modelClassName = classNode.Identifier.Text;

            generator = SyntaxGenerator.GetGenerator(workspace, LanguageNames.CSharp);

            // using
            var usingDeclarationSystem = generator.NamespaceImportDeclaration("System");
            var usingDeclarationSystemLinq = generator.NamespaceImportDeclaration("System.Linq");
            var usingDeclarationSystemCollectionsGeneric = generator.NamespaceImportDeclaration("System.Collections.Generic");
            var usingDeclarationSystemThreadingTasks = generator.NamespaceImportDeclaration("System.Threading.Tasks");

            // _dbContext
            var appDbContextField = generator.FieldDeclaration("_dbContext", SyntaxFactory.ParseTypeName("AppDbContext"), Accessibility.Private, DeclarationModifiers.ReadOnly);

            var constructorParameters = new[] { generator.ParameterDeclaration("appDbContext", SyntaxFactory.ParseTypeName("AppDbContext")) };
            var constructorBody = new[] {
                generator.AssignmentStatement(
                    generator.IdentifierName("_dbContext"),
                    generator.IdentifierName("appDbContext"))};
            // ctor
            var constructor = generator.ConstructorDeclaration($"{modelClassName}Services", constructorParameters, Accessibility.Public,
                statements: constructorBody);

            // GetAsync()
            var getAsyncMethod = GenerateGetAsyncMethod(modelClassName);
            // GetAsync(int id)
            var getByIdAsyncMethod = GenerateGetByIdAsyncMethod(modelClassName);
            // GetAsync(T t)
            var getBySearchObjectMethod = GenerateGetBySearchObjectAsyncMethod(modelClassName);
            // PostAsync(T t)
            var postAsyncMethod = GeneratePostAsyncMethod(modelClassName);
            // PutAsync(T t)
            var putAsyncMethod = GeneratePutAsyncMethod(modelClassName);
            // DeleteAsync(int id)
            var deleteAsyncMethod = GenerateDeleteAsyncMethod(modelClassName);

            var members = new[]
            {
                usingDeclarationSystem,
                usingDeclarationSystemLinq,
                usingDeclarationSystemCollectionsGeneric,
                usingDeclarationSystemThreadingTasks,
                appDbContextField,
                constructor,
                getAsyncMethod,
                getByIdAsyncMethod,
                getBySearchObjectMethod,
                postAsyncMethod,
                putAsyncMethod,
                deleteAsyncMethod
            };

            // Class
            var classDefinition = generator.ClassDeclaration($"{modelClassName}Services", typeParameters: null, accessibility: Accessibility.Public, modifiers: DeclarationModifiers.None,
                baseType: null,
                members: members);

            // Namespaces
            var namespaceDeclaration = generator.NamespaceDeclaration("Services", classDefinition);

            // Compilation unit
            var newNode = generator.CompilationUnit(
                    usingDeclarationSystem,
                    usingDeclarationSystemLinq,
                    usingDeclarationSystemCollectionsGeneric,
                    usingDeclarationSystemThreadingTasks, namespaceDeclaration).
                NormalizeWhitespace();

            return newNode;
        }

        private SyntaxNode GenerateDeleteAsyncMethod(string modelClassName)
        {
            var methodBody = new List<SyntaxNode>
            {
                SyntaxFactory.ParseStatement($"var {modelClassName.ToCamelCase()} = await _dbContext.{modelClassName}s.FindAsync(id);"),
                SyntaxFactory.ParseStatement($"if ({modelClassName.ToCamelCase()} == null) return false;"),
                SyntaxFactory.ParseStatement($"_dbContext.Remove({modelClassName.ToCamelCase()});"),
                SyntaxFactory.ParseStatement($"await _dbContext.SaveChangesAsync();"),
                SyntaxFactory.ParseStatement($"return true;")
            };

            return generator.MethodDeclaration("DeleteAsync", new[] { generator.ParameterDeclaration("id", generator.TypeExpression(SpecialType.System_Int32)) }, null,
                SyntaxFactory.ParseTypeName($"Task<bool>"),
                Accessibility.Public,
                DeclarationModifiers.Async, methodBody);
        }

        private SyntaxNode GeneratePutAsyncMethod(string modelClassName)
        {
            var properties = propertiesWithAttributes.Where(x => x.Value == "Put").Select(x => x.Key).ToList();

            var methodBody = new List<SyntaxNode>
            {
                SyntaxFactory.ParseStatement("if (t == null) throw new ArgumentNullException(nameof(t));"),
                SyntaxFactory.ParseStatement($"var {modelClassName.ToCamelCase()}InDb = await _dbContext.{modelClassName}s.FindAsync(t.Id);"),
                SyntaxFactory.ParseStatement($"if ({modelClassName.ToCamelCase()}InDb == null) return null;")
            };
            foreach (var property in properties)
            {
                methodBody.Add(SyntaxFactory.ParseStatement($"{modelClassName.ToCamelCase()}InDb.{property} = t.{property};"));
            }
            methodBody.Add(SyntaxFactory.ParseStatement("await _dbContext.SaveChangesAsync();"));
            methodBody.Add(SyntaxFactory.ParseStatement($"return {modelClassName.ToCamelCase()}InDb;"));

            return generator.MethodDeclaration("PutAsync",
                new[] { generator.ParameterDeclaration("t", SyntaxFactory.ParseTypeName(modelClassName)) }, null,
                SyntaxFactory.ParseTypeName($"Task<{modelClassName}>"),
                Accessibility.Public,
                DeclarationModifiers.Async, methodBody);
        }

        private SyntaxNode GeneratePostAsyncMethod(string modelClassName)
        {
            var properties = propertiesWithAttributes.Where(x => x.Value == "Post").Select(x => x.Key).ToList();

            var methodBody = new List<SyntaxNode>
            {
                SyntaxFactory.ParseStatement("if (t == null) throw new ArgumentNullException(nameof(t));"),
                SyntaxFactory.ParseStatement($"var {modelClassName.ToCamelCase()} = new {modelClassName}();")
            };
            foreach (var property in properties)
            {
                methodBody.Add(SyntaxFactory.ParseStatement($"{modelClassName.ToCamelCase()}.{property} = t.{property};"));
            }
            methodBody.Add(SyntaxFactory.ParseStatement($"await _dbContext.{modelClassName}s.AddAsync({modelClassName.ToCamelCase()});"));
            methodBody.Add(SyntaxFactory.ParseStatement($"await _dbContext.SaveChangesAsync();"));
            methodBody.Add(SyntaxFactory.ParseStatement($"return {modelClassName.ToCamelCase()};"));

            return generator.MethodDeclaration("PostAsync",
                new[] {generator.ParameterDeclaration("t", SyntaxFactory.ParseTypeName(modelClassName))}, null,
                SyntaxFactory.ParseTypeName($"Task<{modelClassName}>"),
                Accessibility.Public,
                DeclarationModifiers.Async, methodBody);
        }

        private SyntaxNode GenerateGetBySearchObjectAsyncMethod(string modelClassName)
        {
            var properties = propertiesWithAttributes.Where(x => x.Value == "Get").Select(x => x.Key).ToList();
            var includes = propertiesWithAttributes.Where(x => x.Value == "Include").Select(x => x.Key).ToList();


            var methodBody = new List<SyntaxNode>
            {
                SyntaxFactory.ParseStatement("if (t == null) throw new ArgumentNullException(nameof(t));"),
                SyntaxFactory.ParseStatement($"return await _dbContext.{modelClassName}s")
            };
            foreach (var include in includes)
            {
                methodBody.Add(SyntaxFactory.ParseStatement($".Include(x => x.{include})"));
            }
            foreach (var property in properties)
            {
                methodBody.Add(SyntaxFactory.ParseStatement($".Where(x => x.{property} == null || x.{property}.Equals(t.{property}))"));
            }

            methodBody.Add(SyntaxFactory.ParseStatement($".ToListAsync();"));

            return generator.MethodDeclaration("GetAsync", new[] { generator.ParameterDeclaration("t", SyntaxFactory.ParseTypeName(modelClassName)) }, null,
                SyntaxFactory.ParseTypeName($"Task<IEnumerable<{modelClassName}>>"),
                Accessibility.Public,
                DeclarationModifiers.Async, methodBody);
        }

        private SyntaxNode GenerateGetByIdAsyncMethod(string modelClassName)
        {
            var includes = propertiesWithAttributes.Where(x => x.Value == "Include").Select(x => x.Key).ToList();

            var methodBody = new List<SyntaxNode> { SyntaxFactory.ParseStatement($"return await _dbContext.{modelClassName}s") };
            foreach (var include in includes)
            {
                methodBody.Add(SyntaxFactory.ParseStatement($".Include(x => x.{include})"));
            }
            methodBody.Add(SyntaxFactory.ParseStatement($".FirstOrDefaultAsync(x => x.Id == id);"));

            return generator.MethodDeclaration("GetAsync", new[] { generator.ParameterDeclaration("id", generator.TypeExpression(SpecialType.System_Int32)) }, null,
                SyntaxFactory.ParseTypeName($"Task<{modelClassName}>"),
                Accessibility.Public,
                DeclarationModifiers.Async, methodBody);
        }

        private SyntaxNode GenerateGetAsyncMethod(string modelClassName)
        {
            var includes = propertiesWithAttributes.Where(x => x.Value == "Include").Select(x => x.Key).ToList();

            var methodBody = new List<SyntaxNode> { SyntaxFactory.ParseStatement($"return await _dbContext.{modelClassName}s") };
            foreach (var include in includes)
            {
                methodBody.Add(SyntaxFactory.ParseStatement($".Include(x => x.{include})"));
            }
            methodBody.Add(SyntaxFactory.ParseStatement($".ToListAsync();"));

            return generator.MethodDeclaration("GetAsync", null, null,
                SyntaxFactory.ParseTypeName($"Task<IEnumerable<{modelClassName}>>"),
                Accessibility.Public,
                DeclarationModifiers.Async, methodBody);
        }

        private void GetPropertiesAndAttributes(IEnumerable<MemberDeclarationSyntax> members)
        {

            foreach (var memberDeclarationSyntax in members)
            {
                var attributeName = new List<string>();
                var property = memberDeclarationSyntax as PropertyDeclarationSyntax;
                var attributes = property.AttributeLists.ToList();
                foreach (var attributeListSyntax in attributes)
                {
                    attributeName.Add(attributeListSyntax.Attributes.First().Name.NormalizeWhitespace().ToFullString());
                }

                if (attributeName != null)
                    attributeName.ForEach(x => propertiesWithAttributes.Add(new KeyValuePair<string, string>(property.Identifier.Text, x)));
            }
        }
    }
}
