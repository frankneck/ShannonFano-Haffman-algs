using System;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    class Program
    {
        const string filePath = "C:\\Users\\finnf\\Documents\\Learning_DVFU\\6 sem\\Crypto\\teskt_dlya_3_zadachi.docx";
        static int Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            
            // Task 1
            Console.Write($"Введите алфавит: ");
            Crypto.CreateCombinations();
            Crypto.DoTask1();
        
            // Task 2
            Console.Write($"Введите последовательность символов: ");
            string str = Console.ReadLine();
            Crypto.EncdoeSequTask2(str);
            Console.ReadKey();

            // Task 3
            Crypto.FindInDocumentTask3(filePath);
            
            return 0;
        }
    }
}
