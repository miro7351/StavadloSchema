using System;
using System.Text;
using Stavadlo22.Infrastructure.Enums;

namespace Stavadlo22.Infrastructure
{
    #region Documentation
    /// <summary
    /// <remarks>
    /// <para>
    /// Class Info: metody na nacitanie parametrov z config suboru Stavadlo22.exe.config
    /// <list type="bullet">
    /// <item name="author">Author: RNDr. M. Hrabcak, CSc.</item>
    /// <item name="date">June 2013</item>
    /// <item name="email">hrabcak@procaut.sk</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// </summary>
    #endregion
    class ConfigParam
    {

        public static bool GetUseDBLogger()
        {
            // Defaultne sa bude pouzivat rozsirena mapa, pritom dane nastavenie z konfigu sa odstrani
            bool result = true;
            if (null != Stavadlo22.Properties.Settings.Default)
            {
                // Zisk intervalu pre kontrolu spojenija so serverom z konfiguracneho suboru. V konfig subore je hodnota v sekundach.
                result = Stavadlo22.Properties.Settings.Default.UseDBLogger;
            }

            return result;
        }

        public static string GetStavadloCast()
        {
            string stavadloType = "ST22";  // default hodnota
            if (null != Properties.Settings.Default)
            {
                stavadloType = Properties.Settings.Default.StavadloCast;
            }

            return stavadloType;
        }

        public static string GetWebServiceAddress()
        {
            string result = "";  // default hodnota
            if (null != Properties.Settings.Default)
            {
                result = Properties.Settings.Default.dbServer;
            }

            return result;
        }

        public static string GetWebServiceLogin()
        {
            string result = "";  // default hodnota
            if (null != Properties.Settings.Default)
            {
                result = Properties.Settings.Default.dbServerUser;
            }

            return result;
        }

        //public static bool GetScrollViewerFlag()
        //{
        //    bool result =false;  // default hodnota
        //    if (null != Properties.Settings.Default)
        //    {
        //        result = Properties.Settings.Default.UseScrollViewer;
        //    }

        //    return result;
        //}

        public static string GetWebServicePassword()
        {
            string result = "";  // default hodnota
            if (null != Properties.Settings.Default)
            {
                result = Properties.Settings.Default.dbServerPass;
                // dekodovanie z base 64
                try
                {
                    byte[] data = Convert.FromBase64String(result);
                    string decodedString = Encoding.UTF8.GetString(data);
                    result = decodedString;
                }
                catch (Exception)
                {
                    // Todo odchytit
                    throw new Exception("Error in encoded webService password!");
                }
            }
            return result;
        }

        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /*
         * POZNAMKY NACITANIE POLOZIEK Z CONFIG SUBORU: ak sa da kurzor na polozku a stlaci sa F12 dostaneme definovanie polozky napr. applicationMode
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("readonly")]
        public string applicationMode {
            get {
                return ((string)(this["applicationMode"]));
            }
        }
         * Tu je zadefinovana default hodnota!!!
         * Ak v config subore sa zamerne vymaze polozka, potom VS zoberie tuto default hodnotu!!!
         * Tato default hodnota je posledna hodnota z config. suboru zadana pred kompilaciou programu!!!
         * 
        */
        /// <summary>
        /// nacita mod programu polozka applicationMode z config. suboru; MASTER/READONLY;
        /// ak tam nie je polozka applicationMode, vrati READONLY mod
        /// </summary>
        /// <returns></returns>
        public static APPLICATION_MODE GetApplicationMode()
        {
            APPLICATION_MODE appMode = APPLICATION_MODE.READONLY;
            if (null != Stavadlo22.Properties.Settings.Default)
            {
                string s1 = Stavadlo22.Properties.Settings.Default.applicationMode;
                //pozri Settings.Designer.cs
                //Designer obsahuje default hodnotu!!! je to naposledy zadana hodnota v Settings.setitings!!!!!!!
                if (!string.IsNullOrEmpty(s1))
                    appMode = (APPLICATION_MODE)Enum.Parse(typeof(APPLICATION_MODE), s1.ToUpper());
                else
                    appMode = APPLICATION_MODE.READONLY;
            }
            return appMode;
        }//GetApplicationMode()



        /// <summary>
        /// z config suboru nacita prefered role;
        /// Ak uzivatel ma viacej roli v RoleManageri, napr: D|U|R, potom mozeme nastavit rolu, ktoru bude pouzivat;
        /// Default je "R" readOnly rola
        /// </summary>
        /// <returns></returns>
        public static string GetPreferedRole()
        {
            string pRole = "R"; //default value

            if (null != Properties.Settings.Default)
            {
                //ak pouzijem tento kod, potom po skompilovani vymazem z config suboru polozku prefered
                //Stavadlo22.Properties.Settings.Default.prefered vrati naposledy zadanu polozku v Settings.settings subore
                pRole = Properties.Settings.Default.prefered;
                if (String.IsNullOrEmpty(pRole))
                    pRole = "R";
            }
            return pRole;
        }

        /// <summary>
        /// nacita mod programu polozka SimulationMode z config. suboru;
        /// ak tam nie je polozka SimulationMode, vrati false</summary>
        /// <returns></returns>
        public static bool GetSimulationMode()
        {
            bool result = false;
            if (null != Stavadlo22.Properties.Settings.Default)
                result = Stavadlo22.Properties.Settings.Default.simulationMode;
            return result;
        }

        /// <summary>
        /// nacita path pre subory zurnalov
        /// </summary>
        /// <returns></returns>
        public static string GetJournalFilePath()
        {
            string path = String.Empty;
            if (null != Stavadlo22.Properties.Settings.Default)
            {
                path = Stavadlo22.Properties.Settings.Default.journalFiles;
                if (string.IsNullOrEmpty(path))
                    path = "st17/desktop/zabzar_report.php?link=50";
            }
            return path;
        }


        /// <summary>
        /// nacita path pre errorove subory
        /// </summary>
        /// <returns></returns>
        public static string GetErrorFilePath()
        {
            string path = String.Empty;
            if (null != Stavadlo22.Properties.Settings.Default)
            {
                path = Stavadlo22.Properties.Settings.Default.errorFiles;
                if (string.IsNullOrEmpty(path))
                    path = "st17/desktop/zabzar_report.php?link=51";
            }
            return path;
        }

        /// <summary>
        /// nacita path pre subory so systemovymi chybami
        /// </summary>
        /// <returns></returns>
        public static string GetSysErrorFilePath()
        {
            string path = String.Empty;
            if (null != Stavadlo22.Properties.Settings.Default)
            {
                path = Stavadlo22.Properties.Settings.Default.sysErrorFiles;
                if (string.IsNullOrEmpty(path))
                    path = "st22/desktop/zabzar_report.php?link=52";
            }
            return path;
        }


        public static string GetInfoFilePath()
        {
            if (null != Stavadlo22.Properties.Settings.Default)
                return Stavadlo22.Properties.Settings.Default.infoFile;
            else
                return string.Empty;
        }

        //vrati nazov klienta
        /// <summary>
        /// nacita nazov klienta z config suboru
        /// </summary>
        /// <returns>ST22</returns>
        public static string GetClientName()
        {
            string cName = "ST22";
            if (null != Properties.Settings.Default)
            {
                cName = Properties.Settings.Default.clientName;
                if (string.IsNullOrEmpty(cName))
                    cName = "ST22";
            }
            return cName;
        }

        /// <summary>
        /// nacita flag logicDisconnected z config suboru
        /// </summary>
        /// <returns></returns>
        public static bool GetLogicFlag()
        {
            bool result = false;
            if (null != Properties.Settings.Default)
            {
                result = Properties.Settings.Default.logic22Disconnected;
            }
            return result;
        }

        /// <summary>
        /// nacita flag Log4NetIsEnabled z config suboru
        /// </summary>
        /// <returns></returns>
        public static bool GetLog4NetFlag()
        {
            bool result = true;
            if (null != Properties.Settings.Default)
            {
                result = Properties.Settings.Default.Log4NetIsEnabled;
            }
            return result;
        }

        /// <summary>
        /// nacita frekvenciu zvuku pre pipanie
        /// </summary>
        /// <returns></returns>
        public static int GetBeepHz()
        {
            int result = 500;
            if (null != Properties.Settings.Default)
            {
                result = Properties.Settings.Default.BeepHz;
            }
            return result;
        }

        /// <summary>
        /// nacita dlzku zvukoveho signalu pre pipanie v milisekundach 
        /// </summary>
        /// <returns></returns>
        public static int GetBeepTime()
        {
            int result = 500;
            if (null != Stavadlo22.Properties.Settings.Default)
            {
                result = Stavadlo22.Properties.Settings.Default.BeepTime;
            }
            return result;
        }

        /// <summary>
        /// nacitanie priznaku ci sa aplikacia spusta v PlayMode, 
        /// PlayMode sa moze spustit len pre Master aplikaciu
        /// </summary>
        /// <returns></returns>
        public static bool GetPlayMode()
        {
            bool result = false;
            APPLICATION_MODE appMode = APPLICATION_MODE.READONLY;
            appMode = GetApplicationMode();
            if (appMode == APPLICATION_MODE.MASTER)
            {
                if (null != Properties.Settings.Default)
                    result = Properties.Settings.Default.playMode;//pri zmene nazvu propety v config subore sa automaticky zmeni nazov v cs subore
            }
            else
            {
                return false;
            }
            return result;
        }


        /// <summary>
        /// nacita PlayMode status z config suboru alebo z bat suboru;
        /// bat subor ma vyssiu prioritu;
        /// aplikacia musi byt typu master;
        /// </summary>
        /// <param name="dataString"></param>
        /// <returns></returns>
        public static bool GetPlayMode(string[] dataString)
        {
            bool result = false;
            if (dataString.Length > 0)//ak sa Play mode nastavuje v bat.subore
            {
                for (int i = 0; i < dataString.Length; i++)
                    if (dataString[i].ToUpper() == "/P")
                    {
                        if (APPLICATION_MODE.MASTER == ConfigParam.GetApplicationMode())//PlayMode sa moze spustit len v Master aplikacii
                            return true;
                    }
            }
            else //ak sa PlayMode nastavuje v config.subore
            {
                result = ConfigParam.GetPlayMode();
            }
            return result;
        }

        public static string GetRolaManagerName()
        {
            return Properties.Settings.Default.roleManager; //RolMan22
        }

        public static string GetJournalServerIP()
        {
            return Properties.Settings.Default.JournalServerIP;
        }
    }//class ConfigParam
}
