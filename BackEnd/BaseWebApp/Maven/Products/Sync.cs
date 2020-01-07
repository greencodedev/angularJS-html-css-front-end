using BaseWebApp.Maven.Channels;
using BaseWebApp.Maven.Custom_Fields;
using BaseWebApp.Maven.FinaleInventory;
using BaseWebApp.Maven.Products.Conditions;
using BaseWebApp.Maven.Sublocations;
using BaseWebApp.Maven.Users;
using BaseWebApp.Maven.Utils;
using BaseWebApp.Maven.Utils.Api;
using BaseWebApp.Maven.Utils.Email;
using BaseWebApp.Maven.Utils.Excel;
using BaseWebApp.Maven.Utils.Sql;
using MWSUtils.Listings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace BaseWebApp.Maven.Products
{
    public class Sync
    {
        public static ProviderResponse SyncCat(int channelId)
        {

            ProviderResponse response = new ProviderResponse();
            List<Category> New = FinaleClientProvider.SyncCategory(channelId);

            using (Sql sql = new Sql())
            {
                try
                {
                    sql.BeginTransaction("UpdateCategories");
                    List<Category> old = CategoryProvider.SearchCategory(new SearchCriteria());
                    Dictionary<string, Category> OldDict = old.ToDictionary(x => x.AttrName);

                    foreach (Category data in New)
                    {
                        if (!OldDict.ContainsKey(data.AttrName))
                        {
                            //Insert
                            CategoryProvider.InsertCategory(data, sql);
                        }
                        else
                        {
                            Category currData = OldDict[data.AttrName];
                            UpdateCategoryData(data.AttrName, currData, data, sql);
                        }
                    }

                    sql.Commit();
                }
                catch (Exception e)
                {
                    sql.Rollback();
                    Logger.Log(e.ToString());
                    response.Messages.Add("an error has occurred, please try again later when trying to sync categories");
                }

                return response;
            }
        }

        private static void UpdateCategoryData(string AttrName, Category currData, Category newData, Sql sql)
        {
            if (currData.AttrName != newData.AttrName
                || currData.Label != newData.Label
                || currData.SortOrder != newData.SortOrder
               )
            {
                string query = @"update Categories 
                                set Label = @Label,
                                    SortOrder = @SortOrder
                                   
                                where AttrName = @AttrName and AccountId = @AccountId ";

                SqlQuery sqlQuery = new SqlQuery();
                sqlQuery.AddParam("@AttrName", AttrName);
                sqlQuery.AddParam("@Label", newData.Label);
                sqlQuery.AddParam("@SortOrder", newData.SortOrder);
                sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());



                sqlQuery.ExecuteNonQuery(sql, query);
            }
        }

        public static ProviderResponse SyncProd(int channelid)
        {
            return SyncProd(false, channelid);
        }


        public static ProviderResponse SyncProd(bool ignoreMessages, int channelId)
        {
            ProviderResponse response = new ProviderResponse();
            List<Product> New = FinaleClientProvider.SyncProducts(channelId);

            if (New == null)
            {
                response.Success = false;
                response.Messages.Add("an error has occurred when trying to connect to Finale Inventory");
            }
            else
            {
                using (Sql sql = new Sql())
                {

                    try
                    {

                        sql.BeginTransaction("updateProducts");
                        Dictionary<string, Product> old = ProductsProvider.SearchProducts(new SearchCriteria() { ChannelId = channelId}).ToDictionary(x => x.FinaleId);

                        if (old.Count > 0)
                        {

                            // Find if there are duplicate items with the same NCS and Condition. There shouldn't be.
                            var duplicatedList = New.FindAll(x => !string.IsNullOrEmpty(x.Ncs) && x.Status == "PRODUCT_ACTIVE")
                                                    .GroupBy(x => new { x.Condition?.ConditionId, x.Ncs })
                                                    .Where(g => g.Count() > 1)
                                                    .Select(y => y.Key)
                                                    .ToList();


                            // duplicatedList is a list og groups containing items that have a duplicate combination of NCS and Condition
                            if (duplicatedList.Count() > 0)
                            {
                                foreach (var dupe in duplicatedList)
                                {
                                    // Set the duplicate ncs flag to true for each item withing each of these groups
                                    New.FindAll(x => x.Ncs == dupe.Ncs && x.Condition?.ConditionId == dupe.ConditionId).ForEach(x => x._DuplicateCondNcs = true);
                                }
                            }
                        }

                        List<CostumFields> ncsList = new List<CostumFields>();
                        List<Product> productsWithoutNcs = new List<Product>();
                        List<Product> productsToInsert = new List<Product>();
                        List<Product> productsToUpdate = new List<Product>();
                        List<ProductCustomField> productCustomFieldsRedayForInsert = new List<ProductCustomField>();


                        foreach (Product prod in New)
                        {
                            // If item has an NCS, add it to our NCS list so we can update the NCS in our Custom Field Lookup table
                            if (!string.IsNullOrEmpty(prod.Ncs))
                            {
                                ncsList.Add(new CostumFields
                                {
                                    LookupType = CostumFieldType.Ncs.ToString(),
                                    LookupValue = prod.Ncs
                                });
                            } else
                            {
                                // If it does not contain an NCS, add it to a separate list, so we can alert the admins
                                prod.Ncs = null;

                                if(prod.Status == "PRODUCT_ACTIVE")
                                {
                                    productsWithoutNcs.Add(prod);
                                }
                            }

                            // If this product does not yet exist in our database, we need to create it
                            if (!old.ContainsKey(prod.FinaleId))
                            {
                                // insert
                                //ProductsProvider.InsertProduct(prod, sql);
                                productsToInsert.Add(prod);
                            }
                            else
                            {
                                // If this product already exists in our database, we just update it
                                Product currData = old[prod.FinaleId];
                                UpdateProductData(prod.FinaleId, currData, prod, sql);
                               
                                //add ProductId (primary key of the Product) to the custom fields
                                prod.CustomFields.ForEach(x => x.ProductId = currData.ProductId);

                                //add to list
                                productCustomFieldsRedayForInsert.AddRange(prod.CustomFields);
                            }
                        }

                        // Now we add all the new NCSs to our Custom Field Lookup table
                        //CostomFieldProvider.InsertCostomFields(ncsList);

                        ProviderResponse insert = ProductsProvider.InsertProducts(productsToInsert, sql, channelId);

                        if (productsToInsert.Any() && insert.Success)
                        {
                            
                            //pull products again to get the Product ID on newly inserted products - READ UNCOMMITTED
                            List<Product> pull = ProductsProvider.SearchProducts(new SearchCriteria() { ReadUncommited = true, ChannelId = channelId});

                            
                            foreach (Product prodInserted in productsToInsert)
                            {
                                Product prod = pull.Find(x => x.FinaleId == prodInserted.FinaleId);

                                if(prod != null)
                                {
                                    //set ProductId on their custom fields
                                    prodInserted.CustomFields.ForEach(x => x.ProductId = prod.ProductId);

                                    //add to list
                                    productCustomFieldsRedayForInsert.AddRange(prodInserted.CustomFields);
                                }
                            }
                        }
                        

                        //insert custom field values for all products.
                        ProductCustomFieldsProvider.InsertOrUpdateProductCustomFields(UsersProvider.GetCurrentAccountId(), productCustomFieldsRedayForInsert, sql);


                        // We send an email to admin alerting to products without NCS values
                        if (productsWithoutNcs.Count() > 0 && !ignoreMessages)
                        {
                            EmailClient.SendEmail(Config.GetConfig("EMAIL_TO_EMAIL"), 
                                null, 
                                "There are products with missing NCS - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(), 
                                "See Attached " + productsWithoutNcs.Count() + " Products", 
                                ExcelUtil.GetMissingNCSCsvData(productsWithoutNcs));
                        }

                        // We send an email to admin alerting to products with duplicate NCS values
                        List<Product> duplicate = New.FindAll(x => x._DuplicateCondNcs);
                        if (duplicate.Count() > 0 && !ignoreMessages)
                        {
                            
                            EmailClient.SendEmail(Config.GetConfig("EMAIL_TO_EMAIL"), 
                                null,
                                "There are products with duplicate NCS and Condition combinations - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(),
                                "See Attached " + duplicate.Count() + " Products",
                                ExcelUtil.GetduplicateNCSandConditionCsvData(duplicate));
                        }

                        sql.Commit();

                    }
                    catch (Exception e)
                    {
                        Logger.Log(e.ToString());
                        sql.Rollback();
                        response.Messages.Add("an error has occurred when trying to sync products. Please try again later");
                        response.Success = false;
                    }
                }
            }

            return response;
        }


        public static ProviderResponse SyncAmazonProducts(List<AmazonProduct> products, int channelId)
        {
            ProviderResponse response = new ProviderResponse();

            using (Sql sql = new Sql())
            {

                try
                {

                    sql.BeginTransaction("updateProducts");
                    Dictionary<string, Product> old = ProductsProvider.SearchProducts(new SearchCriteria() { ChannelId = channelId }).ToDictionary(x => x.FinaleId);


                   
                    List<Product> productsWithoutNcs = new List<Product>();
                    List<Product> productsToInsert = new List<Product>();
                    List<Product> productsToUpdate = new List<Product>();
                    List<ProductCustomField> productCustomFieldsRedayForInsert = new List<ProductCustomField>();

                    //get all dynamic custom fields for this account
                    List<DynamicCustomField> DynamicCustomFields = (List<DynamicCustomField>)DynamicCustomFieldsProvider.GetCustomFields(UsersProvider.GetCurrentAccountId(), null).Data;

                    //filter for dynamic fields for amazon only
                    List<DynamicCustomField> AmazonDynamicCustomFields = DynamicCustomFields.FindAll(x => !string.IsNullOrEmpty(x.AmazonCustomFieldId));

                    foreach (AmazonProduct prod in products)
                    {

                        //convert to our product object
                        Product product = new Product
                        {
                            Status = "PRODUCT_ACTIVE",
                            FinaleId = prod.seller_sku, //with amazon FinaleId will be the sku
                            ChannelId = channelId,
                            Name = prod.item_name,
                            Description = prod.item_description,
                            Price = Double.Parse(prod.price), //vallidate if empty

                        };

                        
                        // If this product does not yet exist in our database, we need to create it
                        if (!old.ContainsKey(product.FinaleId))
                        {
                            productsToInsert.Add(product);

                            //get custom fields values
                            foreach (var field in AmazonDynamicCustomFields)
                            {
                                string test = (string)prod.GetType().GetProperty(field.AmazonCustomFieldId).GetValue(prod);
                                

                                var sto = "jd";

                                if (test != null)
                                {
                                    //we first add it to the product without the product id and after we insert the product will look up the id
                                    product.CustomFields.Add(new ProductCustomField
                                    {
                                        CustomFieldId = field.CustomFieldId,
                                        Value = test,
                                    });
                                }
                            }

                        }
                        else
                        {
                            // If this product already exists in our database, we just update it
                            Product currData = old[product.FinaleId];

                            UpdateProductData(product.FinaleId, currData, product, sql);


                            //get custom fields values
                            foreach (var field in AmazonDynamicCustomFields)
                            {
                                string value = (string)prod.GetType().GetProperty(field.AmazonCustomFieldId).GetValue(prod, null);

                                var sto = "jd";

                                if(field.AmazonCustomFieldId == "seller-sku")
                                {
                                    value = prod.seller_sku;
                                }
                                if(field.AmazonCustomFieldId == "item_condition")
                                {
                                    value = prod.item_condition;
                                }

                                if (value != null)
                                {
                                    //since this prod already exists we have the productid and can add it to the ready to insert list
                                    productCustomFieldsRedayForInsert.Add(new ProductCustomField
                                    {
                                        CustomFieldId = field.CustomFieldId,
                                        Value = value,
                                        ProductId = product.ProductId
                                    });
                                }
                            }
                        }
                    }

                    ProviderResponse insert = ProductsProvider.InsertProducts(productsToInsert, sql, channelId);

                    //pull products again to get the Product ID on newly inserted products - READ UNCOMMITTED
                    List<Product> pull = ProductsProvider.SearchProducts(new SearchCriteria() { ReadUncommited = true, ChannelId = channelId });


                    foreach (Product prodInserted in productsToInsert)
                    {
                        Product prod = pull.Find(x => x.FinaleId == prodInserted.FinaleId);

                        if (prod != null)
                        {
                            //set ProductId on their custom fields
                            prodInserted.CustomFields.ForEach(x => x.ProductId = prod.ProductId);

                            //add to list
                            productCustomFieldsRedayForInsert.AddRange(prodInserted.CustomFields);
                        }
                    }

                    sql.Commit();

                }
                catch (Exception e)
                {
                    Logger.Log(e.ToString());
                    sql.Rollback();
                    response.Messages.Add("an error has occurred when trying to sync products. Please try again later");
                    response.Success = false;
                }
            }

            return response;
        }

        private static void UpdateProductData(string FinaleId, Product currData, Product newData, Sql sql)
        {
            if (currData.Name != newData.Name
                || currData.Description != newData.Description
                || currData.Status != newData.Status
                || currData.Condition?.ConditionId != newData.Condition?.ConditionId
                || currData.Category?.CategoryId != newData.Category?.CategoryId
                || currData.Ncs != newData.Ncs
                || currData.Price != newData.Price
                || currData.CasePrice != newData.CasePrice 
                || currData.Url != newData.Url
               )
            {
                string query = @"update Products 
                                set Name = @Name,
                                    Description = @Description,
                                    CategoryId = @CategoryId,
                                    Status = @Status,  
                                    ConditionId = @ConditionId,
                                    Ncs = @Ncs,
                                    Price = @Price,
                                    CasePrice = @CasePrice,
                                    Cost = @Cost,
                                    ConditionId = @ConditionId,
                                    Ncs = @Ncs,
                                    Url = @Url
                                where FinaleId = @FinaleId and AccountId = @AccountId";
                               
                SqlQuery sqlQuery = new SqlQuery();
                sqlQuery.AddParam("@Name", newData.Name);
                sqlQuery.AddParam("@Description", newData.Description);
                sqlQuery.AddParam("@CategoryId", newData.Category?.CategoryId);
                sqlQuery.AddParam("@FinaleId", FinaleId);
                sqlQuery.AddParam("@Status", newData.Status);
                sqlQuery.AddParam("@ConditionId", newData.Condition?.ConditionId);
                sqlQuery.AddParam("@Ncs", newData.Ncs);
                sqlQuery.AddParam("@Url", newData.Url);
                sqlQuery.AddParam("@Price", newData.Price);
                sqlQuery.AddParam("@CasePrice", newData.CasePrice);
                sqlQuery.AddParam("@Cost", newData.Cost);
                sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());

                sqlQuery.ExecuteNonQuery(sql, query);
            }
        }

        public static ProviderResponse SyncConditions(int channelId)
        {
            ProviderResponse response = new ProviderResponse();

            List<Condition> conditions = FinaleClientProvider.GetConditions(channelId);

            if (conditions.Count() > 0)
            {
                ConditionProvider.InsertOrUpdateConditionFromFinale(conditions);
            }

            //List<string> newConditionList = FinaleClientProvider.GetCustomFields(Config.GetConfig("FINALE_CUSTOM_FIELD_CONDITION"))[Config.GetConfig("FINALE_CUSTOM_FIELD_CONDITION")];

            return response;
        }

        public static ProviderResponse SyncSublocations(int channelId)
        {
            ProviderResponse response = new ProviderResponse();

            List<Sublocation> sublocations = FinaleClientProvider.GetSublocations(channelId);

            if (sublocations.Count() > 0)
            {
                SublocationProvider.InsertOrUpdateSublocationsFromFinale(sublocations, channelId);
                response.Data = sublocations;
            }

            return response;
        }

        /*public static ProviderResponse SyncCostomField(string name)
        {
            List<string> list = FinaleClientProvider.SyncCostomField();

            ProviderResponse response = new ProviderResponse();
            using (Sql sql = new Sql())
            {
                try
                {
                    sql.BeginTransaction("updateProducts");
                    List<string> New = FinaleClientProvider.SyncCostomField();

                    List<CostumFields> Old = CostomFieldProvider.SearchCostomField(CostumFieldType.Condition);

                    Dictionary<string, CostumFields> OldLower = Old.ToDictionary(x => x.LookupValue.ToLower());


                    //List<CostumFields> OldLower = Old.ConvertAll(d => d.LookupValue.ToLower());
                    List<string> NewLower = New.ConvertAll(d => d.ToLower());

                    //Dictionary<string, List<string>> stuff = CostomFieldProvider.SearchCostomField().ToDictionary(x => x.ToLower);

                    foreach (string field in NewLower)
                    {
                        string FieldLower = field.ToString().ToLower();
                        if (!OldLower.ContainsKey(field))
                        {
                            CostomFieldProvider.InsertCostomField(CostumFieldType.Condition, field, sql);
                        }
                    }

                    foreach (KeyValuePair<string, CostumFields> entry in OldLower)
                    {
                        if (!NewLower.Contains(entry.Key))
                        {
                            CostomFieldProvider.DeleteCostomFiled(entry.Value.id, sql, entry.Key);
                        }
                    }

                    sql.Commit();
                }
                catch (Exception e)
                {
                    Logger.Log(e.ToString());
                    sql.Rollback();
                    response.Messages.Add("an error has occurred, please try again later when trying to sync products");
                }

                return response;
            }
        }*/
    }
}