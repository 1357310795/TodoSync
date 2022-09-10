using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Core.Extensions
{
    public static class UrlExtension
    {
        public static string UrlEscape(this string url)
        {
            return Uri.EscapeDataString(url);
        }
        public static string UrlUnescape(this string url)
        {
            return Uri.UnescapeDataString(url);
        }
        public static string Connect(this IEnumerable<string> iterator,string separator)
        {
            return string.Join(separator, iterator);
        }

        public static string UrlEncodeByParts(this string url)
        {
            return url.Split('/')
                       .Select(s => s.UrlUnescape())
                       .Connect("/");
        }
    }
}
