using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BaseWebApp.Maven.Products;
using BaseWebApp.Maven.Users;
using BaseWebApp.Maven.Utils;
using BaseWebApp.Maven.Utils.Api;
using BaseWebApp.Maven.Utils.Sql;

namespace BaseWebApp.Maven.Sublocations
{
    public class SublocationProvider
    {
        public static ProviderResponse GetSublocationList(SearchCriteria search, int channelId)
        {
            ProviderResponse response = new ProviderResponse();

            try
            {
                response.Data = GetSublocations(search, channelId);
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                response.Messages.Add("Something Went Wrong.");
                response.Success = false;
            }

            return response;
        }

        public static List<Sublocation> GetSublocations(SearchCriteria search, int channelId)
        {
            List<Sublocation> sublocations = new List<Sublocation>();

            Users.User currUser = Users.UsersProvider.GetCurrentUser();
            bool joinUserLocations = !currUser.IsAdmin || !string.IsNullOrEmpty(search.UserId);

            string query = @"select s.SublocationId sSublocationId, s.FinaleSublocationId sFinaleSublocationId, s.Name sName, s.Description sDescription, s.SublocationStatus sSublocationStatus, s.Url sUrl, s.ConvertibleFrom sConvertibleFrom, s.ConvertibleTo sConvertibleTo, 
                            p.SublocationId pSublocationId, p.FinaleSublocationId pFinaleSublocationId, p.Name pName, p.Description pDescription, p.SublocationStatus pSublocationStatus, p.Url pUrl, p.ConvertibleFrom pConvertibleFrom, p.ConvertibleTo pConvertibleTo";

            if (joinUserLocations)
            {
                query += ", us.ConvertTo usConvertTo, us.ConvertFrom usConvertFrom";
            }

            query += @" from Sublocations s 
                        left join Sublocations p on (p.SublocationId = s.ParentId) ";

            if(joinUserLocations)
            {
                query += "left join UserLocations us ON (us.SublocationId = s.SublocationId and us.UserId = @UserId) ";

            }
                            
            query += "WHERE s.AccountId = @AccountId ";

            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.AddParam("@AccountId", UsersProvider.GetCurrentAccountId());

            if (!currUser.IsAdmin)
            {
                query += "and us.UserId = @UserId ";
                sqlQuery.AddParam("@UserId", currUser.UserId);
            }
            else if (!string.IsNullOrEmpty(search.UserId))
            {
                sqlQuery.AddParam("@UserId", search.UserId);
            }

            if (search.Status != null && search.Status == true)
            {
                query += "AND s.SublocationStatus = @Status ";
                sqlQuery.AddParam("@Status", SublocationStatus.FACILITY_ACTIVE.ToString());
            }

            if (search.Id != null)
            {
                query += "and s.SublocationId = @Id ";
                sqlQuery.AddParam("@Id", search.Id);
            }

            if(search.OnlySublocation)
            {
                query += "and s.ParentId is not null ";
            }

            if(!string.IsNullOrEmpty(search.TextSearch))
            {
                query += sqlQuery.AddSearchTerm(search.TextSearch, new List<string> { "s.SublocationId", "s.FinaleSublocationId", "s.Name", "s.Description" });
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

            using (Sql sql = new Sql())
            using (SqlReader reader = sqlQuery.ExecuteReader(sql, query))
            {
                while (reader.HasNext())
                {
                    Sublocation sublocation = new Sublocation
                    {
                        SublocationId = reader.GetInteger("sSublocationId"),
                        FinaleSublocationId = reader.GetString("sFinaleSublocationId"),
                        Name = reader.GetString("sName"),
                        Description = reader.GetString("sDescription"),
                        Status = reader.ParseEnum<SublocationStatus>("sSublocationStatus"),
                        Url = reader.GetString("sUrl"),
                        ConvertibleFrom = reader.GetOptionalBoolean("sConvertibleFrom"),
                        ConvertibleTo = reader.GetOptionalBoolean("sConvertibleTo")
                    };

                    if(joinUserLocations)
                    {
                        sublocation.UserConvertibleFrom = reader.GetOptionalBoolean("usConvertFrom");
                        sublocation.UserConvertibleTo = reader.GetOptionalBoolean("usConvertTo");
                    }

                    if(reader.GetInteger("pSublocationId") != null)
                    {
                        sublocation.ParentLocation = new Sublocation
                        {
                            SublocationId = reader.GetInteger("pSublocationId"),
                            FinaleSublocationId = reader.GetString("pFinaleSublocationId"),
                            Name = reader.GetString("pName"),
                            Description = reader.GetString("pDescription"),
                            Status = reader.ParseEnum<SublocationStatus>("pSublocationStatus"),
                            Url = reader.GetString("pUrl"),
                            ConvertibleFrom = reader.GetOptionalBoolean("pConvertibleFrom"),
                            ConvertibleTo = reader.GetOptionalBoolean("pConvertibleTo")
                        };
                    }

                    sublocations.Add(sublocation);
                }
            }

            if (search.ProductId != 0)
            {
                Product product = ProductsProvider.SearchProducts(new SearchCriteria { Id = search.ProductId })[0];
                Dictionary<string, int> stock = FinaleInventory.FinaleClientProvider.GetStockPerLocation(product, false, channelId);

                foreach(Sublocation sublocation in sublocations) {
                    if(stock.ContainsKey(sublocation.Url))
                    {
                        sublocation.Stock = stock[sublocation.Url];
                    }
                }
            }

            return sublocations;
        }

        public static ProviderResponse InsertOrUpdateSublocations(List<Sublocation> sublocations, int channelId)
        {
            ProviderResponse res = new ProviderResponse();

            try
            {

                SqlQuery sqlQuery = new SqlQuery();

                string query = @"INSERT INTO Sublocations (FinaleSublocationId, AccountId, ParentId, Name, Description, SublocationStatus, Url, ConvertibleFrom, ConvertibleTo) 
                                VALUES ";

                for (int i = 0; i < sublocations.Count(); i++)
                {
                    query += "(@FinaleSublocationId" + i + ", @AccountId" + i + ", @ParentId" + i + ", @Name" + i + ", @Description" + i + ", @SublocationStatus" + i + ", @Url" + i + ", @ConvertibleFrom" + i + ", @ConvertibleTo" + i + ")";
                    sqlQuery.AddParam("@AccountId" + i, UsersProvider.GetCurrentAccountId());
                    sqlQuery.AddParam("@FinaleSublocationId" + i, sublocations[i].FinaleSublocationId);
                    sqlQuery.AddParam("@ParentId" + i, sublocations[i].ParentLocation?.SublocationId);
                    sqlQuery.AddParam("@Name" + i, sublocations[i].Name);
                    sqlQuery.AddParam("@Description" + i, sublocations[i].Description);
                    sqlQuery.AddParam("@SublocationStatus" + i, sublocations[i].Status.ToString());
                    sqlQuery.AddParam("@Url" + i, sublocations[i].Url);
                    sqlQuery.AddParam("@ConvertibleFrom" + i, Convert.ToInt32(sublocations[i].ConvertibleFrom));
                    sqlQuery.AddParam("@ConvertibleTo" + i, Convert.ToInt32(sublocations[i].ConvertibleTo));

                    if (i < sublocations.Count() - 1)
                    {
                        query += ", ";
                    }
                    else
                    {
                        query += " ";
                    }
                }

                query += @"ON DUPLICATE KEY UPDATE 
                        Name = VALUES(Name), 
                        Description = VALUES(Description), 
                        SublocationStatus = VALUES(SublocationStatus), 
                        Url = VALUES(Url),
                        ConvertibleFrom = VALUES(ConvertibleFrom),
                        ConvertibleTo = VALUES(ConvertibleTo); ";

                using (Sql sql = new Sql())
                {
                    sqlQuery.ExecuteInsert(sql, query);

                    res.Data = GetSublocations(new SearchCriteria(), channelId);
                }

            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                res.Success = false;
            }

            return res;
        }

        public static ProviderResponse UpdateUserLocations(string userId, List<Sublocation> sublocations, int channelId)
        {
            ProviderResponse res = new ProviderResponse();

            if(string.IsNullOrEmpty(userId))
            {
                res.Success = false;
                return res;
            }

            try
            {

                SqlQuery sqlQuery = new SqlQuery();
                sqlQuery.AddParam("@UserId", userId);

                string query = @"INSERT INTO UserLocations (UserId, SublocationId, ConvertTo, ConvertFrom) 
                                VALUES ";

                for (int i = 0; i < sublocations.Count(); i++)
                {
                    query += "(@UserId, @SublocationId" + i + ", @ConvertTo" + i + ", @ConvertFrom" + i + ")";
                    sqlQuery.AddParam("@SublocationId" + i, sublocations[i].SublocationId);
                    sqlQuery.AddParam("@ConvertFrom" + i, Convert.ToInt32(sublocations[i].UserConvertibleFrom));
                    sqlQuery.AddParam("@ConvertTo" + i, Convert.ToInt32(sublocations[i].UserConvertibleTo));

                    if (i < sublocations.Count() - 1)
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

                    res.Data = GetSublocations(new SearchCriteria(), channelId);
                }

            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                res.Success = false;
            }

            return res;
        }

        public static ProviderResponse InsertOrUpdateSublocationsFromFinale(List<Sublocation> sublocations, int channelId)
        {
            ProviderResponse res = new ProviderResponse();

            sublocations = sublocations.OrderBy(s => s.ParentLocation?.SublocationId.GetValueOrDefault(Byte.MaxValue)).ToList();

            try
            {

                SqlQuery sqlQuery = new SqlQuery();

                string query = @"INSERT INTO Sublocations (FinaleSublocationId, AccountId, ParentId, Name, SublocationStatus, Url) 
                                VALUES ";

                for (int i = 0; i < sublocations.Count(); i++)
                {
                    query += "(@FinaleSublocationId" + i + ", @AccountId" + i + ", @ParentId" + i + ", @Name" + i + ", @SublocationStatus" + i + ", @Url" + i + ")";
                    sqlQuery.AddParam("@AccountId" + i, UsersProvider.GetCurrentAccountId());
                    sqlQuery.AddParam("@FinaleSublocationId" + i, sublocations[i].FinaleSublocationId);
                    sqlQuery.AddParam("@ParentId" + i, sublocations[i].ParentLocation?.SublocationId);
                    sqlQuery.AddParam("@Name" + i, sublocations[i].Name);
                    sqlQuery.AddParam("@SublocationStatus" + i, sublocations[i].Status.ToString());
                    sqlQuery.AddParam("@Url" + i, sublocations[i].Url);

                    if (i < sublocations.Count() - 1)
                    {
                        query += ", ";
                    }
                    else
                    {
                        query += " ";
                    }
                }

                query += @"ON DUPLICATE KEY UPDATE 
                        Name = VALUES(Name), 
                        SublocationStatus = VALUES(SublocationStatus), 
                        Url = VALUES(Url); ";

                using (Sql sql = new Sql())
                {
                    sqlQuery.ExecuteInsert(sql, query);

                    res.Data = GetSublocations(new SearchCriteria(), channelId); 
                }

            }
            catch(Exception e)
            {
                Logger.Log(e.ToString());
                res.Success = false;
            }

            return res;
        }
    }
}