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
    public class PrinterProvider
    {
        public static ProviderResponse SyncPrinters()
        {
            ProviderResponse response = new ProviderResponse();
            string JobType = "SyncPrinters";
            int jobId = 0;

            try
            {
                if (!SyncProvider.IsJobRunning(JobType))
                {
                    jobId = SyncProvider.InsertJob(JobType);
                    
                    List<Printer> printers = PrintNodeProvider.GetPrinters();
                    InsertOrUpdate(printers);

                    SyncProvider.StopJob(jobId, true);
                }
            }
            catch(Exception e)
            {
                Logger.Log(e.ToString());
                response.Messages.Add("An Error occurred, see log for details");
                response.Success = false;

                if (jobId != 0)
                {
                    SyncProvider.StopJob(jobId, false);
                }
            }

            return response;
        }

        public static void InsertOrUpdate(List<Printer> printers)
        {
            SearchCriteria search = new SearchCriteria();
            List<Printer> currentPrinters = GetPrinters(search);

            using (Sql sql = new Sql())
            {
                foreach (Printer printer in printers)
                {
                    Printer currentPrinter = currentPrinters.Find(x => x.Id == printer.Id);

                    if (currentPrinter != null)
                    {
                        Printer updatePrinter = UtilsLib.Clone(currentPrinter);
                        updatePrinter.Name = printer.Name;
                        updatePrinter.Description = printer.Description;
                        updatePrinter.State = printer.State;

                        Update(sql, updatePrinter, currentPrinter);
                    }
                    else
                    {
                        Insert(sql, printer);
                    }
                }

                foreach (Printer oldPrinter in currentPrinters)
                {
                    Printer newPrinter = printers.Find(x => x.Id == oldPrinter.Id);

                    //old printer not in printNode
                    if(newPrinter == null)
                    {
                        Printer oldNewPrinter = UtilsLib.Clone(oldPrinter);
                        oldNewPrinter.Active = false;
                        Update(sql, oldNewPrinter, oldPrinter);
                    }
                }
            }
        }

        public static void Update(Sql sql, Printer printer, Printer currentPrinter)
        {
            string query = @"UPDATE Printers
                                SET PrinterSizeId = @PrinterSizeId,
                                Id = @Id,
                                Name = @Name,
                                Description = @Description,
                                CustomName = @CustomName,
                                State = @State,
                                Active = @Active
                            WHERE PrinterId = @PrinterId and AccountId = @AccountId ";

            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());
            sqlQuery.AddParam("@PrinterId", currentPrinter.PrinterId);
            sqlQuery.AddParam("@PrinterSizeId", printer.Size?.PrinterSizeId);
            sqlQuery.AddParam("@Id", printer.Id);
            sqlQuery.AddParam("@Name", printer.Name);
            sqlQuery.AddParam("@Description", printer.Description);
            sqlQuery.AddParam("@CustomName", printer.CustomName);
            sqlQuery.AddParam("@State", printer.State.ToString());
            sqlQuery.AddParam("@Active", printer.Active);

            sqlQuery.ExecuteNonQuery(sql, query);
        }

        private static void Insert(Sql sql, Printer printer)
        {
            string query = @"INSERT INTO Printers(AccountId, PrinterSizeId, Id, Name, Description, State) 
                                         VALUES (@AccountId, @PrinterSizeId, @Id, @Name, @Description, @State)";

            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());
            sqlQuery.AddParam("@PrinterSizeId", printer.Size?.PrinterSizeId);
            sqlQuery.AddParam("@Id", printer.Id);
            sqlQuery.AddParam("@Name", printer.Name);
            sqlQuery.AddParam("@Description", printer.Description);
            sqlQuery.AddParam("@State", printer.State.ToString());

            sqlQuery.ExecuteInsert(sql, query);
        }

        public static Printer GetPrinter(int printerId) {
            
            SearchCriteria search = new SearchCriteria
            {
                Id = printerId
            };

            List<Printer> printers = GetPrinters(search);

            return printers.FirstOrDefault();
        }

        public static List<Printer> GetPrinters(SearchCriteria search)
        {
            ProviderResponse response = new ProviderResponse();
            List<Printer> printers = new List<Printer>();


            Users.User currUser = Users.UsersProvider.GetCurrentUser();
            bool joinUserPrinters = (currUser.IsAdmin && search.OnlyAssingedPrinters) || !currUser.IsAdmin || !string.IsNullOrEmpty(search.UserId);


            string query = @"SELECT *, p.Name pName, ps.Name psName 
                            FROM Printers p 
                            LEFT JOIN PrinterSizes ps ON ps.PrinterSizeId = p.PrinterSizeId ";

            if(joinUserPrinters)
            {
                query += "LEFT JOIN UserPrinters up ON (up.PrinterId = p.PrinterId and up.UserId = @UserId) ";
            }

            query += "WHERE p.AccountId = @AccountId ";

            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());

            if (!currUser.IsAdmin)
            {
                query += "and up.UserId = @UserId ";
                sqlQuery.AddParam("@UserId", currUser.UserId);
            }
            else if(!string.IsNullOrEmpty(search.UserId))
            {
                sqlQuery.AddParam("@UserId", search.UserId);
            }
            else if(currUser.IsAdmin && search.OnlyAssingedPrinters)
            {
                sqlQuery.AddParam("@UserId", currUser.UserId);
            }

            if (search.Id != null)
            {
                query += "and p.PrinterId = @Id ";
                sqlQuery.AddParam("@Id", search.Id);
            }
            if (!string.IsNullOrEmpty(search.Name))
            {
                query += "AND Name = @Name ";
                sqlQuery.AddParam("@Name", search.Name);
            }
            if (search.Status == true)
            {
                query += "AND Active = @Active ";
                sqlQuery.AddParam("@Active", 1);
            }
            if (search.PrinterState != null)
            {
                query += "AND State = @State ";
                sqlQuery.AddParam("@State", search.PrinterState.ToString());
            } else
            {
                query += "AND State != @Disappeared ";
                sqlQuery.AddParam("@Disappeared", PrinterStates.disappeared.ToString());
            }
            

            query += sqlQuery.AddSearchTerm(search.TextSearch, new List<string> { "p.Id", "p.Name", "p.Description", "p.CustomName", "ps.Name" });

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


           
            using (Sql sql = new Sql())
            {
                SqlReader reader = sqlQuery.ExecuteReader(sql, query);

                while (reader.HasNext())
                {
                    Printer printer = new Printer
                    {
                        PrinterId = reader.GetInt("PrinterId"),
                        Id = reader.GetString("Id"),
                        Name = reader.GetString("pName"),
                        Description = reader.GetString("Description"),
                        CustomName = reader.GetString("CustomName"),
                        State = (PrinterStates)Enum.Parse(typeof(PrinterStates), reader.GetString("State")),
                        Active = reader.GetBoolean("Active")
                    };

                    if(!string.IsNullOrEmpty(search.UserId))
                    {
                        printer.IsAssigned = reader.GetString("UserId") == search.UserId;
                    }
                    else if((currUser.IsAdmin && search.OnlyAssingedPrinters) || !currUser.IsAdmin)
                    {
                        printer.IsAssigned = reader.GetString("UserId") == currUser.UserId;
                    }

                    int? PrinterSizeId = reader.GetInteger("PrinterSizeId");
                    if (PrinterSizeId.HasValue)
                    {
                        printer.Size = new PrinterSize
                        {
                            PrinterSizeId = reader.GetIntOrZero("PrinterSizeId"),
                            Name = reader.GetString("psName"),
                            Zpl = reader.GetString("Zpl"),
                            Label = reader.GetString("Label")
                        };
                    }

                    printers.Add(printer);
                }

                   

                    
            }
            
           return printers;
        }

        public static ProviderResponse GetAllPrinters(SearchCriteria search)
        {
            ProviderResponse response = new ProviderResponse();

            try
            {
                if (search == null)
                {
                    search = new SearchCriteria();
                }

                ListReponse listReponse = new ListReponse();
                listReponse.List = GetPrinters(search);
                
                if(search.Paginate)
                {
                    search.Paginate = false;
                    listReponse.TotalCount = GetPrinters(search).Count;
                }
                
                response.Data = listReponse;

            }
            catch(Exception e)
            {
                Logger.Log(e.ToString());
                response.Messages.Add("an error has occurred, please try again later");
            }
           
            return response;
        }



        public static ProviderResponse UpdatePrinter(Printer printer)
        {
            ProviderResponse response = new ProviderResponse();

            Printer currentPrinter = GetPrinter(printer.PrinterId);

            //Validation
            response.Messages.AddRange(Validate(currentPrinter, printer, true));

            if (response.Messages.Any())
            {
                return response;
            }

            try
            {
                using (Sql sql = new Sql())
                {
                    Update(sql, printer, currentPrinter);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                response.Messages.Add("An Error occurred, see log for details");
            }

            return response;
        }

        private static List<string> Validate(Printer currentPrinter, Printer printer, bool update)
        {
            List<string> errors = new List<string>();

            // If this is an update, then currentPrinter should already have something. Otherwise it's an invalid printerId.
            if (update && currentPrinter == null)
            {
                errors.Add("Invalid Printer Id");
            }
            if(printer.Size.PrinterSizeId == 0)
            {
                printer.Size = null;
            }
            if (printer.Size != null)
            {
                printer.Size = PrinterSizeProvider.GetPrinterSize(printer.Size.PrinterSizeId);

                if (printer.Size == null)
                {
                    errors.Add("Printer Size is invalid");
                }
            }
            else if(printer.Active)
            {
                errors.Add("You cannot activate a printer without a size");
            }
            if(printer.Active && string.IsNullOrEmpty(printer.CustomName))
            {
                errors.Add("You cannot activate a printer without a custom name");
            }

            return errors;
        }
    }
}