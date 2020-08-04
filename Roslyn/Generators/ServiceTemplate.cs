using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Extensions;

namespace Roslyn.Generators
{
    public class ServiceTemplate
    {
        public static string GenerateTemplate(ClassDeclarationSyntax classNode)
        {
            var modelClassName = classNode.Identifier.Text;     
            var properties = GetPropertiesAndAttributes(classNode.DescendantNodes().OfType<MemberDeclarationSyntax>());
            return GenerateClass(modelClassName, properties);
        }

        private static string GenerateClass(string modelClassName, List<KeyValuePair<string, string>> properties)
        {
            return 
                $@"
            namespace Services 
            {{
                public class {modelClassName}Services : I{modelClassName}Services
                {{
                    private IAsyncRepository<{modelClassName}> _dbContext;
                    private IMapper _mapper;

                    public {modelClassName}Services(IMapper mapper, IAsyncRepository<{modelClassName}> dbContext)
                    {{
                        _mapper = mapper;
                        _dbContext = dbContext;
                    }}

                    public async Task<IEnumerable<{modelClassName}Dto>> GetAsync()
                    {{
                        return _mapper.Map<List<{modelClassName}Dto>>(await _dbContext.ListAllAsync({GenerateIncludes(properties.Where(x => x.Value == "Include").Select(x => x.Key))}));
                    }}

                    public async Task<IEnumerable<{modelClassName}Dto>> GetAsync({modelClassName}Dto t)
                    {{
                        if (t == null) throw new ArgumentNullException(nameof(t));
                        var {modelClassName.ToCamelCase()} = _mapper.Map<{modelClassName}>(t);
                        return _mapper.Map<List<{modelClassName}Dto>>(
                            (await _dbContext.ListAllAsync({GenerateIncludes(properties.Where(x => x.Value == "Include").Select(x => x.Key))}))
                                .Where(x => {GeneratePredicates(properties.Where(x => x.Value == "Get" ).Select(x =>x.Key), modelClassName.ToCamelCase())});
                    }}

                    public async Task<{modelClassName}Dto> GetAsync(int id)
                    {{
                        return _mapper.Map<{modelClassName}Dto>(await _dbContext.GetByIdAsync(id {GenerateIncludesForGetById(properties.Where(x => x.Value == "Include").Select(x => x.Key)) }));
                    }}

                    public async Task<{modelClassName}Dto> PostAsync({modelClassName}Dto t)
                    {{
                        if (t == null) throw new ArgumentNullException(nameof(t));
                        var {modelClassName.ToCamelCase()} = _mapper.Map<{modelClassName}>(t);
                        var {modelClassName.ToCamelCase()}CreateObject = new {modelClassName}();
                        {GenerateCreateObjectProperties(properties.Where(x => x.Value == "Post").Select(x => x.Key), modelClassName.ToCamelCase())}
                        await _dbContext.AddAsync({modelClassName.ToCamelCase()}CreateObject);
                        return _mapper.Map<{modelClassName}Dto>({modelClassName.ToCamelCase()}CreateObject);
                    }}

                    public async Task<{modelClassName}Dto> PutAsync({modelClassName}Dto t)
                    {{
                        if (t == null) throw new ArgumentNullException(nameof(t));
                        var {modelClassName.ToCamelCase()} = _mapper.Map<{modelClassName}>(t);
                        var {modelClassName.ToCamelCase()}InDb = await _dbContext.GetByIdAsync({modelClassName.ToCamelCase()}.Id);
                        {GeneratePutObjectProperties(properties.Where(x => x.Value == "Put").Select(x => x.Key), modelClassName.ToCamelCase())}
                        await _dbContext.UpdateAsync();
                        return _mapper.Map<{modelClassName}Dto>({modelClassName.ToCamelCase()}InDb);
                    }}
                    public async Task<bool> DeleteAsync(int id)
                    {{
                        var {modelClassName.ToCamelCase()}InDb = await _dbContext.GetByIdAsync(id);
                        if ({modelClassName.ToCamelCase()}InDb == null) return false;
                        await _dbContext.DeleteAsync({modelClassName.ToCamelCase()}InDb);
                        return true;
                    }}
                    
                }}
            }}";
        }

        private static string GenerateIncludesForGetById(IEnumerable<string> properties)
        {
            var includes = GenerateIncludes(properties);
            if (String.IsNullOrWhiteSpace(includes)) return "";
            return ", " + includes;
        }

        private static string GeneratePutObjectProperties(IEnumerable<string> properties, string modelObjectName)
        {
            var propertyNames = properties.ToList();
            StringBuilder str = new StringBuilder("");
            foreach (var property in properties)
            {
                str.AppendLine(@$"{modelObjectName}InDb.{property} = {modelObjectName}.{property};");
            }
            return str.ToString();
        }

        private static string GenerateCreateObjectProperties(IEnumerable<string> properties, string modelObjectName)
        {
            var propertyNames = properties.ToList();
            StringBuilder str = new StringBuilder("");
            foreach (var property in properties)
            {
                str.AppendLine(@$"{modelObjectName}CreateObject.{property} = {modelObjectName}.{property};");
            }
            return str.ToString();
        }

        private static string GenerateIncludes(IEnumerable<string> properties)
        {
            var propertyNames = properties.ToList();
            if (propertyNames.Count == 0) return "";
            StringBuilder str = new StringBuilder("new List<string> {");
            for (var i = 0; i < propertyNames.Count; i++)
            {
                if (((i == 0) && (i + 1 == propertyNames.Count)) || (i + 1 == propertyNames.Count && i != 0) )
                    str.Append(@$" ""{propertyNames[i]}"" ");

                if (i + 1 < propertyNames.Count && i != 0)
                    str.Append(@$" ""{propertyNames[i]}"", ");

                if (i == 0 && i + 1 != propertyNames.Count)
                    str.Append(@$" ""{propertyNames[i]}"", ");
            }

            str.Append("}");
            return str.ToString();
        }

        private static string GeneratePredicates(IEnumerable<string> properties, string modelClassName)
        {

            var propertyNames = properties.ToList();
            if (propertyNames.Count == 0) return "x == x )";
            StringBuilder str = new StringBuilder();
            for (var i = 0; i < propertyNames.Count; i++)
            {
                if ((i == 0) && (i + 1 == propertyNames.Count)) 
                    str.Append($" x.{propertyNames[i]} == {modelClassName}.{propertyNames[i]} ");

                if ((i + 1 < propertyNames.Count && i != 0) || (i + 1 == propertyNames.Count && i != 0)) 
                    str.Append($" && x.{propertyNames[i]} == {modelClassName}.{propertyNames[i]} ");

                if (i == 0 && i + 1 != propertyNames.Count) 
                    str.Append($" x.{propertyNames[i]} == {modelClassName}.{propertyNames[i]} ");
            }

            str.Append(")");
            return str.ToString();
        }

        private static List<KeyValuePair<string, string>> GetPropertiesAndAttributes(IEnumerable<MemberDeclarationSyntax> members)
        {
            List<string> attributeName;
            List<KeyValuePair<string, string>> propertiesWithAttributes = new List<KeyValuePair<string, string>>();
            foreach (var memberDeclarationSyntax in members)
            {
                attributeName = new List<string>();
                var property = memberDeclarationSyntax as PropertyDeclarationSyntax;
                var attributes = property.AttributeLists.ToList();
                foreach (var attributeListSyntax in attributes)
                {
                    attributeName.Add(attributeListSyntax.Attributes.First().Name.NormalizeWhitespace().ToFullString());
                }
                
                if (attributeName != null)
                    attributeName.ForEach(x => propertiesWithAttributes.Add(new KeyValuePair<string, string>(property.Identifier.Text, x)));
            }

            return propertiesWithAttributes;
        }
    }
}

