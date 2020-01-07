using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BaseWebApp.Maven.Accounts;
using BaseWebApp.Maven.PrintNode.Printers;
using BaseWebApp.Maven.Utils.Api;
using BaseWebApp.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BaseWebApp.Maven.Users
{
    public class User
    {
        public int AccountId { get; set; } 
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Initials { get; set; }
        public bool Active { get; set; }
        public string UserId { set; get; }
        public bool IsAdmin { get; set; }
        public bool IsSuperAdmin { get; set; }
        public IList<string> Roles { get; set; }
        public List<Printer> Printers { get; set; }

        public User() { }

        public User(ApplicationUser user)
        {
            Email = user.Email;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Active = user.Active;
            UserId = user.Id;
            Initials = user.Initials;
            IsSuperAdmin = user.SuperAdmin.HasValue && user.SuperAdmin.Value;
            AccountId = user.AccountId;

            if (!ThreadProperties.IsProcess())
            {
                IsAdmin = ThreadProperties.GetUserManager().IsInRole(user.Id, UsersProvider.ADMIN_USER_TYPE);
                Roles = ThreadProperties.GetUserManager().GetRoles(user.Id);
            }
            else
            {
                IsAdmin = true;
            }
        }
    }
}