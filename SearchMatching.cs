using System;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.IO;
using System.Collections.Generic;

namespace ConsoleApplication2
{

    public abstract class SearchMatching
    {
        
        public static void CountingOccurrences(String Code)
        {
            string pattern = @"(\w+\s+\w+\s*\(.*\)\s*\{)";
            
            int CountOfFunctions = 0;
            List<int> ArrOfCicle = new List<int>();
            //int[] ArrOfCicle = new int[100]; //массив для хранения значений if... для каждой функции    
            //int num = 0;
            Regex myReg = new Regex(pattern); 
            int length = 0;
            
            while (true)
            {
                if (Code.Length != 0)
                {
                    Match match = myReg.Match(Code);
           
                    CountOfFunctions++;
                    length = 0;
                    int level = 1;
                    for (int i = match.Index + match.Length; level != 0; i++)
                    {


                        if (Equals(Code[i], '{'))
                            level = level + 1;
                        if (Equals(Code[i], '}'))
                            level = level - 1;
                        length = i + 1;

                    }
                    string temp = "";

                    for (int i = match.Index + match.Length; i < length; i++)
                        temp = temp + Code[i];

                    string patternCicle = @"((if\s*\(.*\w\w*.*\))|(for\s*\(.*\)\s*)|(\s*while\s*\(.*\))|(\bcase:))";
                    Regex Reg = new Regex(patternCicle);
                     
                    MatchCollection matches = Reg.Matches(temp);
                    ArrOfCicle.Add(matches.Count);
                    //ArrOfCicle[num] = matches.Count;//сохраняем количество if, while, case, for для каждой функции 
                    //num++;
                    Code = Code.Remove(0, length); // удаляем часть,которую уже просмотрели
                }                
                else break;
            }

            int[] ArrCicleNumber = new int[ArrOfCicle.Count];

            for (int i = 0; i < ArrCicleNumber.Length; i++)
            {
                //Console.WriteLine("Количество if, while, for, case для првой функции: {0}", ArrOfCicle[i]);
                ArrCicleNumber[i] = ArrOfCicle[i] + 1; 
                Console.WriteLine("Цикломатическое число Мак-Кейба для функции {0}: {1}",i+1, ArrCicleNumber[i]);          
            }
            
            Console.WriteLine("Количество функций: {0}", CountOfFunctions);
            
            int ResultMetrix = 0;
            int SumCicleNumber = 0;
            for (int i = 0; i < ArrCicleNumber.Length; i++)
                SumCicleNumber = SumCicleNumber + ArrCicleNumber[i];
            ResultMetrix = SumCicleNumber - CountOfFunctions;
            Console.WriteLine("Число Мак-Кейба: {0}", ResultMetrix);
        }
    }
}