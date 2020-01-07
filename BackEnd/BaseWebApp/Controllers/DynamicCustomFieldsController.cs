using BaseWebApp.Maven.Custom_Fields;
using BaseWebApp.Maven.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BaseWebApp.Controllers
{
    [RoutePrefix("api/CustomFields")]
    public class DynamicCustomFieldsController : BaseApiController
    {
        [HttpGet]
        [Route("")]
        public HttpResponseMessage getFields()
        {
           return ReturnResponse(DynamicCustomFieldsProvider.GetCustomFields(UsersProvider.GetCurrentAccountId(), null));
        }

        [HttpGet, Route("{id}")]
        public HttpResponseMessage getField(int id)
        {
            return ReturnResponse(DynamicCustomFieldsProvider.GetCustomFields(UsersProvider.GetCurrentAccountId(), id));
        }

        [HttpPost]
        [Route("")]
        public HttpResponseMessage insertFields(List<DynamicCustomField> fields)
        {
            return ReturnResponse(DynamicCustomFieldsProvider.InsertDynamicCustomFields(fields, UsersProvider.GetCurrentAccountId()));
        }

        [HttpPut]
        [Route("")]
        public HttpResponseMessage updateField(DynamicCustomField field)
        {
            return ReturnResponse(DynamicCustomFieldsProvider.UpdateCustomFields(field, UsersProvider.GetCurrentAccountId()));
        }

        //[HttpGet, Route("productcustomfields")]  For testing purposes only!!
        //public HttpResponseMessage getProductCustomFields(List<ProductCustomField> fields)
        //{
        //    return ReturnResponse(ProductCustomFieldsProvider.InsertOrUpdateProductCustomFields(UsersProvider.GetCurrentAccountId(), fields, true));
        //}

    }
}
