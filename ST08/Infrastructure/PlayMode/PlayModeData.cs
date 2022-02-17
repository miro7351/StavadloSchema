using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Stavadlo22.Infrastructure.PlayMode
{
    //Obsahuje objekty pre ulozenie/ziskanie polohy curzora, ulozenie udajov pri kliku na prvok stavadla a ulozenie udajov pri kliku na menu

    /// <summary>
    /// Udaje pre odlozenie polohy kurzora mysi pri lavom kliku
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class MousePositionData
    {

        public string dateTime;  //vo formate dd.MM.yyyy HH:mm:ss.FFF
        public double windowTop; //poloha laveho horneho rohu okna
        public double windowLeft;
        public double primaryScreenWidth; //rozmery primarneho displeja napr. 1920x1080
        public double primaryScreenHeight;
        public double windowActualWidth;//aktualy rozmer pre window
        public double windowActualHeight;
        public double mouseX;   //X poloha pre kurzor
        public double mouseY;   //Y poloha pre kurzor

        /// <summary>
        /// nastavuje sa pri mainWindow_SizeChanged evente hl. okna
        /// nastavi parametre windowTop, windowLeft
        /// </summary>
        /// <param name="top"></param>
        /// <param name="left"></param>
        public void SetWindowTopLeft(double top, double left)
        {
            windowTop = top;
            windowLeft = left;
        }

        /// <summary>
        /// nastavuje sa pri spusteni aplkacie
        /// </summary>
        /// <param name="screenWidth"></param>
        /// <param name="screenHeight"></param>
        public void SetScreenSize(double screenWidth, double screenHeight)
        {
            primaryScreenWidth = screenWidth;
            primaryScreenHeight = screenHeight;
        }

        /// <summary>
        /// nastavi primaryScreen width a height
        /// </summary>
        public void SetScreenSize()
        {
            //pre DELL 7539 ktory ma display: Height=1030, Width=1920
            //primaryScreenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;  vzdy PrimaryScreenWidth=1536
            //primaryScreenHeight = System.Windows.SystemParameters.PrimaryScreenHeight; vzdy PrimaryScreenHeight=864
            //MH: zmena 23.04.2019
            primaryScreenWidth = Screen.PrimaryScreen.WorkingArea.Width;//pre DELL 7530:  Height=1030, Width=1920
            primaryScreenHeight = Screen.PrimaryScreen.WorkingArea.Height;
        }

        /// <summary>
        /// nastavuje sa pri mainWindow_SizeChanged evente hl. okna
        /// </summary>
        /// <param name="actualWidth"></param>
        /// <param name="actualHeight"></param>
        public void SetWindowSize(double actualWidth, double actualHeight)
        {
            windowActualWidth = actualWidth;
            windowActualHeight = actualHeight;
        }


        /// <summary>
        /// pouziva sa v normalnom mode, ked ukladame polohu kurzora;
        /// pouziva sa pri lavom kliku
        /// </summary>
        /// <param name="mousePosition"></param>
        public void SetMousePosition(System.Windows.Point mousePosition)
        {
            mouseX = mousePosition.X;
            mouseY = mousePosition.Y;
            dateTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.FFF");
        }

        /// <summary>
        /// pouziva sa v PlayMode ak chceme pohybovat kurzorom pomocou  udajov z logov
        /// </summary>
        /// <returns></returns>
        //public System.Drawing.Point GetMousePosition()
        public System.Windows.Point GetMousePosition()
        {
            //return new Point((int)windowLeft + (int)mouseX, (int)windowTop + (int)mouseY);
            return new System.Windows.Point( windowLeft + mouseX, windowTop + mouseY);
        }
        //POZN: ak monitor kde sa generuju log subory a monitor kde sa prehravaju log udaje nemaju rovnake rozmery alebo rozlisenie,
        //potom treba upravit pomocou prevodnych koeficientov new Point(....)

        public override string ToString()
        {
            return $"{dateTime};{windowTop};{windowLeft};{primaryScreenWidth};{primaryScreenHeight};{windowActualWidth};{windowActualHeight};{mouseX};{mouseY}";
        }

        /// <summary>
        /// Rozparsuje string a vytvori instanciu typu <see cref="MousePositionData"/>
        /// </summary>
        /// <param name="dataString"></param>
        /// <returns>MousePositionData</returns>
        public static MousePositionData FromString(string dataString)
        {
            MousePositionData newMousePosData = null;
            if (string.IsNullOrEmpty(dataString))
                return null;

            string[] d = dataString.Split(';');
            if (d.Length == 9)
            {
                try
                {
                    newMousePosData.dateTime = d[0];
                    newMousePosData.windowTop = Convert.ToDouble(d[1]);
                    newMousePosData.windowLeft = Convert.ToDouble(d[2]);
                    newMousePosData.primaryScreenWidth = Convert.ToDouble(d[3]);
                    newMousePosData.primaryScreenHeight = Convert.ToDouble(d[4]);

                    newMousePosData.windowActualWidth = Convert.ToDouble(d[5]);
                    newMousePosData.windowActualHeight = Convert.ToDouble(d[6]);
                    newMousePosData.mouseX = Convert.ToDouble(d[7]);
                    newMousePosData.mouseY = Convert.ToDouble(d[8]);
                }
                catch (Exception)
                {
                    throw new Exception("Chyba pri konverzii stringu na MousePositionData");
                }
            }//if
            return newMousePosData;
        }//MousePositionData FromString

    }//class MousePositionData


    /// <summary>
    /// obsahuje polohu kurzora mysi a vybratu polozku z Menu (jej CURRENT_MODE v stringovej podobe), ak sa vybrala polozka z menu pomocou mysi
    /// </summary>
    public class MenuData
    {
        public MousePositionData mousePosition;
        public string selectedMode;

        public MenuData() { }

        public MenuData(MousePositionData mousePosition, string selectedMode)
        {
            this.mousePosition = mousePosition;
            this.selectedMode = selectedMode;
        }
        //pouziva sa pri zapise do log4Net, ako stringova forma pre class
        public override string ToString()
        {
            // string s1 = mousePosition.ToString() + ";" + selectedMode;
            return mousePosition.ToString() + ";" + selectedMode;
        }

        //zaznam z log suboru
        //0011 23.04.2019 08:31:05,043  <<HMI-MENU>>|23.04.2019 08:31:05.043;0;0;1536;864;0;0;463;150;PCESTA
        public static MenuData FromString(string dataString)
        {
            MenuData newMenuData = new MenuData();
            MousePositionData newMousePosData = new MousePositionData();
            if (string.IsNullOrEmpty(dataString))
                return null;

            string[] d = dataString.Split(';');
            if (d.Length == 10)
            {
                try
                {
                    newMousePosData.dateTime = d[0];
                    newMousePosData.windowTop = Convert.ToDouble(d[1]);
                    newMousePosData.windowLeft = Convert.ToDouble(d[2]);
                    newMousePosData.primaryScreenWidth = Convert.ToDouble(d[3]);
                    newMousePosData.primaryScreenHeight = Convert.ToDouble(d[4]);

                    newMousePosData.windowActualWidth = Convert.ToDouble(d[5]);
                    newMousePosData.windowActualHeight = Convert.ToDouble(d[6]);
                    newMousePosData.mouseX = Convert.ToDouble(d[7]);
                    newMousePosData.mouseY = Convert.ToDouble(d[8]);

                    newMenuData.mousePosition = newMousePosData;
                    newMenuData.selectedMode = d[9];
                }
                catch (Exception)
                {
                    throw new Exception("Chyba pri konverzii stringu na MenuData");
                }
            }//if (d.Length == 10)
            return newMenuData;
        }//MenuData FromString

    }//class MenuData


    /*
      ZAPIS PRI KLIKU NA PRVOK STAVADLA
        /// <summary>
        /// zapise udaje o polohe kurzora a vybratej polozke z menu
        /// </summary>
        /// <param name="selMode"></param>
        private void WriteMousePositionData(CURRENT_MENU_MODE selMode)
        {
            //poloha kurzora vzhladom na MainWindow
            System.Windows.Point mp = Mouse.GetPosition(Application.Current.MainWindow);
            GlobalData.AppMousePositionData.SetMousePosition(mp);

            GlobalData.AppMenuData.mousePosition = GlobalData.AppMousePositionData;
            GlobalData.AppMenuData.selectedMode = selMode.ToString();
            log?.CustomInfo($"{GlobalData.LogHeaders["MENU"]}{ GlobalData.AppMenuData.ToString()}");
        }
     */

    /// <summary>
    /// obsahuje polohu kurzora mysi a element Name na ktory sa kliklo
    /// </summary>
    public class MouseClickData
    {
        public MousePositionData mousePosition;
        public string clickedElementName;
        public string selectedMode;

        public MouseClickData()
        {

        }

        public MouseClickData(MousePositionData mousePosition, string elementName, string selectedMode)
        {
            this.mousePosition = mousePosition;
            this.clickedElementName = elementName;
            this.selectedMode = selectedMode;
        }
        //pouziva sa pri zapise do log4Net, ako stringova forma pre class
        public override string ToString()
        {
            //return base.ToString();
            //string s1 = mousePosition.ToString() + ";" + clickedElementName + ";" + selectedMode;
            return string.Format("{0};{1};{2}", mousePosition.ToString(), clickedElementName, selectedMode);
        }

        // System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)mainWindow.Left + (int)polohaCursora.X, (int)mainWindow.Top + (int)polohaCursora.Y);
        // System.Windows.Forms.Cursor.Position = MouseClickData.GetMousePosition();
        /// <summary>
        /// vrati polohu kurzora z udajov v log subore
        /// </summary>
        /// <returns></returns>
        //public System.Drawing.Point GetMousePosition()
        //{
        //    return mousePosition.GetMousePosition();
        //}

        public System.Windows.Point GetMousePosition()
        {
            return mousePosition.GetMousePosition();
        }

        public static MouseClickData FromString(String dataString)
        {
            MouseClickData newMouseClickData = new MouseClickData();
            MousePositionData newMousePosData = new MousePositionData();
            if (string.IsNullOrEmpty(dataString))
                return null;

            string[] d = dataString.Split(';');
            if (d.Length == 11)
            {
                try
                {
                    newMousePosData.dateTime = d[0];
                    newMousePosData.windowTop = Convert.ToDouble(d[1]);
                    newMousePosData.windowLeft = Convert.ToDouble(d[2]);
                    newMousePosData.primaryScreenWidth = Convert.ToDouble(d[3]);
                    newMousePosData.primaryScreenHeight = Convert.ToDouble(d[4]);

                    newMousePosData.windowActualWidth = Convert.ToDouble(d[5]);
                    newMousePosData.windowActualHeight = Convert.ToDouble(d[6]);
                    newMousePosData.mouseX = Convert.ToDouble(d[7]);
                    newMousePosData.mouseY = Convert.ToDouble(d[8]);

                    newMouseClickData.mousePosition = newMousePosData;
                    newMouseClickData.clickedElementName = d[9];
                    newMouseClickData.selectedMode = d[10];
                }
                catch (Exception)
                {
                    throw new Exception("Chyba pri konverzii stringu na MousePositionData");
                }
            }
            return newMouseClickData;
        }//MouseClickData FromString
    }//MouseClickData


    /// <summary>
    /// obsahuje informacie o prvku, ktory sa da presuvat pomocou mysi: Vykolajka, PanakUSS, Lokomotiva
    /// </summary>
    public class MoveableData
    {
        public string Name;     //meno elementu ktory sa presuval
        public double X;           //x-ova suradnica posunu
        public double Y;           //y-ova suradnica posunu

        /// <summary>
        /// rozparsuje datastring a vytvori instanciu typ <see cref="MoveableData"/>
        /// </summary>
        /// <param name="dataString"></param>
        /// <returns>MoveableData</returns>
        public static MoveableData FromString(string dataString)
        {
            MoveableData obj = new MoveableData();
            string[] udaje;
            if (dataString == null)
                return null;
            udaje = dataString.Split(';');
            if (udaje.Length != 3)
                return null;
            obj.Name = udaje[2];
            obj.X = Convert.ToDouble(udaje[0]);
            obj.Y = Convert.ToDouble(udaje[1]);

            return obj;
        }
    }
}
