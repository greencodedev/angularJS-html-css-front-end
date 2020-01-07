using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.Custom_Fields
{
    public class ProductCustomField
    {
        public int ProductCustomFieldId { get; set; }
        public int AccountId { get; set; }
        public int ProductId { get; set; }
        public int CustomFieldId { get; set; }
        public string Value { get; set; }
    }
}