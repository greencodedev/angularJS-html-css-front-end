using BaseWebApp.Maven.FinaleInventory;
using BaseWebApp.Maven.Users;
using BaseWebApp.Maven.Utils;
using BaseWebApp.Maven.Utils.Api;
using BaseWebApp.Maven.Utils.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.Products
{
    public class CostomFieldProvider
    {

        public static List<CostumFields> SearchCostomField(CostumFieldType type)
        {


           List<CostumFields>  list = new List<CostumFields>();


            string query = @"select *
                            from Lookup 
                            where LookupType = @LookupType and AccountId = @AccountId";

            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());
            sqlQuery.AddParam("@LookupType", type.ToString());


            using (Sql sql = new Sql())
            using (SqlReader reader = sqlQuery.ExecuteReader(sql, query))
            {
                while (reader.HasNext())
                {
                    list.Add(new CostumFields
                    {
                        id = reader.GetInt("id"),
                        LookupType = reader.GetString("LookupType"),
                        LookupValue = reader.GetString("LookupValue")
                    });

                  

                }
            }




            return list;
        }


        public static ProviderResponse SendCostomField(CostumFieldType type)
        {

            ProviderResponse response = new ProviderResponse();
            try
            {
                

                List<CostumFields> result = SearchCostomField(type);
                response.Data = result;

            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                response.Messages.Add("an error has occurred, please try again later");

            }
            return response;
        }

        public static void InsertCostomField(CostumFieldType type, string field, Sql sql)
        {
            string query = @"insert into Lookup(AccountId, LookupType, LookupValue) values (@AccountId, @LookupType, @LookupValue)";
            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());
            sqlQuery.AddParam("@LookupType", type.ToString());
            sqlQuery.AddParam("@LookupValue", field);
            sqlQuery.ExecuteInsert(sql, query);
        }

        public static void DeleteCostomFiled(int id, Sql sql, string Value)
        {
            string query = @"DELETE FROM Lookup WHERE id = @id and LookupValue = @LookupValue";

            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.AddParam("@id",id);
            sqlQuery.AddParam("@LookupValue",Value);
           
            sqlQuery.ExecuteInsert(sql, query);

        }

        public static void InsertCostomFields(List<CostumFields> customFieldList)
        {
            SqlQuery sqlQuery = new SqlQuery();

            string query = @"INSERT IGNORE INTO Lookup (LookupType, AccountId, LookupValue) 
                            VALUES ";

            for (int i = 0; i < customFieldList.Count(); i++)
            {
                query += "(@LookupType" + i + ",";
                sqlQuery.AddParam("@LookupType" + i, customFieldList[i].LookupType);

                query += "@AccountId" + i + ",";
                sqlQuery.AddParam("@AccountId" + i, UsersProvider.GetCurrentAccountId());

                query += "@LookupValue" + i + ")";
                sqlQuery.AddParam("@LookupValue" + i, customFieldList[i].LookupValue);

                if (i < customFieldList.Count() - 1)
                {
                    query += ", ";
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
        }
    }

    public enum CostumFieldType
    {
        Null,
        Condition,
        Ncs
    }
}