using BaseWebApp.Maven.Utils.Api;
using BaseWebApp.Maven.Utils.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.Custom_Fields
{
    public class ProductCustomFieldsProvider
    {
        public static ProviderResponse InsertOrUpdateProductCustomFields(int AccountId , List<ProductCustomField> productfields, Sql sql)
        {
            ProviderResponse res = new ProviderResponse();

            if (productfields.Any())
            {

                try
                {
                    productfields.ForEach(x => x.AccountId = AccountId);
                

                    List<string> errors = ValidateProductCustomFields(productfields);

                    if (errors.Any())
                    {
                        res.Messages = errors;
                        return res;
                    }

                    string query = @"INSERT INTO ProductCustomFields (AccountId ,ProductId, CustomFieldId, Value)
                                     VALUES ";

                    SqlQuery sqlQuery = new SqlQuery();

                    for (int i = 0; i < productfields.Count; i++)
                    {
                        query += "(@AccountId" + i + ", @ProductId" + i + ",@CustomFieldId" + i + ",@Value" + i + ")";


                        sqlQuery.AddParam("@AccountId" + i, productfields[i].AccountId);
                        sqlQuery.AddParam("@ProductId" + i, productfields[i].ProductId);
                        sqlQuery.AddParam("@CustomFieldId" + i, productfields[i].CustomFieldId);
                        sqlQuery.AddParam("@Value" + i, productfields[i].Value);

                        if (i < productfields.Count - 1)
                        {
                            query += " , ";
                        }
                        else
                        {
                            query += " ";
                        }

                    }

                    query += @" ON DUPLICATE KEY UPDATE 
                            Value = VALUES(Value); ";

                    sqlQuery.ExecuteNonQuery(sql, query);
                
                }
                catch (Exception e)
                {
                    Utils.Logger.Log(e.ToString());
                    throw new Exception(e.ToString());
                }
            }
            return res;
        }


        public static ProviderResponse GetProductCustomFields(int AccountId, int? productCustomFieldId, int? productid)
        {
            ProviderResponse res = new ProviderResponse();

            try
            {
                List<ProductCustomField> productcustomfields = new List<ProductCustomField>();

                string query = "select * from ProductCustomFields where AccountId = @AccountId ";

                SqlQuery sqlQuery = new SqlQuery();
                sqlQuery.AddParam("@AccountId", AccountId);

                if (productCustomFieldId != null && productCustomFieldId != 0)
                {
                    query += "and ProductCustomFieldId = @ProductCustomFieldId ";
                    sqlQuery.AddParam("@ProductCustomFieldId", productCustomFieldId);
                }

                if(productid != null)
                {
                    query += "and ProductId = @ProductId ";
                    sqlQuery.AddParam("@ProductId", productid);
                }

                using (Sql sql = new Sql())
                {
                    using (SqlReader reader = sqlQuery.ExecuteReader(sql, query))
                    {
                        while (reader.HasNext())
                        {
                            productcustomfields.Add(new ProductCustomField
                            {
                                ProductCustomFieldId = reader.GetInt("ProductCustomFieldId"),
                                AccountId = reader.GetInt("AccountId"),
                                ProductId = reader.GetInt("ProductId"),
                                CustomFieldId = reader.GetInt("CustomFieldId"),
                                Value = reader.GetString("Value")
                            });

                        }
                    }
                }

                if (!productcustomfields.Any())
                {
                    res.Messages.Add("No custom fields");
                }
                else
                {
                    // return single
                    if (productCustomFieldId != null)
                    {
                        res.Data = productcustomfields[0];
                    }
                    else  //return list 
                    {
                        res.Data = productcustomfields;
                    }
                }
            }
            catch (Exception e)
            {
                Utils.Logger.Log(e.ToString());
                res.Messages.Add("An error occurred");
            }

            return res;
        }


        public static List<string> ValidateProductCustomFields(List<ProductCustomField> productCustomFields)
        {
            //only the back-end will insert or update Product Custom Fields
            List<string> errors = new List<string>();

            if (productCustomFields.Any(x =>
                x.AccountId == 0 || x.ProductId == 0 || x.CustomFieldId == 0 || string.IsNullOrEmpty(x.Value)))
            {
                errors.Add("Missing fields");
            }
            return errors;
        }
    }
}