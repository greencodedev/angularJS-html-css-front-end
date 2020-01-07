
using BaseWebApp.Maven.Channels;
using BaseWebApp.Maven.Custom_Fields;
using BaseWebApp.Maven.Products;
using BaseWebApp.Maven.Products.Conditions;
using BaseWebApp.Maven.Sublocations;
using BaseWebApp.Maven.Users;
using BaseWebApp.Maven.Utils;
using BaseWebApp.Maven.Utils.Api;
using BaseWebApp.Maven.Utils.Sql;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;

namespace BaseWebApp.Maven.FinaleInventory
{
    public class FinaleClientProvider
    {
        private static readonly string REPORT_TRASACTION_TIME_COLUMN_NAME = "Receive transaction timestamp";
        private static readonly string REPORT_TRASACTION_TYPE_COLUMN_NAME = "Transaction type";
        private static readonly string REPORT_TRASACTION_DESCRIPTION_COLUMN_NAME = "Transaction description";
        private static readonly string REPORT_PRODUCT_ID_COLUMN_NAME = "Product ID";
        private static readonly string REPORT_QUANTITY_COLUMN_NAME = "Units";
        private static readonly string REPORT_COST_COLUMN_NAME = "Transaction cost per unit";
        private static readonly string REPORT_RECORD_DATE_COLUMN_NAME = "Record date";

        public static HttpResponseMessage MakeFinaleGetRequest(string resource, Channel channel)
        {
            return MakeFinaleGetRequest(null, resource, false, channel);
        }

        public static HttpResponseMessage MakeFinalReportRequest(string endpoint, Channel channel)
        {
            return MakeFinaleGetRequest(null, endpoint, true, channel);
        }

        public static HttpResponseMessage MakeFinaleGetRequest(string finaleId, string resource, bool report, Channel channel)
        {
           
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Cookie("ACCOUNT", Config.GetConfig("FINALE_COSTUM_ACCOUNT"), "/", "app.finaleinventory.com"));
            cookieContainer.Add(new Cookie("JSESSIONID", channel.FinaleToken, "/", "app.finaleinventory.com"));

            string url = "https://app.finaleinventory.com/" + channel.FinaleAccountName + "/";

            if (!report)
            {
                url += "api/";
            }

            url += resource + "/" + (!string.IsNullOrEmpty(finaleId) ? finaleId + "/" : "") + "?cookie=" + channel.FinaleToken;

            return InternanFenaleGetRequest(cookieContainer, resource, url, 0);

        }

        private static HttpResponseMessage InternanFenaleGetRequest(CookieContainer cookieContainer, string resource, string url, int retryCount)
        {

            try
            {
                using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
                using (HttpClient client = new HttpClient(handler))
                {
                    Logger.Log("FINALE CLIENT :: " + resource + " :: " + url);
                    HttpResponseMessage result = client.GetAsync(url).Result;
                    Logger.Log("FINALE CLIENT :: " + resource + " :: " + url + " :: " + result.StatusCode);

                    return result;
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());

                if(retryCount < 2)
                {
                    Thread.Sleep(3000);
                    return InternanFenaleGetRequest(cookieContainer, resource, url, ++retryCount);
                }
                else
                {
                    throw e;
                }
            }
        }

        public static HttpResponseMessage TransactionsReport(DateTime syncFrom, string productName, Channel channel)
        {
            //build url
            string url = Config.GetConfig("REPORT_URL_PART_ONE");

            string fianleAccountName = Config.GetConfig("FINALE_COSTUM_ACCOUNT");
           
            //date to correct format
            string datesatring = syncFrom.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

            //build date filter
            string UrlDateFilter = $"[\"stockHistoryRecordDate\",[\"{datesatring}\",null]]";

            //build product filter
            string ProductFilter = !string.IsNullOrEmpty(productName) ? $",[\"productProductUrl\",\"/{fianleAccountName}/api/product/{productName}\"]" : "";

            //build filter string
            string filter = $"[{UrlDateFilter}{ProductFilter}]";
            
            //convert to byts
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(filter);

            //base 64 encode
            string Base64encode =  System.Convert.ToBase64String(plainTextBytes);

            //url encode
            string UrlEncoded = System.Web.HttpUtility.UrlEncode(Base64encode);

            //final url
            url += UrlEncoded;// + Config.GetConfig("REPORT_URL_PART_TWO");


            return MakeFinalReportRequest(url, channel);

        }

        public static List<TrasactionReportObject> ParseTransactionsReport(List<TransactionReportData> ReportDataList)
        {

            TransactionReportData ReportData = ReportDataList[0];

            Dictionary<string, int> HeaderDetails = new Dictionary<string, int>();

            List<TrasactionReportObject> TrasactionList = new List<TrasactionReportObject>();

            for (int i = 0; i < ReportData.data.Count; i++)
            {

                List<string> row = ReportData.data[i];

                TrasactionReportObject Transaction = new TrasactionReportObject();

                for (int x = 0; x < row.Count; x++)
                {
                    string column = row[x];

                    //first row contains header information
                    if (i < 1)
                    {
                        string col = column.Replace("\n", " ");
                        HeaderDetails.Add(col, x);
                    }
                    else
                    {
                        
                        if (x == HeaderDetails[REPORT_PRODUCT_ID_COLUMN_NAME])
                        {
                            Transaction.FinaleId = column;
                        }

                        if (x == HeaderDetails[REPORT_COST_COLUMN_NAME])
                        {
                            bool convert = double.TryParse(column, out double result);
                            if (convert)
                            {
                                Transaction.CostPerUnit = result;
                            }
                        }

                        if (x == HeaderDetails[REPORT_QUANTITY_COLUMN_NAME])
                        {
                            bool convert = int.TryParse(column, out int result);
                            if (convert)
                            {
                                Transaction.Quantity = result;
                            }
                        }

                        if (x == HeaderDetails[REPORT_TRASACTION_DESCRIPTION_COLUMN_NAME])
                        {
                            Transaction.TransactionDescription = column;
                        }

                        if (x == HeaderDetails[REPORT_TRASACTION_TIME_COLUMN_NAME])
                        {
                            bool convert = DateTime.TryParse(column, out DateTime result);

                            if (convert)
                            {
                                Transaction.ReceivedTimestamp = result;
                            }
                        }

                        if (x == HeaderDetails[REPORT_TRASACTION_TYPE_COLUMN_NAME])
                        {
                            Transaction.TransactionType = column;
                        }

                        if(x == HeaderDetails[REPORT_RECORD_DATE_COLUMN_NAME])
                        {
                            bool convert = DateTime.TryParse(column, out DateTime result);

                            if (convert)
                            {
                                Transaction.RecordDate = result;
                            }
                        }
                    }

                }

                //if it's not the header info
                if (i > 0)
                {
                    TrasactionList.Add(Transaction);
                }
            }

            return TrasactionList;
        }
        public static HttpResponseMessage MakeFinalePostRequest(string resource, JObject data, bool cookieUpdated, int channelId)
        {
            return MakeFinalePostRequest(null, resource, data, cookieUpdated, channelId);
        }

        public static HttpResponseMessage MakeFinalePostRequest(string finaleId, string resource, JObject data, bool cookieUpdated, int channelId)
        {
            Channel channel = ChannelsProvider.GetChannelWithSensitiveData(UsersProvider.GetCurrentAccountId(), channelId);

            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Cookie("ACCOUNT", Config.GetConfig("FINALE_COSTUM_ACCOUNT"), "/", "app.finaleinventory.com"));
            cookieContainer.Add(new Cookie("JSESSIONID", channel.FinaleToken, "/", "app.finaleinventory.com"));

            string url = "https://app.finaleinventory.com/" + Config.GetConfig("FINALE_COSTUM_ACCOUNT") + "/api/" + resource + "/" + (!string.IsNullOrEmpty(finaleId) ? finaleId + "/" : "") + "?cookie=" + channel.FinaleToken;


            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (HttpClient client = new HttpClient(handler))
            {
                data.Add("sessionSecret", channel.FinaleToken);
                var content = new StringContent(data.ToString());

                Logger.Log("FINALE CLIENT :: " + resource + " :: " + url);
                HttpResponseMessage result = client.PostAsync(url, content).Result;
                Logger.Log("FINALE CLIENT :: " + resource + " :: " + url + " :: " + result.StatusCode);

                if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    
                    if (!cookieUpdated)
                    {
                        if (UpdateCookie(channel))
                        {
                            MakeFinalePostRequest(resource, data, true, channelId);
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

                return result;
            }

        }

        public static HttpResponseMessage MakeFinalePutRequest(string resource, JObject data, bool cookieUpdated, int channelId)
        {
            Channel channel = ChannelsProvider.GetChannelWithSensitiveData(UsersProvider.GetCurrentAccountId(), channelId);

            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Cookie("ACCOUNT", Config.GetConfig("FINALE_COSTUM_ACCOUNT"), "/", "app.finaleinventory.com"));
            cookieContainer.Add(new Cookie("JSESSIONID", channel.FinaleToken, "/", "app.finaleinventory.com"));

            string url = "https://app.finaleinventory.com/" + Config.GetConfig("FINALE_COSTUM_ACCOUNT") + "/api/" + resource + "/?cookie=" + channel.FinaleToken;


            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (HttpClient client = new HttpClient(handler))
            {
                data.Add("sessionSecret", channel.FinaleToken);
                var content = new StringContent(data.ToString());

                Logger.Log("FINALE CLIENT :: " + resource + " :: " + url);
                HttpResponseMessage result = client.PutAsync(url, content).Result;
                Logger.Log("FINALE CLIENT :: " + resource + " :: " + url + " :: " + result.StatusCode);

                if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (!cookieUpdated)
                    {
                        if (UpdateCookie(channel))
                        {
                            MakeFinalePostRequest(resource, data, true, channelId);
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

                return result;
            }
        }


        public static List<Product> SyncProducts(int channelId)
        {
            return SyncProducts(false, channelId);
        }

        public static List<Category> SyncCategory(int channelId)
        {
            return SyncCategory(false, channelId);
        }

        public static List<Condition> GetConditions(int channelId)
        {
            return GetConditions(false, channelId);
        }

        public static List<Sublocation> GetSublocations(int channelId)
        {
            return GetSublocations(false, channelId);
        }

        public static List<string> GetCustomFields(Channel channel , string name = null)
        {
            return SyncCostomField(false, channel);
        }

        public static JObject GetProduct(bool cookieUpdated, int channelId)
        {
            JObject respons = new JObject();

            Channel channel = ChannelsProvider.GetChannelWithSensitiveData(UsersProvider.GetCurrentAccountId(), channelId);

            HttpResponseMessage result = MakeFinaleGetRequest("product", channel);

            if (result.IsSuccessStatusCode)
            {
                string content = result.Content.ReadAsStringAsync().Result;
                respons = JObject.Parse(content);
            }
            else if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (!cookieUpdated)
                {
                    if (UpdateCookie(channel))
                    {
                        GetProduct(true, channelId);
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

            return respons;
        }

        private static List<Product> SyncProducts(bool cookieUpdated, int channelId)
        {
            List<Product> list = new List<Product>();
            List<Condition> conditions = ConditionProvider.GetAllConditions();

            Channel channel = ChannelsProvider.GetChannelWithSensitiveData(UsersProvider.GetCurrentAccountId(), channelId);

            HttpResponseMessage result = MakeFinaleGetRequest("product", channel);

            if (result.IsSuccessStatusCode)
            {
                string content = result.Content.ReadAsStringAsync().Result;
                FinaleProductsResponse respons = JsonConvert.DeserializeObject<FinaleProductsResponse>(content);
                Dictionary<string, Category> categories = CategoryProvider.SearchCategory(new SearchCriteria()).ToDictionary(x => x.AttrName);

                //pull all dynamic custom fields for this account
                List<DynamicCustomField> DynamicCustomFields = (List<DynamicCustomField>)DynamicCustomFieldsProvider.GetCustomFields(UsersProvider.GetCurrentAccountId(), null).Data;

                //populate product custom fields from the product
                List<ProductCustomField> ProductCustomFields = new List<ProductCustomField>();

                int AccountId = UsersProvider.GetCurrentAccountId();

                for (int i = 0; i < respons.productId.Count; i++)
                {
                    Product product = new Product
                    {
                        FinaleId = respons.productId[i],
                        Name = respons.internalName[i] != null ? respons.internalName[i] : null,
                        //Description = respons.longDescription[i] ?? null,
                        Category = respons.userCategory[i] != null ? categories[respons.userCategory[i]] : null,
                        Status = respons.statusId[i],
                        Url = respons.productUrl[i],
                        Cost = (respons.cost[i] != null ? respons.cost[i] : 0)
                    };

                    //if(respons.priceList[i].Count() > 0)
                    //{
                        double? price = respons.priceList[i].Find(x => x.productPriceTypeId == "LIST_PRICE")?.price;
                        double? casePrice = respons.priceList[i].Find(x => x.productPriceTypeId == "LIST_CASE_PRICE")?.price;
                        product.Price = (price != null ? price : 0);
                        product.CasePrice = (casePrice != null ? casePrice : 0);
                    //}

                    List<UserFieldData> customFields = respons.userFieldDataList[i];
                    UserFieldData conditionField = customFields.Find(x => x.attrName == Config.GetConfig("FINALE_CUSTOM_FIELD_CONDITION"));
                    if(conditionField != null)
                    {
                        Condition cond = conditions.Find(x => x.Name == conditionField.attrValue);
                        product.Condition = cond;
                    }
                    
                    UserFieldData ncs = customFields.Find(x => x.attrName == Config.GetConfig("FINALE_CUSTOM_FIELD_NCS"));
                    if (ncs != null)
                    {
                        product.Ncs = ncs.attrValue;
                    }

                    UserFieldData averagecost = customFields.Find(x => x.attrName == Config.GetConfig("FINALE_CUSTOM_FIELD_AVERAGE_COST"));

                    if (averagecost != null)
                    {
                        product.AverageCost = averagecost.attrValue;
                    }

                    //loop dynamic custom fiedls
                    if (DynamicCustomFields != null && DynamicCustomFields.Any())
                    {
                        foreach (DynamicCustomField DynamicCustomField in DynamicCustomFields)
                        {
                            UserFieldData Field = customFields.Find(x => x.attrName == DynamicCustomField.FinaleCustomFieldId && !string.IsNullOrEmpty(x.attrValue));

                            //if product has this custom field with a value
                            if (Field != null)
                            {
                                product.CustomFields.Add(new ProductCustomField {
                                    CustomFieldId = DynamicCustomField.CustomFieldId,
                                    Value = Field.attrValue,
                                    AccountId = AccountId
                                });

                            }
                            
                        }
                    }

                    list.Add(product);
                }
            }
            else if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (!cookieUpdated)
                {
                    if (UpdateCookie(channel))
                    {
                        SyncProducts(true, channelId);
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
               
            return list;

        }

        public static Product GetSingleProduct(string id, bool cookieUpdated, int channelId)
        {
            Product product = new Product();
            List<Condition> conditions = ConditionProvider.GetAllConditions();

            Channel channel = ChannelsProvider.GetChannelWithSensitiveData(UsersProvider.GetCurrentAccountId(), channelId);

            HttpResponseMessage result = MakeFinaleGetRequest("product/" + id, channel);

            if (result.IsSuccessStatusCode)
            {
                string content = result.Content.ReadAsStringAsync().Result;
                FinaleProductResponse respons = JsonConvert.DeserializeObject<FinaleProductResponse>(content);
                Dictionary<string, Category> categories = CategoryProvider.SearchCategory(new SearchCriteria()).ToDictionary(x => x.AttrName);

                product = new Product
                {
                    FinaleId = respons.productId,
                    Name = respons.internalName != null ? respons.internalName : null,
                    //Description = respons.longDescription ?? null,
                    Category = respons.userCategory != null ? categories[respons.userCategory] : null,
                    Status = respons.statusId,
                    Url = respons.productUrl,
                    Cost = (respons.cost != null ? respons.cost : 0)
                };

                //if(respons.priceList[i].Count() > 0)
                //{
                double? price = respons.priceList.Find(x => x.productPriceTypeId == "LIST_PRICE")?.price;
                double? casePrice = respons.priceList.Find(x => x.productPriceTypeId == "LIST_CASE_PRICE")?.price;
                product.Price = (price != null ? price : 0);
                product.CasePrice = (casePrice != null ? casePrice : 0);
                //}

                List<UserFieldData> customFields = respons.userFieldDataList;
                UserFieldData conditionField = customFields.Find(x => x.attrName == Config.GetConfig("FINALE_CUSTOM_FIELD_CONDITION"));
                if (conditionField != null)
                {
                    Condition cond = conditions.Find(x => x.Name == conditionField.attrValue);
                    product.Condition = cond;
                }

                UserFieldData ncs = customFields.Find(x => x.attrName == Config.GetConfig("FINALE_CUSTOM_FIELD_NCS"));
                if (ncs != null)
                {
                    product.Ncs = ncs.attrValue;
                }

                UserFieldData averagecost = customFields.Find(x => x.attrName == Config.GetConfig("FINALE_CUSTOM_FIELD_AVERAGE_COST"));

                if (averagecost != null)
                {
                    product.AverageCost = averagecost.attrValue;
                }

            }
            else if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (!cookieUpdated)
                {
                    if (UpdateCookie(channel))
                    {
                        GetSingleProduct(id, true, channelId);
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

            return product;

        }

        public static JObject GetProductObject(string id, bool cookieUpdated, int channelId)
        {
            JObject product = new JObject();

            Channel channel = ChannelsProvider.GetChannelWithSensitiveData(UsersProvider.GetCurrentAccountId(), channelId);

            HttpResponseMessage result = MakeFinaleGetRequest("product/" + id, channel);

            if (result.IsSuccessStatusCode)
            {
                product = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            }
            
            else if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (!cookieUpdated)
                {
                    if (UpdateCookie(channel))
                    {
                        GetProductObject(id, true, channelId);
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

            return product;
        }

        private static List<Category> SyncCategory(bool cookieUpdated, int channelId)
        {
            ProviderResponse providerRespons = new ProviderResponse();
            List<Category> list = new List<Category>();

            Channel channel = ChannelsProvider.GetChannelWithSensitiveData(UsersProvider.GetCurrentAccountId(), channelId);

            HttpResponseMessage result = MakeFinaleGetRequest("customization", channel);

            if (result.IsSuccessStatusCode)
            {
                string content = result.Content.ReadAsStringAsync().Result;
                FinaleCategoryResponse respons = JsonConvert.DeserializeObject<FinaleCategoryResponse>(content);
                        
                foreach (FinaleCategory cat in respons.productUserCategoryList)
                {
                    Category category = new Category
                    {
                        AttrName = cat.attrName,
                        Label = cat.label
                    };

                    if (cat.guiOptions != null)
                    {
                        category.SortOrder = cat.guiOptions.sortOrder;
                    }

                    list.Add(category);
                }
            }
            else if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (!cookieUpdated)
                {
                    if (UpdateCookie(channel))
                    {
                        SyncCategory(true, channelId);
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

            return list;
        }

        public static List<Condition> GetConditions(bool cookieUpdated, int channelId)
        {
            List<Condition> conditionList = new List<Condition>();

            Channel channel = ChannelsProvider.GetChannelWithSensitiveData(UsersProvider.GetCurrentAccountId(), channelId);

            HttpResponseMessage result = MakeFinaleGetRequest("customization", channel);

            if (result.IsSuccessStatusCode)
            {
                string content = result.Content.ReadAsStringAsync().Result;
                FinaleCategoryResponse response = JsonConvert.DeserializeObject<FinaleCategoryResponse>(content);

                ProductType prodcutType = response.productTypeList[0];

                foreach (string conditionName in prodcutType.userFieldDefList.Find(x => x.attrName == Config.GetConfig("FINALE_CUSTOM_FIELD_CONDITION")).optionList)
                {
                    conditionList.Add(new Condition
                    {
                        Name = conditionName
                    });
                }

            }
            else if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (result.IsSuccessStatusCode)
                {
                    if (UpdateCookie(channel))
                    {
                        GetConditions(true, channelId);
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

            return conditionList;
        }

        public static List<Sublocation> GetSublocations(bool cookieUpdated, int channelId)
        {
            List<Sublocation> sublocationList = new List<Sublocation>();

            Channel channel = ChannelsProvider.GetChannelWithSensitiveData(UsersProvider.GetCurrentAccountId(), channelId);

            HttpResponseMessage result = MakeFinaleGetRequest("facility", channel);

            if (result.IsSuccessStatusCode)
            {
                string content = result.Content.ReadAsStringAsync().Result;
                FinaleSublocationResponse response = JsonConvert.DeserializeObject<FinaleSublocationResponse>(content);

                List<Sublocation> currentSublocations = SublocationProvider.GetSublocations(new SearchCriteria(), channelId);

                Regex regex = new Regex(@"\/([0-9]+)(?=[^\/]*$)");

                for (int i = 0; i < response.facilityId.Count; i++)
                {
                    Enum.TryParse(response.statusId[i], out SublocationStatus status);

                    Sublocation sublocation = new Sublocation
                    {
                        FinaleSublocationId = response.facilityId[i],
                        Name = response.facilityName[i] != null ? response.facilityName[i] : null,
                        Status = status,
                        Url = response.facilityUrl[i]
                    };

                    if(response.parentFacilityUrl[i] != null)
                    {
                        if(regex.Match(response.parentFacilityUrl[i]).Groups[1].Value != null && currentSublocations.Find(x => x.FinaleSublocationId == regex.Match(response.parentFacilityUrl[i]).Groups[1].Value) != null)
                        {
                            sublocation.ParentLocation = currentSublocations.Find(x => x.FinaleSublocationId == regex.Match(response.parentFacilityUrl[i]).Groups[1].Value);
                        }
                        else
                        {
                            currentSublocations = (List<Sublocation>)SublocationProvider.InsertOrUpdateSublocationsFromFinale(new List<Sublocation> {
                                new Sublocation
                                {
                                    FinaleSublocationId = regex.Match(response.parentFacilityUrl[i]).Groups[1].Value,
                                    Status = SublocationStatus.FACILITY_ACTIVE,
                                    Url = response.parentFacilityUrl[i]
                                }
                            }, channelId).Data;

                            sublocation.ParentLocation = currentSublocations.Find(x => x.FinaleSublocationId == regex.Match(response.parentFacilityUrl[i]).Groups[1].Value);
                        }
                    };

                    sublocationList.Add(sublocation);
                }
            }
            else if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (!cookieUpdated)
                {
                    if (UpdateCookie(channel))
                    {
                        GetConditions(true, channelId);
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

            return sublocationList;
        }

        public static Dictionary<string, int> GetStockForProducts(List<string> productids, bool cookieUpdated, int channelId)
        {
            Dictionary<string, int> ProductsStock = new Dictionary<string, int>();

            Channel channel =  ChannelsProvider.GetChannelWithSensitiveData(UsersProvider.GetCurrentAccountId(), channelId);

            HttpResponseMessage result = MakeFinaleGetRequest("inventoryitem/", channel);

            if (result.IsSuccessStatusCode)
            {
                string content = result.Content.ReadAsStringAsync().Result;

                FinaleInventoryItemResponse response = JsonConvert.DeserializeObject<FinaleInventoryItemResponse>(content);

                for (int i = 0; i < response.productId.Count; i++)
                {
                    foreach (var productid in productids)
                    {
                        if(response.productId[i] == productid && !string.IsNullOrEmpty(response.parentFacilityUrl[i]))
                        {
                            if (!ProductsStock.ContainsKey(productid))
                            {
                                ProductsStock.Add(productid, response.quantityOnHand[i]);
                            }
                            else
                            {
                                ProductsStock[productid] += response.quantityOnHand[i];
                            }
                        }
                    }
                }
            }
            else if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (!cookieUpdated)
                {
                    if (UpdateCookie(channel))
                    {
                        GetStockForProducts(productids, true, channel.ChannelId);
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
            return ProductsStock;
        }

        public static Dictionary<string, int> GetStockPerLocation(Product product, bool cookieUpdated, int channelId)
        {
            Dictionary<string, int> stock = new Dictionary<string, int>() 
            {
                { 
                    "Total", 0
                }
            };

            Channel channel = ChannelsProvider.GetChannelWithSensitiveData(UsersProvider.GetCurrentAccountId(), channelId);

            HttpResponseMessage result = MakeFinaleGetRequest("inventoryitem/", channel);

            if (result.IsSuccessStatusCode)
            {
                string content = result.Content.ReadAsStringAsync().Result;

                FinaleInventoryItemResponse response = JsonConvert.DeserializeObject<FinaleInventoryItemResponse>(content);
                //int index = response.productId.IndexOf(product.FinaleId.ToString());

                for (int i = 0; i < response.productId.Count(); i++)
                {
                    if (response.productId[i] == product.FinaleId.ToString() && !string.IsNullOrEmpty(response.parentFacilityUrl[i]))
                    {
                        stock.Add(response.facilityUrl[i], response.quantityOnHand[i]);
                        stock["Total"] += response.quantityOnHand[i];
                        /*if (sublocation == null || sublocation.Url == response.facilityUrl[i])
                        {
                            stock += response.quantityOnHand[i];
                        }*/
                    }
                }

            }
            else if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (!cookieUpdated)
                {
                    if (UpdateCookie(channel))
                    {
                        GetStockPerLocation(product, true, channelId);
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

            return stock;
        }

        /*public static int GetStock(Product product, Sublocation sublocation, bool cookieUpdated)
        {
            int stock = 0;

            HttpResponseMessage result = MakeFinaleGetRequest("inventoryitem");

            if (result.IsSuccessStatusCode)
            {
                string content = result.Content.ReadAsStringAsync().Result;

                FinaleInventoryItemResponse response = JsonConvert.DeserializeObject<FinaleInventoryItemResponse>(content);

                //int index = response.productId.IndexOf(product.FinaleId.ToString());

                for (int i = 0; i < response.productId.Count(); i++)
                {
                    if (response.productId[i] == product.FinaleId.ToString())
                    {
                        if (sublocation == null || sublocation.Url == response.facilityUrl[i])
                        {
                            stock += response.quantityOnHand[i];
                        }
                    }
                }

            }
            else if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (!cookieUpdated)
                {
                    if (UpdateCookie())
                    {
                        GetStock(product, sublocation, true);
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

            return stock;
        }*/

        /*private static Dictionary<string, List<string>> GetCustomFields(bool cookieUpdated, string name)
        {
            //List<FinalCustomField> customFields = new List<FinalCustomField>();
            Dictionary<string, List<string>> customFieldList = new Dictionary<string, List<string>>();

            string cookie = GetCookie();

            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Cookie("ACCOUNT", Config.GetConfig("FINALE_COSTUM_ACCOUNT"), "/", "app.finaleinventory.com"));
            cookieContainer.Add(new Cookie("JSESSIONID", cookie, "/", "app.finaleinventory.com"));
            string url = "https://app.finaleinventory.com/" + Config.GetConfig("FINALE_COSTUM_ACCOUNT") + "/api/customization/?cookie=" + cookie;

            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (HttpClient client = new HttpClient(handler))
            {
                Logger.Log("FINALE CLIENT :: SyncCostomField :: " + url);
                HttpResponseMessage result = client.GetAsync(url).Result;
                Logger.Log("FINALE CLIENT :: SyncCostomField :: " + url + " :: " + result.StatusCode);
                
                if (result.IsSuccessStatusCode)
                {
                    string content = result.Content.ReadAsStringAsync().Result;
                    FinaleCategoryResponse respons = JsonConvert.DeserializeObject<FinaleCategoryResponse>(content);
                        
                    ProductType goodProdcutType = respons.productTypeList[0];

                    if(name != null)
                    {
                        customFieldList.Add(name, goodProdcutType.userFieldDefList.Find(x => x.attrName == name).optionList.ConvertAll(n => n.ToLower()));
                        //customFields.Add(goodProdcutType.userFieldDefList.Find(x => x.attrName == name));
                    }
                    else
                    {
                        foreach(FinalCustomField customFieldType in goodProdcutType.userFieldDefList)
                        {
                            customFieldList.Add(customFieldType.attrName, customFieldType.optionList.ConvertAll(n => n.ToLower()));
                        }

                        //customFields = goodProdcutType.userFieldDefList;
                    }
                }
                else if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (!cookieUpdated)
                    {
                        if (UpdateCookie())
                        {
                            GetCustomFields(true, name);
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
                
                return customFieldList;
            }
        }*/

        private static List<string> SyncCostomField(bool cookieUpdated, Channel channel)
        {
            List<string> list = new List<string>();
            

            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Cookie("ACCOUNT", Config.GetConfig("FINALE_COSTUM_ACCOUNT"), "/", "app.finaleinventory.com"));
            cookieContainer.Add(new Cookie("JSESSIONID", channel.FinaleToken, "/", "app.finaleinventory.com"));
            string url = "https://app.finaleinventory.com/" + Config.GetConfig("FINALE_COSTUM_ACCOUNT") + "/api/customization/?cookie=" + channel.FinaleToken;

            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (HttpClient client = new HttpClient(handler))
            {
                Logger.Log("FINALE CLIENT :: SyncCostomField :: " + url);
                HttpResponseMessage result = client.GetAsync(url).Result;
                Logger.Log("FINALE CLIENT :: SyncCostomField :: " + url + " :: " + result.StatusCode);

                if (result.IsSuccessStatusCode)
                {
                    string content = result.Content.ReadAsStringAsync().Result;
                    FinaleCategoryResponse respons = JsonConvert.DeserializeObject<FinaleCategoryResponse>(content);

                    ProductType goodProdcutType = respons.productTypeList[0];
                    /*FinalCondition condition = goodProdcutType.userFieldDefList.Find(x => x.attrName == Config.GetConfig("FINALE_CUSTOM_FIELD_CONDITION"));
                    if (condition != null)
                    {
                        list = condition.optionList;
                    }*/
                }
                else if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (!cookieUpdated)
                    {
                        if (UpdateCookie(channel))
                        {
                            SyncCostomField(true, channel);
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

                return list;
            }
        }

        public static bool UpdateCookie(Channel channel)
        {
            var dict = new Dictionary<string, string>
            {
                { "username", channel.FinaleUsername},
                { "password", channel.FinalePassword },
            };
            

            string url = "https://app.finaleinventory.com/"+ channel.FinaleAccountName +  "/api/auth";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    Logger.Log("FINALE CLIENT :: UpdateCookie :: " + url);
                    var result = client.PostAsync(url, new FormUrlEncodedContent(dict)).Result;
                    Logger.Log("FINALE CLIENT :: UpdateCookie :: " + url + " :: " + result.StatusCode);


                    if (result.IsSuccessStatusCode)
                    {
                        List<string> values = result.Headers.GetValues("Set-Cookie").ToList();
                        
                        string newCookiePath = values.Find(x => x.StartsWith("JSESSIONID"));

                        if (!string.IsNullOrEmpty(newCookiePath))
                        {
                            int start = newCookiePath.IndexOf("=") + 1;
                            int end = newCookiePath.IndexOf(";", start);
                            string newCookie = newCookiePath.Substring(start, end - start);

                            InsertCookie(newCookie); //CAN REMOVE THIS LATER just here just in case.

                            channel.FinaleToken = newCookie;

                            ChannelsProvider.UpdateChannelSensetiveData(channel, UsersProvider.GetCurrentAccountId());

                            
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Logger.Log(e.ToString());
                    return false;
                }
            }
            
            return true;
        }

        public static string GetCookie()
        {
            string cookie = "";
            
            string query = @"select * from Settings where 1 = 1 and AccountId = @AccountId";

            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());

            using (Sql sql = new Sql())
            using (SqlReader reader = sqlQuery.ExecuteReader(sql, query))
            {
                if (reader.HasNext())
                {
                    cookie = reader.GetString("Cookie");
                }
            }

            return cookie;
        }

        public static string GetCookie(int channelId)
        {
            Channel channel =(Channel)ChannelsProvider.GetChannelOrChannels(UsersProvider.GetCurrentAccountId(), channelId).Data;

            if(channel != null)
            {
                return channel.FinaleToken;
            }
            return null;
        }
        public static void InsertCookie(string cookie)
        {
            string query = "update Settings set Cookie = @cookie where id = 1 and AccountId = @AccountId";

            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.AddParam("@cookie", cookie);
            sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());

            using (Sql sql = new Sql())
            {
                sqlQuery.ExecuteInsert(sql, query);
            }
        }
    }
}