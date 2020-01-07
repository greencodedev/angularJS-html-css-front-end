using BaseWebApp.Maven.Printing.Printers;
using BaseWebApp.Maven.Printing.PrintJobs;
using BaseWebApp.Maven.PrintNode;
using BaseWebApp.Maven.PrintNode.Printers;
using BaseWebApp.Maven.PrintNode.PrintJobs;
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
    [RoutePrefix("api/print")]
    public class PrintingController : BaseApiController
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("printers/sync")]
        public HttpResponseMessage SyncPrinters()
        {
            return ReturnResponse(PrinterProvider.SyncPrinters());
        }

        //[HttpGet]
        //[Route("printers")]
        //public HttpResponseMessage GetPrinters()
        //{
        //    SearchCriteria search = new SearchCriteria();
        //    return ReturnResponse(PrinterProvider.GetPrinters(search));
        //}

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("printers")]
        public HttpResponseMessage UpdatePrinters([FromBody] Printer printer)
        {
            return ReturnResponse(PrinterProvider.UpdatePrinter(printer));
        }

        [HttpPost]
        [Route("GetPrinters")]
        public HttpResponseMessage GetAllPrinters([FromBody] SearchCriteria search)
        {
            return ReturnResponse(PrinterProvider.GetAllPrinters(search));
        }

        [HttpGet]
        [Route("printersizes")]
        public HttpResponseMessage GetPrinterSizes()
        {
            SearchCriteria search = new SearchCriteria();
            return ReturnResponse(PrinterSizeProvider.GetPrinterSizes(search));
        }

        [HttpPost]
        [Route("printersizes")]
        public HttpResponseMessage UpdatePrinterSize([FromBody] PrinterSize printerSize)
        {
            return ReturnResponse(PrinterSizeProvider.InsertOrUpdate(printerSize));
        }

        /*[HttpGet]
        [Route("printjobs")]
        public HttpResponseMessage GetPrintJobs()
        {
            return ReturnResponse(PrintNodeProvider.GetPrintJobs());
        }*/

        [HttpPost]
        [Route("printjobs")]
        public HttpResponseMessage CreatePrintJob([FromBody] PrintJob printJob)
        {
            return ReturnResponse(PrintJobProvider.CreatePrintJob(printJob));
        }

        [Authorize(Roles = "Printer")]
        [HttpGet]
        [Route("printjobs/{id}")]
        public HttpResponseMessage GetPrintJobStatus(int id)
        {
            return ReturnResponse(PrintJobProvider.GetPrintJob(id));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("printjoblogs")]
        public HttpResponseMessage GetPrintLogs([FromBody] SearchCriteria search)
        {
            return ReturnResponse(PrintJobProvider.GetPrintJobLogs(search));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("printjoblogs/export")]
        public HttpResponseMessage ExportPrintLogs([FromUri] SearchCriteria search)
        {
            return ExcelUtil.DownloadCsvFile(ExcelUtil.GetPrintJobCsvData(search), "EMB-PrintLog-Export.csv");
        }
    }
}
