using BaseWebApp.Maven.Utils.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BaseWebApp.Controllers
{
    [WebApiFilter]
    [Authorize]
    public class BaseApiController : ApiController
    {
        [NonAction]
        public HttpResponseMessage ReturnResponse(ProviderResponse providerResponse)
        {
            if (providerResponse.Success)
            {
                return Request.CreateResponse(HttpStatusCode.OK, providerResponse);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, providerResponse);
            }
        }
    }
}
