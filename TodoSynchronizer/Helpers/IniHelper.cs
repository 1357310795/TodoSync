using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Helpers
{
    public static class IniHelper
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int WritePrivateProfileString(string section, string val, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int WritePrivateProfileSection(string section, string val, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetPrivateProfileString(string lpApplicationName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        public static string GetKeyValue(string Section, string Key, string DefaultText, string iniFilePath)
        {
            int BufferSize = 9999;
            StringBuilder keyValue = new StringBuilder(BufferSize);
            string text = "";
            int Rvalue = GetPrivateProfileString(Section, Key, text, keyValue, BufferSize, iniFilePath);

            bool flag = Rvalue == 0;
            if (flag)
            {
                return DefaultText;
            }
            else
            {
                return keyValue.ToString();
            }
        }

        public static bool SetKeyValue(string Section, string Key, string Value, string iniFilePath)
        {
            string pat = Path.GetDirectoryName(iniFilePath);
            bool flag = !Directory.Exists(pat);
            if (flag)
            {
                Directory.CreateDirectory(pat);
            }
            bool flag2 = !File.Exists(iniFilePath);
            if (flag2)
            {
                File.Create(iniFilePath).Close();
            }
            int OpStation = IniHelper.WritePrivateProfileString(Section, Key, Value, iniFilePath);
            bool flag3 = OpStation == 0L;
            return !flag3;
        }

        public static bool DelKeyValue(string Section, string Key, string iniFilePath)
        {
            int OpStation = IniHelper.WritePrivateProfileString(Section, Key, null, iniFilePath);
            bool flag3 = OpStation == 0L;
            return !flag3;
        }

        public static bool DelSection(string Section, string iniFilePath)
        {
            int OpStation = IniHelper.WritePrivateProfileSection(Section, null, iniFilePath);
            bool flag3 = OpStation == 0L;
            return !flag3;
        }

        public static string inipath = Environment.GetEnvironmentVariable("LocalAppData") + "\\ClipboardPurifier\\Settings.ini";
    }
}
