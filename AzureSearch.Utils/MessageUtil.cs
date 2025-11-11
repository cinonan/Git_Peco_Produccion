using System;

namespace AzureSearch.Utils
{
    public static class MessageUtil
    {
        public static void Write(bool overwrite = false, params string[] message)
        {
            Console.WriteLine("    " + string.Join(" ", message));
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
                result = string.Format(text, 
                    ts.Days.ToString().PadLeft(2,'0'), 
                    ts.Hours.ToString().PadLeft(2, '0'), 
                    ts.Minutes.ToString().PadLeft(2, '0'), 
                    ts.Seconds.ToString().PadLeft(2, '0'), 
                    ts.Milliseconds.ToString().PadLeft(7, '0'));
            }
            else result = string.Format(text, ts);


            Console.WriteLine(result);
        }
    }
}
