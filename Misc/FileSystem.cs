using System;
using System.IO;
using SB1Util.Misc;
using SB1Util.Log;
using SB1Util.Connection;
using System.Security.AccessControl;

namespace SB1Util.Misc
{
    /// <summary>
    /// 
    /// </summary>
    public class FileSystem
    {

        private Logger logger;
        private Properties properties;
        private B1Connection oConnection;
        

        public FileSystem(B1Connection connection)
        {
            this.oConnection = connection;
            this.logger = Logger.getInstance();
            this.properties = Properties.getInstance();
        }

        
        public bool verifyFolder(string path)
        {
            bool retorno = false;

            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                        logger.log(
                            string.Format("Pasta criada em {0} .", path)
                            , Logger.LogType.INFO);
                    }
                    
                    retorno = true;

                    if (!isDirectoryWritable(path))
                    {
                        retorno = false;
                        logger.log(
                        string.Format("Diretório {0} não possui permissão de escrita...", path)
                        , Logger.LogType.ERROR);
                    }
                }

            }
            catch (Exception e)
            {
                retorno = false;
                logger.log(string.Format("Erro ao verificar Pasta {0}: ", path) + e.Message, Logger.LogType.ERROR, e);
            }            

            return retorno;
        }

        private static bool isDirectoryWritable(string sPath)
        {
            bool isWritable = false;

            try
            {
                AuthorizationRuleCollection auth = 
                    Directory.GetAccessControl(sPath).GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
                foreach (FileSystemAccessRule rule in auth)
                {
                    if (rule.AccessControlType == AccessControlType.Allow)
                    {
                        isWritable = true;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                isWritable = false;
            }
            catch (Exception)
            {
                isWritable = false;
            }

            return isWritable;
        }


    }
}
