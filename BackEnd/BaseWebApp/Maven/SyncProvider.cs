using BaseWebApp.Maven.Users;
using BaseWebApp.Maven.Utils.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven
{
    public class SyncProvider
    {
        public static bool IsJobRunning(string jobType)
        {
            string query = "select * from Jobs where JobType = @JobType and Result = @Result and AccountId = @AccountId ";

            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.AddParam("@JobType", jobType);
            sqlQuery.AddParam("@Result", "Running");
            sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());

            using (Sql sql = new Sql())
            using (SqlReader reader = sqlQuery.ExecuteReader(sql, query))
            {
                if (reader.HasNext())
                {
                    return true;
                }
            }

            return false;
        }

        public static int InsertJob(string jobType)
        {
            string query = "Insert into Jobs (AccountId, JobType, StartTime, Result) values (@AccountId, @JobType, @StartTime, @Result)";

            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());
            sqlQuery.AddParam("@JobType", jobType);
            sqlQuery.AddParam("@StartTime", DateTime.UtcNow);
            sqlQuery.AddParam("@Result", "Running");

            using (Sql sql = new Sql())
            {
                return (int)sqlQuery.ExecuteInsert(sql, query);
            }
        }

        public static void StopJob(int jobId, bool success)
        {
            string query = "Update Jobs set EndTime = @EndTime, Result = @Result where JobId = @JobId and AccountId = @AccountId";

            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());
            sqlQuery.AddParam("@EndTime", DateTime.UtcNow);
            sqlQuery.AddParam("@Result", success ? "Success" : "Error");
            sqlQuery.AddParam("@JobId", jobId);

            using (Sql sql = new Sql())
            {
                sqlQuery.ExecuteNonQuery(sql, query);
            }
        }

        public static DateTime LastSynced(string jobType)
        {
            DateTime lastSynced = new DateTime();

            string query = "select * from Jobs where JobType = @JobType and Result = 'Success' and AccountId = @AccountId order by EndTime desc ";

            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());
            sqlQuery.AddParam("@JobType", jobType);

            using (Sql sql = new Sql())
            using (SqlReader reader = sqlQuery.ExecuteReader(sql, query))
            {
                if (reader.HasNext())
                {
                    lastSynced = reader.GetUtcTime("EndTime");
                }
            }

            return lastSynced;
        }
    }
}