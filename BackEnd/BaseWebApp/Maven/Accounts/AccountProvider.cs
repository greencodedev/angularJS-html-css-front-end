using BaseWebApp.Maven.Utils.Api;
using BaseWebApp.Maven.Utils.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.Accounts
{
    public class AccountProvider
    {

        public static ProviderResponse UpdateAccount(Account account, int AccountId)
        {

            ProviderResponse res = new ProviderResponse();
            try
            {
                if(string.IsNullOrEmpty(account.CompanyName) || string.IsNullOrEmpty(account.Email))
                {
                    res.Messages.Add("CompanyName and Email is required");
                    return res;
                }

                AccountUpdate(account, AccountId);
                res.Data =  GetAccount(AccountId);
            }
            catch (Exception e)
            {
                Utils.Logger.Log(e.ToString());
                res.Messages.Add("An error occurred");
            }

            return res;
        }

        public static ProviderResponse GetAccount(int AccountId)
        {
            ProviderResponse res = new ProviderResponse();

            try
            {
                string query = "select * from Accounts where AccountId = @AccountId";

                SqlQuery sqlQuery = new SqlQuery();
                sqlQuery.AddParam("@AccountId", AccountId);

                using(Sql sql = new Sql())
                {
                    using(SqlReader reader = sqlQuery.ExecuteReader(sql, query))
                    {
                        if (reader.HasNext())
                        {
                            res.Data = new Account
                            {
                                AccountId = reader.GetInt("AccountId"),
                                CompanyName = reader.GetString("CompanyName"),
                                FirstName = reader.GetString("FirstName"),
                                LastName = reader.GetString("LastName"),
                                Address = reader.GetString("Address"),
                                Phone = reader.GetString("Phone"),
                                Email = reader.GetString("Email"),
                                Status = reader.GetString("Status"),
                            };
                        }
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

        private static void AccountUpdate(Account account, int AccountId)
        {

            string query = "UPDATE Accounts SET CompanyName = @CompanyName, Email = @Email, FirstName = @FirstName, LastName = @LastName, Phone = @Phone, Address = @Address, Status = @Status WHERE AccountId = @AccountId";

            SqlQuery sqlQuery = new SqlQuery();

            sqlQuery.AddParam("@CompanyName", account.CompanyName);
            sqlQuery.AddParam("@Email", account.Email);
            sqlQuery.AddParam("@FirstName", account.FirstName);
            sqlQuery.AddParam("@LastName", account.LastName);
            sqlQuery.AddParam("@Phone", account.Phone);
            sqlQuery.AddParam("@Address", account.Address);
            sqlQuery.AddParam("@Status", account.Status);
            sqlQuery.AddParam("@AccountId", AccountId);

            using(Sql sql = new Sql())
            {
               sqlQuery.ExecuteNonQuery(sql, query);
            }
        }
    }
}