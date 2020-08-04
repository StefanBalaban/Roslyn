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
                        public string Name { get; set; }

                    }
                }";
            Generator.Generate(models);
        }
    }
}
