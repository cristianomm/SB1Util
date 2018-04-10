using System;
using SAPbobsCOM;
using SB1Util.Log;
using SB1Util.DB;



namespace SB1Util.Misc
{
    public abstract class SAPObject
    {

        protected string code;
        protected string name;
        protected bool managed;
        protected UserTable userTable;
        protected Logger logger;
        protected DBFacade oDBFacade;


        public SAPObject()
        {
            this.logger = Logger.getInstance();
            this.oDBFacade = DBFacade.getInstance();
        }



        public string Code
        {
            get { return code; }
            set { code = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        
        public UserTable UserTable
        {
            get { return userTable; }
            set { userTable = value; }
        }

        public bool Managed
        {
            get { return managed; }
            set { managed = value; }
        }


        public virtual void loadKey(UserObjectFactory oFactory)
        {
            Recordset rs = null;
            try
            {
                rs = (Recordset)oFactory.OConnection.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                //rs.DoQuery("SELECT MAX(CONVERT(INT, Code))+1[Code] FROM [@" + userTable.TableName + "]");
                //code = Convert.ToString(rs.Fields.Item("Code").Value);

                code = Convert.ToString(Convert.ToInt32(DB.DBFacade.getInstance().Max("[@" + userTable.TableName + "]"))+1);
                name = code;
                userTable.Code = code;
                userTable.Name = name;
            }
            catch (Exception e)
            {
                if (rs != null && rs.RecordCount > 0)
                {
                    code = "1";
                }
            }
        }

        public virtual long loadFromDB(string key, UserObjectFactory oFactory)
        {
            long ret = 0;

            //necessario remover os zeros a esquerda...
            key = "" + Convert.ToInt32(key);
            userTable.GetByKey(key);
            code = userTable.Code;
            name = userTable.Name;

            return ret;
        }

        public virtual long add()
        {
            userTable.Code = code;
            userTable.Name = name;
            return userTable.Add();
        }

        public virtual long remove()
        {
            long ret = userTable.Remove();
            return ret;
        }

        public virtual long update()
        {
            long ret = userTable.Update();
            return ret;
        }

    }
}
