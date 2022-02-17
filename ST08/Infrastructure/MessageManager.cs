using System;
using System.Windows.Shapes;
using Stavadlo22.Data;
using Stavadlo22.UserControls;
using Stavadlo22.Infrastructure.Enums;
using Stavadlo22.Infrastructure.PlayMode;
using Stavadlo22.Infrastructure.Communication;
using WPFLocalizeExtension.Engine;

namespace Stavadlo22.Infrastructure
{
    #region Documentation
    /// <summary
    /// <remarks>
    /// <para>
    /// Class Info: MessageManager je napojeny na eventy z Communicator: po prijme tlg. 121, 151, a 161;
    /// podla parametrov z telegramu generuje novy event a prijatu spravu zo servera (Logicu) zapise do MsgWriter;
    /// generovane eventy zachytava VlakovaCestaManager a PosunovaCestaManager a vyfarbuje-oznacuje prislusne useky (vymeny a useky) ako varianty cesty.
    /// <list type="bullet">
    /// <item name="author">Author: RNDr. M. Hrabcak, CSc.</item>
    /// <item name="date">September 2013</item>
    /// <item name="email">hrabcak@procaut.sk</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// </summary>
    #endregion
    public class MessageManager
    {
        private static readonly MessageManager _instance = new MessageManager();
        public static MessageManager Instance => _instance; //skrateny zapis klasickeho gettera


        #region --FIELDS/Events--

        /// <summary>
        /// event odpalovany MessageManagerom po prijme tlg. c. 161
        /// </summary>
        public event Action<LogicEventErrorArgs> Message161ReceivedEvent;

        /// <summary>
        /// HODNOTA Z TLG. 151, nastavuje sa v TlgMessage151Recieved
        /// vynuluje sa vzdy pred vyslanim telegramu do servera ( a z neho do Logicu)
        /// </summary>
        public int VariantValue { get; set; }

        StavadloGlobalData GlobalData;//globalne data pre aplikaciu
        StavadloModel stavadloModel;  //model aplikacie
        Communicator AppCommunicator; //komunikator pre aplikaciu
        StavadloFactory stavadloFactory;  //pomocne funkcie pre aplikaciu
        MessageWriter MsgWriter;      //pre vypis oznaov
        AppEventsInvoker appEventsInvoker; //pre zachytavanie eventov
        PlayModeEventDriver PMeventDriver;  //pre play mode
        CURRENT_MENU_MODE currentMenuMode;  //aktualny mod aplikacie nastaveny z menu

        string s1 = string.Empty; //pre lokalizovany text
        string msg1 = string.Empty;

        bool semforKlikPovolenieCesty;

        #endregion --FIELDS/Event--

        #region -- CONSTRUCTOR --

        private MessageManager()
        {
            GlobalData = StavadloGlobalData.Instance;
            stavadloModel = StavadloModel.Instance;
            AppCommunicator = Communicator.Instance;
            stavadloFactory = StavadloFactory.Instance;
            appEventsInvoker = AppEventsInvoker.Instance;
            MsgWriter = MessageWriter.Instance;
            PMeventDriver = PlayModeEventDriver.Instance;

            currentMenuMode = GlobalData.CurrentMenuMode;
            semforKlikPovolenieCesty = false;

            if (GlobalData.WorkMode)//normalny rezim
            {
                //handler pre event, ktory je odpaleny tesne pred odoslanim telegramu c. 120 do servera
                AppCommunicator.Tlg120SendingEvent += AppCommunicator_Tlg120SendingEvent_Handler;

                //handler pre event ak bol prijaty telegram cislo 161 z Logicu
                //pre prijem oznamu, ktory sa vypisuje vpravo, alebo do noveho okna
                AppCommunicator.TlgMessage161Received += TlgMessage161Received_Handler;

                //handler pre event ak bol prijaty telegram cislo 151 z Logicu, prijem Variantu
                AppCommunicator.TlgMessage151Recieved += TlgMessage151Recieved_Handler;

                //handler pre event odpaleny po inicializacii spojenia so serverom, event sa zachyti v Communicator.cs
                AppCommunicator.InitConnectionRecieved += AppCommunicator_InitConnectionRecieved;

                //handler pre event ak sa prijme textova sprava (telegram c. 121) zo servera (OK, alebo popis chyby z Logicu)
                AppCommunicator.TlgMessage121Recieved += TlgMessage121Recieved_Handler;

                //handler pre event ak tcpClientClass.dll vygeneruje udalost ze sa prerusilo spojenie so serverom, napr. vytiahol sa LAN kabel, alebo ina udalost;
                //event sa odpaluje v Communicator.cs
                AppCommunicator.ServerConnectionFailed += AppCommunicator_ServerConnectionFailed; 

                //handler pre event ak klient hlasi pretecenie prijimaca
                AppCommunicator.ClientRecBufOverflowReceived += AppCommunicator_ServerRecBufOverflowReceived;

                //handler pre event ak klient prijal neznamy telegram
                AppCommunicator.UnknownTelNumberReceived += AppCommunicator_UnknownTelNumberReceived;

                //ak sa prerusi/obnovi spojenie so serverom, odpaluje sa v ServerWatchDogControler
                appEventsInvoker.ServerConnectionEvent += AppEventsInvoker_ServerConnectionEventHandler;
            }
            else
            {   //PlayMode: eventy odpalovane v PlayMode
                if (GlobalData.PlayMode)
                {
                    PMeventDriver.PMtlg120SendingEvent += PMeventDriver_PMtlg120SendingEvent_Handler;
                    PMeventDriver.PMtlg121DataReceivedEvent += PMeventDriver_PMtlg121DataReceived_Handler;
                    PMeventDriver.PMtlg151ReceivedEvent += PMeventDriver_PMtlg151Received_Handler;
                    PMeventDriver.PMtlg161DataReceivedEvent += PMeventDriver_PMtlg161DataReceived_Handler;
                }
            }
        }//ctor

        

        #endregion -- CONSTRUCTOR --

        //***********************************************

        #region --PlayMode handlery--

        void PMeventDriver_PMtlg120SendingEvent_Handler()
        {
            VariantValue = -1;
        }

        void PMeventDriver_PMtlg121DataReceived_Handler(string tlg121Data)
        {
            TlgMessage121Recieved_Handler(tlg121Data);
        }

        void PMeventDriver_PMtlg151Received_Handler(Communication.EventArgs3 e)
        {
            TlgMessage151Recieved_Handler(null, e);
        }

        void PMeventDriver_PMtlg161DataReceived_Handler(Data.Telegrams.LogicMessage161 tlg161)
        {
            TlgMessage161Received_Handler(null, new Communication.EventErrorArgsLogic() { message = tlg161.HMI_text, colorCode = tlg161.color_code, textPosition = tlg161.text_pos });
        }

        #endregion --PlayMode handlery--

        //*********************************************

        /// <summary>
        /// handler pre event odpaleny tesne pred vyslanim telegramu c. 120;
        /// v PlayMode sa nespusta
        /// </summary>
        /// <param name="dtString"></param>
        void AppCommunicator_Tlg120SendingEvent_Handler(string dtString)
        {
            string dt = dtString;
            VariantValue = -1;// VariantValue = 0;
        }

        void AppCommunicator_UnknownTelNumberReceived(object sender, Communication.EventArgs1 e)
        {
            MsgWriter.WriteRight(e.recievedMessage);
        }

        void AppCommunicator_ServerRecBufOverflowReceived(object sender, Communication.EventArgs1 e)
        {
            MsgWriter.WriteRight(e.recievedMessage);
        }

        void AppCommunicator_ServerConnectionFailed(object sender, Communication.EventArgs1 e)
        {
            MsgWriter.WriteRight(e.recievedMessage);
        }


        //MH: 10.12.2013 vsetky texty z telegramu c. 121 sa vypisuju napravo, existuje len jedna vynimka
        //ak variant je > 1, potom nevypisovat text prijaty z Logicu ani vpravo ani vlavo, ale vypisat vlastny text podla modu
        //Oznacte jeden z xx variantov posunovej cesty,Oznacte jednu z xx posunovych ciest na zrusenie,
        //kde pocet zistine z tel. 151 tu je to VariantValue
        //
        //handler pre event: textova odpoved zo servera  na spravu od HMI klienta, sprava obsahuje text OK, alebo popis chyby
        //MH: pri stavani posunovej alebo vlakovej cesty pride telegram c. 121 a telegram c. 151 Variant
        //tel. 121 obsahuje text OK, alebo text Chybove hlasenie...., alebo napr. text Vcesta: Oznacte jeden z 3 variantov vlakovej cesty
        //ak sa klikne na semafor po postaveni cesty, potom tiez prideme tu


        /*  !!!!!!!!! POZOR !!!!!!!!!!
         *   Vsetky handleri odpalovane v Communicator bezia na vlakne kde bezi kod pre tcpClientClass.
         *   Preto ak sa pristupuje k prvkom stavadla musime pouzit App.Current.Dispatcher.BeginInvoke(....) napr.
         *                  App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                            {
                                nazovCesty = stavadloModel.PosunovaCestaManager.GetCurrentFullPathName_Pcesta2();
                            }));

         * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
         */

        /// <summary>
        /// handler pre event po prijati tlg. 121
        /// </summary>
        /// <param name="tlgData"></param>
        void TlgMessage121Recieved_Handler(string tlgData) //bezi na vlakne kde bezi kod pre tcpClientClass
        {
            //string sprava = tlgData;
            currentMenuMode = GlobalData.CurrentMenuMode;
            semforKlikPovolenieCesty = false;
            string tlgMessage = tlgData.ToLower();

#if !DEBUG
            //V Release mode pri kazdej zmene smeru vymeny sa MUSI VZDY KLIKNUT na button VYMENA !!!!!
            if (currentMenuMode == CURRENT_MENU_MODE.VYMENA)//ak bol nastaveny rezim Vymena, tento rezim sa po prijme spravy OK musi zrusit!!!
            {
               if( tlgMessage == "ok")//prijata sprava OK sa nevypisuje
               {
                   appEventsInvoker.Invoke_StopVymenaTimer_Event(); //odpali event pre zastavenie timera pre vymenu
                   //zrusenie nastaveneho rezimu
                   //stavadloFactory.RegisterSelectedMenuMode(CURRENT_MENU_MODE.NONE);
                   GlobalData.CurrentMenuMode = CURRENT_MENU_MODE.NONE;
                   MsgWriter.ClearLeft(); //
                   return;
               }
               else
               {    //ak prisla ina sprava ako OK, potom sa vypise vpravo 
                    MsgWriter.WriteRight(tlgData);
               }
               return;
            }
#endif
            //System.Diagnostics.Debug.WriteLine($"***********MessageManager: TlgMessage121Recieved_Handler tlgData-{tlgData}");
            //zrusenie nastaveneho modu aplikacie pre SUHLASY, po prijme spravy OK
            if (currentMenuMode == CURRENT_MENU_MODE.UDELENIE_SUHLASU ||
                currentMenuMode == CURRENT_MENU_MODE.RUSENIE_SUHLASU ||
                currentMenuMode == CURRENT_MENU_MODE.ZIADOST_O_SUHLAS ||
                
                currentMenuMode == CURRENT_MENU_MODE.SIMULACIA_ZIADOSTI_O_SUHLAS ||
                currentMenuMode == CURRENT_MENU_MODE.SIMULACIA_RUSENIA_SUHLASU ||
                currentMenuMode == CURRENT_MENU_MODE.SIMULACIA_UDELENIA_SUHLASU)
            {
                //if (tlgData.Length == 2)//prijata sprava OK sa nevypisuje
                if (tlgMessage == "ok")//prijata sprava OK sa nevypisuje
                {
                    //stavadloFactory.RegisterSelectedMenuMode(CURRENT_MODE.NONE);//zrusenie nastaveneho rezimu, pre vybrate mody
                    //GlobalData.CurrentMenuMode = CURRENT_MENU_MODE.NONE;//zrusenie nastaveneho rezimu, pre vybrate mody
                    //MsgWriter.ClearLeft(); //
                }
                else
                {
                    MsgWriter.WriteRight(tlgData);//vypis prijatej spravy (nie OK) z Logicu
                }
                return;
            }
            //---------------
            //s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznactePrvokPreZrusPoruchy", LocalizeDictionary.Instance.Culture);
            
            #region ====if(...PCESTA, VCESTA, VYLUKA_POSUNOVEJ_CESTY, VYLUKA_VLAKOVEJ_CESTY)=====

            //Stavanie vlakovej, posunovej cesty, alebo vyluka vlakovej, posunovej cesty
            if (currentMenuMode == CURRENT_MENU_MODE.PCESTA ||
                currentMenuMode == CURRENT_MENU_MODE.VCESTA ||
                currentMenuMode == CURRENT_MENU_MODE.VYLUKA_POSUNOVEJ_CESTY || currentMenuMode == CURRENT_MENU_MODE.VYLUKA_VLAKOVEJ_CESTY)
            {
                //posledna operacia prebehla dobre, Logic  poslal tlg.151:1;L201 a tlg.121: OK
                int vv = VariantValue;

                #region --SPRAVA OK po vybere zaciatku, alebo po vybere posledneho useku vlakovej/posunovej cesty============

                if(tlgMessage.Contains("potvr"))// tlgData=pcesta: potvrďte, či sa u cesty k832-k870 jedná o prevoz ocele!  09:24:16.203
                {
                    if(GlobalData.WorkMode)
                        appEventsInvoker.PrevozOcele_IsValidEvent();
                    MsgWriter.WriteRight(tlgData);//vypise sa oznam prijaty zo servera
                    return;
                }

                //Stavanie cesty, alebo Vyluka/zrusenie vyluky: tu sa pride po vybere zaciatku, alebo po vybere koncoveho useku vlakovej/posunovej cesty
                //if( tlgMessage == "ok")//zobrazit prisluchajuci oznam v spodnom riadku displeja
                //MH: 16.04.2019 pri stavani specialnej PCesty/VCesty sprava moze obsahovat text: Potvrďte , či sa u cesty xyxz jedna o prevoz ocele
                if( (tlgMessage == "ok")  )  //zobrazit prisluchajuci oznam v spodnom riadku displeja
                {
                    var ps = stavadloModel.PosunovaCestaManager.PosunCestaStavIsStart();
                    var pe = stavadloModel.PosunovaCestaManager.PosunCestaStavIsEnd();
                    //!!!! PO POSTAVENI CESTY A PO KLIKU NA SEMAFOR PRE POVOLENIE cesty sa vypise oznam: Oznacte zaciatok dalsej posunovej/ vlakovej cesty

                    //System.Diagnostics.Debug.WriteLine($"***********MessageManager: TlgMessage121Recieved_Handler PovolenieCestyIsEnabled-{stavadloModel.PosunovaCestaManager.PovolenieCestyIsEnabled}, ps={ps}, pe={pe} {DateTime.Now: HH:mm:ss.fff}");
                    
                    if (currentMenuMode == CURRENT_MENU_MODE.PCESTA || currentMenuMode == CURRENT_MENU_MODE.VCESTA)
                    {
                        //kod spusteny po kliku na semafor pre povolenie cesty po prijme tlg. 121 "OK"
                        //ak sa klikne na semafor, ktory nepovoluje PCestu/VCestu, potom to osetri Logic a nepride tlg.OK!!!
                        if ((stavadloModel.ClickedElement is Semafor1Control))
                        {
                            var sem1 = stavadloModel.ClickedElement as Semafor1Control;
                            if (sem1.CestaPovolena)
                            {
                                if (currentMenuMode == CURRENT_MENU_MODE.PCESTA)//"Označte začiatok ďalšej posunovej cesty!
                                    MsgWriter.WriteLeft((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "zaciatokDalsejPosunCesty", LocalizeDictionary.Instance.Culture));
                                else//"Označte začiatok ďalšej vlakovej cesty!"
                                    MsgWriter.WriteLeft((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "zaciatokDalsejVlakCesty", LocalizeDictionary.Instance.Culture));
                                //System.Diagnostics.Debug.WriteLine($"**************MessageManager nasleduje return1, {DateTime.Now: HH:mm:ss.fff}");

                                sem1.IsPCestaStart = false;
                                sem1.IsPCestaEnd = false;
                                sem1.CestaPovolena = false;
                                return;
                            }
                        }
                    }
                    else if (currentMenuMode == CURRENT_MENU_MODE.VCESTA ) //ide to aj takto;  zachytenie kliku na semafor pre povolenie VCesty
                    {
                        App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                        {
                            var e1 = stavadloModel.ClickedElement;
                            if ((stavadloModel.ClickedElement is Semafor1Control))//MH: 28.03.2019  OK
                            {
                                return;
                            }
                        }));
                    }
                   

                    //MH: 26.03.2019 TU PRIDEM AJ PO KLIKU NA SEMAFOR PRE POVOLENIE CESTY!!!!!!!!!!!!!!!!!!
                    //oznacenie zaciatku posunovej cesty/vyluky posunovej cesty bolo OK: 
                    //vypise sa oznam Oznacte koniec...
                    if ((currentMenuMode == CURRENT_MENU_MODE.PCESTA || currentMenuMode == CURRENT_MENU_MODE.VYLUKA_POSUNOVEJ_CESTY) &&
                        stavadloModel.PosunovaCestaManager.PosunCestaStavIsStart()  && (semforKlikPovolenieCesty==false) )
                    {
                        //odpalenie eventu, ktoreho handler oznaci usek pre zaciatok Posunovej cesty
                        //appEventsInvoker?.InvokeStartPathPcesta_IsValidEvent((Path)stavadloModel.ClickedElement);MH: zaremovane 21.03.2019
                        System.Diagnostics.Debug.WriteLine($"**************MessageManager odpaleny event: InvokeStartPathPcesta_IsValidEvent2 pre element..semforKlikPovolenieCesty={semforKlikPovolenieCesty}, {DateTime.Now: HH:mm:ss.fff}");
                        appEventsInvoker?.InvokeStartPathPcesta_IsValidEvent2(stavadloModel.ClickedElement);// MH: pridane 21.03.2019; ClickedElement moze byt Path, alebo Semafor1Control
                       

                        if (currentMenuMode == CURRENT_MENU_MODE.PCESTA)//"Označte koniec posunovej cesty!"
                            MsgWriter.WriteLeft((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteKoniecPosunCesty", LocalizeDictionary.Instance.Culture));
                        else//"Označte koniec posunovej cesty pre výluku!"
                            MsgWriter.WriteLeft((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteKoniecPosunCestyVyluka", LocalizeDictionary.Instance.Culture));

                        return;
                    }
                    //oznacenie zaciatku vlakovej cesty/vyluky vlakovej cesty bolo OK: 
                    //vypise sa oznam Oznacte koniec...
                    else if ((currentMenuMode == CURRENT_MENU_MODE.VCESTA || currentMenuMode == CURRENT_MENU_MODE.VYLUKA_VLAKOVEJ_CESTY) &&
                        stavadloModel.VlakovaCestaManager.VlakCestaStavIsStart())//Vlakova cesta-zaciatok
                    {
                            //odpalenie eventu, ktoreho handler oznaci usek pre zaciatok Posunovej cesty
                            appEventsInvoker?.InvokeStartPathVcesta_IsValidEvent(stavadloModel.ClickedElement);
                        
                        if (currentMenuMode == CURRENT_MENU_MODE.VCESTA)//"Označte koniec vlakovej cesty!
                            MsgWriter.WriteLeft((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteKoniecVlakCesty", LocalizeDictionary.Instance.Culture));
                        else//"Označte koniec vlakovej cesty pre výluku, alebo jej zrusenie!"
                            MsgWriter.WriteLeft((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteKoniecVlakCestaVyluka", LocalizeDictionary.Instance.Culture));

                        return;
                    }

                    //ak sa oznacil koniec cesty bez variantu;
                    // vypise sa oznam Oznacte zaciatok dalsej....
                    else if ((currentMenuMode == CURRENT_MENU_MODE.PCESTA || currentMenuMode == CURRENT_MENU_MODE.VYLUKA_POSUNOVEJ_CESTY) &&
                                 stavadloModel.PosunovaCestaManager.PosunCestaStavIsEnd())
                    {
                        //odpalenie eventu, ktoreho handler prida cestu do zoznamu postaveneCestyList
                        System.Diagnostics.Debug.WriteLine($"**********MessageManager spusteny event: InvokeEndPathPcesta_Variant0_IsValidEvent pre element...");
                        appEventsInvoker?.InvokeEndPathPcesta_Variant0_IsValidEvent( stavadloModel.ClickedElement);//NH 22.03.2019

                        //!!!!!!!!!!!Po postaveni cesty sa NEMENI nastaveny rezim vybraty z menu!!!!!!!!!!! 
                        if (currentMenuMode == CURRENT_MENU_MODE.PCESTA)//"Označte začiatok ďalšej posunovej cesty!"
                            MsgWriter.WriteLeft((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "zaciatokDalsejPosunCesty", LocalizeDictionary.Instance.Culture));//"Označte začiatok ďalšej posunovej cesty!"
                        else if (currentMenuMode == CURRENT_MENU_MODE.VYLUKA_POSUNOVEJ_CESTY)//"Označte začiatok ďalšej posunovej cesty pre výluku, alebo jej zrušenie!!
                            MsgWriter.WriteLeft((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "zaciatokDalsejPosunCestyVyluka", LocalizeDictionary.Instance.Culture));
                        return;
                    }

                    //ak sa oznacil koniec cesty bez variantu
                    // vypise sa oznam Oznacte zaciatok dalsej....
                    else if ((currentMenuMode == CURRENT_MENU_MODE.VCESTA || currentMenuMode == CURRENT_MENU_MODE.VYLUKA_VLAKOVEJ_CESTY) &&
                        stavadloModel.VlakovaCestaManager.VlakCestaStavIsEnd())
                    {   
                        //odpalenie eventu, ktoreho handler prida cestu do zoznamu postaveneCestyList
                        appEventsInvoker?.InvokeEndPathVcesta_Variant0_IsValidEvent( stavadloModel.ClickedElement);

                        //!!!!!!!!!!!Po postaveni cesty sa NEMENI nastaveny rezim, vybraty z menu!!!!!!!!!!! 
                        if (currentMenuMode == CURRENT_MENU_MODE.VCESTA)//"Označte začiatok ďalšej vlakovej cesty!"
                            MsgWriter.WriteLeft((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "zaciatokDalsejVlakCesty", LocalizeDictionary.Instance.Culture));//"Označte začiatok ďalšej vlakovej cesty!"
                        else if (currentMenuMode == CURRENT_MENU_MODE.VYLUKA_VLAKOVEJ_CESTY)//Označte začiatok ďalšej vlakovej cesty pre výluku, alebo jej zrušenie!
                            MsgWriter.WriteLeft((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteZacVlakCestaVyluka", LocalizeDictionary.Instance.Culture));
                        return;
                    }

                    //ak sa oznacil posledny variant cesty, potom tlg.121: OK (cesta je postavena, vypise sa oznam Oznacte zaciatok dalsej....)
                    else if (stavadloModel.VlakovaCestaManager.VlakCestaStavIsVariant() || stavadloModel.PosunovaCestaManager.PosunCestaStavIsVariant())
                    {
                        //App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                        //{
                        //    string usek = stavadloModel.ClickedElement.Name;
                        //}));
                        appEventsInvoker?.InvokeTrainPath_IsValidEvent();

                        //!!!!!!!!!!!Po postaveni cesty sa NEMENI nastaveny rezim, vybraty z menu!!!!!!!!!!! 
                        if (currentMenuMode == CURRENT_MENU_MODE.PCESTA)//"Označte začiatok ďalšej posunovej cesty!"
                            MsgWriter.WriteLeft((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "zaciatokDalsejPosunCesty", LocalizeDictionary.Instance.Culture));//"Označte začiatok ďalšej posunovej cesty!"
                        else if (currentMenuMode == CURRENT_MENU_MODE.VYLUKA_POSUNOVEJ_CESTY)//"Označte začiatok ďalšej posunovej cesty pre výluku, alebo jej zrušenie!!
                            MsgWriter.WriteLeft((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "zaciatokDalsejPosunCestyVyluka", LocalizeDictionary.Instance.Culture));
                        else if (currentMenuMode == CURRENT_MENU_MODE.VCESTA)//"Označte začiatok ďalšej vlakovej cesty!"
                            MsgWriter.WriteLeft((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "zaciatokDalsejVlakCesty", LocalizeDictionary.Instance.Culture));//"Označte začiatok ďalšej vlakovej cesty!"
                        else if (currentMenuMode == CURRENT_MENU_MODE.VYLUKA_VLAKOVEJ_CESTY)//Označte začiatok ďalšej vlakovej cesty pre výluku, alebo jej zrušenie!
                            MsgWriter.WriteLeft((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteZacVlakCestaVyluka", LocalizeDictionary.Instance.Culture));
                        return;
                    }
                }//if (tlgData.ToLower() == "ok")

                #endregion --SPRAVA OK--


                //POZN: az po vybere posledneho Variantu pride sprava OK;
                //Ak este existuju viacere varianty, potom sprava OK nepride!
                //tu sa pride pri vybere konca posunovej/vlakovej cesty
                //Vcesta: Označte jeden z 5 variantov vlakovej cesty K603T-K421a!
                //else if ((tlgData.ToLower().Contains("vcesta") || tlgData.ToLower().Contains("pcesta") ||
                //    tlgData.ToLower().Contains("výluka")) &&
                //    !App.AppTrainPathManager.PosunCestaStavIsVariant() &&
                //    !App.AppTrainPathManager.VlakCestaStavIsVariant() && tlgData.ToLower().Contains("označte jeden z"))



                //tu sa pride len raz, ak koncovy usek pri stavani cesty, alebo vyluke/ruseni vyluky bol vybraty dobre;
                //VariantValue>1 z tlg.151
                //tlgData obsahuje napr. Vcesta: Označte jeden z 5 variantov vlakovej cesty K603T-K421a!
                //
                //tlgData obsahuje napr. Výluka posunovej cesty: Označte jeden z 5 variantov posunovej cesty K603T-K421a!
               
                else if (!stavadloModel.PosunovaCestaManager.PosunCestaStavIsVariant() && !stavadloModel.VlakovaCestaManager.VlakCestaStavIsVariant())
                {
                    //pri stavani, alebo vyluke cesty, koniec cesty bol vybraty dobre a existuju varianty cesty
                    #region --Oznacenie konca cesty--

                    if (VariantValue > 1)
                    {
                        if ((currentMenuMode == CURRENT_MENU_MODE.PCESTA) && stavadloModel.PosunovaCestaManager.PosunCestaStavIsEnd())//App.AppTrainPathManager.PosunCestaStavIsEnd()
                        {
                            MsgWriter.ClearRight();//vymazanie praveho oznamu

                            //handler oznaci usek, nastavi PosunCestaStav = PATH_CONSTRUCTION.VARIANT a prida vybratu cestu do zoznamu
                            //appEventsInvoker.InvokeEndPathPcesta_IsValidEvent(App.ClickedElementName);
                            appEventsInvoker?.InvokeEndPathPcesta_IsValidEvent(stavadloModel.ClickedElement);

                            //MH: 23.12.2013 tu by som mal pridat do zoznamu: zaciatok cesty a koniec cesty
                            //po kliku na koncovy usek cesty nasleduje prvy oznam pri stavani cesty, ak existuju varianty
                            //string nazovCesty = App.AppTrainPathManager.GetFullPathName(App.ClickedElementName);
                            //string nazovCesty = stavadloFactory.GetFullPathName(tlgData);//z tlgData vyberie nazov cesty napr. K603T-k421a

                            string nazovCesty = string.Empty;
                            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                            {
                                nazovCesty = stavadloModel.PosunovaCestaManager.GetCurrentFullPathName_Pcesta2();//App.AppTrainPathManager.GetCurrentFullPathName_Pcesta2();
                            }));

                            //msg1 = string.Format("Označte jeden z {0} variantov posunovej cesty {1}!", VariantValue, nazovCesty);
                            msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "jedenZvariantovPosunCesta", LocalizeDictionary.Instance.Culture), VariantValue, nazovCesty);
                            if ((VariantValue % 4 == 0) || (VariantValue % 6 == 0) || (VariantValue % 7 == 0))
                                // msg1 = string.Format("Označte jeden zo {0} variantov posunovej cesty {1}!", VariantValue, nazovCesty);
                                msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "jedenZoVariantovPosunCesta", LocalizeDictionary.Instance.Culture), VariantValue, nazovCesty);
                            MsgWriter.WriteLeft(msg1);

                            return;
                        }

                        if ((currentMenuMode == CURRENT_MENU_MODE.VYLUKA_POSUNOVEJ_CESTY) && stavadloModel.PosunovaCestaManager.PosunCestaStavIsEnd())
                        {
                            //koncovy usek pe vyluku bol vybraty dobre
                            //App.AppTrainPathManager.PosunCestaStav = PATH_CONSTRUCTION.VARIANT;

                            //handler oznaci usek, nastavi PosunCestaStav = PATH_CONSTRUCTION.VARIANT a prida vybratu cestu do zoznamu postaveneCestyList
                            //appEventsInvoker.InvokeEndPathPcesta_IsValidEvent(App.ClickedElementName);
                            appEventsInvoker?.InvokeEndPathPcesta_IsValidEvent(stavadloModel.ClickedElement );

                            //string nazovCesty = App.AppTrainPathManager.GetCurrentFullPathName_Pcesta2();
                            string nazovCesty = string.Empty;
                            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                            {
                                nazovCesty = stavadloModel.PosunovaCestaManager.GetCurrentFullPathName_Pcesta2();
                            }));
                            
                            //msg1 = string.Format("Označte jeden z {0} variantov posunovej cesty {1} pre výluku, alebo jej zrušenie!", VariantValue, nazovCesty);
                            msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "jedenZvariantovPosunVylukaZrusenie", LocalizeDictionary.Instance.Culture), VariantValue, nazovCesty);
                            if ((VariantValue % 4 == 0) || (VariantValue % 6 == 0) || (VariantValue % 7 == 0))
                                //msg1 = string.Format("Označte jeden zo {0} variantov posunovej cesty {1} pre výluku, alebo jej zrušenie!", VariantValue, nazovCesty);
                                msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "jedenZoVariantovPosunVylukaZrusenie", LocalizeDictionary.Instance.Culture), VariantValue, nazovCesty);
                            MsgWriter.WriteLeft(msg1);

                            return;
                        }
                        if ((currentMenuMode == CURRENT_MENU_MODE.VCESTA) && stavadloModel.VlakovaCestaManager.VlakCestaStavIsEnd())
                        {
                            MsgWriter.ClearRight();//vymazanie praveho oznamu
                            
                             //handler oznaci usek ako koncovy usek, nastavi PosunCestaStav = PATH_CONSTRUCTION.VARIANT a prida vybratu cestu do zoznamu
                             //appEventsInvoker.InvokeEndPathVcesta_IsValidEvent(App.ClickedElementName);
                             appEventsInvoker?.InvokeEndPathVcesta_IsValidEvent( stavadloModel.ClickedElement);//TODO: OPRAVIT VlakovaCestaManager!!!


                            //po kliku na koncovy usek cesty nasleduje prvy oznam pri stavani cesty, ak existuju varianty
                            //string nazovCesty = App.AppTrainPathManager.GetFullPathName(App.ClickedElementName);
                            string nazovCesty = string.Empty;
                            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                            {
                                nazovCesty= stavadloModel.VlakovaCestaManager.GetCurrentFullPathName_Vcesta2();//MH:02.01.2014
                            }));
                            // msg1 = string.Format("Označte jeden z {0} variantov vlakovej cesty {1}!", VariantValue, nazovCesty);
                            msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "jedenZvariantovVlakCesty", LocalizeDictionary.Instance.Culture), VariantValue, nazovCesty);
                            if ((VariantValue % 4 == 0) || (VariantValue % 6 == 0) || (VariantValue % 7 == 0))
                                // msg1 = string.Format("Označte jeden zo {0} variantov vlakovej cesty {1}!", VariantValue, nazovCesty);
                                msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "jedenZoVariantovVlakCesty", LocalizeDictionary.Instance.Culture), VariantValue, nazovCesty);
                            MsgWriter.WriteLeft(msg1);

                            return;
                        }
                        if ((currentMenuMode == CURRENT_MENU_MODE.VYLUKA_VLAKOVEJ_CESTY) && stavadloModel.VlakovaCestaManager.VlakCestaStavIsEnd())
                        {
                           //handler oznaci usek ako koncovy usek, nastavi PosunCestaStav = PATH_CONSTRUCTION.VARIANT a prida vybratu cestu do zoznamu
                           //appEventsInvoker.InvokeEndPathVcesta_IsValidEvent(App.ClickedElementName);
                           appEventsInvoker?.InvokeEndPathVcesta_IsValidEvent((Path)stavadloModel.ClickedElement);


                            //po kliku na koncovy usek cesty nasleduje prvy oznam pri stavani cesty, ak existuju varianty
                            //string nazovCesty = App.AppTrainPathManager.GetFullPathName(App.ClickedElementName);
                            string nazovCesty = string.Empty;
                            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                            {
                                nazovCesty = stavadloModel.VlakovaCestaManager.GetCurrentFullPathName_Vcesta2();//MH:02.01.2014
                            }));
                            //msg1 = string.Format("Označte jeden z {0} variantov vlakovej cesty {1} pre výluku, alebo jej zrušenie!", VariantValue, nazovCesty);
                            msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "jedenZvariantovVylukaZrusenie", LocalizeDictionary.Instance.Culture), VariantValue, nazovCesty);
                            if ((VariantValue % 4 == 0) || (VariantValue % 6 == 0) || (VariantValue % 7 == 0))
                                //msg1 = string.Format("Označte jeden zo {0} variantov vlakovej cesty {1} pre výluku, alebo jej zrušenie!", VariantValue, nazovCesty);
                                msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "jedenZoVariantovVylukaZrusenie", LocalizeDictionary.Instance.Culture), VariantValue, nazovCesty);
                            MsgWriter.WriteLeft(msg1);

                            return;
                        }
                    }//if( VariantValue>1)
                    else
                        MsgWriter.WriteRight(tlgData);//koniec cesty nebol vybraty dobre, vypise sa oznam
                    return;

                    #endregion ----Oznacenie konca cesty----

                }//if (!stavadloModel.PosunovaCestaManager.PosunCestaStavIsVariant() && !stavadloModel.VlakovaCestaManager.VlakCestaStavIsVariant()  )

                //tlg.121 Oznacte jeden z 2 variantov vlakovej cesty...
                //else if ((tlgData.ToLower().Contains("vcesta") || tlgData.ToLower().Contains("pcesta") ||
                //    tlgData.ToLower().Contains("výluka")) ||
                //    tlgData.ToLower().Contains("označte jeden z"))//MH pridane 13.12.2013: tlg.121>Vcesta: K603K nie je začiatkom žiadnej vlakovej cesty!


                //tu sa pride aj viackrat, ak sa vybera variant posunovej cesty pri stavani cesty, alebo vyluke/ruseni vyluky posunovej cesty
                //tlgData obsahuje napr: Oznacte jeden z 2 variantov vlakovej cesty K201-K201b
                else if ( stavadloModel.PosunovaCestaManager.PosunCestaStavIsVariant() )
                {
                    if (VariantValue > 1)
                    {
                        MsgWriter.ClearRight();
                        //string nazovCesty = App.AppTrainPathManager.GetCurrentFullPathName_Pcesta();
                        string nazovCesty = string.Empty;
                        App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                        {
                            nazovCesty = stavadloModel.PosunovaCestaManager.GetCurrentFullPathName_Pcesta2();
                        }));

                        if (currentMenuMode == CURRENT_MENU_MODE.PCESTA)
                        {
                            //msg1 = string.Format("Označte jeden z {0} variantov posunovej cesty {1}!", VariantValue, nazovPcesty);
                            msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "jedenZvariantovPosunCesta", LocalizeDictionary.Instance.Culture), VariantValue, nazovCesty);
                            if ((VariantValue % 4 == 0) || (VariantValue % 6 == 0) || (VariantValue % 7 == 0))
                                //msg1 = string.Format("Označte jeden zo {0} variantov posunovej cesty {1}!", VariantValue, nazovPcesty);
                                msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "jedenZoVariantovPosunCesta", LocalizeDictionary.Instance.Culture), VariantValue, nazovCesty);
                        }
                        if (currentMenuMode == CURRENT_MENU_MODE.VYLUKA_POSUNOVEJ_CESTY)   //K603K nie je zaciatkom ziadnej vlakovej cesty
                        {
                            //msg1 = string.Format("Označte jeden z {0} variantov posunovej cesty {1} pre výluku, alebo jej zrušenie!", VariantValue, nazovPcesty);
                            msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "jedenZvariantovPosunVylukaZrusenie", LocalizeDictionary.Instance.Culture), VariantValue, nazovCesty);
                            if ((VariantValue % 4 == 0) || (VariantValue % 6 == 0) || (VariantValue % 7 == 0))
                                //msg1 = string.Format("Označte jeden zo {0} variantov posunovej cesty {1} pre výluku, alebo jej zrušenie!", VariantValue, nazovPcesty);
                                msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "jedenZoVariantovPosunVylukaZrusenie", LocalizeDictionary.Instance.Culture), VariantValue, nazovCesty);
                        }
                        MsgWriter.WriteLeft(msg1);
                    }
                    else
                        MsgWriter.WriteRight(tlgData);
                    return;
                }
                else if (stavadloModel.VlakovaCestaManager.VlakCestaStavIsVariant())
                {
                    if (VariantValue > 1)
                    {
                        MsgWriter.ClearRight();

                        string nazovCesty = string.Empty;
                        App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                        {
                            nazovCesty = stavadloModel.VlakovaCestaManager.GetCurrentFullPathName_Vcesta2();
                        }));

                        if (currentMenuMode == CURRENT_MENU_MODE.VCESTA)
                        {
                            //msg1 = string.Format("Označte jeden z {0} variantov vlakovej cesty {1}!", VariantValue, nazovVcesty);
                            msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "jedenZvariantovVlakCesty", LocalizeDictionary.Instance.Culture), VariantValue, nazovCesty);
                            if ((VariantValue % 4 == 0) || (VariantValue % 6 == 0) || (VariantValue % 7 == 0))
                                //msg1 = string.Format("Označte jeden zo {0} variantov vlakovej cesty {1}!", VariantValue, nazovVcesty);
                                msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "jedenZoVariantovVlakCesty", LocalizeDictionary.Instance.Culture), VariantValue, nazovCesty);
                        }
                        if (currentMenuMode == CURRENT_MENU_MODE.VYLUKA_VLAKOVEJ_CESTY)  
                        {
                            //msg1 = string.Format("Označte jeden z {0} variantov vlakovej cesty {1} pre výluku, alebo jej zrušenie!", VariantValue, nazovVcesty);
                            msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "jedenZvariantovVylukaZrusenie", LocalizeDictionary.Instance.Culture), VariantValue, nazovCesty);
                            if ((VariantValue % 4 == 0) || (VariantValue % 6 == 0) || (VariantValue % 7 == 0))
                                //msg1 = string.Format("Označte jeden zo {0} variantov vlakovej cesty {1} pre výluku, alebo jej zrušenie!", VariantValue, nazovVcesty);
                                msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "jedenZoVariantovVylukaZrusenie", LocalizeDictionary.Instance.Culture), VariantValue, nazovCesty);
                        }
                        MsgWriter.WriteLeft(msg1);
                    }
                    else
                        MsgWriter.WriteRight(tlgData); //
                    return;
                }

                if (tlgData.Length != 2)//aby sa nevypisoval text OK; vypisuje sa len ak pride chybova hlaska
                {
                    if (VariantValue > 1)
                        return;
                    MsgWriter.WriteRight(tlgData);
                }
            }// if (appCurrentMode == CURRENT_MODE.PCESTA || appCurrentMode == CURRENT_MODE.VCESTA || appCurrentMode == CURRENT_MODE.VYLUKA_POSUNOVEJ_CESTY || appCurrentMode == CURRENT_MODE.VYLUKA_VLAKOVEJ_CESTY)

            #endregion --if(...PCESTA, VCESTA, VYLUKA_POSUNOVEJ_CESTY, VYLUKA_VLAKOVEJ_CESTY)-------------

            //pozn: na zaciatku zrusenia cesty tlg.151:4 a tlg 121:Zruš: Označte jednu zo 4 ciest na zrušenie!
            //po zruseni prvej cesty tlg.161: Zrušenie posunovej cesty K217-K217a;b;d   tlg.151:3; a tlg.121:OK, tlg.141
            //
            //po zruseni poslednej cesty: tlg.161:Zrušenie posunovej cesty K201-K201a;b;d;  tlg.151:0; tlg. 121 OK, tlg.141
            else if ((currentMenuMode == CURRENT_MENU_MODE.ZRUSENIE_CESTY || (currentMenuMode == CURRENT_MENU_MODE.ZRUSENIE_VARIANTU_CESTY)))//&& tlgData.Length > 2
            {
                if (tlgData.ToLower() == "ok")//zrusenie poslednej cesty; zrusi sa mod pre rusenie ciest
                {
                    if (VariantValue == 0)
                    {
                        //stavadloFactory.RegisterSelectedMenuMode(CURRENT_MENU_MODE.NONE);
                        GlobalData.CurrentMenuMode = CURRENT_MENU_MODE.NONE;
                        MsgWriter.WriteLeft("");
                        return;
                    }
                }
                if (VariantValue == 1)
                {
                    //"Označte cestu na zrušenie!";
                    MsgWriter.WriteLeft((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteCestuNaZrusenie", LocalizeDictionary.Instance.Culture));
                    //MsgWriter.ClearRight();//vymazanie praveho oznamu
                    return;
                }
                else if (VariantValue > 1)
                {
                    //msg1 = string.Format("Označte jednu z {0} ciest na zrušenie!", VariantValue);
                    msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteJednuZ", LocalizeDictionary.Instance.Culture), VariantValue);
                    if ((VariantValue % 4 == 0) || (VariantValue % 6 == 0) || (VariantValue % 7 == 0))
                        // msg1 = string.Format("Označte jednu zo {0} ciest na zrušenie!", VariantValue);
                        msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteJednuZo", LocalizeDictionary.Instance.Culture), VariantValue);
                    MsgWriter.WriteLeft(msg1);
                    //MsgWriter.ClearRight();//vymazanie praveho oznamu
                    return;
                }

                else //VariantValue = 0 sa nastavi pred kazdym vyslanim telegamu 120; VariantValue sa nastavi po prijme tlg.151; ak nie je cesta na zrusenie
                {
                    //tu je VariantValue = 0, ale tlg.121 neobsahuje spravu OK
                    MsgWriter.WriteRight(tlgData);//Zruš: žiadne jazdné cesty nie sú postavené!|Zruš:Posunova cesta xxxx-yyyy bude zrusena o zz sekund!
                }
            }
            else
            {
                if (tlgData.ToLower() != "ok")  //OK sa nevypisuje, ina prijata sprava sa vypise do spodneho riadku aplikacie
                {
                    MsgWriter.WriteRight(tlgData);
                }
            }
        }//AppCommunicator_TlgMessage121Recieved

        //tlg.151 pride vzdy pred tlg.121, ktory obsahuje spravu, ktoru treba vypisat;
        //ak VarianValue >1, potom spravu z telegramu 121 nevypisovat, ale vytvorit vlastny message a ten vypisat vlavo!!!!!
        //Ak chceme postavit cestu a existuju nejake varianty
        //Ak chceme zrusit postavenu cestu a pride nam sprava z Logicu, ze existuju rôzne varianty 
        /*
   <<HMI-MENU>>|20.12.2013 10:31:14.526;0;0;1920;1080;1934;1094;433;74;ZRUSENIE_CESTY
  <<HMI-TLG120>>|Zrus;;0;z;x
  <<HMI-TLG151>>|3;
  <<HMI-TLG121>>|Zruš: Označte jednu z 3 ciest na zrušenie!
  <<HMI-Info>>|tcpClientFx: Prijatý neznámy telegram!
  <<HMI-CLICK>>|20.12.2013 10:31:41.352;0;0;1920;1080;1934;1094;591;465;V810;ZRUSENIE_VARIANTU_CESTY
  <<HMI-TLG120>>|Zrus;V810;0;v;+
  <<HMI-TLG161>>|Zrušenie posunovej cesty K604-K230;b;d
  <<HMI-TLG151>>|2;
  <<HMI-TLG121>>|OK
  <<HMI-TLG141>>|

         * ONESKORENE ZRUSENIE CESTY
         *   20.01.2014 08:31:31,083  <<HMI-MENU>>|20.01.2014 08:31:31.083;0;0;1920;1080;1934;1094;415;80;ZRUSENIE_CESTY
             20.01.2014 08:31:31,087  <<HMI-TLG120>>|Zrus;;0;z;x
             20.01.2014 08:31:31,168  <<HMI-TLG151>>|2;
             20.01.2014 08:31:31,387  <<HMI-TLG121>>|Zruš: Označte jednu z 2 ciest na zrušenie!
             20.01.2014 08:31:33,224  <<HMI-CLICK>>|20.01.2014 08:31:33.224;0;0;1920;1080;1934;1094;415;550;K603;ZRUSENIE_VARIANTU_CESTY
             20.01.2014 08:31:33,224  <<HMI-TLG120>>|Zrus;K603;0;v;.
             20.01.2014 08:31:33,512  <<HMI-TLG121>>|Zruš: Vlaková cesta K603T-K230 bude zrušená o 60s!
             20.01.2014 08:31:33,765  <<HMI-TLG141>>|
             20.01.2014 08:32:33,425  <<HMI-TLG161>>|Oneskorené zrušenie vlakovej cesty K603T-K230;b;d
             20.01.2014 08:32:33,667  <<HMI-TLG151>>|1;  po telegrame c.151 uz nepride tlg.121:OK
             20.01.2014 08:32:33,914  <<HMI-TLG141>>|
         * Neviem odlisit normalne rusenie cesty a oneskorene rusenie cesty.
         * Ak je to normalne rusenie cesty, potom po zruseni cesty, ked sa prijme tlg.151, sa vypise napr.: Oznacte jednu z 3 ciest na zrusenie!
         * Ale po prijme tlg.121 sa opat vypise tento oznam napr.: Oznacte jednu z 3 ciest na zrusenie!
         * 
         * Oneskorene rusenie cesty, po prijme tlg.151 po zruseni cesty sa vypise napr.:  Oznacte jednu z 3 ciest na zrusenie!
         * ale uz nepride tlg. 121: OK, takze ostane tento vypis
         */

        void TlgMessage151Recieved_Handler(object sender, EventArgs3 e)//Pozn: tlg. 151 sa spacuvava aj v UC_MapaStavadla.xaml.cs
        {
            //vypis oznamov sa robi po prijme tlg. 121
            VariantValue = e.Variant.var_number;
            currentMenuMode = GlobalData.CurrentMenuMode;
            if (currentMenuMode == CURRENT_MENU_MODE.ZRUSENIE_CESTY || currentMenuMode == CURRENT_MENU_MODE.ZRUSENIE_VARIANTU_CESTY)
            {
                if (e.Variant.var_number > 0)
                {
                    //stavadloFactory.RegisterSelectedMenuMode(CURRENT_MENU_MODE.ZRUSENIE_VARIANTU_CESTY);
                    GlobalData.CurrentMenuMode = CURRENT_MENU_MODE.ZRUSENIE_VARIANTU_CESTY;
                    //ak je oneskorene rusenie cesty, potom po telegrame c.151 uz nepride tlg.121:OK, preto vypis oznamu sa urobi tu
                    if (VariantValue == 1)
                    {
                        //"Označte cestu na zrušenie!";
                        MsgWriter.WriteLeft((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteCestuNaZrusenie", LocalizeDictionary.Instance.Culture));
                        //MsgWriter.ClearRight();//vymazanie praveho oznamu
                        return;
                    }
                    msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteJednuZ", LocalizeDictionary.Instance.Culture), VariantValue);
                    if ((VariantValue % 4 == 0) || (VariantValue % 6 == 0) || (VariantValue % 7 == 0))
                        // msg1 = string.Format("Označte jednu zo {0} ciest na zrušenie!", VariantValue);
                        msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "oznacteJednuZo", LocalizeDictionary.Instance.Culture), VariantValue);
                    MsgWriter.WriteLeft(msg1);

                }
                if (VariantValue == 0)  //Po ozname z Logicu napr.: Vlakova cesta K603T-K229 bude onedlho zrusena! Po tomto ozname uz nepride tlg. 121: OK
                    MsgWriter.ClearLeft();//ak uz nie je cesta na zrusenie, vymaze lavy text: Oznacte cestu na zrusenie
            }
        }//TlgMessage151Recieved

        //void TlgMessage151Recieved(object sender, Infrastructure.Communication.EventArgs3 e)
        //{
        //    //vypis oznamov sa robi po prijme tlg. 121
        //    VariantValue = e.Variant.var_number;

        //    if (appCurrentMode == CURRENT_MODE.ZRUSENIE_CESTY || appCurrentMode == CURRENT_MODE.ZRUSENIE_VARIANTU_CESTY)
        //    {
        //        if (e.Variant.var_number > 0)
        //            stavadloFactory.RegisterSelectedMode(CURRENT_MODE.ZRUSENIE_VARIANTU_CESTY);
        //        if (VariantValue == 0)  //Po ozname z Logicu napr.: Vlakova cesta K603T-K229 bude onedlho zrusena! Po tomto ozname uz nepride tlg. 121: OK
        //            MsgWriter.ClearLeft();//ak uz nie je cesta na zrusenie, vymaze lavy text: Oznacte cestu na zrusenie
        //    }
        //}//TlgMessage151Recieved



        /*POZNAMKY:
         * tlg.161 pride pred tlg.121:OK
         * tlg. 161 pride po odoslani tlg.120 na konci:
         * postavenia vlakovej/posunovej cesty
         * zrusenia vlakovej/posunovej cesty
         * vyluky vlakovej/posunovej cesty
         * zrusenia vyluky vlakovej/posunovej cesty
         * --------------------------------------------
         * Vyluka
         * tlg.161 pride na konci vyluky posunovej/vlakovej cesty ked je cesta uz jednoznacne urcena;
         * obsah napr. tlg.161: Výluka posunovej cesty K201-K201b cez K203;b;d
         * Vcesta, Pcesta
         * tlg.161 pride na konci postavenia posunovej/vlakovej cesty ked je cesta uz jednoznacne urcena;
         * obsah napr: tlg.161:Postavenie vlakovej cest K603T-K421a cez V813a,V810,V808,K603;b;d
         * Zrus
         * tlg.161 pride po zruseni posunovej/vlakovej cesty
         * obsah tlg.161:Zrusenie vlakovej cesty K603T-K421a;b;d
         * ONESKORENE ZRUSENIE CESTY
         *   20.01.2014 08:31:31,083  <<HMI-MENU>>|20.01.2014 08:31:31.083;0;0;1920;1080;1934;1094;415;80;ZRUSENIE_CESTY
             20.01.2014 08:31:31,087  <<HMI-TLG120>>|Zrus;;0;z;x
             20.01.2014 08:31:31,168  <<HMI-TLG151>>|2;
             20.01.2014 08:31:31,387  <<HMI-TLG121>>|Zruš: Označte jednu z 2 ciest na zrušenie!
             20.01.2014 08:31:33,224  <<HMI-CLICK>>|20.01.2014 08:31:33.224;0;0;1920;1080;1934;1094;415;550;K603;ZRUSENIE_VARIANTU_CESTY
             20.01.2014 08:31:33,224  <<HMI-TLG120>>|Zrus;K603;0;v;.
             20.01.2014 08:31:33,512  <<HMI-TLG121>>|Zruš: Vlaková cesta K603T-K230 bude zrušená o 60s!
             20.01.2014 08:31:33,765  <<HMI-TLG141>>|
             20.01.2014 08:32:33,425  <<HMI-TLG161>>|Oneskorené zrušenie vlakovej cesty K603T-K230;b;d
             20.01.2014 08:32:33,667  <<HMI-TLG151>>|1;
             20.01.2014 08:32:33,914  <<HMI-TLG141>>|
         * */

        /// <summary>
        /// handler pre telegram c. 161 z Logicu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TlgMessage161Received_Handler(object sender, Communication.EventErrorArgsLogic e)
        {
            string myMessage = e.message;       //max 100 znakov
            char colorCode = e.colorCode;       //kod farby, 'b' - cierna, 'r' - cervena
            char textPosition = e.textPosition; // pozicia textu, 'c' - v strede obrazovky, 'd' - dole, v spodnom riadku 


            switch (textPosition)
            {
                //21.10.2013 odkaz od M.Mervu zakazat: Vypadok komunikacie s HMI - Master, Obnovenie komunikacie s HMI - Master, Ukončenie komunikacie s HMI - Master
                case 'c'://Dialogove okno v strede displeja; Ak pride sprava Vypadok komunikacie...., Obnovenie komunikacie... potom neodpalovat event!!!!!
                    if (myMessage.Length > 0)
                    {
                        //aby sa nezobrazovali tieto hlasky na displeji pri ladeni
                        //zaremovane 18.11.2013, aby nechodili spravy vypadok komunikacie z guardlogic
                        //!!!!PRI RELEASE VERZII NECHAT ZAREMOVANE, ABY SA SPUSTAL EVENT Message161ReceivedEvent!!!!
                        //if (myMessage.Contains("Vypadok komunik") || myMessage.Contains("Výpadok komunik") || myMessage.Contains("Obnovenie komunik") || myMessage.Contains("Ukončenie komunikácie s HMI"))
                        //    return;
                        MsgWriter.ClearRight();
                        //event zachytava MainWindow; zobrazi okno v strede displeja
                        Message161ReceivedEvent?.Invoke(new LogicEventErrorArgs() { colorCode = colorCode, errorMessage = myMessage });//novsia syntax pre odpalenie eventu
                    }
                    break;
                case 'd': //oznam v spodnom riadku

                    if (MsgWriter != null)
                    {
                        if (colorCode == 'r')//cerveny text
                            MsgWriter.WriteRight(myMessage, Enums.INFO_MODE.ERROR);
                        else //cierny text
                        {
                            MsgWriter.WriteRight(myMessage, Enums.INFO_MODE.INFO);

                            if (myMessage.Contains("Privol") && (myMessage.Contains("štart")))//Ak pride message: Privolávacia návesť - štart, potom vlavo zobrazim oznam: Pre ukončenie....
                            {
                                //Pre ukončenie privolávacej návesti uvoľnite tlačidlo!
                                s1 = (string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "ukonceniePrivolNavesti", LocalizeDictionary.Instance.Culture);
                                MsgWriter.WriteLeft(s1);
                            }
                            if (myMessage.Contains("Privol") && (myMessage.Contains("koniec")))//Ak pride message: Privolávacia návesť - koniec, potom vlavo vymazem oznam
                            {
                                MsgWriter.WriteLeft("");
                            }
                        }
                    }
                    break;
                default: break;
            }
        }//TlgMessage161Received


        //handler pre event po pripojeni na server, event sa zachyti v Communicator.cs
        void AppCommunicator_InitConnectionRecieved(object sender, Communication.EventArgs1 e)
        {
            msg1 = string.Format((string)LocalizeDictionary.Instance.GetLocalizedObject("Stavadlo22", "Trans", "appConnectedToServer", LocalizeDictionary.Instance.Culture));
            //("Aplikácia je pripojená na server");
            MsgWriter.WriteRight(msg1);
        }

        private void AppEventsInvoker_ServerConnectionEventHandler(string msg)
        {
            MsgWriter.WriteRight(msg);
        }


    }//class MessageManager

    public class LogicEventErrorArgs : EventArgs
    {
        /// <summary>
        /// textova sprava o chybe
        /// </summary>
        public string errorMessage;//max. 100 znakov
        public char colorCode; // kod farby pisma, 'b' - cierna, 'r' - cervena
    }
}
