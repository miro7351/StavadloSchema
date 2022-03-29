using PA.Stavadlo.Data.Telegrams;
using ST08;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PA.Stavadlo.Infrastructure.PathHelperPA
{

    /// <summary>
    /// Popisuje stav useku
    /// </summary>
    public enum PATH_MODE
    {
        NEINICIALIZOVANY,
        NERIADENY_USEK,//MH: 25.03.2022 neriadi sa Logicom, v aplikacii sa nemeni jeho farba;
        NORMAL,
        NEIZOLOVANY_USEK,
        IZOLOVANY_USEK,
        POSUNOVA_CESTA,//do 19.11.2013 bolo STAVANIE_CESTY, MH: premenovane na POSUNOVA_CESTA, od verzie 1.0.1.84 vratane
        VLAKOVA_CESTA,
        OBSADENY_USEK,
        UVOLNENA_IZOLACIA,
        START_END_PATH,         //zaciatocny a koncovy usek pri stavani cesty
        START_END_PATH_POSUN,   //Nepouziva sa, onPropertyChanged sa nevolalo - teraz riesene cez sleep 
        START_END_PATH_VLAK,    //Nepouziva sa, onPropertyChanged sa nevolalo - teraz riesene cez sleep
        POSUN_CESTA_VYMENA_NORMAL,       //ak sa stavia posunova cesta, vyhybka je v manipulacii a treba vyfarbit path cez ktoru sa cesta stavia
        POSUN_CESTA_VYMENA_OBSAD,        //ak sa stavia posunova cesta, vyhybka je v manipulacii a treba vyfarbit path cez ktoru sa cesta stavia a ta path je obsadena
        POSUN_CESTA_VYMENA_UVOLIZOL,     //ak sa stavia posunova cesta, vyhybka je v manipulacii a treba vyfarbit path cez ktoru sa cesta stavia a ta path je uvolnenaIzolacia
        VLAK_CESTA_VYMENA_NORMAL,       //ak sa stavia vlakova cesta, vyhybka je v manipulacii a treba vyfarbit path cez ktoru sa cesta stavia
        VLAK_CESTA_VYMENA_OBSAD,        //ak sa stavia vlakova cesta, vyhybka je v manipulacii a treba vyfarbit path cez ktoru sa cesta stavia a ta path je obsadena
        VLAK_CESTA_VYMENA_UVOLIZOL      //ak sa stavia vlakova cesta, vyhybka je v manipulacii a treba vyfarbit path cez ktoru sa cesta stavia a ta path je uvolnenaIzolacia

        //do 19.11.2013 bolo takto, MH: premenovane na nove nazvy od verzie 1.0.1.84
        //STAV_CESTA_VYMENA_NORMAL,       //ak sa stavia posunova cesta, vyhybka je v manipulacii a treba vyfarbit path cez ktoru sa cesta stavia
        //STAV_CESTA_VYMENA_OBSAD,        //ak sa stavia posunova cesta, vyhybka je v manipulacii a treba vyfarbit path cez ktoru sa cesta stavia a ta path je obsadena
        //STAV_CESTA_VYMENA_UVOLIZOL,     //ak sa stavia posunova cesta, vyhybka je v manipulacii a treba vyfarbit path cez ktoru sa cesta stavia a ta path je uvolnenaIzolacia

    }

    /// <summary>
    /// Popisuje stav jedneho ramena vyhybky
    /// </summary>
    //public enum SWITCH_PART_MODE
    //{
    //    NONE = 0,       //Pre mozne pouzitie
    //    NONACTIVE,      //Rameno vyhybky je transparentna
    //    ACTIVE          //Rameno vyhybky je cierne
    //}


    //Pozn: Path je Sealed class, preto sa nedaju pridat DependencyProperty, daju sa pridat len AttachedProperty;
    //      Preto som vytvoril class PathHelperPA, ktora obsahuje attached properties pouzivane pre Path.
    /// <summary>
    /// Obsahuje Attached Properties pouzivane pre Path-usek trate  a pre vyhybku
    /// </summary>
    public class PathHelper : DependencyObject
    {
        static PathHelper()
        {
            //appEventsInvoker = AppEventsInvoker.Instance;
        }

        public PathHelper()
        {
            PreviousPathMode = PATH_MODE.NORMAL;
        }

        //static AppEventsInvoker appEventsInvoker;
        /// <summary>
        /// Mod useku : 0-normal, 1-vlakova cesta, 2-stavanie cesty, 3-blikacka
        /// </summary>
        public static PATH_MODE Mode;

        /// <summary>
        /// Stav v akom bola vymena pred nastavenim aktualneho stavu
        /// </summary>
        public PATH_MODE PreviousPathMode { get; set; }

        #region -------Attached Property pre Path----------

        public static bool GetIsCommonSwitchPart(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsCommonSwitchPartProperty);
        }

        public static void SetIsCommonSwitchPart(DependencyObject obj, bool value)
        {
            obj.SetValue(IsCommonSwitchPartProperty, value);
        }



        public static PATH_MODE GetPreviousMode(DependencyObject obj)
        {
            return (PATH_MODE)obj.GetValue(PreviousModeProperty);
        }

        public static void SetPreviousMode(DependencyObject obj, PATH_MODE value)
        {
            obj.SetValue(PreviousModeProperty, value);
        }
        //mod useku pred aktualne nastavenym modom
        public static readonly DependencyProperty PreviousModeProperty =
            DependencyProperty.RegisterAttached("PreviousMode", typeof(PATH_MODE), typeof(PathHelper), new PropertyMetadata(PATH_MODE.NORMAL));



        //Udava ci pod  bodom useku  na ktory sa kliklo sa nachadza aj cast dalsieho useku
        //pouziva sa na nastavenie parametra kod2 pri stavani ciest
        //nastavuje sa v UC_MapaStavadla.xaml.cs DoHitTest()
        public static readonly DependencyProperty IsCommonSwitchPartProperty =
            DependencyProperty.RegisterAttached("IsCommonSwitchPart", typeof(bool), typeof(PathHelper), new PropertyMetadata(false));


        public static PATH_MODE GetMode(DependencyObject target)
        {
            return (PATH_MODE)target.GetValue(ModeProperty);
        }


        //Nastavenie rovnakeho modu aj pre zretazenu path
        public static void SetMode(DependencyObject target, PATH_MODE value)
        {
            //if (target.GetValue(SecondCrossPartProperty) != null)   //AK sa kliklo na krizove rozpoltenie ciest
            //    target = target.GetValue(ChainedPathProperty) as System.Windows.Shapes.Path;    //Musime to nastavit na nejaku pridruzenu cestu, lebo potom by blblo nastavovanie Zindexu
            //if (target == null)
            //    return;
            target.SetValue(ModeProperty, value);
        }

        //MH september zmena call back funkcie
        //public static readonly DependencyProperty ModeProperty =
        //    DependencyProperty.RegisterAttached("Mode", typeof(PATH_MODE), typeof(PathHelper), new PropertyMetadata(PATH_MODE.NEINICIALIZOVANY, new PropertyChangedCallback(SetModeCallBack)));

        /// <summary>
        /// pre vykreslenie stavu useku;
        /// urci sa z telegramu pre vymenu (napr: V830) z hodnot: stav, uvolIzol, vyluka;
        /// </summary>
        public static readonly DependencyProperty ModeProperty =
       DependencyProperty.RegisterAttached("Mode", typeof(PATH_MODE), typeof(PathHelper), new PropertyMetadata(PATH_MODE.NEINICIALIZOVANY, OnModeChanged));

        //POZN: PropertyChangedCallback is executed when event (property change event) is raised.
        //Note that this is not thread safe, and you will need to use Dispatcher.Invoke to safely set other properties in this callback!!!
        //MH: v UC_StavadloViewModel sa spusta funkcia SpracujData2 a ta je umiestnena do Dispatchera!!
        private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newValue = (PATH_MODE)e.NewValue;
            var oldValue = (PATH_MODE)e.OldValue;

            System.Windows.Shapes.Path controlPath = d as System.Windows.Shapes.Path;
            controlPath.SetValue(PathHelper.PreviousFillColorProperty, controlPath.Fill);

            SetPreviousMode(d, oldValue);

            //if( (control.Name == "K201a"))
            //    System.Diagnostics.Debug.WriteLine("==============OnModeChanged: NASTAVUJEM K201a na: " + e.NewValue.ToString() + "   " + DateTime.Now.ToString("HH:mm:ss,fff") + " =================================");
            //if (control.Name == "K601")
            //    System.Diagnostics.Debug.WriteLine("==============OnModeChanged: NASTAVUJEM K601 na: " + e.NewValue.ToString() + "   " + DateTime.Now.ToString("HH:mm:ss,fff") + " =================================");

            //System.Windows.Shapes.Path crossPath = d.GetValue(CrossConnectorPathProperty) as System.Windows.Shapes.Path;
            //System.Windows.Shapes.Path crossPath = controlPath.GetValue(CrossConnectorPathProperty) as System.Windows.Shapes.Path;


            //Krizove pretinanie ciest
            //if (crossPath != null)
            //{
            //    SetCrossVisibility(crossPath, (PATH_MODE)e.NewValue);
            //    // SetZIndex((System.Windows.Shapes.Path)d, (int)target.GetValue(Panel.ZIndexProperty), (PATH_MODE)e.NewValue); //Zabezpecenie aby bol Cross (to co predstavuje krizik) stale na vrchu
            //}
        }

        //public void SetPreviousMode( PATH_MODE mode)
        //{
        //    PreviousMode = mode;
        //}

        /*
                public static int GetUvolIzol(DependencyObject obj)
                {
                    return (int)obj.GetValue(UvolIzolProperty);
                }

                public static void SetUvolIzol(DependencyObject obj, int value)
                {
                    obj.SetValue(UvolIzolProperty, value);
                }
                /// <summary>
                /// obsahuje stavove slovo uvolIzol;
                /// pouziva sa pre neizolovane useky, ktore su sucastou posunovej, alebo vlakovej cesty, aby sa dobre vykreslilo pozadie ak je usek oznaceny ako obsadeny;
                /// nie vsetky neizolovane useky mozu byt zaciatkom, alebo koncom cesty
                /// </summary>
                public static readonly DependencyProperty UvolIzolProperty =
                    DependencyProperty.RegisterAttached("UvolIzol", typeof(int), typeof(PathHelper), new PropertyMetadata(0));
                */

        public static string GetPathNumber(DependencyObject obj)
        {
            return (string)obj.GetValue(PathNumberProperty);
        }

        public static void SetPathNumber(DependencyObject obj, string value)
        {
            obj.SetValue(PathNumberProperty, value);
        }

        /// <summary>
        /// obsahuje oznacenie useku; pouziva sa pre zobrazenie ToolTipu;
        /// V nasom  projekte vyhybka je  user control TrainSwitchxx, ramena vyhybky-objekty path nepatria do user controlu.
        /// Aby pri zobrazeni ToolTipu pre Path ktora je 'pripojena' ku vyhybke-TrainSwitchxx sa zobrazili udaje pre vyhybku,
        /// potom v xaml  do PathNumber zapisem TrainSwitchxx.Name ku ktorej je path pripojena.
        /// </summary>
        public static readonly DependencyProperty PathNumberProperty =
            DependencyProperty.RegisterAttached("PathNumber", typeof(string), typeof(PathHelper), new PropertyMetadata(string.Empty));


        public static bool GetIsTrainSwitch(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsTrainSwitchProperty);
        }

        public static void SetIsTrainSwitch(DependencyObject obj, bool value)
        {
            obj.SetValue(IsTrainSwitchProperty, value);
        }

        /// <summary>
        /// ak Path je 'pripojena' ku TrainSwitchxx, potom treba ju  nastavit na True,
        /// aby sa v ToolTipe  pre Path zobrazili tie iste udaje ako pre TrainSwitch, t.j. attached property PathNumber;
        /// Ak Path nie je 'pripojena' ku TrainSwitchxx, potom IsTrainSwitch defaultne  nastavina na False,
        /// aby sa v ToolTipe  pre Path zobrazil Path.Name;
        /// </summary>
        public static readonly DependencyProperty IsTrainSwitchProperty =
            DependencyProperty.RegisterAttached("IsTrainSwitch", typeof(bool), typeof(PathHelper), new PropertyMetadata(false));



        public static string GetArmType(DependencyObject obj)
        {
            return (string)obj.GetValue(ArmTypeProperty);
        }

        public static void SetArmType(DependencyObject obj, string value)
        {
            obj.SetValue(ArmTypeProperty, value);
        }

        /// pouziva sa na nastavenie typu ramena vymeny, ci je to +, alebo - rameno;
        /// len pre ramena ktore maju IsTrainSwitch==true
        public static readonly DependencyProperty ArmTypeProperty =
            DependencyProperty.RegisterAttached("ArmType", typeof(string), typeof(PathHelper), new PropertyMetadata(string.Empty));



        /// <summary>
        /// Text zobrazeny v tooltipe
        /// </summary>
        public static string GetToolTipText(DependencyObject obj)
        {
            return (string)obj.GetValue(ToolTipTextProperty);
        }

        public static void SetToolTipText(DependencyObject obj, string value)
        {
            obj.SetValue(ToolTipTextProperty, value);
        }

        public static readonly DependencyProperty ToolTipTextProperty =
            DependencyProperty.RegisterAttached("ToolTipText", typeof(string), typeof(PathHelper), new PropertyMetadata(string.Empty));


        /// <summary>
        /// Posledna zmena polohy vyhybky formate dd.MM.yyyy HH:mm:ss
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetLastModeChange(DependencyObject obj)
        {
            return (string)obj.GetValue(LastModeChangeProperty);
        }

        public static void SetLastModeChange(DependencyObject obj, string value)
        {
            obj.SetValue(LastModeChangeProperty, value);
        }

        public static readonly DependencyProperty LastModeChangeProperty =
            DependencyProperty.RegisterAttached("LastModeChange", typeof(string), typeof(PathHelper), new PropertyMetadata(string.Empty));



        public static string GetComment01(DependencyObject obj)
        {
            return (string)obj.GetValue(Comment01Property);
        }

        public static void SetComment01(DependencyObject obj, string value)
        {
            obj.SetValue(Comment01Property, value);
        }

        /// <summary>
        /// Komentar pripojeny k objektu Path
        /// </summary>
        public static readonly DependencyProperty Comment01Property =
            DependencyProperty.RegisterAttached("Comment01", typeof(string), typeof(PathHelper), new PropertyMetadata(string.Empty));


        //Zretazena Path - pri vyhybkach; kolajovy usek zlozeny z viacerych Path, aby tooltip pre vyhybky ukazoval spravne
        public static System.Windows.Shapes.Path GetChainedPath(DependencyObject obj)
        {
            return (System.Windows.Shapes.Path)obj.GetValue(ChainedPathProperty);
        }

        public static void SetChainedPath(DependencyObject obj, System.Windows.Shapes.Path value)
        {
            obj.SetValue(ChainedPathProperty, value);
        }


        public static readonly DependencyProperty ChainedPathProperty =
            DependencyProperty.RegisterAttached("ChainedPath", typeof(System.Windows.Shapes.Path), typeof(PathHelper), new UIPropertyMetadata(null));


        public static System.Windows.Shapes.Path CrossConnectorPath(DependencyObject obj)
        {
            return (System.Windows.Shapes.Path)obj.GetValue(CrossConnectorPathProperty);
        }

        public static void SetCrossConnectorPath(DependencyObject obj, System.Windows.Shapes.Path value)
        {
            obj.SetValue(CrossConnectorPathProperty, value);
        }

        // Krizove pretinanie ciest 
        public static readonly DependencyProperty CrossConnectorPathProperty =
            DependencyProperty.RegisterAttached("CrossConnectorPath", typeof(System.Windows.Shapes.Path), typeof(PathHelper), new UIPropertyMetadata(null));




        // Druha cast kriza, ktory pretina cestu
        public static System.Windows.Shapes.Path GetSecondCrossPart(DependencyObject obj)
        {
            return (System.Windows.Shapes.Path)obj.GetValue(SecondCrossPartProperty);
        }

        public static void SetSecondCrossPart(DependencyObject obj, System.Windows.Shapes.Path value)
        {
            obj.SetValue(SecondCrossPartProperty, value);
        }

        public static readonly DependencyProperty SecondCrossPartProperty =
            DependencyProperty.RegisterAttached("SecondCrossPart", typeof(System.Windows.Shapes.Path), typeof(PathHelper), new UIPropertyMetadata(null));


        public static bool GetIsUnisolatedPath(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsUnisolatedPathProperty);
        }

        public static void SetIsUnisolatedPath(DependencyObject obj, bool value)
        {
            obj.SetValue(IsUnisolatedPathProperty, value);
        }

        /// <summary>
        /// Priznak ze usek je NEIZOLOVANY, v telegrame Podtyp='0' nastavuje sa v xaml
        /// </summary>
        public static readonly DependencyProperty IsUnisolatedPathProperty =
            DependencyProperty.RegisterAttached("IsUnisolatedPath", typeof(bool), typeof(PathHelper), new PropertyMetadata(false));
        /*POUZITIE:
         *  <Path x:Name="_770A" Data="M120,10 L0,10 0,0 120,0"
                      Height="10" Margin="1056.251,940.052,0,0"  Stretch="Fill" Width="67.577"
                      attProp:PathHelper.PathStatus="{Binding STC[_770A].CombineStatus, Mode=OneWay}"
                      attProp:PathHelper.PathNumber="770A"
                      attProp:PathHelper.IsUnisolatedPath="True"
                      />
         * 
         */



        public static Brush GetPreviousFillColor(DependencyObject obj)
        {
            return (Brush)obj.GetValue(PreviousFillColorProperty);
        }

        public static void SetPreviousFillColor(DependencyObject obj, Brush value)
        {
            obj.SetValue(PreviousFillColorProperty, value);
        }

        //PreviousFillColor sa pouziva v StavadloElementsStyles.xaml  pre Path
        /// <summary>
        /// farbu useku pred nastavenim aktualneho stavu
        /// </summary>
        public static readonly DependencyProperty PreviousFillColorProperty =
            DependencyProperty.RegisterAttached("PreviousFillColor", typeof(Brush), typeof(PathHelper), new UIPropertyMetadata((Brush)App.Current.Resources["izolovanyUsekBrush"]));




        #region--- NASTAVENIE Panel.ZIndex---

        //Nastavenie Z-indexu pre Path
        public static void SetZIndex(System.Windows.Shapes.Path target, int value)
        {
            // if (target == null)
            //     return;
            target?.SetValue(Panel.ZIndexProperty, value);
            //  System.Windows.Shapes.Path chainedPath = target.GetValue(ChainedPathProperty) as System.Windows.Shapes.Path;
        }


        private static void SetCrossVisibility(System.Windows.Shapes.Path target, PATH_MODE mode)
        {

            int value = 15;
            ////Nastavenie ak existuje krizove pretinanie ciest - sklada sa z dvoch path-ov. Prepojenie medzi mini je pomocou SecondCrossPart.
            ////Ak je cesta v inom mode ako NORMAL, tak zabezpecuje, aby bola dana cast krizika vzdy na vrchu
            if (mode != PATH_MODE.NEINICIALIZOVANY)
            {
                if (target.GetValue(SecondCrossPartProperty) != null) //Path ma nastavenu att. property SecondCrossPartProperty
                {
                    System.Windows.Shapes.Path crossSecondPath = target.GetValue(SecondCrossPartProperty) as System.Windows.Shapes.Path;
                    int clickedCrossZIndex = Convert.ToInt32(target.GetValue(Panel.ZIndexProperty));
                    int secondCrossZIndex = Convert.ToInt32(crossSecondPath.GetValue(Panel.ZIndexProperty));
                    PATH_MODE clickedCrossMode = mode; //(PATH_MODE)crossPath.GetValue(ModeProperty);
                    PATH_MODE secondCrossMode = (PATH_MODE)crossSecondPath.GetValue(ModeProperty);

                    if (clickedCrossMode != PATH_MODE.NORMAL)
                        target.SetValue(Panel.ZIndexProperty, Math.Max(clickedCrossZIndex, secondCrossZIndex) + 1);
                    else
                        if (secondCrossMode != PATH_MODE.NORMAL)
                        crossSecondPath.SetValue(Panel.ZIndexProperty, Math.Max(clickedCrossZIndex, secondCrossZIndex) + 1);
                    else
                    {
                        target.SetValue(Panel.ZIndexProperty, value + 2);
                        crossSecondPath.SetValue(Panel.ZIndexProperty, value + 1);
                    }
                }
                else
                    target.SetValue(Panel.ZIndexProperty, value);
            }
        }

        #endregion--- NASTAVENIE Panel.ZIndex---


        #region ===PathStatus===
        //MH: oktober 2018

        public static Int64 GetPathStatus(DependencyObject obj)
        {
            return (Int64)obj.GetValue(PathStatusProperty);
        }

        public static void SetPathStatus(DependencyObject obj, Int64 value)
        {
            obj.SetValue(PathStatusProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty PathStatusProperty =
            DependencyProperty.RegisterAttached("PathStatus", typeof(Int64), typeof(PathHelper), new PropertyMetadata((Int64)0, OnPathStatusChanged));
        /*
         * POUZITIE:
         *  <Path x:Name="K806" Data="M0,0 L108,0 L108,10 L0,10 z"  
                      Height="10" Margin="86.749,114.099,0,0" Stretch="Fill"  Width="62.673" 
                      attProp:PathHelper.PathNumber="K806"
                      attProp:PathHelper.PathStatus="{Binding STC[K806].CombineStatus, Mode=OneWay}"
                      />
         */
        private static void OnPathStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //string typ1 = d.GetType().ToString(); //Windows.Shapes.Path
            Path p1 = d as Path;
            if (e.NewValue == null)
                return;
            OnPathStatusChanged(p1, e);
        }
        static void OnPathStatusChanged(Path p, DependencyPropertyChangedEventArgs e)
        {
            Int64 status = (Int64)e.NewValue;
            //anomynny Tuple typ
            (Int16 stav, Int16 uvolIzol, Int16 vyluka, Int16 podTyp) = StavadloHelper.GetStatusValues2(status);

            char podtypZnak = 'x';
            podtypZnak = StavadloHelper.GetPodTypZnak(podTyp);
            //podtyp 0  - podtypZnak = 'N'; Navestidlo
            //podtyp 1  - podtypZnak = 'n'; izolovany kolaj. usek
            //podtyp 2  - podtypZnak = '0'; neizolovany kolaj. usek
            //podtyp 3  - podtypZnak = 'v'; kolaj. usek patriaci vyhybke
            //podtyp 4  - podtypZnak = 's'; suhlas
            //podtyp 5  - podtypZnak = 'p'; pomocny signal

            if (podtypZnak == 'n') //izolovany usek
            {
                NastavUsek(p, new PRVOK_STAVADLA() { nazov = "", stav = stav, uvolizol = uvolIzol, vyluka = vyluka, podtyp = podtypZnak });
            }
            if (podtypZnak == '0') //neizolovany usek
            {
                NastavNeizolovanyUsek(p, new PRVOK_STAVADLA() { nazov = "", stav = stav, uvolizol = uvolIzol, vyluka = vyluka, podtyp = podtypZnak });
            }
        }


        #endregion ===PathStatus===

        /// <summary>
        /// pre objekt typu Path nastavi jeho ModeProperty, zapis do ModeProperty vykresli usek;
        /// </summary>
        /// <param name="myPath"></param>
        /// <param name="data"></param>
        static void NastavNeizolovanyUsek(Path myPath, PRVOK_STAVADLA data)
        {
            var isUnisolated = (bool)myPath.GetValue(PathHelper.IsUnisolatedPathProperty);// v xaml je nastavene: <Path ....attProp:PathHelper.IsUnisolatedPath="True".../>

            BitArray bityStav = new BitArray(new int[] { data.stav });
            Boolean obsadenyKO = bityStav[3];

            BitArray bityUvolizol = new BitArray(new int[] { data.uvolizol });
            Boolean sucastVC, sucastPC, sucastVC2, sucastPC2, obsadenieNeizolUseku, uvolnenaIzolacia;

            sucastVC = bityUvolizol[7];
            sucastPC = bityUvolizol[6];
            sucastVC2 = bityUvolizol[5];
            sucastPC2 = bityUvolizol[4];
            obsadenieNeizolUseku = bityUvolizol[3];
            uvolnenaIzolacia = bityUvolizol[0];

            if (isUnisolated)
            {
                myPath.SetValue(ModeProperty, PATH_MODE.NEIZOLOVANY_USEK);//len popis pre neizolovany usek ma biele znaky a fialove pozadie, inac sa farba useku nemeni!!!!
            }
            else
            {
                myPath.SetValue(ModeProperty, PATH_MODE.NORMAL);
            }

            if (uvolnenaIzolacia)
            {
                myPath.SetValue(ModeProperty, PATH_MODE.UVOLNENA_IZOLACIA);
            }
            //Simulacia: Udrzba->Oznacenie obsadenia
            //	108 Nazov:    608  Podtyp:0  Stav:0001 Vyluka:0000 UvolIzol:0008
            if (obsadenieNeizolUseku)
            {
                //myPath.SetValue(ModeProperty, PATH_MODE.OBSADENY_USEK);
                myPath.SetValue(ModeProperty, PATH_MODE.NEIZOLOVANY_USEK);
            }

            if (sucastPC)
            {
                myPath.SetValue(ModeProperty, PATH_MODE.POSUNOVA_CESTA);
            }
            if (sucastVC)
            {
                myPath.SetValue(ModeProperty, PATH_MODE.VLAKOVA_CESTA);
            }

            //if (bityStav[0] && obsadenyKO)
            if (obsadenyKO) //Obsadeny usek
            {
                myPath.SetValue(ModeProperty, PATH_MODE.OBSADENY_USEK);
            }

            //PATH_MODE aktMode = (PATH_MODE)myPath.GetValue(ModeProperty);//len pre kontrolu
            //string name = GetElementRealName(data.nazov);

            //MH: 21.02.2019 zaremovane!!!!
            //appEventsInvoker?.Invoke_NastavUsekSucastCesty_Event(myPath);
        }//NastavNeizolovanyUsek

        static void NastavUsek(Path myPath, PRVOK_STAVADLA data)
        {

            //-- Farba cesty je nadradena uvolnenej izolacii!!!--------
            //Ak sa postavi cesta cez vymenu. kt. je v stave UvolnenaIzolacia, potom priechodne rameno ma farbu posun. cesty
            //nepriechodne rameno ma farbu Uvolnenej izolacie. Ano je to OK!
            //Ak je kolajovy usek sucastou postavenej cesty a usek je v stave UvolnenaIzolacia, potom ma farbu cesty.
            //------------------------------------------------------------------------------------
            BitArray bityStav = new BitArray(new int[] { data.stav });
            Boolean obsadenyKO = bityStav[3];

            BitArray bityUvolizol = new BitArray(new int[] { data.uvolizol });
            Boolean sucastVC, sucastPC, sucastVC2, sucastPC2, obsadenieNeizolUseku, uvolnenaIzolacia;

            sucastVC = bityUvolizol[7];
            sucastPC = bityUvolizol[6];
            sucastVC2 = bityUvolizol[5];
            sucastPC2 = bityUvolizol[4];
            obsadenieNeizolUseku = bityUvolizol[3];
            uvolnenaIzolacia = bityUvolizol[0];

            //21.11.2013 ak usek je oznaceny ako zaciatok alebo koniec cesty a usek nie je sucast postavenej cesty, potom nemenit jeho stav(vyfarbenie), t.j. ukoncit funkciu
            //pre vyfarbovanie zaciatku a konca stavanej cesty pozri VlakovaCestaManager, PosunovaCestaManager

            if (data.stav > 0)
            {
                //pathModeProperty.SetValue(stavadloElementsData, PATH_MODE.NORMAL, null);
                myPath.SetValue(ModeProperty, PATH_MODE.NORMAL);
            }

            if (obsadenieNeizolUseku)
            {
                //pathModeProperty.SetValue(stavadloElementsData, PATH_MODE.OBSADENY_USEK, null);
                myPath.SetValue(ModeProperty, PATH_MODE.OBSADENY_USEK);
            }

            if (sucastPC)
            {
                //pathModeProperty.SetValue(stavadloElementsData, PATH_MODE.POSUNOVA_CESTA, null);
                myPath.SetValue(ModeProperty, PATH_MODE.POSUNOVA_CESTA);
            }
            if (sucastVC)
            {
                //pathModeProperty.SetValue(stavadloElementsData, PATH_MODE.VLAKOVA_CESTA, null);
                myPath.SetValue(ModeProperty, PATH_MODE.VLAKOVA_CESTA);
            }

            if (obsadenyKO)//Obsadeny kolajovy obvod: 3.bit stav
            {
                //pathModeProperty.SetValue(stavadloElementsData, PATH_MODE.OBSADENY_USEK, null);
                myPath.SetValue(ModeProperty, PATH_MODE.OBSADENY_USEK);
            }

            //MH: 22.11.2013 uprava://Ak je kolajovy usek sucastou postavenej cesty a usek je v stave UvolnenaIzolacia, potom ma farbu cesty.
            if (uvolnenaIzolacia)//Uvolnena izolacia: 0.bit uvolizol, ma vacsiu prioritu ako obsadeny KO
            {
                if (sucastPC)
                {
                    if (obsadenyKO)//Obsadeny kolajovy obvod: 3.bit stav
                    {
                        //pathModeProperty.SetValue(stavadloElementsData, PATH_MODE.OBSADENY_USEK, null);
                        myPath.SetValue(ModeProperty, PATH_MODE.OBSADENY_USEK);
                        return;
                    }
                    //pathModeProperty.SetValue(stavadloElementsData, PATH_MODE.POSUNOVA_CESTA, null);
                    myPath.SetValue(ModeProperty, PATH_MODE.POSUNOVA_CESTA);
                }
                else if (sucastVC)
                {
                    if (obsadenyKO)//Obsadeny kolajovy obvod: 3.bit stav
                    {
                        //pathModeProperty.SetValue(stavadloElementsData, PATH_MODE.OBSADENY_USEK, null);
                        myPath.SetValue(ModeProperty, PATH_MODE.OBSADENY_USEK);
                        return;
                    }
                    //pathModeProperty.SetValue(stavadloElementsData, PATH_MODE.VLAKOVA_CESTA, null);
                    myPath.SetValue(ModeProperty, PATH_MODE.VLAKOVA_CESTA);
                }
                else
                    //pathModeProperty.SetValue(stavadloElementsData, PATH_MODE.UVOLNENA_IZOLACIA, null);
                    myPath.SetValue(ModeProperty, PATH_MODE.UVOLNENA_IZOLACIA);
            }
            //string name = StavadloHelper.GetElementRealName(data.nazov);

            //TODO: treba to pouzit???? zaremovane 21.02.2019
            //appEventsInvoker?.Invoke_NastavUsekSucastCesty_Event(myPath);
        }//NastavUsek

        #endregion -------Attached Property pre Path----------
    }
}
