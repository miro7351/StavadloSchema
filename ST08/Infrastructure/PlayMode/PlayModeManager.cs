using System;
using System.IO;
using System.Timers;
using System.Windows;
using System.Globalization;
using System.Text.RegularExpressions;

using Stavadlo22.Data;
using Stavadlo22.Data.Telegrams;
using Stavadlo22.Infrastructure.Enums;
using Stavadlo22.Infrastructure.Communication;


namespace Stavadlo22.Infrastructure.PlayMode
{
    /// <summary>
    /// Delegate typ pre nacitanu spravu z log suboru
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void RecordMessageRead(object sender, RecordEventArgs e);


    /// <summary>
    /// Riadi zobrazovanie udalosti v PlayMode;
    /// Instancia sa vytvori v UC_LogPlayerControl.xaml.cs
    /// </summary>
    class PlayModeManager
    {
        StavadloGlobalData globalData;//globalne data pre aplikaciu
        StavadloModel stavadloModel;
        MessageWriter MsgWriter;
        AppEventsInvoker appEventsInvoker;
        PlayModeEventDriver PMeventDriver;

        public PlayModeManager(string sourceFile, double timerTicksMilliseconds)
        {
            selectedRole = string.Empty;
            globalData = StavadloGlobalData.Instance;
            stavadloModel = StavadloModel.Instance;
            MsgWriter = MessageWriter.Instance;
            appEventsInvoker = AppEventsInvoker.Instance;
            PMeventDriver = PlayModeEventDriver.Instance;

            logFileRecordsCount = 0;
            logSourceFile = sourceFile;
            Initialize();                   //nastavi sa odDate, doDate, recordsCount na zaklade zadaneho suboru

            InitializeTimer(timerTicksMilliseconds);
            currentRecord = 0;
            stack = new PlayBuffer(200);    //bude si uchovavat 200 poslednych zaznamov nacitanych zo suboru
            if (logFileRecordsCount == 0)
                return;
            stream = new StreamReader(sourceFile);
        }


        public PlayModeManager(string sourceFile, double timerTicksMillis, double screenHeightProportionalityData, double screenWidthProportionalityData)
        {
            selectedRole = string.Empty;
            logFileRecordsCount = 0;
            logSourceFile = sourceFile;
            Initialize();                   //nastavi sa odDate, doDate, recordsCount na zaklade zadaneho suboru
            InitializeTimer(timerTicksMillis);
            currentRecord = 0;

            this.screenHeightProporcionality = screenHeightProportionalityData;
            this.screenWidhtProporcionality = screenWidthProportionalityData;

            stack = new PlayBuffer(200);    //bude si uchovavat 200 poslednych zaznamov
            if (logFileRecordsCount == 0)
                return;
            stream = new StreamReader(sourceFile);
        }


        #region --FIELDS/PROPERTIES--

        public double primaryScreenWidth;//screnn width pre PC kde HMI bola spustena aplikacia
        public double primaryScreenHeight;//screnn height pre PC kde HMI bola spustena aplikacia

        public double actualWindowWidth;//aktualna sirka pre okno aplikacie pre PC kde HMI bola spustena aplikacia
        public double actualWindowHeight;//aktualna vyska pre okno aplikacie pre PC kde HMI bola spustena aplikacia
        public double windowTop;// aktualna Top poloha pre okno aplikacie 
        public double windowLeftp;// aktualna Left poloha pre okno aplikacie 

        public const string HMI_separator = ">>|";                   //pouziva sa aj v HistManagerViewModel - pri zisteni rozlisenia monitora, na ktorom boli logy vytvorene, preto to je public static

        private string logSourceFile;      //zdrojovy subor pre citanie

        private DateTime? odDate;       //zaznamy od
        private DateTime? doDate;       //zaznamy do

        long logFileRecordsCount;
        public long LogFileRecordsCount
        {
            get
            {
                return logFileRecordsCount;
            }       //pocet zaznamov v subore
        }
        private long currentRecord;     //riadok aktualneho zaznamu
        private PLAYING_MODE playMode;  //mod prehravania
        private PlayBuffer stack;       //stack pre posledne zaznamy
        private StreamReader stream;    //strem pre citanie zo suboru

        private Timer timer;

        public event RecordMessageRead RecordMessageReadEvent;

        /// <summary>
        /// Urcuje, ci sa ma nastavovat pozicia kurzora podla udajov z logu, alebo podla uzivatela
        /// </summary>
        public bool SetMousePosition { get; set; }


        public DateTime? DateOd
        {
            get { return odDate; }
        }

        public DateTime? DateDo
        {
            get { return doDate; }
        }

        public double TimerTicksMillis
        {
            get
            {
                if (timer != null)
                    return timer.Interval;
                else
                    return 0;
            }
            set
            {
                if (timer != null)
                    timer.Interval = value;
            }
        }

        double screenWidhtProporcionality;
        double screenHeightProporcionality;

        string selectedRole;
        /// <summary>
        /// Rola, pre ktoru boli vybrate zaznamy
        /// </summary>
        public string SelectedRole
        {
            get
            {
                return selectedRole;
            }
        }

        #endregion --FIELDS/PROPERTIES--



        //nastavenie timera
        private void InitializeTimer(double timerTicksMillis)
        {
            timer = new Timer();
            timer.Interval = timerTicksMillis;
            timer.Elapsed += (s, e) =>{ Forward(); };
        }

        //nastavi sa odDate, doDate, recordsCount na zaklade zadaneho suboru
       
        private void Initialize()
        {
            if (!string.IsNullOrEmpty(logSourceFile) && File.Exists(logSourceFile))
            {
                try
                {
                    using (StreamReader otvorenySubor = new StreamReader(logSourceFile))
                    {
                        string nacitanyRiadok;
                        nacitanyRiadok = otvorenySubor.ReadLine(); //AK sa nacitaju udaje z DB 1. riadok obsahuje rolu napr.Udrzba!!!
                        selectedRole = nacitanyRiadok.Trim();
                        if (string.IsNullOrEmpty(nacitanyRiadok) )
                        {
                            MessageBox.Show("Súbor neobsahuje záznamy");
                            return;
                        }

                        odDate = VyberDTZoStringu(SelectDtString(nacitanyRiadok));
                        if( odDate == null)
                        {
                            nacitanyRiadok = otvorenySubor.ReadLine();
                            odDate = VyberDTZoStringu(SelectDtString(nacitanyRiadok));
                        }
                        int lines = 1;
                        //while ((nacitanyRiadok = otvorenySubor.ReadLine()) != null)
                        //    lines++;
                        while (!otvorenySubor.EndOfStream)
                        {
                            nacitanyRiadok = otvorenySubor.ReadLine();
                            lines++;
                        }

                        doDate = VyberDTZoStringu(SelectDtString(nacitanyRiadok));
                        logFileRecordsCount = lines;

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Chyba pri čítaní zo suboru: " + logSourceFile + "\n" + ex.StackTrace, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void Close()
        {
            timer.Stop();
            timer.Enabled = false;
            if (stream != null)
            {
                stream.DiscardBufferedData();
                stream.Close();
            }

            App.Current.Resources["visibilityPlayWindow"] = Visibility.Hidden;
            App.Current.Resources["visibilityVirtualCursor"] = Visibility.Hidden;
        }

        public void Close(string val)
        {
            timer.Stop();
            timer.Enabled = false;
        }

        #region ===metody pre tlacitka prehravaca===

        /// <summary>
        /// Krok dopredu
        /// </summary>
        /// <returns></returns>
        public void Forward()
        {
            string line = ReadLine();
            if (line != null)
            {
                currentRecord++;
                RecordHandler(line);
            }
        }
        /// <summary>
        /// Krok dozadu
        /// </summary>
        /// <returns></returns>
        public void Backward()
        {
            if (stack.CanPop)
            {
                currentRecord--;
                RecordHandler(stack.Pop());
            }
            else
                RecordHandler(null);
        }
        /// <summary>
        /// Zacatie od znova
        /// </summary>
        public void Stop()
        {
            timer.Stop();
            timer.Enabled = false;
            stream.BaseStream.Position = 0;
            stream.DiscardBufferedData();
            stack.Flush();
            currentRecord = 0;
            Forward();
        }

        public void Play()
        {
            if (!timer.Enabled)
                timer.Start();
        }

        public void Pause()
        {
            timer.Stop();
        }

        #endregion ===metody pre tlacitka prehravaca===



        /// <summary>
        /// Tu sa bude spracovavat log zaznam
        /// </summary>
        /// <param name="record"></param>
        private void RecordHandler(string record)
        {
            //if (record != null)
            if( !string.IsNullOrEmpty(record))
            {
                int recordNumber;
                string recievedDate;
                string message = string.Empty;//string vypisovany do okna
                string zaznam = string.Empty;//zaznam vybraty z nacitaneho riadku 
                int posun = 0;

                GetRecordInfo(record, out recordNumber, out recievedDate);

                // Aktualizacia zobrazenej roly (t.j. prvy riadok v subore)
                if (recordNumber == -1)
                {
                    appEventsInvoker.Invoke_LogPlayerControl_RoleChangedEvent(record);
                    return;
                }

                // Vytiahnutie datum z prveho realneho zobrazeneho zaznamu a zobrazenie v prehravacom okne
                if (recordNumber == 0)
                {
                    //App.AppEventInvoker.Invoke_LogPlayerControl_SelectedDateFromChanged(recievedDate);
                    try
                    {
                        appEventsInvoker.Invoke_LogPlayerControl_SelectedDateFromChanged(Convert.ToDateTime(recievedDate));
                    }
                    catch
                    {
                        // Chyba
                    }
                }

                if (record.Contains(globalData.LogHeaders["TLG141"]))      //prvky stavadla
                {
                    //  log?.CustomInfo(globalData.LogHeaders["TLG141"] + SerDesManager.SerializeToBase64<Elements>(hmiData)  );
                    zaznam = record.Substring(record.IndexOf(HMI_separator) + HMI_separator.Length + posun);
                    Elements elements = SerDesManager.DeserializeFromBase64<Elements>(zaznam);
                    PMeventDriver.RaisePMtlg141DataRecieved(elements);
                    message = "Tlg. 141 s údajmi";
                }
                else
                if (record.Contains(globalData.LogHeaders["TLG110Login"]))      //prihlasenie
                {
                    message = "Prihlásenie: " + record.Substring(record.IndexOf(HMI_separator) + HMI_separator.Length + posun);
                }
                else
                if (record.Contains(globalData.LogHeaders["TLG110Logout"]))      //odhlasenie
                {
                    message = "Odhlásenie: " + record.Substring(record.IndexOf(HMI_separator) + HMI_separator.Length + posun);
                }
                else
                if (record.Contains(globalData.LogHeaders["HMI-START"]))      //spustenie
                {
                    message = "Spustenie aplikácie";
                }
                else
                if (record.Contains(globalData.LogHeaders["HMI-END"]))      //ukoncenie
                {
                    message = "Ukončenie aplikácie";
                }
                else
                if (record.Contains(globalData.LogHeaders["CLICK"]))      //klik na prvok stavadla
                {
                    zaznam = record.Substring(record.IndexOf(HMI_separator) + HMI_separator.Length + posun);
                    MouseClickData mouseClick = MouseClickData.FromString(zaznam);
                    //System.Drawing.Point position = mouseClick.GetMousePosition();
                    System.Windows.Point position = mouseClick.GetMousePosition();

                    if (SetMousePosition)
                    {
                        //System.Windows.Forms.Cursor.Position = position;//ak aktualny monitor a monitor kde vznikli logy nemaju rovnake rozlisenie, potom treba upravit: System.Windows.Forms.Cursor.Position
                        //LM 24.1.2014 - nenastavuje sa windows kurzor, ale iba virtualny, ktory je v mape stavadla
                        //System.Drawing.Point curPos = new System.Drawing.Point((int)(position.X * App.ScreenWidthProportionality), (int)(position.Y * App.ScreenHeightProportionality)   );
                        //System.Windows.Forms.Cursor.Position = curPos;
                        //System.Diagnostics.Debug.WriteLine("Pozicia x: " + position.X + ", y: " + position.Y);

                        System.Drawing.Point curPos = new System.Drawing.Point((int)(position.X * globalData.ScreenWidthProportionality), (int)(position.Y * globalData.ScreenHeightProportionality));
                       
                        System.Diagnostics.Debug.WriteLine("****PlayModeManager RecordHandler Kurzor na X: " + curPos.X + ", Y: " + curPos.Y);
                        // System.Windows.Forms.Cursor.Position = curPos;
                        //PMeventDriver.RaisePlayModeClickEvent(mouseClick.clickedElementName);
                        PMeventDriver.RaiseCursorMoveEvent(curPos);//zachyti sa v MainWindowStavadlo.xaml.cs
                    }

                    globalData.ClickedElementName = mouseClick.clickedElementName;

                    message = "Klik: X:" + position.X + " Y:" + position.Y + " Element:" + mouseClick.clickedElementName + " Mod:" + mouseClick.selectedMode;
//#if DEBUG
//                    System.Diagnostics.Debug.WriteLine(message);
//#endif

                }
                else if (record.Contains(globalData.LogHeaders["MENU"]))
                {
                    //simuluje klik na polozku menu
                    zaznam = record.Substring(record.IndexOf(HMI_separator) + HMI_separator.Length + posun);
                    MenuData menuData = MenuData.FromString(zaznam);
                    //rozmery pre display kde bola spustena HMI aplikacia
                    primaryScreenWidth  = menuData.mousePosition.primaryScreenWidth;
                    primaryScreenHeight = menuData.mousePosition.primaryScreenHeight;
                    actualWindowWidth   = menuData.mousePosition.windowActualWidth;
                    actualWindowHeight  = menuData.mousePosition.windowActualHeight;

                    int mousePositionX = (int)menuData.mousePosition.mouseX;
                    int mousePositionY = (int)menuData.mousePosition.mouseY;
                    string menuItem = menuData.selectedMode;

                    windowTop = menuData.mousePosition.windowTop;
                    windowLeftp = menuData.mousePosition.windowLeft;

                    message = "Klik na Menu: " + menuData.selectedMode;
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"RecordHandler Menu-Klik: X={mousePositionX} Y={mousePositionY} Mode={menuItem}");
#endif

                    MsgWriter.Clear();
                    CURRENT_MENU_MODE currentMode = (CURRENT_MENU_MODE)Enum.Parse(typeof(CURRENT_MENU_MODE), menuData.selectedMode);

                    globalData.CurrentMenuMode = currentMode;
                    Rezim selRezim = globalData.GetSelectedRezim();
                    MenuFunctions.Instance.SetCurrentMenuAction(selRezim, currentMode, false);
                    PMeventDriver.RaiseCursorMoveEvent(new System.Drawing.Point(mousePositionX, mousePositionY));

                }
                else
              if (record.Contains(globalData.LogHeaders["HMI-INFO"]))
                {
                    zaznam = record.Substring(record.IndexOf(HMI_separator) + HMI_separator.Length + posun);
                    message = "HMI info: " + zaznam;
                }
                else
              if (record.Contains(globalData.LogHeaders["TLG111"]))
                {
                    zaznam = record.Substring(record.IndexOf(HMI_separator) + HMI_separator.Length + posun);
                    message = "RoleManager: " + zaznam;
                }
                else
              //if (record.Contains(globalData.LogHeaders["TLG111v"]))
              //{
              //    string zaznam = record.Substring(record.IndexOf('|') + 1);
              //    message = "RoleManager správa: " + zaznam;
              //} else 
              if (record.Contains(globalData.LogHeaders["TLG120"]))
                {
                    PMeventDriver.RaisePMtlg120SendingEvent();//odpalenie eventu, ktory sa odpaluje v Communicator-e pred vyslanim telegramu
                }
                else
              if (record.Contains(globalData.LogHeaders["TLG121"]))
                {
                    zaznam = record.Substring(record.IndexOf(HMI_separator) + HMI_separator.Length + posun);
                    message = "Tlg. 121: " + zaznam;//pre zobrazenie telegramu, ktory bol prijaty zo servera
                    if(!zaznam.Contains("potvr")) //MH: 23.04.2019 zatial nezobrazujem spravu o prevoze ocele
                        PMeventDriver.RaisePMtlg121DataReceived(zaznam);//odpalenie eventu
                }
                else
              if (record.Contains(globalData.LogHeaders["TLG151"]))
                {
                    zaznam = record.Substring(record.IndexOf(HMI_separator) + HMI_separator.Length + posun);
                    message = "Tlg. 151: " + zaznam;
                    VariantCesty? variantData = VariantCesty.FromString(zaznam);
                    PMeventDriver.RaisePMtlg151DataReceived(new EventArgs3() { Variant = variantData.Value });
                }
                else
              if (record.Contains(globalData.LogHeaders["TLG161"]))
                {
                    zaznam = record.Substring(record.IndexOf(HMI_separator) + HMI_separator.Length + posun);
                    LogicMessage161 mess;
                    if (LogicMessage161.FromString(zaznam, out mess))
                    {
                        PMeventDriver.RaisePMtlg161DataReceived(mess);//odpaleny event sa zachyti v MessageManageri a tam spusti funkciu na spracovanie
                        message = "Tlg.161:" + mess.ToString();
                    }
                }
                else
              if (record.Contains(globalData.LogHeaders["TLG120"]))
                {
                    zaznam = record.Substring(record.IndexOf(HMI_separator) + HMI_separator.Length + posun);
                    message = "HMI Tlg.120: " + zaznam; //pre zobrazenie telegramu, ktory bol vyslany do servera
                }
                else
              if (record.Contains(globalData.LogHeaders["MOVEABLE-ELEMENT"]))
              {
                    zaznam = record.Substring(record.IndexOf(HMI_separator) + HMI_separator.Length + posun);
                    MoveableData data = MoveableData.FromString(zaznam);
                    if (data != null)
                    {
                        PMeventDriver.RaiseElementMovedEvent(data);
                        message = "Presun prvku: " + data.Name + " na offsetX= " + data.X + ", offsetY=" + data.Y;
                    }
              }
                else if(record.Contains(globalData.LogHeaders["TLG191"]))
                {
                    // zapis log?.CustomInfo(globalData.LogHeaders["TLG191"] + SerDesManager.SerializeToBase64<Sets>(supravyData) );
                    zaznam = record.Substring(record.IndexOf(HMI_separator) + HMI_separator.Length + posun);
                    Sets mySets = SerDesManager.DeserializeFromBase64<Sets>(zaznam);
                    PMeventDriver.RaisePMtlg191DataRecieved(mySets);
                    message = "Tlg. 191 s údajmi";
                }

                //event zachyteny v UC_LogsPlayerControl-e
                //RecordMessageReadEvent?.Invoke(this, new RecordEventArgs() { Message = message, RecievedDate = recievedDate, RecordNumber = recordNumber + "/" + (logFileRecordsCount - 2).ToString() });
                RecordMessageReadEvent?.Invoke(this, new RecordEventArgs() { Message = message, RecievedDate = recievedDate, RecordNumber = $"{recordNumber}/{logFileRecordsCount - 2}" }  );
            }
        }//RecordHandler

        //0013 23.04.2019 08:31:06,888  <<HMI-TLG120>>|Posunova cesta;K832;2;z;.;0  priklad zaznamu v subore
        /// <summary>
        /// Vrati poradie zaznamu v subore a datum a cas kedy bol vytvoreny zaznam
        /// </summary>
        /// <param name="record"></param>
        /// <param name="recordNumber"></param>
        /// <param name="recievedDate"></param>
        private void GetRecordInfo(string record, out int recordNumber, out string recievedDate)
        {
            string[] retazce = record.Split(' ');
            if (retazce.Length > 3)
            {
                recordNumber = Convert.ToInt32(retazce[0]);
                recievedDate = retazce[1] + " " + retazce[2];
            }
            else
            {
                recordNumber = -1;
                recievedDate = "00.00.00 00:00";
            }
        }


        private string ReadLine()
        {
            if (stream != null)
            {
                if (stack.BackwadOffset > 0)
                    return stack.GetFromOffset();

                string record = stream.ReadLine();
                if (record != null)
                {
                    stack.Push(record);
                }
                return record;
            }
            return null;
        }


        /// <summary>
        /// Vrati DateTime zaznamu
        /// </summary>
        /// <param name="inputString">riadok zaznamu</param>
        /// <returns></returns>
        private DateTime? VyberDTZoStringu(string inputString)
        {
            DateTime dt;

            string vybratyDtString = SelectDtString(inputString);
            string format = "dd.MM.yyyy HH:mm";
            if (DateTime.TryParseExact(vybratyDtString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt;
            else
                return null;
        }

        /// <summary>
        /// Vytiahne datum a cas z riadku
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        private string SelectDtString(string inputData)
        {
            Regex r = new Regex(@"\d\d.\d\d.\d\d\d\d \d\d:\d\d");
            MatchCollection matches = r.Matches(inputData);
            foreach (Match m in matches)
            {
                string sm = m.ToString();
                return sm;
            }
            return string.Empty;
        }
    }
}
