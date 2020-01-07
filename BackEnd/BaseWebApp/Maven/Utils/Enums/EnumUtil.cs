using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;

namespace BaseWebApp.Maven.Utils.Enums
{
    public class EnumUtil
    {
        private static Hashtable _stringValues = new Hashtable();

        public static string GetDbValue(Enum value)
        {
            string output = null;
            Type type = value.GetType();

            //Check first in our cached results...
            if (_stringValues.ContainsKey(value))
                output = (_stringValues[value] as EnumDbValue).DbValue;
            else
            {
                //Look for our 'StringValueAttribute' 
                //in the field's custom attributes
                FieldInfo fi = type.GetField(value.ToString());
                EnumDbValue[] attrs =
                   fi.GetCustomAttributes(typeof(EnumDbValue),
                                           false) as EnumDbValue[];
                if (attrs.Length > 0)
                {
                    _stringValues.Add(value, attrs[0]);
                    output = attrs[0].DbValue;
                }
            }

            return output;
        }

        public static object GetValue(Enum value, string name)
        {
            object output = null;
            Type type = value.GetType();


            //Look for our 'EnumValue' 
            //in the field's custom attributes
            FieldInfo fi = type.GetField(value.ToString());
            EnumValue[] attrs =
                fi.GetCustomAttributes(typeof(EnumValue),
                                        false) as EnumValue[];
            if (attrs.Length > 0)
            {
                EnumValue enumValue = attrs.First(x => x.Name == name);

                if (enumValue != null)
                {
                    output = enumValue.Value;
                }
            }

            return output;
        }

        public static object Parse(Enum type, string stringValue)
        {
            return Parse(type.GetType(), stringValue, false);
        }

        public static object Parse(Type type, string stringValue, bool ignoreCase)
        {
            object output = null;
            string enumStringValue = null;

            if (!type.IsEnum)
                throw new ArgumentException(String.Format("Supplied type must be an Enum.  Type was {0}", type.ToString()));

            //Look for our string value associated with fields in this enum
            foreach (FieldInfo fi in type.GetFields())
            {
                //Check for our custom attribute
                EnumDbValue[] attrs = fi.GetCustomAttributes(typeof(EnumDbValue), false) as EnumDbValue[];
                if (attrs.Length > 0)
                    enumStringValue = attrs[0].DbValue;

                //Check for equality then select actual enum value.
                if (string.Compare(enumStringValue, stringValue, ignoreCase) == 0)
                {
                    output = Enum.Parse(type, fi.Name);
                    break;
                }
            }

            return output;
        }

        public static string GetEnumMemberAttrValue<T>(T enumVal)
        {
            var enumType = typeof(T);
            var memInfo = enumType.GetMember(enumVal.ToString());
            var attr = memInfo.FirstOrDefault()?.GetCustomAttributes(false).OfType<EnumMemberAttribute>().FirstOrDefault();
            if (attr != null)
            {
                return attr.Value;
            }

            return null;
        }
    }
}