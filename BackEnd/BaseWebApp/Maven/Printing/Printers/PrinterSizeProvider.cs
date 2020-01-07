using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BaseWebApp.Maven.PrintNode;
using BaseWebApp.Maven.PrintNode.Printers;
using BaseWebApp.Maven.Users;
using BaseWebApp.Maven.Utils;
using BaseWebApp.Maven.Utils.Api;
using BaseWebApp.Maven.Utils.Sql;

namespace BaseWebApp.Maven.Printing.Printers
{
    public class PrinterSizeProvider
    {
        public static ProviderResponse InsertOrUpdate(PrinterSize printerSize)
        {
            ProviderResponse response = new ProviderResponse();

            PrinterSize currentPrinterSize = GetPrinterSize(printerSize.PrinterSizeId);

            try
            {
                using (Sql sql = new Sql())
                {
                    if (currentPrinterSize != null)
                    {
                        Update(sql, printerSize);
                    }
                    else
                    {
                        //Insert(sql, printerSize);
                    }

                    response.Data = printerSize;
                }
            }
            catch(Exception e)
            {
                Logger.Log(e.ToString());
                response.Messages.Add("An Error occurred, see log for details");
                response.Success = false;
            }

            return response;
        }

        private static ProviderResponse Update(Sql sql, PrinterSize printerSize)
        {
            ProviderResponse response = new ProviderResponse();

            string query = @"UPDATE PrinterSizes 
                                SET Zpl = @Zpl
                                WHERE PrinterSizeId = @PrinterSizeId and AccountId = @AccountId ";

            try
            {
                SqlQuery sqlQuery = new SqlQuery();
                sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());
                sqlQuery.AddParam("@PrinterSizeId", printerSize.PrinterSizeId);
                sqlQuery.AddParam("@Zpl", printerSize.Zpl);

                printerSize.PrinterSizeId = sqlQuery.ExecuteNonQuery(sql, query);

                response.Data = printerSize;
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                response.Messages.Add("An Error occurred, see log for details");
                response.Success = false;
            }

            return response;
        }

        /*private static void Insert(Sql sql, PrinterSize printerSize)
        {
            ProviderResponse response = new ProviderResponse();

            string query = @"INSERT INTO PrinterSizes(Name, Zpl) 
                                         VALUES (@Name, @Zpl)";

            try
            {
                SqlQuery sqlQuery = new SqlQuery();
                sqlQuery.AddParam("@Name", printerSize.Name);
                sqlQuery.AddParam("@Zpl", printerSize.Zpl);

                printerSize.PrinterSizeId = (int)sqlQuery.ExecuteInsert(sql, query);

                response.Data = printerSize;
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                response.Messages.Add("An Error occurred, see log for details");
                response.Success = false;
            }
        }*/

        public static PrinterSize GetPrinterSize(int printerSizeId)
        {
            SearchCriteria search = new SearchCriteria
            {
                Id = printerSizeId
            };

            List<PrinterSize> printerSizes = (List<PrinterSize>)GetPrinterSizes(search).Data;

            return printerSizes.FirstOrDefault();
        }

        public static ProviderResponse GetPrinterSizes(SearchCriteria search)
        {
            ProviderResponse response = new ProviderResponse();
            try
            {
                response.Data = GetPrinterSizeList(search);
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                response.Messages.Add("An Error occurred, see log for details");
                response.Success = false;
            }

            return response;
        }

        public static List<PrinterSize> GetPrinterSizeList(SearchCriteria search)
        {
            List<PrinterSize> printerSizes = new List<PrinterSize>();

            string query = @"SELECT *  
                            FROM PrinterSizes 
                            WHERE AccountId = @AccountId ";
            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());

            if (search.Id != null)
            {
                query += "and PrinterSizeId = @PrinterSizeId ";
                sqlQuery.AddParam("@PrinterSizeId", search.Id);
            }
            if (!string.IsNullOrEmpty(search.Name))
            {
                query += "AND Name = @Name ";
                sqlQuery.AddParam("@Name", search.Name);
            }

            using (Sql sql = new Sql())
            {
                SqlReader reader = sqlQuery.ExecuteReader(sql, query);

                while (reader.HasNext())
                {
                    PrinterSize printerSize = new PrinterSize
                    {
                        PrinterSizeId = reader.GetInt("PrinterSizeId"),
                        Name = reader.GetString("Name"),
                        Zpl = reader.GetString("Zpl"),
                        Label = reader.GetString("Label")
                    };

                    printerSizes.Add(printerSize);
                }

                return printerSizes;
            }
        }
    }
}