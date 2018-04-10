using System;
using System.Collections;
using System.Collections.Generic;
using SB1Util.Log;
using SB1Util.Misc;
using FtpLib;
using System.IO;

namespace SB1Util.Misc
{
    public class FTP
    {

        public struct FtpFile
        {
            public string fileName;
            public DateTime creationTime;
            public string extension;
        }

        string downPath;
        string host;
        int port;
        private FtpConnection sFTP;
        private Logger logger;


        public FTP(string host, int port)
        {
            this.host = host;
            this.port = port;
            this.logger = Logger.getInstance();
        }


        public string DownPath { get { return downPath; } }
        public string Host { get { return host; } }
        public int Port { get { return port; } }


        public bool connect(string user, string pass)
        {
            bool ret = false;
            logger.pushOperation("FTP.connect");
            try
            {
                sFTP = new FtpConnection(host, port);
                sFTP.Open();
                sFTP.Login(user, pass);
                ret = sFTP.DirectoryExists("/");
                
            }
            catch (Exception e)
            {
                ret = false;
                logger.log("Erro ao conectar FTP: " + e.Message, Logger.LogType.ERROR, e, false);
            }
            finally
            {
                logger.releaseOperation();
            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        public void quit()
        {
            logger.pushOperation("FTP.disconnect");
            try
            {
                sFTP.Close();
            }
            catch (Exception e)
            {
                logger.log("Erro ao conectar FTP: " + e.Message, Logger.LogType.ERROR, e, false);
            }
            finally
            {
                logger.releaseOperation();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public List<FtpFile> list(string remoteDir)
        {
            logger.pushOperation("FTP.list");
            List<FtpFile> ret = new List<FtpFile>();
            try
            {
                try
                {
                    sFTP.SetCurrentDirectory(remoteDir);
                }
                catch (Exception e)
                {
                    logger.log("Erro: " + e.Message, Logger.LogType.ERROR, e, false);
                }
                foreach(FtpFileInfo fi in sFTP.GetFiles() ){
                    FtpFile ftpfile;
                    ftpfile.creationTime = (DateTime)(fi.CreationTime == null? DateTime.Now: fi.CreationTime);
                    ftpfile.extension = fi.Extension;
                    ftpfile.fileName = fi.Name;
                    ret.Add(ftpfile);
                }
            }
            catch (Exception e)
            {
                ret = new List<FtpFile>();
                logger.log("Erro ao listar arquivos no FTP: " + e.Message, Logger.LogType.ERROR, e, false);
            }
            finally
            {
                logger.releaseOperation();
            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="dir"></param>
        public void upload(string fileName, string remoteFile)
        {
            logger.pushOperation("FTP.upload");
            try
            {
                string dir = remoteFile.Substring(0, remoteFile.LastIndexOf("/")+1);
                
                if (File.Exists(fileName))
                {
                    try
                    {
                        sFTP.SetCurrentDirectory(dir);
                    }
                    catch (Exception e)
                    {
                        logger.log("Erro: " +e.Message, Logger.LogType.ERROR, e, false);
                    }
                    sFTP.PutFile(fileName, remoteFile);
                }
                else
                {
                    logger.log(string.Format("Arquivo {0} inexistente.", fileName), Logger.LogType.WARNING);
                }
            }
            catch (Exception e)
            {
                logger.log("Erro ao conectar FTP: " + e.Message, Logger.LogType.ERROR, e, false);
            }
            finally
            {
                logger.releaseOperation();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="remoteFile"></param>
        /// <returns></returns>
        public string download(string fileName, string remoteFile)
        {
            logger.pushOperation("FTP.download");
            string ret = "";

            try
            {
                //caso o arquivo exista, remove o arquivo existente antes de baixar o arquivo
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                sFTP.GetFile(remoteFile, fileName, true);
                
                if (File.Exists(fileName))
                {
                    ret = fileName;
                }
            }
            catch (Exception e)
            {
                ret = "";
                logger.log("Erro ao conectar FTP: " + e.Message, Logger.LogType.ERROR, e, false);
            }
            finally
            {
                logger.releaseOperation();
            }

            return ret;

        }

        public bool removeRemotefile(string remoteFile)
        {
            bool ret = false;
            logger.pushOperation("FTP.removeRemotefile");
            try
            {
                
                sFTP.RemoveFile(remoteFile);
            }
            catch (Exception e)
            {
                ret = false;
                logger.log("Erro ao remover arquivo do FTP: " + e.Message, Logger.LogType.ERROR, e, false);
            }
            finally
            {
                logger.releaseOperation();
            }

            return ret;
        }

        public bool exists(string remoteDir, string fileName)
        {
            logger.pushOperation("FTP.exists");
            bool ret = false;
            try
            {
                List<FtpFile> files = list(remoteDir);
                foreach (FtpFile f in files)
                {
                    if (f.fileName.ToLower().Equals(fileName.ToLower()))
                    {
                        ret = true;
                        break;
                    }
                }

            }
            catch (Exception e)
            {
                ret = false;
                logger.log("Erro ao verificar existencia de arquivo no FTP: " + e.Message, Logger.LogType.ERROR, e, false);
            }
            finally
            {
                logger.releaseOperation();
            }

            return ret;
        }


    }
}
