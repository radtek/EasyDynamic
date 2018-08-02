using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyDynamicObject;
using System.Text.RegularExpressions;

namespace TestEasyDynamicObject
{
    class Program
    {
        static void Main(string[] args)
        {
            dynamic edo = new EasyObject();
            EasyObject.LoadCustomFunctions();

            Console.WriteLine("You can input command to be executed... examples :");
            Console.WriteLine("\t\"Today is \" + DateTime.Today.ToString(\"MMM dd, yyyy\")");
            Console.WriteLine("\t\"Tomorrow is \" + DateTime.Today.AddDays(1).ToString(\"MMM dd, yyyy\")");
            Console.WriteLine("\t\"2 days from now is \" + DateTime.Today.AddDays(@Add(1,1)).ToString(\"MMM dd, yyyy\")");
            Console.WriteLine("\t\"Next week same day is \" + DateTime.Today.AddDays(@Add(1,2*3)).ToString(\"MMM dd, yyyy\")");

            Console.WriteLine();
            Console.WriteLine("@ Methods Available to use :");
            foreach (KeyValuePair<string, TypeClassMapping> kvPair in EasyObject.Methods)
            {
                Console.WriteLine("Method Name ==> " + kvPair.Key);
            }
            Console.WriteLine();


            while (true) {
                Console.Write("Please Enter the Command ===>");
                string command = Console.ReadLine();

                if (String.IsNullOrWhiteSpace(command)) break;
                string execCommand = EasyObject.ExecuteString(command).ToString();
                Console.WriteLine("Output of {0} ==> {1} is <<{2}>>", command, execCommand, EasyObject.EvaluateString(execCommand));
            }
        }
    }
}
