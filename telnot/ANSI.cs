using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace telnot
{
    static class ANSI
    {
        public static string RemoveCursorPosition(string text) =>
            Regex.Replace(text, @"\u001b\[\d+;\d+f", "");
    }
}
