using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BaseWebApp.Maven.Sublocations;
using BaseWebApp.Maven.Utils;

namespace BaseWebApp.Controllers
{
    [RoutePrefix("api/sublocations")]
    public class SublocationController : BaseApiController
    {
        [HttpPost]
        [Route("{channelId}")]
        public HttpResponseMessage GetSublocations([FromBody] SearchCriteria search, int channelId)
        {
            return ReturnResponse(SublocationProvider.GetSublocationList(search, channelId));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("update/{channelId}")]
        public HttpResponseMessage InsertOrUpdateSublocations([FromBody] List<Sublocation> sublocations, int channelId)
        {
            return ReturnResponse(SublocationProvider.InsertOrUpdateSublocations(sublocations, channelId));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("user/{userId}/{channelId}")]
        public HttpResponseMessage UpdateUserLocations(string userId, [FromBody] List<Sublocation> sublocations, int channelId)
        {
            return ReturnResponse(SublocationProvider.UpdateUserLocations(userId, sublocations, channelId));
        }
    }
}
