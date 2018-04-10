using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace SB1Util.UI
{
    public class DataTransfer
    {
        private LinkedList<Data> data;

        public struct Data
        {
            public string name;
            public string value;
        };


        private DataTransfer()
        {
            
            this.data = new LinkedList<Data>();
        }


        public void addValue(string valName, string value)
        {
            Data d;
            if (!getValue(valName).Equals(""))
            {
                d.name = valName;
                d.value = value;
            }
            else
            {
                d.name = valName;
                d.value = value;
            }

            data.AddLast(d);
        }

        public string getValue(string valName)
        {
            Data ret;
            ret.name = "";
            ret.value = "";
            foreach (Data d in data)
            {
                if (d.name.Equals(valName))
                {
                    ret.name = d.name;
                    ret.value = d.value;

                }
            }

            return ret.value;

        }



    }
}
