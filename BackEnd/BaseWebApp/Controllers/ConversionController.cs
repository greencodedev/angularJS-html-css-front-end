
using BaseWebApp.Maven.Conversions;
using BaseWebApp.Maven.Utils;
using BaseWebApp.Maven.Utils.Api;
using BaseWebApp.Maven.Utils.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BaseWebApp.Controllers
{
    [RoutePrefix("api/convert")]
    public class ConversionController : BaseApiController
    {
        [Authorize(Roles = "Converter")]
        [HttpPost]
        [Route("{channelId}")]
        public HttpResponseMessage ConvertStock([FromBody] Conversion conversion, int channelId)
        {
            return ReturnResponse(ConversionProvider.CreateConversion(conversion, channelId));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("logs")]
        public HttpResponseMessage ConversionLogs([FromBody] SearchCriteria search)
        {
            return ReturnResponse(ConversionProvider.GetConversionLogs(search));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("logs/export")]
        public HttpResponseMessage ExportConversionLogs([FromUri] SearchCriteria search)
        {
            return ExcelUtil.DownloadCsvFile(ExcelUtil.GetConversionCsvData(search), "EMB-ConversionLog-Export");
        }
    }
}
