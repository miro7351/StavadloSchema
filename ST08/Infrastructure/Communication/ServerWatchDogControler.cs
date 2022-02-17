using System;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using Stavadlo22.Data;
using Stavadlo22.Windows;
using WPFLocalizeExtension.Engine;

namespace Stavadlo22.Infrastructure.Communication
{
    /// <summary>
    /// kazdych x sekund kontroluje, casovy interval od posledneho prijmu telegramu zo Servera, ak interval je vacsi ako zadana hranica, potom odpali event;
    /// </summary>
    public class ServerWatchdogControler
    {
        public StavadloGlobalData GlobalData;//globalne data pre aplikaciu
        //StavadloModel stavadloModel;
        public Communicator AppCommunicator; //komunikator pre aplikaciu
        public AppEventsInvoker EventInvoker;
        //log4net ukladanie sprav o prijatych telegramoch
        private readonly log4net.ILog log;// = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public  double MillisecInterval;
        private Timer WatchdogTimer; //System.Timers.Timer
        //DispatcherTimer WatchdogTimer;
        //private DateTime WatchDogTime;//datum cas posledneho prijmu telegramu c.121 s obsahom "WATCHDOG"
       
        string msg1;

        private ServerConnectionLostWindow serverConnectionLostWindow;
        Boolean NastaloPrerusenieKomunikacie;//priznak ci nastalo preursenie komunikacie so serverom


        //public static readonly ServerWatchdogControler _instance = new ServerWatchdogControler();

        //Server ma posielat pre HMI aplikaciu kazdych 5 sekund spravu: WATCHDOG,
        //ak sprava nepride do casu urcenom secondsInterval, potom zobrazit okno a vypisat hlasku: Prerusilo sa spojenie s riadiacim pocitacom Logicom!!! 

        /// <summary>
        ///  Timer kontroluje casovy interval od posledneho prijmu telegramu zo servera, ak interval je vacsi ako zadana hranica, potom zobrazi dialogove okno;
        ///  Ak do daneho intervalu nepride telegram  'Watchdog' zo servera, zobrazi sa okno s informaciou o preruseni spojenia so serverom (Logicom).
        ///  Timer sa spusti po prijme prveho telegramu 'Watchdog' zo servera.
        /// </summary>
        /// <param name="secondsInterval">Predstavuje cas v sekundach, ak do daneho intervalu nepride telegram  'Watchdog' zo servera, </param>
        public ServerWatchdogControler(Window parent, double secondsInterval)
        {
            GlobalData = StavadloGlobalData.Instance;
            
            AppCommunicator = Communicator.Instance;
            EventInvoker = AppEventsInvoker.Instance;

            log = GlobalData.Log;

            serverConnectionLostWindow = new ServerConnectionLostWindow("Výpadok komunikácie so serverom!!!");
            serverConnectionLostWindow.Owner = parent;

            
            
            NastaloPrerusenieKomunikacie = false;  //aby sa spustil timer pri spusteni programu
            MillisecInterval = secondsInterval*1000;
            WatchdogTimer = new Timer();
            WatchdogTimer.Interval = MillisecInterval;
            WatchdogTimer.AutoReset = false;//MH pridane marec 2019; Elapsed event handler sa spusti len raz
            WatchdogTimer.Elapsed += timer_Elapsed;

            WatchdogTimer.Start();
            //WatchdogTimer = new DispatcherTimer();
            //WatchdogTimer.Interval = new TimeSpan(0,0,(int)secondsInterval);
            //WatchdogTimer.Tick += WatchdogTimer_Tick;
            
            //zachytenie eventu ak pride sprava WATCHDOG zo servera
            AppCommunicator.WatchdogEvent += ServerWatchdog_EventHandler;
        }

        /*System.Timres.Timer
         * AutoReset= true default
         * If Start is called and AutoReset is set to false, the Timer raises the Elapsed event only once, the first time the interval elapses.
         * If Start is called and AutoReset is set to true, the Timer raises the Elapsed event the first time the interval elapses and continues to raise the event on the specified interval. 
            You can also start timing by setting Enabled to true
            If AutoReset is false, the Start method must be called in order to start the count again.
         * 
         * Start() starts raising the Elapsed event by setting Enabled to true.
         * Stop() stops raising the Elapsed event by setting Enabled to false.
         * The signal to raise the Elapsed event is always queued for execution on a ThreadPool thread
         * 
         * A call to the Start() method when the timer is enabled has no effect!!!!!
         */


        /// <summary>
        /// Osetrenie vypadku komunikacie aplikacie so serverom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Elapsed(object sender, ElapsedEventArgs e)//WatchdogTimer tikne len raz a spusti sa znovu ak pride watchdog telegram zo servera!!!
        {
            NastaloPrerusenieKomunikacie = true;
            EventInvoker.Invoke_ServerConnectionEvent("Výpadok komunikácie so serverom");
            log?.CustomInfo($"{GlobalData.LogHeaders["HMI-INFO"]}Výpadok komunikácie so serverom: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            ShowMessage(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
        }

        /// <summary>
        /// Handler pre event, ak prisla sprava WATCHDOG zo Servera; 
        /// Event sa odpaluje v AppCommunicator po prijati tlg. c 121 so spravou watchdog; 
        /// Odstartuje timer; Ak bolo prerusenie komunikacie, vypise oznam o obnoveni komunikacie, posle ziadost pre Server, aby poslal telegram kde je posledny stav stavadla, zavrie okno s oznamom...
        /// </summary>
        private void ServerWatchdog_EventHandler()//po prijme tlg. watchdog sa VZDY RESTARTUJE WatchdogTimer !!!!!!!
        {
            WatchdogTimer.Enabled = false;
            WatchdogTimer.Start(); //znovu odstartuje timer;A call to the Start() method when the timer is enabled has no effect!!!!!

            if (NastaloPrerusenieKomunikacie) //ak bolo prerusenie komunikacie, vypisem oznam o obnoveni komunikacie, zavriem okno s oznamom....
            {
                NastaloPrerusenieKomunikacie = false;
                {
                    msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "appConnectedToServer", LocalizeDictionary.Instance.Culture));
                    //("Aplikácia je pripojená na server");
                    EventInvoker.Invoke_ServerConnectionEvent(msg1);
                    log?.CustomInfo($"{GlobalData.LogHeaders["HMI-INFO"]}Obnovenie komunikácie so serverom: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                }

                AppCommunicator.SendRequestToServer(141);// //ziadost pre Server, aby poslal telegram kde je posledny stav stavadla;

                //if( serverConnectionLostWindow?.IsActive ?? true) //ak je IsActive == true, potom sa vola serverConnectionLostWindow.Hide()
                App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                {
                    serverConnectionLostWindow?.Hide();
                }));
            }
        }

        /// <summary>
        /// ukaze dialogove okno: Nemam spojenie so serverom Logic
        /// </summary>
        /// <param name="message"></param>
        public void ShowMessage(string message)
        {
            serverConnectionLostWindow.MainText = message;
            {
                App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                {
                    try
                    {
                        if (!serverConnectionLostWindow.IsActive)
                            serverConnectionLostWindow.ShowDialog();
                    }
                    catch { } //LM: hadze chybu ak sa vypne okno a vlakno sa snazi este vykreslit zmenu
                }));
            }
        }

       

        public void Start()
        {
            //running = true;
            WatchdogTimer.Start();
        }

        public void Stop()
        {
            WatchdogTimer.Stop();
            //running = false;
        }

        
    }//class ServerWatchdogControler
}
