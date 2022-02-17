using System;
using Stavadlo22.Data;
using Stavadlo22.Data.Telegrams;
using Stavadlo22.Infrastructure.Enums;
using Stavadlo22.Infrastructure.Communication;

using log4net;
using WPFLocalizeExtension.Engine;

namespace Stavadlo22.Infrastructure
{
    /// <summary>
    /// obsahuje funkcie pre vysielanie telegramov;
    /// Funkcie sa volaju po kliku na polozku menu
    /// </summary>
    class MenuFunctions
    {
         StavadloGlobalData GlobalData;
         Communicator AppCommunicator;
         MessageWriter MsgWriter;
         StavadloModel StavadloModel;
         AppEventsInvoker appEventsInvoker;

        //log4net ukladanie sprav o prijatych telegramoch
        public ILog log;// = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly MenuFunctions _instance = new MenuFunctions();

        public static MenuFunctions Instance => _instance;

        public MenuFunctions()
        {
            GlobalData = StavadloGlobalData.Instance;
            StavadloModel = StavadloModel.Instance;
            AppCommunicator = Communicator.Instance;
            appEventsInvoker = AppEventsInvoker.Instance;
            log = GlobalData.Log;
            MsgWriter = MessageWriter.Instance;
        }
       
        public void SendMessage(MessageToLogic message)
        {
            AppCommunicator.SendTLG120ToServer(message);
            return;
        }

        public  void SendMessage(Rezim rezim, string elementName, char userLevel, char kod2)
        {
            MessageToLogic message = new MessageToLogic(0);
            message.kodRezimu = rezim.KodRezimu;//Branu zavriet
            message.elementName = elementName;
            message.level = userLevel;//level pre Admina-2, level pre Dispecera-0, level pre Udrzbu-1
            message.kod = rezim.Kod;
            message.kod2 = kod2;
            AppCommunicator.SendTLG120ToServer(message);
        }

        public void SendMessage(Rezim rezim, string elementName, USER_LEVEL_MODE userLevelMode, char kod2)
        {
            MessageToLogic message = new MessageToLogic(0);
            message.kodRezimu = rezim.KodRezimu;//Branu zavriet
            message.elementName = elementName;
            message.level = (char)userLevelMode;//level pre Admina-'2', level pre Dispecera-'0', level pre Udrzbu-'1'
            message.kod = rezim.Kod;
            message.kod2 = kod2;
            AppCommunicator.SendTLG120ToServer(message);
        }

        /// <summary>
        /// zaregistruje vybraty mod z menu aplikacie
        /// </summary>
        /// <param name="selMenuMode"></param>
        public void RegisterSelectedMenuMode(CURRENT_MENU_MODE selMenuMode)
        {
            //App.Current.Resources["selectedMode"] = selMenuMode.ToString(); //pre zobrazenie v StatusBare
            //App.Current.Resources["currentMode"] = selMenuMode; //aby vybraty mod bol pristupny z XAML
            //App.currentMode = selMenuMode; //aby vybraty mod bol pristupny aj z C#
            GlobalData.CurrentMenuMode = selMenuMode;
        }

        /// <summary>
        /// nastavenie CURRENT_MODE; vypis oznamu do spodneho riadku dispaleja vlavo;
        /// zaregistruje vybaty mod;
        /// pre niektory mod sa hned posle aj telegram do Servera a z neho do Logicu;
        /// pre niektory mod sa prislusny telegram posle az po kliku na prvok stavadla;
        /// </summary>
        /// <param name="selRezim"></param>
        /// <param name="selMenuMode">true ak je GlobalData.WorkMode==true</param>
        public void SetCurrentMenuAction(Rezim selRezim, CURRENT_MENU_MODE selMenuMode, bool normalMode)
        {

            #region --Kod pre selected mode--

            if (selRezim == null)
                return;
            string s1 = string.Empty;

            //RegisterSelectedMenuMode(selMenuMode);
            GlobalData.CurrentMenuMode = selMenuMode;

            switch (selMenuMode)
            {
                case CURRENT_MENU_MODE.NONE:
                    break;
                case CURRENT_MENU_MODE.STOP:
                    s1 = "STOP";
                    if (normalMode)
                        SendMessage(selRezim, "", GlobalData.App_UserLevelMode, 'x');//vysle message STOP do servera
                    break;

                #region --- Udrzba----

                //Udrzba/Zrusenie poruchy
                case CURRENT_MENU_MODE.ZRUSENIE_PORUCHY://stary manual str. 26; ostava aktivna az do dalsieho vyberu rezimu;
                    //Zrusi poruchu ak vymena, alebo navestidlo bolo v poruchovom stave
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznactePrvokPreZrusPoruchy", LocalizeDictionary.Instance.Culture);
                    //("Označte prvok k zrušeniu poruchy!");
                    break;

                //Udrzba/Vyluka/uplna
                case CURRENT_MENU_MODE.UPLNA_VYLUKA: //nastavuje/rusi vymenu do/z uplnej vyluky
                    //stary manual str. 27; ostava aktivna az do dalsieho vyberu rezimu
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteVymUplnaVyluka", LocalizeDictionary.Instance.Culture);
                    //("Označte výmenu určenú k úplnej výluke, alebo jej zrušeniu!");
                    break;


                //Udrzba/Vyluka/prestavenia
                case CURRENT_MENU_MODE.VYLUKA_PRESTAVENIA: //nastavyje/rusi vymenu do/z  vyluky prestavenia
                    //stary manual str. 27; ostava aktivna az do dalsieho vyberu rezimu
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteVymVylukaPrestav", LocalizeDictionary.Instance.Culture);
                    //("Označte výmenu určenú k výluke prestavenia, alebo jej zrušeniu!");
                    break;


                //Udrzba/Uvolnenie izolacie
                case CURRENT_MENU_MODE.UVOLNENIE_IZOLACIE:
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteUsekUvolIzol", LocalizeDictionary.Instance.Culture);
                    //("Označte úsek určený k uvoľneniu izolácie!");
                    break;

                case CURRENT_MENU_MODE.VYLUKA_NAVESTIDLA:  //Udrzba/Vyluka/Vyluka navestidla
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteNavestidloVyluka", LocalizeDictionary.Instance.Culture);
                    //("Označte návestidlo určené k výluke alebo k jej zrušeniu!");
                    break;

                case CURRENT_MENU_MODE.VYLUKA_POSUNOVEJ_CESTY://Udrzba/Vyluka/posunovej cesty
                    if (StavadloModel.Instance.PosunovaCestaManager.PosunCestaStavIsEnd())
                        s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteKoniecPosunCestyVyluka", LocalizeDictionary.Instance.Culture);
                    // ("Označte koniec posunovej cesty pre výluku!");
                    else
                        s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteZacPosunCestyVyluka", LocalizeDictionary.Instance.Culture);
                    //("Označte začiatok posunovej cesty určenej k výluke, alebo k jej zrušeniu!");

                    break;


                case CURRENT_MENU_MODE.VYLUKA_VLAKOVEJ_CESTY://Udrzba/Vyluka/vlakovej cesty
                    if (StavadloModel.VlakovaCestaManager.VlakCestaStavIsEnd())
                        s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteKoniecVlakCestaVyluka", LocalizeDictionary.Instance.Culture);
                    //("Označte koniec vlakovej cesty pre výluku!");
                    else
                        s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteZacVlakCestaVyluka", LocalizeDictionary.Instance.Culture);
                    // ("Označte začiatok vlakovej cesty určenej k výluke, alebo k jej zrušeniu!");

                    break;

                case CURRENT_MENU_MODE.OZNACENIE_OBSADENIA://Udrzba/Oznacenie obsadenia
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteUsekOznacObsad", LocalizeDictionary.Instance.Culture);
                    //("Označte úsek pre označenie obsadenia, alebo pre jeho zrušenie!");
                    break;
                
                case CURRENT_MENU_MODE.VYBAVENIE_PORUCH://Udrzba/Vybavenie poruch
                    //Udrzba: vybavenie poruch->zrusenie vsetkych poruch naraz ak je poruch viac ako 5
                    if (normalMode)
                        SendMessage(selRezim, "", GlobalData.App_UserLevelMode, 'x');
                    break;

                #endregion --- Udrzba ---

                //Vymena
                case CURRENT_MENU_MODE.VYMENA://stary manual str. 30
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteVymenuPrestavenie", LocalizeDictionary.Instance.Culture);
                    //("Označte výmenu určenú k prestaveniu!");
                    //Zapis do logu: dd:hh:mm Prestavenie vymeny - zaciatok 10 s intervalu
                    string sz1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "prestavenieVymeny10sZaciatok", LocalizeDictionary.Instance.Culture);
                    if (normalMode)
                        log?.CustomInfo(string.Format("{0}{1}-{2}", GlobalData.LogHeaders["HMI-INFO"], DateTime.Now.ToString("dd HH:mm"), sz1));
#if !DEBUG
                   if (normalMode)
                        //v RELEASE mode sa mod Vymena zrusi automaticky po 10sec., ak sa nekliklo na vymenu
                        appEventsInvoker?.Invoke_StartVymenaTimer_Event();//odpali event, zachyti sa v MainPage a spusti VymenaTimer
#endif
                    break;

                    //Zrus
                case CURRENT_MENU_MODE.ZRUSENIE_CESTY:
                    if (normalMode)
                        SendMessage(selRezim, "", GlobalData.App_UserLevelMode, 'x');
                    //Poznamky: Odoslanie Spravy Zrus 
                    // vyslanie telegramu 120 so spravou Zrus cestu, ale s fiktivnym nazvom useku;
                    // Ak este nebola postavena cesta, server vrati tlg. 121: Zrus: Ziadne jazdne cesty nie su postavene!
                    // Server vrati v tel.c. 151 Variant.var_number pocet existujucich ciest na zrusenie,
                    // kde var_number =1,2,...
                    // a podla toho vypisat oznam: Oznacte 1 z X ciest na zrusenie, Oznacte cestu pre zrusenie
                    // Ak neexistuje cesta na zrusenie tel.c. 151 nepride.
                    //
                    //Server odpovie telegramom cislo 151 kt. obsahuje pocet postavenych ciest, a tlg. c.121 a na zaklade jeho odpovede vypisem oznam
                    //"Označte cestu pre zrušenie";

                    break;

                #region --- Simulacia ---
                case CURRENT_MENU_MODE.SIMULACIA_OBSADENIA://PFunkcie/Simulacia/Obsadenosti
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteUsekSimObsadenia", LocalizeDictionary.Instance.Culture);
                    //("Označte úsek pre nastavenie, alebo zrušenie simulácie obsadenia!");
                    break;
                case CURRENT_MENU_MODE.SIMULACIA_PORUCHY_STRATA_DOHLIADANIA://PFunkcie/Simulacia/Poruchy/Strata dohliadania
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteVymenuStrataDohl", LocalizeDictionary.Instance.Culture);
                    //("Označte výmenu určenú k simulácii poruchy straty dohliadania!");
                    break;
                case CURRENT_MENU_MODE.SIMULACIA_PORUCHY_PRESTAVENIA://PFunkcie/Simulacia/Poruchy/Porucha prestavenia
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteVymenuPoruchaPrest", LocalizeDictionary.Instance.Culture);
                    //("Označte výmenu určenú k simulácii poruchy prestavenia!");
                    break;
                case CURRENT_MENU_MODE.SIMULACIA_PORUCHY_ROZREZ://PFunkcie/Simulacia/Poruchy/Rozrez
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteVymenuPoruchaRozrez", LocalizeDictionary.Instance.Culture);
                    //("Označte výmenu určenú k simulácii poruchy rozrezu!");
                    break;
                case CURRENT_MENU_MODE.SIMULACIA_PORUCHY_NADPRUD://PFunkcie/Simulacia/Poruchy/Nadprud
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteVymenuPoruchaNadprud", LocalizeDictionary.Instance.Culture);
                    //("Označte výmenu určenú k simulácii poruchy padprúdu!");
                    break;

                case CURRENT_MENU_MODE.SIMULACIA_PORUCHY_NAVESTIDLA_PORUCHA_CERVENEJ://PFunkcie/Simulacia/Poruchy/Navestidla/Poruch cervenej
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteNavSimPorCervenej", LocalizeDictionary.Instance.Culture);
                    //("Označte návestidlo určené k simulácií poruchy červenej!");
                    break;

                case CURRENT_MENU_MODE.SIMULACIA_PORUCHY_NAVESTIDLA_POVOLOVACEJ_NAVESTI://PFunkcie/Simulacia/Poruchy/Navestidla/Porucha povolovacej navesti
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteNavSimPovolNav", LocalizeDictionary.Instance.Culture);
                    //("Označte návestidlo určené k simulácií poruchy povoľovacej návesti!");
                    break;

                case CURRENT_MENU_MODE.SIMULACIA_ZIADOSTI_O_SUHLAS://PFunkcie/Simulacia/Suhlasu/Ziadost
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "simulZiadostSuhlas", LocalizeDictionary.Instance.Culture);
                    //("Simulácia žiadosti o súhlas: Označte úsek K603T, K201, alebo K210!");
                    break;


                case CURRENT_MENU_MODE.SIMULACIA_UDELENIA_SUHLASU://PFunkcie/Simulacia/Suhlasu/Udelenie
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteUsekUdelSuhlasu", LocalizeDictionary.Instance.Culture);
                    //("Označte úsek pre udelenie súhlasu!");
                    break;
                case CURRENT_MENU_MODE.SIMULACIA_RUSENIA_SUHLASU://PFunkcie/Simulacia/Suhlasu/Rusenie
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteUsekZrusSuhlasu", LocalizeDictionary.Instance.Culture);
                    //("Označte úsek pre zrušenie súhlasu!");
                    break;

                #endregion --- Simulacia ---

                #region======Hl.Menu: Suhlas=========

                //Suhlas/Ziadost o suhlas/Pre vlakovu cestu
                case CURRENT_MENU_MODE.ZIADOST_O_SUHLAS_PRE_VLAKOVU_CESTU://ziadame stavadlo xxx o suhlas na odchod z kolaje kkk
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteUsekVlakCestaOdchod", LocalizeDictionary.Instance.Culture);
                    //Tu by sa mohol poslat telegram pre Logicu, podobne ako pre PosliSpravu_ZiadostOobsluhuV801();
                    // App.MessageWriter.WriteLeft("Vyslanie žiadosti o súhlas pre odchod z K603T");
                    //("Označte úsek pre odchod vlakovou cestou");
                    break;

                //Suhlas/Ziadost o suhlas/Pre posunovu cestu
                case CURRENT_MENU_MODE.ZIADOST_O_SUHLAS_PRE_POSUNOVU_CESTU://ziadame stavadlo xxx o suhlas na posun z kolaje kkkk
                    //App.kodS603T = 'p';
                    //Tu by sa mohol poslat telegram pre Logicu, podobne ako pre PosliSpravu_ZiadostOobsluhuV801();
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteUsekPosunCestaOdchod", LocalizeDictionary.Instance.Culture);
                    //("Označte úsek pre odchod posunovou cestou");
                    break;

                //case CURRENT_MENU_MODE.ZIADOST_O_SUHLAS_PRE_OBSLUHU_V801:
                //    App.ClickedElementName = "V801";
                //    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "vyslanieZiadostiV801Suhlas", LocalizeDictionary.Instance.Culture);
                //    //("Vyslanie žiadosti o súhlas pre obsluhu V801!");
                //    if (normalMode)
                //        SendMessage(selRezim, "V801", GlobalData.App_UserLevelMode, 'x');//sprava Ziadost pre obsluhu V801
                //        //po vyslani telegramu do servera, server vrati tlg.161 :Vyslanie žiadosti o súhlas pre obsluhu V801;b;d a tlg.121: OK
                //        //vypise sa len sprava prijata zo servera
                //    return;
                    

                //Suhlas/Udelenie suhlasu
                case CURRENT_MENU_MODE.UDELENIE_SUHLASU: //moze sa kliknut na kontrol UC_VstupOdchod
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteUsekUdelSuhlasu", LocalizeDictionary.Instance.Culture);
                    //("Označte úsek pre udelenie súhlasu!");
                    break;

                //Suhlas/Rusenie suhlasu
                case CURRENT_MENU_MODE.RUSENIE_SUHLASU://moze sa kliknut na kontrol UC_VstupOdchod
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteUsekZrusSuhlasu", LocalizeDictionary.Instance.Culture);
                    //("Označte úsek pre zrušenie súhlasu!");
                    break;

                #endregion ----- Hl.Menu: Suhlas------------------


                    //Vcesta
                case CURRENT_MENU_MODE.VCESTA:
                    if (StavadloModel.VlakovaCestaManager.VlakCestaStavIsEnd())
                    {    //("Označte koniec vlakovej cesty!");
                        s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteKoniecVlakCesty", LocalizeDictionary.Instance.Culture);
                    }
                    else
                    {
                        //("Označte začiatok vlakovej cesty!");
                        s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteZacVlakCesty", LocalizeDictionary.Instance.Culture);
                    }
                    break;

                    //Pcesta
                case CURRENT_MENU_MODE.PCESTA:
                    //App.MessageWriter.Clear();
                    if (StavadloModel.PosunovaCestaManager.PosunCestaStavIsEnd())
                        //MessageWriter.WriteLeft("Označte koniec posunovej cesty!");
                        s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteKoniecPosunCesty", LocalizeDictionary.Instance.Culture);
                    else
                        //MessageWriter.WriteLeft("Označte začiatok posunovej cesty!");
                        s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteZacPosunCesty", LocalizeDictionary.Instance.Culture);

                    break;

                case CURRENT_MENU_MODE.PRIVOLAVACIA_NAVEST:
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacNavestPrivolNavest", LocalizeDictionary.Instance.Culture);
                    //App.MessageWriter.WriteLeft("Označte návestidlo pre privolávaciu návesť!");
                    break;

                case CURRENT_MENU_MODE.ZAPADKOVA_SKUSKA:
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteVymenuZapadSkuska", LocalizeDictionary.Instance.Culture);
                    //App.MessageWriter.WriteLeft("Označte výmenu na vykonanie západkovej skúšky!");
                    break;

                case CURRENT_MENU_MODE.INFORMACIA_O_PRVKU://PFunkcie/Informacie/o prvku
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznactePrvokZobrInfo", LocalizeDictionary.Instance.Culture);
                    //("Označte prvok pre zobrazenie jeho informácií!");
                    //panel pre zobrazenie informacii sa otvori az po kliku na vybraty prvok stavadla
                    break;

                case CURRENT_MENU_MODE.INFORMACIE_O_ZARIADENI:
                    s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznactePrvokInfoZar", LocalizeDictionary.Instance.Culture);
                    //("Označte prvok pre zobrazenie informácií o zariadení!");
                    //panel pre zobrazenie informacii sa otvori az po kliku na vybraty prvok stavadla
                    break;
                default:
                    break;
            }//switch (selMode)

            MsgWriter.WriteLeft(s1); //vypis textu vlavo

            #endregion --Kod pre selected mode--
        }

        /// <summary>
        /// nastavenie CURRENT_MODE; vypis oznamu do spodneho riadku dispaleja vlavo;
        /// zaregistruje vybaty mod;
        /// pre niektory mod sa hned posle aj telegram do Servera a z neho do Logicu;
        /// pre niektory mod sa prislusny telegram posle az po kliku na prvok stavadla;
        /// </summary>
        /// <param name="selRezim"></param>
        /// <param name="selMenuMode">true ak je GlobalData.WorkMode==true</param>
        public void SetCurrentMenuAction( CURRENT_MENU_MODE selMenuMode)
        {

            #region --Kod pre selected mode--
            Rezim selRezim = GlobalData.GetSelectedRezim();
            if (selRezim == null)
                return;

            string leftMsgInfo = string.Empty;//text pre vypis do spodneho riadku aplikacie

            GlobalData.CurrentMenuMode = selMenuMode;

            switch (selMenuMode)
            {
                case CURRENT_MENU_MODE.NONE:
                    break;
                case CURRENT_MENU_MODE.STOP:
                    leftMsgInfo = "STOP";
                    if (GlobalData.WorkMode)
                        SendMessage(selRezim, "", GlobalData.App_UserLevelMode, 'x');//vysle message STOP do servera
                    break;

                #region ====PFunkcie=======

                #region --- Simulacia ---
                case CURRENT_MENU_MODE.SIMULACIA_OBSADENIA://PFunkcie/Simulacia/Obsadenosti
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteUsekSimObsadenia", LocalizeDictionary.Instance.Culture);
                    //("Označte úsek pre nastavenie, alebo zrušenie simulácie obsadenia!");
                    break;
                case CURRENT_MENU_MODE.SIMULACIA_PORUCHY_STRATA_DOHLIADANIA://PFunkcie/Simulacia/Poruchy/Strata dohliadania
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteVymenuStrataDohl", LocalizeDictionary.Instance.Culture);
                    //("Označte výmenu určenú k simulácii poruchy straty dohliadania!");
                    break;
                case CURRENT_MENU_MODE.SIMULACIA_PORUCHY_PRESTAVENIA://PFunkcie/Simulacia/Poruchy/Porucha prestavenia
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteVymenuPoruchaPrest", LocalizeDictionary.Instance.Culture);
                    //("Označte výmenu určenú k simulácii poruchy prestavenia!");
                    break;
                case CURRENT_MENU_MODE.SIMULACIA_PORUCHY_ROZREZ://PFunkcie/Simulacia/Poruchy/Rozrez
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteVymenuPoruchaRozrez", LocalizeDictionary.Instance.Culture);
                    //("Označte výmenu určenú k simulácii poruchy rozrezu!");
                    break;
                case CURRENT_MENU_MODE.SIMULACIA_PORUCHY_NADPRUD://PFunkcie/Simulacia/Poruchy/Nadprud
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteVymenuPoruchaNadprud", LocalizeDictionary.Instance.Culture);
                    //("Označte výmenu určenú k simulácii poruchy padprúdu!");
                    break;

                case CURRENT_MENU_MODE.SIMULACIA_PORUCHY_NAVESTIDLA_PORUCHA_CERVENEJ://PFunkcie/Simulacia/Poruchy/Navestidla/Poruch cervenej
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteNavSimPorCervenej", LocalizeDictionary.Instance.Culture);
                    //("Označte návestidlo určené k simulácií poruchy červenej!");
                    break;

                case CURRENT_MENU_MODE.SIMULACIA_PORUCHY_NAVESTIDLA_POVOLOVACEJ_NAVESTI://PFunkcie/Simulacia/Poruchy/Navestidla/Porucha povolovacej navesti
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteNavSimPovolNav", LocalizeDictionary.Instance.Culture);
                    //("Označte návestidlo určené k simulácií poruchy povoľovacej návesti!");
                    break;

                case CURRENT_MENU_MODE.SIMULACIA_ZIADOSTI_O_SUHLAS://PFunkcie/Simulacia/Suhlasu/Ziadost
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "simulZiadostSuhlas", LocalizeDictionary.Instance.Culture);
                    //("Označte koľaj pre vyslanie žiadosti o súhlas");
                    break;

                case CURRENT_MENU_MODE.SIMULACIA_UDELENIA_SUHLASU://PFunkcie/Simulacia/Suhlasu/Udelenie
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteUsekUdelSuhlasu", LocalizeDictionary.Instance.Culture);
                    //("Označte koľaj pre udelenie súhlasu!");
                    break;
                case CURRENT_MENU_MODE.SIMULACIA_RUSENIA_SUHLASU://PFunkcie/Simulacia/Suhlasu/Rusenie
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteUsekZrusSuhlasu", LocalizeDictionary.Instance.Culture);
                    //("Označte koľaj pre zrušenie súhlasu!");
                    break;

                #endregion --- Simulacia ---


                case CURRENT_MENU_MODE.INFORMACIA_O_PRVKU://PFunkcie/Informacie/o prvku
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznactePrvokZobrInfo", LocalizeDictionary.Instance.Culture);
                    //("Označte prvok pre zobrazenie jeho informácií!");
                    //panel pre zobrazenie informacii sa otvori az po kliku na vybraty prvok stavadla
                    break;

                //PFunkcie-Informacie-O zariadeni
                case CURRENT_MENU_MODE.INFORMACIE_O_ZARIADENI:
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznactePrvokInfoZar", LocalizeDictionary.Instance.Culture);
                    //("Označte prvok pre zobrazenie informácií o zariadení!");
                    //panel pre zobrazenie informacii sa otvori az po kliku na vybraty prvok stavadla
                    break;

                case CURRENT_MENU_MODE.INFORMACIE_O_POM_SIGNALOCH:
                    SendMessage(selRezim, "", GlobalData.App_UserLevelMode, 'x');
                    break;
                default:
                    break;


                #endregion ====PFunkcie=======

                #region --- Udrzba----

                //Udrzba/Zrusenie poruchy
                case CURRENT_MENU_MODE.ZRUSENIE_PORUCHY://ostava aktivna az do dalsieho vyberu rezimu;
                    //Zrusi poruchu ak vymena, alebo navestidlo bolo v poruchovom stave
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznactePrvokPreZrusPoruchy", LocalizeDictionary.Instance.Culture);
                    //("Označte prvok k zrušeniu poruchy!");
                    break;

                //Udrzba/Vyluka/uplna
                case CURRENT_MENU_MODE.UPLNA_VYLUKA: //nastavuje/rusi vymenu do/z uplnej vyluky
                    //ostava aktivna az do dalsieho vyberu rezimu
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteVymUplnaVyluka", LocalizeDictionary.Instance.Culture);
                    //("Označte výmenu určenú k úplnej výluke, alebo jej zrušeniu!");
                    break;


                //Udrzba/Vyluka/prestavenia
                case CURRENT_MENU_MODE.VYLUKA_PRESTAVENIA: //nastavyje/rusi vymenu do/z  vyluky prestavenia
                    //ostava aktivna az do dalsieho vyberu rezimu
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteVymVylukaPrestav", LocalizeDictionary.Instance.Culture);
                    //("Označte výmenu určenú k výluke prestavenia, alebo jej zrušeniu!");
                    break;


                //Udrzba/Uvolnenie izolacie
                case CURRENT_MENU_MODE.UVOLNENIE_IZOLACIE:
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteUsekUvolIzol", LocalizeDictionary.Instance.Culture);
                    //("Označte úsek určený k uvoľneniu izolácie!");
                    break;

                case CURRENT_MENU_MODE.VYLUKA_NAVESTIDLA:  //Udrzba/Vyluka/Vyluka navestidla
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteNavestidloVyluka", LocalizeDictionary.Instance.Culture);
                    //("Označte návestidlo určené k výluke alebo k jej zrušeniu!");
                    break;

                case CURRENT_MENU_MODE.VYLUKA_POSUNOVEJ_CESTY://Udrzba/Vyluka/posunovej cesty
                    if (StavadloModel.Instance.PosunovaCestaManager.PosunCestaStavIsEnd())
                        leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteKoniecPosunCestyVyluka", LocalizeDictionary.Instance.Culture);
                    // ("Označte koniec posunovej cesty pre výluku!");
                    else
                        leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteZacPosunCestyVyluka", LocalizeDictionary.Instance.Culture);
                    //("Označte začiatok posunovej cesty určenej k výluke, alebo k jej zrušeniu!");

                    break;


                case CURRENT_MENU_MODE.VYLUKA_VLAKOVEJ_CESTY://Udrzba/Vyluka/vlakovej cesty
                    if (StavadloModel.VlakovaCestaManager.VlakCestaStavIsEnd())
                        leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteKoniecVlakCestaVyluka", LocalizeDictionary.Instance.Culture);
                    //("Označte koniec vlakovej cesty pre výluku!");
                    else
                        leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteZacVlakCestaVyluka", LocalizeDictionary.Instance.Culture);
                    // ("Označte začiatok vlakovej cesty určenej k výluke, alebo k jej zrušeniu!");

                    break;

                case CURRENT_MENU_MODE.OZNACENIE_OBSADENIA://Udrzba/Oznacenie obsadenia
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteUsekOznacObsad", LocalizeDictionary.Instance.Culture);
                    //("Označte úsek pre označenie obsadenia, alebo pre jeho zrušenie!");
                    break;

                case CURRENT_MENU_MODE.VYBAVENIE_PORUCH://Udrzba/Vybavenie poruch
                    //Udrzba: vybavenie poruch->zrusenie vsetkych poruch naraz ak je poruch viac ako 5
                    if (GlobalData.WorkMode)
                        SendMessage(selRezim, "", GlobalData.App_UserLevelMode, 'x');
                    break;

                case CURRENT_MENU_MODE.PRIVOLAVACIA_NAVEST:
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacNavestPrivolNavest", LocalizeDictionary.Instance.Culture);
                    //App.MessageWriter.WriteLeft("Označte návestidlo pre privolávaciu návesť!");
                    break;

                case CURRENT_MENU_MODE.ZAPADKOVA_SKUSKA:
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteVymenuZapadSkuska", LocalizeDictionary.Instance.Culture);
                    //App.MessageWriter.WriteLeft("Označte výmenu na vykonanie západkovej skúšky!");
                    break;

                #endregion --- Udrzba ---

                //====Vymena=====
                case CURRENT_MENU_MODE.VYMENA:
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteVymenuPrestavenie", LocalizeDictionary.Instance.Culture);
                    //("Označte výmenu určenú k prestaveniu!");
                    //Zapis do logu: dd:hh:mm Prestavenie vymeny - zaciatok 10 s intervalu
                    string sz1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "prestavenieVymeny10sZaciatok", LocalizeDictionary.Instance.Culture);
                    if (GlobalData.WorkMode)
                        log?.CustomInfo($"{GlobalData.LogHeaders["HMI-INFO"]}{DateTime.Now:dd HH:mm}-{sz1}");
#if !DEBUG
                   if (GlobalData.WorkMode)
                        appEventsInvoker?.Invoke_StartVymenaTimer_Event();//odpali event, ktory sa zachyti v MainPage a spusti VymenaTimer
                        //v RELEASE mode sa mod Vymena zrusi automaticky po 10sec., ak sa nekliklo na vymenu
                        //v Release mode VZDY SA MUSI KLIKNUT NA button VYMENA ak sa ma prestavovat vymena
                       //pozri MessageManager.cs TlgMessage121Recieved_Handler
#endif
                    break;

                //====Zrus====
                case CURRENT_MENU_MODE.ZRUSENIE_CESTY:
                    if (GlobalData.WorkMode)
                        SendMessage(selRezim, "", GlobalData.App_UserLevelMode, 'x');
                    //Poznamky: Odoslanie Spravy Zrus 
                    // vyslanie telegramu 120 so spravou Zrus cestu, ale s fiktivnym nazvom useku;
                    // Ak este nebola postavena cesta, server vrati tlg. 121: Zrus: Ziadne jazdne cesty nie su postavene!
                    // Server vrati v tel.c. 151 Variant.var_number pocet existujucich ciest na zrusenie,
                    // kde var_number =1,2,...
                    // a podla toho vypisat oznam: Oznacte 1 z X ciest na zrusenie, Oznacte cestu pre zrusenie
                    // Ak neexistuje cesta na zrusenie tel.c. 151 nepride.
                    //
                    //Server odpovie telegramom cislo 151 kt. obsahuje pocet postavenych ciest, a tlg. c.121 a na zaklade jeho odpovede vypisem oznam
                    //"Označte cestu pre zrušenie";

                    break;

               

                #region======Hl.Menu: Suhlas=========

                //Ziadost pre vonkajsie stavadlo pre odchod z ST22, blika zelena sipka, smeruje von
                //Suhlas/Ziadost o suhlas; Pre simulaciu potvrdenia ziadosti pre odchod z ST22 sa  musi zvolit PFunkcie/Simulacia/Suhlasu - Udelenie...
                case CURRENT_MENU_MODE.ZIADOST_O_SUHLAS: 
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteUsekOdchod", LocalizeDictionary.Instance.Culture);
                    //("Označte úsek pre vyslanie žiadosti o súhlas pre odchod zo stavadla!");
                    break;

                //Udelenie suhlasu pre vonkajsie stavadlo pre vstup z ST22, zlta sipka smeruje dnu, trvalo svieti
                //Suhlas/Udelenie suhlasu; Najprv sa musi zvolit PFunkcie/Simulacia/Suhlasu - Ziadost...
                case CURRENT_MENU_MODE.UDELENIE_SUHLASU: 
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteUsekUdelSuhlasu", LocalizeDictionary.Instance.Culture);
                    //("Označte úsek pre udelenie súhlasu!");
                    break;

                //Suhlas/Rusenie suhlasu ak predtym bola zadana Ziadost o suhlas
                case CURRENT_MENU_MODE.RUSENIE_SUHLASU:
                    leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteUsekZrusSuhlasu", LocalizeDictionary.Instance.Culture);
                    //("Označte úsek pre zrušenie súhlasu!");
                    break;

                #endregion ----- Hl.Menu: Suhlas------------------


                //========Vcesta=============
                case CURRENT_MENU_MODE.VCESTA:
                    if (StavadloModel.VlakovaCestaManager.VlakCestaStavIsEnd())
                    {    //("Označte koniec vlakovej cesty!");
                        leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteKoniecVlakCesty", LocalizeDictionary.Instance.Culture);
                    }
                    else
                    {
                        //("Označte začiatok vlakovej cesty!");
                        leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteZacVlakCesty", LocalizeDictionary.Instance.Culture);
                    }
                    break;

                //=======Pcesta============
                case CURRENT_MENU_MODE.PCESTA:
                    //App.MessageWriter.Clear();
                    if (StavadloModel.PosunovaCestaManager.PosunCestaStavIsEnd())
                        //MessageWriter.WriteLeft("Označte koniec posunovej cesty!");
                        leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteKoniecPosunCesty", LocalizeDictionary.Instance.Culture);
                    else
                        //MessageWriter.WriteLeft("Označte začiatok posunovej cesty!");
                        leftMsgInfo = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteZacPosunCesty", LocalizeDictionary.Instance.Culture);

                    break;

               

                
            }//switch (selMode)

            MsgWriter.WriteLeft(leftMsgInfo); //vypis textu vlavo

            #endregion --Kod pre selected mode--
        }
    }
}
