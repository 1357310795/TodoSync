using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Core.Extensions
{
    public static class HtmlExtension
    {
        /// <summary>
        /// Html to Esc
        /// </summary>
        /// <param name="input">input</param>
        /// <returns></returns>
        public static string HtmlToEsc(this string input)
        {
            if (string.IsNullOrEmpty(input)) { return ""; }

            input = input.Replace("&", "&amp;")
                        .Replace("'", "&#39;")
                        .Replace("\"", "&quot;")
                        .Replace("<", "&lt;")
                        .Replace(">", "&gt;")
                        .Replace(" ", "&nbsp;")
                        .Replace("©", "&copy;")
                        .Replace("®", "&reg;")
                        .Replace("™", "&#8482;");
            return input;
        }

        /// <summary>
        /// Esc to Html
        /// </summary>
        /// <param name="input">input</param>
        /// <returns></returns>
        public static string EscToHtml(this string input)
        {
            if (string.IsNullOrEmpty(input)) { return ""; }

            input = input.Replace("&#8482;", "™")
                        .Replace("&reg;", "®")
                        .Replace("&copy;", "©")
                        .Replace("&nbsp;", " ")
                        .Replace("&gt;", ">")
                        .Replace("&lt;", "<")
                        .Replace("&quot;", "\"")
                        .Replace("&#39;", "'")
                        .Replace("&amp;", "&");
            return input;
        }
    }
}
