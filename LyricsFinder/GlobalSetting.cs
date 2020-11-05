using System;

namespace LyricsFinder
{
    public static class GlobalSetting
    {
        public static bool DebugMode { get; set; } = false;

        public static int SearchAndDownloadTimeout { get; set; } = 2000;

        public static bool StrictMatch { get; set; } = true;

        public static uint DurationThresholdValue { get; set; } = 1000;

        public static Action<string> OutputFunc { get; set; }

        public static Func<string,string,int> CustomCalculateStringDistanceFunc { get; set; }
    }
}