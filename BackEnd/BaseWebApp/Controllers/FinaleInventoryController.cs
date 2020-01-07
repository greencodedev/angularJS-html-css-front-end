using BaseWebApp.Maven.FinaleInventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BaseWebApp.Controllers
{
    
    [RoutePrefix("api/FinaleInventory")]
    public class FinaleInventoryController : BaseApiController
    {
        /*
        [HttpGet]
        [Route("products")]
        public HttpResponseMessage SyncProducts()
        {
            return ReturnResponse(FinaleClientProvider.SyncProducts());
        }

        [HttpGet]
        [Route("category")]
        public HttpResponseMessage SyncCategory()
        {
            return ReturnResponse(FinaleClientProvider.SyncCategory());
        } */
    }
}
