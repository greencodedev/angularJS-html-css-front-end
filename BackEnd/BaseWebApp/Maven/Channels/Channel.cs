using MWSUtils.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.Channels
{
    public class Channel
    {
        public int ChannelId { get; set; }
        public int AccountId { get; set; }
        public string Name { get; set; }
        public ChannelTypes Type { get; set; }
        public string Status = "Active";
        public string FinaleAccountName { get; set; }
        public string FinaleUsername { get; set; }
        public string FinalePassword { get; set; }
        public string FinaleToken { get; set; }
        public string AmazonSellerId { get; set; }
        public string AmazonToken { get; set; }
        public int? MarketplaceId { get; set; }
        public string CreatedUser { get; set; }
        public DateTime? LastSyncTime { get; set; }
        public DateTime? CreatedTime { get; set; }

        public MWSClientCredentials GetMWSCredentials()
        {
            return new MWSClientCredentials
            {
                Merchant = AmazonSellerId,
                MWSAuthToken = AmazonToken,
                ServiceURL = "https://mws.amazonservices.com",
                TokenKey = "US"
            };
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ChannelTypes
        {
            Finale,
            Amazon
        }
    }
}