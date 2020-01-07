using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;

namespace BaseWebApp.Maven.Utils
{
    public class UtilsLib
    {
        public static string getConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public static DateTime UtcToLocalTime(DateTime utcTime)
        {
            utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);

            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, easternZone);
        }

        public static DateTime ToUtc(DateTime pstTime)
        {
            //TimeZoneInfo pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            DateTime utc = TimeZoneInfo.ConvertTimeToUtc(pstTime);
            return utc;
        }

        public static DateTime UtcToPst(DateTime utc)
        {
            TimeZoneInfo pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            DateTime pst = TimeZoneInfo.ConvertTimeFromUtc(utc, pstZone);
            return pst;
        }

        public static string FirstNotNull(string str1, string str2)
        {
            return !string.IsNullOrEmpty(str1) ? str1 : str2;
        }

        public static string ToUpperCase(string val)
        {
            if (val == null)
                return null;

            return val.ToUpper();
        }

        public static string Replace(string val, string oldChar, string newChar)
        {
            return string.IsNullOrEmpty(val) ? "" : val.Replace(oldChar, newChar);
        }

        public static double ParseToDouble(string val)
        {
            bool success = Double.TryParse(Replace(val, "$", ""), out double result);

            return success ? result : 0;
        }

        public static int ParseIntOrZero(string strValue)
        {
            bool success = int.TryParse(strValue, out int res);
            return success ? res : 0;
        }

        public static int GetUniqueNumber()
        {
            return DateTime.Now.Millisecond;
        }

        public static T Clone<T>(T source)
        {
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }

        public static double TurnOver(double val)
        {
            if (val > 0)
            {
                return val * -1;
            }

            return Math.Abs(val);
        }

        public static DateTime ParseAmazonUtcTime(string dateStr)
        {
            dateStr = dateStr.Replace(" UTC", "Z");
            dateStr = dateStr.Replace(" ", "T");

            return DateTimeOffset.Parse(dateStr).UtcDateTime;
        }

        public static double Round(double val)
        {
            return Math.Round(val * 100) / 100;
        }

        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static string HtmlDecode(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            return WebUtility.HtmlDecode(str);
        }

        public static string Truncate(string str, int length)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            int maxLength = Math.Min(str.Length, length);
            return str.Substring(0, maxLength);
        }
    }
}