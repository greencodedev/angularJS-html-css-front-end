using BaseWebApp.Maven.Users;
using BaseWebApp.Maven.Utils;
using BaseWebApp.Models;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BaseWebApp.Controllers
{
    public class UsersController : BaseApiController
    {
        // POST api/Account/Logout
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        [Authorize(Roles = "Admin")]
        [Route("add")]
        public HttpResponseMessage Create([FromBody]User account)
        {
            return ReturnResponse(UsersProvider.CreateUser(account));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("Users")]
        public HttpResponseMessage Users([FromBody] SearchCriteria search)
        {
            return ReturnResponse(UsersProvider.GetUsers(search));
        }

        [Authorize(Roles = "Admin")]
        [Route("UpdateUsers")]
        public HttpResponseMessage UpdateUsers([FromBody]User user)
        {
            return ReturnResponse(UsersProvider.UpdateUser(user));
        }



        [AllowAnonymous]
        [HttpPost]
        [Route("ResetPassword")]
        public HttpResponseMessage ResetPassword([FromBody] ResetPasswordRequestModel resetModel)
        {
            return ReturnResponse(UsersProvider.ResetPassword(resetModel));
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("forgatPassword")]
        public HttpResponseMessage ForgatPassword([FromBody] User user)
        {
            return ReturnResponse(UsersProvider.ForgatPassword(user.Email));
        }

        [HttpGet]
        [Route("CurrentUser")]
        public HttpResponseMessage GetCurrentUser()
        {
            return ReturnResponse(UsersProvider.GetCurrentUserResponse());
        }
    }
}
