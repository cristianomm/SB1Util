using System;
using SB1Util.Connection;

namespace SB1Util.Misc
{
    public abstract class UserObjectFactory
    {
        protected string[] objects;
        protected B1Connection oConnection;
        protected DB.DBFacade oDBFacade;



        public B1Connection OConnection
        {
            get { return oConnection; }
            set { oConnection = value; }
        }

        
        public bool isObject(string obj)
        {
            bool ret = false;
            try
            {
                foreach (string s in objects)
                {
                    if (obj.Equals(s))
                    {
                        ret = true;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                ret = false;
            }
            return ret;

        }


        public abstract SAPObject GetUserObject(string userObject);


    }
}
