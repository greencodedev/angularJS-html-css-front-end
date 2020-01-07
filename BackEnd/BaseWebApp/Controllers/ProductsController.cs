using BaseWebApp.Maven.FinaleInventory;
using BaseWebApp.Maven.Products;
using BaseWebApp.Maven.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BaseWebApp.Controllers
{
    [RoutePrefix("api/products")]
    public class ProductsController : BaseApiController
    {
        [HttpPost]
        [Route("{channelid}")]
        public HttpResponseMessage SendProducts([FromBody] SearchCriteria search, int channelid)
        {
            return ReturnResponse(ProductsProvider.SendProducts(search, channelid));
        }
        
        [HttpGet]
        [Route("groups")]
        public HttpResponseMessage GetProductGroups()
        {
            return ReturnResponse(ProductsProvider.GetProductGroups());
        }

        
        
       /* [HttpGet]
        [Route("conditions/sync")]
        public HttpResponseMessage SyncCostom()
        {
            return ReturnResponse(Sync.SyncCostomField());
        }*/
    }
}
