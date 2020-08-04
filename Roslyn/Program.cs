using Roslyn.Generators;


namespace Roslyn
{
    class Program
    {
        static void Main(string[] args)
        {
            const string models =
                @"namespace Model
                {
                    public class Make
                    {
                        [Get]
                        [Post]
                        [Put]
                        public string Name { get; set; }
                        [Include]                        
                        public List<Model> Models { get; set; }
                    }
                }";
            Generator.Generate(models);
        }
    }
}
