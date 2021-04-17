using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public class Sudoku
    {
        //static IEnumerable<int> s1 = new List<int>()
        //{
        //    0,0,8,0,0,0,9,6,2,
        //    4,2,9,7,0,0,1,8,5,
        //    5,0,1,9,0,8,0,7,0,
        //    0,0,2,0,0,7,4,0,0,
        //    1,0,0,2,8,0,0,5,0,
        //    7,0,0,3,9,6,0,0,8,
        //    0,0,4,6,7,0,0,3,1,
        //    0,0,0,8,0,0,6,0,0,
        //    0,1,0,5,0,0,0,2,0
        //};

        //static IEnumerable<int> s2;


        //public static string S1
        //{
        //    get { return string.Join(",", s1.ToArray()); }
        //}

        //public static string S2
        //{
        //    get { return string.Join(",", s2.ToArray()); }
        //}

        Dictionary<int, List<int>> randomSudoku = new Dictionary<int, List<int>>();

        public static void generateFirstRow(List<int> listNumbers)
        {
            Random rand = new Random();
            listNumbers.AddRange(Enumerable.Range(1, 9)
                                   .OrderBy(i => rand.Next())
                                   .Take(9));
            //listNumbers.ForEach(x => Console.WriteLine(x));
        }

        public static List<int> shiftLeft(List<int> numbers, int shift)
        {
            List<int> temp = new List<int>();
            for (int g = shift; g < numbers.Count; g++)
            {
                temp.Add(numbers.ElementAt(g));
            }
            for (int h = 0; h < shift; h++)
            {
                temp.Add(numbers.ElementAt(h));
            }
            return temp;
        }

    }
}
