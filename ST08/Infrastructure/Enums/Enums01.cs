
namespace PA.Stavadlo.Infrastructure.Enums
{
    /// <summary>
    /// mod v akom pracuje program; Parkanyi: 21.08.2013 typ aplikacie moze byt len M-Master, R-ReadOnly;
    /// master vysiela, prijima telegramy pre vsetky cinnosti;
    /// udrzba vysiela, prijima telegramy urcene len pre udrzbu
    /// readonly nevysiela telegramy, len prijima telegramy a sleduje nastavenie prvkov stavadla
    /// </summary>
    public enum APPLICATION_MODE
    {
        NONE,
        READONLY,
        // UDRZBA,
        // SERVIS,
        MASTER
    }

    /// <summary>
    /// stavy pre branu 
    /// </summary>
    //public enum GATE_STATE
    //{
    //    NONE,
    //    OPEN,
    //    CLOSED
    //}


    /// <summary>
    /// mod vypisu textu v lavom alebo pravom spodnom rohu displeja
    /// </summary>
    public enum INFO_MODE
    {
        NONE,
        INFO,
        ERROR,
        EXCEPTION
    }

    /// <summary>
    /// mod pre signalizacny buton
    /// </summary>
    public enum BUTTON_MODE
    {
        NONE,
        PRESSED,
        RELEASED
    }
    /// <summary>
    /// mod-rezim vybraty z menu aplikacie; cinnost ktora a bude robit
    /// </summary>
    public enum CURRENT_MENU_MODE
    {
        NONE = 0,
        VYMENA_V_MANIPULACII = 6,//MH:povodne tu nebola
        STAVANIE_CESTY = 7,//MH:povodne tu nebola

        ZRUSENIE_PORUCHY = 8,    //original: ZrusPor
        UPLNA_VYLUKA = 9,        //original: UplVyl
        VYLUKA_PRESTAVENIA = 10, //original:VylPres
        UVOLNENIE_IZOLACIE = 11, //original:UvolIzol

        VYMENA = 12,          //original: Vymena; Prestavenie vymeny-vyhybky, moze sa manipulovat s vyhybkou
        ZRUSENIE_CESTY = 15,  //original: ZrusV

        VYLUKA_NAVESTIDLA = 16, //original:VylNav

        SIMULACIA_OBSADENIA = 17,                  //original:SimObs
        SIMULACIA_PORUCHY_STRATA_DOHLIADANIA = 18, //original:SimPor0
        SIMULACIA_PORUCHY_PRESTAVENIA = 19,        //original:SimPor1
        SIMULACIA_PORUCHY_ROZREZ = 20,             //original:SimPorA
        SIMULACIA_PORUCHY_NADPRUD = 21,            //original:SimPor2A

        POSUN_CESTA_ZACIATOK = 22, //original:JazCpz
        POSUN_CESTA_KONIEC = 23, //original:JazCpk
        POSUN_CESTA_VARIANT = 24, //original:JazCpv

        VLAK_CESTA_ZACIATOK = 25, //original:JazCvz
        VLAK_CESTA_KONIEC = 26, //original:JazCvk
        VLAK_CESTA_VARIANT = 27, //original:JazCvv

        SIMULACIA_PORUCHY_NAVESTIDLA_PORUCHA_CERVENEJ = 28, //original:SimNav0
        SIMULACIA_PORUCHY_NAVESTIDLA_POVOLOVACEJ_NAVESTI = 29,    //original:SimNav10

        INFORMACIA_O_PRVKU = 30,  //original: InfoP  

        VYLUKA_POSUNOVEJ_CESTY = 131, //MH pridane
        VYLUKA_POSUN_CESTY_ZACIATOK = 31, //original: VylCpz
        VYLUKA_POSUN_CESTY_KONIEC = 32, //original: VylCpk
        VYLUKA_POSUN_CESTY_VARIANT = 33, //original: VylCpv

        VYLUKA_VLAKOVEJ_CESTY = 134, //MH pridane
        VYLUKA_VLAK_CESTY_ZACIATOK = 34, //original: VylCvz
        VYLUKA_VLAK_CESTY_KONIEC = 35, //original: VylCvkz
        VYLUKA_VLAK_CESTY_VARIANT = 36, //original: VylCvv

        OZNACENIE_OBSADENIA = 37,  //original: OznObs
        ZRUSENIE_RIADENIA_ROZPUSTANIA = 38, //original: SP1

        //ZIADOST_O_SUHLAS_PRE_VLAK_CESTU = 39, //original: ZiadOsuh
        ZIADOST_O_SUHLAS = 39,
        UDELENIE_SUHLASU = 40,    //original: UdelSuh
        RUSENIE_SUHLASU = 41,    //RusSuh

        SIMULACIA_ZIADOSTI_O_SUHLAS = 42, //original: SimZsuh
        SIMULACIA_UDELENIA_SUHLASU = 43, //original: SimUsuh
        SIMULACIA_RUSENIA_SUHLASU = 44, //original: SimRsuh

        // INFORMACIA_O_ZARIADENI = 46,   //original: EditInf
        ZIADOST_O_SUHLAS_PRE_POSUNOVU_CESTU = 47, //original: ZiadOsuh1
        ZIADOST_O_SUHLAS_PRE_OBSLUHU_V801 = 48, //original: ZiadOsuh2

        BRANU_OTVORIT = 49, //original: GateO
        BRANU_ZAVRIET = 50, //original: GateC
        GATE_LOCAL_CONTROL_ENABLE,
        GATE_LOCAL_CONTROL_DISABLE,
        SIMULACIA_NAVESTIDLA,
        SIMULACIA_PORUCHY,
        SIMULACIA_SIGNALU,
        SIMULACIA_SIGNALU_RZV,
        SIMULACIA_SIGNALU_VPP,
        SIMULACIA_SIGNALU_SPR,
        SIMULACIA_SIGNALU_AUT,
        SIMULACIA_SIGNALU_DENNOC,
        SIMULACIA_SIGNALU_VR,


        //MH: tieto som pridal
        PRIHLASENIE_A_ODHLASENIE = 100,
        ZIADOST_O_SUHLAS_PRE_VLAKOVU_CESTU = 101,

        INFORMACIE_O_SYMBOLOCH,
        INFORMACIE_POMOCNE,
        INFORMACIE_O_POM_SIGNALOCH,
        INFORMACIE_O_SUHLASOCH,
        INFORMACIE_O_ZARIADENI,//pre zobrazenie suboru s informaciami a zariadeni, neposiela sa nijaky telegram
        NASTAVENIE_ZVUK,
        NASTAVENIE_POZADIA,

        VCESTA = 200,
        PCESTA = 201,
        ZRUS = 202,
        STOP,
        ZRUSENIE_VARIANTU_CESTY,
        PRIVOLAVACIA_NAVEST,
        VYBAVENIE_PORUCH,
        ZAPADKOVA_SKUSKA,
        ROZVADZACE,
        SKRYVANIE_PANELU,
        KOMUNIKACNE_PARAMETRE,
        O_APLIKACII,
        UKONCENIE_APLIKACIE
    }//public enum CURRENT_MODE


    /// <summary>
    /// typ pre rolu uzivatela
    /// </summary>
    public enum USER_ROLE
    {
        NONE,
        ADMIN,
        DISPECER,
        UDRZBA,
        READONLY
    }

    /// <summary>
    /// Hodnota pre level mode, int hodnota sa pouziva v telegrame vysielanom do Servera
    /// </summary>
    public enum USER_LEVEL_MODE
    {
        DISPECER_LEVEL = '0',
        UDRZBA_LEVEL = '1',
        ADMIN_LEVEL = '2',
        NONE ='x'
    }

    public enum User_DBlogger
    {
        ST22D,
        ST22U,
        ST22A
    }


    /// <summary>
    /// typ usekov pri stavani/vyluke cesty
    /// </summary>
    public enum PATH_CONSTRUCTION_STATE
    {
        NONE,
        READY,  //MH 09.10.2013 operacia pre stavanie/vyluku cesty bola dokoncena
        START,
        END,
        VARIANT //je znamy zaciatok cesty, koniec cesty, ale je viacej moznosti pre vytvorenie cesty, preto program ziada operatora
                //o zadanie usekov (operator klikne na usek), ktorymi ma cesta prechadzat
    }

    //--MH: pridane september 2018--
    /// <summary>
    /// stavy pre sipku znazornujucu vstup a odchod ak blikanie sa riadi DataTrigerom a animaciou
    /// </summary>
    public enum ARROW_SYMBOL_MODE
    {
        INVISIBLE = 0,
        ZIADOST_VCHOD,  //blika Transparent<->Yellow kazdu sekundu
        SUHLAS_VCHOD,   //zlty trvalo svieti
        ZIADOST_ODCHOD, //blika Transparent<->Green kazdu sekundu
        SUHLAS_ODCHOD,  //zeleny trvalo svieti
        NONE
    }

    /// <summary>
    /// stavy pre sipku znazornujucu vstup alebo odchod. 
    /// </summary>
    public enum ARROW_MODE
    {
        ZIADOST_VSTUP,  //blika Transparent<->Yellow kazdu sekundu
        SUHLAS_VSTUP,   //zlty trvalo svieti
        ZIADOST_ODCHOD, //blika Transparent<->Green kazdu sekundu
        SUHLAS_ODCHOD,  //zeleny trvalo svieti
        NONE
    }

    /// <summary>
    /// Typ user controlu ktory znazornuje suhlas pre vstup alebo odchod z kolajoveho useku
    /// </summary>
    public enum TYP_SUHLASU
    {
        ZIADOST_VSTUP,  //blika Transparent<->Yellow kazdu sekundu
        SUHLAS_VSTUP,   //zlty trvalo svieti
        ZIADOST_ODCHOD, //blika Transparent<->Green kazdu sekundu
        SUHLAS_ODCHOD,  //zeleny trvalo svieti
        NONE
    }
    //-------------------------------
    public enum PLAY_MESSAGE_FILTER
    {
        ALL,
        LOGIN_LOGOUT,
        START_END,
        ERROR
    }

    public enum PLAYING_MODE
    {
        STOP,       //zastavi prehravanie a hodi sa na zaciatok citaneho suboru
        STEP_LEFT,  //nacita predchadzajuci telegram
        STEP_RIGHT, //nacita nasledujuci telegram
        PLAY,       //postupne prehravanie telegramov
        PAUSE       //poyastavenie prehravanie
    }

    /// <summary>
    /// Typ zobrazenia pre UC_ObsadenieKolaje
    /// </summary>
    public enum OBSADENIE_TYPE
    {
        R1, //zobrazenie vsetkych prvkov v jednom riadku: cislo kolaje, zoznam suprav, button close
        R2  //zobrazenie  prvkov v dvoch riadkoch: cislo kolaje,  button close hodny riadok, zoznam suprav dolny riadok
    }

}
