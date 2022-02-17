using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Stavadlo22.Infrastructure.PlayMode
{
    /// <summary>
    /// funkcie pre serializaciu/deserializaciu structur
    /// </summary>
    public class SerDesManager
    {
        /// <summary>
        /// Serializuje T do MemoryStreamu a ten konvertuje na string pomocou Convert.ToBase64String(ms.ToArray())
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string SerializeToBase64<T>(T item) where T : struct
        {
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, item);
                ms.Position = 0;
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        /// <summary>
        /// Deserializuje string na objekt typu T; pouziva Convert.FromBase64String
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataString"></param>
        /// <returns></returns>
        public static T DeserializeFromBase64<T>(string dataString)
        {
            //13.05.2014 pre subor z log servera pre ST17
            //exception:{"The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters. "}
            byte[] byteArray = Convert.FromBase64String(dataString);   //Encoding.Unicode.GetBytes(dataString.ToArray());
            using (var ms = new MemoryStream(byteArray))
            {
                ms.Position = 0;
                IFormatter formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(ms);
            }
        }

        /// <summary>
        /// Deserializuje string na objekt typu T; pouziva Convert.FromBase64String
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataString"></param>
        /// <returns></returns>
        //public static T DeserializeFromBase64_2<T>(string dataString)
        //{
        //    //13.05.2014 pre subor z log servera pre ST17
        //    //exception:{"The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters. "}
        //    // byte[] byteArray = Convert.FromBase64String(dataString);   //Encoding.Unicode.GetBytes(dataString.ToArray());
        //    char[] byteArray = dataString.ToCharArray();

        //    using (var ms = new MemoryStream(byteArray.Length) )
        //    {
        //        ms.Position = 0;
        //        IFormatter formatter = new BinaryFormatter();
        //        return (T)formatter.Deserialize(ms);
        //    }
        //}
    }
}
