using DataPuller.DataObj;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace PullFinanceData.Util
{
    /// <summary>
    ///     Util class contains all common util functions like Convert types, IsNull, Format.
    /// </summary>
    public static class DataTypeUtil
    {
        /// <summary>
        ///     default decimal place when format double value to string.
        /// </summary>
        public const int s_decimalPlace = 10;

        /// <summary>
        ///     very small double so we can compare with double to decide IsZero
        /// </summary>
        public const double CloseToZero = 0.0000001;

        public const int IntNullValue = int.MinValue;
        public const long LongNullValue = long.MinValue;
        public const double DoubleNullValue = double.NaN;
        public const decimal DecimalNullValue = decimal.MinValue;
        public const short ShortNullValue = short.MinValue;
        public const char CharNullValue = char.MinValue;
        public const byte ByteNullValue = byte.MinValue;
        public const float FloatNullValue = float.NaN;
        public const string StringNullValue = null;

        public const string HTTPShortDateFormat = "yyyy-MM-dd";

        public const long s_overFlowValue = 100000000000;

        public static DateTime DateTimeNullValue = DateTime.MinValue;
        public static DateTime MinSmallDateTime = new(1900, 1, 1);
        public static DateTime MaxSmallDateTime = new(2079, 6, 6);
        public static Regex s_URLPattern = new(@"^[a-zA-z]+://(\w+(-\w+)*)(\.(\w+(-\w+)*))*(\?\S*)?$");

        public static double AdjustedOverflowValue(double value)
        {
            return AdjustedOverflowValue(value, s_overFlowValue);
        }

        public static double AdjustedOverflowValue(double value, long limit)
        {
            if (IsNull(value) || value >= limit || value <= 0 - limit) //Too Big Number or too small number
                return 0;
            return value;
        }

        public static bool IsOverflow(double value)
        {
            return value >= s_overFlowValue || value <= -0 - s_overFlowValue;
        }

        public static bool IsFirstBussinessDay(DateTime currentDt)
        {
            var dt = new DateTime(currentDt.Year, currentDt.Month, 1);
            for (var i = 0; i < 5; i++)
            {
                if (dt.DayOfWeek != DayOfWeek.Saturday && dt.DayOfWeek != DayOfWeek.Sunday &&
                    !(dt.Month == 1 && dt.Day == 1))
                {
                    if (dt.Day == currentDt.Day)
                        return true;
                    return false;
                }

                dt = dt.AddDays(1);
            }

            return false;
        }

        public static string ObjectStringValueWithTrim(object obj)
        {
            if (ObjectIsNull(obj))
                return null;
            if (obj is string)
                return ((string)obj).Trim();
            if (obj is bool)
                return Format((bool)obj);
            return obj.ToString().Trim();
        }

        //  Get total seconds since 1/1/1970 00:00:00.000
        public static long GetTimestampInSec(DateTime dt)
        {
            return (long)(dt - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds;
        }

        //  Get total milliseconds since 1/1/1970 00:00:00.000
        public static long GetTimestampInMsec(DateTime dt)
        {
            return (long)(dt - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
        }

        /// <summary>
        ///     Format datetime to Epoch
        /// </summary>
        /// <param name="dateTime">Sample: 2014-01-24 11:30:05</param>
        /// <returns>Epoch seconds</returns>
        public static long DateTime2Seconds(DateTime dateTime)
        {
            var dateTimeOffset = new DateTimeOffset(dateTime.ToUniversalTime());
            return dateTimeOffset.ToUnixTimeSeconds();
        }

        public static DateTimeOffset DateTime2DateTimeOffset(DateTime dateTime)
        {
            var dateOffset = DateTimeOffset.MinValue;
            if (dateTime != DateTimeNullValue) dateOffset = new DateTimeOffset(dateTime);
            return dateOffset;
        }

        //
        //  Calculate age, giving current date and birthday.
        //  If either input parameter is invalid, return DataTypeUtil.IntNullValue.
        public static int GetAge(DateTime now, DateTime birthDay)
        {
            if (IsNull(now) || IsNull(birthDay))
                return IntNullValue;

            return now.Year - birthDay.Year
                            - (now.Month < birthDay.Month || (now.Month == birthDay.Month && now.Day < birthDay.Day)
                                ? 1
                                : 0);
        }

        public static string ArrayToString(List<string> array, string splitChar, bool removeEmpty)
        {
            var str = string.Empty;
            if (removeEmpty)
                array = array.Where(a => !IsNull(a)).ToList();

            if (array.Count > 0)
            {
                if (!IsNull(array[0]))
                    str = array[0];

                for (var i = 1; i < array.Count; i++) str += splitChar + (IsNull(array[i]) ? string.Empty : array[i]);
            }

            return IsNull(str) ? null : str;
        }

        public static int LevensteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source) && string.IsNullOrEmpty(target))
                return 100;

            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
                return 0;

            var n = source.Length;
            var m = target.Length;
            var d = new int[n + 1, m + 1];

            for (var i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (var j = 1; j <= m; d[0, j] = j++)
            {
            }

            for (var i = 1; i <= n; i++)
            for (var j = 1; j <= m; j++)
            {
                var cost = target[j - 1] == source[i - 1] ? 0 : 1;
                var min1 = d[i - 1, j] + 1;
                var min2 = d[i, j - 1] + 1;
                var min3 = d[i - 1, j - 1] + cost;
                d[i, j] = Math.Min(Math.Min(min1, min2), min3);
            }

            return d[n, m] * 200 / (n + m);
        }

        public static bool IsNumeric(object value)
        {
            if (value is sbyte) return true;
            if (value is byte) return true;
            if (value is short) return true;
            if (value is ushort) return true;
            if (value is int) return true;
            if (value is uint) return true;
            if (value is long) return true;
            if (value is ulong) return true;
            if (value is float) return true;
            if (value is double) return true;
            if (value is decimal) return true;
            return false;
        }

        //split[2012-1-1,2012-1-10] to [2012-1-1,2012-1-4][2012-1-5,2012-1-8][2012-1-9,2012-1-10]
        public static Dictionary<DateTime, DateTime> SplitDateRange(DateTime dtStart, DateTime dtEnd, int days)
        {
            var list = new Dictionary<DateTime, DateTime>();
            for (var dt = dtStart; dt <= dtEnd;)
            {
                var tmp = dt.AddDays(days - 1);
                if (tmp > dtEnd) tmp = dtEnd;
                list.Add(dt, tmp);
                dt = tmp.AddDays(1);
            }

            return list;
        }

        public static T DeserializeObject<T>(string[] fieldNames, object[] values)
        {
            var entity = (T)Activator.CreateInstance(typeof(T));

            var properties = typeof(T).GetProperties();
            for (var i = 0; i < fieldNames.Length; i++)
                if (!ObjectIsNull(values[i]))
                {
                    var prop = properties.FirstOrDefault(x => x.Name.Equals(fieldNames[i]
                        , StringComparison.CurrentCultureIgnoreCase));

                    if (prop != null)
                    {
                        var propType = prop.PropertyType;
                        if (Nullable.GetUnderlyingType(prop.PropertyType) != null)
                            propType = Nullable.GetUnderlyingType(prop.PropertyType);

                        var propValue = values[i];
                        if (!Equals(propValue, DBNull.Value))
                        {
                            if (propType.Name.ToLower().Contains("int")) propValue = ObjectIntValue(values[i]);
                            if (propType.Name.ToLower().Contains("guid")) propValue = ObjectGuidValue(propValue);
                            if (propType.IsEnum)
                            {
                                var safeValue = Enum.Parse(propType, propValue.ToStr());
                                prop.SetValue(entity, safeValue);
                            }
                            else
                            {
                                var safeValue = Convert.ChangeType(propValue, propType);
                                prop.SetValue(entity, safeValue);
                            }
                        }
                    }
                }

            return entity;
        }


        #region Define IsZero,IsSame,Round,IsGuid and Compare functions

        public static bool IsZero(double d)
        {
            return d < CloseToZero && d > 0 - CloseToZero;
        }

        public static bool IsQuantityZero(double d)
        {
            return d < 0.0005 && d > -0.0005;
        }

        public static bool IsQuantityZero(double d, int calcDecimal)
        {
            return Math.Abs(d) < 0.5 * Math.Pow(0.1, calcDecimal);
        }

        public static bool IsAmountZero(double d)
        {
            return d < 0.005 && d > -0.005;
        }

        public static bool IsSame(double d1, double d2)
        {
            var b1 = IsNull(d1);
            var b2 = IsNull(d2);
            if (b1 && b2)
                return true;
            if (b1 || b2)
                return false;
            return IsZero(d1 - d2);
        }

        public static bool IsSame(int i1, int i2)
        {
            var b1 = IsNull(i1);
            var b2 = IsNull(i2);
            if (b1 && b2)
                return true;
            if (b1 || b2)
                return false;
            return i1 == i2;
        }

        public static int Round(double d)
        {
            if (IsNull(d))
                return IntNullValue;
            return (int)Math.Round(d, 0);
        }

        public static double Round(double d, int decimalplace)
        {
            decimal dec;
            try
            {
                dec = Convert.ToDecimal(d);
            }
            catch (OverflowException)
            {
                return Math.Round(d, decimalplace);
            }

            dec = Math.Round(dec, decimalplace);
            return Convert.ToDouble(dec);
        }

        /// <summary>
        ///     if d is close to an int, return int value. else return IntNullValue.
        /// </summary>
        public static int ConvertToInt(double d)
        {
            if (IsNull(d))
                return IntNullValue;
            if (IsQuantityZero(d - (int)d))
                return (int)d;
            if (IsQuantityZero((int)d + 1 - d))
                return (int)d + 1;
            return IntNullValue;
        }

        public static int[] GetGuessSplitRate(double ratio)
        {
            for (var i = 1; i < 201; i++)
            {
                var iV = ConvertToInt(ratio * i);
                if (!IsNull(iV))
                    return new[] { i, iV };
            }

            for (var i = 100000000; i > 1; i = i / 10)
                if (i * ratio < int.MaxValue)
                    return new[] { i, Round(ratio * i) };
            return new[] { 1, Round(ratio) };
        }

        public static bool IsGuid(string id)
        {
            var val = ObjectGuidValue(id);
            return val != Guid.Empty;
        }

        public static bool IsURL(string url)
        {
            if (IsNull(url))
                return false;

            return s_URLPattern.IsMatch(url);
        }

        public static string RemoveZeroTerminator(string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                var index = content.IndexOf('\0');
                if (index > -1) return content.Substring(0, index);
            }

            return content;
        }

        public static int Compare(int x, int y)
        {
            if (IsNull(x))
            {
                if (IsNull(y))
                    return 0;
                return 2;
            }

            if (IsNull(y))
                return -2;
            if (x > y)
                return 1;
            if (x < y)
                return -1;
            return 0;
        }

        public static int Compare(double x, double y)
        {
            if (IsNull(x))
            {
                if (IsNull(y))
                    return 0;
                return 2;
            }

            if (IsNull(y))
                return -2;
            if (x > y)
                return 1;
            if (x < y)
                return -1;
            return 0;
        }

        public static int Compare(string x, string y)
        {
            if (IsNull(x))
            {
                if (IsNull(y))
                    return 0;
                return 2;
            }

            if (IsNull(y))
                return -2;
            var res = string.Compare(x, y, true);
            if (res > 0)
                return 1;
            if (res < 0)
                return -1;
            return 0;
        }

        public static int Compare(DateTime x, DateTime y)
        {
            if (IsNull(x))
            {
                if (IsNull(y))
                    return 0;
                return 2;
            }

            if (IsNull(y))
                return -2;
            return CompareDay(x, y);
        }

        /// <summary>
        ///     Compare the DateTime by year, month, day and ignore time
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int CompareDay(DateTime x, DateTime y)
        {
            var index1 = x.Year * 10000 + x.Month * 100 + x.Day;
            var index2 = y.Year * 10000 + y.Month * 100 + y.Day;
            if (index1 > index2)
                return 1;
            if (index1 < index2)
                return -1;
            return 0;
        }

        public static bool LessThan(double x, double y)
        {
            if (x < y && !IsSame(x, y))
                return true;
            return false;
        }

        public static DateTime SubStractMonthEnd(DateTime endDt, int monthes)
        {
            var index = endDt.Year * 12 + endDt.Month - monthes - 1;
            var year = index / 12;
            var month = index % 12 + 1;
            return new DateTime(year, month, DateTime.DaysInMonth(year, month));
        }

        public static bool IsMonthEnd(DateTime dt)
        {
            return dt.Day == DateTime.DaysInMonth(dt.Year, dt.Month);
        }

        public static bool IsSameDay(int day, DateTime dt)
        {
            // correct day or Feb last day if init day is more than 28
            return day == dt.Day ||
                   (day > 28 && dt.Month == 2 && IsMonthEnd(dt));
        }

        public static bool IsSame(DateTime dt1, DateTime dt2)
        {
            return dt1.Year == dt2.Year && dt1.Month == dt2.Month && dt1.Day == dt2.Day;
        }

        public static bool CheckSameDay(DateTime dt1, DateTime dt2)
        {
            var day1 = DateTime.DaysInMonth(dt1.Year, dt1.Month);
            var day2 = DateTime.DaysInMonth(dt2.Year, dt2.Month);
            if (day1 == dt1.Day) //if begin date is month end
                return day2 == dt2.Day;
            if (dt1.Day > day2) //if next date is Feb
                return day2 == dt2.Day;
            return dt1.Day == dt2.Day;
        }

        public static DateTime FromHttpShortDateString(string date)
        {
            DateTime dt;
            if (string.IsNullOrEmpty(date) ||
                !DateTime.TryParseExact(date, HTTPShortDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out dt))
                dt = DateTime.MinValue;
            return dt;
        }

        public static string GetModifyFlagDesc(int flag)
        {
            switch (flag)
            {
                case 0:
                    return "No Change";
                case 1:
                    return "Insert";
                case 2:
                    return "Update";
                case 3:
                    return "Delete";
            }

            return null;
        }

        public static DateTime GetLastMonthEndDate(DateTime dt)
        {
            if (dt.Month == 1)
                return new DateTime(dt.Year - 1, 12, 31);
            return new DateTime(dt.Year, dt.Month - 1, DateTime.DaysInMonth(dt.Year, dt.Month - 1));
        }

        public static DateTime GetMonthEndDate(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month));
        }

        public static DateTime GetLastQuarterEndDate(DateTime dt)
        {
            switch (dt.Month)
            {
                case 1:
                case 2:
                case 3:
                    return new DateTime(dt.Year - 1, 12, 31);
                case 4:
                case 5:
                case 6:
                    return new DateTime(dt.Year, 3, 31);
                case 7:
                case 8:
                case 9:
                    return new DateTime(dt.Year, 6, 30);
                case 10:
                case 11:
                case 12:
                    return new DateTime(dt.Year, 9, 30);
            }

            return DateTimeNullValue;
        }

        public static DateTime GetQuarterEndDate(DateTime dt)
        {
            switch (dt.Month)
            {
                case 1:
                case 2:
                case 3:
                    return new DateTime(dt.Year, 3, 31);
                case 4:
                case 5:
                case 6:
                    return new DateTime(dt.Year, 6, 30);
                case 7:
                case 8:
                case 9:
                    return new DateTime(dt.Year, 9, 30);
                case 10:
                case 11:
                case 12:
                    return new DateTime(dt.Year, 12, 31);
            }

            return DateTimeNullValue;
        }

        public static DateTime GetLastWorkDay(DateTime dt)
        {
            var lastWorkDay = DateTimeNullValue;
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                case DayOfWeek.Tuesday:
                case DayOfWeek.Wednesday:
                case DayOfWeek.Thursday:
                case DayOfWeek.Friday:
                    lastWorkDay = dt.AddDays(-1);
                    break;
                case DayOfWeek.Sunday:
                    lastWorkDay = dt.AddDays(-2);
                    break;
                case DayOfWeek.Monday:
                    lastWorkDay = dt.AddDays(-3);
                    break;
            }

            return lastWorkDay;
        }

        public static double[] GetDoubleNullArray(int size)
        {
            var dArr = new double[size];
            for (var i = 0; i < size; i++)
                dArr[i] = DoubleNullValue;
            return dArr;
        }

        #endregion

        #region Convert object to data types

        public static string ObjectStringValue(object obj)
        {
            if (ObjectIsNull(obj))
                return null;
            if (obj is string)
                return (string)obj;
            if (obj is bool)
                return Format((bool)obj);
            return obj.ToString();
        }

        public static Guid ObjectGuidValue(object obj)
        {
            return ObjectGuidValue(obj, Guid.Empty);
        }

        public static Guid ObjectGuidValue(object obj, Guid defaultValue)
        {
            if (ObjectIsNull(obj))
                return defaultValue;
            //if (obj is Guid)
            //    return (Guid)obj;
            try
            {
                var s = obj.ToString();
                if (s.Length == 0)
                    return defaultValue;
                if (s.Length > 36 && (s[36] == ';' || s[36] == ',' || s[36] == '|' || s[36] == ':'))
                    s = s.Substring(0, 36);
                if (s.Length <= 10)
                    return defaultValue;

                Guid ret;
                if (Guid.TryParse(s, out ret))
                    return ret;

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static bool ObjectBoolValue(object obj)
        {
            return ObjectBoolValue(obj, false);
        }

        public static bool? ObjectBoolOptionalValue(object obj)
        {
            if (!ObjectIsNull(obj)) return ObjectBoolValue(obj, false);
            return null;
        }

        public static bool ObjectBoolValue(object obj, bool defaultBool)
        {
            if (!ObjectIsNull(obj))
            {
                if (obj is bool)
                    return (bool)obj;
                switch (obj.ToString().ToLower())
                {
                    case "true":
                    case "t":
                    case "yes":
                    case "y":
                    case "1":
                        return true;
                    case "false":
                    case "f":
                    case "no":
                    case "n":
                    case "0":
                        return false;
                }
            }

            return defaultBool;
        }

        public static int ObjectIntValue(object obj)
        {
            return ObjectIntValue(obj, IntNullValue);
        }

        public static int ObjectIntValue(object obj, int defaultValue)
        {
            if (ObjectIsNull(obj))
                return defaultValue;
            if (obj is int)
                return (int)obj;
            if (obj is byte)
                return (byte)obj;
            if (obj is short)
                return (short)obj;
            if (obj is bool)
                return (bool)obj ? 1 : 0;
            var s = obj.ToString();
            if (s.Length == 0)
                return defaultValue;
            int ret;
            if (int.TryParse(s, out ret))
                return ret;
            return defaultValue;
        }

        public static long ObjectLongValue(object obj)
        {
            return ObjectLongValue(obj, LongNullValue);
        }

        public static long ObjectLongValue(object obj, long defaultValue)
        {
            if (ObjectIsNull(obj))
                return defaultValue;
            if (obj is long)
                return (long)obj;
            var s = obj.ToString();
            if (s.Length == 0)
                return defaultValue;
            long ret;
            if (long.TryParse(s, out ret))
                return ret;
            return defaultValue;
        }

        public static double ObjectDoubleValue(object obj)
        {
            return ObjectDoubleValue(obj, DoubleNullValue);
        }

        public static double ObjectDoubleValue(object obj, double defaultValue)
        {
            if (obj is double)
                return (double)obj;
            if (ObjectIsNull(obj))
                return defaultValue;
            if (obj is decimal)
                return (double)(decimal)obj;

            return double.TryParse(obj.ToString(), out var result) ? result : defaultValue;
        }

        public static DateTime ObjectDateValue(object obj)
        {
            return ObjectDateValue(obj, DateTimeNullValue);
        }

        public static DateTime ObjectDateValue(object obj, DateTime defaultValue)
        {
            if (ObjectIsNull(obj))
                return defaultValue;
            if (obj is DateTime)
                return (DateTime)obj;
            var s = obj.ToString();
            if (s.Length == 0)
                return defaultValue;
            DateTime dt;
            if (DateTime.TryParse(s, out dt))
            {
                if (dt.Year > 2999 || dt.Year < 1753)
                    return defaultValue;
                return dt;
            }

            return defaultValue;
        }

        public static byte ObjectByteValue(object obj)
        {
            return ObjectByteValue(obj, ByteNullValue);
        }

        public static byte ObjectByteValue(object obj, byte defaultByte)
        {
            if (ObjectIsNull(obj))
                return defaultByte;

            if (obj is byte)
                return (byte)obj;

            byte result;
            if (byte.TryParse(obj.ToString(), out result))
                return result;

            return defaultByte;
        }

        public static decimal ObjectDecimalValue(object obj)
        {
            return ObjectDecimalValue(obj, DecimalNullValue);
        }

        public static decimal ObjectDecimalValue(object obj, decimal defaultValue)
        {
            if (ObjectIsNull(obj))
                return defaultValue;

            if (obj is decimal)
                return (decimal)obj;

            if (obj is double)
                return (decimal)(double)obj;

            decimal result;
            if (decimal.TryParse(obj.ToString(), out result))
                return result;

            return defaultValue;
        }

        public static short ObjectShortValue(object obj)
        {
            return ObjectShortValue(obj, ShortNullValue);
        }

        public static short ObjectShortValue(object obj, short defaultValue)
        {
            if (ObjectIsNull(obj))
                return defaultValue;

            if (obj is short)
                return (short)obj;

            short result;
            if (short.TryParse(obj.ToString(), out result))
                return result;

            return defaultValue;
        }

        public static float ObjectFloatValue(object obj)
        {
            return ObjectFloatValue(obj, ShortNullValue);
        }

        public static float ObjectFloatValue(object obj, float defaultValue)
        {
            if (ObjectIsNull(obj))
                return defaultValue;

            if (obj is float)
                return (float)obj;

            float result;
            if (float.TryParse(obj.ToString(), out result))
                return result;

            return defaultValue;
        }

        public static char ObjectCharValue(object obj)
        {
            return ObjectCharValue(obj, CharNullValue);
        }

        public static char ObjectCharValue(object obj, char defaultValue)
        {
            if (ObjectIsNull(obj))
                return defaultValue;

            if (obj is char)
                return (char)obj;

            char result;
            if (char.TryParse(obj.ToString(), out result))
                return result;

            return defaultValue;
        }

        public static byte[] ObjectBytesValue(object obj)
        {
            return ObjectBytesValue(obj, new byte[0]);
        }

        public static byte[] ObjectBytesValue(object obj, byte[] defaultValue)
        {
            if (ObjectIsNull(obj))
                return defaultValue;

            if (obj is byte[])
                return (byte[])obj;

            return AWDEnvironment.s_DefaultEncoding.GetBytes(obj.ToString());
        }

        public static DateTime CheckSmallDateTime(DateTime dt)
        {
            return dt.Year < 1900 || dt.Year > 2078 ? DateTimeNullValue : dt;
        }

        public static string ConvertSize(string rawSize)
        {
            if (IsNull(rawSize))
                return null;
            var fileSize = ObjectIntValue(rawSize, 0);
            var M = fileSize / 1058816.0;
            if (M >= 1) return (int)(M + 0.5) + "MB";
            var K = fileSize / 1024.0;
            if (K >= 1) return (int)(K + 0.5) + "KB";
            return fileSize + "B";
        }

        #endregion

        #region Format data types to string

        public static string Format(bool b)
        {
            return b ? "1" : "0";
        }

        public static string Format(bool? b)
        {
            if (b == null || !b.HasValue)
                return "";

            return Format(b.Value);
        }

        public static string Format(int i)
        {
            if (IsNull(i))
                return null;
            return i.ToString();
        }

        public static string Format(int i, string defaultValue)
        {
            if (IsNull(i))
                return defaultValue;
            return i.ToString();
        }

        public static string Format(int? i, string defaultValue = null)
        {
            if (IsNull(i))
                return defaultValue;
            return i.ToString();
        }

        public static string Format(long l)
        {
            if (IsNull(l))
                return null;
            return l.ToString();
        }

        public static string Format(double d)
        {
            return Format(d, s_decimalPlace);
        }

        public static string Format(string s, int maxLen)
        {
            if (s != null && s.Length > maxLen)
                return s.Substring(0, maxLen);
            return s;
        }

        public static string Format(double d, int decimalPlace)
        {
            if (IsNull(d))
                return null;
            //10-16-2006: cchen to avoid scientific format(1E-05) if d is too small
            var temp = Math.Round(d, decimalPlace).ToString("G", NumberFormatInfo.InvariantInfo);
            if (temp.Contains("E"))
            {
                if (decimalPlace < 0)
                    decimalPlace = 6;
                var strScale = "F" + decimalPlace;
                temp = d.ToString(strScale, NumberFormatInfo.InvariantInfo);
            }

            return temp;
        }

        public static string Format(double d, int decimalPlace, int intPlace)
        {
            var s = Format(d, decimalPlace);
            if (s == null)
                return s;
            var index = s.IndexOf(".");
            var sb = new StringBuilder();
            for (var i = index < 0 ? s.Length : index; i < intPlace; i++)
                sb.Append("0");
            sb.Append(s);
            if (decimalPlace > 0)
            {
                if (index < 0)
                {
                    sb.Append(".");
                    for (var i = 0; i < decimalPlace; i++)
                        sb.Append("0");
                }
                else
                {
                    for (var i = s.Length - index - 1; i < decimalPlace; i++)
                        sb.Append("0");
                }
            }

            return sb.ToString();
        }

        public static string Format(DateTime dt)
        {
            if (IsNull(dt))
                return null;
            return dt.ToString("MM-dd-yyyy");
        }

        public static string FormatDateTime(DateTime dt)
        {
            if (IsNull(dt))
                return null;
            return dt.ToString("MM-dd-yyyy HH:mm:ss");
        }

        public static string FormatDateTime2(DateTime dt)
        {
            if (IsNull(dt))
                return null;
            return dt.ToString("MM/dd/yyyy HH:mm:ss");
        }

        public static string Format2(DateTime dt)
        {
            if (IsNull(dt))
                return null;
            return dt.ToString("MM/dd/yyyy");
        }

        public static string Format3(DateTime dt)
        {
            if (IsNull(dt))
                return null;

            return dt.ToString("yyyyMMdd");
        }

        public static string FormatWithCulture(AWDEnvironment env, DateTime dt)
        {
            if (IsNull(dt))
                return null;
            string cultureId = LangIdMapping.Instance.GetLangCode(env.RegionId);
            CultureInfo culture = null;
            try
            {
                if (!string.IsNullOrEmpty(cultureId) || cultureId != "ENU")
                    culture = new CultureInfo(cultureId);
            }
            catch (Exception ex)
            {
                ExceptionUtil.AppendException(ex, "Invalid Culture " + env.RegionId + " " + cultureId);
            }

            if (culture == null)
                return Format(dt);
            return dt.ToString("d", culture);
        }

        public static string Format(Guid guid)
        {
            if (IsNull(guid))
                return null;
            return guid.ToString();
        }

        public static string FormatObject(object obj)
        {
            return FormatObject(obj, null);
        }

        public static string FormatObject(object obj, string nullData)
        {
            if (obj == null || obj == DBNull.Value)
                return nullData;
            string s;
            var type = obj.GetType().ToString().ToUpper();
            switch (type)
            {
                case "SYSTEM.DATETIME":
                    s = Format((DateTime)obj);
                    break;
                case "SYSTEM.BOOLEAN":
                    s = Format((bool)obj);
                    break;
                case "SYSTEM.DECIMAL":
                    s = Format(decimal.ToDouble((decimal)obj));
                    break;
                case "SYSTEM.DOUBLE":
                    s = Format((double)obj);
                    break;
                case "SYSTEM.INT32":
                    s = Format((int)obj);
                    break;
                default:
                    s = obj.ToString();
                    break;
            }

            return s ?? nullData;
        }

        public static string GetTZFormat(DateTime dt)
        {
            var currentOffset = TimeZone.CurrentTimeZone.GetUtcOffset(dt);
            var s = currentOffset.ToString();
            if (!s.StartsWith("-"))
                s = "+" + s;
            return dt.ToString("s") + ".000" + s;
        }

        public static string GetTZFormat(DateTime dt, int nOffset)
        {
            if (IsNull(nOffset))
                return GetTZFormat(dt);
            var currentOffset = new TimeSpan(0, nOffset, 0);
            dt -= currentOffset;
            return dt.ToString("s") + ".000Z";
        }

        public static string GetTZFormat(DateTime dt, TimeZoneInfo tzinfo)
        {
            if (null == tzinfo)
                tzinfo = TimeZoneInfo.Local;
            if (dt.Kind != DateTimeKind.Utc)
            {
                dt = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
                dt = TimeZoneInfo.ConvertTimeToUtc(dt, tzinfo);
            }

            return dt.ToString("s") + ".000Z";
        }

        public static DateTime GetTZDateTime(string sDt, int nOffset)
        {
            var dt = ObjectDateValue(sDt);
            if (!IsNull(nOffset))
            {
                var currentOffset = new TimeSpan(0, nOffset, 0);
                dt -= currentOffset;
            }

            return dt;
        }

        public static DateTime ToLocalTime(DateTime date, string timeZoneId)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(date, timeZoneId, TimeZoneInfo.Local.StandardName);
        }

        public static DateTime ToLocalTime(DateTime date)
        {
            return ToLocalTime(date, "Central Standard Time");
        }

        public static DateTimeOffset Object2DateTimeOffset(object datetime)
        {
            var dateOffset = DateTimeOffset.MinValue;
            if (datetime != null)
            {
                if (datetime is DateTimeOffset)
                    return (DateTimeOffset)datetime;
                if (DateTimeOffset.TryParse(datetime + "", out dateOffset)) return dateOffset;
            }

            return dateOffset;
        }

        #endregion

        #region IsNull functions

        public static bool IsNull(Guid guid)
        {
            return guid == Guid.Empty;
        }

        public static bool IsNull(Guid? guid)
        {
            return guid == null || guid == Guid.Empty;
        }

        public static bool IsNull(int i)
        {
            return i == IntNullValue;
        }

        public static bool IsNull(long l)
        {
            return l == LongNullValue;
        }

        public static bool IsNull(int? i)
        {
            return i == null || i == IntNullValue;
        }

        public static bool IsNull(long? l)
        {
            return l == LongNullValue || l == null;
        }

        public static bool IsNull(string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static bool IsNull(double d)
        {
            return double.IsNaN(d) || double.IsInfinity(d);
        }

        public static bool IsNullOrZero(double d)
        {
            return IsNull(d) || IsZero(d);
        }

        public static bool IsNull(DateTime dt)
        {
            return dt == DateTimeNullValue || dt.Year > 2999; // || dt.Year < 1900;
        }

        public static bool ObjectIsNull(object obj)
        {
            return obj == null || obj == DBNull.Value || (obj is double && IsNull((double)obj));
        }

        public static bool IsNull(ICollection objList)
        {
            return objList == null || objList.Count <= 0;
        }

        public static bool IsNull<T>(ICollection<T> objList)
        {
            return objList == null || objList.Count <= 0;
        }

        public static bool IsNull<T>(List<T> objList)
        {
            return objList == null || objList.Count <= 0;
        }

        public static bool IsNull<K, V>(Dictionary<K, V> objHash)
        {
            return objHash == null || objHash.Count <= 0;
        }

        public static bool IsNull<T>(T[] objArray)
        {
            return objArray == null || objArray.Length <= 0;
        }

        public static bool IsNull(byte[] bytes)
        {
            return bytes == null || bytes.Length <= 0;
        }

        #endregion

        #region data check functions

        public static bool IsCustomSecId(string secid)
        {
            if (secid.StartsWith("(("))
                return true;

            return false;
        }

        public static string GetCustomType(string secid)
        {
            if (secid.StartsWith("((A"))
                return "AG";
            if (secid.StartsWith("((S"))
                return "IM";
            if (secid.StartsWith(
                    "((")) //in the getholding case, the secid is ((+masterportfolioId which is an int Donglin add 20061107
                return "IM";

            return null;
        }

        public static DateTime GetSettlementDate(DateTime dt)
        {
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                case DayOfWeek.Monday:
                case DayOfWeek.Tuesday:
                    return dt.AddDays(3);
                case DayOfWeek.Wednesday:
                case DayOfWeek.Thursday:
                case DayOfWeek.Friday:
                    return dt.AddDays(5);
                case DayOfWeek.Saturday:
                    return dt.AddDays(4);
            }

            return dt;
        }

        #endregion
    }

    public static class DataTypeExtensions
    {
        public static bool IsBetween(this DateTime dt, DateTime dtStart, DateTime dtEnd)
        {
            return dt >= dtStart && dt <= dtEnd;
        }

        public static bool IsEarlierThan(this DateTime dt, DateTime dtTo)
        {
            return !DataTypeUtil.IsNull(dt) && dt < dtTo;
        }

        public static bool IsLaterThan(this DateTime dt, DateTime dtTo)
        {
            return !DataTypeUtil.IsNull(dt) && dt > dtTo;
        }

        public static IEnumerable<string> SplitByComma(this string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
                foreach (var item in text.Split(new[] { ',' },
                             StringSplitOptions.RemoveEmptyEntries))
                    yield return item;
        }

        public static IEnumerable<string> SplitBySemicolin(this string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
                foreach (var item in text.Split(new[] { ';' },
                             StringSplitOptions.RemoveEmptyEntries))
                    yield return item;
        }

        public static DateTime? ToNullDate(this object obj)
        {
            var date = DataTypeUtil.ObjectDateValue(obj);
            if (DataTypeUtil.IsNull(date)) return null;
            return date;
        }

        #region string

        public static XmlDocument ToXml(this string xml)
        {
            var doc = XmlUtil.LoadXml(xml);
            return doc;
        }

        public static SvrException ToException(this string msg)
        {
            return new SvrException(ErrorInfo.s_GenericException, msg);
        }

        public static string FormatTo(this string format, params object[] para)
        {
            return string.Format(format, para);
        }

        public static XmlDocument FormatToXml(this string format, params object[] para)
        {
            return string.Format(format, para).ToXml();
        }

        public static string AggregateStrList(this List<string> list, char separator)
        {
            return list.Aggregate("", (current, s) => current + (s + separator)).Trim(separator);
        }

        #endregion string


        #region object

        public static string ToStr(this object obj)
        {
            return DataTypeUtil.ObjectStringValue(obj);
        }

        public static double ToDouble(this object obj)
        {
            return DataTypeUtil.ObjectDoubleValue(obj);
        }

        public static decimal ToDecimal(this object obj)
        {
            return DataTypeUtil.ObjectDecimalValue(obj);
        }

        public static double ToDouble(this object obj, double defaultValue)
        {
            return DataTypeUtil.ObjectDoubleValue(obj, defaultValue);
        }

        public static int ToInt(this object obj)
        {
            return DataTypeUtil.ObjectIntValue(obj);
        }

        public static long ToLong(this object obj)
        {
            return DataTypeUtil.ObjectLongValue(obj);
        }

        public static short ToShort(this object obj)
        {
            return DataTypeUtil.ObjectShortValue(obj);
        }

        public static int ToInt(this object obj, int defaultValue)
        {
            return DataTypeUtil.ObjectIntValue(obj, defaultValue);
        }

        public static bool ToBool(this object obj)
        {
            return DataTypeUtil.ObjectBoolValue(obj);
        }

        public static bool ToBool(this object obj, bool defaultValue)
        {
            return DataTypeUtil.ObjectBoolValue(obj, defaultValue);
        }

        public static Guid ToGuid(this object obj)
        {
            return DataTypeUtil.ObjectGuidValue(obj);
        }

        public static Guid ToGuid(this object obj, Guid defaultValue)
        {
            return DataTypeUtil.ObjectGuidValue(obj, defaultValue);
        }

        public static DateTime ToDate(this object obj)
        {
            return DataTypeUtil.ObjectDateValue(obj);
        }

        public static DateTime ToDate(this object obj, DateTime defaultValue)
        {
            return DataTypeUtil.ObjectDateValue(obj, defaultValue);
        }

        public static string ToHTTPShortDateString(this DateTime date)
        {
            if (date == DateTime.MinValue) return null;

            return date.ToString(DataTypeUtil.HTTPShortDateFormat);
        }

        #endregion object


        #region xml

        public static string GetElementValue(this XmlNode node, string name)
        {
            return XmlUtil.GetTextValue(node, name);
        }

        public static void SetElementValue(this XmlNode parent, string name, string data)
        {
            XmlUtil.SetElementValue(parent, name, data);
        }

        public static XmlDocument ToErrorXml(this Exception ex)
        {
            return XmlUtil.GenerateErrorXml(ErrorInfo.s_GenericException, ex.Message);
        }

        public static XmlElement GetOrAddElement(this XmlNode parent, string name)
        {
            return XmlUtil.GetOrAddElement(parent, name);
        }

        public static bool HasParameter(this XmlDocument doc, string name)
        {
            if (doc != null)
            {
                var node = doc.SelectSingleNode("//parameters/" + name);
                return node != null && !DataTypeUtil.IsNull(node.InnerText);
            }

            return false;
        }

        public static string GetParamerter(this XmlNode doc, string name)
        {
            return doc.GetElementValue("//parameters/" + name);
        }

        public static string GetAttributeValue(this XmlNode node, string tagName, string name)
        {
            return node == null ? null : XmlUtil.GetAttributeValue((XmlElement)node.SelectSingleNode(tagName), name);
        }

        public static string GetAttributeValue(this XmlNode node, string name)
        {
            return node == null ? null : XmlUtil.GetAttributeValue((XmlElement)node, name);
        }

        public static int GetErrorCode(this XmlNode node)
        {
            return node.GetAttributeValue("//error", "code").ToInt(0);
        }

        #endregion xml

        #region BaseSerializableObj

        public static List<T> DeserializeObjects<T>(this DataTable dt) where T : BaseSerializableObj
        {
            var data = new List<T>();
            if (dt != null && dt.Rows.Count > 0)
            {
                var i = 0;
                var fieldNames = new string[dt.Columns.Count];
                foreach (DataColumn dc in dt.Columns)
                    fieldNames[i++] = dc.ColumnName.ToLower();

                foreach (DataRow row in dt.Rows)
                {
                    var client = (T)Activator.CreateInstance(typeof(T));
                    client.Deserialize(fieldNames, row.ItemArray);
                    data.Add(client);
                }
            }

            return data;
        }

        /// <summary>
        ///     write nothing if input is null; write parentTag if input is empty and parentTag is given
        /// </summary>
        public static void WriteTo(this IEnumerable<BaseSerializableObj> list, XmlWriter writer,
            string parentTag = null)
        {
            if (list == null) return;
            if (!string.IsNullOrWhiteSpace(parentTag)) writer.WriteStartElement(parentTag);
            foreach (var obj in list) obj.WriteTo(writer);
            if (!string.IsNullOrWhiteSpace(parentTag)) writer.WriteEndElement();
        }

        /// <summary>
        ///     write nothing if input has no element
        /// </summary>
        public static void WriteTo<T>(this IEnumerable<T> enumerable, XmlWriter writer,
            Action<XmlWriter, T> code, string parentTag = null)
            where T : BaseSerializableObj
        {
            if (enumerable == null) return;
            var list = enumerable as T[] ?? enumerable.ToArray();
            if (!list.Any()) return;
            if (!string.IsNullOrWhiteSpace(parentTag)) writer.WriteStartElement(parentTag);
            foreach (var o in list)
                if (code != null)
                    code(writer, o);
                else
                    o.WriteTo(writer);
            if (!string.IsNullOrWhiteSpace(parentTag)) writer.WriteEndElement();
        }

        public static void WriteTo(this IEnumerable<BaseSerializableObj> list, XmlElement parentElem, string tag)
        {
            if (list == null) return;
            var elem = XmlUtil.AddElement(parentElem, tag);
            foreach (var obj in list) obj.WriteTo(elem);
        }

        /// <summary>
        ///     always write parent tag even if input has no element, unless a parent tag is not provided and code does nothing
        ///     with null
        /// </summary>
        public static void WriteObjects<T>(this XmlWriter writer, string tag, List<T> objs, Action<XmlWriter, T> code)
            where T : BaseSerializableObj
        {
            if (tag != null)
                writer.WriteStartElement(tag);

            if (objs != null)
            {
                if (code != null)
                    objs.ForEach(o => code(writer, o));
                else
                    objs.ForEach(o => o.WriteTo(writer));
            }
            else if (code != null)
            {
                code(writer, null);
            }

            if (tag != null)
                writer.WriteEndElement();
        }

        #endregion
    }
}