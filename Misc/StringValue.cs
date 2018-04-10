using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace SB1Util.Misc
{
    public class EnumStringValue:System.Attribute
    {
        private string stringValue;

        public EnumStringValue(string value)
        {
            stringValue = value;
        }

        public string StringValue
        {
            get { return stringValue; }
        }

        public static string GetEnumStringValue(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            EnumStringValue[] attributes =
                (EnumStringValue[])fi.GetCustomAttributes(typeof(EnumStringValue), false);

            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].StringValue;
            }
            else
            {
                return value.ToString();
            }
        }


    }
}
