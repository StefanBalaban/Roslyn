using System.Collections.Generic;
using System.Text;
using Roslyn.Attributes;

namespace Roslyn.Classes
{
    class Person
    {
        [Get]
        public int Id { get; set; }
        [Get]
        [Post]
        [Put]
        public string Name { get; set; }
        [Include]
        [Post]
        [Put]
        public List<Child> Children { get; set; }
    }
}
