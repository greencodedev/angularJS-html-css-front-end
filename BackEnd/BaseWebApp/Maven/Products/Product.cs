using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BaseWebApp.Maven.Custom_Fields;
using BaseWebApp.Maven.FinaleInventory;
using BaseWebApp.Maven.Products.Conditions;
using BaseWebApp.Maven.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BaseWebApp.Maven.Products
{
    public class Product
    {
        public int ProductId { get; set; }
        public int ChannelId { get; set; }
        public Category Category { get; set; }
        public string FinaleId { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { set; get; }
        public string Ncs { get; set; }
        public double? Price { get; set; }
        public double? CasePrice { get; set; }
        public double? Cost { get; set; }
        public double? CostFromConversion { get; set; }
        public string LastCostInfo { get; set; }
        public Condition Condition { set; get; }
        public bool _DuplicateCondNcs { get; set; }
        public string AverageCost { get; set; }
        


        public List<ProductCustomField> CustomFields = new List<ProductCustomField>();
        

        public JObject ConvertToFinaleObject()
        {
            JObject finaleProduct = new JObject();
            List<UserFieldData> customFieldList = new List<UserFieldData>();
            List<FinalePrice> priceList = new List<FinalePrice>();
            

            if (!string.IsNullOrEmpty(Condition.Name))
            {
                customFieldList.Add(new UserFieldData
                {
                    attrName = Config.GetConfig("FINALE_CUSTOM_FIELD_CONDITION"),
                    attrValue = Condition.Name
                });
            }

            if (!string.IsNullOrEmpty(Ncs))
            {
                customFieldList.Add(new UserFieldData
                {
                    attrName = Config.GetConfig("FINALE_CUSTOM_FIELD_NCS"),
                    attrValue = Ncs
                });
            }

            if (CostFromConversion != null && CostFromConversion != 0)
            {
                customFieldList.Add(new UserFieldData
                {
                    attrName = Config.GetConfig("FINALE_CUSTOM_FIELD_CFC"),
                    attrValue = CostFromConversion.ToString()
                });
            }

            if (!string.IsNullOrEmpty(LastCostInfo))
            {
                customFieldList.Add(new UserFieldData
                {
                    attrName = Config.GetConfig("FINALE_CUSTOM_FIELD_LCI"),
                    attrValue = LastCostInfo
                });
            }

            if (Price != null && Price != 0)
            {
                priceList.Add(new FinalePrice
                {
                    currencyUomId = "USD",
                    productPriceTypeId = "LIST_PRICE",
                    price = Price
                });
            }

            if (CasePrice != null && CasePrice != 0)
            {
                priceList.Add(new FinalePrice
                {
                    currencyUomId = "USD",
                    productPriceTypeId = "LIST_CASE_PRICE",
                    price = CasePrice
                });
            }


            if (customFieldList.Count() > 0)
            {
                finaleProduct.Add("userFieldDataList", JArray.FromObject(customFieldList));
            }

            if (priceList.Count() > 0)
            {
                finaleProduct.Add("priceList", JArray.FromObject(priceList));
            }

            if (!string.IsNullOrEmpty(Name))
            {
                finaleProduct.Add("internalName", Name);
            }

            if (!string.IsNullOrEmpty(Description))
            {
                finaleProduct.Add("longDescription", Description);
            }

            if (Category != null && !string.IsNullOrEmpty(Category.AttrName))
            {
                finaleProduct.Add("userCategory", Category.AttrName);
            }

            if (Cost != null && Cost != 0)
            {
                finaleProduct.Add("cost", Cost);
            }

            finaleProduct.Add("statusId", "PRODUCT_ACTIVE");

            return finaleProduct;
        }

        public JObject UpdateConvertInfo(int channelId)
        {
            JObject finaleProduct = FinaleClientProvider.GetProductObject(FinaleId, false, channelId);
            List<UserFieldData>  customFieldList = JsonConvert.DeserializeObject<List<UserFieldData>>(finaleProduct.GetValue("userFieldDataList").ToString());

            if (CostFromConversion != null && CostFromConversion != 0)
            {

                UserFieldData field = customFieldList.Find(f => f.attrName == Config.GetConfig("FINALE_CUSTOM_FIELD_CFC"));
                if (field != null)
                {
                    field.attrValue = CostFromConversion.ToString();
                }
                else
                {
                    customFieldList.Add(new UserFieldData
                    {
                        attrName = Config.GetConfig("FINALE_CUSTOM_FIELD_CFC"),
                        attrValue = CostFromConversion.ToString()
                    });
                }
            }

            if (!string.IsNullOrEmpty(LastCostInfo))
            {
                UserFieldData field = customFieldList.Find(f => f.attrName == Config.GetConfig("FINALE_CUSTOM_FIELD_LCI"));
                if (field != null)
                {
                    field.attrValue = LastCostInfo;
                } else
                {
                    customFieldList.Add(new UserFieldData
                    {
                        attrName = Config.GetConfig("FINALE_CUSTOM_FIELD_LCI"),
                        attrValue = LastCostInfo
                    });
                }
            }

            if (customFieldList.Count() > 0)
            {
                finaleProduct["userFieldDataList"] = JArray.FromObject(customFieldList);
            }

            finaleProduct["statusId"] = "PRODUCT_ACTIVE";

            return finaleProduct;
        }
    }
}