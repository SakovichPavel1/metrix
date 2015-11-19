using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metrics;
using System.Text.RegularExpressions;



namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            string text = System.IO.File.ReadAllText(@"D:\метрика\ConsoleApplication2\ConsoleApplication2\MainCharacter.cpp");

            // Display the file contents to the console. Variable text is a string.
            System.Console.WriteLine("Contents of WriteText.txt:\n{0}", text);
            text = Preprocessor.PreprocessCode(text);
            System.Console.WriteLine("___________________________________________________________________________");
            System.Console.WriteLine("CodeText without comments and literals:\n{0}", text);
            System.Console.WriteLine("___________________________________________________________________________");
            //System.Console.WriteLine("Count of if, case, while, for =  {0}", SearchMatching.CountingOccurrences(text));
            SearchMatching.CountingOccurrences(text);
            System.Console.ReadKey();
        }
    }
}
