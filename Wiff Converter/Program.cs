using System;

namespace Wiff_Converter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("About to call some functions and do stuff from the command line...");
            Wiff_Converter.fMain fm1 = Wiff_Converter.fMain();
            fm1.cliConvert();
        }
    }
}
