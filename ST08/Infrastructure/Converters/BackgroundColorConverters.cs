using ST08;
using System;
using System.Collections;
using System.Windows.Data;
using System.Windows.Media;

namespace PA.Stavadlo.MH.Infrastructure.Converters
{
    /// <summary>
    /// Nastavuje farbu pozadia textbloku koncovym castiam vlakovej cesty ak je dana cesta nastavena ako obsadeny usek
    /// inak robi pozadie priehladnym
    /// </summary>
    public class BackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
           // if (!(value is PATH_MODE))
           //     return null;

            SolidColorBrush TransparentBrush = App.Current.Resources["TransparentBrush"] as SolidColorBrush;
            SolidColorBrush obsadenyUsekBrush = App.Current.Resources["obsadenyUsekBrush"] as SolidColorBrush;

            //if (value == null || targetType == null || obsadenyUsekBrush == null)
            //    return App.Current.Resources["TransparentBrush"] as SolidColorBrush;

            //Brush toSet = App.Current.Resources["obsadenyUsekBrush"] as SolidColorBrush;

            //if (value.ToString() == toSet.ToString())
            //{
            //    return App.Current.Resources["obsadenyUsekBrush"] as SolidColorBrush;
            //}

            if (value == null || targetType == null || obsadenyUsekBrush == null)
                return App.Current.Resources["TransparentBrush"] as SolidColorBrush;



            if (value.ToString() == obsadenyUsekBrush.ToString())
            {
                return obsadenyUsekBrush;
            }

            return TransparentBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return App.Current.Resources["TransparentBrush"];
        }
    }

    /// <summary>
    /// pre popis neizolovaneho useku;
    /// Ak je neizolovany usek oznaceny ako obsadeny, potom popis useku bude mat fialove pozadie,
    /// inac je transparent
    /// </summary>
    public class BackgroundColorConverter2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            SolidColorBrush TransparentBrush = App.Current.Resources["TransparentBrush"] as SolidColorBrush;
            SolidColorBrush obsadenyUsekBrush = App.Current.Resources["obsadenyUsekBrush"] as SolidColorBrush;


            //if (value == null || targetType == null || obsadenyUsekBrush == null || !(value is PATH_MODE))
            //    return App.Current.Resources["TransparentBrush"] as SolidColorBrush;

            //PATH_MODE mode = (PATH_MODE)value;

            //if (mode == PATH_MODE.OBSADENY_USEK)
            //{
            //    return obsadenyUsekBrush;
            //}

            return TransparentBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return App.Current.Resources["TransparentBrush"];
        }
    }


    /*POZN:
     * PFunkcie-Simulacia-Obsadenosti
        len pre IZOLOVANE USEKY
        usek ma fialovu farbu, popisny text sa nemeni

        stav=9 uvolIzol =0 usek-fialova farba
        stav=1 uvolIzol =0 usek-
        ----
        UDRZBA-Oznacenie obsadenia
        Stav  uvolIzol
         1      0         text cierny, pozadie Transparent
         1      8         text biely, pozadie fialove
     */

    /// <summary>
    /// Pre nastavenie farby textu a pozadia pre usek (izolovany a neizolovany), value obsahuje  PathStatus pre usek;
    /// Ak je neizolovany usek oznaceny ako obsadeny, potom popis useku bude mat biele znaky na fialovom pozadi; 
    /// inac cierne znaky na transparent pozadi;
    /// Ak je izolovany usek oznaceny ako obsadeny, potom popis useku bude mat cierne znaky na transparent pozadi (default nastavenie)
    /// </summary>
    public class BackgroundColorConverter3 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush TransparentBrush = App.Current.Resources["TransparentBrush"] as SolidColorBrush;
            SolidColorBrush obsadenyUsekBrush = App.Current.Resources["obsadenyUsekBrush"] as SolidColorBrush;

           // if (value == null || targetType == null || obsadenyUsekBrush == null)
           //     return App.Current.Resources["TransparentBrush"] as SolidColorBrush;

           // //Int64 status = (Int64)value;
           // (Int16 stav, Int16 uvolIzol, Int16 vyluka, Int16 podTyp) = StavadloHelper.GetStatusValues2((Int64)value);

           //// int uvolIzol = (int)value;
           // BitArray uvolIzolBity = new BitArray(new int[] { uvolIzol });
           // bool obsadenieNeizolUseku = uvolIzolBity[3];

           // if (obsadenieNeizolUseku)
           // {
           //     return obsadenyUsekBrush;
           // }

            return TransparentBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return App.Current.Resources["TransparentBrush"];
        }
    }

    //MH: pridane 15.11.2013
    /// <summary>
    /// Zmeni farbu textu pre textblock, ktory zobrazuje nazov vymeny;
    /// ak vymena ma zaver a nie je sucast cesty, potom text ma cervenu farbu;
    /// ak vymena ma zaver a je sucast cesty text ostava biely
    /// ak vymena nema zaver text ostava biely
    /// Pre TextBlock Foreground="{Binding ElementName=V810, Path=(....ZaverBezCesty, Mode=OneWay), Converter={StaticResource ForeTextConv}
    /// </summary>
    /// 
    [ValueConversion( /*source*/ typeof(Boolean), /*target*/typeof(SolidColorBrush))]
    public class SwitchNameColorConverter : IValueConverter
    {
        /// <summary>
        /// bool kovertuje na SolidColorBrush
        /// </summary>
        /// <param name="value">obsahuje bool</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// 
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return App.Current.Resources["whiteSwitchNameTextBrush"] as SolidColorBrush;

            bool zaverBezCesty = (bool)value;
            if (zaverBezCesty)
                return App.Current.Resources["redSwitchNameTextBrush"] as SolidColorBrush;
            else
                return App.Current.Resources["whiteSwitchNameTextBrush"] as SolidColorBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion( /*source*/ typeof(Boolean), /*target*/typeof(SolidColorBrush))]
    public class SytostFarbyConverter : IValueConverter
    {
        /// <summary>
        /// byte kovertuje na SolidColorBrush
        /// </summary>
        /// <param name="value">obsahuje bool</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// 
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return new SolidColorBrush(Color.FromArgb((byte)50, 0x00, 0x00, 0x00));
            byte sytostFarby = (byte)value;

            return new SolidColorBrush(Color.FromArgb(sytostFarby, 0x00, 0x00, 0x00));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
