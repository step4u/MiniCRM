using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Com.Huen.Libs
{
    public class Ini
    {
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(
            String section, String key, String def, StringBuilder retVal, int Size, String filePat);

        [DllImport("Kernel32.dll")]
        private static extern long WritePrivateProfileString(String Section, String Key, String val, String filePath);

        private string _avsPath = string.Empty;
        public Ini(string iniPath)
        {
            _avsPath = iniPath;
        }

        public void IniWriteValue(String Section, String Key, String Value)
        {
            WritePrivateProfileString(Section, Key, Value, _avsPath);
        }

        public String IniReadValue(String Section, String Key)
        {
            StringBuilder temp = new StringBuilder(2000);
            int i = GetPrivateProfileString(Section, Key, "", temp, 2000, _avsPath);
            return temp.ToString();
        }
    }
}
