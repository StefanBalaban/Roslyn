using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Roslyn.Interfaces;

namespace Roslyn.Classes
{
    public class Services : IExampleServices
    {
        private AppDbContext _dbContext;
        private IMapper _mapper;
        public Services(IMapper mapper, AppDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<ExampleDto>> GetAsync()
        {
            return _mapper.Map<List<ExampleDto>>(await _dbContext.Examples.ToListAsync());
        }

        public async Task<IEnumerable<ExampleDto>> GetAsync(ExampleDto t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            var example = _mapper.Map<Example>(t);
            return _mapper.Map<List<ExampleDto>>(_dbContext.Examples.Where(x => x.Name == example.Name).ToListAsync());
        }

        public async Task<ExampleDto> GetAsync(int id)
        {
            return _mapper.Map<ExampleDto>(_dbContext.Examples.SingleOrDefaultAsync(x => x.Id == id));
        }

        public async Task<ExampleDto> PostAsync(ExampleDto t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            var example = _mapper.Map<Example>(t);
            await _dbContext.Examples.AddAsync(example);
            await _dbContext.SaveChangesAsync();
            return _mapper.Map<ExampleDto>(example);

        }

        public async Task<ExampleDto> PutAsync(ExampleDto t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            var example = _mapper.Map<Example>(t);
            var exampleInDb = await _dbContext.Examples.SingleOrDefaultAsync(x => x.Id == example.Id);
            exampleInDb.Name = example.Name;
            await _dbContext.SaveChangesAsync();
            return _mapper.Map<ExampleDto>(example);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var exampleInDb = await _dbContext.Examples.SingleOrDefaultAsync(x => x.Id == id);
            if (exampleInDb != null)
            {
                _dbContext.Examples.Remove(exampleInDb);
                await _dbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
