using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.FinaleInventory
{
    public class FinaleCategoryResponse
    {
        public List<FinaleCategory> productUserCategoryList { get; set; }
        public List<ProductType> productTypeList { get; set; }
    }

    public class FinaleCategory
    {
        public string attrName { set; get; }
        public string label { set; get; }
        public guiOptions guiOptions { set; get; }

    }

    public class guiOptions
    {
        public int sortOrder { set; get; }
    }

    public class ProductType
    {
        public List<FinalCustomField> userFieldDefList { set; get; }
    }

    public class FinalCustomField
    {
        public string attrName { set; get; }
        public string label { set; get; }
        public string dataType { set; get; }
        public List<string> optionList = new List<string>();
    }
}