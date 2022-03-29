
using System;
using System.Windows;

using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;



namespace PA.Stavadlo.UserControls
{

    //Pozn: vyhybka, vyhybna: shunt; vykolajka:derailer 
    //MH: marec 2019
    /// <summary>
    /// User control pre vykolajku.
    /// Ak sa zvoli z menu polozka 'VYMENA' a klikne sa na vykolajku, ktora je v UNBLOCKED stave (do servera sa posle prislusny telegram), potom (server vrati v telegrame c.141 novy stav pre vykolajku) prejde do BLOCKED stavu a naopak.
    ///  Len DEBUG rezime sa da prehadzovat vykolajka: Menu 'Vymena' a klik na vykolajku....
    ///  pozri StavadloPage.xaml.cs: void DerailerClickAttachedEventHandler(...)
    ///  V RELEASE rezime sa vykolajka nastavuje len v telegrame c. 141.
    /// </summary>
    public partial class UC_Derailer : UserControl
    {

           /*Blikajuce stavy:
            * PORUCHA
            * PORUCHA_PRESTAVENIA
            * STRATA_DOHLIADANIA
            */

        public enum CLICKED_ARM
        {
            NONE,
            RAMENO_PLUS,    //ak je klik na TopArm
            RAMENO_MINUS    //ak je klik na BottomArm
        }

        public UC_Derailer()
        {
            InitializeComponent();
            //appEventsInvoker = AppEventsInvoker.Instance;

            this.Loaded += UC_Derailer_Loaded;
        }

        private void UC_Derailer_Loaded(object sender, RoutedEventArgs e)
        {
            //Pre kazdu vymenu nastavime binding pre DP TimerFlagProperty na property DataContext.TimerFlag
            //aby sme to nerobili v xaml pre kazdy TrainSwitch64 napr. <controls:TrainSwitch64 x:Name="V1124"  TimerFlag="{Binding TimerFlag, Mode=OneWay}".../>
            //Binding urobime v kode pred zobrazenim controlu, lebo pre vymenu DP TimerFlagProperty sa binduje na jeden zdroj v DataContexte!!!
            //"TimerFlag" je meno property z DataContextu

            Binding timerFlagBinding = new Binding("TimerFlag") { Mode = BindingMode.OneWay, Source = this.DataContext };
            BindingOperations.SetBinding(this, TimerFlagProperty, timerFlagBinding);
        }

        #region --- FIELDS----

        //AppEventsInvoker appEventsInvoker;//pre odpalovanie eventov
        private Path ClickedPath;         //path na ktoru sa kliklo
        public CLICKED_ARM ClickedArm { get; set; } //typ ramena vykolajky, na ktore sa kliklo

        #endregion -FIELDS ---


        #region --Dependency properties--

        public string Orientation
        {
            get { return (string)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        //orientacia vykolajky: "P" prava, "L"  lava
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(string), typeof(UC_Derailer), new PropertyMetadata("L"));


        public bool ZaverBezCestyDR
        {
            get { return (bool)GetValue(ZaverBezCestyDRProperty); }
            set { SetValue(ZaverBezCestyDRProperty, value); }
        }

        /// <summary>
        /// ak vyhybka ma zaver a nie je sucast cesty, potom ZaverBezCesty=true
        /// ak vyhybka ma zaver a je sucast cesty, potom ZaverBezCesty=false
        /// ak vyhybka nema zaver, potom ZaverBezCesty=false
        /// </summary>
        public static readonly DependencyProperty ZaverBezCestyDRProperty =
            DependencyProperty.Register("ZaverBezCestyDR", typeof(bool), typeof(BaseTrainSwitchControl), new PropertyMetadata(false));


        public Int64 DrStatus
        {
            get { return (Int64)GetValue(DrStatusProperty); }
            set { SetValue(DrStatusProperty, value); }
        }

        //DrStatus-Derailer Status, tu sa zapisuje udaj prijaty z telegramu
        //<controls:UC_Derailer x:Name="Vk998" Orientation="L" Margin="2624,105,0,0" State="NOTINITIALIZED" DrStatus="{Binding STC[Vk998].CombineStatus, Mode=OneWay}" />
        public static readonly DependencyProperty DrStatusProperty =
            DependencyProperty.Register("DrStatus", typeof(Int64), typeof(UC_Derailer), new PropertyMetadata((Int64)0, OnDrStatusChanged));

        private static void OnDrStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UC_Derailer ucd = d as UC_Derailer;
            if (ucd == null)
                return;
            ucd.OnDrStatusChanged(e);
        }

        bool vykolajkaVpolohePlus;

        /// <summary>
        /// spracovanie udaju z telegramu
        /// </summary>
        /// <param name="e"></param>
        void OnDrStatusChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;
            string name = Name;

           
        }

        #region --Derailer State --
        public DERAILER_STATE State
        {
            get { return (DERAILER_STATE)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        // Stav vykolajky, pri zapise do DP State sa vykreslia prislusne casti user controlu
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(DERAILER_STATE), typeof(UC_Derailer), new PropertyMetadata(DERAILER_STATE.NONE, OnStateChanged));

        private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UC_Derailer ucd = d as UC_Derailer;
            if (ucd == null)
                return;
            ucd.OnStateChanged(e);
        }

        void OnStateChanged(DependencyPropertyChangedEventArgs e)
        {
            DERAILER_STATE newState = (DERAILER_STATE)e.NewValue;
            //!! Visual Studio switch enum auto fill !!!
            //napis sw, stlac tab, tab, dostaneme
            //switch (switch_on)
            //{
            //    default:
            //}
            //switch_on prepisat na newState, stlacit Enter

           
        }

        #endregion --Derailer State --


        public bool EnableErrorSound
        {
            get { return (bool)GetValue(EnableErrorSoundProperty); }
            set { SetValue(EnableErrorSoundProperty, value); }
        }

        /// <summary>
        /// Priznak ci pri chybovom stave sa ma spustat zvukove znamenie
        /// </summary>
        public static readonly DependencyProperty EnableErrorSoundProperty =
            DependencyProperty.Register("EnableErrorSound", typeof(bool), typeof(UC_Derailer), new PropertyMetadata(true));


        #region ---TimerFlag--
        public bool TimerFlag
        {
            get { return (bool)GetValue(TimerFlagProperty); }
            set { SetValue(TimerFlagProperty, value); }
        }

        // TimerFlag sa pouziva na zobrazenie vymeny v blikajucom stave.
        //TimerFlag je bindovany na property  TimerFlag vo view modeli,  hodnotu pre TimerFlag nastavuje 1 sekundovy timer na true/false
        public static readonly DependencyProperty TimerFlagProperty =
            DependencyProperty.Register("TimerFlag", typeof(bool), typeof(UC_Derailer), new PropertyMetadata(false, OnTimerFlagChanged));

        private static void OnTimerFlagChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UC_Derailer vykolajka = d as UC_Derailer;
            vykolajka.OnTimerFlagChanged(e);
        }

        //spusta sa kazdu sekundu!!!
        /// <summary>
        /// Podla State meni farby pre blikajuci stav
        /// </summary>
        /// <param name="e"></param>
        void OnTimerFlagChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;
            bool timerFlag = (bool)e.NewValue;

            
        }//OnTimerFlagChanged

        #endregion-TimerFlag--

        #endregion --Dependency properties--

        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private new void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ClickedPath = null;
            ClickedArm = CLICKED_ARM.NONE;
            DoHitTest(sender, e);       //nastavi ClickedPath, ak sa kliklo na TopArm alebo BottomArm

            if (ClickedPath != null)
            {
                string name = ClickedPath.Name.ToLower();
                if (name.Contains("top"))
                {
                    ClickedArm = CLICKED_ARM.RAMENO_MINUS;
                }
                else if( name.Contains("bottom"))
                {
                    ClickedArm = CLICKED_ARM.RAMENO_PLUS;
                }
            }
            RaiseDerailerClickAttachedEvent();//AttachedEvent sa zachyti v StavadloPage.xaml.cs a tam sa spracuje
            // controls:UC_Derailer.DerailerClickAttachedEvent="TrainSwitchChangedModeAttachedEvent"
        }
        */
        //================== Attached event ========================//
        /// <summary>
        /// Attached event odpaleny po kliku na rameno vykolajky
        /// </summary>
        public static readonly RoutedEvent DerailerClickAttachedEvent = EventManager.RegisterRoutedEvent("DerailerClick", RoutingStrategy.Bubble, typeof(RoutedEventArgs), typeof(UC_Derailer)  );

        //odpaluje sa v ArmLeftMouseDown(...)
        void RaiseDerailerClickAttachedEvent()
        {
            RoutedEventArgs eventArgs = new RoutedEventArgs(UC_Derailer.DerailerClickAttachedEvent, new DerailerRoutedEventArgs() { sender = this, clickedArm = ClickedArm });
            RaiseEvent(eventArgs);
        }

        public static void AddDerailerClickAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ((UIElement)d).AddHandler(UC_Derailer.DerailerClickAttachedEvent, handler);
        }

        public static void RemoveDerailerClickAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ((UIElement)d).RemoveHandler(UC_Derailer.DerailerClickAttachedEvent, handler);
        }
        //=====================================================//

     
        /// <summary>
        /// Handler pre klik na rameno vykolajky
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ArmLeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            //Path clickedPath = sender as Path;
            //string pathName = clickedPath.Name;
            //ClickedArm = CLICKED_ARM.NONE;

            //string armType = clickedPath.GetValue(PathHelper.ArmTypeProperty) as String;//znak "+", alebo"-"
            //if (armType == "+")
            //    ClickedArm = CLICKED_ARM.RAMENO_PLUS;
            //else
            //    ClickedArm = CLICKED_ARM.RAMENO_MINUS;
            //RaiseDerailerClickAttachedEvent pouziva ClickedArm

            RaiseDerailerClickAttachedEvent();
            //AttachedEvent sa zachyti v StavadloPage.xaml.cs a tam sa spracuje; v RELEASE mode sa neda ovladat rucne, pozri DerailerClickAttachedEventHandler(...) v  StavadloPage.xaml.c

        }
    }

    /// <summary>
    /// Enum Typ pre urcenie stavu vykolajky.
    /// </summary>
    public enum DERAILER_STATE : int
    {
        NONE = 0,

        /// <summary>
        /// Neinicializovany stav. (Cierna farba)
        /// </summary>
        NOTINITIALIZED,

        //PLUS,

        //MINUS,

        /// <summary>
        /// Vykolajka je v stave, ked blokuje kolaj, je na kolaji (Cervena farba okraja)
        /// </summary>
        BLOCKED,

        /// <summary>
        /// Prechodovy stav, t.j. vykolajka prechadza z blokujucej na neblokujucu resp. naopak, modra farba okraja
        /// </summary>
        V_MANIPULACII,

        /// <summary>
        /// Vykolajka neblokuje cestu.Je nad kolajou, cierny okraj;
        /// </summary>
        UNBLOCKED,

        /// <summary>
        /// Porucha vykolajky, blikaju vsetky casti
        /// </summary>
        PORUCHA,

        /// <summary>
        /// Strata dohliadania, blikaju vsetky casti
        /// </summary>
        /// </summary>
        STRATA_DOHLIADANIA,

        /// <summary>
        /// je na kolaji a blika, siva/cervena
        /// </summary>
        PORUCHA_PRESTAVENIA,

        /// <summary>
        /// je na kolaji, okraj cierny vnutro zlte
        /// </summary>
        VYLUKA_PRESTAVENIA,

        /// <summary>
        /// je na kolaji, zlty okraj
        /// </summary>
        VYLUKA_UPLNA,

        ZAVER,
        /// <summary>
        /// 
        /// </summary>
        NORMAL
    }


    /// <summary>
    /// pre prenos parametra pre RoutedEvent DerailerClickAttachedEvent 
    /// </summary>
    class DerailerRoutedEventArgs : RoutedEventArgs
    {
        public object sender;
        public UC_Derailer.CLICKED_ARM clickedArm;
    }
}
