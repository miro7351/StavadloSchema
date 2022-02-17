using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Windows.Input;
using System.Windows.Resources;
using Stavadlo22.Data;

namespace Stavadlo22.Infrastructure
{

    ///// <summary>
    ///// delegat pre odpoved servera; server vrati data pre vseky prvky stavadla
    ///// </summary>
    ///// <param name="sender"></param>
    ///// <param name="e"></param>
    //public delegate void MessageDelegate(object sender, EventArgs e);


    #region Documentation
    /// <summary
    /// <remarks>
    /// <para>
    /// Class Info: obsahuje funkcie, ktore sa volaju z roznych Window, alebo Page
    /// <list type="bullet">
    /// <item name="author">Author: RNDr. M. Hrabcak, CSc.</item>
    /// <item name="date">Juny 2013</item>
    /// <item name="email">hrabcak@procaut.sk</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// </summary>
    #endregion
    public class StavadloFactory
    {
        public StavadloGlobalData GlobalData;

        private static readonly StavadloFactory _instance = new StavadloFactory();

        public static StavadloFactory Instance => _instance;
       
        private StavadloFactory()//place for instance initialization code
        {
            //inicializacia v App.xaml.cs OnStart(), ak sa to robi tu potom vznika exception
            /*
               StavadloFactory stavadloFactory = StavadloFactory.Instance;
                stavadloFactory.GlobalData = GlobalData;
             */
        }


        /// <summary>
        /// nastavi kurzor pre aplikaciu
        /// </summary>
        /// <returns></returns>
        public Cursor GetApplicationCursor()
        {
            Uri uri = new Uri("pack://application:,,,/Images/CursorIcons/kurzorYellowXs.ico");
            Stream iconstream = GetCURFromICO(uri, 2, 2);
            return new Cursor(iconstream);
            // Mouse.OverrideCursor = cursor;--toto sa nastavi v App.xaml.cs
        }

        //KT: pridane 22.11.2013, 
        /// <summary>
        /// nastavi kurzor pre aplikaciu podla uri
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="hotspotx">nastavenie spicky-X pre kurzor</param>
        /// <param name="hotspoty">nastavenie spicky-Y pre kurzor</param>
        /// <returns></returns>
        Stream GetCURFromICO(Uri uri, byte hotspotx, byte hotspoty)
        {
            StreamResourceInfo sri = System.Windows.Application.GetResourceStream(uri);
            Stream s = sri.Stream;
            byte[] buffer = new byte[s.Length];

            MemoryStream ms = new MemoryStream();
            ms.WriteByte(0); // always 0
            ms.WriteByte(0);
            ms.WriteByte(2); // change file type to CUR
            ms.WriteByte(0);
            ms.WriteByte(1); // 1 icon in table
            ms.WriteByte(0);

            s.Position = 6; // skip over first 6 bytes in ICO as we just wrote
            s.Read(buffer, 0, 4);
            ms.Write(buffer, 0, 4);

            ms.WriteByte(hotspotx);
            ms.WriteByte(0);

            ms.WriteByte(hotspoty);
            ms.WriteByte(0);

            s.Position += 4; // skip 4 bytes as we just wrote our own

            int remaining = (int)s.Length - 14;

            s.Read(buffer, 0, remaining);
            ms.Write(buffer, 0, remaining);
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Nastavi  resource App.Current.Resources["visibilityRozvadzace"]; riadi Visibility pre  <Style x:Key="txbRozvadzacStyle".../>
        /// Zviditelni/skryje rozvadzace a popisy
        /// </summary>
        public void NastavFlagRozvadzace()
        {
            System.Windows.Visibility res = GlobalData.RozvadzacVisibility;// (System.Windows.Visibility)App.Current.TryFindResource("visibilityRozvadzace");
            //treba zapis do zurnalu???
            if (res == System.Windows.Visibility.Hidden)
            {
                App.Current.Resources["visibilityRozvadzace"] = System.Windows.Visibility.Visible;
                GlobalData.RozvadzacVisibility = System.Windows.Visibility.Visible;
            }
            else
            {
                App.Current.Resources["visibilityRozvadzace"] =System.Windows.Visibility.Hidden;
                GlobalData.RozvadzacVisibility = System.Windows.Visibility.Hidden;
            }
        }


        //MH: otestovane 15.5.2013 pre Win-7 a Win XP, vsetko OK!!!!!
        /// <summary>
        /// vrati IP adresu pocitaca, kde je spusteny program
        /// </summary>
        /// <returns></returns>
        public static string GetIPaddres()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            var ipaddress = ipEntry.AddressList;
            var q = from a in ipaddress
                    where a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                    select a;
            return q.Last().ToString();
        }


        /// <summary>
        /// zo vstupneho stringu vrati nazov cesty napr. K603T-K421a
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        //public static string GetFullPathName(string inputString)
        //{
        //    string regexString = @"K\d{3}[a-zA-Z]{0,1}[-]K\d{3}[a-zA-Z]{0,1}";
        //    Regex r = new Regex(regexString);
        //    MatchCollection matches = r.Matches(inputString);
        //    if (matches.Count > 0)
        //        return matches[0].ToString();
        //    return string.Empty;
        //}

    }//class StavadloFactory
}
