using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BaseWebApp.Maven.Products;
using BaseWebApp.Maven.Utils;

namespace BaseWebApp.Controllers
{
    [RoutePrefix("api/category")]
    public class CategoryController : BaseApiController
    {
        [HttpGet]
        [Route("")]
        public HttpResponseMessage SerachCategory([FromUri] SearchCriteria search)
        {
            return ReturnResponse(CategoryProvider.SendCat(search));
        }
        
        /*[HttpGet]
        [Route("sync")]
        public HttpResponseMessage SyncCategory()
        {
            return ReturnResponse(Sync.SyncCat());
        }*/
    }
}
