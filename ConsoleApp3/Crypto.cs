using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using DocumentFormat.OpenXml.Packaging;         // для работы с доками с расширениями .docx, .xml, .pptx
using DocumentFormat.OpenXml.Wordprocessing;    // для работы с текстом

namespace ConsoleApp3
{
    static public class Crypto
    {
        static int n = 0;   // alpha
        static int k = 0;   // length
        static int i = 0;   // counter
        static float sumProbability = 0.0f;
        static List<char> charList = new List<char>();
        static List<float> probabilities = new List<float>();
        static List<string> gettingSequences = new List<string>();
        static List<float> probabilityOfSequences = new List<float>();
        static List<(string sequence, float probability)> sequencesWithProbabilities = new List<(string sequence, float probability)>();
        static Dictionary<char, float> dictionaryOfProbabilities = new Dictionary<char, float>();
        static Dictionary<string, string> shannonFanoCodes = new Dictionary<string, string>();
        static Dictionary<string, string> huffmanCodes = new Dictionary<string, string>();

        

        // Порог вероятности для фильтрации
        static float probabilityThreshold = 0.01f;

        public static void DoTask1()
        {
            DoShannonFano(sequencesWithProbabilities);
            DoHuffman(sequencesWithProbabilities);
        }

        public static void DoShannonFano(List<(string sequence, float probability)> listWithProbabilities)
        {
            // Создаем список пар (последовательность, вероятность) и сортируем по убыванию вероятности
            sequencesWithProbabilities = listWithProbabilities;

            // Запускаем рекурсивную функцию кодирования
            ShannonFanoRecursive(sequencesWithProbabilities, "");

            // Print table
            PrintTable(shannonFanoCodes, "Shannon-Fano");
        }

        private static void ShannonFanoRecursive(List<(string sequence, float probability)> sequences, string prefix)
        {
            if (sequences.Count == 1)
            {
                shannonFanoCodes[sequences[0].sequence] = prefix;
                return;
            }

            if (sequences.Count == 0)
            {
                return;
            }

            float totalProbability = sequences.Sum(pair => pair.probability);

            if (totalProbability < probabilityThreshold)
            {
                foreach (var seq in sequences)
                {
                    shannonFanoCodes[seq.sequence] = prefix;
                }
                return;
            }

            float halfProbability = 0;
            int splitIndex = 0;

            for (int i = 0; i < sequences.Count - 1; i++)
            {
                halfProbability += sequences[i].probability;
                if (halfProbability >= totalProbability / 2)
                {
                    splitIndex = i + 1;
                    break;
                }
            }

            // Защита от пустых частей разбиения
            if (splitIndex == 0 || splitIndex >= sequences.Count)
            {
                splitIndex = sequences.Count / 2; // Если разбиение не работает, делим список пополам
            }

            var leftPart = sequences.Take(splitIndex).ToList();
            var rightPart = sequences.Skip(splitIndex).ToList();

            // Проверяем, чтобы обе части не были пустыми
            if (leftPart.Count > 0 && rightPart.Count > 0)
            {
                ShannonFanoRecursive(leftPart, prefix + "0");
                ShannonFanoRecursive(rightPart, prefix + "1");
            }
            else
            {
                foreach (var seq in sequences)
                {
                    shannonFanoCodes[seq.sequence] = prefix;
                }
            }
        }

        public static void DoHuffman(List<(string sequence, float probability)> listWithProbabilities)
        {
            // sort seq
            listWithProbabilities = listWithProbabilities.OrderByDescending(pair => pair.probability).ToList();

            // create the Haffman tree
            var huffmanTree = BuildHuffmanTree(listWithProbabilities);

            // Кодируем с использованием дерева Хаффмана
            GenerateHuffmanCodes(huffmanTree, "");

            // Print table
            PrintTable(huffmanCodes, "Huffman");
        }
        
        private static HuffmanNode BuildHuffmanTree(List<(string sequence, float probability)> sequences)
        {
            var nodes = sequences.Select(pair => new HuffmanNode(pair.sequence, pair.probability)).ToList(); // good

            while (nodes.Count > 1)
            {
                nodes = nodes.OrderBy(node => node.Probability).ToList();
                
                var left = nodes[0];
                var right = nodes[1];
                nodes.RemoveRange(0, 2);

                var parent = new HuffmanNode("", left.Probability + right.Probability)
                {
                    Left = left,
                    Right = right
                };

                nodes.Add(parent);
            }

            return nodes.First();
        }
        // recuirsive
        private static void GenerateHuffmanCodes(HuffmanNode node, string prefix)
        {
            if (node.Left == null && node.Right == null)
            {
                huffmanCodes[node.Sequence] = prefix;
                return;
            }

            if (node.Left != null)
                GenerateHuffmanCodes( node.Left, prefix + "0");

            if (node.Right != null)
                GenerateHuffmanCodes(node.Right, prefix + "1");
        }

        public static void CreateCombinations()
        {
            ReadSequence();

            // Ввод длины слова с проверкой
            while (true)
            {
                Console.Write("Enter length of words: ");
                string inputLength = Console.ReadLine().Trim();

                // Попытка преобразовать ввод в целое число
                if (int.TryParse(inputLength, out k) && k > 0)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input! Please enter a positive integer for the length.");
                }
            }

            Console.WriteLine("Enter probability for each symbol!");
            while (true)
            {
                probabilities.Clear();
                sumProbability = 0.0f;
                i = 0;

                while (i < n)
                {
                    Console.Write($"P({charList[i]}) = ");
                    float probability;
                    if (float.TryParse(Console.ReadLine(), out probability))
                    {
                        probabilities.Add(probability);
                        sumProbability += probability;
                    }
                    else
                    {
                        Console.WriteLine("Invalid probability! Please enter a valid float number.");
                        i--;  // Перезапрашиваем текущую вероятность
                    }
                    i++;
                }

                if (Math.Abs(sumProbability - 1.0f) < 0.0001f)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Your entered invalid data (your sum of probability doesn't equal 1. Restarting...)");
                }
            }

            // Добавляем вероятности в словарь
            for (i = 0; i < n; i++)
            {
                dictionaryOfProbabilities[charList[i]] = probabilities[i];
            }

            // Генерация всех возможных комбинаций
            GenerateCombination("", 0);

            // Подсчитываем вероятность для каждой комбинации
            for (i = 0; i < gettingSequences.Count; i++)
            {
                float probabilityOfSequence = 1.0f;

                foreach (char ch in gettingSequences[i])
                {
                    if (dictionaryOfProbabilities.TryGetValue(ch, out float symbolProbability))
                    {
                        probabilityOfSequence *= symbolProbability;
                    }
                }

                probabilityOfSequences.Add(probabilityOfSequence);
            }

            // Засунул сюда преобразование в список кортежей чтобы облегчить себе работу (была в DoShannon)
            sequencesWithProbabilities = gettingSequences.Zip(probabilityOfSequences, (s, p) => (s, p)).OrderByDescending(pair => pair.Item2).ToList();
        }

        public static void ReadSequence()
        {
            charList.Clear();

            while (true)
            {
                charList = Console.ReadLine().Distinct().ToList();
                n = charList.Count;

                if (n == 0)
                {
                    Console.WriteLine($"Error! Line mustn't be void. ");
                }
                else
                {
                    break;
                }
            }
        }

        public static void GenerateCombination(string current, int depth)
        {
            if (depth == k)
            {
                gettingSequences.Add(current);
                return;
            }

            foreach (char c in charList)
            {
                GenerateCombination(current + c, depth + 1);
            }
        }

        public static void EncdoeSequTask2(string str)
        {
            Dictionary<string, int> pairs = new Dictionary<string, int>();
            Dictionary<string, float> probabilities = new Dictionary<string, float>();
            List<(string, float)> listWithProbabilities = new List<(string, float)>();

            List<char> matcherBox = str.Distinct().ToList();
            float strLen = str.Length;

            int count;

            foreach (char matcher in matcherBox)
            {
                count = 0;

                foreach (char c in str)
                {
                    if (matcher == c)
                    {
                        count++;
                    }
                }

                pairs[matcher.ToString()] = count;
            }

            foreach (KeyValuePair<string, int> keyValuePair in pairs)
            {
                probabilities[keyValuePair.Key] = keyValuePair.Value / strLen;
            }

            listWithProbabilities = probabilities.Select(pair => (pair.Key, pair.Value)).ToList();

            DoShannonFano(listWithProbabilities);
            DoHuffman(listWithProbabilities);
        }

        public static void EncdoeSequTask2(string str, List<string> wordsToEncode)
        {
            // Словарь для хранения частоты встречаемости слов
            Dictionary<string, int> pairs = new Dictionary<string, int>();
            Dictionary<string, float> probabilities = new Dictionary<string, float>();
            List<(string, float)> listWithProbabilities = new List<(string, float)>();

            // Рассчитываем вероятность для каждого из слов из wordsToEncode
            float strLen = str.Length;

            // Подсчитываем количество каждого слова из списка в строке
            foreach (string word in wordsToEncode)
            {
                int count = 0;

                for (int i = 0; i <= str.Length - word.Length; i++)
                {
                    if (str.Substring(i, word.Length) == word)
                    {
                        count++;
                    }
                }

                if (count > 0)
                {
                    pairs[word] = count;
                }
            }

            // Рассчитываем вероятности
            foreach (KeyValuePair<string, int> keyValuePair in pairs)
            {
                probabilities[keyValuePair.Key] = (float)keyValuePair.Value / strLen;
            }

            // Заполняем список вероятностей
            listWithProbabilities = probabilities.Select(pair => (pair.Key, pair.Value)).ToList();

            // Вызываем методы кодирования (Shannon-Fano и Huffman)
            DoShannonFano(listWithProbabilities);
            DoHuffman(listWithProbabilities);
        }

        static public void PrintTable(Dictionary<string, string> dic, string name)
        {
            // Вывод всех последовательностей и их кодов в консоль
            Console.WriteLine($"\n{name} codes");
            Console.WriteLine("{0,-15} {1,-15} {2,-15}", "Sequence", "Probability", "Code");
            Console.WriteLine(new string('-', 45));  // Для разделителя

            foreach (KeyValuePair<string, string> pair in dic)
            {
                foreach (var item in sequencesWithProbabilities)
                {
                    if (pair.Key == item.sequence)
                    {
                        Console.WriteLine("{0,-15} {1,-15:F4} {2,-15}", pair.Key, item.probability, pair.Value);
                    }
                }
            }
        }

        static public void FindInDocumentTask3(string path)
        {
            string text = null;

            using (WordprocessingDocument doc = WordprocessingDocument.Open(path, false)) // temperary block that will be cleared in memory
            {
                // Получаем основной текстовый документ (main part in doc)
                var body = doc.MainDocumentPart.Document.Body;
                
                // Проходим по всем абзацам и выводим текст
                foreach (var paragraph in body.Elements<Paragraph>()) // body.Elements<Paragraph> return all paragraphs in our text
                {
                    if (text != null)
                        text += "\n\t" + paragraph.InnerText; // print inner text
                    else
                        text = paragraph.InnerText;
                }
            }

            List<string> wordsList = new List<string>{ "th", "tion", "ing", "ed", "re", "are", "is", "an", "the", "to" };

            EncdoeSequTask2(text, wordsList);
        }

        // Class for Haffman nodes
        class HuffmanNode
        {
            public string Sequence { get; set; }
            public float Probability { get; set; }
            public HuffmanNode Left { get; set; }
            public HuffmanNode Right { get; set; }

            public HuffmanNode(string sequence, float probability)
            {
                Sequence = sequence;
                Probability = probability;
            }
        }
    }
}


// Hello, world!
// 13 chars
// посчитать сколько встречается каждый символ
// вычислить вероятность для каждого символа
// вычислить код для каждой буквы или пробела