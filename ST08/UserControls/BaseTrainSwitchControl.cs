using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;


using PA.Stavadlo.MH.Enums;

using ST08;

namespace PA.Stavadlo.MH.UserControls
{
    
    /// <summary>
 /// pre prenos parametra pre RoutedEvent TrainSwitchChangedModeEvent
 /// </summary>
    class TrainSwitchRoutedEventArgs : RoutedEventArgs
    {
        public string myNewValue;
    }



    /// <summary>
    /// pre prenos parametra pre RoutedEvent TrainSwitchChangedModeAttachedEvent 
    /// </summary>
    class TrainSwitchRoutedEventArgs2 : RoutedEventArgs
    {
        public object sender;
        public string myNewValue;
        public BaseTrainSwitchControl.CLICKED_ARM clickedArm;
    }

    /// <summary>
    /// zakladna trieda pre user controls TrainSwitchXX
    /// </summary>
    public class BaseTrainSwitchControl : UserControl
    {
        //!!!!! POZRI constructor pre TrainSwitch64 v TrainSwitch64.xaml.cs   !!!!!!!!!!!!!!!!!

        #region ------Enums----------

        /// <summary>
        /// udava stav v ktorom sa vyhybka-vymena nachadza
        /// </summary>
        public enum SWITCH_STATE
        {
            NONE,
            NORMAL,

            UVOLNENA_IZOLACIA,
            OBSADENY_USEK,
            VLAKOVA_CESTA,
            POSUNOVA_CESTA,

            VYHYBKA_V_MANIPULACII,
            VYLUKA_PRESTAVENIA,
            UPLNA_VYLUKA,
            STRATA_DOHLIADANIA,
            NADPRUD,
            PORUCHA_PRESTAVENIA,
            ROZREZ,
            ODVRAT_VC,
            ODVRAT_PC,
            ODVRAT_VYLUKA_PC,
            ODVRAT_VYLUKA_VC,
            V_MANIPULACII_BEZ_POLOHY,        //prechodovy stav zo stavu plus do stavu minus - ak nie je znama poloha vymeny
            //MH: pridane 19.11.2013, tieto stavy by mali mat zebrovane rameno
            ODVRAT_VC_PRECHOD,
            ODVRAT_PC_PRECHOD,
            ODVRAT_VYLUKA_PC_PRECHOD,
            ODVRAT_VYLUKA_VC_PRECHOD,
            VARIANT                     //oznacenie ako variant pri stavani cesty (posunovej, vlakovej)
        }


        /// <summary>
        /// udava ktory smer na vyhybke nie je zapnuty(nie je prechodny), rameno vyhybky je zaciernene-vyplnene
        /// </summary>
        public enum INACTIVE_ARM //nebolo by lepsie VYPNUTE_RAMENO??
        {
            NONE = 0,
            RAMENO1,
            RAMENO2
        }


        /// <summary>
        /// popisuje suhlas pre obsluhu pre vymenu V801
        /// </summary>
        public enum SUHLAS_PRE_OBSLUHU_V801
        {
            NEUDELENY,
            UDELENY
        }

        public enum CLICKED_ARM
        {
            NONE,
            ROZREZ,
            RAMENO_PLUS,    //ak je klik na rameno 1 a invertArm == false;    ak je klik na rameno 2 a invertArm == true
            RAMENO_MINUS    //ak je klik na rameno 1 a invertArm == true;    ak je klik na rameno 2 a invertArm == false
        }


        #endregion --------Enums--------

        #region ====Constructor============

        public BaseTrainSwitchControl()
        {
            CurrentInactiveArm = INACTIVE_ARM.NONE;
            actualSwitchState = SWITCH_STATE.NORMAL;
            BlinkedStateEnabled = false;
            //appEventsInvoker = AppEventsInvoker.Instance;

            PreviousSwitchState = SWITCH_STATE.NONE;

            transparentBrush = App.Current.Resources["TransparentBrush"] as SolidColorBrush;
            blackBrush = App.Current.Resources["BlackBrush"] as SolidColorBrush;
            redBrush   = App.Current.Resources["RedBrush"] as SolidColorBrush;
            vyhybkaVManipulaciiBrush = App.Current.Resources["vyhybkaVManipulaciiBrush"] as SolidColorBrush;
            vylukaPrestaveniaBrush   = App.Current.Resources["vylukaPrestaveniaBrush"] as SolidColorBrush;
            posunCestaBrush = App.Current.Resources["posunCestaBrush"] as SolidColorBrush;
            vlakCestaBrush  = App.Current.Resources["vlakCestaBrush"] as SolidColorBrush;
            vyhybkaRozrezPoruchaBrush = App.Current.Resources["vyhybkaRozrezPoruchaBrush"] as SolidColorBrush;
            ramenoVyhybkyPoruchaBrush = App.Current.Resources["ramenoVyhybkyPoruchaBrush"] as SolidColorBrush;
            startEndPathBrush = App.Current.Resources["startEndPathBrush"] as SolidColorBrush;
            odvratVC_PrechodBrush = App.Current.Resources["odvratVC_PrechodBrush"] as VisualBrush;
            odvratPC_PrechodBrush = App.Current.Resources["odvratPC_PrechodBrush"] as VisualBrush;

            this.Loaded += BaseTrainSwitchControl_Loaded;
           
        }

        private void BaseTrainSwitchControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Pre kazdu vymenu nastavime binding pre DP TimerFlagProperty na property DataContext.TimerFlag
            //aby sme to nerobili v xaml pre kazdy TrainSwitchXX napr. <controls:TrainSwitch64 x:Name="V1124"  TimerFlag="{Binding TimerFlag, Mode=OneWay}".../>
            //Binding urobime v kode pred zobrazenim controlu, lebo pre vymenu DP TimerFlagProperty sa binduje na jeden zdroj v DataContexte!!!
            //"TimerFlag" je meno property z DataContextu

            Binding timerFlagBinding = new Binding("TimerFlag") { Mode = BindingMode.OneWay, Source = this.DataContext };
            BindingOperations.SetBinding(this, TimerFlagProperty, timerFlagBinding);

            //NastavPohotovostnyStav();
        }

        #endregion ====Constructor============


        #region --------Fields------------

        //AppEventsInvoker appEventsInvoker;

        protected string Path01Name;
        protected string Path02Name;

        protected Path Rameno1, Rameno2, Rozrez1, Rozrez2;
        protected Grid MainGrid;
        protected Grid VariantGrid; //grid ktory obsahuje obrysove ciary pre zobrazenie stavu variant
        //napr. TrainSwitch27   base.MainGrid = this.mainGrid27;
        //public Path Rameno1, Rameno2, Rozrez1, Rozrez2;

        protected Path filledRozrez, transparentRozrez, visibleRameno /*neprechodny smer, nezapnuty smer vyfarbeny cierno*/, inVisibleRameno /*priechodny smer,zapnuty smer, priehadny*/;
        protected Grid variantGrid;
       


        /// <summary>
        /// aktualny stav vyhybky
        /// </summary>
        private SWITCH_STATE actualSwitchState;

        /// <summary>
        /// rameno vyhybky v neprechodnom smere; je to zaciernene rameno
        /// </summary>
        private INACTIVE_ARM CurrentInactiveArm;

        public CLICKED_ARM ClickedArm { get; set; }
        private Path ClickedPath;

        SolidColorBrush transparentBrush; //App.Current.Resources["TransparentBrush"] as SolidColorBrush
        SolidColorBrush blackBrush;//App.Current.Resources["blackBrush"] as SolidColorBrush;
        SolidColorBrush redBrush;  //App.Current.Resources["redBrush"] as SolidColorBrush;
        SolidColorBrush vyhybkaVManipulaciiBrush;  //App.Current.Resources["vyhybkaVManipulaciiBrush"] as SolidColorBrush;
        SolidColorBrush vylukaPrestaveniaBrush;   //App.Current.Resources["vylukaPrestaveniaBrush"] as SolidColorBrush;
        SolidColorBrush posunCestaBrush;//App.Current.Resources["posunCestaBrush"] as SolidColorBrush;
        SolidColorBrush vlakCestaBrush;//App.Current.Resources["vlakCestaBrush"] as SolidColorBrush;
        SolidColorBrush ramenoVyhybkyPoruchaBrush;
        SolidColorBrush vyhybkaRozrezPoruchaBrush;
        SolidColorBrush startEndPathBrush; // App.Current.Resources["startEndPathBrush"] as SolidColorBrush;

        //App.Current.Resources["odvratVC_PrechodBrush"] as VisualBrush;
        VisualBrush odvratVC_PrechodBrush, odvratPC_PrechodBrush;
        //App.Current.Resources["odvratPC_PrechodBrush"] as VisualBrush;

        /// <summary>
        /// priznak ci kontrol ma blikat
        /// </summary>
        public bool BlinkedStateEnabled
        {
            get; set;
        }


        //
        #endregion --------Fields------------




        public bool EnableErrorSound
        {
            get { return (bool)GetValue(EnableErrorSoundProperty); }
            set { SetValue(EnableErrorSoundProperty, value); }
        }

        /// <summary>
        /// Priznak ci pri chybovom stave sa ma spustat zvukove znamenie
        /// </summary>
        public static readonly DependencyProperty EnableErrorSoundProperty =
            DependencyProperty.Register("EnableErrorSound", typeof(bool), typeof(BaseTrainSwitchControl), new PropertyMetadata(true));


        public bool TimerFlag
        {
            get { return (bool)GetValue(TimerFlagProperty); }
            set { SetValue(TimerFlagProperty, value); }
        }

        // TimerFlag sa pouziva na zobrazenie vymeny v blikajucom stave.
        //TimerFlag je bindovany na property  TimerFlag vo view modeli,  hodnotu pre TimerFlag nastavuje 1 sekundovy timer na true/false
        public static readonly DependencyProperty TimerFlagProperty =
            DependencyProperty.Register("TimerFlag", typeof(bool), typeof(BaseTrainSwitchControl), new PropertyMetadata(false, OnTimerFlagChanged));

        private static void OnTimerFlagChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BaseTrainSwitchControl vymena = d as BaseTrainSwitchControl;
            vymena.OnTimerFlagChanged(e);
        }

        //spusta sa kazdu sekundu!!!
        /// <summary>
        /// Podla SwitchState meni farby pre blikajuci stav
        /// </summary>
        /// <param name="e"></param>
        void OnTimerFlagChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;
           
        }//OnTimerFlagChanged

        #region ---------Functions------------

     

        /// <summary>
        /// vrati actualSwitchState
        /// </summary>
        /// <returns></returns>
        public SWITCH_STATE GetActualSwitchState()
        {
            return actualSwitchState;
        }

        public INACTIVE_ARM GetInactiveArm()
        {
            return CurrentInactiveArm;
        }

        /// <summary>
        /// rameno vyhybky v neprechodnom smere vyfarbi na blackBrush;
        /// nastavi CurrentInactiveArm; nemeni ZIndex;
        /// </summary>
        /// <param name="rameno">rameno-path ktore sa ma vyfarbit</param>
        private void NastavNeprechodneRameno(Path rameno)
        {
            if (rameno != null)
            {
                string name = rameno.Name.ToUpper();
                rameno.Fill = rameno.Stroke = blackBrush;//   App.Current.Resources["blackBrush"] as SolidColorBrush;
                //TODO: nemohlo by byt rameno = Visibility.Visible
                if (name.Contains("RAMENO1"))
                    CurrentInactiveArm = INACTIVE_ARM.RAMENO1;
                else
                    CurrentInactiveArm = INACTIVE_ARM.RAMENO2;
            }
        }


        /// <summary>
        /// pre rameno vyhybky v prechodnom smere nastavi transparent brush;
        /// nastavi aj CurrentInactiveArm; nemeni ZIndex;
        /// </summary>
        /// <param name="rameno"></param>
        public void NastavPrechodneRameno(Path rameno)
        {
            if (rameno != null)
            {
                string name = rameno.Name.ToUpper();
                rameno.Fill = rameno.Stroke = transparentBrush; //App.Current.Resources["TransparentBrush"] as SolidColorBrush;
                //TODO: nemohlo by byt rameno = Visibility.Hidden
                if (name.Contains("RAMENO2"))
                    CurrentInactiveArm = INACTIVE_ARM.RAMENO1;
                else
                    CurrentInactiveArm = INACTIVE_ARM.RAMENO2;
            }
        }


        /// <summary>
        /// Nastavi Fill a Stroke pre rameno na transparent; 
        /// nemeni ActiveArm; nemeni ZIndex
        /// </summary>
        /// <param name="rameno"></param>
        public void SkryRameno(Path rameno)//SkryRameno
        {
            if (rameno != null)
            {
                rameno.Fill = rameno.Stroke = transparentBrush; 
                //TODO: nemohlo by byt rameno = Visibility.Hidden???
            }
        }

        /// <summary>
        /// pre rameno rozrezu nastavi Fill a Stroke na transparent
        /// </summary>
        /// <param name="ramenoRozrezu"></param>
        private void SkryRozrez(Path ramenoRozrezu)
        {
            if (ramenoRozrezu != null)
                ramenoRozrezu.Fill = ramenoRozrezu.Stroke = transparentBrush; //App.Current.Resources["TransparentBrush"] as SolidColorBrush;
            //ramenoRozrezu.Visibility = Visibility.Hidden;//v kode sa 'Visibility' sa riadi pomocou transparentBrush !!!
            //ak sa nastavi raz ramenoRozrezu.Visibility = Visibility.Hidden potom treba nastavovat ramenoRozrezu.Visibility = Visibility.Visible !!!
            //TODO: nemohlo by byt ramenoRozrezu = Visibility.Hidden
        }

        /// <summary>
        /// pre rameno rozrezu nastavi Fill a Stroke na redBrush; nemeni ZIndex;
        /// </summary>
        /// <param name="ramenoRozrezu"></param>
        private void UkazRozrez(Path ramenoRozrezu)
        {
            if (ramenoRozrezu != null)
                ramenoRozrezu.Fill = ramenoRozrezu.Stroke = App.Current.Resources["RedBrush"] as SolidColorBrush;
        }

        /*
        /// <summary>
        /// podla InactiveArm parametra, nastavi ktore rameno(Rameno1 alebo Rameno2) vymeny je viditelne(neprechodne sa vyfarbuje) a ktore nevyditelne( prechodne sa nevyfarbuje);
        /// ktore rameno pre rozrez(Rozrez1 alebo Rozrez2) je transparentne a ktore vyplnene
        /// </summary>
        /// <param name="filledRozrez">rameno rozrezu, ktore je vyplnene</param>
        /// <param name="transparentRozrez">rameno rozrezu, ktore je transparentne </param>
        /// <param name="visibleRameno">rameno vymeny, ktore je viditelne(neprechodny smer smer)</param>
        /// <param name="inVisibleRameno">rameno vymeny, ktore je neviditelne(prechodny smer)</param>
        public void ZistiStavRamien_old(out Path filledRozrez, out Path transparentRozrez, out Path visibleRameno, out Path inVisibleRameno)
        {
            if (CurrentInactiveArm == INACTIVE_ARM.RAMENO1)
            {
                transparentRozrez = Rozrez2;
                filledRozrez = Rozrez1;
                visibleRameno = Rameno1;
                inVisibleRameno = Rameno2;
            }
            else
            {
                transparentRozrez = Rozrez1;
                filledRozrez = Rozrez2;
                visibleRameno = Rameno2;
                inVisibleRameno = Rameno1;
            }
        }
        */



        /// <summary>
        /// Podla aktualneho stavu vymeny nastavi vystupne parametre
        /// </summary>
        /// <param name="filledRozrez">Rozrez-prvok vymeny v TrainSwitch, ktory ma byt zobrazeny ciernou farbou</param>
        /// <param name="transparentRozrez">Rozrez-prvok vymeny v TrainSwitch, ktory ma byt zobrazeny transparent farbou</param>
        /// <param name="visibleRameno">Rameno-prvok vymeny v TrainSwitch, ktory ma byt zobrazeny ciernou farbou</param>
        /// <param name="inVisibleRameno">Rameno-prvok vymeny v TrainSwitch, ktory ma byt zobrazeny transparent farbou</param>
        public void ZistiStavRamien(out Path filledRozrez, out Path transparentRozrez, out Path visibleRameno, out Path inVisibleRameno)//MH: zmena 20.03.2019
        {
            visibleRameno = GetNeprechodne_Rameno;//vrati Rameno1, alebo Rameno2
            inVisibleRameno = GetPrechodne_Rameno;

            string visibleName = visibleRameno.Name.ToUpper();
            if (visibleName.Contains("RAMENO1") )
            {
                transparentRozrez = Rozrez2;
                filledRozrez = Rozrez1;
            }
            else
            {
                transparentRozrez = Rozrez1;
                filledRozrez = Rozrez2;
            }
        }

        /// <summary>
        /// Pre vymenu nastavi rameno vymeny do neprechodneho stavu (rameno ma ciernu farbu);
        /// </summary>
        /// <param name="switchArm"></param>
        public void SetInactiveArm(INACTIVE_ARM switchArm)
        {
            InactiveArm = switchArm;//pri zmene InactiveArm sa spusti funkcia...
        }

        #endregion ---------Functions------------




        #region ------Dependency Property---------



        /// <summary>
        /// obsahuje aktualny rezim aplikacie, je to vybraty Rezim (CURRENT_MODE) z menu
        /// nastavuje sa v xaml: CurrentAppMode="{DynamicResource currentMode}" 
        /// </summary>
        public CURRENT_MENU_MODE CurrentAppMode
        {
            get { return (CURRENT_MENU_MODE)GetValue(CurrentAppModeProperty); }
            set { SetValue(CurrentAppModeProperty, value); }
        }


        public static readonly DependencyProperty CurrentAppModeProperty =
        DependencyProperty.Register("CurrentAppMode", typeof(CURRENT_MENU_MODE), typeof(BaseTrainSwitchControl), new UIPropertyMetadata(CURRENT_MENU_MODE.NONE));

        /// <summary>
        /// Path-usek pod ramenom 01
        /// </summary>
        public Path Part01
        {
            get { return (Path)GetValue(Part01Property); }
            set { SetValue(Part01Property, value); }
        }


        public static readonly DependencyProperty Part01Property =
            DependencyProperty.Register("Part01", typeof(Path), typeof(BaseTrainSwitchControl), new UIPropertyMetadata(null));


        /// <summary>
        /// Path-usek pod ramenom 02
        /// </summary>
        public Path Part02
        {
            get { return (Path)GetValue(Part02Property); }
            set { SetValue(Part02Property, value); }
        }

        public static readonly DependencyProperty Part02Property =
            DependencyProperty.Register("Part02", typeof(Path), typeof(BaseTrainSwitchControl), new UIPropertyMetadata(null));


        //--------------------------------

        public BaseTrainSwitchControl ConnectedSwitch1
        {
            get { return (BaseTrainSwitchControl)GetValue(ConnectedSwitch1Property); }
            set { SetValue(ConnectedSwitch1Property, value); }
        }


        public static readonly DependencyProperty ConnectedSwitch1Property =
            DependencyProperty.Register("ConnectedSwitch1", typeof(BaseTrainSwitchControl), typeof(BaseTrainSwitchControl), new UIPropertyMetadata(null));


        //----------------------------
        public BaseTrainSwitchControl ConnectedSwitch2
        {
            get { return (BaseTrainSwitchControl)GetValue(ConnectedSwitch2Property); }
            set { SetValue(ConnectedSwitch2Property, value); }
        }


        public static readonly DependencyProperty ConnectedSwitch2Property =
            DependencyProperty.Register("ConnectedSwitch2", typeof(BaseTrainSwitchControl), typeof(BaseTrainSwitchControl), new UIPropertyMetadata(null));

        //--------------------

        public int SwitchConnectMode2
        {
            get { return (int)GetValue(SwitchConnectMode2Property); }
            set { SetValue(SwitchConnectMode2Property, value); }
        }

        public static readonly DependencyProperty SwitchConnectMode2Property =
            DependencyProperty.Register("SwitchConnectMode2", typeof(int), typeof(BaseTrainSwitchControl), new UIPropertyMetadata(0));


        //----------------

        public int SwitchConnectMode1
        {
            get { return (int)GetValue(SwitchConnectMode1Property); }
            set { SetValue(SwitchConnectMode1Property, value); }
        }

        public static readonly DependencyProperty SwitchConnectMode1Property =
            DependencyProperty.Register("SwitchConnectMode1", typeof(int), typeof(BaseTrainSwitchControl), new UIPropertyMetadata(0));

        //-------------MH: pridane september 2018-------------
        /*MH poznamky:
         * Stare riesenie
         * pre nastavenie vymeny sa v UC_stavadloViewModel spusta funkcia: NastavVymenu(PRVOK_STAVADLA data){...}
         * 
         * class StavadloElements musi obsahovat property: SWITCH_STATE V810s, INACTIVE_ARM V810a, bool V810zbc, PATH_MODE V810k1, PATH_MODE V810k2
           Pomocou reflexie sa zapisuju hodnoty pre tieto property, podla udajov z telegramu.
           Pri zapise do dependency property SwitchState sa spusti funkcia OnSwitchStateChanged, ktora nastavi, vyfarbi prislusne casti vymeny;
                                          InactiveArmProp sa spusti funkcia OnArmChanged, ktora nastavi, vyfarbi prislusne casti vymeny;

         *  <controls:TrainSwitch37 x:Name="V810" CurrentWorkingMode="{DynamicResource currentMode}" 
                                        Part01="{Binding ElementName=V810p1}" 
                                        Part02="{Binding ElementName=V810p2}" 
                                        InactiveArmProp="{Binding Path=StavadloElementsData.V810a, Mode=OneWay}" 
                                        SwitchState="{Binding Path=StavadloElementsData.V810s, Mode=OneWay}"
                                        InvertArm="False" 
                                        ZaverBezCesty="{Binding Path=StavadloElementsData.V810zbc, Mode=OneWay}"
                                        Margin="561.666,329.683,0,370.317" Panel.ZIndex="30"> 
         <Path x:Name="V810p1" attProp:PathHelper.IsTrainSwitch="True" attProp:PathHelper.PathNumber="V810" attProp:PathHelper.ArmType="+"
                     attProp:PathHelper.Mode="{Binding StavadloElementsData.V810k1, Mode=OneWay}".../>
         <Path x:Name="V810p2" attProp:PathHelper.IsTrainSwitch="True" attProp:PathHelper.PathNumber="V810" attProp:PathHelper.ArmType="+"
                     attProp:PathHelper.Mode="{Binding StavadloElementsData.V810k2, Mode=OneWay}".../>

        
            NOVE RIESENIE
            Funkcionalita pre vykreslenie vymeny bola premiestnena z UC_StavadloViewModel do BaseTrainSwitchControl!!!
           
            Pri zapise do dependency property TsStatus sa spusti funkcia OnTsStatusChanged, ktora zabezpeci spravne vykreslenie vymeny a prilahlych usekov.

            

            <controls:TrainSwitch37 x:Name="V810" CurrentWorkingMode="{DynamicResource currentMode}" 
                                        Part01="{Binding ElementName=V810p1}" 
                                        Part02="{Binding ElementName=V810p2}" 
                                        TsStatus="{Binding STC[V810].CombineStatus, Mode=OneWay}""
                                        InvertArm="False" 
                                        Margin="561.666,329.683,0,370.317" Panel.ZIndex="30"> 

         <Path x:Name="V810p1" attProp:PathHelper.IsTrainSwitch="True" attProp:PathHelper.PathNumber="V810" attProp:PathHelper.ArmType="+"
                     .../>
         <Path x:Name="V810p2" attProp:PathHelper.IsTrainSwitch="True" attProp:PathHelper.PathNumber="V810" attProp:PathHelper.ArmType="-"
                     .../>
         * 

         */

        #region ==== TSstatus ====

        public Int64 TsStatus
        {
            get { return (Int64)GetValue(TsStatusProperty); }
            set { SetValue(TsStatusProperty, value); }
        }

        // Train switch Status
        //Obsahuje kombinovany udaj z telegramu Int64 obsahuje: Int16-stav, Int16-uvolIzol, Int16-vyluka, Int16-typElementu
        public static readonly DependencyProperty TsStatusProperty =
            DependencyProperty.Register("TsStatus", typeof(Int64), typeof(BaseTrainSwitchControl), new PropertyMetadata((Int64)0, OnTsStatusChanged));

        //BaseTrainSwitchControl btsControl;
        private static void OnTsStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BaseTrainSwitchControl btsControl = d as BaseTrainSwitchControl;
            btsControl.OnTsStatusChanged(e);
        }

        /* Vymena blika ak je v jednom zo stavov
         * SWITCH_STATE.STRATA_DOHLIADANIA
         * SWITCH_STATE.NADPRUD
         * SWITCH_STATE.PORUCHA_PRESTAVENIA
         * ROZREZ
         */

            /// <summary>
            /// vrati prehodne ak je plus==true
            /// </summary>
        Path GetPrechodne_RamenoPlus
        {
            get
            {
                if (InvertArm == true)//InvertArm == true a plus: neprechodne Rameno-...Rameno1...Part01, prechodne Rameno+...Rameno2...Part02
                    return Part02;
                else                 //InvertArm == false a plus: neprechodne Rameno-...Rameno2...Part02, prechodne Rameno+...Rameno1...Part01
                    return Part01;
            }
        }

        /// <summary>
        /// vrati prechodne rameno ak je minus==true
        /// </summary>
        Path GetPrechodne_RamenoMinus
        {
            get
            {
                if (InvertArm == true)// InvertArm == true a minus: neprechodne Rameno+...Rameno2...Part02, prechodne Rameno-...Rameno1...Part01
                    return Part01;
                else                 //InvertArm == false a minus:neprechodne Rameno+...Rameno1...Part01, prechodne Rameno-...Rameno2...Part02
                    return Part02;
            }
        }

        /// <summary>
        /// Podla hodnoty InvertArm a vymenaVpolohePlus vrati  Part01, alebo Part02 pre prechodne rameno vymeny
        /// </summary>
        Path GetPrechodne_RamenoVymeny
        {
            get
            {
                return Rameno2;
            }
        }

        /// <summary>
        /// Podla hodnoty InvertArm a vymenaVpolohePlus vrati Part01, alebo Part02 pre neprechodne rameno vymeny
        /// </summary>
        Path GetNeprechodne_RamenoVymeny
        {
            get
            {
                return Rameno2;
            }
        }


        /// <summary>
        /// Podla hodnoty InvertArm a vymenaVpolohePlus vrati  Rameno1, alebo Rameno2 pre prechodne rameno vymeny
        /// </summary>
        Path GetPrechodne_Rameno
        {
            get
            {
                return Rameno2;
            }
        }

        /// <summary>
        /// Podla hodnoty InvertArm a vymenaVpolohePlus vrati  Rameno1, alebo Rameno2 pre neprechodne rameno vymeny
        /// </summary>
        Path GetNeprechodne_Rameno
        {
            get
            {
                return Rameno2;
            }
        }

        /// <summary>
        /// priznak pre polohu vymeny
        /// True-prechodne je Rameno+;
        /// False-prechodne je Rameno-
        /// </summary>
        bool vymenaVpolohePlus;

        /// <summary>
        /// Zabezpeci vykreslenie vymeny podla udajov z telegramu
        /// </summary>
        /// <param name="e"></param>
        void OnTsStatusChanged( DependencyPropertyChangedEventArgs e)//najdolezitejsia a najzlozitejsia metoda aplikacie!!!
        {
            if (e.NewValue == null)
                return;

            
        }//OnTsStatusChanged(DependencyPropertyChangedEventArgs e)


        #endregion ==== TSstatus ====


        //------------------------------------

        public SWITCH_STATE PreviousSwitchState
        {
            get { return (SWITCH_STATE)GetValue(PreviousSwitchStateProperty); }
            set { SetValue(PreviousSwitchStateProperty, value); }
        }

        /// <summary>
        /// Hodnota SwitchState pred nastavenim novej hodnoty
        /// </summary>
        public static readonly DependencyProperty PreviousSwitchStateProperty =
            DependencyProperty.Register("PreviousSwitchState", typeof(SWITCH_STATE), typeof(BaseTrainSwitchControl), new PropertyMetadata(SWITCH_STATE.NONE));



        public SWITCH_STATE SwitchState
        {
            get { return (SWITCH_STATE)GetValue(SwitchStateProperty); }
            set { SetValue(SwitchStateProperty, value); }
        }

        /// <summary>
        /// Stav v akom je nastavena vymena-vyhybka;
        /// pri zapise nastavi-vykresli vymenu do daneho stavu
        /// </summary>
        public static readonly DependencyProperty SwitchStateProperty =
            DependencyProperty.Register("SwitchState", typeof(SWITCH_STATE), typeof(BaseTrainSwitchControl), new UIPropertyMetadata(SWITCH_STATE.NONE, OnSwitchStateChanged));

        /// <summary>
        /// nastavi-vykresli vymenu do stavu podla hodnoty SwitchState
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnSwitchStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BaseTrainSwitchControl switchControl = (BaseTrainSwitchControl)d;
            SWITCH_STATE oldValue = (SWITCH_STATE)e.OldValue;
            switchControl.PreviousSwitchState = oldValue;

            switchControl.OnSwitchStateChanged(e);
        }//OnSwitchStateChanged

        
        void OnSwitchStateChanged( DependencyPropertyChangedEventArgs e)
        {
            SWITCH_STATE oldValue = (SWITCH_STATE)e.OldValue;
            SWITCH_STATE newValue = (SWITCH_STATE)e.NewValue;
            
            
        }



        public INACTIVE_ARM InactiveArm
        {
            get { return (INACTIVE_ARM)GetValue(InactiveArmProperty); }
            set { SetValue(InactiveArmProperty, value); }
        }


        /// <summary>
        /// udava ktory rameno na vyhybke je prechodne-zaciernene;
        /// hodnoty: RAMENO01, RAMENO02, NONE;
        /// pri zmene sa  vola funkcia OnArmChanged
        /// </summary>
        public static readonly DependencyProperty InactiveArmProperty =
            DependencyProperty.Register("InactiveArm", typeof(INACTIVE_ARM), typeof(BaseTrainSwitchControl), new UIPropertyMetadata(INACTIVE_ARM.NONE, new PropertyChangedCallback(OnInactiveArmChanged)));

        // tato funkcia sa vola, ak sa zmeni hodnota dependency property : InactiveArm
        /// <summary>
        /// na cierno vyfarbi (bude neprechodne) rameno vyhybky, spusti VypniRameno02(), alebo VypniRameno01()
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnInactiveArmChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BaseTrainSwitchControl control = (BaseTrainSwitchControl)d;
            string name = control.Name;
            control.OnInactiveArmChanged(e);
        }

        void OnInactiveArmChanged(DependencyPropertyChangedEventArgs e)
        {
            //INACTIVE_ARM oldValue = (INACTIVE_ARM)e.OldValue;
            INACTIVE_ARM newValue = (INACTIVE_ARM)e.NewValue;
            //switch (newValue)
            //{
            //    case INACTIVE_ARM.NONE:
            //        break;
            //    case INACTIVE_ARM.RAMENO1://neaktivne ma byt Rameno1
            //        if (InvertArm)
            //            VypniRameno02();//Rameno2 bude cierne-neprechodne
            //        else
            //            VypniRameno01();//Rameno1 bude cierne-neprechodne
            //        break;
            //    case INACTIVE_ARM.RAMENO2:
            //        if (InvertArm)
            //            VypniRameno01();
            //        else
            //            VypniRameno02();//Rameno2 bude cierne-neprechodne
            //        break;
            //    default:
            //        break;
            //}
        }



        /*POZNAMKY:
         * 
       
        Flag InvertArm udava ci sa meni-invertuje Rameno+ za Rameno- a naopak;
        DP InvertArm nam prepaja graficky objekt TrainSwitch64 a Situacnu  schemu ST22.
        Pouziva sa pri nastavovani vyhybky;
        Pre niektoru vyhybku rameno Rameno1 (pozri TransSwitchXX.xaml) odpoveda Ramenu+ a pre niektoru vyhybku rameno Rameno1 odpoveda Ramenu-;
        Aby stav vyhybky sa nastavil ako je vychodzi-pohotovostny stav na stavadle; 
        Zaciernene rameno vyhybky na scheme stavadla (vypnute-nepriechodne ) v pohotovostnom stave je pre Logic Rameno-.
        Pohotovostny stav: stavove slovo stav=0x0001;
        Ak v pohotovostnom stave vymeny je vypnute-nepriechodne Rameno1 potom treba nastavit InvertArm="True"
        
         * V praxi na stavadle sa ramena vymeny-vyhybky oznacuju ako +rameno a -rameno. Pozri vykres 'Situacna schema ST22'.
         * Rameno vymeny je kolajovy usek.
         * V pohotovostnom stave vymeny je neprechodne (zaciernene) -rameno. Pozri obrazok ST22.
         * 
         * V aplikacii su vymeny instancie grafickeho objektu TrainSwict64.xaml. Tento graficky objekt ma "ramena" Rameno1 a Rameno2.
         * Pre niektoru vymenu rameno Rameno1 odpoveda +ramenu a pre niektoru vymenu rameno Rameno1 odpoveda -ramenu;
         * Aby sa v aplikacii nastavila vymena do pohotovostneho stavu podla obrazku, zavediem dependency property InvertArm typu bool. InvertArm sa nastavuje v *.xaml; Default je InvertArm="False".
         * 
         * InvertArm="True"  -  pohotovostnom stave ma vymena nepriechodne-zaciernene Rameno1 (napr. V528, V529,...)
         * InvertArm="False"  - pohotovostnom stave ma vymena nepriechodne-zaciernene Rameno2  (napr. V1124, V1125, V1168,...).  Netreba nastavit InvertArm="False" lebo je to default hodnota pre InvertArm!!!!!
         * PRIKLAD:
         * <controls:TrainSwitch64 x:Name="V528"   Margin="764.812,443.332,0,0"
            TsStatus="{Binding STC[V528].CombineStatus, Mode=OneWay}"
            InvertArm="True"
            Part01="{Binding ElementName=V528p1}"
            Part02="{Binding ElementName=V528p2}"
          />
         * 
         */

        public bool InvertArm
        {
            get { return (bool)GetValue(InvertArmProperty); }
            set { SetValue(InvertArmProperty, value); }
        }

        
        /// <summary>
        /// Flag udava ci sa meni-invertuje INACTIVE_ARM vyhybky;
        /// Pouziva sa pri nastavovani vyhybky;
        /// Pre niektoru vyhybku rameno Rameno1 odpoveda +ramenu a pre niektoru vyhybku rameno Rameno1 odpoveda -ramenu;
        /// Aby stav vyhybky sa nastavil ako je vychodzi-pociatocny stav na stavadle; 
        /// Zaciernene rameno vyhybky na scheme stavadla (vypnute-nepriechodne, INACTIVE_ARM) je pre Logic -rameno 
        /// Ak v pohotovostnom stave vymeny je vypnute-nepriechodne Rameno1 potom treba nastavit InvertArm="True"
        /// Ak v pohotovostnom stave vymeny je vypnute-nepriechodne Rameno2 potom treba uz netreba nastavit InvertArm="False" lebo je to default hodnota pre InvertArm.
        /// </summary>
        public static readonly DependencyProperty InvertArmProperty =
            DependencyProperty.Register("InvertArm", typeof(bool), typeof(BaseTrainSwitchControl), new PropertyMetadata(false));


        public bool ZaverBezCesty
        {
            get { return (bool)GetValue(ZaverBezCestyProperty); }
            set { SetValue(ZaverBezCestyProperty, value); }
        }

        //Pre nastavenie farby textu pre nazov vymeny!!!!!
        /// <summary>
        /// ak vyhybka ma zaver a nie je sucast cesty, potom ZaverBezCesty=true
        /// ak vyhybka ma zaver a je sucast cesty, potom ZaverBezCesty=false
        /// ak vyhybka nema zaver, potom ZaverBezCesty=false
        /// </summary>
        public static readonly DependencyProperty ZaverBezCestyProperty =
            DependencyProperty.Register("ZaverBezCesty", typeof(bool), typeof(BaseTrainSwitchControl), new PropertyMetadata(false));



        #endregion ------Dependency Property---------


        #region ---------RoutedEvents----------------------


        //public static readonly RoutedEvent TrainSwitchChangedModeEvent = EventManager.RegisterRoutedEvent("TrainSwitchChangedMode", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BaseTrainSwitchControl));

        //public event RoutedEventHandler TrainSwitchChangedMode
        //{
        //    add { AddHandler(TrainSwitchChangedModeEvent, value); }
        //    remove { RemoveHandler(TrainSwitchChangedModeEvent, value); }
        //}

        //void RaiseTrainSwitchChangedModeEvent()
        //{
        //    RoutedEventArgs newEventArgs = new RoutedEventArgs(BaseTrainSwitchControl.TrainSwitchChangedModeEvent, new TrainSwitchRoutedEventArgs() { myNewValue = "zmena" });
        //    RaiseEvent(newEventArgs);
        //}

        //============MH: attached event===================

        public static readonly RoutedEvent TrainSwitchChangedModeAttachedEvent = EventManager.RegisterRoutedEvent("TrainSwitchChangedMode2", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BaseTrainSwitchControl));

        //spusta sa v handleri pre MouseLeftButtonDown 
        void RaiseTrainSwitchChangedModeAttachedEvent()
        {
            object p = ClickedPath;
            RoutedEventArgs newEventArgs = new RoutedEventArgs(BaseTrainSwitchControl.TrainSwitchChangedModeAttachedEvent, new TrainSwitchRoutedEventArgs2() { myNewValue = "zmena", sender = this, clickedArm = ClickedArm });
            RaiseEvent(newEventArgs);
        }


        public static void AddTrainSwitchChangedModeAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {

            ((UIElement)d).AddHandler(BaseTrainSwitchControl.TrainSwitchChangedModeAttachedEvent, handler);
        }
        public static void RemoveTrainSwitchChangedModeAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {

            ((UIElement)d).RemoveHandler(BaseTrainSwitchControl.TrainSwitchChangedModeAttachedEvent, handler);
        }

        //=======================================================================

        #endregion ---------RoutedEvents----------------------


        #region ----------- Mouse handlers --------------------

        /// <summary>
        /// handler pre lavy klik na vyhybku;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected new void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }

      
        #endregion ----------- Mouse handlers --------------------

        #region ---------Handlers functions-------------

        /// <summary>
        /// nastavi vymenu do pohotovostneho stavu podla schemy ST22 zadanej na obrazku
        /// Pre pohotovostny stav je stav=0x0001, plus prechodne je Rameno+
        /// </summary>
        public void NastavPohotovostnyStav()
        {
            VypniRameno_Minus();//19.03.2019 MH
        }

        /*
        /// <summary>
        /// zmeni smer vyhybky; RAMENO1<->RAMENO2;
        /// vyfarbi na cierno RAMENO1, alebo RAMENO2 (smer je neprechodny);
        /// prechodny smer ktory je priehladny;
        /// nastavi InactiveArm
        /// </summary>
        public void ZmenaSmeruVyhybky_old()
        {
            if (CurrentInactiveArm == INACTIVE_ARM.RAMENO1)
            {
                VypniRameno02();
                return;
            }
            if (CurrentInactiveArm == INACTIVE_ARM.RAMENO2)
            {
                VypniRameno01();
                return;
            }
        }
        */


        public void ZmenaSmeruVyhybky()//MH: zmena 19.03.2019
        {
            if (vymenaVpolohePlus)//PRECHODNE Rameno+;  InvertArm=false prechodne je Rameno+...Rameno1, neprechodne Rameno-...Rameno2;  InvertArm=true prechodne je Rameno+....Rameno2, neprechodne je Rameno-...Rameno1
                VypniRameno_Plus();
            else
                VypniRameno_Minus();
        }


        /// <summary>
        /// vyhybku nastavi do normalneho stavu, vykresli Rameno1 a lebo Rameno2
        /// </summary>
        public void ZrusUplnuVyluku()
        {
            ZrusPoruchu();
            actualSwitchState = SWITCH_STATE.NORMAL;//nastavenie fieldu
        }


        /// <summary>
        /// vyhybku nastavi do normalneho stavu, vykresli Rameno1 a lebo Rameno2
        /// </summary>
        public void ZrusVylukuPrestavenia()
        {
            ZrusPoruchu();
        }

        /// <summary>
        /// Rameno1 -neprechodny smer, vyfarbi sa nacierno;
        /// Rameno2 ja zapnuty smer, bude transparent;
        /// InactiveArm nastavi na INACTIVE_ARM.RAMENO1;
        /// </summary>
        private void VypniRameno01()
        {
          
        }



        /// <summary>
        /// Rameno2-neprechodny smer, vyfarbi nacierno;
        /// Rameno1-je prechodne, bude transparent;
        /// InactiveArm nastavi na INACTIVE_ARM.RAMENO2;
        /// nastavi Panel.ZIndex pre pridruzene Path
        /// 
        /// </summary>
        private void VypniRameno02()
        {
           
        }

        /*
         *  base.Rameno1 = this.Rameno1_64;
            base.Rameno2 = this.Rameno2_64;
            base.Rozrez1 = this.Rozrez1_64;
            base.Rozrez2 = this.Rozrez2_64;
            base.MainGrid = this.mainGrid_64;
            base.VariantGrid = this.VariantGrid_64; //pozri xaml: <Grid x:Name="VariantGrid_64" .../>
         */

        /// <summary>
        /// Rameno+ bude NEPRECHODNE, Rameno- bude PRECHODNE
        /// </summary>
        void VypniRameno_Plus()
        {
           
        }

        /// <summary>
        /// Rameno+ bude PRECHODNE, Rameno- bude NEPRECHODNE
        /// </summary>
        void VypniRameno_Minus()
        {
           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clickedSwitch">vybrata vyhybka</param>
        /// <param name="pridruzenaVyhybka">vyhybka na spolocnom useku</param>
        private void VykonajNastaveniePridruzenejVyhybky(BaseTrainSwitchControl clickedSwitch, BaseTrainSwitchControl pridruzenaVyhybka)
        {
         
            
        }//VykonajNastaveniePridruzenejVyhybky

        private void VykonajNastaveniePridruzenejVyhybky2(BaseTrainSwitchControl clickedSwitch, BaseTrainSwitchControl Part04)
        {
            int stat = clickedSwitch.SwitchConnectMode1;
            clickedSwitch.SwitchConnectMode1 = clickedSwitch.SwitchConnectMode2;
            VykonajNastaveniePridruzenejVyhybky(clickedSwitch, Part04);
            clickedSwitch.SwitchConnectMode1 = stat;
        }

        /// <summary>
        /// Rameno vymeny nastavi do modu PATH_MODE.VLAKOVA_CESTA, alebo ak uz je v mode PATH_MODE.VLAKOVA_CESTA,
        /// potom ho nastavi do modu PATH_MODE.NORMAL
        /// </summary>
        /// <param name="rameno"></param>
        private void NastavVlakCestu(Path rameno)
        {
          

            //actualSwitchState = SWITCH_STATE.VLAKOVA_CESTA;
        }

        private void NastavSimulaciuObsadenia(bool both, Path path)
        {
            
        }//NastavSimulaciuObsadenia



        /// <summary>
        /// stav pred zmenou smeru vyhybky;
        /// prechodzie rameno ma modry okraj a vypln transparent;
        /// druhe rameno ma modry okraj a black vypln
        /// </summary>
        public void NastavVyhybkuVmanipulacii_old()
        {
            
        }

        /// <summary>
        /// stav pred zmenou smeru vyhybky;
        /// prechodne rameno ma modry okraj a vypln transparent;
        /// neprechodne rameno ma modry okraj a black vypln
        /// </summary>
        public void NastavVyhybkuVmanipulacii()//MH: uprava 20.03.2019
        {
            
            //visibleRameno.Tag = inVisibleRameno.Tag = false;    //true = ma blikat, false = neblikat; Animacia pre true sa spusta v xaml
        }

        /// <summary>
        /// Ak sa prestavuje vymena a PLC nevie polohu ramien
        /// </summary>
        public void NastavVyhybkuVmanipulacii_BezPolohy()
        {
            
        }


        public void NastavNeinicializovanuVyhybku()
        {
            
        }


        /// <summary>
        /// vymena sa nesmie prestavovat, ale moze sa cez nu stavat cesta;
        /// zlty lichobeznik v polohe do ktorej sa nesmie prestavit. okraj je cierny
        /// </summary>
        public void NastavVylukuPrestavenia()
        {
            
        }


        /// <summary>
        /// vymena sa nesmie prestavovat a nesmie sa cez nu stavat cesta;
        /// zlte oramovanie aktualnej polohy vymeny -prechodzieho smeru(zlte oramovanie zapnuteho ramena);
        /// zlte oramovanie nezapnuteho-zacierneneho ramena
        /// </summary>
        public void NastavUplnuVyluku()
        {
          
        }

        /// <summary>
        /// nastavi farby pre Rozrez1 a Rozrez2; Nastavi transparent brush pre Rameno1 a Rameno2;
        /// nemeni InactiveArm
        /// </summary>
        void NastavRozrez()
        {
           
        }


        /// <summary>
        /// zapnuty-prechodny smer vyhybky ma cervenu farbu;vypnuty smer ma ciernu farbu a cervene oramovanie
        /// </summary>
        void NastavNadprud()
        {
          
        }


        /// <summary>
        /// zapnuty smer (prechodne rameno) vyhybky ma cervenu farbu;
        /// vypnuty smer (neprechodne rameno) ma ciernu farbu a cervene oramovanie
        /// </summary>
        void NastavPoruchuPrestavenia() 
        {
           
        }

        /// <summary>
        /// obidve ramena vyhybky su cierne a oramovane cervenou farbou
        /// </summary>
        void NastavStratuDohliadania()
        {
           
        }


        public void NastavOdvratPC()  //Nastavi Odvrat pre  Posunovu estu
        {


        }

        public void NastavOdvratVC()//Nastavi Odvrat pre  Vlakovu estu
        {
            //System.Diagnostics.Debug.WriteLine($"=============NastavOdvratVC pre {Name}==================");
            // Path filledRozrez, transparentRozrez, visibleRameno /*nezapnuty smer */, inVisibleRameno /*priechodzi smer,zapnuty smer*/;
            ZistiStavRamien(out filledRozrez, out transparentRozrez, out visibleRameno, out inVisibleRameno);

                   }

        public void NastavOdvratVylukaPC()
        {
          
           
        }

        public void NastavOdvratVylukaVC()
        {
            
        }

        public void NastavOdvratPC_Prechod()
        {
              
        }

        public void NastavOdvratVC_Prechod()
        {
               
        }

        public void NastavOdvratVylukaPC_Prechod()
        {
            
        }

        public void NastavOdvratVylukaVC_Prechod()
        {
            
        }


        public void NastavVariantLM() //13.12.2013 LM
        {
            
        }

        //MH: 14.12.2013
        public void NastavVariant()
        {
            VariantGrid.Visibility = System.Windows.Visibility.Visible;//OK
        }
        public void VypniVariant()
        {
            VariantGrid.Visibility = System.Windows.Visibility.Collapsed;
        }
        //--------------------------------

       

        /// <summary>
        /// podla hodnoty vymenaVpolohePlus nastavi ramena, a nastavi actualSwitchState = SWITCH_STATE.NORMAL;
        /// </summary>
        public void ZrusPoruchu()//MH 19.03.2019
        {
            string name = this.Name;
            Rameno1.Tag = Rameno2.Tag = false; //true = ma blikat, false = neblikat
            if (vymenaVpolohePlus)
                VypniRameno_Minus();
            else
                VypniRameno_Plus();
            actualSwitchState = SWITCH_STATE.NORMAL;
        }

        /// <summary>
        /// Vrati true ak sa moze zmenit smer vymeny
        /// </summary>
        /// <returns></returns>
        public bool SwitchChange_IsEnabled()
        {
            bool result = false;
            switch(actualSwitchState)
            {
                case SWITCH_STATE.NONE:
                    break;
                case SWITCH_STATE.NORMAL:
                    //zmena smeru sa moze vykonat
                    return true;
                   
                case SWITCH_STATE.VYHYBKA_V_MANIPULACII:
                    break;
                case SWITCH_STATE.VYLUKA_PRESTAVENIA:
                    break;
                case SWITCH_STATE.UPLNA_VYLUKA:
                    break;
                case SWITCH_STATE.STRATA_DOHLIADANIA: //blika
                    break;
                case SWITCH_STATE.NADPRUD:   //blika
                    break;
                case SWITCH_STATE.PORUCHA_PRESTAVENIA: //blika
                    break;
                case SWITCH_STATE.ROZREZ:    //blika
                    break;
                case SWITCH_STATE.ODVRAT_PC:
                    break;
                case SWITCH_STATE.ODVRAT_VC:
                    break;
                case SWITCH_STATE.ODVRAT_VYLUKA_PC:
                    break;
                case SWITCH_STATE.ODVRAT_VYLUKA_VC:
                    break;
                case SWITCH_STATE.V_MANIPULACII_BEZ_POLOHY:
                    break;
                case SWITCH_STATE.ODVRAT_PC_PRECHOD:
                    break;
                case SWITCH_STATE.ODVRAT_VC_PRECHOD:
                    break;
                case SWITCH_STATE.ODVRAT_VYLUKA_PC_PRECHOD:
                    break;
                case SWITCH_STATE.ODVRAT_VYLUKA_VC_PRECHOD:
                    break;
                case SWITCH_STATE.VARIANT:
                    break;
                default:
                    break;
            }

            return result;
        }

        #endregion ---------Handlers functions-------------


        #region ------HitTest----------

        /// <summary>
        /// nastavi clickedPath;
        /// Path, ktora nema tvar stvoruholnika, napr. ma tvar hokejky, zabera ovela vacsiu plochu stvoruholnika;
        /// Tato funkcia zisti, ci bol klik len na plochu vykresleneho objektu;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoHitTest(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition((UIElement)MainGrid);    // MainGrid ==  grid v ktorom su vsetky graficke komponenty vymeny
            // Clear the contents of the list used for hit test results.
            // Set up a callback to receive the hit test result enumeration.
            VisualTreeHelper.HitTest(MainGrid,                  //Klik testovanie
                new HitTestFilterCallback(MyHitTestFilter),     //filter
                new HitTestResultCallback(MyHitTestResult),     //ResultCallBack
                new PointHitTestParameters(pt));                //Bod
            //if (hitResultsList.Count > 0)
            //{
            //    return (Path)hitResultsList[0];
            //}
            //return null;

        }//DoHitTest

        //prida do zoznamu vsetky pathy na ktore sa kliklo
        public HitTestResultBehavior MyHitTestResult(HitTestResult result)
        {
            // Set the behavior to return visuals at all z-order levels. 
            return HitTestResultBehavior.Continue;
        }

        //odfiltruje vsetky UserControly
        public HitTestFilterBehavior MyHitTestFilter(DependencyObject o)
        {
            Path p = o as Path;
            if (p != null && !p.Name.ToLower().Contains("pozadie"))//pozadie je objekt typu Path, oblast do ktorej je vlozene telo vyhybky
            {
                ClickedPath = p;
                return HitTestFilterBehavior.Stop;
            }
            else
            {
                // Visual object is part of hit test results enumeration. 
                return HitTestFilterBehavior.Continue;
            }
        }

        #endregion------HitTest----------

    }//partial class BaseTrainSwitchControl
}
