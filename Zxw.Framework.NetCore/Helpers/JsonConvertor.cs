using Jil;
using System;

namespace Zxw.Framework.NetCore.Helpers
{
    /// <summary>
    /// json serialization and deserialization, using Jil.
    /// </summary>
    public class JsonConvertor
    {
        public static string Serialize(object source, Jil.Options options = null)
        {
            return JSON.Serialize(source, options);
        }
        public static string Serialize<T>(T source, Jil.Options options = null)
        {
            return JSON.Serialize(source, options);
        }

        public static T Deserialize<T>(string source, Jil.Options options = null)
        {
            return JSON.Deserialize<T>(source, options);
        }

        public static object Deserialize(string source, Type destinationType, Jil.Options options = null)
        {
            return JSON.Deserialize(source, destinationType, options);                
        }

        public static dynamic Deserialize(string source, Jil.Options options = null)
        {
            return JSON.DeserializeDynamic(source, options);
        }
    }
}
