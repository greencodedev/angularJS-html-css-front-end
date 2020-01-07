using BaseWebApp.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace BaseWebApp.Maven.Utils.Api
{
    public class WebApiFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            ApplicationUserManager UserManager = actionContext.Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ApplicationUser user = UserManager.FindById(actionContext.RequestContext.Principal.Identity.GetUserId());
            ThreadProperties.SetUser(user);
            ThreadProperties.SetUserManager(UserManager);
            ThreadProperties.SetPrincipal(actionContext.RequestContext.Principal);           
        }
    }
}