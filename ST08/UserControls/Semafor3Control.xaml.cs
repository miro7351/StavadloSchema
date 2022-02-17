using System;
using System.Windows;

using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;



namespace PA.Stavadlo.MH.UserControls
{
    //MH: August 2018
    

    public enum SEMAFOR3_MODE
    {
        NONINIT_STATE,
        VOLNO, //svieti zelena
        OBSADENY_KOLAJ_OBVOD, //svieti biela
        PORUCHA_NAVESTIDLA    //blikanie
    }



    /// <summary>
    /// Specialny semafor pre priecestie
    /// </summary>
    public partial class Semafor3Control : UserControl
    {
        //AppEventsInvoker appEventsInvoker;
        public Semafor3Control()
        {
            InitializeComponent();
            //appEventsInvoker = AppEventsInvoker.Instance;

            PreviousLightMode = SEMAFOR3_MODE.NONINIT_STATE;
            this.Loaded += Semafor3Control_Loaded;
        }

        private void Semafor3Control_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //Pre kazdy semafor mozeme nastavit v xaml binding pre DP TimerFlag: <controls:Semafor1Control x:Name="L808"  TimerFlag="{Binding TimerFlag, Mode=OneWay}".../>
            //Zmena  dependency property TimerFlag zabezpecuje prekreslenie semaforu, ak je v blikajucom stave.
            //Aby sme nepisali pre kazdy semafor binding pre DP TimerFag v xaml, binding nastavime tu v kode.
            //Je to vyhodne, lebo pre vsetky semafory DP TimerFlagProperty sa binduje na jeden zdroj v DataContexte!!!
            //{Binding TimerFlag,...} "TimerFlag" je meno property z DataContextu, z UC_StavadloViewModel
            //DataContext.TimerFlag sa nastavuje pomocou 1 sec. casovaca true/false, pouziva na to, aby vsetky semafory (vsetky kontroly ktore blikaju) blikali synchronizovane (naraz)!!

            Binding timerFlagBinding = new Binding("TimerFlag") { Mode = BindingMode.OneWay, Source = this.DataContext };
            BindingOperations.SetBinding(this, TimerFlagProperty, timerFlagBinding);
        }

        #region ---TimerFlag---

        /*
        semafor blika ak je v stave:
        SEMAFOR3_MODE.PORUCHA
       */
        public bool TimerFlag
        {
            get { return (bool)GetValue(TimerFlagProperty); }
            set { SetValue(TimerFlagProperty, value); }
        }

        // TimerFlag je bindovany na property TimerFlag vo view modeli,  hodnotu property TimerFlag vo view modeli nastavuje 1 sekundovy timer na true/false
        public static readonly DependencyProperty TimerFlagProperty =
            DependencyProperty.Register("TimerFlag", typeof(bool), typeof(Semafor3Control), new PropertyMetadata(false, OnTimerFlagChanged));

        private static void OnTimerFlagChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Semafor3Control semafor = d as Semafor3Control;
            semafor.OnTimerFlagChanged(e);
        }

        void OnTimerFlagChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;
            bool timerFlag = (bool)e.NewValue;

            if (timerFlag == false)//normalny chybovy stav; farby su cervene a cierne
            {
                switch (LightMode)
                {
                    case SEMAFOR3_MODE.PORUCHA_NAVESTIDLA:
                        SetPoruchovyStavTick();
                        break;
                    default: break;
                }
            }
            else //zasiveny chybovy stav
            {
                switch (LightMode)
                {
                    case SEMAFOR3_MODE.PORUCHA_NAVESTIDLA:
                        SetPoruchovyStavBlick();
                        //SetPoruchovyStavCervenaBlick();
                        break;
                   
                    default: break;
                }
            }
        }//OnTimerFlagChanged

        public void SetPoruchovyStavTick()
        {
            mainBorder.Background = this.Resources["redLightBrush"] as SolidColorBrush;
            BottomLight.Stroke = this.Resources["redLightBrush"] as SolidColorBrush;
            BottomLight.Fill = this.Resources["lightOffBrush"] as SolidColorBrush;
            TopLight.Stroke = this.Resources["redLightBrush"] as SolidColorBrush;
            TopLight.Fill = this.Resources["lightOffBrush"] as SolidColorBrush;
            
        }
        public void SetPoruchovyStavBlick()
        {
            mainBorder.Background = this.Resources["silverRedBrush"] as SolidColorBrush;
            BottomLight.Stroke = this.Resources["silverRedBrush"] as SolidColorBrush;
            BottomLight.Fill = this.Resources["silverSilverRedBrush"] as SolidColorBrush;
            TopLight.Stroke = this.Resources["silverRedBrush"] as SolidColorBrush;
            TopLight.Fill = this.Resources["silverSilverRedBrush"] as SolidColorBrush;
            SetOpacity(1);
        }

        #endregion --TimerFlag-----

        /// <summary>
        /// Mode pred aktualnym stavom
        /// </summary>
        public SEMAFOR3_MODE PreviousLightMode { get; set; }
        
        public SEMAFOR3_MODE LightMode
        {
            get { return (SEMAFOR3_MODE)GetValue(LightModeProperty); }
            set { SetValue(LightModeProperty, value); }
        }

        public static readonly DependencyProperty LightModeProperty =
            DependencyProperty.Register("LightMode", typeof(SEMAFOR3_MODE), typeof(Semafor3Control), new UIPropertyMetadata(SEMAFOR3_MODE.NONINIT_STATE, new PropertyChangedCallback(OnLightModeChanged)));

        private static void OnLightModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Semafor3Control control = (Semafor3Control)d;
            SEMAFOR3_MODE previousMode = (SEMAFOR3_MODE)e.OldValue;
            control.PreviousLightMode = previousMode;
            SEMAFOR3_MODE newMode = (SEMAFOR3_MODE)e.NewValue;

            switch (newMode)
            {
                case SEMAFOR3_MODE.NONINIT_STATE:
                    control.SetNonInitState();
                    break;
                case SEMAFOR3_MODE.VOLNO:
                    control.SetVolno();
                    break;
                case SEMAFOR3_MODE.OBSADENY_KOLAJ_OBVOD:
                    control.SetObsadeny_KolajObvod();
                    break;
                case SEMAFOR3_MODE.PORUCHA_NAVESTIDLA://pozri OnTimerFlagChanged(...)
                    break;
                default: break;
            }
        }

        public Int64 SemaforStatus
        {
            get { return (Int64)GetValue(SemaforStatusProperty); }
            set { SetValue(SemaforStatusProperty, value); }
        }
        //Obsahuje kombinovany udaj z telegramu zlozeny z Int16-stav, Int16-uvolIzol, Int16-vyluka, Int16-typElementu
        public static readonly DependencyProperty SemaforStatusProperty =
            DependencyProperty.Register("SemaforStatus", typeof(Int64), typeof(Semafor3Control), new PropertyMetadata((Int64)0, OnSemaforStatusChanged));


        //spusta sa pri zapise do SemaforStatus
        private static void OnSemaforStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Semafor3Control semafor = d as Semafor3Control;
            semafor.OnSemaforStatusChanged(e);
        }


        private void OnSemaforStatusChanged(DependencyPropertyChangedEventArgs e)
        {
            //obsahuje  stav     uvolIzol    vyluka  typElementu
            //bity:    63-48      47-32      31-16        15-0
            if (e.NewValue == null)//obsahuje stav, uvolIzol a vyluka, typElementu(podtyp)
                return;
            
        }//OnSemaforStatusChanged



        #region ---FUNKCIE---

        /// <summary>
        /// zobrazi Semafor v neinicializovanom stave
        /// </summary>
        public void SetNonInitState()//vypnuta ZELENA, BIELA
        {
            BottomLight.Fill = this.TryFindResource("lightOffBrush") as SolidColorBrush;
            TopLight.Fill = this.TryFindResource("lightOffBrush") as SolidColorBrush;
            //--MH: 10.05.2019
            mainBorder.Background = this.TryFindResource("backgroundBrush") as SolidColorBrush;
            mainBorder.BorderBrush = this.TryFindResource("blackBrush") as SolidColorBrush;
            BottomLight.Stroke = this.TryFindResource("blackBrush") as SolidColorBrush;
            TopLight.Stroke = this.TryFindResource("blackBrush") as SolidColorBrush;
            SetOpacity(0.4);
        }

        public void SetVolno()//svieti ZELENA, biela VYPNUTA
        {
            mainBorder.Background = this.TryFindResource("backgroundBrush") as SolidColorBrush;
            TopLight.Fill = this.TryFindResource("greenLightBrush") as SolidColorBrush;
            BottomLight.Fill = this.TryFindResource("lightOffBrush") as SolidColorBrush;

            //--10.05.2019
            mainBorder.BorderBrush = this.TryFindResource("blackBrush") as SolidColorBrush;
            BottomLight.Stroke = this.TryFindResource("blackBrush") as SolidColorBrush;
            TopLight.Stroke = this.TryFindResource("blackBrush") as SolidColorBrush;
            SetOpacity(1.0);
        }

        public void SetObsadeny_KolajObvod()// vypnuta ZELENA, svieti BIELA 
        {
            TopLight.Fill = this.TryFindResource("lightOffBrush") as SolidColorBrush;
            BottomLight.Fill = this.TryFindResource("whiteLightBrush") as SolidColorBrush;
            //--10.05.2019
            mainBorder.Background = this.TryFindResource("backgroundBrush") as SolidColorBrush; 
            mainBorder.BorderBrush = this.TryFindResource("blackBrush") as SolidColorBrush;
            BottomLight.Stroke = this.TryFindResource("blackBrush") as SolidColorBrush;
            TopLight.Stroke = this.TryFindResource("blackBrush") as SolidColorBrush;
            SetOpacity(1.0);
        }

        /// <summary>
        /// Nastavi opacity pre jednotlive casti semaforu
        /// </summary>
        /// <param name="opacity"></param>
        public void SetOpacity(double opacity)
        {
            BottomLight.Opacity = opacity;//ellipse BottomLight spodne svetlo
            TopLight.Opacity = opacity;  //ellipse TopLight horne svetlo
            //if (opacity == 1)
            {
                mainBorder.Opacity = opacity;
            }
        }

        #endregion --FUNKCIE---

        private void SemaforLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RaiseSemafor3LeftClickAttachedEvent();
        }

        // --------Lavy klik na semafor3--------------
        public static readonly RoutedEvent Semafor3LeftClickAttachedEvent = EventManager.RegisterRoutedEvent("Semafor3LeftClickAttachedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Semafor3Control));
        private void RaiseSemafor3LeftClickAttachedEvent()
        {
            //RoutedEventArgs newEventArgs = new RoutedEventArgs(Semafor3Control.Semafor3LeftClickAttachedEvent, new SemaphoreEventArgs() { Name = "Left Click", sender = this });
            //RaiseEvent(newEventArgs);
        }
        public static void AddSemafor3LeftClickAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ((UIElement)d).AddHandler(Semafor3Control.Semafor3LeftClickAttachedEvent, handler);
        }

        public static void RemoveSemafor3LeftClickAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ((UIElement)d).RemoveHandler(Semafor3Control.Semafor3LeftClickAttachedEvent, handler);
        }

        //----------------------------------------------------------------------------------------------

    }
}
