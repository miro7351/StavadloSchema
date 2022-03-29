using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Collections.Generic;

using PA.Stavadlo.Data;

using PA.Stavadlo.Infrastructure;
using PA.Stavadlo.Infrastructure.Enums;
using PA.Stavadlo.Infrastructure.PathHelperPA;


using PA.Stavadlo.Data.Telegrams;

namespace PA.Stavadlo.UserControls
{
    /// <summary>
    /// Interaction logic for UC_MapaST08.xaml
    /// </summary>
    public partial class UC_MapaST08 : UserControl
    {

        #region --FIELDS/PROPERTIES--
        /*
        /// <summary>
        /// List do ktoreho sa ulozia objekty na ktore bol klik
        /// </summary>
        private List<Object> hitResultsList;

        StavadloGlobalData GlobalData;
        StavadloModel stavadloModel;
        Communicator AppCommunicator;
        PlayModeEventDriver PMeventDriver;

        private StavadloViewModel stavadloViewModel;
        */
        private DispatcherTimer BlinkTimer; //timer pre blikanie elementov (vyhybka s semafory) v chybovom stave
        private bool blinkTimerFlag;

        //Ako a kde nastavit jeho hodnotu??????????
        //private int pocetElementov_ChybovyStav;//pocet elementov v chybovom stave: Vyhybky a semafory
        //polozka Udrzba->Vybavenie poruch by mala byt pristupna len ak je viac poruch ako 5;

        //Brushe pouzivane pri vyfarbovani vyhybiek
        SolidColorBrush vyhybkaRozrezPoruchaColor;
        SolidColorBrush redColor;
        SolidColorBrush blackBrush;
        SolidColorBrush transparentBrush;
        SolidColorBrush redBrush;
        SolidColorBrush ramenoVyhybkyPoruchaBrush;
        SolidColorBrush vyhybkaRozrezPoruchaBrush;

        #endregion --FIELDS/PROPERTIES--

        #region == ctor, MapaStavadla_Loaded, MapaStavadla_Unloaded ==

        public UC_MapaST08()
        {
            InitializeComponent();
            /*
            GlobalData = StavadloGlobalData.Instance;
            stavadloModel = StavadloModel.Instance;
            AppCommunicator = Communicator.Instance;
            PMeventDriver = PlayModeEventDriver.Instance;

            hitResultsList = new List<object>();


            vyhybkaRozrezPoruchaColor = App.Current.Resources["vyhybkaRozrezPoruchaColor"] as SolidColorBrush;
            redColor = App.Current.Resources["redColor"] as SolidColorBrush;
            blackBrush = App.Current.Resources["BlackBrush"] as SolidColorBrush; ;
            transparentBrush = App.Current.Resources["TransparentBrush"] as SolidColorBrush; ;
            redBrush = App.Current.Resources["RedBrush"] as SolidColorBrush;
            vyhybkaRozrezPoruchaBrush = App.Current.Resources["vyhybkaRozrezPoruchaBrush"] as SolidColorBrush;
            ramenoVyhybkyPoruchaBrush = App.Current.Resources["ramenoVyhybkyPoruchaBrush"] as SolidColorBrush;

            //MH: preto je tu viewModel, aby sme vedeli spustit a stopnut casovac pri Loaded a Unloaded
            mainDockPanel.DataContext = stavadloViewModel = new StavadloViewModel();//MH v xaml sa nenastavuje DataContext
            InitBlinkTimer();

            //blinkRect1.Visibility = System.Windows.Visibility.Hidden;//signalizuje blikanie, len pre vyvoj

            //handler ak sa prijme telegram cislo 151, aby sa nastavil kurzor na zadany semafor
            if (GlobalData.WorkMode)
                AppCommunicator.TlgMessage151Recieved += TlgMessage151ReceivedHandler;
            else  //play mode
            {
                PMeventDriver.PMtlg151ReceivedEvent += PMeventDriver_PMtlg151ReceivedHandler;
                PMeventDriver.ElementMovedEvent += PMeventDriver_ElementMovedEventHandler;
            }
            */
        }//ctor


        private void MapaStavadla_Loaded(object sender, RoutedEventArgs e)
        {
            /*
            BlinkTimer.Start();
            bool logicDisconnected = false;//v RELEASE mode sa stale komunikuje so serverom
#if DEBUG             
            logicDisconnected = GlobalData.LogicDisconnected;//v DEBUG mode nastavenie parametra logicDisconnected podla settings suboru
#endif           
            if (logicDisconnected) //nastavenie pociatocnej pozicie vyhybiek, ak sa neprijima inicializacny telegram z Logicu;
            {
                stavadloModel.FirstTelegram141Received = true;
                InitVymeny(); //aby pociatocny stav vyhybiek stavadla odpovedal pohotovostnemu stavu
            }
            */
        }


        private void MapaStavadla_Unloaded(object sender, RoutedEventArgs e)
        {
            BlinkTimer.Stop();
        }

        #endregion == ctor, MapaStavadla_Loaded, MapaStavadla_Unloaded ==

        //==============================================================================

        #region --FUNKCIE--
        void PMeventDriver_PlayModeClickEvent(String elementName)
        {
            //dostaneme presnu suradnicu bodu monitora, na ktory sa kliklo a potrebujeme ho previest na suradnice marginu podla ktoreho sa nastavi virtualny kurzor
            //Point gridPosition = GridMapAndElements.PointToScreen(new Point(0, 0));   //35, 18
            //double x = clickedPoint.X - gridPosition.X;
            //double y = clickedPoint.Y - gridPosition.Y;
            //SetVirtualKurzorAt(x, y);
            //SetMousePosition(elementName);
        }

        private void SetVirtualKurzorAt(double x, double y, double dx, double dy)
        {
            // virtualKurzor.Margin = new Thickness(x, y, dx, dy);
        }

        /*
        /// <summary>
        /// Zmena pozicie posuvacieho objektu (lokomotiva, panak, vykolajka) pri play mode
        /// </summary>
        /// <param name="obj"></param>
        void PMeventDriver_ElementMovedEventHandler(MoveableData obj)
        {
            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
            {
                var uiElement = GridMapaStavadla.Children.OfType<FrameworkElement>().Where(e => e.Name == obj.Name).FirstOrDefault();

                if (uiElement is UC_Vykolajka || uiElement is UClocomotive || uiElement is UC_PanakUss) //ak je to vykolajka, panak, lokomotiva nastav jeho poziciu
                {
                    try
                    {
                        uiElement.RenderTransform = new MatrixTransform();
                        //upravi velkost otoci...
                        double scale_size = 1;
                        ScaleTransform scale = new ScaleTransform(scale_size, scale_size);
                        RotateTransform rotate = new RotateTransform(0);
                        TranslateTransform translate = new TranslateTransform(obj.X, obj.Y);
                        SkewTransform skew = new SkewTransform();
                        TransformGroup tgroup = new TransformGroup();
                        tgroup.Children.Add(scale);
                        tgroup.Children.Add(rotate);
                        tgroup.Children.Add(translate);
                        tgroup.Children.Add(skew);
                        uiElement.RenderTransform = tgroup;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Problem pri nastaveni polohy pohybliveho kontrolu: " + ex.Message);
                    }
                }
            }));
        }

        */
        /*
        /// <summary>
        /// handler pre event generovany z udajov log subore
        /// </summary>
        /// <param name="e"></param>
        void PMeventDriver_PMtlg151ReceivedHandler(EventArgs3 e)
        {
            string elementName = e.Variant.signalName;
            SetMousePosition(elementName);
        }
        */
        //Task zvukTask = new Task(() => Console.Beep(1000, 300));

        /// <summary>
        /// inicializacia timera BlinkTimer pre blikanie elementov ak su v chybovom stave;
        /// BlinkTimer tika stale!!!!
        /// </summary>
        private void InitBlinkTimer()
        {
            /*
            blinkTimerFlag = false;
            BlinkTimer = new DispatcherTimer();
            BlinkTimer.Interval = new TimeSpan(0, 0, 0, 0, 700);
            BlinkTimer.Tick += async(s, e) =>
            {
                blinkTimerFlag = !blinkTimerFlag;
                stavadloViewModel.TimerFlag = blinkTimerFlag;
                //TimerFlag zabezpecuje synchronizaciu zobrazovania blikajucich (chybovych ) stavov prvkov stavadla.
                if (!blinkTimerFlag && (StavadloModel.Instance.BlinkedControlsList.Count > 0))
                {
                    if (GlobalData.SoundEnabled)// SoundEnabled sa nastavuje v pomocou buttonu pre zvuk
                    {
                        await Task.Factory.StartNew(() => Console.Beep(stavadloViewModel.BeepHz, stavadloViewModel.BeepTime));
                    }
                }
                //Semafor, vymena, UC_VstupOdchod, UC_Derailer si sami  zobrazuju normalny stav/chybovy stav podla hodnoty svojej dependency property TimerFlag, 
                //ktora je bindovana na stavadloViewModel.TimerFlag!!!!!
            };
            */
        }

        /// <summary>
        /// nastavi vsetky vymeny-vyhybky do pohotovostneho stavu
        /// </summary>
        void InitVymeny()
        {
            var vymeny = GridMapaStavadla.Children.OfType<BaseTrainSwitchControl>();
            foreach (var vymena in vymeny)
                vymena.NastavPohotovostnyStav();//Pre pohotovostny stav je stav=0x0001, prechodne je Rameno+, neprechodne (zaciernene) je Rameno-;
        }

        /*
        //Po postaveni posunovej alebo vlakovej cesty server vysle tlg. 151 s obsahom '1;S386d'   e.Variant.var_number=1; e.Variant.signalName=S386d
        //e.Variant.signalName je Name pre semafor, na ktory sa presunie kurzor
        /// <summary>
        /// handler pre event pri prijme tlg.151
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TlgMessage151ReceivedHandler(object sender, EventArgs3 e)
        {
            if ((GlobalData.CurrentMenuMode == CURRENT_MENU_MODE.PCESTA) || (GlobalData.CurrentMenuMode == CURRENT_MENU_MODE.VCESTA))
            {
                //var n1 = e.Variant.var_number;
                //string elementName = e.Variant.signalName; je Name pre smafor, na ktory sa presunie kurzor
                //SetMousePosition(elementName);
                SetMousePosition(e.Variant.signalName);
            }
        }
        */
        /*
        /// <summary>
        /// Nastavi polohu kurzora ak sa prijme zo servera telegram cislo 151 Variant;
        /// kurzor nastavi na semafor, ktoreho Name je v telegrame;
        /// </summary>
        /// <param name="elementName"></param>
        private void SetMousePosition(string elementName) //MH: marec 2019 prerobene pomocou LINQ
        {
            if (string.IsNullOrEmpty(elementName))
                return;
            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
            {
                //var elements1 = GridMapaStavadla.Children.Cast<FrameworkElement>();//cast convertuje kazdy prvok z children, ak sa prvok neda konvertovat, potom vyvola vynimku!!
                var elements = GridMapaStavadla.Children.OfType<FrameworkElement>();//elements obsahuje len tie prvky z Children, ktore sa daju konvertovat, nevyvola vynimku

                FrameworkElement findElement = elements?.Where(e => e.Name == elementName).FirstOrDefault();

                if (findElement == null)
                {
                    elementName = "_" + elementName;
                    findElement = elements.Where(e => e.Name == elementName).FirstOrDefault();
                }

                if (findElement != null)
                {
                    try
                    {
                        //Converts a Point that represents the current coordinate system of the visual into a Point in screen coordinates.
                        Point locationFromScreen = findElement.PointToScreen(new Point(findElement.ActualWidth / 2, findElement.ActualHeight / 2));
                        if (GlobalData.WorkMode)//nastavenie app kurzora na findElement
                            System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)locationFromScreen.X, (int)locationFromScreen.Y);
                        else
                        {    //PlayMode
                            Point center = findElement.TransformToAncestor(GridMapaStavadla).Transform(new Point(findElement.ActualWidth / 2, findElement.ActualHeight / 2));
                            //MH: 23.04.2019 event sa zachyti v MainWindowStavadlo.xaml.cs a nastavi polohu virtualneho kurzora
                            PMeventDriver.RaiseElementPositionEvent(center);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Problem pri získani PointToScreen: " + ex.Message);
                    }
                }
            }));
            return;
        }//SetMousePosition
        */
        #endregion --FUNKCIE--

        //Spracovanie udalosti pomocou routed eventu; Event zavedeny v xaml subore v Style pre Path
        //Odchytava sa v StavadloPage.xaml

        public void PathMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }
            /*
            /// <summary>
            /// Handler pre zachytenie kliku na usek;
            /// Pozri Style pre Path;
            /// Odpali routed event
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void PathMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                Path myPath = sender as Path;//usek na ktory sa kliklo
                if (myPath == null)
                    return;

                Path clickedPath = DoHitTest(sender, e);

                if (clickedPath == null)
                    return;
                CURRENT_MENU_MODE currentMenuMode = GlobalData.CurrentMenuMode;
                if (currentMenuMode == CURRENT_MENU_MODE.STAVANIE_CESTY ||
                    currentMenuMode == CURRENT_MENU_MODE.SIMULACIA_OBSADENIA ||
                    currentMenuMode == CURRENT_MENU_MODE.VCESTA ||
                    currentMenuMode == CURRENT_MENU_MODE.PCESTA ||
                    currentMenuMode == CURRENT_MENU_MODE.ZRUSENIE_PORUCHY ||
                    currentMenuMode == CURRENT_MENU_MODE.INFORMACIE_O_ZARIADENI ||
                    currentMenuMode == CURRENT_MENU_MODE.OZNACENIE_OBSADENIA ||
                    currentMenuMode == CURRENT_MENU_MODE.INFORMACIA_O_PRVKU ||
                    currentMenuMode == CURRENT_MENU_MODE.UVOLNENIE_IZOLACIE ||
                    currentMenuMode == CURRENT_MENU_MODE.SIMULACIA_ZIADOSTI_O_SUHLAS ||
                    currentMenuMode == CURRENT_MENU_MODE.UDELENIE_SUHLASU ||
                    currentMenuMode == CURRENT_MENU_MODE.RUSENIE_SUHLASU ||
                    currentMenuMode == CURRENT_MENU_MODE.SIMULACIA_RUSENIA_SUHLASU ||
                    currentMenuMode == CURRENT_MENU_MODE.SIMULACIA_UDELENIA_SUHLASU ||
                    currentMenuMode == CURRENT_MENU_MODE.ZIADOST_O_SUHLAS ||
                    currentMenuMode == CURRENT_MENU_MODE.ZRUSENIE_VARIANTU_CESTY ||
                    currentMenuMode == CURRENT_MENU_MODE.VYLUKA_POSUNOVEJ_CESTY ||
                    currentMenuMode == CURRENT_MENU_MODE.VYLUKA_VLAKOVEJ_CESTY)
                {
                    //RaisePathConstructionAttachedEvent(clickedPath);
                    e.Handled = true;
                }
            }//PathMouseLeftButtonDown

            */
            #region =================HIT TEST==================

            /*
            /// <summary>
            /// vrati Path na ktory sa kliklo;
            /// nastavi PathHelper.IsCommonSwitchPartProperty ak sa kliklo na usek v spolocnej casti vymeny
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            /// <returns></returns>
            private Path DoHitTest(object sender, MouseButtonEventArgs e)
            {
                Point pt = e.GetPosition((UIElement)GridMapaStavadla);    // GridWithPaths == hlavny grid v ktorom su vsetky graficke komponenty
                // Clear the contents of the list used for hit test results.
                hitResultsList.Clear();
                // Set up a callback to receive the hit test result enumeration.
                VisualTreeHelper.HitTest(GridMapaStavadla,                  //Klik testovanie
                    new HitTestFilterCallback(MyHitTestFilter),     //filter
                    new HitTestResultCallback(MyHitTestResult),     //ResultCallBack
                    new PointHitTestParameters(pt));                //Bod
                if (hitResultsList.Count > 0)
                {
                    //MH: pridane 11.12.2013--
                    if (hitResultsList.Count >= 2) //ak sa kliklo na miesto, kde pod usekom je este dalsi usek, useky su nad sebou v spolocnej casti vymeny
                        ((Path)hitResultsList[0]).SetValue(PathHelper.IsCommonSwitchPartProperty, true);
                    else
                        ((Path)hitResultsList[0]).SetValue(PathHelper.IsCommonSwitchPartProperty, false);
                    //-------------------------
                    return (Path)hitResultsList[0];
                }
                return null;

            }//DoHitTest

            //prida do zoznamu vsetky pathy na ktore sa kliklo
            public HitTestResultBehavior MyHitTestResult(HitTestResult result)
            {
                // Add the hit test result to the list that will be processed after the enumeration.
                hitResultsList.Add(result.VisualHit);

                // Set the behavior to return visuals at all z-order levels. 
                return HitTestResultBehavior.Continue;
            }
            */

            //odfiltruje vsetky UserControly
            public HitTestFilterBehavior MyHitTestFilter(DependencyObject o)
        {
            // Test for the object value you want to filter. 
            if (o is BaseTrainSwitchControl)         //ak sa klikne na vlastny USER_CONTROL, tak preskoc
            {
                // Visual object and descendants are NOT part of hit test results enumeration. 
                return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
            }
            if (o.GetType() != typeof(Path))
            {
                return HitTestFilterBehavior.ContinueSkipSelf;
            }
            else
            {
                // Visual object is part of hit test results enumeration. 
                return HitTestFilterBehavior.Continue;
            }
        }

        #endregion


        #region ==RoutedEvents==

        //Routed Event pre kliknutie na path. Spracuvava sa v StavadloPage.xaml.cs
        public static readonly RoutedEvent PathConstructionAttachedEvent = EventManager.RegisterRoutedEvent("PathConstruction", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UC_MapaST08));

        /*
        //event sa odpaluje v vo funkcii TrainPathMouseLeftButtonDown, pozri kod vyssie
        public void RaisePathConstructionAttachedEvent(System.Windows.Shapes.Path clickedPath)
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(UC_MapaStavadla.PathConstructionAttachedEvent, new PathEventArgs() { ClickedPath = clickedPath });
            RaiseEvent(newEventArgs);
        }


        public static void AddPathConstructionAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ((UIElement)d).AddHandler(UC_MapaStavadla.PathConstructionAttachedEvent, handler);
        }
        public static void RemovePathConstructionAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ((UIElement)d).RemoveHandler(UC_MapaStavadla.PathConstructionAttachedEvent, handler);
        }

        */
        #endregion==RoutedEvents==
        /*
        //===LEN PRE LADENIE===
        //   minus = bityStav[1];//WPF: neprechodne (cierne ) je Rameno2, prechodne Rameno1; stara aplikacia: true-prechodne je -Rameno vymeny, neprechodne +Rameno vymeny (vyfarbit na cierno  +Rameno)
        //plus  = bityStav[0];   //WPF: neprechodne (cierne ) je Rameno1, prechodne Rameno2; stara aplikacia: true-prechodne je +Rameno vymeny, neprechodne -Rameno vymeny (vyfarbit na cierno  -Rameno)
        private void SimulData_Click1(object sender, RoutedEventArgs e)
        {

            AppCommunicator.SendRequestToServer(141);   //ziadost pre Server, aby poslal telegram s poslednym stavom stavadla, spusti sa len raz po otvoreni stranky
          
            //AppCommunicator.SendRequestToServer(191);   //ziadost pre Server, aby poslal telegram c.191 do HMI, len pre ST22

            //stavadloViewModel.SpracujData(1);//L808 je cerveny
            //Int16 stav = 0x20 | 0x01; //DEN AUT SPR=1
            //stavadloViewModel.STC["POMSIG"].Stav = stav;
            //stavadloViewModel.STC["POMSIG"].CombineStatus = StavadloHelper.CreateCombineStatus((Int16)stav, (Int16)0, (Int16)0, (Int16)0);

            //pre testovanie samostatnej vymeny V10
            //stavadloViewModel.TT1 = StavadloHelper.CreateCombineStatus((Int16)0x0001, (Int16)0, (Int16)0, (Int16)0);
        }
        */
        private void SimulData_Click2(object sender, RoutedEventArgs e)
        {
            //stavadloViewModel.SpracujData(2);//L808 je zeleny
            //Int16 stav = 0x20 | 0x00; //DEN AUT SPR=0
            //stavadloViewModel.STC["POMSIG"].Stav = stav;
            //stavadloViewModel.STC["POMSIG"].CombineStatus = StavadloHelper.CreateCombineStatus((Int16)stav, (Int16)0, (Int16)0, (Int16)0);
            //stavadloViewModel.TT1 = StavadloHelper.CreateCombineStatus((Int16)0x0805, (Int16)0x0000, (Int16)0x0080, (Int16)0);
            //stavadloViewModel.TT1 = StavadloHelper.CreateCombineStatus((Int16)0x0002, (Int16)0x0000, (Int16)0x0000, (Int16)0);
        }
        /*
        //zobrazi okno kde su hodnoty prijate v telegrame;
        private void btnSTC_Click(object sender, RoutedEventArgs e)
        {
            WindowSTC windowSTC = new WindowSTC();
            windowSTC.Show();
        }
        */
        /*
        //zobrazi okno kde su useky, semafory, vymeny a vykolajka v roznom stave
        private void btnST22prvky_Click(object sender, RoutedEventArgs e)
        {
            Window windowControlTest = new WindowControlTest();
            windowControlTest.Show();
        }

        */

        /*
        //zobrazi okno VymenaTestWindow
        private void btnVymenaTest_Click(object sender, RoutedEventArgs e)
        {
            Window vymenaTest = new VymenaTestWindow();
            vymenaTest.Show();

        }
        */
        /*
        //test pre zobrazenie suprav na usekoch; len pre ST22
        private void btnSupravyTest_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            TestSupravy();
            //udaje z tlg.191
            Window windowSupravaData = new WindowSupravyData();
            windowSupravaData.Show();
#endif
        }

       

#if DEBUG
        //V tlg. 191 pridu cisla kolaji na ktorych su supravy
        void TestSupravy()
        {
            Sets mySets = new Sets();
            mySets.Set();
            mySets.set[0] = new SupravaData { cislo_supravy = 7, kolaj = "383", datum = "01.03.2019", cas = "10:02:43", priorita = 1 };
            mySets.set[1] = new SupravaData { cislo_supravy = 8, kolaj = "383", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[2] = new SupravaData { cislo_supravy = 9, kolaj = "383", datum = "01.03.2019", cas = "10:02:43", priorita = 1 };
            mySets.set[3] = new SupravaData { cislo_supravy = 10, kolaj = "383", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[4] = new SupravaData { cislo_supravy = 12, kolaj = "384", datum = "01.03.2019", cas = "10:02:43", priorita = 1 };
            //hala UB
            mySets.set[5] = new SupravaData { cislo_supravy = 20, kolaj = "894", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[6] = new SupravaData { cislo_supravy = 21, kolaj = "892", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[7] = new SupravaData { cislo_supravy = 22, kolaj = "893", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[8] = new SupravaData { cislo_supravy = 23, kolaj = "891", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[9] = new SupravaData { cislo_supravy = 24, kolaj = "891", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            //ZPO-1
            mySets.set[10] = new SupravaData { cislo_supravy = 2, kolaj = "888", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[11] = new SupravaData { cislo_supravy = 3, kolaj = "887", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[12] = new SupravaData { cislo_supravy = 4, kolaj = "887", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[13] = new SupravaData { cislo_supravy = 5, kolaj = "303", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[14] = new SupravaData { cislo_supravy = 6, kolaj = "303", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[15] = new SupravaData { cislo_supravy = 15, kolaj = "K863", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[16] = new SupravaData { cislo_supravy = 18, kolaj = "K864", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[17] = new SupravaData { cislo_supravy = 71, kolaj = "K870", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[18] = new SupravaData { cislo_supravy = 72, kolaj = "K870", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[19] = new SupravaData { cislo_supravy = 87, kolaj = "K870b", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[20] = new SupravaData { cislo_supravy = 88, kolaj = "K870b", datum = "01.03.2019", cas = "10:02:43", priorita = 1 };
            mySets.set[21] = new SupravaData { cislo_supravy = 82, kolaj = "K162", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[22] = new SupravaData { cislo_supravy = 81, kolaj = "K162", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };

            mySets.set[23] = new SupravaData { cislo_supravy = 83, kolaj = "K162", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[24] = new SupravaData { cislo_supravy = 42, kolaj = "K894", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[25] = new SupravaData { cislo_supravy = 90, kolaj = "K963", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[26] = new SupravaData { cislo_supravy = 91, kolaj = "K964", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[27] = new SupravaData { cislo_supravy = 92, kolaj = "K998", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[28] = new SupravaData { cislo_supravy = 94, kolaj = "K998A", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[29] = new SupravaData { cislo_supravy = 98, kolaj = "K998", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[30] = new SupravaData { cislo_supravy = 80, kolaj = "800", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[31] = new SupravaData { cislo_supravy = 81, kolaj = "800", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };

            mySets.set[32] = new SupravaData { cislo_supravy = 84, kolaj = "K844a", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[33] = new SupravaData { cislo_supravy = 44, kolaj = "804", datum = "01.03.2019", cas = "10:02:43", priorita = 1 };
            mySets.set[34] = new SupravaData { cislo_supravy = 38, kolaj = "K386a", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };

            mySets.set[35] = new SupravaData { cislo_supravy = 84, kolaj = "K162", datum = "01.03.2019", cas = "10:02:43", priorita = 1 };
            mySets.set[36] = new SupravaData { cislo_supravy = 36, kolaj = "K385", datum = "01.03.2019", cas = "10:02:43", priorita = 1 };
            mySets.set[37] = new SupravaData { cislo_supravy = 37, kolaj = "K385", datum = "01.03.2019", cas = "10:02:43", priorita = 1 };

            mySets.set[38] = new SupravaData { cislo_supravy = 86, kolaj = "K866", datum = "01.03.2019", cas = "10:02:43", priorita = 1 };
            mySets.set[39] = new SupravaData { cislo_supravy = 87, kolaj = "K866", datum = "01.03.2019", cas = "10:02:43", priorita = 1 };
            mySets.set[40] = new SupravaData { cislo_supravy = 70, kolaj = "K870a", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[41] = new SupravaData { cislo_supravy = 71, kolaj = "K870a", datum = "01.03.2019", cas = "10:02:43", priorita = 1 };

            mySets.set[42] = new SupravaData { cislo_supravy = 26, kolaj = "K326", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[43] = new SupravaData { cislo_supravy = 27, kolaj = "K326", datum = "01.03.2019", cas = "10:02:43", priorita = 1 };

            mySets.set[44] = new SupravaData { cislo_supravy = 63, kolaj = "K863K", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[45] = new SupravaData { cislo_supravy = 64, kolaj = "K863K", datum = "01.03.2019", cas = "10:02:43", priorita = 1 };

            mySets.set[46] = new SupravaData { cislo_supravy = 64, kolaj = "K864", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[47] = new SupravaData { cislo_supravy = 64, kolaj = "K864", datum = "01.03.2019", cas = "10:02:43", priorita = 1 };

            mySets.set[48] = new SupravaData { cislo_supravy = 30, kolaj = "K330", datum = "01.03.2019", cas = "10:02:43", priorita = 0 };
            mySets.set[49] = new SupravaData { cislo_supravy = 31, kolaj = "K330", datum = "01.03.2019", cas = "10:02:43", priorita = 1 };

            mySets.NumOfSets = 50;//max hodnota NumOfSets=50 !!!

            stavadloModel.WriteSupravyData(mySets);
            GlobalData.Log?.CustomInfo(GlobalData.LogHeaders["TLG191"] + SerDesManager.SerializeToBase64<Sets>(mySets));
        }
       
#endif
     
        //private void btnSupravyClear_Click(object sender, RoutedEventArgs e)
        //{
        //    stavadloModel.ClearSupravyCollection();
        //}

        /// <summary>
        /// Zobrazi okno o symboloch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OSymboloch_Click(object sender, RoutedEventArgs e)
        {
            Window oSymboloch = new OSymbolochWindow2();
            oSymboloch.Show();
        }
  */ 
        //======================
    }
}
