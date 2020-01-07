using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BaseWebApp.Maven.Products;
using BaseWebApp.Maven.Sublocations;
using BaseWebApp.Maven.Users;
using Newtonsoft.Json.Linq;

namespace BaseWebApp.Maven.Conversions
{
    public class Conversion
    {
        public int ConversionId { get; set; }
        public Product FromProduct { get; set; }
        public Product ToProduct { get; set; }
        public Sublocation FromSublocation { get; set; }
        public Sublocation ToSublocation { get; set; }
        public string Note { get; set; }
        public int Quantity { get; set; }
        public string PoNumber { get; set; }
        public bool CreateNewProduct { get; set; }
        public string ConvertType { get; set; }
        public User user { get; set; }
        public DateTime DateTime { get; set; }
        

        public JObject ConvertToFinaleObject()
        {
            List <Variance> varienceList = new List<Variance>();

            varienceList.Add(new Variance {
                productUrl = this.FromProduct.Url,
                facilityUrl = this.FromSublocation.Url,
                quantityOnHandVar = System.Math.Abs(this.Quantity) * (-1)
            });

            varienceList.Add(new Variance
            {
                productUrl = this.ToProduct.Url,
                facilityUrl = this.FromSublocation.Url,
                quantityOnHandVar = this.Quantity
            });

            JObject finaleConversion = new JObject
            {

            };

            finaleConversion.Add("physicalInventoryDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 17, 0, 0));
            finaleConversion.Add("statusId", "PHSCL_INV_COMMITTED");
            finaleConversion.Add("physicalInventoryTypeId", "FACILITY");
            finaleConversion.Add("facilityUrl", this.FromSublocation.Url);

            finaleConversion.Add("inventoryItemVarianceList", JArray.FromObject(varienceList));

            return finaleConversion;
        }

        public JObject CreateFinaleTransfer()
        {
            JObject finaleTransfer = new JObject();

            finaleTransfer.Add("createdDate", DateTime.Now);
            finaleTransfer.Add("productId", this.ToProduct.ProductId);
            finaleTransfer.Add("productUrl", this.ToProduct.Url);
            finaleTransfer.Add("facilityUrlFrom", this.FromSublocation.Url);
            finaleTransfer.Add("facilityUrlTo", this.ToSublocation.Url);
            finaleTransfer.Add("quantity", this.Quantity);

            return finaleTransfer;
        }
    }

    public class Variance
    {
        public string productUrl { get; set; }
        public string facilityUrl { get; set; }
        public int quantityOnHandVar { get; set; }
    }
}