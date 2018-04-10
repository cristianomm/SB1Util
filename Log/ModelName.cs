using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SB1Util.Log
{
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
    public class ModelName : System.Attribute
    {

        public string Name { get; set; }

        public ModelName(string name)
        {
            this.Name = name;
        }

    }
}
