﻿using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Zxw.Framework.NetCore.Extensions
{
    public static class StringExtensions
    {
        public static object ParseTo(this string str, string type)
        {
            switch (type)
            {
                case "System.Boolean":
                    return ToBoolean(str);
                case "System.SByte":
                    return ToSByte(str);
                case "System.Byte":
                    return ToByte(str);
                case "System.UInt16":
                    return ToUInt16(str);
                case "System.Int16":
                    return ToInt16(str);
                case "System.uInt32":
                    return ToUInt32(str);
                case "System.Int32":
                    return str.ToInt32();
                case "System.UInt64":
                    return ToUInt64(str);
                case "System.Int64":
                    return ToInt64(str);
                case "System.Single":
                    return ToSingle(str);
                case "System.Double":
                    return ToDouble(str);
                case "System.Decimal":
                    return ToDecimal(str);
                case "System.DateTime":
                    return ToDateTime(str);
                case "System.Guid":
                    return ToGuid(str);
            }
            throw new NotSupportedException(string.Format("The string of \"{0}\" can not be parsed to {1}", str, type));
        }

        public static sbyte? ToSByte(this string value)
        {
            sbyte value2;
            if (sbyte.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static byte? ToByte(this string value)
        {
            byte value2;
            if (byte.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static ushort? ToUInt16(this string value)
        {
            ushort value2;
            if (ushort.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static short? ToInt16(this string value)
        {
            short value2;
            if (short.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static uint? ToUInt32(this string value)
        {
            uint value2;
            if (uint.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static ulong? ToUInt64(this string value)
        {
            ulong value2;
            if (ulong.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static long? ToInt64(this string value)
        {
            long value2;
            if (long.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static float? ToSingle(this string value)
        {
            float value2;
            if (float.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static double? ToDouble(this string value)
        {
            double value2;
            if (double.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static decimal? ToDecimal(this string value)
        {
            decimal value2;
            if (decimal.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static bool? ToBoolean(this string value)
        {
            bool value2;
            if (bool.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static T? ToEnum<T>(this string str) where T : struct
        {
            T t;
            if (Enum.TryParse(str, true, out t) && Enum.IsDefined(typeof(T), t))
            {
                return t;
            }
            return null;
        }

        public static Guid? ToGuid(this string str)
        {
            Guid value;
            if (Guid.TryParse(str, out value))
            {
                return value;
            }
            return null;
        }

        public static DateTime? ToDateTime(this string value)
        {
            DateTime value2;
            if (DateTime.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static int? ToInt32(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }
            int value;
            if (int.TryParse(input, out value))
            {
                return value;
            }
            return null;
        }

        /// <summary>
        ///     替换空格字符
        /// </summary>
        /// <param name="input"></param>
        /// <param name="replacement">替换为该字符</param>
        /// <returns>替换后的字符串</returns>
        public static string ReplaceWhitespace(this string input, string replacement = "")
        {
            return string.IsNullOrEmpty(input) ? null : Regex.Replace(input, "\\s", replacement, RegexOptions.Compiled);
        }

        /// <summary>
        ///     返回一个值，该值指示指定的 String 对象是否出现在此字符串中。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value">要搜寻的字符串。</param>
        /// <param name="comparisonType">指定搜索规则的枚举值之一。</param>
        /// <returns>如果 value 参数出现在此字符串中则为 true；否则为 false。</returns>
        public static bool Contains(this string source, string value,
            StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            return source.IndexOf(value, comparisonType) >= 0;
        }

        /// <summary>
        ///     清除 Html 代码，并返回指定长度的文本。(连续空行或空格会被替换为一个)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="maxLength">返回的文本长度（为0返回所有文本）</param>
        /// <returns></returns>
        public static string StripHtml(this string text, int maxLength = 0)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            text = text.Trim();

            text = Regex.Replace(text, "[\\r\\n]{2,}", "<&rn>"); //替换回车和换行为<&rn>，防止下一行代码替换空格的时候被替换掉
            text = Regex.Replace(text, "[\\s]{2,}", " "); //替换 2 个以上的空格为 1 个
            text = Regex.Replace(text, "(<&rn>)+", "\n"); //还原 <&rn> 为 \n
            text = Regex.Replace(text, "(\\s*&[n|N][b|B][s|S][p|P];\\s*)+", " "); //&nbsp;

            text = Regex.Replace(text, "(<[b|B][r|R]/*>)+|(<[p|P](.|\\n)*?>)", "\n"); //<br>
            text = Regex.Replace(text, "<(.|\n)+?>", " ", RegexOptions.IgnoreCase); //any other tags

            if (maxLength > 0 && text.Length > maxLength)
                text = text.Substring(0, maxLength);

            return text;
        }

        public static string ToPascalCase(this string original)
        {
            Regex invalidCharsRgx = new Regex("[^_a-zA-Z0-9]");
            Regex whiteSpace = new Regex(@"(?<=\s)");
            Regex startsWithLowerCaseChar = new Regex("^[a-z]");
            Regex firstCharFollowedByUpperCasesOnly = new Regex("(?<=[A-Z])[A-Z0-9]+$");
            Regex lowerCaseNextToNumber = new Regex("(?<=[0-9])[a-z]");
            Regex upperCaseInside = new Regex("(?<=[A-Z])[A-Z]+?((?=[A-Z][a-z])|(?=[0-9]))");

            // replace white spaces with undescore, then replace all invalid chars with empty string
            var pascalCase = invalidCharsRgx.Replace(whiteSpace.Replace(original, "_"), string.Empty)
                // split by underscores
                .Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                // set first letter to uppercase
                .Select(w => startsWithLowerCaseChar.Replace(w, m => m.Value.ToUpper()))
                // replace second and all following upper case letters to lower if there is no next lower (ABC -> Abc)
                .Select(w => firstCharFollowedByUpperCasesOnly.Replace(w, m => m.Value.ToLower()))
                // set upper case the first lower case following a number (Ab9cd -> Ab9Cd)
                .Select(w => lowerCaseNextToNumber.Replace(w, m => m.Value.ToUpper()))
                // lower second and next upper case letters except the last if it follows by any lower (ABcDEf -> AbcDef)
                .Select(w => upperCaseInside.Replace(w, m => m.Value.ToLower()));

            return string.Concat(pascalCase);
        }
        public static bool IsNullOrWhiteSpace(this string source)
        {
            return string.IsNullOrEmpty(source) || string.IsNullOrWhiteSpace(source);
        }
        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }
    }
}
