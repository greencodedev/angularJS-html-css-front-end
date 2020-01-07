using BaseWebApp.Maven.Channels;
using BaseWebApp.Maven.FinaleInventory;
using BaseWebApp.Maven.Products.AverageCost;
using BaseWebApp.Maven.Users;
using BaseWebApp.Maven.Utils;
using BaseWebApp.Maven.Utils.Email;
using BaseWebApp.Maven.Utils.Sql;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace BaseWebApp.Maven.Products
{
    public class TransactionsReportProvider
    {
        private static Object LOCK = new Object();

        public static void sync(bool cookieUpdated, string productName, DateTime date, int channelId)
        {
            try
            {
                
                lock (LOCK)
                {
                    //DateTime date =  DateTime.Now.AddDays(-2);
                    Channel channel = ChannelsProvider.GetChannelWithSensitiveData(UsersProvider.GetCurrentAccountId(), channelId);

                    HttpResponseMessage res = FinaleClientProvider.TransactionsReport(date, productName, channel);

                    if (res.IsSuccessStatusCode)
                    {
                        string content = res.Content.ReadAsStringAsync().Result;

                        List<TransactionReportData> ReportDataList = JsonConvert.DeserializeObject<List<TransactionReportData>>(content);

                        //parse to object
                        List<TrasactionReportObject> Trasactions = FinaleClientProvider.ParseTransactionsReport(ReportDataList);

                        List<string> FinaleProductIds = Trasactions.DistinctBy(x => x.FinaleId).Select(x => x.FinaleId).ToList();

                        //get current stock
                        Dictionary<string, int> CurrentProductsStock = FinaleClientProvider.GetStockForProducts(FinaleProductIds, false, channelId);

                        foreach (var productId in FinaleProductIds)
                        {
                            //get all transaction for this product
                            List<TrasactionReportObject> TrasactionsForProduct = Trasactions.FindAll(x => x.FinaleId == productId).ToList();

                            if (!TrasactionsForProduct.Any(x => x.TransactionType == "Purchase shipment"))
                            {
                                continue;
                            }

                            //pull last log from bd
                            AverageCostLog LastLog =  AverageCostLogProvider.GetLastLog(productId, "Purchase shipment");

                            //we need to find only the trasactions we don't currently have.
                            List<TrasactionReportObject> NewTrasactions = new List<TrasactionReportObject>();

                            if (LastLog != null)
                            {
                                NewTrasactions =  FindNewTransactions(TrasactionsForProduct, LastLog);
                            }
                            else
                            {
                                NewTrasactions = TrasactionsForProduct;
                            }

                            if (NewTrasactions.Any(x => x.TransactionType == "Purchase shipment"))
                            {
                                //pull product from finaly to get current average cost in finale
                                Product prod = FinaleClientProvider.GetSingleProduct(productId, false, channelId);

                                //Get avaerage cost before the trasactions -if average was overridden in finaley then take that
                                //get current avg cost for product
                                double AverageCostBeforeTransaction = 0;

                                //if we have prod.avg cost, use that,
                                double.TryParse(prod.AverageCost, out AverageCostBeforeTransaction);

                                //calculate the stock before the new transactions (by turning negative sum into positive and vice verse)
                                int TrasactionQuantity = NewTrasactions.Sum(x => x.Quantity) * -1;
                        
                                int StockBeforeTrasactions = 0;
                                if(CurrentProductsStock.ContainsKey(productId))
                                {
                                    StockBeforeTrasactions = CurrentProductsStock[productId] + TrasactionQuantity;
                                }

                                //populate and insert new logs
                                PopulateAndInsertLogsForProduct(prod.FinaleId, NewTrasactions, StockBeforeTrasactions, AverageCostBeforeTransaction, channelId);
                            }
                        }
                    }
                    else if (res.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        if (!cookieUpdated)
                        {
                            if (FinaleClientProvider.UpdateCookie(channel))
                            {
                                sync(true, productName, date, channelId);
                            }
                            else
                            {
                                Logger.Log("Problems Updating cookie");
                                throw new Exception("Problems Updating cookie");
                            }
                        }
                        else
                        {
                            Logger.Log("Problems authorizing");
                            throw new Exception("Unauthorized twice");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());

                EmailClient.SendEmail("yitz@mavensoftwaresolutions.com", "Error on avg cost sync", e.ToString());
            }
        }

        public static List<TrasactionReportObject> FindNewTransactions(List<TrasactionReportObject> TrasactionsForProduct, AverageCostLog LastLog)
        {
            int Position = TrasactionsForProduct.FindIndex(x => x.TransactionDescription == LastLog.TransactionDescription
                                                                && x.ReceivedTimestamp == LastLog.ReceivedDate
                                                                && LastLog.ReceivedDate != null);
            //if was found
            if (Position != -1)
            {
                int startIndex = Position + 1;
                return TrasactionsForProduct.GetRange(Position + 1, TrasactionsForProduct.Count - startIndex);
            }

            return TrasactionsForProduct;
        }

        private static void PopulateAndInsertLogsForProduct(string finaleProductId, List<TrasactionReportObject> NewTrasactions, int BeginingStock, double BeginingAvgCost, int channelId)
        {
            if (NewTrasactions.Any())
            {
                List<AverageCostLog> newlogs = new List<AverageCostLog>();

                int StockTracker = BeginingStock;
                double AverageCost = BeginingAvgCost;

                for (int i = 0; i < NewTrasactions.Count; i++)
                {
                    var trans = NewTrasactions[i];

                    double currAvgCost = AverageCost;
                    double currCost = StockTracker * currAvgCost;
                    StockTracker += trans.Quantity;

                    if (trans.TransactionType == "Purchase shipment" || trans.TransactionType == "Convert")
                    {

                        double additionalCost = trans.CostPerUnit * trans.Quantity;
                        double totalCost = currCost + additionalCost;

                        if(trans.TransactionType == "Convert" && trans.CostPerUnit == 0)
                        {
                            //leave AverageCost as is
                        }
                        else if(StockTracker == 0)
                        {
                            AverageCost = 0;
                        }
                        else if (AverageCost == 0)
                        {
                            AverageCost = trans.CostPerUnit;
                        }
                        else
                        {
                            AverageCost = Math.Round(totalCost / StockTracker, 2, MidpointRounding.AwayFromZero);
                        }

                        newlogs.Add(new AverageCostLog
                        {
                            FinalProductId = trans.FinaleId,
                            CostPerUnit = trans.CostPerUnit,
                            CurrAvgCost = Math.Round(currAvgCost, 2, MidpointRounding.AwayFromZero),
                            CurrentStock = StockTracker,
                            Quantity = trans.Quantity,
                            NewAverageCost = Math.Round(AverageCost, 2, MidpointRounding.AwayFromZero),
                            TransactionType = trans.TransactionType,
                            TransactionDescription = trans.TransactionDescription,
                            ReceivedDate = trans.ReceivedTimestamp
                        });
                    }
                }

                AverageCostLogProvider.InsertLogs(newlogs);

                //update last line avg cost to finale
                if(BeginingAvgCost != AverageCost)
                {
                    UpdateFinaleAvgCost(finaleProductId, AverageCost, channelId);
                }
            }
        }

        public static void CalcualteCostOnConvert(string finaleProductId, double origItemAvgCost, int qty, string description, int BeginingStock, double BeginingAvgCost, int channelid)
        {
            List<TrasactionReportObject> NewTrasactions = new List<TrasactionReportObject> {
                new TrasactionReportObject{
                    FinaleId = finaleProductId,
                    CostPerUnit = origItemAvgCost,
                    Quantity = qty,
                    TransactionType = "Convert",
                    TransactionDescription = description,
                    ReceivedTimestamp = DateTime.UtcNow
                }
            };

            PopulateAndInsertLogsForProduct(finaleProductId, NewTrasactions, BeginingStock, BeginingAvgCost, channelid);
        }

        private static void UpdateFinaleAvgCost(string finaleProductId, double lastAvgCost, int channelid)
        {
            //Get new object from finale
            JObject finaleProduct = FinaleClientProvider.GetProductObject(finaleProductId, false, channelid);
            List<UserFieldData> customFieldList = JsonConvert.DeserializeObject<List<UserFieldData>>(finaleProduct.GetValue("userFieldDataList").ToString());

            //set avg cost custom field
            UserFieldData field = customFieldList.Find(f => f.attrName == Config.GetConfig("FINALE_CUSTOM_FIELD_AVERAGE_COST"));
            if (field != null)
            {
                field.attrValue = lastAvgCost.ToString();
            }
            else
            {
                customFieldList.Add(new UserFieldData
                {
                    attrName = Config.GetConfig("FINALE_CUSTOM_FIELD_AVERAGE_COST"),
                    attrValue = lastAvgCost.ToString()
                });
            }

            //set new list of custom field values on the finale product object
            if (customFieldList.Count() > 0)
            {
                finaleProduct["userFieldDataList"] = JArray.FromObject(customFieldList);
            }

            //make sure product is active
            finaleProduct["statusId"] = "PRODUCT_ACTIVE";

            //update finale
            HttpResponseMessage res = FinaleClientProvider.MakeFinalePostRequest(finaleProductId, "product", finaleProduct, false, channelid);
        }
    }
}