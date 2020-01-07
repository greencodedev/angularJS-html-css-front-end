using BaseWebApp.Maven.Utils.Api;
using BaseWebApp.Maven.Utils.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.Custom_Fields
{
    public class DynamicCustomFieldsProvider
    {
        public static ProviderResponse InsertDynamicCustomFields(List<DynamicCustomField> fields, int AccountId)
        {
            ProviderResponse res = new ProviderResponse();

            try
            {
                fields.ForEach(x => x.AccountId = AccountId); 
                

                List<string> errors =  ValidateCustomFields(fields, false);

                if (errors.Any())
                {
                    res.Messages = errors;
                    return res;
                }

                string query = @"INSERT INTO CustomFields (CustomFieldId ,AccountId, Name, Active, FinaleCustomFieldId ,AmazonCustomFieldId,ShowOnProduct,OptionToFilter)
                                 VALUES ";

                SqlQuery sqlQuery = new SqlQuery();

                for (int i = 0; i < fields.Count; i++)
                {
                    query += "(@CustomFieldId" + i + ", @AccountId" + i + ",@Name" + i + ",@Active" + i + ",@FinaleCustomFieldId" + i 
                        + ", @AmazonCustomFieldId" + i + ", @ShowOnProduct" + i + ", @OptionToFilter" + i + ")";


                    sqlQuery.AddParam("@CustomFieldId" + i, fields[i].CustomFieldId);
                    sqlQuery.AddParam("@AccountId" + i, fields[i].AccountId);
                    sqlQuery.AddParam("@Name" + i, fields[i].Name);
                    sqlQuery.AddParam("@Active" + i, fields[i].Active);
                    sqlQuery.AddParam("@FinaleCustomFieldId" + i, fields[i].FinaleCustomFieldId);
                    sqlQuery.AddParam("@AmazonCustomFieldId" + i, fields[i].AmazonCustomFieldId);
                    sqlQuery.AddParam("@ShowOnProduct" + i, fields[i].ShowOnProduct);
                    sqlQuery.AddParam("@OptionToFilter" + i, fields[i].OptionToFilter);

                    if(i < fields.Count - 1)
                    {
                        query += " , ";
                    }
                    else
                    {
                        query += " ";
                    }
                }
                using (Sql sql = new Sql())
                {
                     sqlQuery.ExecuteInsert(sql, query);
                }

                //return all custom fields with the newly added ones
                return GetCustomFields(AccountId, null);
            }
            catch (Exception e)
            {
                Utils.Logger.Log(e.ToString());
                res.Messages.Add("An error occurred");
                return res;
            }

        }

        public static ProviderResponse GetCustomFields(int AccountId, int? customfieldId)
        {
            ProviderResponse res = new ProviderResponse();

            try
            {
                List<DynamicCustomField> fields = new List<DynamicCustomField>();

                string query = "select * from CustomFields where AccountId = @AccountId ";

                SqlQuery sqlQuery = new SqlQuery();
                sqlQuery.AddParam("@AccountId", AccountId);

                if (customfieldId != null && customfieldId != 0)
                {
                    query += "and CustomFieldId = @CustomFieldId ";
                    sqlQuery.AddParam("@CustomFieldId", customfieldId);
                }

                using (Sql sql = new Sql())
                {
                    using (SqlReader reader = sqlQuery.ExecuteReader(sql, query))
                    {
                        while (reader.HasNext())
                        {
                            fields.Add(new DynamicCustomField
                            {
                                CustomFieldId = reader.GetInt("CustomFieldId"),
                                AccountId = reader.GetInt("AccountId"),
                                Name = reader.GetString("Name"),
                                Active = reader.GetBoolean("Active"),
                                FinaleCustomFieldId = reader.GetString("FinaleCustomFieldId"),
                                AmazonCustomFieldId = reader.GetString("AmazonCustomFieldId"),
                                ShowOnProduct = reader.GetBoolean("ShowOnProduct"),
                                OptionToFilter = reader.GetBoolean("OptionToFilter")
                            });

                        }
                    }
                }

                if (!fields.Any())
                {
                    res.Messages.Add("No custom fields");
                }
                else
                {
                     //if id specified return single
                    if (customfieldId != null)
                    {
                        res.Data = fields[0];
                    }
                    else //return list
                    {
                        res.Data = fields;
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

        public static ProviderResponse UpdateCustomFields(DynamicCustomField customfield, int AccountId)
        {
            ProviderResponse res = new ProviderResponse();

            try
            {
                customfield.AccountId = AccountId;

                List<string> errors = ValidateCustomFields(new List<DynamicCustomField> { customfield }, true);

                if (errors.Any())
                {
                    res.Messages = errors;
                    return res;
                }

                string query = @"UPDATE CustomFields SET Name = @Name, Active = @Active, FinaleCustomFieldId = @FinaleCustomFieldId,
                               AmazonCustomFieldId = @AmazonCustomFieldId, ShowOnProduct = @ShowOnProduct, OptionToFilter = @OptionToFilter WHERE AccountId = @AccountId";

                SqlQuery sqlQuery = new SqlQuery();

                sqlQuery.AddParam("@Name", customfield.Name);
                sqlQuery.AddParam("@Active", customfield.Active);
                sqlQuery.AddParam("@FinaleCustomFieldId", customfield.FinaleCustomFieldId);
                sqlQuery.AddParam("@AmazonCustomFieldId", customfield.AmazonCustomFieldId);
                sqlQuery.AddParam("@ShowOnProduct", customfield.ShowOnProduct);
                sqlQuery.AddParam("@OptionToFilter", customfield.OptionToFilter);
                sqlQuery.AddParam("@AccountId", customfield.AccountId);

                using(Sql sql = new Sql())
                {
                    sqlQuery.ExecuteNonQuery(sql, query);
                }

                return GetCustomFields(AccountId, customfield.CustomFieldId);
            }
            catch (Exception e)
            {
                Utils.Logger.Log(e.ToString());
                res.Messages.Add("An error occurred");
                return res;
            }
        }

        public static List<string> ValidateCustomFields(List<DynamicCustomField> field, bool update)
        {
            List<string> errors = new List<string>();

            if(field.Any(x => (x.CustomFieldId == 0 && update) || x.AccountId == 0))
            {
                errors.Add("Error");
            }
            else
            if(field.Any(x => string.IsNullOrEmpty(x.Name)))
            {
                errors.Add("Missisng name");
            }
            if(field.Any(x => string.IsNullOrEmpty(x.AmazonCustomFieldId) && string.IsNullOrEmpty(x.FinaleCustomFieldId)))
            {
                errors.Add("Please enter Amazon or Finale id");
            }

            return errors;
        }
    }
}