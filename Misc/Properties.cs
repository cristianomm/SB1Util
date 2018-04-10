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
        
        private LinkedList<Property> properties;

        private static Properties instance;
        private Logger logger;

        public struct Property
        {
            public string name;
            public string value;
        };

        private Properties()
        {
            this.logger = Logger.getInstance();
            this.properties = new LinkedList<Property>();
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
                    Property p;;
                    string[] entry = line.Split('=');
                    p.name = entry[0];
                    p.value = entry[1];
                    properties.AddLast(p);
                }

            }

        }


        public void loadFromDB()
        {
            try
            {
                Recordset rs = DB.DBFacade.getInstance().Query("SELECT * FROM [@ITS_ADDON_CONFIG]");
                properties = new LinkedList<Property>();
                while (!rs.EoF)
                {
                    Property p;
                    p.name = (string)rs.Fields.Item("Name").Value;
                    p.value = (string)rs.Fields.Item("U_Value").Value;
                    properties.AddLast(p);
                    rs.MoveNext();
                }
            }catch(Exception e)
            {
                logger.log("Erro ao carregar configurações: " + e.Message, Logger.LogType.ERROR, e);
            }
        }


        public LinkedList<Property> PropertiesList
        {
            get { return properties; }
        }

        public string getByKey(string key)
        {
            string val="";

            try
            {
                foreach (Property p in properties)
                {
                    if (p.name.Equals(key))
                    {
                        val = p.value;
                        break;
                    }
                }

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
                Property aux;
                aux.name = "";
                aux.value = "";
                foreach (Property p in properties)
                {
                    if (p.name.Equals(key))
                    {
                        aux = p;
                        break;
                    }
                }

                LinkedListNode<Property> n = new LinkedListNode<Property>(aux);
                properties.Remove(n);
                aux.name = key;
                aux.value = value;                
                properties.AddLast(new LinkedListNode<Property>(aux));

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
                foreach (Property p in properties)
                {
                    if (p.name.Equals(key))
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


    }
}
