using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Core.Extensions
{
    public static class EmojiExtension
    {
        public static string CleanEmoji(this string str)
        {
            return Clean2(Clean1(str));
        }

        public static string Clean1(string str)
        {
            foreach (var a in str)
            {
                byte[] bts = Encoding.UTF32.GetBytes(a.ToString());

                if (bts[0].ToString() == "253" && bts[1].ToString() == "255")
                {
                    str = str.Replace(a.ToString(), "");
                }

            }
            return str.Trim();
        }

        public static string Clean2(string str)
        {
            StringBuilder sb = new StringBuilder(str.Length);
            foreach (var a in str)
            {
                if (char.GetUnicodeCategory(a) == System.Globalization.UnicodeCategory.OtherSymbol || char.GetUnicodeCategory(a) == System.Globalization.UnicodeCategory.Surrogate)
                {

                }
                else
                   sb.Append(a);

            }
            return sb.ToString().Trim();
        }
    }
}
