using System;
using System.Timers;
using Stavadlo22.Data;
using Stavadlo22.Data.Telegrams;
using Stavadlo22.Infrastructure.Enums;

namespace Stavadlo22.Infrastructure.Communication
{
    
    /// <summary>
    /// Len MASTER aplikacia posiela do servera (Logicu) telegram c.121 s textom Watchdog v intervale 5 s.
    /// je to pre to, aby server (Logic) vedel, ze HMI aplikacia master funguje;
    /// Zabezpecuje vysielanie spravy WATCHDOG do servera kazdych 5 sekund;
    /// </summary>

    public class HMI_Watchdog_Sender  //MainWindowStavadlo.cs tu sa vytvori instancia HMI_Watchdog_Sender;
    {
        public static readonly HMI_Watchdog_Sender _instance = new HMI_Watchdog_Sender();
        public static HMI_Watchdog_Sender Instance => _instance;

        private HMI_Watchdog_Sender()
        {
            WatchdogTimer = new Timer();
            //WatchdogTimer.Interval sa nastavi az pri nastaveni property TimerInterval;
            WatchdogTimer.Elapsed += new ElapsedEventHandler(WatchdogTimer_Elapsed);
            //WatchdogTimer sa spusti az v handleri AppCommunicator_ServerLoginReceived po prihlaseni uzivatela
        }

        /*  Kod spusteny v MainWindowStavadlo.xaml.cs!!!
            WatchdogSender = HMIwatchdog_Sender.Instance;
            WatchdogSender.GlobalData = StavadloGlobalData.Instance;
            WatchdogSender.AppCommunicator = Communicator.Instance;
            WatchdogSender.TimerInterval = StavadloGlobalData.Instance.HMI_HMI_HMI_WatchDogTimerInterval;//default 5 sekund
            WatchdogSender.SetServerLoginEvent();
            WatchdogSender.InitMessage();
         */

        /// <summary>
        /// Nastavi strukturu, ktora sa vysiela do servera v tlg. 120
        /// </summary>
        public void InitMessage()
        {
            message = new MessageToLogic(0);
            message.kodRezimu = "WATCHDOG";
            //message.elementName = "";
            message.elementName = string.Format("{0}_{1}", GlobalData.HMIClientName, GlobalData.ComputerIP);
            message.level = '0';
            //message.level nastavi sa az v timer_Elapsed
            //message.level = GlobalData.AppUserLevelMode; // Dispecer je to '0', pre Udrzba='1', Admin='2'
            message.kod = 'x';
            message.kod2 = 'x';
        }

        /// <summary>
        /// nastavi handler pre event AppCommunicator.ServerLoginReceived
        /// </summary>
        public void SetServerLoginEvent()
        {
            //handler spusteny po prihlaseni/odhlaseni uzivatela. Po logine sa posle telegram do komunikacneho servera, a ten vrati HMI aplikacii telegram...
            //event ServerLoginReceived je odpaleny po prijme tlg. 111
            if (AppCommunicator != null)
                AppCommunicator.ServerLoginReceived += AppCommunicator_ServerLoginReceived;
        }

        #region --FIELDS---

        private Timer WatchdogTimer;  //System.Timers.Timer

        int timerInterval;
        /// <summary>
        /// Interval pre tikanie timera v sekundach, ktory riadi vysielanie spravy 'WatchDog'
        /// </summary>
        public int TimerInterval
        {
            get
            {
                return timerInterval;
            }
            set
            {
                timerInterval = value;
                WatchdogTimer.Interval = timerInterval *1000;
            }
        }

        private MessageToLogic message;//sprava odosielana do Logicu

        //globalne udaje pre aplikaciu
        public StavadloGlobalData GlobalData;

        //Communicator appCommunicator;
        public Communicator AppCommunicator;


        #endregion --FIELDS---


        /// <summary>
        /// Event, ktory nastane ak sa strati spojenie s HMI (messageCounter >= 2)
        /// </summary>
        //public event Action<EventErrorArgs> ConnectionLostEvent;

        /// <summary>
        /// Event, ktory bude odpaleny po odoslani tlg. c.120 so spravou Watchdog do Logicu
        /// </summary>
        public event Action MessageFromHMISentEvent;


        /*
        /// <summary>
        /// Zabezpecuje vysielanie spravy WATCHDOG do Logic servera kazdych 5 sekund;
        /// Spusti sa po prihlaseni uzivatela;
        /// </summary>
        /// <param name="timeInterval">interval v milisekundach</param>
        public HMIwatchdog_Sender(int timeInterval)
        {
            GlobalData = StavadloGlobalData.Instance;
            AppCommunicator = Communicator.Instance;

            //handler pre prihlasenie/odhlasenie uzivatela.
            //Po logine sa posle telegram do komunikacneho servera, a komunikacny server vrati aplikacii telegram...
            if (AppCommunicator != null)
                AppCommunicator.ServerLoginReceived += AppCommunicator_ServerLoginReceived;

            //messagesCounter = 0;
            message = new MessageToLogic(0);
            message.kodRezimu = "WATCHDOG";
            //message.elementName = "";
            message.elementName = string.Format("{0}_{1}", GlobalData.HMIClientName, GlobalData.ComputerIP); 
            message.level = '0';
            //tu este nie je platny  App.AppUserLevelMode,  nastavi sa az v timer_Elapsed
            //message.level = App.AppUserLevelMode; // Dispecer je to '0', pre Udrzba='1', Admin='2'
            message.kod = 'x';
            message.kod2 = 'x';

            WatchdogTimer = new Timer();
            WatchdogTimer.Interval = timeInterval; //new TimeSpan(0, 0, 5).Milliseconds; //5 sekund
            WatchdogTimer.Elapsed += WatchdogTimer_Elapsed;
        }
        */

        /// <summary>
        /// Event handler spusteny po prihlaseni uzivatela;
        /// Spusti/zastavi casovac po prihlaseni/odhlaseni uzivatela
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AppCommunicator_ServerLoginReceived(object sender, UserLoginEventArgs e)
        {
            if (string.IsNullOrEmpty(e.userLoginData.userRole))  // odhlasenie
            {
                Stop();
            }
            else  //prihlasenie
            {
                Start();
            }
        }

        /// <summary>
        /// vyslanie spravy WATCHDOG do Servera;
        /// odpalenie eventu MessageFromHMISentEvent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WatchdogTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //po odhlaseni a prihlaseni noveho uzivatela sa meni aj App_UserLevelMode
            message.level = (char)GlobalData.App_UserLevelMode;

            //System.Diagnostics.Debug.WriteLine($"*********HMIwatchdog_Sender:{message} ************");
            AppCommunicator.SendTLG120Watchdog(message);//Watchdog telegram sa neloguje
            
             MessageFromHMISentEvent?.Invoke(); //odpalenie eventu, po vyslani tlg.120 so spravou WATCHDOG, event sa registruje v UC_ConnectionIndicators;
            //UC_ConnectionIndicators riadi animaciu ikony v pravom spodnom rohu displeja (siva-zelena)
        }

        void Start()
        {
            if (GlobalData.ApplicationMode == APPLICATION_MODE.MASTER)//len aplikacia  typu Master vysiela do servera  telegram so spravou WATCHDOG 
                WatchdogTimer.Start();
        }

        void Stop()
        {
            WatchdogTimer.Stop();
        }

        //public void ResetCounter()
        //{
        //    this.messagesCounter = 0;
        //}

        /// <summary>
        /// pre simulovanie spustenie eventu
        /// </summary>
        //public void ConnectionLostSimulation()
        //{
        //    Stavadlo17.Infrastructure.Communication.EventErrorArgs e = new Stavadlo17.Infrastructure.Communication.EventErrorArgs() { errorMessage = "Simulacia", telegramNumber = 141 };
        //    if (ConnectionLostEvent != null)
        //    {
        //        ConnectionLostEvent(e);
        //    }
        //}

    }//class HMIwatchdog_Sender
}
