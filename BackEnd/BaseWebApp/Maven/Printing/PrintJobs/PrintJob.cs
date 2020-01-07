using Antlr3.ST;
using BaseWebApp.Maven.PrintNode.Printers;
using BaseWebApp.Maven.Products;
using BaseWebApp.Maven.Users;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;

namespace BaseWebApp.Maven.PrintNode.PrintJobs
{
    public class PrintJob
    {
        public int PrintJobId { get; set; }
        public int PrintNodePrintJobId { get; set; }
        public int Quantity { get; set; }
        public Printer Printer { get; set; }
        public Product Product { get; set; }
        public string Title { get; set; }
        // public User User { get; set; }
        public PrintNodePrintJobStatuses state { get; set; }
        public MssPrintJobStatus Status { get; set; }
        public User user { get; set; }
        public DateTime DateTime { get; set; }
        public bool FromConvert { get; set; }

        public Dictionary<string, string> PrepareForPrintNode()
        {
            Dictionary<string, string> response = new Dictionary<string, string>();

            response.Add("printerId", Printer.Id);
            response.Add("title", Product.Name);
            
            StringTemplate temp = new StringTemplate(Printer.Size.Zpl);
            temp.SetAttribute("product_id", Product.FinaleId);
            temp.SetAttribute("initials", Users.UsersProvider.GetCurrentUser().Initials);
            temp.SetAttribute("qty", Quantity.ToString());

            DateTime localTime = Utils.UtilsLib.UtcToLocalTime(DateTime.UtcNow);

            if (Printer.Size.Name == "Large")
            {
                temp.SetAttribute("description", Product.Name);
                temp.SetAttribute("formatted_date", localTime.ToString("yyyy-MM-dd hh:mm"));
            }
            else if (Printer.Size.Name == "Small" || Printer.Size.Name == "Small Convert")
            {
                string newDescription = Product.Name;

                var maxTitleLen = 43;
                string titleCondition = new Regex(@"\(.*\)").Match(Product.Name).Value;
                if (Product.Name.Length > maxTitleLen)
                {
                    newDescription = Product.Name.Substring(0, (maxTitleLen - titleCondition.Length - 4)) + "... " + titleCondition;
                }

                temp.SetAttribute("description", newDescription);
                temp.SetAttribute("formatted_date", localTime.ToString("yyyy/MM/dd hh:mm"));
            }
            else if (Printer.Size.Name == "Medium")
            {
                string newDescription = Product.Name;

                var maxTitleLen = 53;
                string titleCondition = new Regex(@"\(.*\)").Match(Product.Name).Value;
                if (Product.Name.Length > maxTitleLen)
                {
                    newDescription = Product.Name.Substring(0, (maxTitleLen - titleCondition.Length - 4)) + "... " + titleCondition;
                }

                temp.SetAttribute("description", newDescription);
                temp.SetAttribute("formatted_date", localTime.ToString("yyyy-MM-dd hh:mm"));
            }
            
            string zpl = temp.ToString();

            response.Add("contentType", "raw_base64");
            response.Add("content", Base64Encode(zpl));
            response.Add("source", "Maven app - " + Users.UsersProvider.GetCurrentUser().Email);
            response.Add("qty", Quantity.ToString());

            return response;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PrintNodePrintJobStatuses
    {
        New,
        pending_confirmation,
        sent_to_client,
        deleted,
        done,
        error,
        in_progress,
        queued,
        disappeared,
        received,
        downloading,
        downloaded,
        preparing_to_print,
        queued_to_print,
        expired
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum MssPrintJobStatus
    {
        Null,
        InProgress,
        Success,
        Error
    }

    public static class MssPrintJobStatusExtensions
    {
        public static MssPrintJobStatus ToMssPrintJobStatus(this PrintNodePrintJobStatuses PrintNodeStatus)
        {
            switch (PrintNodeStatus)
            {
                case PrintNodePrintJobStatuses.done:
                    return MssPrintJobStatus.Success;

                case PrintNodePrintJobStatuses.error:
                case PrintNodePrintJobStatuses.deleted:
                case PrintNodePrintJobStatuses.disappeared:
                case PrintNodePrintJobStatuses.expired:
                    return MssPrintJobStatus.Error;

                default:
                    return MssPrintJobStatus.InProgress;
            }
        }
    }
}