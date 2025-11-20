using System;
namespace CEAM.AzureSearch.Loader.Utils
{
    public static class MessageUtil
    {
        public static void Write(string message)
        {
            Console.WriteLine(message);
        }
        
        public static void WriteWithDatetime(string message, DateTime? now = null, string format = ConstantUtil.DateTimeFormat.Now)
        {
            DateTime dt;
            if (now == null) dt = DateTime.Now;
            else dt = now.Value;

            var text = string.Format(message + " {0}", dt.ToString(format));
            Console.WriteLine(text);
        }

        public static void WriteWithTimeSpan(string message, TimeSpan ts, string format = ConstantUtil.DateTimeFormat.Now)
        {
            string text = message + " " + format;
            string result = "";

            if (format == ConstantUtil.DateTimeFormat.ProcessTimeShort)
            {
                result = string.Format(text, ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            }
            else result = string.Format(text, ts);


            Console.WriteLine(result);
        }
    }
}
