using System;
using System.Collections.Generic;
using System.Linq;

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

        public static void DoShannonFano()
        {
            // Создаем список пар (последовательность, вероятность) и сортируем по убыванию вероятности
            sequencesWithProbabilities =
                gettingSequences.Zip(probabilityOfSequences, (s, p) => (s, p))
                .OrderByDescending(pair => pair.Item2)
                .ToList();

            // Запускаем рекурсивную функцию кодирования
            ShannonFanoRecursive(sequencesWithProbabilities, "");

            // Вывод всех последовательностей и их кодов в консоль
            Console.WriteLine("\nAll sequences and their Shannon-Fano codes:");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine($"{"Sequence",-15}   |   {"Probability",10}   |   {"Code"}");
            Console.WriteLine("-------------------------------------------------");

            foreach (KeyValuePair<string, string> item in shannonFanoCodes)
            {
                Console.WriteLine($"{item.Key,-15}   |   {Math.Round(probabilityOfSequences[gettingSequences.IndexOf(item.Key)], 5)}   |    {item.Value}");
            }

            Console.WriteLine("-------------------------------------------------");
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

            if (totalProbability < probabilityThreshold || sequences.Count == 1)
            {
                shannonFanoCodes[sequences[0].sequence] = prefix;
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

            var leftPart = sequences.Take(splitIndex).ToList();
            var rightPart = sequences.Skip(splitIndex).ToList();

            ShannonFanoRecursive(leftPart, prefix + "0");
            ShannonFanoRecursive(rightPart, prefix + "1");
        }

        public static void DoHuffman()
        {
            // Сортируем последовательности по вероятности
            var sequencesWithProbabilitiesSorted = sequencesWithProbabilities
                .OrderByDescending(pair => pair.probability)
                .ToList();

            // Создаем дерево Хаффмана
            var huffmanTree = BuildHuffmanTree(sequencesWithProbabilitiesSorted);

            // Кодируем с использованием дерева Хаффмана
            GenerateHuffmanCodes(huffmanTree, "");

            // Вывод всех последовательностей и их кодов Хаффмана
            Console.WriteLine("\nAll sequences and their Huffman codes:");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine($"{"Sequence",-15}   |   {"Probability",10}   |   {"Code"}");
            Console.WriteLine("-------------------------------------------------");

            foreach (KeyValuePair<string, string> item in huffmanCodes)
            {
                Console.WriteLine($"{item.Key,-15}   |   {Math.Round(probabilityOfSequences[gettingSequences.IndexOf(item.Key)], 5)}   |    {item.Value}");
            }

            Console.WriteLine("-------------------------------------------------");
        }

        private static HuffmanNode BuildHuffmanTree(List<(string sequence, float probability)> sequences)
        {
            var nodes = sequences.Select(pair => new HuffmanNode(pair.sequence, pair.probability)).ToList();

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

        private static void GenerateHuffmanCodes(HuffmanNode node, string prefix)
        {
            if (node.Left == null && node.Right == null)
            {
                huffmanCodes[node.Sequence] = prefix;
                return;
            }

            if (node.Left != null)
                GenerateHuffmanCodes(node.Left, prefix + "0");

            if (node.Right != null)
                GenerateHuffmanCodes(node.Right, prefix + "1");
        }

        public static void CreateCombinations()
        {
            // Ввод алфавита с проверкой
            while (true)
            {
                Console.Write("Enter alphabet for your words: ");
                charList = Console.ReadLine().Distinct().ToList();  // Считываем алфавит

                n = charList.Count;

                if (n == 0)
                {
                    Console.WriteLine("Alphabet cannot be empty! Please enter at least one character.");
                }
                else
                {
                    break;
                }
            }

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

        // Класс для узлов дерева Хаффмана
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
