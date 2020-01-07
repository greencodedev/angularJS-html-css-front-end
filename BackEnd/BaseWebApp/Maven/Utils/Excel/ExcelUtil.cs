using BaseWebApp.Maven.Conversions;
using BaseWebApp.Maven.Printing.PrintJobs;
using BaseWebApp.Maven.PrintNode.PrintJobs;
using BaseWebApp.Maven.Products;
using BaseWebApp.Maven.Products.AverageCost;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;

namespace BaseWebApp.Maven.Utils.Excel
{
    public class ExcelUtil
    {
        public static HttpResponseMessage DownloadExcelFile(IWorkbook workbook, string fileName)
        {
            MemoryStream stream = new MemoryStream();
            workbook.Write(stream);
            stream.Position = 0;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream)
            };
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName + ".xls"
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.ms-excel");

            return response;
        }

        
        public static string GetConversionCsvData(SearchCriteria search)
        {
            var csv = new StringBuilder();

            if (search == null)
            {
                search = new SearchCriteria();
            }

            List<Conversion> conversions = ConversionProvider.searchConversionLogs(search);

            var header = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16}",
                    "User",
                    "Source Product ID",
                    "Source Product Name",
                    "Source Condition Code",
                    "Source Condition Name",
                    "Category",
                    "Source Location",
                    "Destination Product ID",
                    "Destination Product Name",
                    "Destination Condition Code",
                    "Destination Condition Name",
                    "Destination Location",
                    "PO Number",
                    "Note",
                    "Qantity",
                    "Time",
                    "New Item",
                    "Convert Type"
                );
            csv.AppendLine(header);

            for (int i = 0; i < conversions.Count; i++)
            {
                Conversion conversion = conversions[i];

                var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16}",
                    conversion.user.FirstName + " " + conversion.user.LastName,
                    conversion.FromProduct.FinaleId,
                    conversion.FromProduct.Name,
                    conversion.FromProduct.Condition?.Code,
                    conversion.FromProduct.Condition?.Name,
                    conversion.FromProduct.Category?.Label,
                    conversion.FromSublocation.Name,
                    conversion.ToProduct.FinaleId,
                    conversion.ToProduct.Name,
                    conversion.ToProduct.Condition?.Code,
                    conversion.ToProduct.Condition?.Name,
                    conversion.ToSublocation.Name,
                    conversion.PoNumber,
                    conversion.Note.Replace("\n", " "),
                    conversion.Quantity,
                    conversion.DateTime.ToString(),
                    conversion.CreateNewProduct.ToString(),
                    conversion.ConvertType
                );

                csv.AppendLine(newLine);
            }

            return csv.ToString();
        }

        public static string GetPrintJobCsvData(SearchCriteria search)
        {
            var csv = new StringBuilder();

            if(search == null)
            {
                search = new SearchCriteria();
            }

            List<PrintJob> printJobs = PrintJobProvider.searchPrintJobLogs(search);

            var header = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                    "User",
                    "Printer ID",
                    "Printer Name",
                    "Product ID",
                    "Product Name",
                    "Condition Code",
                    "Condition Name",
                    "Qty",
                    "Time",
                    "Status"
                );
            csv.AppendLine(header);

            for (int i = 0; i < printJobs.Count; i++)
            {
                PrintJob printJob = printJobs[i];

                var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                    printJob.user.FirstName + " " + printJob.user.LastName,
                    printJob.Printer.Id,
                    printJob.Printer.CustomName,
                    printJob.Product.FinaleId,
                    printJob.Product.Name,
                    printJob.Product.Condition?.Code,
                    printJob.Product.Condition?.Name,
                    printJob.Quantity,
                    printJob.DateTime.ToString(),
                    printJob.Status.ToString()
                );

                csv.AppendLine(newLine);
            }

            return csv.ToString();
        }

        public static string GetAvgCostCsvData(SearchCriteria search)
        {
            var csv = new StringBuilder();

            if (search == null)
            {
                search = new SearchCriteria();
            }

            search.Paginate = false;
            List<AverageCostLog> logs = AverageCostLogProvider.SearchLogs(search);

            var header = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                    "Product ID",
                    "Product Name",
                    "Transaction Type",
                    "Transaction Description",
                    "Current Average Cost",
                    "Current Quantity On Hend",
                    "Transaction Quantity",
                    "Transaction Cost Per Unit", 
                    "New Quantity On Hend",
                    "New Average Cost",
                    "Time"
                );
            csv.AppendLine(header);

            for (int i = 0; i < logs.Count; i++)
            {
                AverageCostLog log = logs[i];

                var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                    log.FinalProductId,
                    log.ProductName,
                    log.TransactionType,
                    log.TransactionDescription.Replace(",", " -"),
                    log.CurrAvgCost,
                    log.CurrentStock - log.Quantity,
                    log.Quantity,
                    log.CostPerUnit,
                    log.CurrentStock,
                    log.NewAverageCost,
                    log.ReceivedDate.ToString()
                );

                csv.AppendLine(newLine);
            }

            return csv.ToString();
        }

        public static HttpResponseMessage DownloadCsvFile(string data, string fileName)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(data)
            };
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName + ".csv"
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

            return response;
        }

        public static IWorkbook ExportPrintJobLogs()
        {
            IWorkbook workbook = new HSSFWorkbook();
            ICellStyle headerStyle = NpoiUtils.GetHeaderStyle(workbook);

            ISheet sheet1 = workbook.CreateSheet("List");
            IRow header1 = sheet1.CreateRow(0);
            NpoiUtils.CreateCell(header1, 0, "User", headerStyle);
            NpoiUtils.CreateCell(header1, 1, "Printer Name", headerStyle);
            NpoiUtils.CreateCell(header1, 2, "Product Name", headerStyle);
            NpoiUtils.CreateCell(header1, 3, "Qty", headerStyle);
            NpoiUtils.CreateCell(header1, 4, "Status", headerStyle);
            NpoiUtils.CreateCell(header1, 5, "Time", headerStyle);

            sheet1.SetColumnWidth(0, 25 * 256);
            sheet1.SetColumnWidth(1, 25 * 256);
            sheet1.SetColumnWidth(2, 35 * 256);
            sheet1.SetColumnWidth(3, 6 * 256);
            sheet1.SetColumnWidth(4, 25 * 256);
            sheet1.SetColumnWidth(5, 25 * 256);

            List<PrintJob> printJobs = PrintJobProvider.searchPrintJobLogs(new SearchCriteria());

            for (int i = 0; i < printJobs.Count; i++)
            {
                IRow pRow = sheet1.CreateRow(i + 1);
                PrintJob printJob = printJobs[i];

                NpoiUtils.CreateCell(pRow, 0, printJob.user.FirstName + " " + printJob.user.LastName);
                NpoiUtils.CreateCell(pRow, 1, printJob.Printer.CustomName);
                NpoiUtils.CreateCell(pRow, 2, printJob.Product.Name);
                NpoiUtils.CreateCell(pRow, 3, printJob.Quantity);
                NpoiUtils.CreateCell(pRow, 4, printJob.Status.ToString());
                NpoiUtils.CreateCell(pRow, 5, printJob.DateTime.ToString());
            }

            return workbook;
        }

        public static IWorkbook ExportConversionLogs()
        {
            IWorkbook workbook = new HSSFWorkbook();
            ICellStyle headerStyle = NpoiUtils.GetHeaderStyle(workbook);

            ISheet sheet1 = workbook.CreateSheet("List");
            IRow header1 = sheet1.CreateRow(0);
            NpoiUtils.CreateCell(header1, 0, "User", headerStyle);
            NpoiUtils.CreateCell(header1, 1, "Source Product", headerStyle);
            NpoiUtils.CreateCell(header1, 2, "Destination Product", headerStyle);
            NpoiUtils.CreateCell(header1, 3, "Source Location", headerStyle);
            NpoiUtils.CreateCell(header1, 4, "Destination Location", headerStyle);
            NpoiUtils.CreateCell(header1, 5, "Qty", headerStyle);
            NpoiUtils.CreateCell(header1, 6, "PO Number", headerStyle);
            NpoiUtils.CreateCell(header1, 7, "Date", headerStyle);

            sheet1.SetColumnWidth(0, 25 * 256);
            sheet1.SetColumnWidth(1, 25 * 256);
            sheet1.SetColumnWidth(2, 25 * 256);
            sheet1.SetColumnWidth(3, 25 * 256);
            sheet1.SetColumnWidth(4, 25 * 256);
            sheet1.SetColumnWidth(5, 6 * 256);
            sheet1.SetColumnWidth(6, 25 * 256);
            sheet1.SetColumnWidth(7, 25 * 256);

            List<Conversion> conversions = ConversionProvider.searchConversionLogs(new SearchCriteria());

            for (int i = 0; i < conversions.Count; i++)
            {
                IRow pRow = sheet1.CreateRow(i + 1);
                Conversion conversion = conversions[i];

                NpoiUtils.CreateCell(pRow, 0, conversion.user.FirstName + " " + conversion.user.LastName);
                NpoiUtils.CreateCell(pRow, 1, conversion.FromProduct.Name);
                NpoiUtils.CreateCell(pRow, 2, conversion.ToProduct.Name);
                NpoiUtils.CreateCell(pRow, 3, conversion.FromSublocation.Name);
                NpoiUtils.CreateCell(pRow, 4, conversion.ToSublocation.Name);
                NpoiUtils.CreateCell(pRow, 5, conversion.Quantity);
                NpoiUtils.CreateCell(pRow, 6, conversion.PoNumber);
                NpoiUtils.CreateCell(pRow, 7, conversion.DateTime.ToString());
            }

            return workbook;
        }

        public static Attachment GetMissingNCSCsvData(List<Product> products)
        {
            var csv = new StringBuilder();

            var header = string.Format("{0},{1},{2}",
                    "ID",
                    "Name",
                    "Condition"
                );
            csv.AppendLine(header);

            foreach (Product product in products)
            {
                var newLine = string.Format("{0},{1},{2}",
                    product.FinaleId,
                    product.Name,
                    product.Condition?.Code
                );

                csv.AppendLine(newLine);
            }

            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(csv.ToString()));

            Attachment attachment = new Attachment(stream, new ContentType("text/csv"))
            {
                Name = "missing_ncs.csv"
            };

            return attachment;
        }

        public static Attachment GetduplicateNCSandConditionCsvData(List<Product> products)
        {
            var csv = new StringBuilder();

            var header = string.Format("{0},{1},{2},{3}",
                    "ID",
                    "Name",
                    "Ncs",
                    "Condition"
                );
            csv.AppendLine(header);

            foreach (Product product in products)
            {
                var newLine = string.Format("{0},{1},{2},{3}",
                    product.FinaleId,
                    product.Name,
                    product.Ncs,
                    product.Condition?.Code
                );

                csv.AppendLine(newLine);
            }

            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(csv.ToString()));

            Attachment attachment = new Attachment(stream, new ContentType("text/csv"))
            {
                Name = "duplicate_ncs.csv"
            };

            return attachment;
        }
    }
}