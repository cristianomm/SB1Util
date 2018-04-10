using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using SB1Util.Serializer;
using SB1Util.ItsControlException;

namespace SB1Util.ItsControlException
{
    /// <summary>
    /// Classe que grava em arquivi as exceções do sistema
    /// </summary>
    public class ItsControlException
    {
        /// <summary>
        /// Serializa e salva a exceção gerada em arquivo XML dentro do diretório LogSystem, na raiz do AddOn
        /// </summary>
        /// <param name="Ex">A exceção disparada</param>   
        public static void Save(Exception Ex)
        {
            try
            {
                B1Exception B1ExceptionObj = new B1Exception(Ex);
                //Verificando se o diretório de log existe
                string path = string.Format(@"{0}LogSystem", AppDomain.CurrentDomain.BaseDirectory);
                string XmlDocumentName = string.Format("log_application_exception_{0}.xml", DateTime.Now.ToString("dd_MM_yyyy__hh_MM_ss"));
                Serializer<B1Exception> SerializerObj = new Serializer<B1Exception>();

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                XmlDocument XDoc = new XmlDocument();

                XDoc.InnerXml = SerializerObj.SerializeObject(B1ExceptionObj);
                XDoc.Save(string.Format(@"{0}\{1}", path, XmlDocumentName));
            }
            catch (Exception e)
            {
            }
        }  

    }
}
