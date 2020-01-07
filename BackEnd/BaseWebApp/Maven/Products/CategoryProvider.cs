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
    public class CategoryProvider
    {
        public static List<Category> SearchCategory(SearchCriteria search)
        {
            List<Category> list = new List<Category>();
         
            string query = @"select * from Categories WHERE AccountId = @AccountId ";

            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());

            if (search.category != null)
            {
                query += "and CategoryId = @Id ";
                sqlQuery.AddParam("@Id", search.category);
            }

            if (!string.IsNullOrEmpty(search.TextSearch))
            {
                query += @"and (Label like @TextSearch) ";
                sqlQuery.AddParam("@TextSearch", "%" + search.TextSearch + "%");
            }

            query += "order by SortOrder ";

            using (Sql sqlConnection = new Sql())
            using (SqlReader reader = sqlQuery.ExecuteReader(sqlConnection, query))
            {
                while (reader.HasNext())
                {
                    list.Add(new Category
                    {
                        CategoryId = reader.GetInt("categoryId"),
                        AttrName = reader.GetString("AttrName"),
                        Label = reader.GetString("Label"),
                        SortOrder = reader.GetInt("SortOrder"),
                    });
                }
            }
            
            return list;
        }

        public static ProviderResponse SendCat(SearchCriteria search)
        {
            ProviderResponse response = new ProviderResponse();

            try
            {
                if(search == null)
                {
                    search = new SearchCriteria();
                }
                
                response.Data = SearchCategory(search);
            }
            catch(Exception e)
            {
                Logger.Log(e.ToString());
                response.Messages.Add("an error has occurred, please try again later");
            }

            return response;
        }

        public static void InsertCategory(Category category, Sql SqlConnection)
        {
            ProviderResponse res = new ProviderResponse();
            string query = @"insert into Categories (AccountId, AttrName, Label, SortOrder) values (@AccountId, @user4, @labelTest4, @SortOrder) ";

            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());
            sqlQuery.AddParam("@user4", category.AttrName);
            sqlQuery.AddParam("@labelTest4", category.Label);
            sqlQuery.AddParam("@SortOrder", category.SortOrder);
            
            int id = (int)sqlQuery.ExecuteInsert(SqlConnection, query);
        }
    }
}