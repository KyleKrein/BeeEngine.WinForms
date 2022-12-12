using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace BeeEngine;

public static class CloningHelper
{
    public static T CreateDeepCopy<T>(this T obj, CloneType cloneType = CloneType.Xml)
    {
        using (var ms = new MemoryStream())
        {
            if (cloneType == CloneType.Binary)
            {
                var formatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011
                formatter.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(ms);
#pragma warning restore SYSLIB0011
            }

            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            serializer.Serialize(ms, obj);
            ms.Seek(0, SeekOrigin.Begin);
            return (T)serializer.Deserialize(ms);
        }
    }
}

public enum CloneType
{
    Binary,
    Xml
}