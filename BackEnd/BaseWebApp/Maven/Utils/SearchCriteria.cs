using BaseWebApp.Maven.PrintNode.Printers;
using BaseWebApp.Maven.PrintNode.PrintJobs;
using BaseWebApp.Maven.Sublocations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.Utils
{
    public class SearchCriteria
    {
      
        public int? category { set; get;}
        public string TextSearch { set; get; }
        public bool? Status { get; set; }
        public PrinterStates? PrinterState { get; set; }
        public SublocationStatus? SublocationStatus { get; set; }
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Conditions { get; set; }
        public string SortBy { get; set; }
        public int PrinterId { get; set; }
        public int ProductId { get; set; }
        public int? SubLocationId { get; set; }
        public string UserId { get; set; }
        public MssPrintJobStatus printJobStatus { get; set; }
        public string Ncs { get; set; }
        public string FinaleId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string TnxType { get; set; }
        public bool OnlyAssingedPrinters { get; set; }

        public bool OnlySublocation { get; set; }

        public bool Ascending = true;
        public bool Paginate = false;
        public int Page = 1;
        public int Limit = 20;
        public bool ReadUncommited = false;
        public int? ChannelId { get; set; }
    }
}