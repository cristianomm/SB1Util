using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using SB1Util.Serializer;
using System.Diagnostics;

namespace SB1Util.Log
{
    public class LogManager
    {

        private static LogManager instance;
        private Dictionary<string, Logger> loggers;



        private LogManager()
        {
        }


        public static LogManager getInstance()
        {
            if (instance == null)
            {
                instance = new LogManager();
            }
            return instance;
        }



        public Logger getRootLogger()
        {
            Logger log = null;
            try
            {
                string logFile = AppDomain.CurrentDomain.BaseDirectory + "LogConfig.xml";
                //log4net.Config.XmlConfigurator.Configure(new System.IO.FileStream(logFile, System.IO.FileMode.Open));
                log = LogManager.getInstance().getLogger("root");

            }
            catch (Exception e)
            {
                log = null;
            }
            return log;
        }



        public Logger getLogger(string loggerName)
        {
            Logger log = null;
            try
            {


            }
            catch (ArgumentNullException e)
            {
                log = null;
            }
            return log;
        }

        /// <summary>
        /// Salva os dados de log, que são armazenados em banco de dados, em arquivo xml Se ocorrer falha de acesso ao banco de dados
        /// </summary>
        /// <param name="LoggerData">O objeto Logger Contendo as mesmas informações que vão para a tabela de log</param>k        
        public static void SaveLoggerData(LoggerData LoggerData)
        {

            string sSource;
            string sLog;
            string sEvent;

            string path = string.Format(@"{0}LogSystem", AppDomain.CurrentDomain.BaseDirectory);
            Serializer<LoggerData> SerializerObj = new Serializer<LoggerData>();
            string XmlDocumentName = string.Format("log_application_sap_{0}.xml", DateTime.Now.ToString("dd_MM_yyyy__hh_MM_ss"));

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            XmlDocument XDoc = new XmlDocument();

            XDoc.InnerXml = SerializerObj.SerializeObject(LoggerData);
            try
            {
                XDoc.Save(string.Format(@"{0}\{1}", path, XmlDocumentName));
            }
            catch (Exception Ex)
            {

                sSource = "SB1Util Exeption";
                sLog = "Ação com dados ";
                sEvent = Ex.Message;

                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);

                EventLog.WriteEntry(sSource, sEvent);
                EventLog.WriteEntry(sSource, sEvent,
                    EventLogEntryType.Warning, 234);
            }
        }
    }
}
