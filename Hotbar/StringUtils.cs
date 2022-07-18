using System;

namespace Hotbar
{
    public static class StringUtils
    {
        public static String ToUpperFirstLowerLast(this String str)
        {
            return str.Substring(0, 1).ToUpper() + str.Substring(1, str.Length - 1).ToLower();
        }
    }
}
