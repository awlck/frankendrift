using System;
using System.Runtime.InteropServices.JavaScript;

namespace FrankenDrift.WebRunner
{
    public partial class Core
    {
        public static void Main()
        {
            Console.WriteLine("Hello, Browser!");
        }

        [JSExport]
        public static string Greeting()
        {
            return "Hello from .NET!";
        }


    }
}