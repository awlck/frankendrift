﻿using System;
using System.Text.RegularExpressions;
using Adravalon.Glue.Infragistics.Win.UltraWinToolbars;

namespace Adravalon.Glue
{
    public static class Util
    {
        public static string StripCarats(string str) => StripCarets(str);
        public static string StripCarets(string str)
        {
            if (str.Contains("<#") && str.Contains("#>"))
                str = str.Replace("<#", "[[==~~").Replace("#>", "~~==]]");
            var re = new Regex("<(.|\n)+?>");
            return re.Replace(str, "");
        }

        public static string GetSetting(string _a, string _b, string _c, string _d = "") => _d;
        public static void SaveSetting(string _a, string _b, string _c, string _val) { }
        public static void DeleteSetting(string _a, string _b, string _c) {  }

        public static void AddPrevious() { }
        public static void AddPrevious(UltraToolbarsManager _a, string _b) { }
    }

    public static class Application
    {
        static UIGlue _frontend;
        public static void SetFrontend(UIGlue app)
        {
            _frontend = app;
        }

        public static string LocalUserAppDataPath =>
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static string StartupPath => _frontend.GetExecutableLocation();
        public static string ExecutablePath => _frontend.GetExecutablePath();
        public static string ProductVersion => _frontend.GetClaimedAdriftVersion();

        public static void DoEvents() => _frontend.DoEvents();
    }
}
