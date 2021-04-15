using System;

namespace Phuntasia
{
    public static class CoreUtility
    {
        public static bool GetCommandLineBool(string argName, bool defaultValue)
        {
            var args = Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == argName)
                {
                    return true;
                }
            }

            return defaultValue;
        }

        public static int GetCommandLineInt(string argName, int defaultValue)
        {
            var str = GetCommandLineArg(argName, null);

            return str == null ? defaultValue : int.Parse(str);
        }

        public static string GetCommandLineArg(string argName, string defaultValue)
        {
            var args = Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == argName && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }

            return defaultValue;
        }
    }
}