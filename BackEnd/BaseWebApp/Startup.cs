using System;
using System.Collections.Generic;
using System.Linq;
using BaseWebApp.Maven.Utils;
using Microsoft.Owin;
using MWSUtils;
using Owin;

[assembly: OwinStartup(typeof(BaseWebApp.Startup))]

namespace BaseWebApp
{
    public partial class Startup
    {
        public object Libs { get; private set; }

        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            Dictionary<string, string> accessKeys = new Dictionary<string, string>();
            Dictionary<string, string> secretKeys = new Dictionary<string, string>();

            List<string> keys = Config.GetMwsTokenConfigs();

            foreach (string key in keys)
            {
                string tokeKey = key.Replace(Config.MWS_ACCESS_KEY_PREFIX, "");

                string accessKey = Config.GetConfig(Config.MWS_ACCESS_KEY_PREFIX + tokeKey);
                accessKeys.Add(tokeKey, accessKey);

                string secretKey = Config.GetConfig(Config.MWS_SECRET_KEY_PREFIX + tokeKey);
                secretKeys.Add(tokeKey, secretKey);
            }

            MWSProvider.SetAccessKey(
                accessKeys,
                secretKeys,
                Config.GetConfig("MWS_APP_NAME"),
                Config.GetConfig("MWS_APP_VERSION")
            );

        }

    }
}
