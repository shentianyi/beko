using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TEST.Console1
{
    class Program
    {
        static void Main(string[] args)
        {
            string a = "a";
            string b = "b";
            b = a;
            a = "aa";

            Console.WriteLine(b);
            Console.Read();
        }
    }
}
