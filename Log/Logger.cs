using System;
using System.Text;
using SB1Util.Misc;
using SB1Util.UI;
using SB1Util.Connection;
using System.IO;
using SAPbobsCOM;
using SB1Util.DB;
using System.Reflection;
using SB1Util.Serializer;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;

namespace SB1Util.Log
{

    public class Logger
    {
        /// <summary>
        /// Os tipos possíveis de Log no sistema
        /// </summary>
        public enum LogType
        {
            DEBUG, INFO, WARNING, ERROR, FATAL
        }

                
        private static Logger instance;
        //private log4net.ILog logger;        
        private Properties properties;
        //private TextWriter fileLog;
        private UIUtils uiUtils;

        private bool prepared;
        private string user;
        private string companyVersion;
        private string libVersion;
        private string addonVersion;
        private string addonName;
        private string xmlException;

        private Stack<string> opStack;




        private Logger()
        {
            try
            {
                //this.fileLog = new StreamWriter(Directory.GetCurrentDirectory() + "/log.log", true, Encoding.UTF8);
                this.opStack = new Stack<string>();
            }
            catch (Exception e)
            {
                //B1Connection.getInstance().App.SetStatusBarMessage("Erro ao Iniciar Log: " + e.Message, SAPbouiCOM.BoMessageTime.bmt_Long);
                //this.fileLog = new StreamWriter(Directory.GetCurrentDirectory() + "/log_"+ DateTime.Now +".log", true, Encoding.UTF8);

                SB1ControlException.SB1ControlException.Save(e);
            }

        }
        /// <summary>
        /// Retorna as informações das DLL's SB1Util e Model
        /// </summary>
        public void prepare()
        {
            try
            {
                prepared = true;
                this.user = DBFacade.getInstance().Connection.Company.UserName;
                this.companyVersion = "" + DBFacade.getInstance().Connection.Company.Version;

                Assembly AssLib = Assembly.LoadFrom("SB1Util.dll");
                Assembly AssMod = Assembly.LoadFrom("Model.dll");
                
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo("SB1Util.dll");
                this.libVersion = versionInfo.FileVersion;// AssLib.GetName().Version.ToString();
                
                versionInfo = FileVersionInfo.GetVersionInfo("Model.dll");
                this.addonVersion = versionInfo.FileVersion;// AssMod.GetName().Version.ToString();

                System.Attribute attr = System.Attribute.GetCustomAttribute(AssMod, typeof(ModelName));
                this.addonName = ((ModelName)attr).Name;
                
            }catch(Exception e)
            {
                //B1Connection.getInstance().App.SetStatusBarMessage("Erro ao Iniciar Log: " + e.Message, SAPbouiCOM.BoMessageTime.bmt_Long);
                SB1ControlException.SB1ControlException.Save(e); 
            }
        }
        /// <summary>
        /// Flag que sinalizar que o Log pode ou não estar pronto para ser executado
        /// </summary>
        public bool Prepared
        {
            get { return prepared; }
            set { prepared = value; }
        }

        public void pushOperation(string operation)
        {
            try
            {
                opStack.Push(operation);
            }
            catch (Exception e)
            {
                log("Erro ao empilhar operacao: " + e.Message, LogType.ERROR, e, false);
            }
        }

        public string releaseOperation()
        {
            string ret = "";
            try
            {
                ret = opStack.Pop();
            }
            catch (Exception e)
            {
                ret = "";
                log("Erro ao desempilhar operacao: " + e.Message, LogType.ERROR, e, false);
            }
            return ret;
        }


        /// <summary>
        /// Retorna um objeto Logger instanciado
        /// </summary>
        /// <returns></returns>
        public static Logger getInstance()
        {
            if (instance == null)
            {
                instance = new Logger();
            }
            return instance;
        }

        public void close()
        {
            //this.fileLog.Close();
        }

        public void setProperties(Properties properties)
        {
            this.properties = properties;
        }

        public void setUiUtils(UIUtils uiUtils)
        {
            this.uiUtils = uiUtils;
        }
        /// <summary>
        /// Método que dispara as rotinas para gerar log de exceção
        /// </summary>
        /// <param name="message">A mensagem de Exceção ou erro </param>
        /// <param name="type">O tipo de log, podendo ser DEBUG, INFO, WARNING, ERROR, FATAL </param>
        /// <param name="ex">A exceção disparada</param>
        /// <param name="statusBar">sinalizar se o erro será mostrado na statusbar do B1</param>
        public void log(string message, LogType type, Exception ex = null, bool statusBar = true, bool messageBox = false)
        {
            try
            {
                if (prepared)
                {
                    switch (type)
                    {
                        case LogType.DEBUG:
                            Debug(message, ex, statusBar, messageBox);
                            break;

                        case LogType.ERROR:
                            Error(message, ex, statusBar, messageBox);
                            break;

                        case LogType.FATAL:
                            Fatal(message, ex, statusBar, messageBox);
                            break;

                        case LogType.INFO:
                            Info(message, ex, statusBar, messageBox);
                            break;

                        case LogType.WARNING:
                            Info(message, null, statusBar, messageBox);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                prepared = false;
                //setStatusBarMessage("Erro ao escrever log: " + e.Message + " " + e.StackTrace, true, LogType.INFO);
                SB1ControlException.SB1ControlException.Save(e);
            }

        }

        /// <summary>
        /// Trata o nível de exceção em tempo de debug. Útil para depuração do sistema
        /// </summary>
        /// <param name="message">A mensagem de erro</param>
        /// <param name="ex">A exceção disparada. Se não for informado nada, o valor padrão é null</param>
        /// <param name="statusBar">Flag que diz se o erro será mostrado em statusbar do B1. Se não for informado nada, o valor padrão é true</param>
        private void Debug(string message, Exception ex = null, bool statusBar = true, bool messageBox = false)
        {
            string msg = message + (ex != null ? "\n" + ex.Message + "\n" + ex.StackTrace + "\n" + ex.Source : "");
            if (statusBar) setStatusBarMessage(msg, false, LogType.DEBUG);
            printText(msg, LogType.DEBUG);
            if (messageBox)
            {
                showMessageBox(msg, LogType.DEBUG);
            }
        }

        /// <summary>
        /// Trata o nível de exceção quando é disparada a exception de fato ou de regra de negócios. 
        /// Neste nível, o sistema não parou, mas o erro ocorreu. Nível básico
        /// </summary>
        /// <param name="message">A mensagem de Exceção</param>
        /// <param name="ex">A Exceção Disparada. Se não for informado nada, o valor padrão é null</param>
        /// <param name="statusBar">Flag que diz se o erro será mostrado em statusbar do B1. Se não for informado nada, o valor padrão é true</param>
        private void Info(string message, Exception ex = null, bool statusBar = true, bool messageBox = false)
        {
            string msg = message + (ex != null ? "\n" + ex.Message + "\n" + ex.StackTrace + "\n" + ex.Source : "");
            if (statusBar) setStatusBarMessage(msg, false, LogType.INFO);
            printText(msg, LogType.INFO);
            if (messageBox)
            {
                showMessageBox(msg, LogType.INFO);
            }
        }

        /// <summary>
        /// Trata o nível de exceção quando é disparada a exception de fato ou de regra de negócios. 
        /// Neste nível, o sistema não parou, mas o erro ocorreu. Nível básico
        /// </summary>
        /// <param name="message">A mensagem de Exceção</param>
        /// <param name="ex">A Exceção Disparada. Se não for informado nada, o valor padrão é null</param>
        /// <param name="statusBar">Flag que diz se o erro será mostrado em statusbar do B1. Se não for informado nada, o valor padrão é true</param>
        private void Error(string message, Exception ex = null, bool statusBar = true, bool messageBox = false)
        {
            string msg = message + (ex != null ? "\n" + ex.Message + "\n" + ex.StackTrace + "\n" + ex.Source : "");
            if (statusBar) setStatusBarMessage(msg, true, LogType.ERROR);
            printText(msg, LogType.ERROR);
            if (messageBox)
            {
                showMessageBox(msg, LogType.ERROR);
            }
        }
        /// <summary>
        /// Trata o nível de exceção quando é disparada a exception de fato ou de regra de negócios. 
        /// Neste nível, o sistema não parou, mas o erro ocorreu e este deve ter atenção . Nível Médio
        /// </summary>
        /// <param name="message">A mensagem de Exceção</param>
        /// <param name="ex">A Exceção Disparada. Se não for informado nada, o valor padrão é null</param>
        /// <param name="statusBar">Flag que diz se o erro será mostrado em statusbar do B1. Se não for informado nada, o valor padrão é true</param>
        private void Warning(string message, Exception ex = null, bool statusBar = true, bool messageBox = false)
        {
            string msg = message + (ex != null ? "\n" + ex.Message + "\n" + ex.StackTrace + "\n" + ex.Source : "");
            if (statusBar) setStatusBarMessage(msg, false, LogType.WARNING);
            printText(msg, LogType.WARNING);
            if (messageBox)
            {
                showMessageBox(msg, LogType.WARNING);
            }
        }
        /// <summary>
        /// Trata o nível de exceção quando é disparada a exception de fato ou de regra de negócios. 
        /// Neste nível, o sistema parou e o erro é crítico . Nível Catastrófico
        /// </summary>
        /// <param name="message">A mensagem de Exceção</param>
        /// <param name="ex">A Exceção Disparada. Se não for informado nada, o valor padrão é null</param>
        /// <param name="statusBar">Flag que diz se o erro será mostrado em statusbar do B1. Se não for informado nada, o valor padrão é true</param>
        private void Fatal(string message, Exception ex = null, bool statusBar = true, bool messageBox = false)
        {
            string msg = message + (ex != null ? "\n" + ex.Message + "\n" + ex.StackTrace + "\n" + ex.Source : "");
            if (statusBar) setStatusBarMessage(msg, true, LogType.FATAL);
            printText(msg, LogType.FATAL);
            if (messageBox)
            {
                showMessageBox(msg, LogType.FATAL);
            }
        }
        /// <summary>
        /// Salva o log em banco de dados. Caso ocorra falha, o log será gerado no diretório raiz, dentro do diretório LogSystem
        /// </summary>
        /// <param name="text">A mensagem da exceção</param>
        /// <param name="type">O tipo de exceção</param>
        private void printText(string text, LogType type)
        {
            DateTime dt = DateTime.Now;
            LoggerData LoggerDataObj = null;
            string Time = String.Format("{0:HH:mm:ss}", dt);

            try
            {
                LoggerDataObj = new LoggerData();
                string form = "";
                if (DBFacade.getInstance().Connection.App.Forms.ActiveForm != null)
                {
                    try
                    {
                        form = DBFacade.getInstance().Connection.App.Forms.ActiveForm.TypeEx;
                    }
                    catch (Exception e)
                    {
                        form = "";
                    }
                }


                UserTable table = DBFacade.getInstance().getUserTable("SB1_ADDON_LOG");
                table.Code = "" + (dt.Ticks + 1);
                table.Name = "" + (dt.Ticks + 1);

                //Ajusta valores nos campos pois pode ocorrer exception caso esteja NULL
                addonVersion = addonVersion == null ? "" : addonVersion;
                addonName = addonName == null ? "" : addonName;
                companyVersion = companyVersion == null ? "" : companyVersion;
                libVersion = libVersion == null ? "" : libVersion;
                type = type == null ? LogType.INFO : type;
                text = text == null ? "" : text;

                table.UserFields.Fields.Item("U_AddonVersion").Value = addonVersion;
                table.UserFields.Fields.Item("U_AddonName").Value = addonName;
                table.UserFields.Fields.Item("U_CompanyVersion").Value = companyVersion;
                table.UserFields.Fields.Item("U_Date").Value = dt;
                table.UserFields.Fields.Item("U_LibVersion").Value = libVersion;
                table.UserFields.Fields.Item("U_LogType").Value = type.ToString();
                table.UserFields.Fields.Item("U_Text").Value = text;
                table.UserFields.Fields.Item("U_Time").Value = Time;
                table.UserFields.Fields.Item("U_User").Value = user;
                table.UserFields.Fields.Item("U_FormID").Value = form;
                table.UserFields.Fields.Item("U_XMLException").Value = "";
                table.UserFields.Fields.Item("U_Operation").Value = (opStack.Count==0 ? "" : opStack.Peek());

                table.Add();
                table = null;

                LoggerDataObj.U_AddonVersion = addonVersion;
                LoggerDataObj.U_CompanyVersion = companyVersion;
                LoggerDataObj.U_Date = dt;
                LoggerDataObj.U_LibVersion = libVersion;
                LoggerDataObj.U_LogType = type.ToString();
                LoggerDataObj.U_Text = text;
                LoggerDataObj.U_Time = Time;
                LoggerDataObj.U_User = user;
            }
            catch (Exception e)
            {
                LogManager.SaveLoggerData(LoggerDataObj);
                SB1ControlException.SB1ControlException.Save(e);                
                //B1Connection.getInstance().App.SetStatusBarMessage("Erro ao escrever Log: " + e.Message, SAPbouiCOM.BoMessageTime.bmt_Long);
            }
        }
        /// <summary>
        /// Seta a mensagem de 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isError"></param>
        private void setStatusBarMessage(string message, bool isError)
        {
            try
            {
                if (uiUtils == null)
                {
                    log("Utilitario de interface não inicializado.", LogType.ERROR, null, false);
                    return;
                }

                uiUtils.StatusBarMessage(message, SAPbouiCOM.BoMessageTime.bmt_Long, isError);
            }
            catch (Exception e)
            {
            }

        }

        private void setStatusBarMessage(string message, bool isError, LogType LogType)
        {
            try
            {
                if (uiUtils == null)
                {
                    log("Utilitario de interface não inicializado.", LogType.ERROR, null, false);
                    return;
                }

                uiUtils.StatusBarMessage(message, LogType, SAPbouiCOM.BoMessageTime.bmt_Long);
            }
            catch (Exception e)
            {
            }
        }

        private void showMessageBox(string message, LogType type)
        {
            B1Connection.getInstance().App.MessageBox("[" + type.ToString() + "]\n" + message);
        }

        private string getFormattedText(string[] options)
        {
            string text = "";

            try
            {

            }
            catch (Exception e)
            {

            }
            return text;
        }


        public static void logCaos(string msg, Exception e)
        {
            //StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "\\log_" + DateTime.Now + ".log", true, Encoding.UTF8);
            //sw.WriteLine(msg + e.Message);
            //sw.WriteLine(e.Source + " " + e.StackTrace + " " + e.InnerException.StackTrace);
            SB1ControlException.SB1ControlException.Save(e); 
        }



    }
    [Serializable]
    public class LoggerData
    {
        /// <summary>
        /// Construtor Padrão. O serializer exige um construtor sem parametros
        /// </summary>
        public LoggerData()
        {
        }
        /// <summary>
        /// O tipo de log
        /// </summary>
        public string U_LogType { get; set; }
        /// <summary>
        /// O texto descrevendo o que está acontecendo
        /// </summary>
        public string U_Text { get; set; }
        /// <summary>
        /// A data da ação
        /// </summary>
        public  DateTime U_Date { get; set; }
        /// <summary>
        /// A hora da ação
        /// </summary>
        public string U_Time { get; set; }
        /// <summary>
        /// O nome da Empresa que está usando o addon
        /// </summary>
        public string U_CompanyVersion { get; set; }
        /// <summary>
        /// A versão do addon
        /// </summary>
        public string U_AddonVersion { get; set; }
        /// <summary>
        /// A versão da DLL
        /// </summary>
        public string U_LibVersion { get; set; }
        /// <summary>
        /// O usuário logado no sistema
        /// </summary>
        public string U_User { get; set; }
    }

}
