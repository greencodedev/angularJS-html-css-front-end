using BaseWebApp.Maven;
using BaseWebApp.Maven.Channels;
using BaseWebApp.Maven.Products;
using BaseWebApp.Maven.Users;
using BaseWebApp.Maven.Utils;
using BaseWebApp.Maven.Utils.Api;
using MWSUtils;
using MWSUtils.Listings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BaseWebApp.Controllers
{
    [RoutePrefix("api/sync")]
    public class SyncController : BaseApiController
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("{channelId}")]
        public HttpResponseMessage SyncCategories(int channelId)
        {
            ProviderResponse res = new ProviderResponse();
            channelId = 2;
            string JobType = "SyncProducts";
            int jobId = 0;

            try
            {
                if (!SyncProvider.IsJobRunning(JobType))
                {
                    jobId = SyncProvider.InsertJob(JobType);

                    Channel channel = (Channel)ChannelsProvider.GetChannelWithSensitiveData(UsersProvider.GetCurrentAccountId(), channelId);

                    if(channel != null)
                    {
                        

                        if (channel.Type == Channel.ChannelTypes.Finale)
                        {
                            //sync
                            ProviderResponse categories = Sync.SyncCat(channel.ChannelId);
                            if (categories.Success)
                            {
                                ProviderResponse conditions = Sync.SyncConditions(channelId);
                                if (conditions.Success)
                                {
                                    ProviderResponse products = new ProviderResponse();
                                    products = Sync.SyncProd(channel.ChannelId);
                                    if (products.Success)
                                    {
                                        ProviderResponse sublocations = Sync.SyncSublocations(channel.ChannelId);
                                    }
                                    else
                                    {
                                        res.Success = false;
                                    }
                                }

                                if (!conditions.Success)
                                {
                                    res.Success = false;
                                }
                            }
                            else
                            {
                                res.Success = false;
                            }
                        }
                        else
                        {
                            //sync amazon products
                            //pull active listing report
                            List<AmazonProduct> products = MWSProvider.GetMerchantListingsData(channel.GetMWSCredentials());

                            if (products.Any())
                            {
                              Sync.SyncAmazonProducts(products, channelId);
                            }
                        }
                        
                    }


                    SyncProvider.StopJob(jobId, res.Success);
                } else
                {
                    res.Success = false;
                    res.Messages.Add("There is currently a job running");
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                res.Messages.Add("an error has occurred, please try again later.");

                if (jobId != 0)
                {
                    SyncProvider.StopJob(jobId, false);
                }
            }
            
            return ReturnResponse(res);
        }

        [HttpGet]
        [Route("lastSynced/{syncType}")]
        public HttpResponseMessage LastSyncedProducts(string syncType)
        {
            ProviderResponse res = new ProviderResponse();

            try
            {
                res.Data = SyncProvider.LastSynced(syncType);
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                res.Messages.Add("an error has occurred, please try again later.");
            }
            
            return ReturnResponse(res);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("transactionReport")]
        public HttpRequestMessage SyncTransactionReport()
        {
            DateTime date =  DateTime.Now.AddDays(-2);

            TransactionsReportProvider.sync(false, null, date, 1);

            return new HttpRequestMessage();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("transactionReportByDate/{date}")]
        public HttpRequestMessage SyncTransactionReport(DateTime date)
        {
            TransactionsReportProvider.sync(false, null, date, 1);

            return new HttpRequestMessage();
        }
    }
}
