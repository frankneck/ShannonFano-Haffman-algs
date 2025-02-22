using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Crypto.CreateCombinations();
            
            Crypto.DoShannonFano();
            Crypto.DoHuffman();

            Console.ReadKey();

            return 0;
        }
    }
}
