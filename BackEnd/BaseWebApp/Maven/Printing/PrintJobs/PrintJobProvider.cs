using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BaseWebApp.Maven.Printing.Printers;
using BaseWebApp.Maven.PrintNode;
using BaseWebApp.Maven.PrintNode.Printers;
using BaseWebApp.Maven.PrintNode.PrintJobs;
using BaseWebApp.Maven.Users;
using BaseWebApp.Maven.Utils;
using BaseWebApp.Maven.Utils.Api;
using BaseWebApp.Maven.Utils.Sql;

namespace BaseWebApp.Maven.Printing.PrintJobs
{
    public class PrintJobProvider
    {
        public static ProviderResponse CreatePrintJob(PrintJob printJob)
        {
            ProviderResponse res = new ProviderResponse();
            
            res.Messages.AddRange(validatePrintJob(printJob));

            if(!res.Success)
            {
                return res;
            }

            try
            {
                if(printJob.FromConvert)
                {
                    printJob.Printer.Size = PrinterSizeProvider.GetPrinterSizeList(new SearchCriteria { Name = "Small Convert" })[0];
                }

                printJob.PrintNodePrintJobId = PrintNodeProvider.CreatePrintJob(printJob);

                SavePrintLog(printJob);
                
                res.Data = printJob;
            }
            catch (Exception e)
            {
                res.Success = false;
                res.Messages.Add("Something went wrong");
                Logger.Log(e.ToString());
            }

            return res;
        }

        private static List<string> validatePrintJob(PrintJob printJob)
        {
            List<string> errors = new List<string>();

            if(printJob == null)
            {
                errors.Add("Missing Product");
            }
            else
            {
                if(printJob.Product == null || printJob.Product.ProductId == 0)
                {
                    errors.Add("Missing Product");
                }
                else
                {
                    List<Products.Product> list = Products.ProductsProvider.SearchProducts(new SearchCriteria {
                        Id = printJob.Product.ProductId
                    });
                    
                    if(list.Any())
                    {
                        printJob.Product = list[0];
                    }
                    else
                    {
                        errors.Add("Invalid Product");
                    }
                }

                if(printJob.Printer == null || printJob.Printer.PrinterId == 0)
                {
                    errors.Add("Missing Printer");
                }
                else
                {
                    Printer printer = PrinterProvider.GetPrinter(printJob.Printer.PrinterId);

                    if(printer != null)
                    {
                        printJob.Printer = printer;

                        if(!printer.Active)
                        {
                            errors.Add("Printer is inactive");
                        }
                    }
                    else
                    {
                        errors.Add("Invalid Printer");
                    }
                }

                if(printJob.Quantity < 1 || printJob.Quantity > 999)
                {
                    errors.Add("You can only print 1 to 999 copies");
                }
            }
            
            return errors;
        }

        public static ProviderResponse GetPrintJob(int printJobId)
        {
            ProviderResponse response = new ProviderResponse();

            try
            {
                PrintJob res = PrintNodeProvider.GetPrintJob(printJobId);

                response.Data = res;

                UpdatePrintLogStatus(printJobId, res.Status);
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                response.Messages.Add("An Error occurred, see log for details");
            }

            return response;
        }

        public static ProviderResponse GetPrintJobLogs(SearchCriteria search)
        {
            ProviderResponse response = new ProviderResponse();
            ListReponse listReponse = new ListReponse();

            if (search == null)
            {
                search = new SearchCriteria();
            }

            search.Status = true;
            search.Paginate = true;

            listReponse.List = searchPrintJobLogs(search);

            search.Paginate = false;
            listReponse.TotalCount = searchPrintJobLogs(search).Count;

            response.Data = listReponse;

            return response;
        }

        public static ProviderResponse ExportPrintJobLogs(SearchCriteria search)
        {
            ProviderResponse response = new ProviderResponse();
            ListReponse listReponse = new ListReponse();

            if (search == null)
            {
                search = new SearchCriteria();
            }

            search.Status = true;
            search.Paginate = true;

            listReponse.List = searchPrintJobLogs(search);

            search.Paginate = false;
            listReponse.TotalCount = searchPrintJobLogs(search).Count;

            response.Data = listReponse;

            return response;
        }

        public static List<PrintJob> searchPrintJobLogs(SearchCriteria search)
        {
            List<PrintJob> jobs = new List<PrintJob>();


            using (Sql sql = new Sql())
            {
                SqlQuery sqlQuery = new SqlQuery();

                string query = @"SELECT pj.*, pri.CustomName printerName, pri.Id PrintNodePrinterId, pro.Name productName, pro.Description productDescription, pro.FinaleId productFinaleId, c.Name cName, c.Code cCode, u.FirstName, u.LastName, u.Initials 
                                 FROM PrintJobs pj
                                 LEFT JOIN Printers pri ON (pri.PrinterId = pj.PrinterId) 
                                 LEFT JOIN Products pro ON (pro.ProductId = pj.ProductId) 
                                 LEFT JOIN Conditions c ON (c.ConditionId = pro.ConditionId) 
                                 LEFT JOIN idty_users u ON (u.UserId = pj.UserId) 
                                 WHERE pj.AccountId = @AccountId ";

                sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());

                if (search.Id != null)
                {
                    query += "AND PrintJobId = @PrintJobId ";
                    sqlQuery.AddParam("@PrintJobId", search.Id);
                }

                if (search.printJobStatus != MssPrintJobStatus.Null)
                {
                    query += "AND pj.Status = @Status ";
                    sqlQuery.AddParam("@Status", search.printJobStatus.ToString());
                }

                if (search.PrinterId != 0)
                {
                    query += "AND pj.PrinterId = @PrinterId ";
                    sqlQuery.AddParam("@PrinterId", search.PrinterId);
                }

                if (search.ProductId != 0)
                {
                    query += "AND pj.ProductId = @ProductId ";
                    sqlQuery.AddParam("@ProductId", search.ProductId);
                }

                if (!string.IsNullOrEmpty(search.UserId))
                {
                    query += "AND pj.UserId =  @UserId ";
                    sqlQuery.AddParam("@UserId", search.UserId);
                }

                if(search.FromDate != null && search.ToDate != null)
                {
                    query += "AND Created BETWEEN @Start AND @End ";
                    sqlQuery.AddParam("@Start", search.FromDate);
                    sqlQuery.AddParam("@End", search.ToDate);
                }

                if (!string.IsNullOrEmpty(search.TextSearch))
                {
                    query += sqlQuery.AddSearchTerm(search.TextSearch, new List<string> { "u.UserId", "u.FirstName", "u.LastName", "u.Initials", "pri.Name", "pri.CustomName", "pri.Id" ,"pri.Description", "pro.Name", "pro.FinaleId", "pro.Description", "c.Code", "c.Name" });
                }


                if (search.SortBy != null)
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

                using (SqlReader reader = sqlQuery.ExecuteReader(sql, query))
                {
                    while (reader.HasNext())
                    {
                        PrintJob printJob = new PrintJob
                        {
                            PrintJobId = reader.GetInt("PrintJobId"),
                            PrintNodePrintJobId = reader.GetInt("PrintNodePrintJobId"),
                            Quantity = reader.GetInt("Quantity"),
                            Status = reader.ParseEnum<MssPrintJobStatus>("Status"),
                            Product = new Products.Product
                            {
                                ProductId = reader.GetInt("ProductId"),
                                Name = reader.GetString("productName"),
                                Description = reader.GetString("productDescription"),
                                FinaleId = reader.GetString("productFinaleId"),
                                Condition = new Products.Conditions.Condition
                                {
                                    Name = reader.GetString("cName"),
                                    Code = reader.GetString("cCode")
                                }
                            },
                            Printer = new Printer
                            {
                                PrinterId = reader.GetInt("PrinterId"),
                                Id = reader.GetString("PrintNodePrinterId"),
                                CustomName = reader.GetString("printerName")
                            },
                            user = new User
                            {
                                FirstName = reader.GetString("FirstName"),
                                LastName = reader.GetString("LastName"),
                                Initials = reader.GetString("Initials")
                            },
                            DateTime = reader.GetTime("Created")
                        };

                        jobs.Add(printJob);
                    }
                }
            }

            return jobs;
        }

        private static void SavePrintLog(PrintJob printJob)
        {
            using (Sql sql = new Sql())
            {
                string query = @"INSERT INTO PrintJobs(AccountId, PrintNodePrintJobId, UserId, PrinterId, ProductId, Quantity, Status) 
                                         VALUES (@AccountId, @PrintNodePrintJobId, @UserId, @PrinterId, @ProductId, @Quantity, @Status)";

                SqlQuery sqlQuery = new SqlQuery();
                sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());
                sqlQuery.AddParam("@PrintNodePrintJobId", printJob.PrintNodePrintJobId);
                sqlQuery.AddParam("@UserId", ThreadProperties.GetUser().Id);
                sqlQuery.AddParam("@PrinterId", printJob.Printer.PrinterId);
                sqlQuery.AddParam("@ProductId", printJob.Product.ProductId);
                sqlQuery.AddParam("@Quantity", printJob.Quantity);
                sqlQuery.AddParam("@Status", printJob.state.ToMssPrintJobStatus().ToString());

                sqlQuery.ExecuteInsert(sql, query);
            }
        }

        private static void UpdatePrintLogStatus(int printJobId, MssPrintJobStatus status)
        {
            using (Sql sql = new Sql())
            {
                string query = @"UPDATE PrintJobs 
                                SET Status = @Status 
                                WHERE PrintNodePrintJobId = @PrintNodePrintJobId and AccountId = @AccountId;";

                SqlQuery sqlQuery = new SqlQuery();
                sqlQuery.AddParam("@PrintNodePrintJobId", printJobId);
                sqlQuery.AddParam("@Status", status.ToString());
                sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());

                sqlQuery.ExecuteNonQuery(sql, query);
            }
        }
    }
}