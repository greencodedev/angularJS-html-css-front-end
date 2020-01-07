using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.PrintNode.Printers
{
    public class Printer
    {
        public int PrinterId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CustomName { get; set; }
        // Active is our status, not PrintNode's
        public bool Active { get; set; }
        public bool IsAssigned { get; set; }
        // PrinterState is PrintNode status
        public PrinterStates State { get; set; }
        public PrinterSize Size { get; set; }
    }

    public enum PrinterStates
    {
        disappeared,
        offline,
        online
    }
}