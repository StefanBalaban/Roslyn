using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Roslyn.Classes;
using Roslyn.Interfaces;

namespace Roslyn.Classes
{
    [Route("api/examples")]
    //[ApiController]
    public class ExamplesApi : ControllerBase
    {
        private readonly IExampleServices _exampleServices;

        public ExamplesApi(IExampleServices exampleServices)
        {
            _exampleServices = exampleServices;
        }
        // GET: api/<ExamplesApi>
        [HttpGet]
        public async Task<IEnumerable<ExampleDto>> Get()
        {
            return await _exampleServices.GetAsync();
        }

        // GET api/<ExamplesApi>/5
        [HttpGet("{id}")]
        public async Task<ExampleDto> Get(int id)
        {
            return await _exampleServices.GetAsync(id);
        }
        // POST api/<ExamplesApi>
        [HttpPost]
        public async Task<IEnumerable<ExampleDto>> Get([FromBody] ExampleDto example)
        {
            return await _exampleServices.GetAsync(example);
        }

        // POST api/<ExamplesApi>
        [HttpPost]
        public async Task<ExampleDto> Post([FromBody] ExampleDto example)
        {
            return await _exampleServices.PostAsync(example);
        }

        // PUT api/<ExamplesApi>/5
        [HttpPut("{id}")]
        public async Task<ExampleDto> Put(int id, [FromBody] ExampleDto example)
        {
            return await _exampleServices.PutAsync(example);
        }

        // DELETE api/<ExamplesApi>/5
        [HttpDelete("{id}")]
        public async Task<bool> Delete(int id)
        {
            return await _exampleServices.DeleteAsync(id);
        }
    }
}
