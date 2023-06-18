using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUAI;



namespace DTAI
{

    internal class Program
    {
        #region region 修饰部分
        private static String OrdinalSuffex(Int32 Cardinal)
        {
            if (Cardinal % 100 == 11) return "th";
            if (Cardinal % 100 == 12) return "th";
            if (Cardinal % 10 == 1) return "st";
            if (Cardinal % 10 == 2) return "nd";
            if (Cardinal % 10 == 3) return "rd";
            return String.Empty;
        }
        private static void PrintLine() => Console.WriteLine(String.Empty.PadLeft(64, '-'));
        private static void PrintParams(String[] Params)
        {
            PrintLine();
            switch (Params.Length)
            {
                case 0: Console.WriteLine($"There is not paramater inputed."); break;
                case 1: Console.WriteLine($"There is 1 paramter inputed: {Params[1]}"); break;
                default:
                    Console.WriteLine($"There are {Params.Length} paramaters inputed:");
                    Int32 ParamsLengthBit = Params.Length.ToString().Length;
                    for (Int32 i = 0; i < Params.Length; i++)
                        Console.WriteLine($"The {i.ToString().PadLeft(ParamsLengthBit, '0')}" +
                            $"{OrdinalSuffex(i)} paramater is: {Params[i]}"); break;
            }
            PrintLine();
        }
        #endregion 

        internal static void Main(String[] Params)
        {
            Console.InputEncoding = Encoding.UTF8; Console.OutputEncoding = Encoding.UTF8;
            PrintParams(Params);




            






            Console.ReadKey();
        }
    }
}
