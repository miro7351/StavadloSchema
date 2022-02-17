
using System;
using System.Xml;
using System.Net;
using System.Text;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading;
using System.IO.Compression;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Stavadlo22.Data;

namespace Stavadlo22.Infrastructure
{

    /// <summary>
    /// Zapisuje/Cita udaje do/z logovacej databazy.
    /// Pre zapis sa pouziva Thread _dbLoggingThread.
    /// Pri skonceni aplikacie treba _dbLoggingThread ukoncit!!! pozri App.xaml.cs: OnClose(...)
    /// </summary>
    public class DBLogger
    {
        static StavadloGlobalData globalData;
        private ManualResetEvent _wasDataInserted = new ManualResetEvent(false);
        private readonly object _queueLocker = new object();
        private Queue<string> _messageQueue = new Queue<string>();
        private Thread _dbLoggingThread;

        bool logEnabled { get; set; }

        public void EnqueLogData(string message)
        {
            if (logEnabled)
            {
                if (message.Contains('<'))  // logovanie iba relevantnych dat
                {
                    lock (_queueLocker)
                    {
                        _messageQueue.Enqueue(message);
                    }
                }
            }
        }

        public void AbortThread()  //pozri App.xaml.cs: OnClose(...)
        {
            if (_dbLoggingThread != null && _dbLoggingThread.IsAlive)
            {
                _dbLoggingThread.Abort();
            }
        }


        public DBLogger()
        {
            globalData = StavadloGlobalData.Instance;
            logEnabled = true;

            //pri skonceni aplikacie treba _dbLoggingThread ukoncit!!! pozri App.xaml.csL: OnClose(...)
            _dbLoggingThread = new Thread(new ThreadStart(Work));
            _dbLoggingThread.IsBackground = true;
            _dbLoggingThread.Start();
        }

        public DBLogger(bool logEnabled)
        {
            globalData = StavadloGlobalData.Instance;
            this.logEnabled = logEnabled;
            if (logEnabled)
            {
                //pri skonceni aplikacie treba _dbLoggingThread ukoncit!!! pozri App.xaml.csL: OnClose(...)
                _dbLoggingThread = new Thread(new ThreadStart(Work));
                _dbLoggingThread.IsBackground = true;
                _dbLoggingThread.Start();
            }
        }

        /// <summary>
        /// nacita message z fronty _messageQueue a zapise na server
        /// </summary>
        public void Work()
        {
            while (true)
            {
                int messageCount = 0;
                lock (_queueLocker)
                {
                    messageCount = _messageQueue.Count;
                }

                string currentMessage = null;
                if (messageCount > 0)
                {
                    lock (_queueLocker)
                    {
                        currentMessage = _messageQueue.Dequeue();
                    }
                }

                if (currentMessage != null)
                {
                    string retValue = SEND_HMI_EVENT(currentMessage);
                }

                Thread.Sleep(150);
            }
        }

        /// <summary>
        /// nacitanie udajov z databazy
        /// </summary>
        /// <param name="dateTimeFrom"></param>
        /// <param name="dateTimeTo"></param>
        /// <param name="clienRole">ST22D-dispecer,ST22A-admin, ST22U-udrzba</param>
        /// <returns></returns>
        public static ObservableCollection<LogEngry> F_GET_HMI_EVENTS(DateTime dateTimeFrom, DateTime dateTimeTo, string clientRole)//string clientName
        {
            ObservableCollection<LogEngry> readedData = new ObservableCollection<LogEngry>();

            String retStr = "";
            int int_return = 0;
            int int_fault = 0;
            int int_ora_err = 0;
            string outStr = string.Empty;

            try
            {
               
                string axlrequest = "F_GET_HMI_EVENTS";  // soap action
                string axlparameters = "";

                axlparameters = "<P_DATE_TO-VARCHAR2-IN>" + dateTimeTo.ToString("yyyy-MM-dd HH:mm:ss.fff") + "</P_DATE_TO-VARCHAR2-IN>";
                axlparameters += "<P_DATE_FROM-VARCHAR2-IN>" + dateTimeFrom.ToString("yyyy-MM-dd HH:mm:ss.fff") + "</P_DATE_FROM-VARCHAR2-IN>";
                axlparameters += "<P_CLIENT-VARCHAR2-IN>" + clientRole + "</P_CLIENT-VARCHAR2-IN>";
                //clientRole: ST22D - dispecer; ST22U-udrzba; ST22A-admin;

                //HttpWebRequest req = (HttpWebRequest)WebRequest.Create(App.WebServiceAddress);
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(globalData.WebServiceAddress);
                req.Credentials = new NetworkCredential(globalData.WebServiceLogin, globalData.WebServicePassword, ""); //This line ensures the request is processed through Basic Authentication
                req.KeepAlive = false;
                req.ContentType = "text/xml; charset=utf-8";
                req.Method = "POST";
                req.Accept = "text/xml";
                req.UserAgent = "Apache-HttpClient/4.1.1 (java 1.5)"; //"Mozilla/4.0 (compatible; MSIE 6.0; MS Web Services Client Protocol 2.0.50727.5456)";
                req.Headers.Add(String.Format("SOAPAction: \"{0}\"", axlrequest));
                Stream myStream = req.GetRequestStream();

                string soaprequest;
                soaprequest = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n";
                soaprequest += "<soap:Envelope  xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\n";
                soaprequest += "<soap:Body>\n";
                soaprequest += "<CXMLTYPE-F_GET_HMI_EVENTSInput>\n";
                soaprequest += axlparameters + "";
                soaprequest += "</CXMLTYPE-F_GET_HMI_EVENTSInput>\n";
                soaprequest += "</soap:Body>\n";
                soaprequest += "</soap:Envelope>";

                //s.Write(System.Text.Encoding.ASCII.GetBytes(soaprequest), 0, soaprequest.Length);
                myStream.Write(System.Text.Encoding.UTF8.GetBytes(soaprequest), 0, soaprequest.Length);
                myStream.Close();

                WebResponse resp = req.GetResponse();
                StreamReader sr = new StreamReader(resp.GetResponseStream());

                XmlReader readerMsg = XmlReader.Create(sr, null);
                bool isHMIEventStarted = false;
                bool isDateStarted = false;
                bool IsMessageStarted = false;
                bool IsClientNameStarted = false;
                bool IsMessage2Started = false;
                bool IsLogLevelStarted = false;
                string customDateFormat = "YY-MMM-dd hh.mm.ss.fff";
                LogEngry tmpLogEntry = null;

                StringBuilder outputXML = new StringBuilder("");

                while (readerMsg.Read())
                {
                    //outputXML.Append(readerMsg.ReadInnerXml());
                    //Console.WriteLine("NodeType = "+readerMsg.NodeType);
                    switch (readerMsg.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (readerMsg.Name == "RETURN")
                                int_return = 1;
                            if ((readerMsg.Name == "faultcode") || (readerMsg.Name == "faultstring"))
                                int_fault = 1;
                            if ((readerMsg.Name == "ErrorNumber") || (readerMsg.Name == "Message"))
                                int_ora_err = 1;
                            if (readerMsg.Name == "HMI_EVENTS")
                            {
                                isHMIEventStarted = true;
                                tmpLogEntry = new LogEngry();
                            }
                            if (readerMsg.Name == "DATETIME")
                                isDateStarted = true;
                            if (readerMsg.Name == "MESSAGE")
                                IsMessageStarted = true;
                            if (readerMsg.Name == "MESSAGE2")
                                IsMessage2Started = true;
                            if (readerMsg.Name == "LOG_LEVEL")
                                IsLogLevelStarted = true;
                            if (readerMsg.Name == "CLIENT_NAME")
                                IsClientNameStarted = true;
                            break;
                        case XmlNodeType.Text: //Display the text in each element.
                            if (int_return == 1)
                            {
                                if (isHMIEventStarted)
                                {
                                    if (isDateStarted)
                                    {
                                        //DateTime tmpDateTime = DateTime.ParseExact(readerMsg.Value, "dd-MMM-yy hh.mm.ss.fff tt", CultureInfo.InvariantCulture);
                                        DateTime tmpDateTime = DateTime.ParseExact(readerMsg.Value, "yyyy.MM.dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                                        tmpLogEntry.TimeStamp = tmpDateTime;
                                    }

                                    if (IsMessageStarted)
                                    {
                                        string tmpMessage = readerMsg.Value;
                                        tmpLogEntry.Message = tmpMessage;
                                    }

                                    if (IsClientNameStarted)
                                    {
                                        string tmpClientName = readerMsg.Value;
                                        tmpLogEntry.ClientName = tmpClientName;
                                    }

                                    if (IsMessage2Started)
                                    {
                                        string tmpMessage2 = readerMsg.Value;
                                        tmpLogEntry.Message2 = tmpMessage2;
                                    }

                                    if (IsLogLevelStarted)
                                    {
                                        string tmpLogLevel = readerMsg.Value;
                                        tmpLogEntry.LogType = tmpLogLevel;
                                    }
                                }

                                //retStr = retStr + "(OK) Id:" + readerMsg.Value;
                            }
                            if ((int_fault == 1) || (int_ora_err == 1))
                                retStr = retStr + "," + readerMsg.Value;


                            break;
                        case XmlNodeType.EndElement: //Display the end of the element.
                            if (int_return == 1)
                                if (readerMsg.Name == "RETURN")
                                    int_return = 0;
                            if (int_fault == 1)
                                if ((readerMsg.Name == "faultcode") || (readerMsg.Name == "faultstring"))
                                    int_fault = 0;
                            if (int_ora_err == 1)
                                if ((readerMsg.Name == "ErrorNumber") || (readerMsg.Name == "Message"))
                                    int_ora_err = 0;
                            if (readerMsg.Name == "HMI_EVENTS")
                            {
                                isHMIEventStarted = false;
                                readedData.Add(tmpLogEntry);
                            }
                            if (readerMsg.Name == "DATETIME")
                                isDateStarted = false;
                            if (readerMsg.Name == "MESSAGE")
                                IsMessageStarted = false;
                            if (readerMsg.Name == "CLIENT_NAME")
                                IsClientNameStarted = false;
                            if (readerMsg.Name == "MESSAGE2")
                                IsMessage2Started = false;
                            if (readerMsg.Name == "LOG_LEVEL")
                                IsLogLevelStarted = false;
                            break;
                        case XmlNodeType.CDATA:
                            if ((int_fault == 1) || (int_ora_err == 1))
                                retStr = retStr + "," + readerMsg.Value;
                            break;
                    }
                }

                outStr = outputXML.ToString();
            }
            catch (WebException wex1)
            {
                retStr = "0-" + wex1.Message;
            }
            catch (ApplicationException ex1)
            {
                retStr = "0-" + ex1.Message;
            }
            catch (Exception ex2)
            {
                retStr = "0-" + ex2.Message;
            }


            if (!string.IsNullOrEmpty(outStr))
                MessageBox.Show(outStr);

            //log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType).Info(string.Format("{0} Messages: '{1}'  ==>  '{2}'", retStr, msg1, msg2));

            return readedData;
        }//F_GET_HMI_EVENTS

        // Pokus o skratenie ukladaneho retazca ... ale kryptovanim sa dlzka zvacsi.
        ////initialze the byte arrays to the public key information.
        //byte[] PublicKey = {214,46,220,83,160,73,40,39,201,155,19,202,3,11,191,178,56,
        //                       74,90,36,248,103,18,144,170,163,145,87,54,61,34,220,222,
        //                        207,137,149,173,14,92,120,206,222,158,28,40,24,30,16,175,
        //                        108,128,35,230,118,40,121,113,125,216,130,11,24,90,48,194,
        //                        240,105,44,76,34,57,249,228,125,80,38,9,136,29,117,207,139,
        //                        168,181,85,137,126,10,126,242,120,247,121,8,100,12,201,171,
        //                        38,226,193,180,190,117,177,87,143,242,213,11,44,180,113,93,
        //                        106,99,179,68,175,211,164,116,64,148,226,254,172,147};
        ////byte[] PublicKey = {214,46,220,83,160,73,40,39,201,155,19,202,3};


        //byte[] Exponent = {1,0,1};	
        ////Values to store encrypted symmetric keys.	
        //byte[] EncryptedSymmetricKey;
        //byte[] EncryptedSymmetricIV;

      
        /*
         * DeflateStream Provides methods and properties for compressing and decompressing streams using
         *               the Deflate algorithm. 
         */

        /// <summary>
        /// Pole bytov skompresuje s pouzitim Deflate algorithm
        /// </summary>
        /// <param name="data"></param>
        /// <returns>compressed string</returns>
        public static string DeflateAndEncodeBase64(byte[] data)
        {
            if (null == data || data.Length < 1) return null;
            string compressedBase64 = "";

            //write data into a new memory stream wrapped by a deflate stream
            using (MemoryStream ms = new MemoryStream())
            {
                using (DeflateStream deflateStream = new DeflateStream(ms, CompressionMode.Compress, true))
                {
                    //write byte buffer into memorystream
                    deflateStream.Write(data, 0, data.Length);
                    deflateStream.Close();

                    //rewind memory stream and write to base 64 string
                    byte[] compressedBytes = new byte[ms.Length];
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Read(compressedBytes, 0, (int)ms.Length);
                    compressedBase64 = Convert.ToBase64String(compressedBytes);
                }
            }
            return compressedBase64;
        }

        //=====================================================================

        // Log4NetExtensions:   
        /*
         * CustomInfo(this ILog log, string message)
         * {
         *      ...
         *      GlobalData.WebServiceLogger.EnqueLogData(message + "#INFO#" + DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff"));
         * }
         */
        /// <summary>
        /// zapis zaznamu do databazy
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public string SEND_HMI_EVENT(string message)
        {
            string[] parameters = message.Split('#');
            if (parameters.Length != 3)
                return null;

            string msg1 = null;
            string msg2 = null;

            string clientName = "ST22";
            switch (globalData.CurrentUser.Role)
            {
                case Enums.USER_ROLE.ADMIN:
                    clientName = Enums.User_DBlogger.ST22A.ToString();
                    break;
                case Enums.USER_ROLE.DISPECER:
                    clientName = Enums.User_DBlogger.ST22D.ToString();
                    break;

                case Enums.USER_ROLE.UDRZBA:
                    clientName = Enums.User_DBlogger.ST22U.ToString();
                    break;
                default:
                    break;
            }

           
            //RSAParameters rsaKeyInfo = new RSAParameters { Exponent = new byte[] {1, 0, 1} };
            //
            //string msgConverted = null;
            ////Create a new instance of RSACryptoServiceProvider.
            //using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            //{
            //    //Create a new instance of RSAParameters.
            //    RSAParameters RSAKeyInfo = new RSAParameters();

            //    //Set RSAKeyInfo to the public key values. 
            //    RSAKeyInfo.Modulus = PublicKey;
            //    RSAKeyInfo.Exponent = Exponent;

            //    //Import key parameters into RSA.
            //    RSA.ImportParameters(RSAKeyInfo);

            //    //Create a new instance of the RijndaelManaged class.
            //    RijndaelManaged RM = new RijndaelManaged();

            //    //Encrypt the symmetric key and IV.
            //    //EncryptedSymmetricKey = RSA.Encrypt(RM.Key, false);
            //    //EncryptedSymmetricIV = RSA.Encrypt(RM.IV, false);

            //    byte[] encrypted = RSA.Encrypt(System.Text.Encoding.UTF8.GetBytes(message), true);
            //    msgConverted = System.Convert.ToBase64String(encrypted);
            //    //string decrypted = System.Text.Encoding.UTF8.GetString(Provider.Decrypt(encrypted, true));
            //}

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(message);
            //string msgConverted = System.Convert.ToBase64String(plainTextBytes);
            string zippedMessage = DeflateAndEncodeBase64(plainTextBytes);
         

            if (zippedMessage.Length > 8000)//zippedMessage.Length > msgConverted.Length && 
            {
                globalData.Log.Info($"Deflated message length contains more then 8000 characters! {zippedMessage.Length}");
            }

          
            if (zippedMessage.Length > 4000)
            {
                msg1 = zippedMessage.Substring(0, 4000);
                msg2 = zippedMessage.Substring(4000, parameters[0].Length - 4000);
            }
            else
            {
                msg1 = zippedMessage;
                msg2 = string.Empty;
            }


            String retStr = "";
            int int_return = 0;
            int int_fault = 0;
            int int_ora_err = 0;
            try
            {
                //parameters[2] datum a cas vo vstupnom parametri message
                DateTime logDate = DateTime.ParseExact(parameters[2], "yyyy.MM.dd HH:mm:ss.fff", null);
                /*      
                      <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:iss="http://xmlns.oracle.com/orawsv/ISS/ISS_HMI_EVENT">
                     <soapenv:Header/>
                     <soapenv:Body>
                        <iss:SVARCHAR2-F_SET_HMI_EVENTInput>
                           <iss:P_THREAD-VARCHAR2-IN>THREAD</iss:P_THREAD-VARCHAR2-IN>
                           <iss:P_MESSAGE2-VARCHAR2-IN>MSG2</iss:P_MESSAGE2-VARCHAR2-IN>
                           <iss:P_MESSAGE-VARCHAR2-IN>MSG</iss:P_MESSAGE-VARCHAR2-IN>
                           <iss:P_LOG_LEVEL-VARCHAR2-IN>LOGLEVEL</iss:P_LOG_LEVEL-VARCHAR2-IN>
                           <iss:P_LOGGER-VARCHAR2-IN>LOGGER</iss:P_LOGGER-VARCHAR2-IN>
                           <iss:P_DATETIME-VARCHAR2-IN>2016-01-12 10:29:00.971</iss:P_DATETIME-VARCHAR2-IN>
                           <iss:P_CLIENT_NAME-VARCHAR2-IN>CLN_NAM</iss:P_CLIENT_NAME-VARCHAR2-IN>
                        </iss:SVARCHAR2-F_SET_HMI_EVENTInput>
                     </soapenv:Body>
                  </soapenv:Envelope>
                      */

                string axlrequest = "F_SET_HMI_EVENT";
                string axlparameters = "";

                //"thread", "message1", "message2", "INFO", "Logger", DateTime.Now.ToString(), "ST9R"
                //var plainTextBytes1 = System.Text.Encoding.UTF8.GetBytes(msg1);
                //msg1Converted = System.Convert.ToBase64String(plainTextBytes1);
                //var plainTextBytes2 = System.Text.Encoding.UTF8.GetBytes(msg2);
                //msg2Converted = System.Convert.ToBase64String(plainTextBytes2);

                axlparameters = "<P_THREAD-VARCHAR2-IN>THREAD</P_THREAD-VARCHAR2-IN>";
                axlparameters = string.Concat(axlparameters, "<P_MESSAGE2-VARCHAR2-IN>", msg1, "</P_MESSAGE2-VARCHAR2-IN>");
                axlparameters = string.Concat(axlparameters, "<P_MESSAGE-VARCHAR2-IN>", msg2, "</P_MESSAGE-VARCHAR2-IN>");

                //parameters[1]="INFO" vo vstupnom parametri message
                axlparameters += "<P_LOG_LEVEL-VARCHAR2-IN>" + parameters[1] + "</P_LOG_LEVEL-VARCHAR2-IN>" +
                     "<P_LOGGER-VARCHAR2-IN>" + "" + "</P_LOGGER-VARCHAR2-IN>" +
                     "<P_DATETIME-VARCHAR2-IN>" + logDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + "</P_DATETIME-VARCHAR2-IN>" +
                      "<P_CLIENT_NAME-VARCHAR2-IN>" + clientName + "</P_CLIENT_NAME-VARCHAR2-IN>";

                //HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://drls-t.dbs.usske.sk:8181/orawsv/ISS/ISS_HMI_EVENT");
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(globalData.WebServiceAddress);
                //req.Credentials = new NetworkCredential("iss_hmi_entry", "abc123", ""); //This line ensures the request is processed through Basic Authentication
                req.Credentials = new NetworkCredential(globalData.WebServiceLogin, globalData.WebServicePassword, ""); //This line ensures the request is processed through Basic Authentication

                req.KeepAlive = false;
                req.ContentType = "text/xml; charset=utf-8";
                req.Method = "POST";
                req.Accept = "text/xml";
                req.UserAgent = "Apache-HttpClient/4.1.1 (java 1.5)"; //"Mozilla/4.0 (compatible; MSIE 6.0; MS Web Services Client Protocol 2.0.50727.5456)";
                req.Headers.Add(String.Format("SOAPAction: \"{0}\"", axlrequest));
                Stream myStream = req.GetRequestStream();

                string soaprequest;
                soaprequest = "<?xml version=\"1.0\" encoding=\"utf-8\"?><soap:Envelope  xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">";
                soaprequest += "<soap:Body>";
                soaprequest += "<SVARCHAR2-F_SET_HMI_EVENTInput>\n";
                soaprequest += axlparameters + "";
                soaprequest += "</SVARCHAR2-F_SET_HMI_EVENTInput>";
                soaprequest += "</soap:Body>";
                soaprequest += "</soap:Envelope>";

                //s.Write(System.Text.Encoding.ASCII.GetBytes(soaprequest), 0, soaprequest.Length);
                myStream.Write(System.Text.Encoding.UTF8.GetBytes(soaprequest), 0, soaprequest.Length);
                myStream.Close();

                WebResponse resp = req.GetResponse();
                StreamReader sr = new StreamReader(resp.GetResponseStream());

                XmlReader readerMsg = XmlReader.Create(sr, null);
                while (readerMsg.Read())
                {
                    //Console.WriteLine("NodeType = "+readerMsg.NodeType);
                    switch (readerMsg.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (readerMsg.Name == "RETURN")
                                int_return = 1;
                            if ((readerMsg.Name == "faultcode") || (readerMsg.Name == "faultstring"))
                                int_fault = 1;
                            if ((readerMsg.Name == "ErrorNumber") || (readerMsg.Name == "Message"))
                                int_ora_err = 1;
                            break;
                        case XmlNodeType.Text: //Display the text in each element.
                            if (int_return == 1)
                            {
                                retStr = retStr + "(OK) Id:" + readerMsg.Value;
                            }
                            if ((int_fault == 1) || (int_ora_err == 1))
                                retStr = retStr + "," + readerMsg.Value;
                            break;
                        case XmlNodeType.EndElement: //Display the end of the element.
                            if (int_return == 1)
                                if (readerMsg.Name == "RETURN")
                                    int_return = 0;
                            if (int_fault == 1)
                                if ((readerMsg.Name == "faultcode") || (readerMsg.Name == "faultstring"))
                                    int_fault = 0;
                            if (int_ora_err == 1)
                                if ((readerMsg.Name == "ErrorNumber") || (readerMsg.Name == "Message"))
                                    int_ora_err = 0;
                            break;
                        case XmlNodeType.CDATA:
                            if ((int_fault == 1) || (int_ora_err == 1))
                                retStr = retStr + "," + readerMsg.Value;
                            break;
                    }
                }
            }
            catch (WebException wex1)
            {
                retStr = "0-" + wex1.Message;
            }
            catch (ApplicationException ex1)
            {
                retStr = "0-" + ex1.Message;
            }
            catch (Exception ex2)
            {
                retStr = "0-" + ex2.Message;
            }

            //log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType).Info(string.Format("{0} Messages: '{1}'  ==>  '{2}'", retStr, msg1, msg2));
            globalData.Log.Info($"client={clientName} {retStr} Messages: '{msg1}'  ==>  '{msg2}'");
            return retStr;
        }//SEND_HMI_EVENT

        //=====================================================================
    }
}
