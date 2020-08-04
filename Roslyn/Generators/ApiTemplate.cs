using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Extensions;

namespace Roslyn.Generators
{
    class ApiTemplate
    {
        public static string GenerateTemplate(ClassDeclarationSyntax classNode)
        {
            var modelClassName = classNode.Identifier.Text;
            return GenerateClass(modelClassName);
        }
        private static string GenerateClass(string modelClassName)
        {
            return
                $@"
                namespace Controllers
                {{
                    [Route(""api/{modelClassName.ToCamelCase()}s"")]
                    [ApiController]
                    public class {modelClassName}sApi : ControllerBase
                    {{
                        private readonly I{modelClassName}Services _{modelClassName.ToCamelCase()}Services;

                        public {modelClassName}sApi(I{modelClassName}Services {modelClassName.ToCamelCase()}Services)
                        {{
                            _{modelClassName.ToCamelCase()}Services = {modelClassName.ToCamelCase()}Services;
                        }}
                        // GET: api/<{modelClassName}sApi>
                        [HttpGet]
                        public async Task<IActionResult> Get()
                        {{
                            return Ok(await _{modelClassName.ToCamelCase()}Services.GetAsync());
                        }}

                        // GET api/<{modelClassName}Api>/5
                        [HttpGet(""{{id}}"")]
                        public async Task<IActionResult> Get(int id)
                        {{
                            var {modelClassName.ToCamelCase()} = await _{modelClassName.ToCamelCase()}Services.GetAsync(id);
                            if ({modelClassName.ToCamelCase()} == null) return NotFound();
                            return Ok({modelClassName.ToCamelCase()});
                        }}
                        // POST api/<{modelClassName}sApi>
                        [HttpPost]
                        public async Task<IActionResult> Get([FromBody] {modelClassName}Dto {modelClassName.ToCamelCase()})
                        {{
                            return Ok(await _{modelClassName.ToCamelCase()}Services.PostAsync({modelClassName.ToCamelCase()}));
                        }}

                        // PUT api/<{modelClassName}sApi>/5
                        [HttpPut]
                        public async Task<IActionResult> Put([FromBody] {modelClassName}Dto {modelClassName.ToCamelCase()}Dto)
                        {{
                            var {modelClassName.ToCamelCase()} = await _{modelClassName.ToCamelCase()}Services.PutAsync({modelClassName.ToCamelCase()}Dto);
                            if ({modelClassName.ToCamelCase()} == null) return NotFound();
                            return Ok({modelClassName.ToCamelCase()});
                        }}

                        // DELETE api/<{modelClassName}sApi>/5
                        [HttpDelete(""{{id}}"")]
                        public async Task<IActionResult> Delete(int id)
                        {{
                            var {modelClassName.ToCamelCase()} = await _{modelClassName.ToCamelCase()}Services.DeleteAsync(id);
                            if (!{modelClassName.ToCamelCase()}) return NotFound();
                            return Ok({modelClassName.ToCamelCase()});
                        }}
                    }}
                }}";
        }
    }
}
