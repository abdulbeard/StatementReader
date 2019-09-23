using System;
using System.Collections.Generic;

namespace StatementReader.Utils
{
    public class Parsing
    {
        public static DateTime ParseDate(string date)
        {
            return DateTime.Parse(date);
        }

        public static decimal ParseAmount(string amount)
        {
            return decimal.Parse(amount);
        }

        public static string ParseDescription(IEnumerable<string> description)
        {
            return string.Join(" ", description);
        }
    }
}
