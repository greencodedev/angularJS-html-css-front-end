using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.Products.Conditions
{
    public class Condition
    {
        public int ConditionId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Convertible { get; set; }
        public ConditionStatus status { get; set; }
        public bool? UserConvertibleFrom { get; set; }
        public bool? UserConvertibleTo { get; set; }
    }

    public enum ConditionStatus
    {
        Active,
        Removed
    }
}