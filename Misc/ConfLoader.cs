using System;
using System.Configuration;

namespace SB1Util.Misc
{
    public class ConfLoader
    {
        private Configuration config;
        private static ConfLoader instance;

        private ConfLoader(string confFile)
        {
            try
            {
                ExeConfigurationFileMap configMap = new ExeConfigurationFileMap { ExeConfigFilename = confFile };
                config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao carregar arquivo de configurações: " + e.Message, e); 
            }
        }


        public string getAppConfigValue(string key)
        {
            return config.AppSettings.Settings[key].Value;
        }

        public string getConnStringConfigValue(string key)
        {
            return config.ConnectionStrings.ConnectionStrings[key].ConnectionString;
        }




        public static ConfLoader getInstance(string confFile) 
        {
            if (instance == null)
            {
                try
                {
                    instance = new ConfLoader(confFile);
                }catch(Exception e)
                {
                    throw new Exception(e.Message, e); 
                }
            }
            return instance;
        }


    }
}
