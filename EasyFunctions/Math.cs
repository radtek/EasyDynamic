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

        public DateTime AddDays(object startDate, object numDays)
        {
            DateTime originalDate = DateTime.Today;
            if (startDate.GetType() == typeof(String)) originalDate = DateTime.Parse(startDate.ToString());
            if (startDate.GetType() == typeof(DateTime)) originalDate = (DateTime)startDate;
            int days = int.MinValue;
            if (numDays.GetType() == typeof(String)) days = int.Parse(numDays.ToString());
            if (numDays.GetType() == typeof(int)) days = (int)numDays;
            return originalDate.AddDays(days);
        }
    }
}
