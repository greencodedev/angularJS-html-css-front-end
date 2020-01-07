using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.Utils.Enums
{
    public class EnumValue : Attribute
    {
        private string _name;
        private object _value;

        public EnumValue(string name, object value)
        {
            _name = name;
            _value = value;
        }

        public string Name { get { return _name; } }
        public object Value { get { return _value; } }
    }
}