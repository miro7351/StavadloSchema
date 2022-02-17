using System;
using System.Net;
using System.Linq;
using System.Text;

using tcpClientClass;
using Stavadlo22.Data;
using Stavadlo22.Windows;
using Stavadlo22.Data.Telegrams;
using Stavadlo22.Infrastructure.PlayMode;

using log4net;

namespace Stavadlo22.Infrastructure.Communication
{
    #region ===Delegates===

    public delegate void InitServerConectionDelegate(object sender, EventArgs1 e);

    /// <summary>
    /// Delegate pre event ServerErrorMessage
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ServerErrorMessageDelegate(object sender, EventErrorArgs e);

    /// <summary>
    /// Delegate pre event LogicXXErrorMessage, ak sa prijme telegram cislo 161
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void LogicMessageDelegate(object sender, EventErrorArgsLogic e);

    /// <summary>
    /// Delegat pre event MessageASCIIRecieved
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    //public delegate void MessageUTF8Delegate(object sender, EventArgs1 e);

   
    /// <summary>
    /// delegat pre odpoved servera; server vrati data pre vseky prvky stavadla
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void MessageDataAllDelegate(object sender, EventArgs2 e);

    /// <summary>
    /// delegat pre neuspesnu inicializaciu connektu na server
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void InitConnectionFailedDelegate(object sender, EventArgs1 e);

    /// <summary>
    /// delegate pre spravu o logine uzivatela
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ServerLoginDelegate(object sender, UserLoginEventArgs e);

    /// <summary>
    /// delegate pre spravu Info o prvku
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    //public delegate void MessageInfoElementDelegate(object sender, EventArgsInfo e);//Zaremovane MH 25.10.2013, event je definovany nizsie

    /// <summary>
    /// delegate pre prijem spravy watchdog z LogicXX, pre telegram c.121
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void WatchdogDelegate(object sender, EventArgs e);

    /// <summary>
    /// delegate pre telegram c.151; prijem variantov pri ruseni a stavani ciest
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void VariantRecievedDelegate(object sender, EventArgs3 e);


    /// <summary>
    /// delegate pre event, ak klient vygenerovat spravu o preteceni jeho prijimacieho buffera
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ClientRecBufOverflowDelegate(object sender, EventArgs1 e);

    /// <summary>
    /// delegate pre event, ak klient prijal neznamy telegram do prijimacieho buffera
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void UnknownTelNumberDelegate(object sender, EventArgs1 e);

    #endregion ===Delegates===

    #region Documentation

    /// <summary
    /// <remarks>
    /// <para>
    /// Class Info: wrapper for tcpClientFx;
    /// sends/receives messages to/form server
    /// <list type="bullet">
    /// <item name="author">Author: Bc. Lukas Mitro, RNDr. M. Hrabcak, CSc.</item>
    /// <item name="date">July 2013</item>
    /// <item name="email">hrabcak@procaut.sk</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// </summary>
    #endregion Documentation

    public class Communicator
    {
        private static readonly Communicator _instance = new Communicator();

        public static Communicator Instance => _instance;

        public StavadloGlobalData globalData;

        /* kod v app.xaml.cs
         *  AppCommunicator = Communicator.Instance;
            AppCommunicator.ClientName = GlobalData.HMIClientName;
            AppCommunicator.globalData = GlobalData;
            AppCommunicator.Log = GlobalData.Log;
         */
        //log4net ukladanie sprav o prijatych telegramoch
        public ILog log;

        public string ClientName//nazov klienta, nacita sa z config suboru
        {
            get;set;
        }

        private int clientRequestTelegramId;
        private int clientToServerTelegramId;
        private bool connectionIsOpen;//flag ci konekcia na server je v stave open

        //pre sledovanie dlzky spracovania
        System.Diagnostics.Stopwatch myStopWatch;

        /// <summary>
        /// priznak ci je komunikacny kanal uz otvoreny;
        /// nastavuje sa po prijati spravy zo servera o pripojeni
        /// </summary>
        public bool ConnectionIsOpen
        {
            get { return connectionIsOpen; }
        }

        string logicMessage = string.Empty;
        //    private static Communicator Instance = null;

        /// <summary>
        /// event pri prijati ASCII spravy zo servera: Server Connected
        /// </summary>
        public event InitServerConectionDelegate InitConnectionRecieved;

        /// <summary>
        /// event pri prijati  telegramu c.121 zo servera
        /// </summary>
        //public event ServerData1Delegate MessageUTF8Recieved;
        //public event ServerTlg121Delegate TlgMessage121Recieved;
        public event Action<string> TlgMessage121Recieved;

        /// <summary>
        /// event pred vyslanim telegramu c. 120 do LogicXX
        /// </summary>
        public event Action<string> Tlg120SendingEvent;

        /// <summary>
        /// event pri prijati telegramu c.141 zo servera; struktura obsahuje udaje pre kazdy prvok stavadla
        /// </summary>
        public event MessageDataAllDelegate TlgMessage141Recieved;

        /// <summary>
        /// event pri preruseni spojenia klienta so serverom; napr. vytiahne sa kabel z PC;
        /// generuje ho tcpClientFX
        /// </summary>
        public event InitConnectionFailedDelegate ServerConnectionFailed;

        /// <summary>
        /// event pri prijati spravy o logine uzivatela, telegram c. 111
        /// </summary>
        public event ServerLoginDelegate ServerLoginReceived;

        /// <summary>
        /// event pri prijati telegramu c.131 Info o prvku
        /// </summary>
        //public  event MessageInfoElementDelegate MessageInfoElementReceived;
        public event Action<EventArgsInfo> TlgMessage131Received; //MH: novsi sposob definicie eventu

        /// <summary>
        /// event pri prijati telegramu c.191
        /// </summary>
        public event Action<EventArgsSupravy> TlgMessage191Received;

        /// <summary>
        /// event pri prijati telegramu s fx=101, ak vypadol zdroj nejakeho telegramu
        /// </summary>
        public event ServerErrorMessageDelegate ServerErrorMessageReceived;

        /// <summary>
        /// event pri prijati telegramu cislo 161
        /// </summary>
        public event LogicMessageDelegate TlgMessage161Received;


        /// <summary>
        /// event pri prijati telegramu c.121 a fx=1, ak bola prijata sprava WATCHDOGL zo servera
        /// </summary>
        public event Action WatchdogEvent;

        /// <summary>
        /// event pri prijati telegramu c. 151 Variant ak sa rusi alebo stavia cesta
        /// </summary>
        public event VariantRecievedDelegate TlgMessage151Recieved;

        /// <summary>
        /// event pri prijati fx=-3, server poslal spravu o preteceni jeho prijimaciehp buffera
        /// </summary>
        public event ClientRecBufOverflowDelegate ClientRecBufOverflowReceived;


        public event UnknownTelNumberDelegate UnknownTelNumberReceived;

        public event Action<string> Tlg202SendingEvent;

        

        /// <summary>
        /// Vytvori instanciu s hodnotami: clientName=ST17, clientToServerId=120, clientRequestId=212
        /// a pripoji sa na event tcpClientFx.fxSomethingHapens
        /// </summary>
        private Communicator()//place for instance initialization code
        {
            //nastavenie v App.xaml.cs OnStart()
            //globalData = StavadloGlobalData.Instance;
            //Log = globalData.Log;

            //this.clientName       = "STxx";
            clientToServerTelegramId = 120;
            clientRequestTelegramId  = 121;
            connectionIsOpen = false;

            //nastavenie handlera pre event odpaleny po prijme telegramu
            tcpClientFx.fxSomethingHapens += tcpClientFx_fxSomethingHapens;
            myStopWatch = new System.Diagnostics.Stopwatch();
        }

        /// <summary>
        /// Vytvori instanciu triedy a pripoji sa na event tcpClientFx.fxSomethingHapens;
        /// nastavuje clientToServerId = 120 a clientRequestId  = 121;
        /// vola sa v App.xaml.cs
        /// </summary>
        /// <param name="clientName">Nazov klienta</param>
        /// <param name="clientToServerId">client to server ID</param>
        /// <param name="clientRequestId">client request ID</param>
        //public Communicator(string clientName)
        //{
        //    ClientName = clientName;
        //    this.clientToServerId = 120;
        //    this.clientRequestId  = 121;
        //    this.connectionIsOpen = false;
        //    tcpClientFx.fxSomethingHapens += new tcpClientFx.EventHandler(tcpClientFx_fxSomethingHapens);
        //    myStopWatch = new System.Diagnostics.Stopwatch();
        //}

        /// <summary>
        /// Vytvori instanciu triedy a pripoji sa na event tcpClientFx.fxSomethingHapens;
        /// nastavuje clientToServerId = 120 a clientRequestId  = 121;
        /// vola sa v App.xaml.cs
        /// </summary>
        /// <param name="clientName"></param>
        /// <param name="enableEventHandlerFlag">ci sa ma spustat handler na spracovanie eventu z tcpClientFx</param>
        //public Communicator(string clientName, bool enableEventHandlerFlag)
        //{
        //    this.clientName = clientName;
        //    this.clientToServerId = 120;
        //    this.clientRequestId = 121;
        //    this.connectionIsOpen = false;
        //    if (enableEventHandlerFlag)
        //        tcpClientFx.fxSomethingHapens += new tcpClientFx.EventHandler(tcpClientFx_fxSomethingHapens);
        //    myStopWatch = new System.Diagnostics.Stopwatch();
        //}

        /// <summary>
        /// Vytvori instanciu triedy
        /// </summary>
        /// <param name="clientName">Nazov klienta</param>
        /// <param name="clientToServerId">client to server ID</param>
        /// <param name="clientRequestId">client request ID</param>
        //public Communicator(string clientName, int clientToServerId, int clientRequestId)
        //{
        //    this.clientName = clientName;
        //    this.clientToServerId = clientToServerId;
        //    this.clientRequestId = clientRequestId;
        //    this.connectionIsOpen = false;
        //    tcpClientFx.fxSomethingHapens += new tcpClientFx.EventHandler(tcpClientFx_fxSomethingHapens);
        //    myStopWatch = new System.Diagnostics.Stopwatch();
        //}

        /// <summary>
        /// Inicializacia komunikacie so serverom pre zadane clientName, 
        /// </summary>
        public void InitCommunication()
        {
            if (connectionIsOpen)
                return;
            tcpClientFx.fxInitConnectionToServer(ClientName);
            connectionIsOpen = true;
        }

        /// <summary>
        /// Inicializacia komunikacie pre zadane clientName
        /// </summary>
        /// <param name="clientName">meno</param>
        public void InitCommunication(string clientName)
        {
            if (connectionIsOpen)
                return;
            tcpClientFx.fxInitConnectionToServer(clientName);
            //connectionIsOpen = true; nastavi sa az pri prijme spravy o pripojeni ku serveru
        }

        /// <summary>
        /// Uzavrie spojenie
        /// </summary>
        public void CloseConnection()
        {
            if (connectionIsOpen == true)
            {
                tcpClientFx.fxCloseConnectionToServer();
                connectionIsOpen = false;
            }
        }

        /// <summary>
        /// odosle clientRequestId do servera
        /// </summary>
        private void SubscribeToServer()
        {
            tcpClientFx.tlgSubscribe(clientRequestTelegramId);
        }

        /// <summary>
        /// odosle requestId do servera
        /// Server zaregistruje, ze aplikacii ma posielat telegram requestID
        /// </summary>
        /// <param name="requestID"></param>
        public void SubscribeToServer(int requestID)
        {
            tcpClientFx.tlgSubscribe(requestID);
        }

        /// <summary>
        /// Aplikacia posle serveru ziadost, aby server poslal aplikacii telegram requestID s udajmi
        /// </summary>
        /// <param name="requestID"></param>
        public void SendRequestToServer(int requestID)
        {
            tcpClientFx.tlgRequest(requestID);
        }
        
        /// <summary>
        /// Vrati meno klienta
        /// </summary>
        /// <returns>meno klienta</returns>
        public string GetClientName()
        {
            return ClientName;
        }

        /// <summary>
        /// Vrati client request ID
        /// </summary>
        /// <returns>client request ID</returns>
        public int GetClientRequestId()
        {
            return this.clientRequestTelegramId;
        }

        /// <summary>
        /// Vrati client to server ID
        /// </summary>
        /// <returns>clientToServerId</returns>
        public int GetClientToServerId()
        {
            return this.clientToServerTelegramId;
        }


        //MH: otestovane 15.5.2013 pre Win-7 a Win XP, vsetko OK!!!!!
        public string GetLocalIPaddres()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            var ipaddress = ipEntry.AddressList;

            var q = from a in ipaddress
                    where a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                    select a;
            return q.Last().ToString();
        }

        /// <summary>
        /// Odoslanie spravy typu string do servera; cislo telegramu je clientToServerId
        /// </summary>
        /// <param name="message">sprava</param>
        public void SendMessageToServer(string message)
        {
            //TelegramObject tel = tcpClientFx.fxPrepareTelegram(clientToServerId, message);
            //tcpClientFx.tlgSend(tel);
            try
            {
                TelegramObject tel = tcpClientFx.fxPrepareTelegram(clientToServerTelegramId, message);
                tcpClientFx.tlgSend(tel);
            }
            catch (Exception ex)
            {
                string msg = string.Format("Výnimka pri vyslaní telegramu c.{0} chyba:{1}", clientToServerTelegramId, ex.Message);
                //Log.Info("msg");
                throw new Exception(msg);//zachyti sa az v App.xaml.cs
            }
        }

        /// <summary>
        /// Odoslanie spravy typu object do servera; cislo telegramu je clientToServerId;
        /// </summary>
        /// <param name="message">sprava</param>
        public void SendMessageToServer(object message)
        {
            //TelegramObject tel = tcpClientFx.fxPrepareTelegram(clientToServerId, message);
            //tcpClientFx.tlgSend(tel);
            if (globalData.Log4NetIsEnabled)
                log?.CustomInfo(string.Format("{0}{1}", globalData.LogHeaders["TLG120"], (MessageToLogic)message).ToString());
            try
            {
                TelegramObject tel = tcpClientFx.fxPrepareTelegram(clientToServerTelegramId, message);
                tcpClientFx.tlgSend(tel);
            }
            catch (Exception ex)
            {
                string msg = string.Format("Výnimka pri vyslaní telegramu c.{0} chyba:{1}", clientToServerTelegramId, ex.Message);
                //Log.Info("msg");
                throw new Exception(msg);//zachyti sa az v App.xaml.cs
            }
        }

        /// <summary>
        /// do servera posle tlg. c.120;
        /// urobi zapis do logu
        /// </summary>
        /// <param name="message"></param>
        public void SendTLG120ToServer(MessageToLogic message)
        {
            //TODO: naco je to dobre???
            // if (App.AppMessageManager != null)
            //     App.AppMessageManager.VariantValue = 0;
           
            Tlg120SendingEvent?.Invoke(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.TTT"));
            if (globalData.Log4NetIsEnabled)
                log?.CustomInfo(string.Format("{0}{1}", globalData.LogHeaders["TLG120"], message.ToString()));
            SendMessageToServer(120, message);
        }

        /// <summary>
        /// de servera odosle telegram c. 120 typu WATCHDOG;
        /// nerobi zapis do log4Net
        /// </summary>
        /// <param name="message"></param>
        public void SendTLG120Watchdog(MessageToLogic message)
        {
            SendMessageToServer(120, message);
        }

        /// <summary>
        /// Odoslanie telegramu pre server
        /// </summary>
        /// <param name="telNumber">cislo telegramu</param>
        /// <param name="message">odosielany sprava-string</param>
        public void SendMessageToServer(int telNumber, string message)
        {
            try
            {
                TelegramObject tel = tcpClientFx.fxPrepareTelegram(telNumber, message);
                tcpClientFx.tlgSend(tel);
            }
            catch (Exception ex)
            {
                string msg = string.Format("Výnimka pri vyslaní telegramu c.{0} chyba:{1}", telNumber, ex.Message);
                // Log.Info("msg");
                log?.CustomInfo(string.Format("{0} tlg:{1}   {2}", globalData.LogHeaders["HMI-UNHANDLED_EXCEPTION"], telNumber, ex.StackTrace));
                throw new Exception(msg);
            }
        }

        /// <summary>
        /// Odoslanie telegramu pre server
        /// </summary>
        /// <param name="telNumber">cislo telegramu</param>
        /// <param name="message">>odosielany sprava-objekt</param>
        public void SendMessageToServer(int telNumber, object message)
        {
            try
            {
                TelegramObject tel = tcpClientFx.fxPrepareTelegram(telNumber, message);
                tcpClientFx.tlgSend(tel);
            }
            catch (Exception ex)
            {
                string msg = string.Format("Výnimka pri vyslaní telegramu c.{0} chyba:{1}", telNumber, ex.Message);
                //Log.Info("msg");
                log?.CustomInfo(string.Format("{0} tlg:{1}   {2}", globalData.LogHeaders["HMI-UNHANDLED_EXCEPTION"], telNumber, ex.StackTrace));
                throw new Exception(msg);
            }

        }

        //pridane 02.12.2013 MH
        /// <summary>
        /// odosle na server User_Login data;
        /// urobi zapis do log4net
        /// </summary>
        /// <param name="userLogin"></param>
        public void SendLoginMessageToServer(User_Login userLogin)
        {
            if (globalData.Log4NetIsEnabled)
                log?.CustomInfo(string.Format("{0}{1}", globalData.LogHeaders["TLG110Login"], userLogin.ToString()));
            SendMessageToServer(110, userLogin);
        }

        //pridane 02.12.2013 MH
        /// <summary>
        /// odosle na server User_Login data;
        /// urobi zapis do log4net
        /// </summary>
        /// <param name="userLogin"></param>
        public void SendLogoutMessageToServer(User_Login userLogin)
        {
            if (globalData.Log4NetIsEnabled)
                log?.CustomInfo(string.Format("{0}{1}", globalData.LogHeaders["TLG110Logout"], userLogin.ToString()));
            SendMessageToServer(110, userLogin);
        }

        /// <summary>
        /// Odoslanie telegramu pre logic ohladom prihlaseneho pouzivatela.
        /// </summary>
        /// <param name="logString"></param>
        public void SendTLG202ToServer(string logString)
        {
            Tlg202SendingEvent?.Invoke(logString);

            SendMessageToServer(202, logString);
            if (globalData.Log4NetIsEnabled)
                log?.CustomInfo(string.Format("{0}| Send appStartingInfo to Logic - message: {1}", globalData.LogHeaders["TLG202"], logString));
        }

        /// <summary>
        /// vrati IP adresu servera s ktorym bude aplikacia komunikovat 
        /// </summary>
        /// <returns></returns>
        public string GetServerIP()
        {
            //string s = tcpClientClass.tcpClientFx.fxGetServerInfo(tcpClientClass.tcpClientFx.InfoType.ServerIP);
            return tcpClientClass.tcpClientFx.fxGetServerInfo(tcpClientClass.tcpClientFx.InfoType.ServerIP);
        }

        /// <summary>
        /// vrati port cez ktory ma aplikacia komunikovat 
        /// </summary>
        /// <returns></returns>
        public string GetServerPort()
        {
            return tcpClientClass.tcpClientFx.fxGetServerInfo(tcpClientClass.tcpClientFx.InfoType.ServerPort);
        }

        public string GetServerDomain()
        {
            return tcpClientClass.tcpClientFx.fxGetServerInfo(tcpClientClass.tcpClientFx.InfoType.ServerAddr);
        }

        public string GetProcessIdentity(string appName)
        {
            string pid = tcpClientClass.tcpClientFx.fxGetProcessId(appName);
            return pid;
        }

        /*MH: marec 2019
         * Ak uz je v aplikacii RoleManager prihlaseny dispecer z aplikacie HMI, potom funkcia tcpClientFx.fxGetProcessCount vrati -1
         */
        /// <summary>
        /// Server vrati stav aplikacie RoleManager pre aplikaciu HMI
        /// </summary>
        /// <param name="roleManagerName"></param>
        /// <returns>true RoleManager is OK, false RoleManager is down</returns>
        public bool RoleManagerIsWorking(string roleManagerName)
        {
            int result = 0;
            try
            {
                result = tcpClientFx.fxGetProcessCount(roleManagerName);
            }
            catch (Exception)
            {
            }
            if (result == 1)
                return true;
            return false;
        }

        /* Cisla telegramov od Mervu
         *   PLC_LOGIC        = 100,     // telegram z PLC do Logic
             LOGIC_PLC        = 101,     // telegram z Logic do PLC
             HMI_RM           = 110,     // telegram z HMI do RoleManagera
             RM_HMI           = 111,     // telegram z RoleManager do HMI
             HMI_LOGIC        = 120,     // telegram z viz. programu do Logic - povel
             LOGIC_HMI        = 121,     // telegram z Logic do viz. programu - odpoved

             LOGIC_HMI_INFO   = 131,     // telegram z Logic viz. programu - info o prvku
             LOGIC_LOGIC      = 140,     // telegram z Logic do Logic (simulacny mod)
             LOGIC_HMI_DATA   = 141,     // telegram z Logic do viz. programu - data
             DIAG_LOGIC       = 150,     // telegram z diag. programu DiagLogic do Logic
             LOGIC_DIAG       = 151;     // telegram z Logic do DiagLogic
                                161;     //telegram z Logic do HMI popis operacie; prijem LogicErrorMessage structury z LogicXX 
                                191;     //supravy na ST22
                                202      telegram z HMI do servera ak dve aplikacie HMI riadia jedno stavadlo

         */
        /// <summary>
        /// Spracovanie telegramu prijateho zo servera. Odpaluje prislusne eventy.
        /// </summary>
        /// <param name="fx"></param>
        /// <param name="tlg"></param>
        private void tcpClientFx_fxSomethingHapens(int fx, TelegramObject tlg)
        {
            int dataLength = (int)tlg.m_head.m_tlgLength;
            string dataType = tcpClientFx.convCharArrayToString(tlg.m_head.m_typeOfTlg_chr).ToLower();

            int telNumber = (int)tlg.m_head.m_tlgID;

            //System.Diagnostics.Debug.WriteLine("tcpClientFx_fxSomethingHapens-telNumber: " + telNumber.ToString());
            //System.Diagnostics.Debug.WriteLine("datatype=" + dataType + " length=" + dataLength.ToString());

            //zalogovat prijem telegramu
            //Log.Info(string.Format("HMI-tcpClientFx_fxSomethingHapens: fx={0}, telNumer={1}", fx, telNumber));

            //od 16.8.2013 chodi watchdog - stale prichadzaju nejake spravy
            // ak fx == 1 ... ide o skutocne telegramy
            // fx<0 ... chyby
            // fx>1 hlasenia
            switch (fx)
            {
                case 1: //Prijata sprava z Logicu alebo RoleManagera
                    //zalogovat prijem telegramu
                    //Log.Info(string.Format("tcpClientFx_fxSomethingHapens: fx={0}, telNumer={1}", fx,telNumber) );
                    if (telNumber == 111)//odpoved servera na telegram Login: cisloTelegramu=110
                    {
                        User_Login userLogin = new User_Login(0);
                        object obj = (object)userLogin;
                        tcpClientFx.convByteArrayToStructure(tlg.m_body.m_byte, ref obj);

                        ServerLoginReceived?.Invoke(this, new UserLoginEventArgs((User_Login)obj));
                    }

                    //tieto hlasky vypisovat do spodneho riadku displeja napravo!! 
                    //Logic ma posielat kazdych 5 sekund spravu: WATCHDOG, ak sprava nepride, potom vypisat hlasku: Prerusilo sa spojenie s riadiacim pocitacom Logic!!!
                    else if (telNumber == 121)//odpoved z Logic: OK, Alebo popis chyby?? na cisloTelegramu=120 MessageToServer01; napr. Vymena801
                    {
                        logicMessage = Encoding.UTF8.GetString(tlg.m_body.m_byte, 0, dataLength);//odpoved zo servera OK, BAD???
                        // System.Diagnostics.Debug.WriteLine("telNumber=121 received: " + logicStatus + DateTime.Now.ToString("HH:mm:ss.tt"));
                       
                        if (logicMessage.ToLower().Contains("watchdog")  )
                        {
                            WatchdogEvent?.Invoke();
                            return;
                        }

                        log?.CustomInfo(globalData.LogHeaders["TLG121"] + logicMessage);
                        TlgMessage121Recieved?.Invoke(logicMessage);
                    }


                    else if (telNumber == 131)//prijem telegramu Info o prvku z Logicu
                    {
                        InfoData infoData = new InfoData();
                        object obj = (object)infoData;
                        try
                        {
                            tcpClientFx.convByteArrayToStructure(tlg.m_body.m_byte, ref obj);
                            InfoData info = (InfoData)obj;
                            log?.CustomInfo($"{globalData.LogHeaders["TLG131"]}{info.ToString()}" );
                            TlgMessage131Received?.Invoke(new EventArgsInfo() { ReceivedInfoData = info });//zachytava ho ElementInfoWindowViewModel
                        }
                        catch (Exception ex)
                        {
                            log.CustomError(ex.Message);  
                            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                            {
                                CustomMessageWindow cmb = new CustomMessageWindow(ex.Message);
                                cmb.ShowDialog();
                            }));
                        }
                    }
                    else if (telNumber == 141) //prijem structury Elements
                    {
                        myStopWatch.Start();

                        Elements hmiData1 = new Elements(0);
                        object obj = (object)hmiData1;
                        try
                        {
                            {
                                tcpClientFx.convByteArrayToStructure(tlg.m_body.m_byte, ref obj);
                                Elements hmiData = (Elements)obj;

                                log.Info($"tcpClientFx_fxSomethingHapens: fx={fx}, telNumer={telNumber}, NumOfElements={hmiData.NumOfElements} vypis-start");
#if DEBUG
                                System.Diagnostics.Debug.WriteLine($"tcpClientFx_fxSomethingHapens: fx={fx}, telNumer={telNumber}, NumOfElements={hmiData.NumOfElements}   vypis-start");

                                for (int i = 0; i < hmiData.NumOfElements; i++)
                                {
                                    PRVOK_STAVADLA data = hmiData.element[i];
                                    if (!string.IsNullOrEmpty(data.nazov))
                                    {
                                        //Log sa neda formatovat
                                        //Log.Info(string.Format("{0:000} Nazov:{1,7}  Podtyp:{2,-2} Stav:{3:X4} Vyluka:{4:X4} UvolIzol:{5:X4}", i, data.nazov, data.podtyp.ToString(), data.stav, data.vyluka, data.uvolizol));
                                        //Debug sa da formatovat

                                        //System.Diagnostics.Debug.WriteLine(string.Format("{0:000} Nazov:{1,7}  Podtyp:{2,-2} Stav:{3:X4} Vyluka:{4:X4} UvolIzol:{5:X4}", i, data.nazov, data.podtyp.ToString(), data.stav, data.vyluka, data.uvolizol));
                                        System.Diagnostics.Debug.WriteLine($"{i:000} Nazov:{ data.nazov,7}  Podtyp:{data.podtyp.ToString(),-2} Stav:{data.stav:X4} Vyluka:{data.vyluka:X4} UvolIzol:{data.uvolizol:X4}");
                                    }
                                }
                                //Log.Info(string.Format("tcpClientFx_fxSomethingHapens: fx={0}, telNumer={1} vypis-koniec", fx, telNumber));
                                //Log.Info(string.Format("Tlg:{0} vypis-koniec", telNumber));
                                System.Diagnostics.Debug.WriteLine($"tcpClientFx_fxSomethingHapens: fx={fx}, telNumer={telNumber} vypis-koniec");
#endif   
                                //System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss.FFF}");

                                //TODO: nechat zapis do logu!!!!!!
                                log?.CustomInfo(globalData.LogHeaders["TLG141"] + SerDesManager.SerializeToBase64<Elements>(hmiData)  );
                                TlgMessage141Recieved?.Invoke(this, new EventArgs2() { RecievedStruct = hmiData });
                            }
                        }
                        catch (Exception ex)
                        {
                            string msg =$"{globalData.LogHeaders["HMI-UNHANDLED_EXCEPTION"]} tcpClientFx_fxSomethingHapens: fx={fx} tlg={telNumber} {ex.Message}";
                            log.CustomError(msg);

                            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                            {
                                CustomMessageWindow win = new CustomMessageWindow("Error", msg);
                                win.ShowDialog();
                            }));
                        }
                        myStopWatch.Stop();
                        System.Diagnostics.Debug.WriteLine("tcpClientFx_fxSomethingHapens: telNumber=141 koniec spracovania: " + DateTime.Now.ToString("HH:mm:ss.FFF") + " dlzka [ms]:" + myStopWatch.ElapsedMilliseconds.ToString());
                        myStopWatch.Reset();
                    }
                    else if (telNumber == 151)//prijem structury variant; pri stavani vlak. a  posun. cesty
                    {
                        VariantCesty variant = new VariantCesty();
                        object obj = (object)variant;
                        tcpClientFx.convByteArrayToStructure(tlg.m_body.m_byte, ref obj);
                        //InfoData info = (InfoData)obj;
                        // Log.Info(string.Format("HMI-tlg 151-Variant: var_number={0}, signalName={1} ", ((VariantCesty)obj).var_number, ((VariantCesty)obj).signalName));
                        log?.CustomInfo(globalData.LogHeaders["TLG151"] + ((VariantCesty)obj).ToString());

                        TlgMessage151Recieved?.Invoke(this, new EventArgs3() { Variant = (VariantCesty)obj });//Spracuje sa v MessageManger.cs a UC_MapaStavadla.xaml.cs
                    }
                    else if (telNumber == 161)//Error message z Logicu
                    {
                        LogicMessage161 errMessage = new LogicMessage161();
                        object obj = (object)errMessage;
                        tcpClientFx.convByteArrayToStructure(tlg.m_body.m_byte, ref obj);

                        LogicMessage161 erm = (LogicMessage161)obj;
                        // Log.Info(globalData.LogHeaders["TLG161"] + string.Format("{0};{1};{2}", erm.HMI_text, erm.text_pos.ToString(), erm.color_code.ToString()));
                        log?.CustomInfo(string.Format("{0}{1}", globalData.LogHeaders["TLG161"], erm.ToString()));

                        TlgMessage161Received?.Invoke(this, new EventErrorArgsLogic() { message = erm.HMI_text, textPosition = erm.text_pos, colorCode = erm.color_code });
                    }
                    else if (telNumber == 191)
                    {
                        Sets supravyData1 = new Sets();
                        object obj = (object)supravyData1;
                        try
                        {
                            tcpClientFx.convByteArrayToStructure(tlg.m_body.m_byte, ref obj);
                            Sets supravyData = (Sets)obj;
#if DEBUG
                            //log.Info($"tcpClientFx_fxSomethingHapens: fx={fx}, telNumer={telNumber}, NumOfElements={supravyData.NumOfSets} vypis-start");

                            for (int i = 0; i < supravyData.NumOfSets; i++)
                            {
                                SupravaData data = supravyData.set[i];
                                //log.Info($"{i:00} Kolaj:{data.kolaj}  Cislo supravy:{data.cislo_supravy} Datum:{data.datum} Cas:{data.cas} Priorita:{data.priorita}");
                                System.Diagnostics.Debug.WriteLine($"{i:00} Kolaj:{data.kolaj}  Cislo supravy:{data.cislo_supravy} Datum:{data.datum} Cas:{data.cas} Priorita:{data.priorita}");
                            }
                            //log.Info($"tcpClientFx_fxSomethingHapens: fx={fx}, telNumer={telNumber}, NumOfElements={supravyData.NumOfSets} vypis-koniec");

#endif
                            log?.CustomInfo(globalData.LogHeaders["TLG191"] + SerDesManager.SerializeToBase64<Sets>(supravyData) );
                            TlgMessage191Received?.Invoke(new EventArgsSupravy() { RecievedStruct = supravyData });
                        }
                        catch( Exception ex)
                        {
                            string msg = $"{globalData.LogHeaders["HMI-UNHANDLED_EXCEPTION"]} tcpClientFx_fxSomethingHapens: fx={fx} tlg={telNumber} {ex.Message}";
                            log.CustomError(msg);

                            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                            {
                                CustomMessageWindow win = new CustomMessageWindow("Error", msg);
                                win.ShowDialog();
                            }));
                        }
                    }
                    else
                        System.Diagnostics.Debug.WriteLine("telNumber: " + telNumber.ToString());
                    break;

                case 2: //Pripojenie k Serveru je OK

                    //System.Diagnostics.Debug.Write(string.Format("=======tcpClientFx_fxSomethingHapens:Pripojenie k Serveru je OK!! fx={0}, telNumer={1}===========", fx, telNumber));
                    logicMessage = Encoding.ASCII.GetString(tlg.m_body.m_byte, 0, dataLength);
                    //System.Diagnostics.Debug.WriteLine($"Communicator fx=2 logicMessage={logicMessage}");
                    log?.CustomInfo($"{globalData.LogHeaders["HMI-INFO"]} Pripojenie k serveru: {logicMessage}");//logicMessage: Connected 2 to server

                    connectionIsOpen = true;
                    InitConnectionRecieved?.Invoke(this, new EventArgs1() { recievedMessage = logicMessage });

                    SubscribeToServer(111);//server bude pre klienta HMI posielat aj telegramy z RoleManagera
                    SubscribeToServer(121);//jednoducha odpoved OK zo servera, alebo chybova hlaska
                    SubscribeToServer(131);//pre prijem telegramu Info o prvku z LogicXX
                    SubscribeToServer(141);//prijem structury pre vsetky prvky stavadla
                    SubscribeToServer(151);//prijem variantu pri stavani cesty - VariantCesty
                    SubscribeToServer(161);//prijem LogicErrorMessage structury z LogicXX
                    SubscribeToServer(191);//prijem  telegramu, ktory obsahuje udaje o supravach na usekoch
                    break;

                case 3:  //Odpojenie od servera
                    log?.CustomInfo(string.Format("{0}{1}", globalData.LogHeaders["HMI-INFO"], "tcpClientFx: Odpojenie od servera"));
                    break;

                case 5:  //Warning: ClientINI.xml problem or some values are missing. Default values are used.
                    log?.CustomInfo(string.Format("{0}{1}", globalData.LogHeaders["HMI-INFO"], "tcpClientFx: ClientINI.xml problem or some values are missing. Default values are used."));
                    break;

                case 6:  //Warning: ClientINI.xml problem or some values are missing. Default values are used.
                    log?.CustomInfo(string.Format("{0}{1}", globalData.LogHeaders["HMI-INFO"], "tcpClientFx: ClientINI.xml does not exist. Default values are used."));
                    break;

                case -2: //chyba pri konekte na server, generuje ju tcpClientClass.dll, napr. ak sa vytiahne sietovy kabel z PC kde bezi aplikacia
                         //ak nefunguje komunikacia asi 45 sekund
                    log?.CustomInfo(string.Format("{0}{1}", globalData.LogHeaders["HMI-INFO"], "tcpClientFx: Chyba pri spojení so serverom!"));
                    
                    ServerConnectionFailed?.Invoke(this, new EventArgs1() { recievedMessage = "TcpClient: Chyba pri spojení so serverom!" });
                    break;


                case -3://pretecenie prijimacieho buffera na klientovi, generuje to klient

                    log?.CustomInfo(string.Format("{0}{1}", globalData.LogHeaders["HMI-INFO"], "tcpClientFx: Pretecenie receive buffera na klientovi!!"));
                    ClientRecBufOverflowReceived?.Invoke(this, new EventArgs1() { recievedMessage = "Pretecenie receive buffera na klientovi!!" });

                    break;

                case -4://klient prijal neznamy telegram
                    log?.CustomInfo(string.Format("{0}{1}", globalData.LogHeaders["HMI-INFO"], "tcpClientFx: Prijatý neznámy telegram!"));
                    UnknownTelNumberReceived?.Invoke(this, new EventArgs1() { recievedMessage = "Prijatý neznámy telegram" });
                    break;

                case 101://zo servera prisla sprava, ze spadol zdroj nejakeho telegramu
                    tcpClientClass.AnswerTlg serverData = tcpClientFx.DecodeAnswerTlg(tlg);
                    int errorTelNumber = serverData.i_par1;//cislo telegramu, ktoreho zdroj crashol
                    string message = string.Format("Zdroj telegramu: {0} neodpovedá!!", errorTelNumber);
                    log?.CustomInfo(string.Format("{0}{1}", globalData.LogHeaders["HMI-INFO"], message));

                    ServerErrorMessageReceived.Invoke(this, new EventErrorArgs() { errorMessage = message, telegramNumber = errorTelNumber });
                    break;

                default:
                    //txbfxFromserver.Text = txt;
                    break;
            }
        }//tcpClientFx_fxSomethingHapens
    }//class Communicator

    /// <summary>
    /// Argument eventu
    /// </summary>
    public class EventArgs1 : EventArgs
    {
        public string recievedMessage;
    }

    public class EventArgs2 : EventArgs
    {
        public Elements RecievedStruct;
    }

    public class EventArgs3 : EventArgs
    {
        public VariantCesty Variant;
    }


    public class EventArgsSupravy : EventArgs
    {
        public Sets RecievedStruct;
    }

    public class EventArgsInfo : EventArgs
    {
        public InfoData? ReceivedInfoData;
    }

    public class EventErrorArgs : EventArgs
    {
        /// <summary>
        /// textova sprava o chybe
        /// </summary>
        public string errorMessage;
        /// <summary>
        /// cislo telegramu, ktoreho zdroj sa nehlasi serveru
        /// </summary>
        public int telegramNumber;
    }

    public class EventErrorArgsLogic : EventArgs
    {
        /// <summary>
        /// textova sprava o chybe
        /// </summary>
        public string message;//max. 100 znakov
        public char textPosition;// pozicia textu, 'c' - v strede obrazovky: Dialogove okno, 'd' - dole, v spodnom riadku
        public char colorCode; // kod farby, 'b' - cierna, 'r' - cervena
    }
}
