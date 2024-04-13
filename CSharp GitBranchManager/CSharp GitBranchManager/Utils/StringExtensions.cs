using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharp_GitBranchManager.Utils
{
    public static class StringExtensions
    {
        private static readonly char[] defaultSeparators = [',', ';'];

        public static List<string> CSVSplitAndTrim(this string input, char[] separators = null)
        {
            separators ??= defaultSeparators;
            return input.Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }
    }
}
