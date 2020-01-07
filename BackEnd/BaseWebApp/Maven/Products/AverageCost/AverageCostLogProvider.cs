using BaseWebApp.Maven.Utils;
using BaseWebApp.Maven.Utils.Api;
using BaseWebApp.Maven.Utils.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.Products.AverageCost
{
    public class AverageCostLogProvider
    {

        public static void InsertLogs(List<AverageCostLog> logs)
        {
            if(logs.Any())
            {
                string query = "INSERT INTO AverageCostLog (FinalId, TransactionType, CurrAvgCost, Quantity, CostPerUnit, CurrentStock, TransactionDescription, ReceivedDate, NewAverageCost) VALUES ";

                SqlQuery sqlQuery = new SqlQuery();

                for (int i = 0; i < logs.Count; i++)
                {
                    query += "( @FinalId" + i +
                             ", @TransactionType" + i +
                             ", @CurrAvgCost" + i +
                             ", @Quantity" + i +
                             ", @CostPerUnit" + i +
                             ", @CurrentStock" + i +
                             ", @TransactionDescription" + i +
                             ", @ReceivedDate" + i +
                             ", @NewAverageCost" + i + " )";

                    if (i != logs.Count - 1)
                    {
                        query += " , ";
                    }

                    sqlQuery.AddParam("@FinalId" + i, logs[i].FinalProductId);
                    sqlQuery.AddParam("@TransactionType" + i, logs[i].TransactionType);
                    sqlQuery.AddParam("@CurrAvgCost" + i, logs[i].CurrAvgCost);
                    sqlQuery.AddParam("@Quantity" + i, logs[i].Quantity);
                    sqlQuery.AddParam("@CostPerUnit" + i, logs[i].CostPerUnit);
                    sqlQuery.AddParam("@CurrentStock" + i, logs[i].CurrentStock);
                    sqlQuery.AddParam("@TransactionDescription" + i, logs[i].TransactionDescription);
                    sqlQuery.AddParam("@ReceivedDate" + i, logs[i].ReceivedDate);
                    sqlQuery.AddParam("@NewAverageCost" + i, logs[i].NewAverageCost);
                }

                using (Sql sql = new Sql())
                {
                    sqlQuery.ExecuteInsert(sql, query);
                }

            }


        }

        public static AverageCostLog GetLastLog(string finaleid, string tnxType)
        {
            AverageCostLog lastLog = null;

            string query = "SELECT * FROM AverageCostLog WHERE 1 = 1 ";

            SqlQuery sqlQuery = new SqlQuery();

            if (!string.IsNullOrEmpty(finaleid))
            {
                query += " and FinalId = @FinalId ";
                sqlQuery.AddParam("@FinalId", finaleid);
            }
            if(!string.IsNullOrEmpty(tnxType))
            {
                query += " and TransactionType = @tnxType ";
                sqlQuery.AddParam("@tnxType", tnxType);
            }


            query += "order by TransactionId DESC LIMIT 1";

            using (Sql sql = new Sql())
            {
                using (SqlReader reader = sqlQuery.ExecuteReader(sql, query))
                {
                    if (reader.HasNext())
                    {
                        lastLog = new AverageCostLog
                        {
                            TransactionId = reader.GetInt("TransactionId"),
                            FinalProductId = reader.GetString("FinalId"),
                            TransactionType = reader.GetString("TransactionType"),
                            CurrAvgCost = reader.GetDoubleOrZero("CurrAvgCost"),
                            Quantity = reader.GetInt("Quantity"),
                            CostPerUnit = reader.GetDouble("CostPerUnit"),
                            CurrentStock = reader.GetInt("CurrentStock"),
                            NewAverageCost = reader.GetDoubleOrZero("NewAverageCost"),
                            TransactionDescription = reader.GetString("TransactionDescription"),
                            ReceivedDate = reader.GetOptionalTime("ReceivedDate")
                        };
                    }
                }
            }

            return lastLog;
        }

        public static ProviderResponse GetAvgCostLogs(SearchCriteria search)
        {
            ProviderResponse response = new ProviderResponse();
            ListReponse listReponse = new ListReponse();

            try
            {
                if (search == null)
                {
                    search = new SearchCriteria();
                }

                //search.Status = true;
                search.Paginate = true;

                listReponse.List = SearchLogs(search);

                search.Paginate = false;
                listReponse.TotalCount = SearchLogs(search).Count;

                response.Data = listReponse;

            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                response.Messages.Add("Error");
            }


            return response;
        }

        public static List<AverageCostLog> SearchLogs(SearchCriteria search)
        {
            List<AverageCostLog> logs = new List<AverageCostLog>();

            string query = @"SELECT l.*, p.Name 
                             FROM AverageCostLog l	
	                            left join Products p on (p.FinaleId = l.FinalId)
                            where 1 = 1 ";

            SqlQuery sqlQuery = new SqlQuery();

            if(!string.IsNullOrEmpty(search.FinaleId))
            {
                query += "and l.FinalId = @FinaleId";
                sqlQuery.AddParam("@FinaleId", search.FinaleId);
            }
            if (search.ProductId != 0)
            {
                query += "AND p.ProductId = @ProductId ";
                sqlQuery.AddParam("@ProductId", search.ProductId);
            }
            if (!string.IsNullOrEmpty(search.TnxType))
            {
                query += " and l.TransactionType = @tnxType ";
                sqlQuery.AddParam("@tnxType", search.TnxType);
            }
            if (!string.IsNullOrEmpty(search.TextSearch))
            {
                query += sqlQuery.AddSearchTerm(search.TextSearch, new List<string>
                {
                    "l.FinalId", "l.TransactionType", "l.TransactionDescription", "l.CurrAvgCost", "l.Quantity", "l.CostPerUnit", "l.CurrentStock", "l.NewAverageCost",
                    "p.ProductId", "p.Name"
                });
            }
            if (search.FromDate != null && search.ToDate != null)
            {
                query += "AND ReceivedDate BETWEEN @Start AND @End ";
                sqlQuery.AddParam("@Start", search.FromDate);
                sqlQuery.AddParam("@End", search.ToDate);
            }

            if (!string.IsNullOrEmpty(search.SortBy))
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
            using (SqlReader reader = sqlQuery.ExecuteReader(sql, query))
            {
                while (reader.HasNext())
                {
                    logs.Add(new AverageCostLog
                    {
                        TransactionId = reader.GetInt("TransactionId"),
                        FinalProductId = reader.GetString("FinalId"),
                        TransactionType = reader.GetString("TransactionType"),
                        CurrAvgCost = reader.GetDoubleOrZero("CurrAvgCost"),
                        Quantity = reader.GetInt("Quantity"),
                        CostPerUnit = reader.GetDouble("CostPerUnit"),
                        CurrentStock = reader.GetInt("CurrentStock"),
                        NewAverageCost = reader.GetDoubleOrZero("NewAverageCost"),
                        TransactionDescription = reader.GetString("TransactionDescription"),
                        ReceivedDate = reader.GetOptionalTime("ReceivedDate"),
                        ProductName = reader.GetString("Name")
                    });
                }
            }

            return logs;
        }
    }

    public class AverageCostLog
    {
        public int TransactionId { get; set; }
        public string FinalProductId { get; set; }
        public string TransactionType { get; set; }
        public string TransactionDescription { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public double CurrAvgCost { get; set; }
        public int Quantity { get; set; }
        public double? CostPerUnit { get; set; }
        public int CurrentStock { get; set; }
        public double NewAverageCost { get; set; }
        public string ProductName { get; set; }
    }
}