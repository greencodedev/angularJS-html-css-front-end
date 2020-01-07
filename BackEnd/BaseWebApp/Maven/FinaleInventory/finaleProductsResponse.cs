using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.FinaleInventory
{
    public class FinaleProductsResponse
    {
        public List<string> productId { get; set; }
        public List<string> internalName { get; set; }
        public List<string> longDescription { get; set; } 
        public List<string> userCategory { get; set; }
        public List<string> statusId { get; set; }
        public List<string> productUrl { get; set; }
        public List<double?> cost { get; set; }
        public List<List<UserFieldData>> userFieldDataList { get; set;}
        public List<List<FinalePrice>> priceList { get; set; }
    }

    public class FinaleProductResponse
    {
        public string productId { get; set; }
        public string internalName { get; set; }
        public string longDescription { get; set; }
        public string userCategory { get; set; }
        public string statusId { get; set; }
        public string productUrl { get; set; }
        public double? cost { get; set; }
        public List<UserFieldData> userFieldDataList { get; set; }
        public List<FinalePrice> priceList { get; set; }
    }

    public class FinalePrice
    {
        public string currencyUomId { get; set; }
        public string productPriceTypeId { get; set; }
        public double? price { get; set; }
    }

    public class UserFieldData
    {
        public string attrName { get; set; }
        public string attrValue { get; set; }
    }

    public class FinaleInventoryItemResponse
    {
        public List<string> inventoryItemUrl { get; set; }
        public List<string> productId { get; set; }
        public List<string> productUrl { get; set; }
        public List<string> facilityUrl { get; set; }
        public List<int> quantityOnHand { get; set; }
        public List<string> parentFacilityUrl { get; set; }
    }

    public class TransactionReportData
    {
        public string MasterVarirables { get; set; }
        public List<List<string>> data { get; set; }
    }

    public class TrasactionReportObject
    {
        public string FinaleId { get; set; }
        public DateTime? ReceivedTimestamp { get; set; }
        public DateTime RecordDate { get; set; }
        public string TransactionType { get; set; }
        public string TransactionDescription { get; set; }
        public int Quantity { get; set; }
        public double CostPerUnit { get; set; }
    }
}