using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OnTrac.Extensions.String
{
    public static class Helpers
    {
        public static bool IsNullOrWhiteSpaceEmpty(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return true;

            return string.IsNullOrWhiteSpace(value.Trim());
        }

        public static string ReplaceStringPatternValues(this string original, string valueToMatch, string valueToInsert)
        {
            Regex rx = new Regex(@"(?<=\{)[^}]*(?=\})");

            return rx.Replace(valueToMatch, valueToInsert);
        }

        public static string SafeTrim(this string value)
        {
            if (value.IsNullOrWhiteSpaceEmpty())
                return string.Empty;

            return value.Trim();
        }
    }
}
