namespace AzureSearch.Utils
{
    public class ConstantUtil
    {
        public class DateTimeFormat
        {
            public const string Now = @"dd/MM/yyyy hh:mm:ss";
            public const string ProcessTimeShort = @"{0} days - {1}:{2}:{3}.{4}"; //@"dd\:hh\:mm\:ss\.fffffff";
            public const string ProcessTimeLong = @"{0} Days, {1} Hours, {2} Minutes, {3} Seconds and {4} Milliseconds";

        }
    }
}
