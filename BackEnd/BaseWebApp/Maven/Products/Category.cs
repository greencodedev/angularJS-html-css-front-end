using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.Products
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string AttrName { get; set; }
        public string Label { get; set; }
        public int SortOrder { get; set; }
        
    }
}