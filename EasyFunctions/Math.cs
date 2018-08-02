using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyFunctions
{
    public class Math
    {
        public static void SayMyName()
        {
            Console.WriteLine("My name is " + MyName());
        }

        public static string MyName()
        {
            return Environment.UserName;
        }

        public Int64 Add(params object[] nums)
        {
            Int64 result = 0;
            foreach (object v in nums)
            {
                result += Int64.Parse(v.ToString());
            }
            return result;
        }

        public DateTime AddDays(string startDate, string numDays)
        {
            return DateTime.Parse(startDate).AddDays(int.Parse(numDays));
        }
    }
}
