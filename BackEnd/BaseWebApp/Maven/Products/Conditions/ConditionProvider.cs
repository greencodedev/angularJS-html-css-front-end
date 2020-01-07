using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BaseWebApp.Maven.Users;
using BaseWebApp.Maven.Utils;
using BaseWebApp.Maven.Utils.Api;
using BaseWebApp.Maven.Utils.Sql;

namespace BaseWebApp.Maven.Products.Conditions
{
    public class ConditionProvider
    {
        public static ProviderResponse GetConditionList(SearchCriteria search)
        {
            ProviderResponse response = new ProviderResponse();

            if(search == null)
            {
                search = new SearchCriteria { Status = true };
            }

            try
            {
                search.Status = true;
                response.Data = GetConditions(search);
            }
            catch(Exception e)
            {
                Logger.Log(e.ToString());
                response.Messages.Add("Something Went Wrong.");
                response.Success = false;
            }

            return response;
        }

        public static ProviderResponse UpdateConditions(List<Condition> conditions)
        {
            ProviderResponse response = new ProviderResponse();
            List<Condition> currentConditions = GetAllConditions();

            response.Messages.AddRange(Validate(currentConditions, conditions, true));

            if (!response.Success)
            {
                return response;
            }

            try
            {
                UpdateConditionsFromProvider(conditions);
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                response.Messages.Add("An Error occurred, see log for details");
            }

            return response;
        }

        public static List<Condition> GetAllConditions()
        {
            return GetConditions(new SearchCriteria());
        }

        public static List<Condition> GetConditions(SearchCriteria search)
        {
            List<Condition> conditions = new List<Condition>();

            Users.User currUser = Users.UsersProvider.GetCurrentUser();
            bool joinUserConditions = !currUser.IsAdmin || !string.IsNullOrEmpty(search.UserId);

            string query = @"select *";

            if (joinUserConditions)
            {
                query += ", uc.ConvertTo ucConvertTo, uc.ConvertFrom ucConvertFrom";
            }

            query += " from Conditions c ";

            if (joinUserConditions)
            {
                query += "left join UserConditions uc ON (uc.ConditionId = c.ConditionId and uc.UserId = @UserId) ";

            }

            query += "WHERE c.AccountId = @AccountId ";

            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());

            if (!currUser.IsAdmin)
            {
                query += "and uc.UserId = @UserId ";
                sqlQuery.AddParam("@UserId", currUser.UserId);
            }
            else if (!string.IsNullOrEmpty(search.UserId))
            {
                sqlQuery.AddParam("@UserId", search.UserId);
            }

            if (search.Id != null)
            {
                query += "AND c.ConditionId = @Id ";
                sqlQuery.AddParam("@Id", search.Id);
            }

            if(search.Status != null && search.Status == true)
            {
                query += "AND c.Status = @Status ";
                sqlQuery.AddParam("@Status", ConditionStatus.Active.ToString());
            }

            using (Sql sql = new Sql())
            using (SqlReader reader = sqlQuery.ExecuteReader(sql, query))
            {
                while (reader.HasNext())
                {
                    Condition condition = new Condition
                    {
                        ConditionId = reader.GetInt("ConditionId"),
                        Name = reader.GetString("Name"),
                        Description = reader.GetString("Description"),
                        Code = reader.GetString("Code"),
                        Convertible = reader.GetBoolean("Convertible"),
                        status = reader.ParseEnum<ConditionStatus>("Status")
                    };

                    if(joinUserConditions)
                    {
                        condition.UserConvertibleFrom = reader.GetOptionalBoolean("ucConvertFrom");
                        condition.UserConvertibleTo = reader.GetOptionalBoolean("ucConvertTo");
                    }

                    conditions.Add(condition);
                }
            }

            return conditions;
        }

        public static void InsertOrUpdateConditionFromFinale(List<Condition> conditions)
        {
            SqlQuery sqlQuery = new SqlQuery();

            List<Condition> currentConditions = GetAllConditions();

            string query = @"INSERT INTO Conditions (Name, AccountId, Status) 
                            VALUES ";

            for (int i = 0; i < conditions.Count(); i++)
            {
                query += "(@Name" + i + ",";
                sqlQuery.AddParam("@Name" + i, conditions[i].Name);

                query += "@AccountId" + i + ",";
                sqlQuery.AddParam("@AccountId" + i, UsersProvider.GetCurrentAccountId());

                query += "@Status" + i + ")";
                sqlQuery.AddParam("@Status" + i, ConditionStatus.Active.ToString());

                if (i < conditions.Count() - 1)
                {
                    query += ", ";
                }
                else
                {
                    query += " ";
                }
            }

            for (int i = 0; i < currentConditions.Count(); i++)
            {
                if (!conditions.Any(cond => cond.Name == currentConditions[i].Name))
                {
                    currentConditions[i].status = ConditionStatus.Removed;
                    query += ", (@CName" + i + ",";
                    sqlQuery.AddParam("@CName" + i, currentConditions[i].Name);

                    query += "@CAccountId" + i + ",";
                    sqlQuery.AddParam("@CAccountId" + i, UsersProvider.GetCurrentAccountId());

                    query += "@CStatus" + i + ")";
                    sqlQuery.AddParam("@CStatus" + i, ConditionStatus.Removed.ToString());

                    if (i < conditions.Count() - currentConditions.Count() - 1)
                    {
                        query += ", ";
                    }
                    else
                    {
                        query += " ";
                    }
                }
            }

            query += @"ON DUPLICATE KEY UPDATE 
                        Name = VALUES(Name), 
                        Status = VALUES(Status); ";

            using (Sql sql = new Sql())
            {
                sqlQuery.ExecuteInsert(sql, query);
            }
        }

        private static void UpdateConditionsFromProvider(List<Condition> conditions)
        {
            SqlQuery sqlQuery = new SqlQuery();

            string query = @"INSERT INTO Conditions (ConditionId, AccountId, Name, Code, Description, Convertible, Status) 
                            VALUES ";

            for (int i = 0; i < conditions.Count(); i++)
            {
                query += "(@ConditionId" + i + ",";
                sqlQuery.AddParam("@ConditionId" + i, conditions[i].ConditionId);

                query += "@AccountId" + i + ",";
                sqlQuery.AddParam("@AccountId" + i, UsersProvider.GetCurrentAccountId());

                query += "@Name" + i + ",";
                sqlQuery.AddParam("@Name" + i, conditions[i].Name);

                query += "@Code" + i + ",";
                sqlQuery.AddParam("@Code" + i, conditions[i].Code);

                query += "@Description" + i + ",";
                sqlQuery.AddParam("@Description" + i, conditions[i].Description);

                query += "@Convertible" + i + ",";
                sqlQuery.AddParam("@Convertible" + i, conditions[i].Convertible);

                query += "@Status" + i + ")";
                sqlQuery.AddParam("@Status" + i, ConditionStatus.Active.ToString());

                if (i < conditions.Count() - 1)
                {
                    query += ", ";
                }
                else
                {
                    query += " ";
                }
            }

            query += @"ON DUPLICATE KEY UPDATE 
                        ConditionId = VALUES(ConditionId), 
                        Name = VALUES(Name), 
                        Description = VALUES(Description), 
                        Code = VALUES(Code), 
                        Status = VALUES(Status),
                        Convertible = VALUES(Convertible); ";

            using (Sql sql = new Sql())
            {
                sqlQuery.ExecuteInsert(sql, query);
            }
        }

        public static ProviderResponse UpdateUserConditions(string userId, List<Condition> conditions)
        {
            ProviderResponse res = new ProviderResponse();

            if (string.IsNullOrEmpty(userId))
            {
                res.Success = false;
                return res;
            }

            try
            {

                SqlQuery sqlQuery = new SqlQuery();
                sqlQuery.AddParam("@UserId", userId);

                string query = @"INSERT INTO UserConditions (UserId, ConditionId, ConvertTo, ConvertFrom) 
                                VALUES ";

                for (int i = 0; i < conditions.Count(); i++)
                {
                    query += "(@UserId, @ConditionId" + i + ", @ConvertTo" + i + ", @ConvertFrom" + i + ")";
                    sqlQuery.AddParam("@ConditionId" + i, conditions[i].ConditionId);
                    sqlQuery.AddParam("@ConvertFrom" + i, Convert.ToInt32(conditions[i].UserConvertibleFrom));
                    sqlQuery.AddParam("@ConvertTo" + i, Convert.ToInt32(conditions[i].UserConvertibleTo));

                    if (i < conditions.Count() - 1)
                    {
                        query += ", ";
                    }
                    else
                    {
                        query += " ";
                    }
                }

                query += @"ON DUPLICATE KEY UPDATE 
                        ConvertFrom = VALUES(ConvertFrom), 
                        ConvertTo = VALUES(ConvertTo); ";

                using (Sql sql = new Sql())
                {
                    sqlQuery.ExecuteInsert(sql, query);

                    res.Data = GetConditions(new SearchCriteria());
                }

            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                res.Success = false;
            }

            return res;
        }

        private static List<string> Validate(List<Condition> currentConditions, List<Condition> conditions, bool update)
        {
            List<string> errors = new List<string>();

            foreach (Condition condition in conditions)
            {
                Condition currentCondition = currentConditions.Find(x => x.ConditionId == condition.ConditionId);

                if (update && currentCondition == null)
                {
                    errors.Add("Invalid Condition Id");
                }

                // Names are read only. They are being updated from Finale
                condition.Name = currentCondition.Name;


                //remove parentheses from code
                condition.Code = condition.Code?.Trim().Replace("(", "").Replace(")", "");

                if(condition.Convertible && string.IsNullOrEmpty(condition.Code))
                {
                    errors.Add("Missing Condition Code");
                }
            }

            return errors;
        }
    }
}