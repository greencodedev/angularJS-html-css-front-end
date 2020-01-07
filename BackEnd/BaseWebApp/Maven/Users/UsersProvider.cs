
using BaseWebApp.Maven.Printing.Printers;
using BaseWebApp.Maven.PrintNode.Printers;
using BaseWebApp.Maven.Utils;
using BaseWebApp.Maven.Utils.Api;
using BaseWebApp.Maven.Utils.Email;
using BaseWebApp.Maven.Utils.Sql;
using BaseWebApp.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;

namespace BaseWebApp.Maven.Users
{
    public class UsersProvider
    {
        public static readonly string ADMIN_USER_TYPE = "Admin";
        public static readonly string USER_USER_TYPE = "User";
        public static readonly string PRINTER_USER_TYPE = "Printer";
        public static readonly string CONVERTER_USER_TYPE = "Converter";
        public static readonly string CONVERT_PLUS_USER_TYPE = "ConvertPlus";

        public static User GetCurrentUser()
        {
            ApplicationUser user = ThreadProperties.GetUser();

            if(user != null)
            {
                return new User(user);
            }
            else
            {
                return new User {
                    Active = true,
                    UserId = "System",
                    Initials = "SYS",
                };
            }
        }

        public static int GetCurrentAccountId()
        {
            return GetCurrentUser().AccountId;
        }

        public static ProviderResponse GetCurrentUserResponse()
        {
            ProviderResponse res = new ProviderResponse();

            try
            {
                res.Data = GetCurrentUser();
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                res.Messages.Add("An Error occurred, see log for details");
            }
            
            return res;
        }

        public static ProviderResponse CreateUser(User user)
        {
            ProviderResponse res = new ProviderResponse();


            // Validate
            res.Messages.AddRange(Validate(null, user, false));

            if (res.Messages.Any())
            {
                res.Success = false;
                return res;
            }

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                IdentityResult result;

                try
                {
                    // fill user object
                    var appUser = new ApplicationUser()
                    {
                        UserName = user.Email,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Active = user.Active,
                        Initials = user.Initials
                    };

                    //Create User
                    result = ThreadProperties.GetUserManager().Create(appUser);

                    if (!result.Succeeded)
                    {
                        scope.Dispose();
                        res.Success = false;

                        if (result.Errors != null)
                        {
                            foreach (string error in result.Errors)
                            {
                                res.Messages.Add(error);
                            }
                        }
                        else
                        {
                            res.Messages.Add("An Error occurred, see log for details");
                        }
                    }
                    else
                    {
                        // add role to user
                        IdentityResult RolesResult = ThreadProperties.GetUserManager().AddToRole(appUser.Id, USER_USER_TYPE);

                        if (!RolesResult.Succeeded)
                        {
                            scope.Dispose();
                            res.Success = false;

                            if (RolesResult.Errors != null)
                            {
                                foreach (string error in RolesResult.Errors)
                                {
                                    res.Messages.Add(error);
                                }
                            }
                            else
                            {
                                res.Messages.Add("An Error occurred, see log for details");
                            }
                        }
                        else
                        {
                            if (user.Roles != null)
                            {
                                SetUserRoles(appUser.Id, user.Roles.ToArray(), false);
                            }

                            if (user.Printers != null)
                            {
                                SetUserPrinters(appUser.Id, new List<Printer>(), user.Printers.FindAll(p => p.IsAssigned), false);
                            }

                            scope.Complete();

                            var code = ThreadProperties.GetUserManager().GeneratePasswordResetToken(appUser.Id);
                            string callbackUrl = Config.GetConfig("CLIENT_BASE_URL") + "reset-password?Token=" + Uri.EscapeDataString(code) + "&Email=" + Uri.EscapeDataString(appUser.Email);

                            EmailClient.SendEmail(appUser.Email, "EMB SIGN UP",
                                        "Please set up your new EMB account password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");
                        }

                        res.Data = appUser;

                    }

                }
                catch(Exception e)
                {
                    Logger.Log(e.ToString());
                    scope.Dispose();
                    res.Messages.Add("An Error occurred, see log for details");

                }

            }

            return res;
        }

        public static ProviderResponse GetUsers(SearchCriteria search)
        {
            ProviderResponse res = new ProviderResponse();

            if(search == null)
            {
                search = new SearchCriteria();
            }
            try
            {

            List<User> list = new List<User>();
            using (var context = new ApplicationDbContext())
            {
                var customers = context.Users.ToList();

                if(search.Status.HasValue && search.Status.Value == true)
                {
                    customers = customers.Where(x => x.Active).ToList();
                }

                foreach (ApplicationUser user in customers)
                {
                    list.Add(new User(user));
                }
            }
            res.Data = list;
            }
            catch (Exception e)
            {
            }
            

            //if (search.Paginate)
            //{

            //    query += "limit @startIndex, @count ";
            //    sqlQuery.AddParam("@startIndex", (search.Page - 1) * search.Limit);
            //    sqlQuery.AddParam("@count", search.Limit);
            //}


            return res;

            
        }

        public static ProviderResponse UpdateUser(User user)
        {
            ProviderResponse res = new ProviderResponse();

            // Validate
            res.Messages.AddRange(Validate(null, user, true));

            if (res.Messages.Any())
            {
                res.Success = false;
                return res;
            }
            ApplicationUser old = ThreadProperties.GetUserManager().FindById(user.UserId);
            if (old == null)
            {
                res.Messages.Add("Error Finding this user");
                return res;

            }


            try
            {

                // compare for changes
                if (old.Email != user.Email || old.FirstName != user.FirstName || old.LastName != user.LastName || old.Active != user.Active || old.Initials != user.Initials)
                {
                    // fill user object
                    old.UserName = user.Email;
                    old.Email = user.Email;
                    old.FirstName = user.FirstName;
                    old.LastName = user.LastName;
                    old.Active = user.Active;
                    old.Initials = user.Initials;


                    IdentityResult identityResult = ThreadProperties.GetUserManager().Update(old);

                    if(!identityResult.Succeeded)
                    {

                        if (identityResult.Errors != null && identityResult.Errors.Any())
                        {
                            foreach (string error in identityResult.Errors)
                            {
                                res.Messages.Add(error);
                                
                            }
                        }
                        else
                        {
                            res.Messages.Add("An Error occurred when updating, see log for details");
                        }

                    }
                
                }

                if(user.Roles != null)
                {
                    SetUserRoles(user.UserId, user.Roles.ToArray(), old.SuperAdmin.HasValue && old.SuperAdmin.Value);
                } 

                if(user.Printers != null)
                {
                    List<Printer> currPrinters = PrinterProvider.GetPrinters(new SearchCriteria { UserId = user.UserId }).FindAll(p => p.IsAssigned);
                    SetUserPrinters(user.UserId, currPrinters, user.Printers.FindAll(p => p.IsAssigned), true);
                }

            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                res.Messages.Add("An Error occurred, see log for details");

            }


            return res;

        }

        private static void SetUserPrinters(string userId, List<Printer> currPrinters, List<Printer> printers, bool createTransaction)
        {
            List<object> toRemove = new List<object>();
            List<object> toAdd = new List<object>();

            //loop curr printers, and check what to remove
            foreach (Printer currPrinter in currPrinters)
            {
                if(printers.Find(x => x.PrinterId == currPrinter.PrinterId) == null)
                {
                    toRemove.Add(currPrinter.PrinterId.ToString());
                }
            }

            //loop new printers, and check what to add
            foreach (Printer newPrinter in printers)
            {
                if (currPrinters.Find(x => x.PrinterId == newPrinter.PrinterId) == null)
                {
                    toAdd.Add(newPrinter.PrinterId.ToString());
                }
            }

            if(toAdd.Any() || toRemove.Any())
            {
                using (Sql sql = new Sql())
                {
                    try
                    {
                        if(createTransaction)
                        {
                            sql.BeginTransaction("updateUserPrinters");
                        }

                        if(toRemove.Any())
                        {
                            SqlQuery removeSqlQuery = new SqlQuery();
                            removeSqlQuery.AddParam("@UserId", userId);

                            string removeQuery = @"DELETE FROM UserPrinters
                                        WHERE UserId = @UserId 
                                        AND PrinterId IN (" + removeSqlQuery.listToInString(toRemove) + ")";

                            removeSqlQuery.ExecuteNonQuery(sql, removeQuery);
                        }

                        if(toAdd.Any())
                        {
                            SqlQuery addSqlQuery = new SqlQuery();
                            addSqlQuery.AddParam("@UserId", userId);

                            string addQuery = @"INSERT INTO UserPrinters(UserId, PrinterId)
                                    VALUES ";

                            //add values
                            for (int i = 0; i < toAdd.Count(); i++)
                            {
                                addQuery += "(@UserId, @PrinterId" + i + ") ";
                                addSqlQuery.AddParam("@PrinterId" + i, toAdd[i]);

                                if (i < toAdd.Count() - 1)
                                {
                                    addQuery += ", ";
                                }
                            }

                            addSqlQuery.ExecuteInsert(sql, addQuery);
                        }

                        if (createTransaction)
                        {
                            sql.Commit();
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log(e.ToString());

                        if(createTransaction)
                        {
                            sql.Rollback();
                        }
                        throw e;
                    }
                }
            }
        }

        public static ProviderResponse ForgatPassword(string emailAddress)
        {
            ProviderResponse res = new ProviderResponse();

            if (string.IsNullOrEmpty(emailAddress) || !UtilsLib.IsValidEmail(emailAddress))
            {
                res.Messages.Add("Invalid Email");
                return res;
            }

            try
            {
                ApplicationUser user = ThreadProperties.GetUserManager().FindByEmail(emailAddress);

                if (user == null)
                {
                    Logger.Log("RESET :: Password reset token requested for non existing email");
                    return res;
                }

                var code = ThreadProperties.GetUserManager().GeneratePasswordResetToken(user.Id);
                string callbackUrl = Config.GetConfig("CLIENT_BASE_URL") + "reset-password?Token=" + Uri.EscapeDataString(code) + "&Email=" + Uri.EscapeDataString(emailAddress);

                EmailClient.SendEmail(emailAddress, "Reset Password",
                            "Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");

                return res;
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                res.Messages.Add("An Error occurred, see log for details");
            }

            return res;
        }

        public static ProviderResponse ResetPassword(ResetPasswordRequestModel resetModel)
        {
            ProviderResponse res = new ProviderResponse();

            if (string.IsNullOrEmpty(resetModel.Email) || !UtilsLib.IsValidEmail(resetModel.Email))
            {
                res.Messages.Add("Invalid Email");
                return res;
            }
            if (string.IsNullOrEmpty(resetModel.NewPassword))
            {
                res.Messages.Add("Invalid Password");
                return res;
            }
            if (resetModel.NewPassword != resetModel.ConfirmPassword)
            {
                res.Messages.Add("The new password and confirmation password do not match.");
                return res;
            }

            try
            {
                var user = ThreadProperties.GetUserManager().FindByEmail(resetModel.Email);
                if (user == null)
                {
                    Logger.Log("RESET :: Reset password request for non existing email");
                    res.Messages.Add("Error");
                    return res;
                }

                if (!ThreadProperties.GetUserManager().UserTokenProvider.ValidateAsync("ResetPassword", resetModel.Token, ThreadProperties.GetUserManager(), user).Result)
                {
                    Logger.Log("RESET :: Reset password requested with wrong token");
                    res.Messages.Add("Error");
                    return res;
                }


                var result = ThreadProperties.GetUserManager().ResetPassword(user.Id, resetModel.Token, resetModel.NewPassword);

                if (!result.Succeeded)
                {
                    if (result.Errors != null && result.Errors.Count() > 0)
                    {
                        foreach (string error in result.Errors)
                        {
                            res.Messages.Add(error);
                        }
                    }
                    else
                    {
                        res.Messages.Add("Error");
                    }
                    return res;
                }

                const string subject = "EMB Phones - Password reset success.";
                var body = "<html><body>" +
                           "<h1>Your password for EMB Phones was reset</h1>" +
                           $"<p>Hi {user.Email}!</p>" +
                           "<p>Your password for Client was reset. Please inform us if you did not request this change.</p>" +
                           "</body></html>";

                EmailClient.SendEmail(resetModel.Email, subject, body);
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                res.Messages.Add("An Error occurred, see log for details");
            }

            return res;
        }

        public static ProviderResponse SetUserRoles(string userId, string[] roleIds, bool isSuperAdmin)
        {
            ProviderResponse response = new ProviderResponse();

            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

                IList<string> userRoles = UserManager.GetRoles(userId);

                foreach (string role in userRoles)
                {
                    if (!roleIds.Contains(role))
                    {
                        if(role == ADMIN_USER_TYPE && isSuperAdmin)
                        {

                        }
                        else
                        {
                            UserManager.RemoveFromRole(userId, role);
                        }
                    }
                }

                foreach (string role in roleIds)
                {
                    if(!userRoles.Contains(role))
                    {
                        UserManager.AddToRole(userId, role);
                    }
                }

                context.SaveChanges();
            }
            catch(Exception e)
            {
                Logger.Log(e.ToString());
            }

            return response;
        }


        private static List<string> Validate(User curr, User user, bool update)
        {
            List<string> errors = new List<string>();

            //if (update && curr == null)
            //{
            //    errors.Add("Invalid Id");
            //}
            //else
            //{
                if (update)
                {
                    if (string.IsNullOrEmpty(user.UserId))
                    {
                        errors.Add("A User must have a User ID");

                    }
                }
                if (string.IsNullOrEmpty(user.FirstName))
                {
                    errors.Add("A User must have a First Name");
                }if (string.IsNullOrEmpty(user.LastName))
                {
                    errors.Add("A User must have a Last Name");
                }
                if (string.IsNullOrEmpty(user.Email))
                {
                    errors.Add("A User  must have an Email address");
                } else
                if (!UtilsLib.IsValidEmail(user.Email))
                {
                    errors.Add("A User must have a valid Email address");
                }
                if (string.IsNullOrEmpty(user.Initials))
                {
                    errors.Add("A User must have a valid Initial");
                }
                


            //}

            return errors;
        }
       




    }

  

    }