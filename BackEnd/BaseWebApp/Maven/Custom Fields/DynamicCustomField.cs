using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.Custom_Fields
{
    public class DynamicCustomField
    {
        public int CustomFieldId { get; set; }
        public int AccountId { get; set; }
        public string Name { get; set; }
        public bool Active = true;
        public string FinaleCustomFieldId { get; set; }
        public string AmazonCustomFieldId { get; set; }
        public bool ShowOnProduct = true;
        public bool OptionToFilter = true;
    }
}