using BaseWebApp.Maven.Users;
using BaseWebApp.Maven.Utils.Api;
using BaseWebApp.Maven.Utils.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static BaseWebApp.Maven.Channels.Channel;

namespace BaseWebApp.Maven.Channels
{
    public class ChannelsProvider
    {
        public static ProviderResponse GetChannelOrChannels(int AccountId, int? ChannelId)
        {
            ProviderResponse res = new ProviderResponse();

            try
            {

                if (ChannelId != null && ChannelId != 0)
                {
                    List<Channel> channels = GetChannels(AccountId, ChannelId, false);

                    if (!channels.Any())
                    {
                        res.Messages.Add("Invalid channel id");
                        return res;
                    }

                    res.Data = channels[0];
                }
                else
                {
                    List<Channel> channels = GetChannels(AccountId, null, false);

                    if (!channels.Any())
                    {
                        res.Messages.Add("No channels for this account");
                        return res;
                    }

                    res.Data = channels;
                }

            }
            catch (Exception e)
            {
                Utils.Logger.Log(e.ToString());
                res.Messages.Add("An error occurred");
            }

            return res;
        }

        public static Channel GetChannelWithSensitiveData(int AccountId, int channelId)
        {
            try
            {
                if (channelId != 0)
                {
                    List<Channel> channels = GetChannels(AccountId, channelId, true);

                    if (channels.Any())
                    {
                        return channels[0];
                    }
                    
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return null;
        }

        public static ProviderResponse InsertChannel(Channel channel, int AccountId)
        {
            ProviderResponse res = new ProviderResponse();

            try
            {
                channel.AccountId = AccountId;

                //validate
                List<string> error = ValidateChannel(channel, false);

                if (error.Any())
                {
                    res.Messages = error;
                    return res;
                }
                
                //populate user
                channel.CreatedUser = UsersProvider.GetCurrentUser().Email;

                //insert
                int ChaanelId = InsertChannel(channel);

                //return newly created channel
                return GetChannelOrChannels(AccountId, ChaanelId);

            }
            catch (Exception e)
            {
                Utils.Logger.Log(e.ToString());
                res.Messages.Add("An error occurred");
            }

            return res;
        }

        public static void UpdateChannelSensetiveData(Channel channel, int AccountId)
        {
            try
            {
                channel.AccountId = AccountId;

                List<string> errors = ValidateChannel(channel, true);

                if (!errors.Any())
                {
                    UpdateChannel(channel, true);
                }
                
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static ProviderResponse UpdateChannel(Channel channel, int AccountId)
        {
            ProviderResponse res = new ProviderResponse();

            try
            {
                channel.AccountId = AccountId;

                List<string> errors = ValidateChannel(channel, true);

                if (errors.Any())
                {
                    res.Messages = errors;
                    return res;
                }

                UpdateChannel(channel, false);

                //return updated channel
                return GetChannelOrChannels(AccountId, channel.ChannelId);
            }
            catch (Exception e)
            {
                Utils.Logger.Log(e.ToString());
                res.Messages.Add("An error occurred");
            }

            return res;
        }

        private static List<Channel> GetChannels(int AccountId, int? channelId, bool sensitiveData)
        {
            List<Channel> channels = new List<Channel>();

            string query = "select * from Channels where AccountId = @AccountId ";

            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.AddParam("@AccountId", AccountId);

            if(channelId != null && channelId != 0)
            {
                query += "and ChannelId = @ChannelId ";
                sqlQuery.AddParam("@ChannelId", channelId);
            }

            using (Sql sql = new Sql())
            {
                using (SqlReader reader = sqlQuery.ExecuteReader(sql, query))
                {
                    while (reader.HasNext())
                    {
                        Channel channel = new Channel
                        {
                            ChannelId = reader.GetInt("ChannelId"),
                            AccountId = reader.GetInt("AccountId"),
                            Name = reader.GetString("Name"),
                            Type = (ChannelTypes)Enum.Parse(typeof(ChannelTypes), reader.GetString("Type")),
                            Status = reader.GetString("Status"),
                            MarketplaceId = reader.GetInteger("MarketplaceId"),
                            CreatedUser = reader.GetString("CreatedUser"),
                            LastSyncTime = reader.GetOptionalTime("LastSyncTime"),
                            CreatedTime = reader.GetOptionalTime("CreatedTime")
                        };

                        if (sensitiveData)
                        {
                            channel.FinaleAccountName = reader.GetString("FinaleAccountName");
                            channel.FinaleUsername = reader.GetString("FinaleUsername");
                            channel.FinalePassword = reader.GetString("FinalePassword");
                            channel.FinaleToken = reader.GetString("FinaleToken");
                            channel.AmazonSellerId = reader.GetString("AmazonSellerId");
                            channel.AmazonToken = reader.GetString("AmazonToken");
                        }

                        channels.Add(channel);
                    }
                }
            }
            
            return channels;
        }

        private static void UpdateChannel(Channel channel, bool updateSensetiveData)
        {

            string query = @"UPDATE Channels SET Name = @Name, Type = @Type, Status = @Status, ";


            if (updateSensetiveData)
            {
                query += @"FinaleAccountName = @FinaleAccountName, FinaleUsername = @FinaleUsername, 
                             FinalePassword = @FinalePassword, FinaleToken = @FinaleToken, AmazonSellerId = @AmazonSellerId, AmazonToken = @AmazonToken, ";
            }

            query += @"MarketplaceId = @MarketplaceId, CreatedUser = @CreatedUser,LastSyncTime = @LastSyncTime
                             WHERE AccountId = @AccountId ";

            SqlQuery sqlQuery = new SqlQuery();

            sqlQuery.AddParam("@Name", channel.Name);
            sqlQuery.AddParam("@Type", channel.Type);
            sqlQuery.AddParam("@Status", channel.Status);
            sqlQuery.AddParam("@FinaleAccountName", channel.FinaleAccountName);
            sqlQuery.AddParam("@FinaleUsername", channel.FinaleUsername);
            sqlQuery.AddParam("@FinalePassword", channel.FinalePassword);
            sqlQuery.AddParam("@FinaleToken", channel.FinaleToken);
            sqlQuery.AddParam("@AmazonSellerId", channel.AmazonSellerId);
            sqlQuery.AddParam("@AmazonToken", channel.AmazonToken);
            sqlQuery.AddParam("@MarketplaceId", channel.MarketplaceId);
            sqlQuery.AddParam("@CreatedUser", channel.CreatedUser);
            sqlQuery.AddParam("@LastSyncTime", channel.LastSyncTime);
            sqlQuery.AddParam("@AccountId", channel.AccountId);

            using (Sql sql = new Sql())
            {
                sqlQuery.ExecuteNonQuery(sql, query);
            }
        }

        public static int InsertChannel(Channel channel)
        {
            string query = @"INSERT INTO Channels (AccountId,Name, Type, Status, FinaleAccountName,FinaleUsername,FinalePassword,FinaleToken,AmazonSellerId, AmazonToken, MarketplaceId,CreatedUser,LastSyncTime)
                             VALUES (@AccountId, @Name, @Type,@Status,@FinaleAccountName,@FinaleUsername,@FinalePassword,@FinaleToken,@AmazonSellerId,@AmazonToken,@MarketplaceId,@CreatedUser,@LastSyncTime)";

            SqlQuery sqlQuery = new SqlQuery();

            sqlQuery.AddParam("@AccountId", channel.AccountId);
            sqlQuery.AddParam("@Name", channel.Name);
            sqlQuery.AddParam("@Type", channel.Type);
            sqlQuery.AddParam("@Status", channel.Status);
            sqlQuery.AddParam("@FinaleAccountName", channel.FinaleAccountName);
            sqlQuery.AddParam("@FinaleUsername", channel.FinaleUsername);
            sqlQuery.AddParam("@FinalePassword", channel.FinalePassword);
            sqlQuery.AddParam("@FinaleToken", channel.FinaleToken);
            sqlQuery.AddParam("@AmazonSellerId", channel.AmazonSellerId);
            sqlQuery.AddParam("@AmazonToken", channel.AmazonToken);
            sqlQuery.AddParam("@MarketplaceId", channel.MarketplaceId);
            sqlQuery.AddParam("@CreatedUser", channel.CreatedUser);
            sqlQuery.AddParam("@LastSyncTime", channel.LastSyncTime);

            using (Sql sql = new Sql())
            {
               return (int)sqlQuery.ExecuteInsert(sql, query);
            }
        }

        public static List<string> ValidateChannel(Channel channel, bool update)
        {
            List<string> erros = new List<string>();

            //erros not from user dasplayed fields
            if (string.IsNullOrEmpty(channel.Status) || (channel.ChannelId == 0 && update) || channel.AccountId == 0)
            {
                erros.Add("Error");
            }

            //erros from user dasplayed fields
            else
            if (string.IsNullOrEmpty(channel.Name))
            {
                erros.Add("Missing channel name");
            }
            
            //TODO incative channels
            return erros;
        }
    }
}