
using BaseWebApp.Maven.FinaleInventory;
using BaseWebApp.Maven.Users;
using BaseWebApp.Maven.Utils;
using BaseWebApp.Maven.Utils.Api;
using BaseWebApp.Maven.Utils.Email;
using BaseWebApp.Maven.Utils.Sql;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;

namespace BaseWebApp.Maven.Products
{
    public class ProductsProvider
    {
        public static List<Product> SearchProducts(SearchCriteria search)
        {
            Logger.Log(search.ToString());
            List<Product> list = new List<Product>();
            

                string query = @"select p.*, c.*, co.Name conditionName, co.Description conditionDescription, co.Code conditionCode, co.Convertible 
                            from Products p
	                            left join Categories c on (c.CategoryId = p.CategoryId)
                                left join Conditions co on (co.ConditionId = p.ConditionId) 
                            where p.AccountId = @AccountId ";

            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());

            
            if(search.Id != null)
            {
                query += "and p.ProductId = @ProductId ";
                sqlQuery.AddParam("@ProductId", search.Id);
            }
            if (search.category != null)
            {
                query += "and c.CategoryId = @Id ";
                sqlQuery.AddParam("@Id", search.category);
            }
            if(search.Conditions != null)
            {
                query += "and p.ConditionId = @ConditionId ";
                sqlQuery.AddParam("@ConditionId", search.Conditions);
            }
            if (!string.IsNullOrEmpty(search.Ncs))
            {
                query += "and p.Ncs = @Ncs ";
                sqlQuery.AddParam("@Ncs", search.Ncs);
            }
            if(!string.IsNullOrEmpty(search.FinaleId))
            {
                query += "and p.FinaleId = @FinaleId ";
                sqlQuery.AddParam("@FinaleId", search.FinaleId);
            }
            if(search.ChannelId != null)
            {
                query += "and ChannelId = @ChannelId ";
                sqlQuery.AddParam("@ChannelId", search.ChannelId);
            }

            query += sqlQuery.AddSearchTerm(search.TextSearch, new List<string> { "c.Label", "p.Name", "p.FinaleId", "co.Name", "co.Description", "co.Code" });


            
            if (search.Status != null)
            {
                if(search.Status == true)
                {
                    query += "and p.Status = 'PRODUCT_ACTIVE' ";
                }
                else
                {
                    query += "and p.Status = 'PRODUCT_INACTIVE' ";
                }
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
           

            using (Sql sql = new Sql())
            {
                sql.ReadUncommitted = search.ReadUncommited;
                using (SqlReader reader = sqlQuery.ExecuteReader(sql, query))
                {
                    while (reader.HasNext())
                    {
                        Product product = new Product
                        {
                            ProductId = reader.GetInt("ProductId"),
                            ChannelId = reader.GetInt("ChannelId"),
                            FinaleId = reader.GetString("FinaleId"),
                            Name = reader.GetString("Name"),
                            //Description = reader.GetString("Description"),
                            Status = reader.GetString("Status"),
                            Ncs = reader.GetString("Ncs"),
                            Url = reader.GetString("Url"),
                            Price = reader.GetDoubleOrZero("Price"),
                            CasePrice = reader.GetDoubleOrZero("CasePrice"),
                            Cost = reader.GetDoubleOrZero("Cost")
                        };

                        if(reader.GetInteger("ConditionId") != null)
                        {
                            product.Condition = new Conditions.Condition
                            {
                                ConditionId = reader.GetInt("ConditionId"),
                                Name = reader.GetString("conditionName"),
                                Description = reader.GetString("conditionDescription"),
                                Code = reader.GetString("conditionCode"),
                                Convertible = reader.GetBoolean("Convertible")
                            };
                        }

                        if(reader.GetInteger("CategoryId") != null)
                        {
                            product.Category = new Category
                            {
                                CategoryId = reader.GetInt("CategoryId"),
                                AttrName = reader.GetString("AttrName"),
                                Label = reader.GetString("Label"),
                                SortOrder = reader.GetInt("SortOrder")
                            };
                        }

                        list.Add(product);   
                    }
                }
            }

               
          
            
            return list;
        }

        public static ProviderResponse GetProductGroups()
        {
            ProviderResponse response = new ProviderResponse();

            //ListReponse productList = (ListReponse)SendProducts(null).Data;
            //List<Product> products = (List<Product>)productList.List;
            List<string> ncsList = CostomFieldProvider.SearchCostomField(CostumFieldType.Ncs).Select(n => n.LookupValue).ToList();
            response.Data = ncsList;

            return response;
        }


        public static ProviderResponse SendProducts(SearchCriteria search, int channelid)
        {
            ProviderResponse response = new ProviderResponse();

            try
            {
                if(search == null)
                {
                    search = new SearchCriteria();
                }
                if(channelid == 0)
                {
                    response.Messages.Add("Please specify a channel");
                    return response;
                }
                search.Status = true;
                search.Paginate = true;
                search.ChannelId = channelid;

                ListReponse listReponse = new ListReponse();
                
                listReponse.List = SearchProducts(search);
                
                search.Paginate = false;
                listReponse.TotalCount = SearchProducts(search).Count;
                
                response.Data = listReponse;
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                response.Messages.Add("an error has occurred, please try again later");
            }

            return response;
        }

        public static ProviderResponse InsertProducts(List<Product> products, Sql sql, int channelId)
        {
            ProviderResponse response = new ProviderResponse();

            try
            {
                string query = @"INSERT INTO Products(AccountId, channelId, CategoryId, FinaleId, Name, Description, Status, ConditionId, Ncs, Url, Price, CasePrice, Cost) 
                                values ";
                SqlQuery sqlQuery = new SqlQuery();

                for (int i = 0; i < products.Count(); i++)
                {
                    query += "(@AccountId" + i + ", @channelId" + i + ", @CategoryId" + i + ", @FinaleId" + i + ", @name" + i + ", @Description" + i + ", @Status" + i + ", @ConditionId" + i + ", @Ncs" + i + ", @Url" + i + ", @Price" + i + ", @CasePrice" + i + ", @Cost" + i + ")";
                    sqlQuery.AddParam("@AccountId" + i, UsersProvider.GetCurrentAccountId());
                    sqlQuery.AddParam("@channelId" + i, channelId);
                    sqlQuery.AddParam("@CategoryId" + i, products[i].Category?.CategoryId);
                    sqlQuery.AddParam("@FinaleId" + i, products[i].FinaleId);
                    sqlQuery.AddParam("@Name" + i, products[i].Name);
                    sqlQuery.AddParam("@Description" + i, products[i].Description);
                    sqlQuery.AddParam("@Status" + i, products[i].Status);
                    sqlQuery.AddParam("@ConditionId" + i, products[i].Condition?.ConditionId);
                    sqlQuery.AddParam("@Ncs" + i, products[i].Ncs);
                    sqlQuery.AddParam("@Url" + i, products[i].Url);
                    sqlQuery.AddParam("Price" + i, products[i].Price);
                    sqlQuery.AddParam("CasePrice" + i, products[i].CasePrice);
                    sqlQuery.AddParam("Cost" + i, products[i].Cost);

                    if (i < products.Count() - 1)
                    {
                        query += ", ";
                    }
                    else
                    {
                        query += " ";
                    }
                }

                if (products.Count() > 0)
                {
                  sqlQuery.ExecuteInsert(sql, query);
                }
                
                response.Success = true;
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                response.Success = false;
            }

            return response;
        }

        public static ProviderResponse InsertProduct(Product product, Sql sql)
        {
            ProviderResponse response = new ProviderResponse();
           
            try
            {
                string query = @"insert into Products(AccountId,CategoryId,FinaleId,Name,Description,Status,ConditionId,Ncs,Url,Price,CasePrice,Cost) values (@CategoryId, @FinaleId, @name, @Description, @Status, @ConditionId, @Ncs, @Url, @Price, @CasePrice, @Cost)";
                SqlQuery sqlQuery = new SqlQuery();
                sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());
                sqlQuery.AddParam("@CategoryId", product.Category?.CategoryId);
                sqlQuery.AddParam("@FinaleId", product.FinaleId);
                sqlQuery.AddParam("@Name", product.Name);
                sqlQuery.AddParam("@Description", product.Description);
                sqlQuery.AddParam("@Status", product.Status);
                sqlQuery.AddParam("@ConditionId", product.Condition?.ConditionId);
                sqlQuery.AddParam("@Ncs", product.Ncs);
                sqlQuery.AddParam("@Url", product.Url);
                sqlQuery.AddParam("Price", product.Price);
                sqlQuery.AddParam("CasePrice", product.CasePrice);
                sqlQuery.AddParam("Cost", product.Cost);

                product.ProductId = (int)sqlQuery.ExecuteInsert(sql, query);

                response.Success = true;
            }
            catch(Exception e)
            {
                Logger.Log(e.ToString());
                response.Success = false;
            }

            return response;
        }

        public static void CreateFinaleProduct(Product product, int channelId)
        {
            JObject data = product.ConvertToFinaleObject();
            HttpResponseMessage res = FinaleInventory.FinaleClientProvider.MakeFinalePostRequest("product", data, false, channelId);

            dynamic prodResponse = JObject.Parse(res.Content.ReadAsStringAsync().Result);
            product.FinaleId = prodResponse.productId;
            product.Url = prodResponse.productUrl;
            User user = UsersProvider.GetCurrentUser();

            string emailBody = @"The following product was automatically created in Finale: <br />
                                  Finale Product Id: " + product.FinaleId + "<br />" +
                                 "Name: " + product.Name + "<br />" +
                                 "Condition: " + product.Condition.Name +
                                 $"<br/>User: {user.FirstName} {user.LastName} ({user.Initials})";

            EmailClient.SendEmail(Config.GetConfig("EMAIL_TO_EMAIL"), "A new product was automatically created in Finale", emailBody);

            using(Sql sql = new Sql())
            {
                InsertProduct(product, sql);
            }
        }

        public static void UpdateFinaleCost(Product product, int channelId)
        {
            //JObject finaleProduct = FinaleClientProvider.GetProductObject(product.FinaleId, false);
            JObject data = product.UpdateConvertInfo(channelId);

            HttpResponseMessage res = FinaleInventory.FinaleClientProvider.MakeFinalePostRequest(product.FinaleId, "product", data, false, channelId);
            string resultString = res.Content.ReadAsStringAsync().Result;
        }
    }
}