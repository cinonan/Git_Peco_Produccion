using System;
using System.Collections.Generic;
using System.Text;

namespace CEAM.AzureSearch.Loader.Utils
{
    public class ConstantUtil
    {
        public class DateTimeFormat
        {
            public const string Now = @"dd/MM/yyyy hh:mm:ss";
            public const string ProcessTimeShort = @"dd\:hh\:mm\:ss\.fffffff";
            public const string ProcessTimeLong = @"{0} Days, {1} Hours, {2} Minutes, {3} Seconds and {4} Milliseconds";
        }
    }
}
