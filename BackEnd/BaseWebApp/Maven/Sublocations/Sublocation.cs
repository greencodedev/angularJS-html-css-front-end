using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BaseWebApp.Maven.Products;
using Newtonsoft.Json.Linq;

namespace BaseWebApp.Maven.Sublocations
{
    public class Sublocation
    {
        public int? SublocationId { get; set; }
        public string FinaleSublocationId { get; set; }
        public Sublocation ParentLocation { get; set; }
        public string Name { get; set; }
        public SublocationStatus Status { get; set; }
        public string Description { get; set; }
        public bool? ConvertibleFrom { get; set; }
        public bool? ConvertibleTo { get; set; }
        public string Url { get; set; }
        public int Stock { get; set; }
        public bool? UserConvertibleFrom { get; set; }
        public bool? UserConvertibleTo { get; set; }
    }

    public enum SublocationStatus
    {
        Null,
        FACILITY_ACTIVE,
        FACILITY_INACTIVE
    }

    public class FinaleSublocationResponse
    {
        public List<string> facilityId { get; set; }
        public List<string> facilityUrl { get; set; }
        public List<string> statusId { get; set; }
        public List<string> facilityName { get; set; }
        public List<string> parentFacilityUrl { get; set; }
    }
}