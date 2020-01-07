using BaseWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;

namespace BaseWebApp.Maven.Utils.Api
{
    public class ThreadProperties
    {
        private static AsyncLocal<ApplicationUser> currentUser = new AsyncLocal<ApplicationUser>();
        private static AsyncLocal<IPrincipal> principal = new AsyncLocal<IPrincipal>();
        private static AsyncLocal<ApplicationUserManager> userManager = new AsyncLocal<ApplicationUserManager>();
        private static AsyncLocal<bool> _isProcess = new AsyncLocal<bool>();

        public static void SetUser(ApplicationUser newUser)
        {
            currentUser.Value = newUser;
        }

        public static ApplicationUser GetUser()
        {
            return currentUser.Value;
        }

        public static void SetUserManager(ApplicationUserManager manager)
        {
            userManager.Value = manager;
        }

        public static ApplicationUserManager GetUserManager()
        {
            return userManager.Value;
        }

        public static void SetPrincipal(IPrincipal newPrincipal)
        {
            principal.Value = newPrincipal;
        }

        public static IPrincipal GetPrincipal()
        {
            return principal.Value;
        }

        public static void SetIsProcess(bool isProcess)
        {
            _isProcess.Value = isProcess;
        }

        public static bool IsProcess()
        {
            return _isProcess.Value;
        }
    }
}