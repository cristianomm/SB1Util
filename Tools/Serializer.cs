using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Xml;
using System.Text;

namespace SB1Util.Serializer
{
    public class Serializer<T>
    {
        public string SerializeObject(T object2serialize)
        {
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(object2serialize.GetType());
            MemoryStream ms = new MemoryStream();
            x.Serialize(ms, object2serialize);
            ms.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(ms);
            string text = reader.ReadToEnd();
            return text;
        }
        public T DeserializeObject(string docSerialized)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(docSerialized);
            MemoryStream mstream = new MemoryStream(bytes);
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(T));
            return (T)x.Deserialize(mstream);
        }
    }
}