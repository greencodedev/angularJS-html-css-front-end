using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace BaseWebApp.Maven.Utils.Email
{
    public class TemplateEngine
    {
        public static string GetSetting(string name)
        {
            var settings = WebConfigurationManager.OpenWebConfiguration("~/Configuration");
            return settings.AppSettings.Settings[name].Value;
        }

        /*public static StringTemplate GetTempalte(string tempName)
        {
            try
            {
                StringTemplateGroup group = new StringTemplateGroup("Templates", AppDomain.CurrentDomain.BaseDirectory + Config.GetConfig("TEMP_DIR"));
                StringTemplate temp = group.GetInstanceOf(GetSetting(tempName));
                return temp;
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                return null;
            }
        }*/
    }
}