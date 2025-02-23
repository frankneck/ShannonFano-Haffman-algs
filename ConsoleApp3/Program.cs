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
            Crypto.DoTask1();
        
            // Console.Write($"Введите последовательность символов: ");
            // Crypto.EncodeSequance();
            Console.ReadKey();

            return 0;
        }
    }
}
