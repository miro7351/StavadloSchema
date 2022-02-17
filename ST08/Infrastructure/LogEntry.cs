using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Stavadlo22.Infrastructure
{
    /// <summary>
    /// Trieda reprezentujuca nacitany logovaci zaznam v DB - nepotrebne stlpce nie su zaradene
    /// </summary>
    public class LogEngry
    {
        public DateTime TimeStamp { get; set; }
        public string LogType { get; set; }

        //do DB sa zapisuje (message + "#INFO#" + DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff"));
        /// <summary>
        /// cast zakodovanej spravy z Databazy ak kodovana sprava je dhlsia ako 4000 bytov
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// cast zakodovanej spravy z Databazy ak kodovana sprava je dhlsia ako 4000 bytov
        /// </summary>
        public string Message2 { get; set; }

        /// <summary>
        /// dekodovana sprava 'message' z Databazy 
        /// </summary>
        public string CombinedMessage { get; set; }
        public string ClientName { get; set; }


        /// <summary>
        /// Zapis dekodovaneho stringu do property CombinedMessage
        /// </summary>
        public void DecodeLogMessage()
        {
            string messageFromDB = Message + Message2;//Message a Message2 su zipovane stringy nacitane z DB
            string fullDecodedMmessage = InflateAndEncodeBase64(messageFromDB);
            //do DB sa zapisuje (message + "#INFO#" + DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff"));
            string[] splitedMessage = fullDecodedMmessage.Split('#');
            if (splitedMessage.Length >= 3)
                CombinedMessage = splitedMessage[0];
            else
                CombinedMessage = string.Empty;
        }


        /// <summary>
        /// Dekompresuje string nacitany z DB s pouzitim Deflate algoritmu
        /// </summary>
        /// <param name="compressedData"></param>
        /// <returns>dekompresovany string vo formate Encoding.UTF8</returns>
        string InflateAndEncodeBase64(string compressedData)//MH: 10.05.2019 upravena verzia
        {
            if (string.IsNullOrEmpty(compressedData)) return null;
            string decompressedStringBase64 = string.Empty;
            byte[] decompressedBytes;


            byte[] rawData = Convert.FromBase64String(compressedData);
            int decompressedByteLengt = 0;
            using (MemoryStream ms1 = new MemoryStream())
            {
                ms1.Seek(0, SeekOrigin.Begin);
                ms1.Write(rawData, 0, rawData.Length);
                ms1.Flush();
                ms1.Seek(0, SeekOrigin.Begin);

                using (DeflateStream deflateStream = new DeflateStream(ms1, CompressionMode.Decompress))//deflateStream bude decompresovat ms1
                {
                    using (MemoryStream ms2 = new MemoryStream())
                    {
                        ms2.Seek(0, SeekOrigin.Begin);
                        deflateStream.CopyTo(ms2);//ms2 je uz decompresovany
                        ms2.Flush();
                        ms2.Seek(0, SeekOrigin.Begin);
                        deflateStream.Flush();
                        decompressedByteLengt = (int)ms2.Length;
                        //nacitanie z memory streamu ms2 do pola bytov decompressedBytes
                        decompressedBytes = new byte[decompressedByteLengt];
                        int l2 = ms2.Read(decompressedBytes, 0, decompressedByteLengt);
                        decompressedStringBase64 = System.Text.Encoding.UTF8.GetString(decompressedBytes);
                    }
                }
            }
            //return System.Text.Encoding.UTF8.GetString(decompressedBytes);// decompressedStringBase64;
            return  decompressedStringBase64;
        }
    }
}
