using System;
using System.Linq;

namespace Phuntasia
{
    public static class EnumUtility
    {
        public static T[] GetValues<T>()
            where T : struct, Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        public static int Min<T>()
            where T : struct, Enum
        {
            return Enum.GetValues(typeof(T)).Cast<int>().Min();
        }

        public static int Min(Type enumType)
        {
            return Enum.GetValues(enumType).Cast<int>().Min();
        }

        public static int Max<T>()
            where T : struct, Enum
        {
            return Enum.GetValues(typeof(T)).Cast<int>().Max();
        }

        public static int Max(Type enumType)
        {
            return Enum.GetValues(enumType).Cast<int>().Max();
        }

        public static T ToEnum<T>(this string enumString)
            where T : struct, Enum
        {
            return (T)Enum.Parse(typeof(T), enumString, true);
        }

        public static bool TryToEnum<T>(this string enumString, out T value)
            where T : struct, Enum
        {
            return Enum.TryParse(enumString, true, out value);
        }

        public static bool IsEnum<T>(this string enumString)
            where T : struct, Enum
        {
            return Enum.TryParse<T>(enumString, true, out _);
        }

        public static T ToEnum<T>(this int value)
            where T : struct, Enum
        {
            return (T)Convert.ChangeType(value, Enum.GetUnderlyingType(typeof(T)));
        }

        public static string ToName(this Enum value)
        {
            return value.ToString().Beautify();
        }
    }
}