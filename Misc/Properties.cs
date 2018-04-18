using System;
using System.Collections.Generic;
using System.IO;
using SAPbobsCOM;
using SB1Util.Log;

namespace SB1Util.Misc
{
    public class Properties
    {

        private static string KEY = "";
        private static string VALUE = "";
        private static string SEPARATOR = "[=|:]";
        private static string COMMENT = "#";
        private static string PROPERTY = KEY + SEPARATOR + VALUE;
        
        private Dictionary<string, string> properties;

        private static Properties instance;
        private Logger logger;

        
        private Properties()
        {
            this.logger = Logger.getInstance();
            this.properties = new Dictionary<string, string>();
            loadFromDB();
        }


        public static Properties getInstance()
        {
            if (instance == null)
            {
                instance = new Properties();
            }
            return instance;
        }




        //carrega o arquivo properties
        public void load(string fileName)
        {
            //para cada linha...
            foreach (string line in File.ReadAllLines(fileName))
            {
                //verifica se eh comentario ou prop
                //se for comentario, vai pra proxima linha
                //se for prop, adiciona no dicionario
                if(! line.StartsWith("#")){
                    string[] entry = line.Split('=');
                    properties.Add(entry[0], entry[1]);
                }

            }

        }


        public void loadFromDB()
        {
            try
            {
                Recordset rs = DB.DBFacade.getInstance().Query("SELECT * FROM [@SB1_ADDON_CONFIG]");
                properties = new Dictionary<string, string>();
                while (!rs.EoF)
                {
                    string name = (string)rs.Fields.Item("Name").Value;
                    string value = (string)rs.Fields.Item("U_Value").Value;
                    properties.Add(name, value);
                    rs.MoveNext();
                }
            }catch(Exception e)
            {
                logger.log("Erro ao carregar configurações: " + e.Message, Logger.LogType.ERROR, e);
            }
        }


        public Dictionary<string, string> PropertiesList
        {
            get { return properties; }
        }

        public string getByKey(string key)
        {
            string val="";

            try
            {
                properties.TryGetValue(key, out val);
            }
            catch (Exception e)
            {
                val = "";
                logger.log("Erro ao recuperar Chave de configuração.", Logger.LogType.WARNING, e, false);
            }

            if (val.Equals(""))
            {
                logger.log("Chave " + key + " nao encontrada na lista de configuracoes.", Logger.LogType.WARNING, null, true);
            }

            return val;
        }


        public void put(string key, string value)
        {
            try
            {
                properties.Add(key, value);

            }
            catch (Exception e)
            {
            }

        }


        public bool exists(string key)
        {
            bool ret = false;
            try
            {
                ret = properties.ContainsKey(key);
            }
            catch (Exception e)
            {
                ret = false;
            }
            return ret;
        }


    }
}
