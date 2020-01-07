using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.Utils
{
    public class Config
    {
        public static readonly string MWS_ACCESS_KEY_PREFIX = "MWS_ACCESS_KEY_";
        public static readonly string MWS_SECRET_KEY_PREFIX = "MWS_SECRET_KEY_";

        public static bool SendEmail()
        {
            return "Y".Equals(GetConfig("EMAIL_SEND_EMAIL"));
        }

        public static bool UseSmtpSSL()
        {
            return "Y".Equals(GetConfig("EMAIL_SMTP_SSL"));
        }

        public static int GetSmtpPort()
        {
            return int.Parse(GetConfig("EMAIL_SMTP_PORT"));
        }

        public static string GetConfig(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public static List<string> GetMwsTokenConfigs()
        {
            return ConfigurationManager.AppSettings.AllKeys.ToList().FindAll(x => x.StartsWith(MWS_ACCESS_KEY_PREFIX));
        }
    }
}