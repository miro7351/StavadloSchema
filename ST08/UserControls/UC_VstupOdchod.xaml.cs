using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using PA.Stavadlo.MH.Controls;
using PA.Stavadlo.MH.Enums;

namespace PA.Stavadlo.MH.UserControls
{
    //MH: september 2018
    //testovane 7.9.2018 OK!! synchronizacia pre 2 UserControly spolu so zvukom je OK!!
    //Poznamka: zvukove znamenie sa spusta z view modelu pre UC_MapaStavadla

    /// <summary>
    /// User control pre zobrazenie suhlasov
    /// </summary>
    public partial class UC_VstupOdchod : UserControl
    {
       // AppEventsInvoker appEventsInvoker;
        public UC_VstupOdchod()
        {
            InitializeComponent();
            //appEventsInvoker = AppEventsInvoker.Instance;
            this.Loaded += UC_VstupOdchod_Loaded;
            this.MouseLeftButtonDown += UC_VstupOdchod_MouseLeftButtonDown;
        }

        private void UC_VstupOdchod_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RaiseSuhlasLeftClickAttachedEvent();
        }

        private void UC_VstupOdchod_Loaded(object sender, RoutedEventArgs e)
        {
            //Pre kazdy UC_VstupOdchod nastavime binding pre DP TimerFlag: <controls:UC_VstupOdchod x:Name="sxxx"  TimerFlag="{Binding TimerFlag, Mode=OneWay}".../>
            //Je to vyhodne, lebo pre vsetky UC_VstupOdchod DP TimerFlagProperty sa binduje na jeden zdroj v DataContexte!!!
            //"TimerFlag" je meno property z DataContextu.
            //TimerFlag z DataContextu sa pouziva na to, aby vsetky kontroly blikali synchronizovane!!
            var dc = this.DataContext;
            Binding timerFlagBinding = new Binding("TimerFlag") { Mode = BindingMode.OneWay, Source = this.DataContext };
            BindingOperations.SetBinding(this, TimerFlagProperty, timerFlagBinding);
        }

        /* Stavy pre sipku vstup:- ziadost pre vstup, potom sipka blika na zlto
         *                       - suhlas pre vstup, potom sipka trvalo svieti na zlto
         * Stavy pre sipku odchod:- ziadost pre odchod, potom sipka blika na zeleno
         *                        - suhlas pre odchod, potom sipka trvalo svieti na zeleno                         
         */

        /// <summary>
        /// priznak ci kontrol ma blikat
        /// </summary>
        public bool BlinkedStateEnabled
        {
            get; set;
        }

        public TYP_SUHLASU SuhlasTyp
        {
            get { return (TYP_SUHLASU)GetValue(SuhlasTypProperty); }
            set { SetValue(SuhlasTypProperty, value); }
        }

        //Nastavuje orientaciu a farbu sipky
        public static readonly DependencyProperty SuhlasTypProperty =
            DependencyProperty.Register("SuhlasTyp", typeof(TYP_SUHLASU), typeof(UC_VstupOdchod), new PropertyMetadata(TYP_SUHLASU.NONE));

        public bool EnableErrorSound
        {
            get { return (bool)GetValue(EnableErrorSoundProperty); }
            set { SetValue(EnableErrorSoundProperty, value); }
        }

        /// <summary>
        /// Priznak ci pri blikani sa ma spustat zvukove znamenie
        /// </summary>
        public static readonly DependencyProperty EnableErrorSoundProperty =
            DependencyProperty.Register("EnableErrorSound", typeof(bool), typeof(UC_VstupOdchod), new PropertyMetadata(true));


        public bool TimerFlag
        {
            get { return (bool)GetValue(TimerFlagProperty); }
            set { SetValue(TimerFlagProperty, value); }
        }

        // DependencyProperty TimerFlag je bindovany na property  TimerFlag vo view modeli. Hodnotu fieldu TimerFlag vo view modeli nastavuje 1 sekundovy timer na true/false
        public static readonly DependencyProperty TimerFlagProperty =
            DependencyProperty.Register("TimerFlag", typeof(bool), typeof(UC_VstupOdchod), new PropertyMetadata(false) );

        
        #region === Int64: SuhlasStatus ===

        public Int64 SuhlasStatus
        {
            get { return (Int64)GetValue(SuhlasStatusProperty); }
            set { SetValue(SuhlasStatusProperty, value); }
        }

        /// <summary>
        /// Hodnota obsahuje v Int64 zakodovane udaje: Int16 stav, Int16 uvolIzol, Int16 vyluka, Int16 podtyp z telegramu
        /// </summary>
        public static readonly DependencyProperty SuhlasStatusProperty =
            DependencyProperty.Register("SuhlasStatus", typeof(Int64), typeof(UC_VstupOdchod), new PropertyMetadata((Int64)0, OnSuhlasStatusChanged));


        //spusta sa pri zapise do SuhlasStatus ak stara a nova hodnota nie su rovnake!!!!
        private static void OnSuhlasStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UC_VstupOdchod vstupOdchod = d as UC_VstupOdchod;
            vstupOdchod.OnSuhlasStatusChanged(e);
        }

        private void OnSuhlasStatusChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;

            
        }//OnSuhlasStatusChanged

        /*
         * 
             Kodovanie stavovych slov suhlasov Podtyp=s
            Stav:
             0x01,  // ziadost o suhlas pre odchod z ST22
             0x02,  // suhlas pre vstup na ST22 udeleny
             0x04,  // ziadost o suhlas pre vstup na ST22
             0x08,  // suhlas pre odchod z ST22 udeleny

         */

        #endregion === Int64: SuhlasStatus ===


        // --------22.03.2019 Pridane lavy klik na UC_VstupOdchod --------------
        public static readonly RoutedEvent SuhlasLeftClickAttachedEvent = EventManager.RegisterRoutedEvent("SuhlasLeftClickAttachedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UC_VstupOdchod));
        private void RaiseSuhlasLeftClickAttachedEvent()
        {
            //RoutedEventArgs newEventArgs = new RoutedEventArgs(UC_VstupOdchod.SuhlasLeftClickAttachedEvent, new SuhlasEventArgs() { sender = this });
            //RaiseEvent(newEventArgs);
        }
        public static void AddSuhlasLeftClickAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ((UIElement)d).AddHandler(UC_VstupOdchod.SuhlasLeftClickAttachedEvent, handler);
        }

        public static void RemoveSuhlasLeftClickAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ((UIElement)d).RemoveHandler(UC_VstupOdchod.SuhlasLeftClickAttachedEvent, handler);
        }
    }

    /* Aby sa dali simulovat akcie z vonkajsieho stavadla (ziadost o vstup na ST22, udelenie suhlasu pre odchod z ST22, rusenie ziadosti pre vstup na ST22 ), pozri PFunkcie-Simulacia-Suhlasu
     * 
     * <!--PFunkcie/Simulacia/Suhlasu   Simuluje akcie z vonkajsieho stavadla-->
       <MenuItem Header="{lex:LocText Key=suhlasu}">
           <!--Simulacia ziadosti o vstup na ST22, zlta sipka smeruje dnu a blika; Pre udelenie suhlasu menuitem 'Suhlas' 'Udelenie suhlasu'-->
           <MenuItem Header="{lex:LocText Key=ziadosti}" Command="{Binding MenuCommand}" CommandParameter="SIMULACIA_ZIADOSTI_O_SUHLAS"/>
           <!--Simulacia udelenia suhlasu pre odchod z ST22 zelena sipka trvalo svieti; najprv sa musi zvolit 'Suhlas' 'Ziadost o suhlas', zelena sipka smeruje von a blika-->
           <MenuItem Header="{lex:LocText Key=udelenia}" Command="{Binding MenuCommand}" CommandParameter="SIMULACIA_UDELENIA_SUHLASU" />
           <!--Rusi ziadost o vstup na ST22, najprv sa musi zvolit Simulacia ziadosti o suhlas-->
           <MenuItem Header="{lex:LocText Key=rusenia}"  Command="{Binding MenuCommand}" CommandParameter="SIMULACIA_RUSENIA_SUHLASU"  />
       </MenuItem>

       <!--Suhlas-->
        <MenuItem Header="{lex:LocText Key=suhlas}" IsEnabled="{Binding Path=MenuButtonIsEnabledFlag, Mode=OneWay, FallbackValue=False}"
            attProp:EnabledRolesHelper.EnabledRoles="ADMIN,DISPECER"> 
            <MenuItem.ToolTip>
                <ToolTip Style="{StaticResource menuTooltip}" Content="Obsluha súhlasov"/>
            </MenuItem.ToolTip>
            <!-- Ziadost pre odchod z ST22
            Pre simulaciu udelenia suhlasu pre odchod z ST22 treba vybrat -PFunkcie/Simulacia/Suhlas udelenie -->
            <MenuItem Header="{lex:LocText Key=ziadostOSuhlas}"  Command="{Binding MenuCommand}" CommandParameter="ZIADOST_O_SUHLAS"/>
             <!-- Udelenie suhlasu pre vstup na ST22
             Pre simulaciu ziadosti o vstup na ST22 treba vybrat -PFunkcie/Simulacia/Suhlas ziadost -->
            <MenuItem Header="{lex:LocText Key=udelenieSuhlasu}" Command="{Binding MenuCommand}" CommandParameter="UDELENIE_SUHLASU"/>
            <!--Zrusenie ziadosti pre odchod z ST22 -->
            <MenuItem Header="{lex:LocText Key=zrusenieSuhlasu}" Command="{Binding MenuCommand}" CommandParameter="RUSENIE_SUHLASU"/>
        </MenuItem>
     * 
     */
    //Infrastructure.Enum: Enums01.cs
    /*public enum TYP_SUHLASU
    
             ZIADOST_VSTUP,  //sipka smeruje dnu, blika Transparent<->Yellow kazdu sekundu
             SUHLAS_VSTUP,   //sipka smeruje dnu, zlta trvalo svieti
             ZIADOST_ODCHOD, //sipka smeruje von, blika Transparent<->Green kazdu sekundu
             SUHLAS_ODCHOD,  //sipka smeruje von, zelena trvalo svieti
             NONE
     */
}
