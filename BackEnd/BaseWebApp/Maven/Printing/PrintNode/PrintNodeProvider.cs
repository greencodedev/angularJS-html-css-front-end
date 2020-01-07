using BaseWebApp.Maven.PrintNode.Printers;
using BaseWebApp.Maven.PrintNode.PrintJobs;
using BaseWebApp.Maven.Products;
using BaseWebApp.Maven.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;

namespace BaseWebApp.Maven.PrintNode
{
    public static class PrintNodeProvider
    {
        public static string BaseUrl = "https://api.printnode.com/";

        public static List<Printer> GetPrinters()
        {
            HttpResponseMessage result = new HttpResponseMessage();
            List<Printer> printerList = new List<Printer>();


            result = MakeGetRequest("printers?limit=250");
            if (result.IsSuccessStatusCode)
            {
                printerList = JsonConvert.DeserializeObject<List<Printer>>(result.Content.ReadAsStringAsync().Result);
            }
            else
            {
                throw new Exception("Error getting printers");
            }
            
            return printerList;
        }

        public static PrintJob GetPrintJob(int printJobId)
        {
            PrintJob printJob = null;

            HttpResponseMessage result = MakeGetRequest("printjobs/" + printJobId);

            if (result.IsSuccessStatusCode)
            {
                string res = result.Content.ReadAsStringAsync().Result;
                List<PrintJob> jobs = JsonConvert.DeserializeObject<List<PrintJob>>(result.Content.ReadAsStringAsync().Result);
                if(jobs.Any())
                {
                    printJob = jobs[0];
                    printJob.Status = printJob.state.ToMssPrintJobStatus();
                }
            }
            
            return printJob;
        }

        internal static List<PrintJob> GetPrintJobs()
        {
            throw new NotImplementedException();
        }

        public static int CreatePrintJob(PrintJob printJob)
        {
            HttpResponseMessage result = new HttpResponseMessage();
            
            result = MakePostRequest("printjobs", printJob.PrepareForPrintNode());
            return int.Parse(result.Content.ReadAsStringAsync().Result);
        }

        private static HttpResponseMessage MakePostRequest(string url, Dictionary<string, string> content)
        {
            HttpResponseMessage result = new HttpResponseMessage();
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);
            var byteArray = Encoding.ASCII.GetBytes(Config.GetConfig("PRINT_NODE_TOKEN"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            FormUrlEncodedContent urlEncodedContent = new FormUrlEncodedContent(content);

            Logger.Log("PRINT-NODE Client :: POST :: " + url);
            result = client.PostAsync(BaseUrl + url, urlEncodedContent).Result;
            Logger.Log("PRINT-NODE Client :: POST :: " + url + " :: " + result.StatusCode);

            return result;
        }

        public static HttpResponseMessage MakeGetRequest(string url)
        {
            HttpResponseMessage result = new HttpResponseMessage();

            HttpClientHandler handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);
            var byteArray = Encoding.ASCII.GetBytes(Config.GetConfig("PRINT_NODE_TOKEN"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            Logger.Log("PRINT-NODE Client :: GET :: " + url);
            result = client.GetAsync(BaseUrl + url).Result;
            Logger.Log("PRINT-NODE Client :: GET :: " + url + " :: " + result.StatusCode);

            return result;
        }

    }
}