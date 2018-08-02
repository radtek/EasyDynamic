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
            foreach (KeyValuePair<string, TypeClassMapping> kvPair in EasyObject.Methods)
            {
                Console.WriteLine("Method Name ==> " + kvPair.Key);
            }
            while (true) {
                Console.Write("Please Enter the Command ===>");
                string command = Console.ReadLine();

                if (String.IsNullOrWhiteSpace(command)) break;
                string execCommand = EasyObject.ExecuteString(command).ToString();
                Console.WriteLine("Output of {0} ==> {1} is <<{2}>>", command, execCommand, EasyObject.EvaluateString(execCommand));
            }
            //edo.LoadCustomFunctions();
            //edo.MyName = "Arasu Elango";
            //Console.WriteLine("My name is " + edo.MyName);
            //edo.MyName = "Elango, Arasu";
            //Console.WriteLine("Changed name is " + edo.MyName);
            //edo.Math_SayMyName();
            //Console.WriteLine("1+2+3+4+5 = " + EasyObject.ExecuteMethod("Add", new int[] { 1,2,3,4,5}));
            //Console.WriteLine("1+2+3+4 = " +  edo.Add(new int[] { 1, 2, 3, 4}));
            //Console.WriteLine("Called Method " + edo.GetMyAddress("Arasu", "Elango", "Pipers", "Glen"));
        }
    }
}
