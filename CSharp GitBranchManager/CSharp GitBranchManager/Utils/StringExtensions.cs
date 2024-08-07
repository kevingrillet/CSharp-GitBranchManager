using System;
using System.Collections.Generic;

namespace CSharp_GitBranchManager.Utils
{
    public static class StringExtensions
    {
        private static readonly char[] defaultSeparators = [',', ';'];

        public static List<string> CSVSplitAndTrim(this string input, char[] separators = null)
        {
            return [.. input.Split(separators ?? defaultSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
        }
    }
}
