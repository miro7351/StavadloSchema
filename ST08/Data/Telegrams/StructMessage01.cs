using System;
using System.Runtime.InteropServices;

namespace PA.Stavadlo.Data.Telegrams
{
    //POZN: Merva tuto structuru nazyva Povel
    /// <summary>
    /// telegram c. 120 vysielany do servera Logic22
    /// telegram obsahuje kod rezimu, elementName, level a kod 
    /// </summary>
    public struct MessageToLogic
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
        public string kodRezimu;  //skratka rezimu: napr. Vymena 

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string elementName;//nazov prvku stavadla: napr. V830

        public char level;  //ako char 0,1,2
        public char kod;    //napr. 'x
        public char kod2;    //rameno vyhybky '+'/'-'/'.'(pri stavani ciest)
        public char kod3;   // 0,1,2 pri stavani PCesty/VCesty kde sa moze prevazat ocel
                            // '0' - nevieme, ci sa jedna o prevoz ocele
                            // '1' - jedna sa o prevoz ocele
                            // '2' - nejedna sa o prevoz ocele


        public MessageToLogic(int i)
        {
            kodRezimu = "";
            elementName = "";
            level = '0';
            kod = 'x';
            kod2 = '.';
            kod3 = '0';
        }

        //pridane:30.11.2013 MH
        /// <summary>
        /// vrati string kde su ulozene polozky stuctury, oddelovac je znak ;
        /// pouziva sa pri zapise do logu
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //return base.ToString();
            //string s1 = string.Format("{0};{1};{2};{3};{4}", kodRezimu, elementName, level.ToString(), kod.ToString(), kod2.ToString());
            return $"{kodRezimu};{elementName};{level.ToString()};{kod.ToString()};{kod2.ToString()};{kod3.ToString()}";
        }


        /// <summary>
        /// zo stringu vytvori struct MessageToLogic;
        /// Pouziva sa v PlayMOde
        /// </summary>
        /// <param name="dataString"></param>
        /// <returns>null alebo </returns>
        public static MessageToLogic? FromString(string dataString)
        {
            MessageToLogic? newMessageToLogic = null;
            if (string.IsNullOrEmpty(dataString))
                return null;

            string[] data = dataString.Split(';');
            if (data.Length != 6)
                return null;
            try
            {
                newMessageToLogic = new MessageToLogic() { kodRezimu = data[0], elementName = data[1], level = data[2][0], kod = data[3][0], kod2 = data[4][0], kod3 = data[5][0] };
            }
            catch (Exception ex)
            {
                return newMessageToLogic;
                throw new Exception("Chyba pri vytvoreni struct MessageToLogic: " + ex.Message);
            }

            return newMessageToLogic;
        }
    }//MessageToLogic



    //pouziva sa pre telegram c. 151
    public struct VariantCesty
    {
        public UInt16 var_number; //pocet variantov pri stavani, alebo ruseni cesty
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string signalName;   //nazov navestidla povolujuceho cestu


        //--pridane 30.11.2013 MH--
        public VariantCesty(int i)
        {
            var_number = 0;
            signalName = string.Empty;

        }

        /// <summary>
        /// vrati string kde su ulozene polozky stuctury, oddelovac je znak ;
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0};{1}", var_number, signalName);
        }

        //public static VariantCesty FromString(string dataString)
        //{
        //    VariantCesty newVariantCesty = new VariantCesty(0);
        //    if (string.IsNullOrEmpty(dataString))
        //        return newVariantCesty;

        //    string[] data = dataString.Split(';');
        //    if (data.Length != 2)
        //        return newVariantCesty;
        //    newVariantCesty.var_number = Convert.ToUInt16(data[0]);
        //    newVariantCesty.signalName = data[1];

        //    return newVariantCesty;
        //}

        //pozn.: VariantCesty? novyVariantCesty = VariantCesty.FromString( data).GetValueOrDefault();
        /// <summary>
        /// zo stringu vygeneruje VariantCesty
        /// </summary>
        /// <param name="dataString"></param>
        /// <returns></returns>
        public static VariantCesty? FromString(string dataString)
        {
            VariantCesty? newVariantCesty = null;
            if (string.IsNullOrEmpty(dataString))
                return newVariantCesty;

            string[] data = dataString.Split(';');
            if (data.Length != 2)
                return newVariantCesty;
            try
            {
                newVariantCesty = new VariantCesty() { var_number = Convert.ToUInt16(data[0]), signalName = data[1] };
            }
            catch (Exception)
            {
                return newVariantCesty;
                throw new Exception("Chyba pri konverzii stringu na Variant!");
            }

            return newVariantCesty;
        }

        public static bool FromString(string dataString, out VariantCesty newVariantCesty)
        {
            newVariantCesty = new VariantCesty(0);
            bool result = false;

            if (string.IsNullOrEmpty(dataString))
                return result;

            string[] data = dataString.Split(';');
            if (data.Length != 2)
                return result;

            try
            {
                newVariantCesty.var_number = Convert.ToUInt16(data[0]);
                newVariantCesty.signalName = data[1];
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        //----------------------------------
    }//struct VariantCesty


    //M.Merva:
    //Telegram sa vyuziva aj pre zapisy do databazy, preto su v strukture doplnene posledne 4 polozky: date_time, element, status_code, err_text, ktore pre Teba nemaju vyznam.
    //M.Merva: zmena 29.10.2013 pre HMI pouzivat len HMI_text, color_code a text_pos
    //Prosim Ta, prihlas sa na odber tohto telegramu (c. 161) a v pripade, ze HMI_text != "", tak vykonaj vypis textu podla parametrov color_code a text_pos.
    public struct LogicMessage161  //povodne ErrMessage
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        public string HMI_text;
        // text chyboveho hlasenia na obrazovke HMI
        public char color_code;    // kod farby, 'b' - cierna, 'r' - cervena
        public char text_pos;      // pozicia textu, 'c' - v strede obrazovky, 'd' - dole, v spodnom riadku

        public LogicMessage161(int i)
        {
            HMI_text = string.Empty;
            color_code = 'x';
            text_pos = 'x';
        }
        //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        //public string date_time;  // vo formate "yyyy-MM-dd HH24:mm:ss"

        //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        //public string element;   // nazov prvku

        //public Int16 status_code;// kod stavu prvku

        //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        //public string err_text;  // text chyboveho hlasenia pre zapis do DB

        public override string ToString()
        {
            return string.Format("{0};{1};{2}", HMI_text, color_code.ToString(), text_pos.ToString());
        }

        public static bool FromString(string dataString, out LogicMessage161 newLogicMessage)
        {
            bool result = false;
            newLogicMessage = new LogicMessage161(0);


            if (string.IsNullOrEmpty(dataString))
                return result;

            string[] data = dataString.Split(';');
            if (data.Length != 3)
                return result;

            try
            {
                newLogicMessage.HMI_text = data[0];
                newLogicMessage.color_code = data[1][0];
                newLogicMessage.text_pos = data[2][0];
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }
    }//struct LogicMessage161



    public struct User_Login //Telegram 110 (HMI -> login rolemanager) a 111 (RoleManager-> HMI overenie uzivatela)
    {
        /// <summary>
        /// ip adresa pocitaca odkial sa chce prihlasit
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string ip_addres;

        /// <summary>
        /// mod programu:M-master, D-dispecer, S-servis(udrzba), R-readonly
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string typ_app;  //typ applikacie M-master(Dispecer,Servisny) , R - readonly

        /// <summary>
        /// login uzivatela (priezvisko)
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
        public string meno;    //login uzivatela

        /// <summary>
        /// heslo uzivatela
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
        public string heslo; //heslo uzivatela

        public int akcia;   //ci ide o prihlasenie , odhlasenie , zmenu hesla (1 - prihlasenie, 2 - odhlasenie, 3 - zmena hesla)

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
        public string heslo_nove;

        /// <summary>
        /// status prihlasenia, 1 - OK, inac chybne prihlasenie;
        /// nastavi RoleManager
        /// </summary>
        public int status; //status prihlasenia 1 - OK, inac chybne prihlasenie

        /// <summary>
        /// text statusu prihlasenia;
        /// OK, alebo pri chybnom prihlaseni obsahuje chybovu hlasku
        /// nastavi RoleManager
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string odpoved;  //textova odpoved statusu prihlasenia


        /// <summary>
        /// rola prihlaseneho uzivatela;
        /// nastavi RoleManager
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string userRole;  ////Role uzivatela napr: "U|D|R|A" U-Udrzba, D-Dispecer, R - readonly, A-Administrator(pri zasahu procesnej)

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string hmi_identity;  //nazov identity hmi ktorej odpovedal. (Nastavuje role manager z dovodu toho ze to posiela aj do logic17)


        public User_Login(int i)
        {
            ip_addres = "";
            typ_app = "R";
            meno = "";
            heslo = "";
            status = 0;
            odpoved = "";
            userRole = "";
            akcia = 0;
            heslo_nove = "";
            hmi_identity = "";
        }

        public override string ToString()
        {
            //string s1 = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8}", meno, heslo, ip_addres, typ_app, status, odpoved, userRole, akcia, heslo_nove, hmi_identity);
            return string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9}", meno, heslo, ip_addres, typ_app, status, odpoved, userRole, akcia, heslo_nove, hmi_identity);
        }

        //pre PlayMode
        public User_Login? FromString(string dataString)
        {
            User_Login? newUserLogin = null;
            if (string.IsNullOrEmpty(dataString))
                return newUserLogin;

            string[] data = dataString.Split(';');
            if (data.Length != 10)
                return newUserLogin;

            try
            {
                newUserLogin = new User_Login()
                {
                    meno = data[0],
                    heslo = data[1],
                    ip_addres = data[2],
                    typ_app = data[3],
                    status = Convert.ToInt32(data[4]),
                    odpoved = data[5],
                    userRole = data[6],
                    akcia = Convert.ToInt16(data[7]),
                    heslo_nove = data[8],
                    hmi_identity = data[9]
                };
            }
            catch (Exception)
            {
                throw new Exception("Chyba pri konverzii stringu na User_Login");
            }

            return newUserLogin;
        }
    }//User_Login



    public class UserLoginEventArgs : EventArgs
    {
        public UserLoginEventArgs(User_Login loginData)
        {
            userLoginData = new User_Login(0);
            userLoginData.akcia = loginData.akcia;
            userLoginData.ip_addres = loginData.ip_addres;
            userLoginData.odpoved = loginData.odpoved;
            userLoginData.status = loginData.status;//1-OK; inac chyba
            userLoginData.userRole = loginData.userRole;
        }
        public User_Login userLoginData;
    }



    /// <summary>
    /// popisuje stav prvku na stavadle;
    /// telegram c.131
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct PRVOK_STAVADLA    //povodny nazov prvok
    {
        /// <summary>
        /// nazov prvku na mape stavadla
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string nazov;//nazov prvku na mape stavadla, napr. K464, V801, S228, L229c,....

        public Int16 stav;
        public char podtyp;//podla typu: n-kolaj. usek patri k navestidlu, v-kolaj. usek patri k vyhybke, o-patri k obvodu, O-kolaj. usek neizolovany, s-suhlas, p-pomocny signal ak je typ suhlas

        /// <summary>
        /// stav navestidla alebo vymeny
        /// </summary>
        public Int16 vyluka;

        /// <summary>
        /// uvolnenie izolacie kolajoveho useku
        /// </summary>
        public Int16 uvolizol;

        public override string ToString()
        {
            return string.Format("{0};{1};{2};{3};{4}", nazov, stav, podtyp.ToString(), vyluka, uvolizol);
        }

        public PRVOK_STAVADLA? FromString(string dataString)
        {
            PRVOK_STAVADLA? newPRVOK_STAVADLA = null;
            if (string.IsNullOrEmpty(dataString))
                return newPRVOK_STAVADLA;

            string[] data = dataString.Split(';');
            if (data.Length != 5)
                return newPRVOK_STAVADLA;

            try
            {
                newPRVOK_STAVADLA = new PRVOK_STAVADLA() { nazov = data[0], stav = Convert.ToInt16(data[1]), podtyp = data[2][0], vyluka = Convert.ToInt16(data[3]), uvolizol = Convert.ToInt16(data[4]) };
            }
            catch (Exception)
            {
                throw new Exception("Chyba pri konverzii stringu na PRVOK_STAVADLA");
            }
            return newPRVOK_STAVADLA;
        }
    }//struct PRVOK_STAVADLA

    /*  ST22
     *  Merva Logic:dimenzovane na max. 260 prvkov /100 navestidiel, 150 kolajovych usekov a 10 pomocnych signalov/

       MH Januar 2019: useky neizolovane: 28
                         useky izolovane: 30
                                  vymeny: 61
                                 semfory: 82 
                              priecestie:  1
                              ---------------
                   pocet vsetkych prvkov:202
       Na ST22 je 149 prvkov, ale M.Merva odporuca nastavit rozmer pola na 180
     */

    /// <summary>
    /// struktura prijata z Logic22, obsahuje stav kazdeho prvku stavadla;
    /// prijima sa v telegrame c. 141
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Elements
    {
        public int NumOfElements;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 225)]//MH 08.02.2019 v telegrame NumOfElements=221
        public PRVOK_STAVADLA[] element;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
        public string dateTime;  //vo formate dd.MM.yyyy HH:mm:ss.FFF
        public Elements(int i)
        {
            NumOfElements = 0;
            element = new PRVOK_STAVADLA[225];
            dateTime = "";
        }
    }

    /// <summary>
    /// struktura pre zobrazenie informacii o prvku stavadla;
    /// Od Logic17 dostaneme telegram c. 131 (v pripade, ze element je prvkom kolajiska) alebo
    ///telegram c. 121 s chybovou hlaskou.
    ///Merva: Stavy suhlasov a pomocnych signalov by som neprenasal v kazdom telegrame, ale len na vyziadanie
    ///(napr. kliknutim na nejaky symbol na obrazovke).
    /// </summary>
    public struct InfoData
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string element;    // prvok, ktoreho data su pozadovane
        public char typ;          // 'n' - navestidlo, 'v' - vymena, 'u' - izolovany kol. usek, '0' - neizolovany kol. usek, 's' - suhlas
                                  // public char podTyp;          //od 22.10.2013 od verzie 1.0.1.54 'N' - navestidlo,'n'-usek patriaci navestidlu   'v' - usek patriaci vymene, '0' - neizolovany kol. usek, 's' - suhlas, 'p'-pomocny signal
        public UInt16 index;      // por. cislo v zozname navestidiel / kol. usekov / suhlasov
        public Int16 stav;       //stavove slovo stav
        public Int16 vystup;
        public UInt16 adresa;
        public UInt16 stojan;
        public Int16 vyluka;     //stavove slovo vyluka
        public Int16 uvolizol;  //stavove slovo uvolizol
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string KO;         // prisluchajuci kolajovy obvod (navestidlo alebo vymena, resp. zdruzene vymeny);
        //pre usek je to priradene navestidlo; pre vyhybku je to pridareny usek; pre navestidlo je to priradeny usek

        public override string ToString()
        {
            //return base.ToString();
            //return string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9}", element, typ.ToString(), index, stav, vystup, adresa, stojan, vyluka, uvolizol, KO);
            return $"{element};{typ.ToString()};{index};{stav};{vystup};{adresa};{stojan};{vyluka};{uvolizol};{KO}";
        }

    }//struct InfoData


    #region === Obsadenie kolaje====
    //Telegram c.191 obsahuje tuto structuru:
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Sets   // 01.03.2019 struktura dat s informaciami o vsetkych supravach
    {
        public void Set()
        {
            set = new SupravaData[50];//MAXSUP=50
        }

        public int NumOfSets;                                               // pocet suprav
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 50)]
        public SupravaData[] set;
    }


    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct SupravaData         // 01.03.2019 struktura dat s informaciami o suprave prijata v tlg. 191
    {
        public UInt16 cislo_supravy;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        public string kolaj;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
        public string datum;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        public string cas;
        public UInt16 priorita;
    }

    /// <summary>
    /// Class pre pouzitie vo WPF aplikacii
    /// </summary>
    public sealed class SupravaDataST
    {
        public UInt16 CisloSupravy { get; set; }
        public string Kolaj { get; set; }
        public string Datum { get; set; }
        public string Cas { get; set; }
        public UInt16 Priorita { get; set; }
    }
    #endregion === Obsadenie kolaje====


}
