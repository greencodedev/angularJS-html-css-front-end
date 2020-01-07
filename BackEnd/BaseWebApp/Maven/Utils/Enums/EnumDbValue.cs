using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.Utils.Enums
{
    public class EnumDbValue : Attribute
    {
        private string _dbValue;

        public EnumDbValue(string dbValue)
        {
            _dbValue = dbValue;
        }

        public string DbValue
        {
            get { return _dbValue; }
        }
    }
}