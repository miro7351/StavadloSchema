using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PA.Stavadlo.UserControls
{
    /*
     * Semafor1Control  ma vertikalnu nozicku x:Name="Nozicka1"
     * a dve horizontalne podstavy:
     * - x:Name="Nozicka2" celkom spodna podstava
     * - x:Name="Nozicka3" nad castou  x:Name="Nozicka2"
     */
    /// <summary>
    /// Typ semaforu.
    /// </summary>
    public enum SEMAFOR_TYPE
    {
        /// <summary>
        /// Zatial neinicializovany
        /// </summary>
        NONE,

        /// <summary>
        /// Klasicky stojanovy semafor s nozickou (zelena/cervena).
        /// viditelne:"Nozicka1", "Nozicka2"
        /// </summary>
        S1,

        /// <summary>
        /// Zriadovaci semafor bez nozicky (biela/modra)-trpaslici.
        ///  viditelne: len "Nozicka3",
        /// </summary>
        S2,

        /// <summary>
        ///trpaslici s podstavou, farby ako S1
        /// </summary>
        S3,

        /// <summary>
        /// Zriadovaci semafor bez nozicky (biela/modra).
        /// viditelne:"Nozicka1", "Nozicka2"
        /// </summary>
        S4
    }

    /// <summary>
    /// mod semaforu
    /// </summary>
    public enum SEMAFOR1_MODE
    {
        NONINIT_STATE, //pokial sa neprijme telegram z Logicu, semafor bude v NONINIT stave
        STOJ,
        VOLNO,
        POSUN_NEZABEZP_DOVOLENY,

        //      POSUN_ZABEZP_DOVOLENY,
        //      POSUN_NEDOVOLENY,
        VYLUKA_NAVESTIDLA,   
        VYLUKA_CERVENA,
        VYLUKA_ZELENA,
        VYLUKA_BIELA,

        PORUCHA_NAVESTIDLA,         //semafor zasiveny, potom normalny s cervenym obrysom blika
        PORUCHA_POVOLOVACEJ_NAVESTI,//semafor zasiveny, potom normalny s cervenym obrysom blika, dole modre svetlo
        PRECHODOVY_STAV,            //semafor zasiveny neblika 
        ZLTY_STAV,                  //obidve svetla su zltej farby, neblikaju
        // ZLTY_STAV_PRECHOD //pri prechode do zlteho stavu (cerveno - sivy border)
        PRIVOLAVACIA_NAVEST,       //Hore: CERVENA dole BIELA BLIKA
        START_END_PART_OF_WAY      //ak semafor je vybraty za pociatok PCesty
    }

    /// <summary>
    /// Interaction logic for Semafor1Control.xaml
    /// </summary>
    public partial class Semafor1Control : UserControl
    {
        //AppEventsInvoker appEventsInvoker;
        //StavadloGlobalData globalData;
        //StavadloModel stavadloModel;
        double errorOpacity;//opacity pri poruchovych stavoch a pri blikani

        public Semafor1Control()
        {
            InitializeComponent();
            //appEventsInvoker = AppEventsInvoker.Instance;
            //globalData = StavadloGlobalData.Instance;
            //stavadloModel = StavadloModel.Instance;

            // na zaciatku su sede neinicializovane
            PrevTopLightBrush = (SolidColorBrush)Resources["lightOffBrush"];
            PrevBottomLightBrush = (SolidColorBrush)Resources["lightOffBrush"];
           
            errorOpacity = Convert.ToDouble(this.Resources["opacity"]);

            IsPCestaStart = IsPCestaEnd = false;
            CestaPovolena = false;
            this.Loaded += Semafor1Control_Loaded;
        }

       

        private void Semafor1Control_Loaded(object sender, RoutedEventArgs e)
        {
            //Pre kazdy semafor mozeme nastavit v xaml binding pre DP TimerFlag: <controls:Semafor1Control x:Name="L808"  TimerFlag="{Binding TimerFlag, Mode=OneWay}".../>
            //Zmena TimerFlag zabezpecuje prekreslenie semaforu, ak je v blikajucom stave.
            //Aby sme nepisali pre kazdy semafor binding pre DP TimerFag v xaml, binding nastavime tu v kode.
            //Je to vyhodne, lebo pre vsetky semafory DP TimerFlagProperty sa binduje na jeden zdroj v DataContexte!!!
            //"TimerFlag" je meno property z DataContextu.
            //DataContext.TimerFlag sa nastavuje pomocou 1 sec. casovaca true/false, pouziva na to, aby vsetky semafory blikali synchronizovane (naraz)!!

            Binding timerFlagBinding = new Binding("TimerFlag") { Mode = BindingMode.OneWay, Source = this.DataContext };
            BindingOperations.SetBinding(this, TimerFlagProperty, timerFlagBinding);
            string n1 = this.Name;  //n1=ucSemafor??? preco to nie je x:Name uvedeny v xaml????
        }


        /// <summary>
        /// priznak ci treba nastavit semafor do normalneho stavu: vsetky prvky su cierne a svetla vypnute???
        /// </summary>
        public bool CanSemaphoreResetToNormal = true;

        /*semafor blika ak je v jednom zo stavov:
         SEMAFOR1_MODE.PORUCHA_POVOLOVACEJ_NAVESTI spusta sa aj zvukove znamenie
         SEMAFOR1_MODE.PORUCHA_NAVESTIDLA   spusta sa aj zvukove znamenie
         SEMAFOR1_MODE.PRIVOLAVACIA_NAVEST
         */

        /// <summary>
        /// priznak ci kontrol ma blikat
        /// </summary>
        public bool BlinkedStateEnabled
        {
            get; set;
        }

        /// <summary>
        /// flag udava ci uz bol klik na semafor pre povolenie cesty
        /// </summary>
        public bool CestaPovolena
        {
            get; set;
        }

        /// <summary>
        /// priznak, ci semafor je vybraty ako zaciatok PCesty
        /// </summary>
        public bool IsPCestaStart
        {
            get;set;
        }

        /// <summary>
        /// priznak, ci semafor je vybraty ako koniec PCesty
        /// </summary>
        public bool IsPCestaEnd
        {
            get; set;
        }

        private SolidColorBrush prewTopLightBrush;
        /// <summary>
        /// Previous top light brush; minula farba horneho svetla
        /// </summary>
        public SolidColorBrush PrevTopLightBrush
        {
            get { return prewTopLightBrush; }
            set { prewTopLightBrush = value; }
        }

        private SolidColorBrush prewBottomLightBrush;
        /// <summary>
        /// Previous bottom light brush; minula farba spodneho svetla
        /// </summary>
        public SolidColorBrush PrevBottomLightBrush
        {
            get { return prewBottomLightBrush; }
            set { prewBottomLightBrush = value; }
        }

       

        #region ========== RoutedEvents =============


        // 9.11.2015 lavy dvojklik na semafor v suvislosti so stavanim Posunovych/Vlakovych Ciest
        public static readonly RoutedEvent Semafor1LeftDoubleClickAttachedEvent = EventManager.RegisterRoutedEvent("Semafor1LeftDoubleClickAttachedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Semafor1Control));
        private void RaiseSemafor1LeftDoubleClickAttachedEvent()
        {
            //RoutedEventArgs newEventArgs = new RoutedEventArgs(Semafor1Control.Semafor1LeftDoubleClickAttachedEvent, new SemaphoreEventArgs() { Name = "Left Double Click", sender = this });
            //RaiseEvent(newEventArgs);
        }
        public static void AddSemafor1LeftDoubleClickAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ((UIElement)d).AddHandler(Semafor1Control.Semafor1LeftDoubleClickAttachedEvent, handler);
        }

        public static void RemoveSemafor1LeftDoubleClickAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ((UIElement)d).RemoveHandler(Semafor1Control.Semafor1LeftDoubleClickAttachedEvent, handler);
        }

        // --------9.11.2015 Lavy klik na semafor --------------
        public static readonly RoutedEvent Semafor1LeftClickAttachedEvent = EventManager.RegisterRoutedEvent("Semafor1LeftClickAttachedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Semafor1Control));
        private void RaiseSemafor1LeftClickAttachedEvent()
        {
            //RoutedEventArgs newEventArgs = new RoutedEventArgs(Semafor1Control.Semafor1LeftClickAttachedEvent, new SemaphoreEventArgs() { Name = "Left Click", sender = this });
            //RaiseEvent(newEventArgs);
        }
        public static void AddSemafor1LeftClickAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ((UIElement)d).AddHandler(Semafor1Control.Semafor1LeftClickAttachedEvent, handler);
        }

        public static void RemoveSemafor1LeftClickAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ((UIElement)d).RemoveHandler(Semafor1Control.Semafor1LeftClickAttachedEvent, handler);
        }

        //----------------------------------------------------------------------------------------------

        //pravy klik na semafor
        public static readonly RoutedEvent Semafor1RightClickAttachedEvent = EventManager.RegisterRoutedEvent("Semafor1RightClickAttachedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Semafor1Control));


        private void RaiseSemafor1RightClickAttachedEvent()
        {
            //RoutedEventArgs newEventArgs = new RoutedEventArgs(Semafor1Control.Semafor1RightClickAttachedEvent, new SemaphoreEventArgs() { Name = "Right Click", sender = this });
            //RaiseEvent(newEventArgs);
        }

        public static void AddSemafor1RightClickAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ((UIElement)d).AddHandler(Semafor1Control.Semafor1RightClickAttachedEvent, handler);
        }

        public static void RemoveSemafor1RightClickAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ((UIElement)d).RemoveHandler(Semafor1Control.Semafor1RightClickAttachedEvent, handler);
        }


        #endregion ========== RoutedEvents =============

        /// <summary>
        /// nastavi mod-farbu semaforu
        /// </summary>
        /// <param name="mode"></param>
        public void SetMode(SEMAFOR1_MODE mode)
        {
            //if (App.AllowPathBySemaphoreClick == false)
            //{
            //    return;
            //}

//#if DEBUG_SEMAPHORE
//            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType).CustomInfo(string.Format("Semaphore '{0}' light mode changed from '{1}' to '{2}' [SetMode()].", this.Name, this.LightMode, mode));
//#endif
            LightMode = mode;
        }

        public SEMAFOR1_MODE PreviousLightMode
        {
            get { return (SEMAFOR1_MODE)GetValue(PreviousLightModeProperty); }
            set { SetValue(PreviousLightModeProperty, value); }
        }

        /// <summary>
        /// Hodnota LightMode pred nastavenim novej hodnoty
        /// </summary>
        public static readonly DependencyProperty PreviousLightModeProperty =
            DependencyProperty.Register("PreviousLightMode", typeof(SEMAFOR1_MODE), typeof(Semafor1Control), new PropertyMetadata(SEMAFOR1_MODE.NONINIT_STATE));


        public SEMAFOR1_MODE LightMode
        {
            get { return (SEMAFOR1_MODE)GetValue(LightModeProperty); }
            set
            {
                SetValue(LightModeProperty, value);
            }
        }


        /// <summary>
        /// Pre nastavenie svetiel na semafore, pri zmene vykresli prvky semaforu do prislusneho stavu;
        /// </summary>
        public static readonly DependencyProperty LightModeProperty =
            DependencyProperty.Register("LightMode", typeof(SEMAFOR1_MODE), typeof(Semafor1Control), new UIPropertyMetadata(SEMAFOR1_MODE.NONINIT_STATE, new PropertyChangedCallback(OnLightModeChanged)));

        public bool StartEndEnabled
        {
            get { return (bool)GetValue(StartEndEnabledProperty); }
            set { SetValue(StartEndEnabledProperty, value); }
        }

        /// <summary>
        /// priznak ci semafor sa moze pouzit na oznacenie zaciatku, alebo konca PCesty
        /// </summary>
        public static readonly DependencyProperty StartEndEnabledProperty =
            DependencyProperty.Register("StartEndEnabled", typeof(bool), typeof(Semafor1Control), new PropertyMetadata(false));




        public Path AttachedPath
        {
            get { return (Path)GetValue(AttachedPathProperty); }
            set { SetValue(AttachedPathProperty, value); }
        }

        /// <summary>
        /// priradeny usek k semaforu
        /// </summary>
        public static readonly DependencyProperty AttachedPathProperty =
            DependencyProperty.Register("AttachedPath", typeof(Path), typeof(Semafor1Control), new UIPropertyMetadata(null));



        public static string GetAttachedPathName(DependencyObject obj)
        {
            return (string)obj.GetValue(AttachedPathNameProperty);
        }

        public static void SetAttachedPathName(DependencyObject obj, string value)
        {
            obj.SetValue(AttachedPathNameProperty, value);
        }

        //MH: 09.11.2015 aby sme vedeli pri kliku na Semafor zistit, ktora Path patri k semaforu
        public static readonly DependencyProperty AttachedPathNameProperty =
            DependencyProperty.RegisterAttached("AttachedPathName", typeof(string), typeof(Semafor1Control), new PropertyMetadata(string.Empty));

        /*SEMAFOR_TYPE  pozri OSymbolochWindow.xaml
         * S1  stojanovy semafor: farby cervena, zelena
         * S2  Sexx trpaslici semafor - bez nozicky s podstavou: farby modra, biela
         * S3  trpaslici, farby ako S1
         * S4  stojanovy semafor farby ako Sexx
         */

        /// <summary>
        /// Pre nastavenie typu semaforu: S1, S2, S3
        /// </summary> 
        public static readonly DependencyProperty SemaphoreTypeProperty =
            DependencyProperty.Register("SemaphoreType", typeof(SEMAFOR_TYPE), typeof(Semafor1Control), new UIPropertyMetadata(SEMAFOR_TYPE.NONE, new PropertyChangedCallback(OnSemaphoreTypeChanged)));

        public SEMAFOR_TYPE SemaphoreType
        {
            get { return (SEMAFOR_TYPE)GetValue(SemaphoreTypeProperty); }
            set { SetValue(SemaphoreTypeProperty, value); }
        }
        // End ZK_NazVas


      

       

        /// <summary>
        /// Metoda spustana pri zmene typu semaforu zo stavu NO_INIT na iny. V tejto metode sa zviditelnuju resp. skryva nozicka a podstava...
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnSemaphoreTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Semafor1Control control = (Semafor1Control)d;
            SEMAFOR_TYPE newMode = (SEMAFOR_TYPE)e.NewValue;
            control.OnSemaphoreTypeChanged(e);
        }

        void OnSemaphoreTypeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;
            SEMAFOR_TYPE newMode = (SEMAFOR_TYPE)e.NewValue;

            //log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType).Info(string.Format("Semaphore mode changed from '{0}' to '{1}'.", control.LightMode, newMode));

            switch (newMode)
            {
                case SEMAFOR_TYPE.S1:  //stojanovy cervena/ zelena
                    if (Name.ToLower().StartsWith("pr"))//semafor pre Priecestie
                        PrevTopLightBrush = (SolidColorBrush)this.Resources["yellowLightBrush"];
                    else //klasicky semafor: zelena a cervena
                        PrevTopLightBrush = (SolidColorBrush)this.Resources["redLightBrush"];
                    Nozicka3.Visibility = Visibility.Hidden; //vodorovna podstava-Nozicka3
                    break;
                case SEMAFOR_TYPE.S2:  //Zriadovaci semafor Sexx, trpaslici bez nozicky len s podstavou
                    PrevBottomLightBrush = (SolidColorBrush)this.Resources["blueLightBrush"];
                    Nozicka1.Visibility = Visibility.Hidden;//zvisla nozicka-Nozicka1
                    Nozicka2.Visibility = Visibility.Hidden;//vodorovna podstava-Nozicka2
                    break;
                case SEMAFOR_TYPE.S3: //trpaslici s podstavou, farby ako S1
                    PrevBottomLightBrush = (SolidColorBrush)this.Resources["blueLightBrush"];
                    //Nozicka3.Visibility = Visibility.Hidden;//vodorovna podstava-Nozicka3
                    Nozicka1.Visibility = Visibility.Hidden;//zvisla nozicka-Nozicka1
                    Nozicka2.Visibility = Visibility.Hidden;//vodorovna podstava-Nozicka2
                    break;

                case SEMAFOR_TYPE.S4: //Zriadovaci semafor Sexx, ale stojanovy
                    PrevBottomLightBrush = (SolidColorBrush)this.Resources["blueLightBrush"];
                    Nozicka3.Visibility = Visibility.Hidden; //vodorovna podstava-Nozicka3
                    break;

            }
        }//OnSemaphoreTypeChanged

        private static void OnLightModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Semafor1Control control = (Semafor1Control)d;
            SEMAFOR1_MODE previousMode = (SEMAFOR1_MODE)e.OldValue;
            SEMAFOR1_MODE newMode = (SEMAFOR1_MODE)e.NewValue;

            control.PreviousLightMode = previousMode;

//#if DEBUG_SEMAPHORE
//            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType).CustomInfo(string.Format("Semaphore '{0}' light mode changed from '{1}' to '{2}' [OnLightModeChanged()]. {3}", control.Name, prewMode, newMode, control.CanSemaphoreResetToNormal));
//#endif
            //author: V.Nazmudinov  switch (previousMode)...
            //MH: nastavenie control.PrevTopLightBrush a control.PrevBottomLightBrush
            //tak by to nefungovalo???
            control.PrevTopLightBrush = control.TopLight.Fill as SolidColorBrush;
            control.PrevBottomLightBrush = control.BottomLight.Fill as SolidColorBrush;

            //---------
            /*
            switch (previousMode)
            {
                case SEMAFOR1_MODE.STOJ:
                    if( (control.SemaphoreType == SEMAFOR_TYPE.S1) || (control.SemaphoreType == SEMAFOR_TYPE.S3))
                    {
                        if (control.Name.ToLower().StartsWith("pr"))//semafor pre Priecestie
                        {
                            control.PrevTopLightBrush = (SolidColorBrush)control.Resources["yellowLightBrush"];
                        }
                        else  //klasicky semafor
                            control.PrevTopLightBrush = (SolidColorBrush)control.Resources["redLightBrush"];
                        control.PrevBottomLightBrush = (SolidColorBrush)control.Resources["lightOffBrush"];
                    }
                    else
                    {
                        control.PrevBottomLightBrush = (SolidColorBrush)control.Resources["blueLightBrush"];
                        control.PrevTopLightBrush = (SolidColorBrush)control.Resources["lightOffBrush"];
                    }
                    break;
                case SEMAFOR1_MODE.VOLNO:
                    if ((control.SemaphoreType == SEMAFOR_TYPE.S1) || (control.SemaphoreType == SEMAFOR_TYPE.S3))
                    {
                        control.PrevTopLightBrush = (SolidColorBrush)control.Resources["lightOffBrush"];
                        control.PrevBottomLightBrush = (SolidColorBrush)control.Resources["greenLightBrush"];
                    }
                    else
                    {
                        control.PrevBottomLightBrush = (SolidColorBrush)control.Resources["lightOffBrush"];
                        control.PrevTopLightBrush = (SolidColorBrush)control.Resources["whiteLightBrush"];
                    }
                    break;
                case SEMAFOR1_MODE.POSUN_NEZABEZP_DOVOLENY:
                    if ((control.SemaphoreType == SEMAFOR_TYPE.S1) || (control.SemaphoreType == SEMAFOR_TYPE.S3))
                    {
                        if (control.Name.ToLower().StartsWith("pr"))//semafor pre Priecestie
                        {
                            control.PrevTopLightBrush = (SolidColorBrush)control.Resources["yellowLightBrush"];
                        }
                        else
                            control.PrevTopLightBrush = (SolidColorBrush)control.Resources["redLightBrush"];
                        control.PrevBottomLightBrush = (SolidColorBrush)control.Resources["whiteLightBrush"];
                    }
                    else
                    {
                        control.PrevBottomLightBrush = (SolidColorBrush)control.Resources["blueLightBrush"];
                        control.PrevTopLightBrush = (SolidColorBrush)control.Resources["whiteLightBrush"];
                    }

                    break;
                case SEMAFOR1_MODE.VYLUKA_NAVESTIDLA:
                    control.PrevTopLightBrush = (SolidColorBrush)control.Resources["lightOffBrush"];
                    control.PrevBottomLightBrush = (SolidColorBrush)control.Resources["lightOffBrush"];
                    break;
                case SEMAFOR1_MODE.PORUCHA_NAVESTIDLA:
                    break;
                case SEMAFOR1_MODE.PORUCHA_POVOLOVACEJ_NAVESTI:
                    break;
                // ZK_NazVas
                //case SEMAFOR1_MODE.START_END_PART_OF_WAY:

                //    break;
                default:
                    break;
            }
            */

            /*semafor blika ak je v jednom zo stavov:
            SEMAFOR1_MODE.PORUCHA_POVOLOVACEJ_NAVESTI alebo
            SEMAFOR1_MODE.PORUCHA_NAVESTIDLA alebo
            SEMAFOR1_MODE.PRIVOLAVACIA_NAVEST
            */
            switch (newMode)
            {
                case SEMAFOR1_MODE.NONINIT_STATE:
                    control.SetNonInitState();
                    break;
                case SEMAFOR1_MODE.STOJ://stav=0x0001
                    control.SetStoj();
                    break;
                case SEMAFOR1_MODE.VOLNO://stav=0x0002
                    control.SetVolno();
                    break;
                case SEMAFOR1_MODE.POSUN_NEZABEZP_DOVOLENY:
                    control.SetPosunNezabezpecenyDovoleny();
                    break;
                case SEMAFOR1_MODE.VYLUKA_NAVESTIDLA:
                    control.SetVylukaNavestidla();
                    break;
                case SEMAFOR1_MODE.VYLUKA_CERVENA:
                    control.SetVylukaCervena();
                    break;
                case SEMAFOR1_MODE.VYLUKA_ZELENA:
                    control.SetVylukaZelena();
                    break;
                case SEMAFOR1_MODE.VYLUKA_BIELA:
                    control.SetVylukaPosunNezabezpecenyDovoleny();
                    break;
                case SEMAFOR1_MODE.PORUCHA_NAVESTIDLA://periodicke prefarbovanie, pozri OnTimerFlagChanged
                    break;
                case SEMAFOR1_MODE.PORUCHA_POVOLOVACEJ_NAVESTI://periodicke prefarbovanie, pozri OnTimerFlagChanged
                    break;
                case SEMAFOR1_MODE.PRECHODOVY_STAV:
                    control.SetPrechodovyStav();
                    break;
                case SEMAFOR1_MODE.ZLTY_STAV:
                    control.SetZltyStav();
                    break;
                case SEMAFOR1_MODE.PRIVOLAVACIA_NAVEST://periodicke prefarbovanie, pozri OnTimerFlagChanged
                    break;
                // ZK_NazVas
                case SEMAFOR1_MODE.START_END_PART_OF_WAY:
                    control.CanSemaphoreResetToNormal = false;
                    control.SetStartEndPartOfWay();//zaciatkom alebo koncom posunovej cesty môžu byt aj návestidlá Se9 a Se11
                    break;
                //case SEMAFOR1_MODE.UNSELECTED:
                //    control.SetUnselectedState();
                //    CanSemaphoreResetToNormal = true;
                //    break;
                default:
                    break;
            }
        }//OnLightModeChanged

        /*
         * Klik na semafor je povoleny:
         * - pre zaciatok PCesty, koniec PCesty, povolenie PCesty a povolenie VCesty;
         * - pri simulacii poruchy, pri odstraneni poruchy
         * Len semafory Se9, Se11 maju nastavenu DP StartEndEnabled=True, pozri UC_MapaStavadla.xaml;
         */
        /// <summary>
        /// klik na semafor, vyfarbenie semaforu pre mod START_END_PART_OF_WAY  sa urobi az po prijme spravy "OK" zo Servera
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SemaforLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string name = this.Name;
            RaiseSemafor1LeftClickAttachedEvent();
        }

        // if (globalData.CurrentMenuMode == CURRENT_MENU_MODE.PRIVOLAVACIA_NAVEST) z menu bola vybrata funkcia Udrzba->Privolavacia navest
        /// <summary>
        /// pravy klik na semafor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SemaforRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine(name);
            //if (globalData.CurrentMenuMode == CURRENT_MENU_MODE.PRIVOLAVACIA_NAVEST)
            //    RaiseSemafor1RightClickAttachedEvent(); //odpalenie Routed eventu
        }

        public void SetPoruchovyStavCervenaTick()
        {
            if ((SemaphoreType == SEMAFOR_TYPE.S1) || (SemaphoreType == SEMAFOR_TYPE.S3))
                TopLight.Fill = this.Resources["redLightBrush"] as SolidColorBrush;//horne svetlo
            else
                BottomLight.Fill = this.Resources["blueLightBrush"] as SolidColorBrush;//dolne svetlo
        }

        public void SetPoruchovyStavTick()
        {
            mainBorder.BorderBrush = this.Resources["redLightBrush"] as SolidColorBrush;
            TopLight.Stroke    = this.Resources["redLightBrush"] as SolidColorBrush;
            BottomLight.Stroke = this.Resources["redLightBrush"] as SolidColorBrush;
            TopLight.Fill    = this.Resources["lightOffBrush"] as SolidColorBrush;
            BottomLight.Fill = this.Resources["lightOffBrush"] as SolidColorBrush;
            Nozicka1.Stroke  = this.Resources["redLightBrush"] as SolidColorBrush;
            Nozicka2.Stroke  = this.Resources["redLightBrush"] as SolidColorBrush;
            Nozicka3.Stroke  = this.Resources["redLightBrush"] as SolidColorBrush;
            Nozicka1.Fill = this.Resources["redLightBrush"] as SolidColorBrush;
            Nozicka2.Fill = this.Resources["redLightBrush"] as SolidColorBrush;
            Nozicka3.Fill = this.Resources["redLightBrush"] as SolidColorBrush;
        }

        

        public void SetPoruchovyStavBlick()
        {
            mainBorder.BorderBrush = this.Resources["silverRedBrush"] as SolidColorBrush;
            TopLight.Stroke = this.Resources["silverRedBrush"] as SolidColorBrush;
            BottomLight.Stroke = this.Resources["silverRedBrush"] as SolidColorBrush;
            TopLight.Fill = this.Resources["silverSilverRedBrush"] as SolidColorBrush;
            BottomLight.Fill = this.Resources["silverSilverRedBrush"] as SolidColorBrush;
            Nozicka1.Stroke = this.Resources["silverRedBrush"] as SolidColorBrush;
            Nozicka2.Stroke = this.Resources["silverRedBrush"] as SolidColorBrush;
            Nozicka3.Stroke = this.Resources["silverRedBrush"] as SolidColorBrush;
            Nozicka1.Fill = this.Resources["silverRedBrush"] as SolidColorBrush;
            Nozicka2.Fill = this.Resources["silverRedBrush"] as SolidColorBrush;
            Nozicka3.Fill = this.Resources["silverRedBrush"] as SolidColorBrush;

            SetOpacity(1);
        }

        public void SetPoruchovyStavCervenaBlick()
        {
            if( (SemaphoreType == SEMAFOR_TYPE.S1) || (SemaphoreType == SEMAFOR_TYPE.S3) )
                TopLight.Fill = this.Resources["SilverRedRedBrush"] as SolidColorBrush;
            else
                TopLight.Fill = this.Resources["SilverBlueBlueBrush"] as SolidColorBrush;
        }


        /// <summary>
        /// Vsetky graficke prvky semaforu nastavi na blackBrush;
        /// Svetla budu vypnute!!
        /// </summary>
        private void SetUnselectedState()
        {
            TopLight.Fill = this.Resources["lightOffBrush"] as SolidColorBrush;
            BottomLight.Fill = this.Resources["lightOffBrush"] as SolidColorBrush;
            mainBorder.BorderBrush = this.Resources["blackBrush"] as SolidColorBrush;
            TopLight.Stroke = this.Resources["blackBrush"] as SolidColorBrush;
            BottomLight.Stroke = this.Resources["blackBrush"] as SolidColorBrush;
            Nozicka1.Stroke = this.Resources["blackBrush"] as SolidColorBrush;
            Nozicka2.Stroke = this.Resources["blackBrush"] as SolidColorBrush;
            Nozicka3.Stroke = this.Resources["blackBrush"] as SolidColorBrush;
            Nozicka1.Fill = this.Resources["blackBrush"] as SolidColorBrush;
            Nozicka2.Fill = this.Resources["blackBrush"] as SolidColorBrush;
            Nozicka3.Fill = this.Resources["blackBrush"] as SolidColorBrush;
            SetOpacity(1);
        }

        public void SetStatusThrowNormal(SEMAFOR1_MODE mode)
        {
            SetNormalStav();
            CanSemaphoreResetToNormal = true;
            //SetMode(mode);

            //if (mode == SEMAFOR1_MODE.VOLNO && this.Name.StartsWith("Se"))
            //    OnLightModeChanged(this, new DependencyPropertyChangedEventArgs(Semafor1Control.LightModeProperty, this.LightMode, SEMAFOR1_MODE.POSUN_NEZABEZP_DOVOLENY));
            //else
            OnLightModeChanged(this, new DependencyPropertyChangedEventArgs(Semafor1Control.LightModeProperty, this.LightMode, mode));
        }

        /// <summary>
        /// Vsetky graficke prvky semaforu nastavi na blackBrush;
        /// Svetla budu vypnute!!
        /// </summary>
        private void SetNormalStav()
        {
            mainBorder.BorderBrush = this.Resources["blackBrush"] as SolidColorBrush;
            TopLight.Stroke = this.Resources["blackBrush"] as SolidColorBrush;
            TopLight.Fill = this.Resources["lightOffBrush"] as SolidColorBrush;
            BottomLight.Stroke = this.Resources["blackBrush"] as SolidColorBrush;
            BottomLight.Fill = this.Resources["lightOffBrush"] as SolidColorBrush;

            Nozicka1.Stroke = this.Resources["blackBrush"] as SolidColorBrush;
            Nozicka2.Stroke = this.Resources["blackBrush"] as SolidColorBrush;
            Nozicka3.Stroke = this.Resources["blackBrush"] as SolidColorBrush;
            Nozicka1.Fill = this.Resources["blackBrush"] as SolidColorBrush;
            Nozicka2.Fill = this.Resources["blackBrush"] as SolidColorBrush;
            Nozicka3.Fill = this.Resources["blackBrush"] as SolidColorBrush;
            SetOpacity(1);
        }

        /// <summary>
        /// Vsetky graficke prvky semaforu nastavi na whiteLightBrush: #FFFFFFFF
        /// Nezmeni stav svetiel!!
        /// </summary>
        private void SetStartEndPartOfWay() //ak semafor je oznaceny ako zaciatok, alebo koniec PCesty
        {
            mainBorder.BorderBrush = this.Resources["whiteLightBrush"] as SolidColorBrush;
            TopLight.Stroke = this.Resources["whiteLightBrush"] as SolidColorBrush;
            BottomLight.Stroke = this.Resources["whiteLightBrush"] as SolidColorBrush;
            Nozicka1.Stroke = this.Resources["whiteLightBrush"] as SolidColorBrush;
            Nozicka2.Stroke = this.Resources["whiteLightBrush"] as SolidColorBrush;
            Nozicka3.Stroke = this.Resources["whiteLightBrush"] as SolidColorBrush;
            Nozicka1.Fill = this.Resources["whiteLightBrush"] as SolidColorBrush;
            Nozicka2.Fill = this.Resources["whiteLightBrush"] as SolidColorBrush;
            Nozicka3.Fill = this.Resources["whiteLightBrush"] as SolidColorBrush;
            SetOpacity(1);
        }

        /// <summary>
        /// Nastavi opacity pre jednotlive casti semaforu
        /// </summary>
        /// <param name="opacity"></param>
        public void SetOpacity(double opacity)
        {
            TopLight.Opacity = opacity;// ellipse TopLight horne svetlo
            BottomLight.Opacity = opacity;  //ellipse BottomLight dolne svetlo
            if (opacity == 1)
            {
                mainBorder.Opacity = Nozicka1.Opacity = Nozicka2.Opacity = Nozicka3.Opacity = opacity;
            }
        }

        public void SetStoj()
        {
            // ZK_NazVas pridany test ci sa ma zrusit oramovanie na bielo
            //if (CanSemaphoreResetToNormal)
                SetNormalStav();
            //else
            //    SetUnselectedState();

            if( (SemaphoreType == SEMAFOR_TYPE.S1) || (SemaphoreType == SEMAFOR_TYPE.S3) )
            {
                //if (this.Name.ToLower().StartsWith("pr"))  //semafor pre priecestie
                //    TopLight.Fill = this.Resources["yellowLightBrush"] as SolidColorBrush;
                //else
                    TopLight.Fill = this.Resources["redLightBrush"] as SolidColorBrush;
            }
            else
            {
                BottomLight.Fill = this.Resources["blueLightBrush"] as SolidColorBrush;
            }

        }

        public void SetVolno()
        {
            // ZK_NazVas pridany test ci sa ma zrusit oramovanie na bielo
            if (CanSemaphoreResetToNormal)
                SetNormalStav();
            else
                SetUnselectedState();

            if ((SemaphoreType == SEMAFOR_TYPE.S1) || (SemaphoreType == SEMAFOR_TYPE.S3))
            {
                BottomLight.Fill = this.Resources["greenLightBrush"] as SolidColorBrush;
            }
            else
            {
                TopLight.Fill = this.Resources["whiteLightBrush"] as SolidColorBrush;
            }
        }

        public void SetPosunNezabezpecenyDovoleny()
        {
            // ZK_NazVas pridany test ci sa ma zrusit oramovanie na bielo
            if (CanSemaphoreResetToNormal)
                SetNormalStav();
            else
                SetUnselectedState();

            if ((SemaphoreType == SEMAFOR_TYPE.S1) || (SemaphoreType == SEMAFOR_TYPE.S3))
            {
                if (this.Name.ToLower().StartsWith("pr"))
                    TopLight.Fill = this.Resources["yellowLightBrush"] as SolidColorBrush;
                else
                    TopLight.Fill = this.Resources["redLightBrush"] as SolidColorBrush;
                BottomLight.Fill = this.Resources["whiteLightBrush"] as SolidColorBrush;
            }
            else if (SemaphoreType == SEMAFOR_TYPE.S2) //S2
            {
                BottomLight.Fill = this.Resources["lightOffBrush"] as SolidColorBrush;//spodne  svetlo
                TopLight.Fill = this.Resources["whiteLightBrush"] as SolidColorBrush; //horne svetlo
            }
            else if(SemaphoreType == SEMAFOR_TYPE.S4)
            {
                BottomLight.Fill = this.Resources["lightOffBrush"] as SolidColorBrush;//spodne  svetlo
                TopLight.Fill = this.Resources["whiteLightBrush"] as SolidColorBrush; //horne svetlo
            }
        }

        public void SetVylukaCervena()
        {
            SetStoj();
            
            SetOpacity(errorOpacity);
            mainBorder.Opacity = Nozicka1.Opacity = Nozicka2.Opacity = Nozicka3.Opacity = errorOpacity;
        }

        public void SetVylukaZelena()
        {
            SetVolno();
           
            SetOpacity(errorOpacity);
            mainBorder.Opacity = Nozicka1.Opacity = Nozicka2.Opacity = errorOpacity;
        }

        public void SetVylukaPosunNezabezpecenyDovoleny()
        {
            SetPosunNezabezpecenyDovoleny();
           
            SetOpacity(errorOpacity);
            mainBorder.Opacity = Nozicka1.Opacity = Nozicka2.Opacity = errorOpacity;
        }

        public void SetVylukaNavestidla()
        {
            SetNormalStav();
        }

        public void SetPrechodovyStav()
        {
            // ZK_NazVas pridany test ci sa ma zrusit oramovanie na bielo
            if (CanSemaphoreResetToNormal)
                SetNormalStav();
            else
                SetUnselectedState();

            TopLight.Fill = PrevTopLightBrush;
            BottomLight.Fill = PrevBottomLightBrush;
            
            SetOpacity(errorOpacity);
            mainBorder.Opacity = Nozicka1.Opacity = Nozicka2.Opacity = Nozicka3.Opacity = errorOpacity;
        }

        public void SetZltyStav()
        {
            // ZK_NazVas pridany test ci sa ma zrusit oramovanie na bielo
            if (CanSemaphoreResetToNormal)
                SetNormalStav();
            else
                SetUnselectedState();

            TopLight.Fill = this.Resources["yellowBrush"] as SolidColorBrush;
            BottomLight.Fill = this.Resources["yellowBrush"] as SolidColorBrush;
        }

        /// <summary>
        /// zobrazi Semafor v neinicializovanom stave
        /// </summary>
        public void SetNonInitState()
        {
            // ZK_NazVas pridany test ci sa ma zrusit oramovanie na bielo
            if (CanSemaphoreResetToNormal)
                SetNormalStav();
            else
                SetUnselectedState();

            TopLight.Fill = this.Resources["blueLightBrush"] as SolidColorBrush;
            BottomLight.Fill = this.Resources["blueLightBrush"] as SolidColorBrush;
            SetOpacity(0.4);
        }

        public void SetPrivolavaciaNavestTick()
        {
            // SetPosunNezabezpecenyDovoleny();
            if ((SemaphoreType == SEMAFOR_TYPE.S1) || (SemaphoreType == SEMAFOR_TYPE.S3))
            {
                TopLight.Fill = this.Resources["redLightBrush"] as SolidColorBrush;
                BottomLight.Fill = this.Resources["whiteLightBrush"] as SolidColorBrush;
            }
            else  //S2,S4
            {
                BottomLight.Fill = this.Resources["lightOffBrush"] as SolidColorBrush;//spodne  svetlo
                TopLight.Fill = this.Resources["whiteLightBrush"] as SolidColorBrush; //horne svetlo
            }
        }

        public void SetPrivolavaciaNavestBlick()
        {
            // SetStoj();
            // ZK_NazVas pridany test ci sa ma zrusit oramovanie na bielo
            if (CanSemaphoreResetToNormal)
                SetNormalStav();
            else
                SetUnselectedState();

            if ((SemaphoreType == SEMAFOR_TYPE.S1) || (SemaphoreType == SEMAFOR_TYPE.S3))
            {
                TopLight.Fill = this.Resources["redLightBrush"] as SolidColorBrush;
            }
            else //S2,S4
            {
                BottomLight.Fill = this.Resources["lightOffBrush"] as SolidColorBrush;
            }

        }

        private void ucSemafor_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        //--Test MH: september 2018--



        #region ===PrvokStavadla: SemaforData ===
        /*
    public PrvokStavadla SemaforData
    {
        get { return (PrvokStavadla)GetValue(SemaforDataProperty); }
        set { SetValue(SemaforDataProperty, value); }
    }

    public static readonly DependencyProperty SemaforDataProperty =
        DependencyProperty.Register("SemaforData", typeof(PrvokStavadla), typeof(Semafor1Control),
            new PropertyMetadata(new PrvokStavadla() {Nazov="x",Podtyp='-', Stav=0, Uvolizol=0, Vyluka=0  }, OnSemaforDataChanged ));

    private static void OnSemaforDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        Semafor1Control semafor = d as Semafor1Control;
        semafor.OnSemaforDataChanged(e);
    }

    void OnSemaforDataChanged(DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue == null)
            return;
        PrvokStavadla prvok = (PrvokStavadla)e.NewValue;  //prvok.podtyp ='N'

        if (prvok.Podtyp != 'N')
            return;

        BitArray bityStav = new BitArray(new int[] { prvok.Stav });
        Boolean porucha, prechodovyStav, privolNavest, zlta2x, obsKO, biela, zelena, cervena;
        cervena = bityStav[0];
        zelena = bityStav[1];
        biela = bityStav[2];
        obsKO = bityStav[3];//obsadeny kolajovy obvod

        zlta2x = bityStav[4];
        privolNavest = bityStav[5];
        prechodovyStav = bityStav[6];
        porucha = bityStav[7];

        BitArray bityVyluka = new BitArray(new int[] { prvok.Vyluka });
        Boolean voVyluke = bityVyluka[0];

        if (voVyluke)
        {
            if (cervena)
            {
                //lightModeInfo.SetValue(stavadloElementsData, SEMAFOR1_MODE.VYLUKA_CERVENA, null);
                LightMode = SEMAFOR1_MODE.VYLUKA_CERVENA;
            }
            if (zelena)
            {
                //lightModeInfo.SetValue(stavadloElementsData, SEMAFOR1_MODE.VYLUKA_ZELENA, null);
                LightMode = SEMAFOR1_MODE.VYLUKA_ZELENA;
            }
            if (biela)
            {
                //lightModeInfo.SetValue(stavadloElementsData, SEMAFOR1_MODE.VYLUKA_BIELA, null);
                LightMode = SEMAFOR1_MODE.VYLUKA_BIELA;
            }

            if (!porucha)
                return;
        }


        // System.Diagnostics.Debug.WriteLine("Semafor: " + semaforData.Nazov + " nastavujem na: " + semaforData.stav);
        //MH: 21.11.2013 zmena priority signalov od najnizsej po najvyssiu
        if (cervena == true)
        {
            //lightModeInfo.SetValue(stavadloElementsData, SEMAFOR1_MODE.STOJ, null);
            LightMode = SEMAFOR1_MODE.STOJ;
        }
        if (zelena == true)
        {
            //lightModeInfo.SetValue(stavadloElementsData, SEMAFOR1_MODE.VOLNO, null);
            LightMode = SEMAFOR1_MODE.VOLNO;
        }

        if (biela && cervena)//MH: test 21.11.2013
        {
            //lightModeInfo.SetValue(stavadloElementsData, SEMAFOR1_MODE.POSUN_NEZABEZP_DOVOLENY, null);
            LightMode = SEMAFOR1_MODE.POSUN_NEZABEZP_DOVOLENY;
        }


        if ((zlta2x == true) && (zelena == true))//MH 08.10.2013
        {
            //lightModeInfo.SetValue(stavadloElementsData, SEMAFOR1_MODE.ZLTY_STAV, null);
            LightMode = SEMAFOR1_MODE.ZLTY_STAV;
        }
        if (privolNavest == true)
        {
            //lightModeInfo.SetValue(stavadloElementsData, SEMAFOR1_MODE.PRIVOLAVACIA_NAVEST, null);
            LightMode = SEMAFOR1_MODE.PRIVOLAVACIA_NAVEST;
        }
        if (prechodovyStav == true)
        {
            //lightModeInfo.SetValue(stavadloElementsData, SEMAFOR1_MODE.PRECHODOVY_STAV, null);
            LightMode = SEMAFOR1_MODE.PRECHODOVY_STAV;
        }

        if (porucha == true && cervena)
        {
            //lightModeInfo.SetValue(stavadloElementsData, SEMAFOR1_MODE.PORUCHA_POVOLOVACEJ_NAVESTI, null);
            LightMode = SEMAFOR1_MODE.PORUCHA_POVOLOVACEJ_NAVESTI;
        }

        if (porucha == true && !cervena)
        {
            //lightModeInfo.SetValue(stavadloElementsData, SEMAFOR1_MODE.PORUCHA_NAVESTIDLA, null);
            LightMode = SEMAFOR1_MODE.PORUCHA_NAVESTIDLA;
        }
    }//OnSemaforDataChanged
    */
        #endregion ===PrvokStavadla: SemaforData ===

        public bool EnableErrorSound
        {
            get { return (bool)GetValue(EnableErrorSoundProperty); }
            set { SetValue(EnableErrorSoundProperty, value); }
        }

        /// <summary>
        /// Priznak ci pri chybovom stave sa ma spustat zvukove znamenie
        /// </summary>
        public static readonly DependencyProperty EnableErrorSoundProperty =
            DependencyProperty.Register("EnableErrorSound", typeof(bool), typeof(Semafor1Control), new PropertyMetadata(true));



        /*
         semafor blika ak je v stave:
         SEMAFOR1_MODE.PORUCHA_POVOLOVACEJ_NAVESTI alebo
         SEMAFOR1_MODE.PORUCHA_NAVESTIDLA alebo
         SEMAFOR1_MODE.PRIVOLAVACIA_NAVEST
        */
        public bool TimerFlag
        {
            get { return (bool)GetValue(TimerFlagProperty); }
            set { SetValue(TimerFlagProperty, value); }
        }

        // TimerFlag je bindovany na property TimerFlag vo view modeli,  hodnotu property TimerFlag vo view modeli nastavuje 1 sekundovy timer na true/false
        public static readonly DependencyProperty TimerFlagProperty =
            DependencyProperty.Register("TimerFlag", typeof(bool), typeof(Semafor1Control), new PropertyMetadata(false, OnTimerFlagChanged));

        private static void OnTimerFlagChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Semafor1Control semafor = d as Semafor1Control;
            semafor.OnTimerFlagChanged(e);
        }

        void OnTimerFlagChanged( DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;
            bool timerFlag = (bool)e.NewValue;

            //if (Name=="Se9")
            //System.Diagnostics.Debug.WriteLine($"OnTimerFlagChanged: {timerFlag}  {this.Name} {LightMode}-{DateTime.Now:HH:mm:ss.fff}");

            if (timerFlag == false)//normalny chybovy stav; farby su cervene a cierne
            {
                switch (LightMode )
                {
                    case SEMAFOR1_MODE.PORUCHA_POVOLOVACEJ_NAVESTI:
                        SetPoruchovyStavTick();       //vyfarbenie elementov semaforu
                        SetPoruchovyStavCervenaTick();//vyfarbenie svetiel
                        break;
                    case SEMAFOR1_MODE.PORUCHA_NAVESTIDLA:
                        SetPoruchovyStavTick();
                        SetPoruchovyStavCervenaTick();//vyfarbenie svetiel, MH: 07.05.2019
                        break;
                    case SEMAFOR1_MODE.PRIVOLAVACIA_NAVEST:
                        SetPrivolavaciaNavestTick();
                        break;
                    //case SEMAFOR1_MODE.VYLUKA_CERVENA://mh.18.02.2019 uz blika
                    //    SetPoruchovyStavTick();
                    //    SetPoruchovyStavCervenaTick();
                    //    break;
                    default: break;
                }     
            }
            else //zasiveny chybovy stav
            {
                switch (LightMode)
                {
                    case SEMAFOR1_MODE.PORUCHA_POVOLOVACEJ_NAVESTI:
                        SetPoruchovyStavBlick();      //vyfarbenie elementov semaforu
                        SetPoruchovyStavCervenaBlick();//vyfarbenie elementov semaforu
                        break;
                    case SEMAFOR1_MODE.PORUCHA_NAVESTIDLA:
                        SetPoruchovyStavBlick();
                        break;
                    case SEMAFOR1_MODE.PRIVOLAVACIA_NAVEST:
                        SetPrivolavaciaNavestBlick();
                        break;
                    //case SEMAFOR1_MODE.VYLUKA_CERVENA://mh.18.02.2019
                    //    SetPoruchovyStavBlick();
                    //    SetPoruchovyStavCervenaBlick();
                    //    break;
                    default: break;
                }
            }
        }//OnTimerFlagChanged



        #region --Int64 SemaforStatus --

        public Int64 SemaforStatus
        {
            get { return (Int64)GetValue(SemaforStatusProperty); }
            set { SetValue(SemaforStatusProperty, value); }
        }
        //Obsahuje kombinovany udaj z telegramu zlozeny z Int16-stav, Int16-uvolIzol, Int16-vyluka, Int16-typElementu
        public static readonly DependencyProperty SemaforStatusProperty =
            DependencyProperty.Register("SemaforStatus", typeof(Int64), typeof(Semafor1Control), new PropertyMetadata((Int64)0, OnSemaforStatusChanged)  );


        //spusta sa pri zapise do SemaforStatus
        private static void OnSemaforStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Semafor1Control semafor = d as Semafor1Control;
            semafor.OnSemaforStatusChanged(e);
        }

        private void OnSemaforStatusChanged(DependencyPropertyChangedEventArgs e)
        {
            //         stav     uvolIzol    vyluka  typElementu
            //bity:    63-48      47-32      31-16        15-0
            if (e.NewValue == null)//obsahuje stav, uvolIzol a vyluka, typElementu(podtyp)
                return;

            
        }//OnStatusDataChanged

        #endregion --Int64 SemaforStatus --


    }
}
