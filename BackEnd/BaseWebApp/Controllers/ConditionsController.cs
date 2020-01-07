using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BaseWebApp.Maven.Products.Conditions;
using BaseWebApp.Maven.Utils;

namespace BaseWebApp.Controllers
{
    [RoutePrefix("api/conditions")]
    public class ConditionsController : BaseApiController
    {
        [HttpPost]
        public HttpResponseMessage GetConditions([FromBody] SearchCriteria search)
        {
            return ReturnResponse(ConditionProvider.GetConditionList(search));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("update")]
        public HttpResponseMessage UpdateConditions([FromBody] List<Condition> conditions)
        {
            return ReturnResponse(ConditionProvider.UpdateConditions(conditions));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("user/{userId}")]
        public HttpResponseMessage UpdateUserConditions(string userId, [FromBody] List<Condition> conditions)
        {
            return ReturnResponse(ConditionProvider.UpdateUserConditions(userId, conditions));
        }
    }
}
