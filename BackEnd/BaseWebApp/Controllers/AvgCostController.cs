using BaseWebApp.Maven.Products.AverageCost;
using BaseWebApp.Maven.Utils;
using BaseWebApp.Maven.Utils.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace BaseWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    [RoutePrefix("api/avgcost")]
    public class AvgCostController : BaseApiController
    {
        
        [HttpPost]
        [Route("logs")]
        public HttpResponseMessage GetLogs([FromBody] SearchCriteria search)
        {
            return ReturnResponse(AverageCostLogProvider.GetAvgCostLogs(search));
        }

        [HttpGet]
        [Route("logs/export")]
        public HttpResponseMessage ExportLogs([FromUri] SearchCriteria search)
        {
            return ExcelUtil.DownloadCsvFile(ExcelUtil.GetAvgCostCsvData(search), "EMB-AvgCostLog-Export.csv");
        }
    }
}