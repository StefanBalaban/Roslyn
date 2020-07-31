using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;

namespace Roslyn.Classes
{
    class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<Example, ExampleDto>();
            CreateMap<ExampleDto, Example>();
            
        }
    }
}
