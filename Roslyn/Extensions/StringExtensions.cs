﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Roslyn.Extensions
{
    public static class StringExtensions
    {
        public static string UpperToLower(this string str)
        {
            if (Char.IsUpper(str[0]) == true) { str = str.Replace(str[0], char.ToLower(str[0])); return str; }

            return str;
        }
    }
}
