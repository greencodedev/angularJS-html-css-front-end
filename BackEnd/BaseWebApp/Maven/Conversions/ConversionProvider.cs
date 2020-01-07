using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using BaseWebApp.Maven.FinaleInventory;
using BaseWebApp.Maven.Products;
using BaseWebApp.Maven.Products.Conditions;
using BaseWebApp.Maven.Sublocations;
using BaseWebApp.Maven.Users;
using BaseWebApp.Maven.Utils;
using BaseWebApp.Maven.Utils.Api;
using BaseWebApp.Maven.Utils.Email;
using BaseWebApp.Maven.Utils.Sql;
using BaseWebApp.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BaseWebApp.Maven.Conversions
{
    public class ConversionProvider
    {
        public static ProviderResponse CreateConversion(Conversion conversion, int channelId)
        {
            ProviderResponse response = new ProviderResponse();

            response.Messages.AddRange(ValidateConversion(conversion, channelId));

            if(response.Messages.Count() > 0)
            {
                response.Success = false;
                return response;
            }

            try
            {
                if (conversion.FromProduct.Condition.ConditionId != conversion.ToProduct.Condition.ConditionId || conversion.FromProduct.ProductId != conversion.ToProduct.ProductId)
                {
                    if (conversion.CreateNewProduct)
                    {
                        CreateProductIfNotExists(conversion.ToProduct, channelId);
                    }
                }

                Task.Run(() => CreateConversionAsync(conversion, channelId));

                response.Data = conversion;
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());

                response.Messages.Add("An error has occurred, please try again later");
            }

            return response;
        }

        private static void CreateConversionAsync(Conversion conversion, int channelId)
        {
            ThreadProperties.SetIsProcess(true);
            string status = "1 - Start";
            Logger.Log($"CONVERT :: STATUS :: {status}");

            try
            {
                if (conversion.FromProduct.Condition.ConditionId != conversion.ToProduct.Condition.ConditionId || conversion.FromProduct.ProductId != conversion.ToProduct.ProductId)
                {
                    // Make 2 stock conversions
                    JObject data = conversion.ConvertToFinaleObject();

                    status = "6 - Convert - Create Conversion - Start";
                    Logger.Log($"CONVERT :: STATUS :: {status}");

                    HttpResponseMessage finaleConversion = FinaleInventory.FinaleClientProvider.MakeFinalePostRequest("inventoryvariance", data, false, channelId);

                    status = "7 - Convert - Create Conversion - End";
                    Logger.Log($"CONVERT :: STATUS :: {status}");

                    status = "8 - Convert - Set Cost - Start";
                    Logger.Log($"CONVERT :: STATUS :: {status}");

                    SetCostFromConversion(conversion, channelId);

                    status = "9 - Convert - Set Cost - End";
                    Logger.Log($"CONVERT :: STATUS :: {status}");
                }

                // If sublocations are not the same, create a transfer
                if (conversion.FromSublocation.SublocationId != conversion.ToSublocation.SublocationId)
                {
                    status = "10 - Convert - Create Transafer - Start";
                    Logger.Log($"CONVERT :: STATUS :: {status}");

                    CreateTransfer(conversion, channelId);

                    status = "11 - Convert - Create Transafer - End";
                    Logger.Log($"CONVERT :: STATUS :: {status}");
                }

                status = "12 - Convert - Save Log - Start";
                Logger.Log($"CONVERT :: STATUS :: {status}");

                SaveConversionLog(conversion);

                status = "13 - Convert - Save Log - End";
                Logger.Log($"CONVERT :: STATUS :: {status}");

            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());

                string body = $@"<html>
                                    <h3>Conversion Details:</h3>
                                    <p>User: {conversion.user?.FirstName} {conversion.user?.LastName} ({conversion.user?.Initials})</p>
                                    <p>From Product: {conversion.FromProduct.FinaleId} - {conversion.FromProduct.Name} ({conversion.FromProduct.Condition?.Name})</p>
                                    <p>To Product: {conversion.ToProduct.FinaleId} - {conversion.ToProduct.Name} ({conversion.ToProduct.Condition?.Name})</p>
                                    <p>Quantity: {conversion.Quantity}</p>
                                    <p>PO Number: {conversion.PoNumber}</p>
                                    <p>Note: {conversion.Note}</p>
                                    <p>Error Message: {e.Message}</p>
                                    <span>Error Details: {e.ToString()}</span>
                                 </html>";

                EmailClient.SendEmail(Config.GetConfig("EMAIL_TO_EMAIL"), "Error in product conversion - " + status, body);
            }
        }

        private static void CreateTransfer(Conversion conversion, int channelId)
        {
            JObject data = conversion.CreateFinaleTransfer();

            FinaleInventory.FinaleClientProvider.MakeFinalePostRequest("inventorytransfer", data, false, channelId);
        }

        private static void CreateProductIfNotExists(Product product, int channelId)
        {
            product.FinaleId = null;
            product.Url = null;
            Match productName = Regex.Match(product.Name, @".+?(?=\([^)]*\))");
            product.Name = (productName.Length > 0 ? productName.ToString() : product.Name + " ") + "(" + product.Condition.Code + ")";
            //product.Name = product.Ncs + " (" + product.Condition.Code + ")";
            ProductsProvider.CreateFinaleProduct(product, channelId);
        }

        public static void SetCostFromConversion(Conversion conversion, int channelId)
        {
            DateTime date = DateTime.Now.AddDays(-2);

            //sync custom avg cost
            TransactionsReportProvider.sync(false, conversion.FromProduct.FinaleId, date, channelId);
            TransactionsReportProvider.sync(false, conversion.ToProduct.FinaleId, date, channelId);

            Product fromProd = FinaleClientProvider.GetSingleProduct(conversion.FromProduct.FinaleId, false, channelId);
            Product toProd = FinaleClientProvider.GetSingleProduct(conversion.ToProduct.FinaleId, false, channelId);


            double? originalCost = fromProd.Cost;
            double? destinationCost = toProd.Cost;
            int destinationQty = GetTotalQuantity(conversion.ToProduct, channelId);
            double? totalInventoryValue = originalCost * conversion.Quantity + destinationCost * destinationQty;
            double? newAverageCost = totalInventoryValue / (conversion.Quantity + destinationQty);
            double? roundedCost = newAverageCost.HasValue ? (double?)Math.Round(newAverageCost.Value, 2) : null;

            conversion.ToProduct.CostFromConversion = roundedCost;
            conversion.ToProduct.LastCostInfo = "Date: " + DateTime.Now.ToString() + ", Source Product ID: " + conversion.FromProduct.FinaleId + ", Qty: " + conversion.Quantity + ", Initials: " + ThreadProperties.GetUser().Initials;

            ProductsProvider.UpdateFinaleCost(conversion.ToProduct, channelId);

            //save in new log table
            double.TryParse(fromProd.AverageCost, out double origProductAvgCost);
            double.TryParse(toProd.AverageCost, out double currAvgCost);

            string description = $"User {ThreadProperties.GetUser().Initials}, From {conversion.FromProduct.FinaleId}";

            TransactionsReportProvider.CalcualteCostOnConvert(conversion.ToProduct.FinaleId, origProductAvgCost, conversion.Quantity, description, destinationQty, currAvgCost, channelId);
        }

        private static int GetTotalQuantity(Product product, int channelId)
        {
            return FinaleClientProvider.GetStockPerLocation(product, false, channelId)["Total"];
        }

        public static ProviderResponse SaveConversionLog(Conversion conversion)
        {
            ProviderResponse response = new ProviderResponse();

            using (Sql sql = new Sql())
            {
                string query = @"INSERT INTO Conversions
                                            (AccountId, FromProductId, ToProductId, FromSublocationId, ToSublocationId, Note, 
                                            Quantity, PoNumber, UserId, NewProduct, ConvertType,
                                            FromProductFinaleId, FromProductName, FromProductConditionCode, FromProductCategoryName,
                                            ToProductFinaleId, ToProductName, ToProductConditionCode, ToProductCategoryName) 
                                         VALUES 
                                            (@AccountId, @FromProductId, @ToProductId, @FromSublocationId, @ToSublocationId, @Note, 
                                            @Quantity, @PoNumber, @UserId, @NewProduct, @ConvertType,
                                            @FromProductFinaleId, @FromProductName, @FromProductConditionCode, @FromProductCategoryName,
                                            @ToProductFinaleId, @ToProductName, @ToProductConditionCode, @ToProductCategoryName) ";

                SqlQuery sqlQuery = new SqlQuery();
                sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());
                sqlQuery.AddParam("@FromProductId", conversion.FromProduct.ProductId);
                sqlQuery.AddParam("@ToProductId", conversion.ToProduct.ProductId);
                sqlQuery.AddParam("@FromSublocationId", conversion.FromSublocation.SublocationId);
                sqlQuery.AddParam("@ToSublocationId", conversion.ToSublocation.SublocationId);
                sqlQuery.AddParam("@Note", conversion.Note);
                sqlQuery.AddParam("@Quantity", conversion.Quantity);
                sqlQuery.AddParam("@PoNumber", conversion.PoNumber);
                sqlQuery.AddParam("@UserId", ThreadProperties.GetUser().Id);
                sqlQuery.AddParam("@NewProduct", (conversion.CreateNewProduct ? 1 : 0));
                sqlQuery.AddParam("@ConvertType", conversion.ConvertType);
                sqlQuery.AddParam("@FromProductFinaleId", conversion.FromProduct.FinaleId);
                sqlQuery.AddParam("@FromProductName", conversion.FromProduct.Name);
                sqlQuery.AddParam("@FromProductConditionCode", conversion.FromProduct.Condition?.Code);
                sqlQuery.AddParam("@FromProductCategoryName", conversion.FromProduct.Category?.Label);
                sqlQuery.AddParam("@ToProductFinaleId", conversion.ToProduct.FinaleId);
                sqlQuery.AddParam("@ToProductName", conversion.ToProduct.Name);
                sqlQuery.AddParam("@ToProductConditionCode", conversion.ToProduct.Condition?.Code);
                sqlQuery.AddParam("@ToProductCategoryName", conversion.ToProduct.Category?.Label);

                sqlQuery.ExecuteInsert(sql, query);
            }

            return response;
        }

        public static ProviderResponse GetConversionLogs(SearchCriteria search)
        {
            ProviderResponse response = new ProviderResponse();
            ListReponse listReponse = new ListReponse();

            if (search == null)
            {
                search = new SearchCriteria();
            }

            search.Status = true;
            search.Paginate = true;

            listReponse.List = searchConversionLogs(search);

            search.Paginate = false;
            listReponse.TotalCount = searchConversionLogs(search).Count;

            response.Data = listReponse;

            return response;
        }

        public static List<Conversion> searchConversionLogs(SearchCriteria search)
        {
            List<Conversion> conversions = new List<Conversion>();


            using (Sql sql = new Sql())
            {
                SqlQuery sqlQuery = new SqlQuery();

                string query = @"SELECT c.*, 
                                    frP.ProductId frProductId, frC.Name frCName, 
                                    ifnull(c.FromProductName, frP.Name) frProductName, 
                                    ifnull(c.FromProductConditionCode, frC.Code) frCCode, 
                                    ifnull(c.FromProductCategoryName, frCat.Label) frCat,  
                                    ifnull(FromProductFinaleId, frP.FinaleId) frFinaleId, 
                                    toP.ProductId toProductId, toC.Name toCName,
                                    ifnull(c.ToProductName, toP.Name) tProductName, 
                                    ifnull(c.ToProductConditionCode, toC.Code) toCCode,  
                                    ifnull(c.ToProductCategoryName, toCat.Label) toCat, 
                                    ifnull(c.ToProductFinaleId, toP.FinaleId) toFinaleId, 
                                    frS.SublocationId frSublocationId, frS.Name frSublocationName, 
                                    toS.SublocationId toSublocationId, toS.Name toSublocationName, 
                                    u.FirstName, u.LastName, u.Initials 
                                 FROM Conversions c
                                 LEFT JOIN Products frP ON (frP.ProductId = c.FromProductId) 
                                 LEFT JOIN Products toP ON (toP.ProductId = c.ToProductId) 
                                 LEFT JOIN Conditions frC ON (frC.ConditionId = frP.ConditionId) 
                                 LEFT JOIN Conditions toC ON (toC.ConditionId = toP.ConditionId) 
                                 LEFT JOIN Categories frCat ON (frCat.CategoryId = frP.CategoryId) 
                                 LEFT JOIN Categories toCat ON (toCat.CategoryId = toP.CategoryId) 
                                 LEFT JOIN Sublocations frS ON (frS.SublocationId = c.FromSublocationId) 
                                 LEFT JOIN Sublocations toS ON (toS.SublocationId = c.ToSublocationId) 
                                 LEFT JOIN idty_users u ON (u.UserId = c.UserId) 
                                 WHERE c.AccountId = @AccountId ";

                sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());

                if (search.Id != null)
                {
                    query += "AND ConversionId = @ConversionId ";
                    sqlQuery.AddParam("@ConversionId", search.Id);
                }

                if (!string.IsNullOrEmpty(search.UserId))
                {
                    query += "AND c.UserId = @UserId ";
                    sqlQuery.AddParam("@UserId", search.UserId);
                }

                if (search.FromDate != null && search.ToDate != null)
                {
                    query += "AND Created BETWEEN @Start AND @End ";
                    sqlQuery.AddParam("@Start", search.FromDate);
                    sqlQuery.AddParam("@End", search.ToDate);
                }

                if (!string.IsNullOrEmpty(search.TextSearch))
                {
                    query += sqlQuery.AddSearchTerm(search.TextSearch, new List<string> { "u.UserId", "u.FirstName", "u.LastName", "u.Initials", "frP.Name", "frP.FinaleId", "toP.Name", "toP.FinaleId", "frS.Name", "toS.Name", "c.PoNumber", "c.ConversionId" });
                }

                if (search.ProductId != 0)
                {
                    query += "AND (frP.ProductId = @ProductId or toP.ProductId = @ProductId) ";
                    sqlQuery.AddParam("@ProductId", search.ProductId);
                }

                if(search.SubLocationId.HasValue)
                {
                    query += "AND (frS.SublocationId = @SublocationId or toS.SublocationId = @SublocationId) ";
                    sqlQuery.AddParam("@SublocationId", search.SubLocationId.Value);
                }



                if (search.SortBy != null)
                {

                    query += "ORDER BY " + search.SortBy.ToString() + " ";
                    if (search.Ascending)
                    {
                        query += "ASC ";
                    }
                    else
                    {
                        query += "DESC ";
                    }
                }
                if (search.Paginate)
                {

                    query += "limit @startIndex, @count ";
                    sqlQuery.AddParam("@startIndex", (search.Page - 1) * search.Limit);
                    sqlQuery.AddParam("@count", search.Limit);
                }

                using (SqlReader reader = sqlQuery.ExecuteReader(sql, query))
                {
                    while (reader.HasNext())
                    {
                        Conversion conversion = new Conversion
                        {
                            ConversionId = reader.GetInt("ConversionId"),
                            FromProduct = new Product
                            {
                                ProductId = reader.GetInt("frProductId"),
                                Name = reader.GetString("frProductName"),
                                FinaleId = reader.GetString("frFinaleId"),
                                Category = new Category {
                                    Label = reader.GetString("frCat")
                                },
                                Condition = new Condition
                                {
                                    Code = reader.GetString("frCCode"),
                                    Name = reader.GetString("frCName")
                                }
                            },
                            ToProduct = new Product
                            {
                                ProductId = reader.GetInt("toProductId"),
                                Name = reader.GetString("tProductName"),
                                FinaleId = reader.GetString("toFinaleId"),
                                Category = new Category
                                {
                                    Label = reader.GetString("toCat")
                                },
                                Condition = new Condition
                                {
                                    Code = reader.GetString("toCCode"),
                                    Name = reader.GetString("toCName")
                                }
                            },
                            FromSublocation = new Sublocations.Sublocation
                            {
                                SublocationId = reader.GetInt("frSublocationId"),
                                Name = reader.GetString("frSublocationName")
                            },
                            ToSublocation = new Sublocations.Sublocation
                            {
                                SublocationId = reader.GetInt("toSublocationId"),
                                Name = reader.GetString("toSublocationName")
                            },
                            user = new User
                            {
                                FirstName = reader.GetString("FirstName"),
                                LastName = reader.GetString("LastName"),
                                Initials = reader.GetString("Initials")
                            },
                            DateTime = reader.GetTime("Created"),
                            Note = reader.GetString("Note"),
                            Quantity = reader.GetInt("Quantity"),
                            PoNumber = reader.GetString("PoNumber"),
                            ConvertType = reader.GetString("ConvertType"),
                            CreateNewProduct = reader.GetIntAsBoolean("NewProduct")
                        };

                        conversions.Add(conversion);
                    }
                }
            }

            return conversions;
        }


        private static List<String> ValidateConversion(Conversion conversion, int channelId)
        {
            List<string> messages = new List<string>();
            ApplicationUser user = ThreadProperties.GetUser();
            bool convertPlus = ThreadProperties.GetUserManager().IsInRole(user.Id, "ConvertPlus");
            bool isConvertingToProduct = convertPlus && conversion.ConvertType == "item";

            if ((conversion.ToProduct.Condition.ConditionId == 0 && !isConvertingToProduct) || conversion.FromSublocation.SublocationId == null)
            {
                messages.Add("Some fields are empty");
            } else
            {

                if (conversion.ToProduct.Condition?.ConditionId != 0)
                {
                    Condition toCondition = ConditionProvider.GetConditions(new SearchCriteria { Id = conversion.ToProduct.Condition.ConditionId })[0];
                    conversion.ToProduct = ProductsProvider.SearchProducts(new SearchCriteria { Id = conversion.ToProduct.ProductId })[0];
                    conversion.ToProduct.Condition = toCondition;
                }

                conversion.ToSublocation = SublocationProvider.GetSublocations(new SearchCriteria { Id = conversion.ToSublocation.SublocationId }, channelId)[0];
                conversion.FromSublocation = SublocationProvider.GetSublocations(new SearchCriteria { Id = conversion.FromSublocation.SublocationId }, channelId)[0];

                if (string.IsNullOrEmpty(conversion.FromProduct?.Url))
                {
                    messages.Add("Product Url needs to be populated");
                }
                if (string.IsNullOrEmpty(conversion.FromSublocation?.Url) || string.IsNullOrEmpty(conversion.ToSublocation?.Url))
                {
                    messages.Add("Sublocations are invalid");
                }
                if (conversion.Quantity <= 0)
                {
                    messages.Add("Quantity must be a positive integer");
                }
                /*if (conversion.FromProduct.Condition.ConditionId == conversion.ToProduct.Condition.ConditionId)
                {
                    messages.Add("You are trying to convert to the same as the current condition");
                }*/
                if (!isConvertingToProduct && string.IsNullOrEmpty(conversion.FromProduct.Ncs))
                {
                    messages.Add("Cannot convert from a product that does not have an NCS value");
                }
                if (!conversion.ToProduct.Condition.Convertible)
                {
                    messages.Add("You cannot convert to this condition");
                }
                if (!conversion.FromSublocation.ConvertibleFrom.HasValue)
                {
                    messages.Add("You cannot transfer from this location");
                }
                if (conversion.FromSublocation.SublocationId != conversion.ToSublocation.SublocationId && !conversion.ToSublocation.ConvertibleTo.Value)
                {
                    messages.Add("You cannot transfer to this location");
                }
                if (conversion.FromSublocation.SublocationId == conversion.ToSublocation.SublocationId && conversion.FromProduct.ProductId == conversion.ToProduct.ProductId && !conversion.CreateNewProduct)
                {
                    messages.Add("You cannot convert the same product to the same location");
                }
                //if not other errors, then check stock
                if(!messages.Any())
                {
                    Dictionary<string, int> stock = FinaleInventory.FinaleClientProvider.GetStockPerLocation(conversion.FromProduct, false, channelId);
                    if (!stock.ContainsKey(conversion.FromSublocation.Url) || stock[conversion.FromSublocation.Url] < conversion.Quantity)
                    {
                        messages.Add("There is insufficient stock available to make this conversion");
                    }
                }
            }

            return messages;
        }
    }
}