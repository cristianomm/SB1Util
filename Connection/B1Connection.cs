using System;
using SAPbobsCOM;
using SAPbouiCOM;
using SB1Util.Misc;
using System.Windows.Forms;

namespace SB1Util.Connection
{
    /*
     * Esta classe é responsavel por implementar a conexao ao B1 na forma Single Singon
     * */
    public class B1Connection
    {
        private static B1Connection instance;
        //private static ILog logger = LogManager.GetLogger("B1Connection");
        private Log.Logger logger;
        private string CONNSTR;

        private SAPbouiCOM.Application oApp;
        private SAPbobsCOM.Company oCompany;
        private SboGuiApi sboGui;
        private ConfLoader configs;
        private NotifyIcon notifyIcon;


        private B1Connection()
        {
            notifyIcon = new NotifyIcon();
            this.logger = Log.Logger.getInstance();
        }


        public SAPbobsCOM.Company connect(string connString)
        {
            try{
                sboGui = new SboGuiApi();
                CONNSTR = connString;
                sboGui.Connect(CONNSTR);
                oApp = sboGui.GetApplication();
                oCompany = (SAPbobsCOM.Company)oApp.Company.GetDICompany();
                
                oApp.StatusBar.SetText("Conectado", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                //logger.log("Conectado", Log.Logger.LogType.INFO);
            }
            catch(Exception e)
            {
                SB1ControlException.SB1ControlException.Save(e);
            }

            return oCompany;

        }

        public void disconnect()
        {
            try
            {
                oCompany.Disconnect();
            }
            catch(Exception e)
            {
                SB1ControlException.SB1ControlException.Save(e);
            }
        }

        public static B1Connection getInstance()
        {
            if (instance == null)
            {
                instance = new B1Connection();
            }
            return instance;
        }


        public SAPbouiCOM.Application App
        {
            get { return oApp; }
            set { oApp = value; }
        }

        public SAPbobsCOM.Company Company
        {
            get { return oCompany; }
            set { oCompany = value; }
        }

        public SboGuiApi SboGui
        {
            get { return sboGui; }
            set { sboGui = value; }
        }

        public ConfLoader Configurations
        {
            get { return configs; }
        }


        public NotifyIcon NotifyIcon
        {
            get { return notifyIcon; }
        }

        public void addConfFile(string fileName)
        {
            configs = ConfLoader.getInstance(fileName);
        }

        public bool isConnected()
        {
            bool con;
            try
            {
                con = oCompany.Connected;
            }
            catch (Exception e)
            {
                SB1ControlException.SB1ControlException.Save(e);
                con = false;
            }
            return con;

        }

    }
}
