using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ST08;


namespace PA.Stavadlo.Infrastructure.Converters
{
    /// <summary>
    /// Pre Button nastavi IsEnabled property;
    /// pouziva App.Current.Resources["currentUserRole"], tato polozka sa nastavi po prihlaseni uzivatela do  programu
    /// </summary>
    [ValueConversion( /*source*/ typeof(System.String), /*target*/typeof(Boolean))]
    public class RoleConverter : IValueConverter
    {
        //value obsahuje zoznam roli oddelenych ciarkou napr.: admin,otk; t.j. roly povolene pre dany button
        //App.Current.Resources["currentUserRole"] obsahuje rolu prihlaseneho uzivatela
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return true;

            string v1 = value.ToString();//zoznam roli napr. : admin,otk
            if (string.IsNullOrEmpty(v1))
                return true;

            if (App.Current.Resources["currentUserRole"] == null)
                return true;
            string cur = (string)App.Current.Resources["currentUserRole"];

            if (v1.Contains(cur))
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }//class RoleConverter

    /*
     * Priklad:
       <Style TargetType="Button">
            <Setter Property="IsEnabled" 
                    Value="{Binding RelativeSource={RelativeSource Mode=Self}, Path=(infraStructure:EnabledRolesHelper.EnabledRoles), Mode=OneWay, Converter={StaticResource roleConv1}}"/>
        </Style>
      
     * <Button Content="otk" Margin="10" infraStructure:EnabledRolesHelper.EnabledRoles="admin,otk" /><!--pristupny pre roly: admin a pre otk-->
   <Button Content="Vsetci-1" Margin="10"/> <!--pristupny pre vsetkych-->
   <Button Content="Vsetci-2" Margin="10" infraStructure:EnabledRolesHelper.EnabledRoles=""/> <!--pristupny pre vsetkych-->
   <Button Content="Vsetci-3" Margin="10" infraStructure:EnabledRolesHelper.EnabledRoles="majster"/><!--rola majster neexistuje, buton bude nepristupny-->
    */


    //pouziva sa pri bindovani Control.Visibility property
    public class RoleConverter3 : IValueConverter
    {
        //value obsahuje: UDRZBA, ADMIN; t.j. roly povolene pre dany button, je bindovana na AttachedProperty EnabledRolesHelper.EnabledRoles
        //App.Current.Resources["currentUserRole"] obsahuje rolu prihlaseneho uzivatela, nastavuje sa v App.xaml.cs a po prihlaseni uzivatela
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            string strValue = value.ToString();//zoznam roli napr. : ADMIN,UDRZBA"
            if (string.IsNullOrEmpty(strValue))
                //return System.Windows.Visibility.Collapsed;
                return System.Windows.Visibility.Visible;

            if (App.Current.Resources["currentUserRole"] == null)//nie je nastaveny resource
                return System.Windows.Visibility.Collapsed;

            string curRole = (string)App.Current.Resources["currentUserRole"];

            if (strValue.Contains(curRole))
                return System.Windows.Visibility.Visible;
            else
                return System.Windows.Visibility.Collapsed; ;

        }

        //pre Binding nepouziva Mode=TwoWay
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /*
     * Priklad:
       <Style TargetType="Button">
            <Setter Property="Visibility" 
                    Value="{Binding RelativeSource={RelativeSource Mode=Self}, Path=(infraStructure:EnabledRolesHelper.EnabledRoles), Mode=OneWay, Converter={StaticResource roleConv3}}"/>
        </Style>
      
     * <Button Content="otk" Margin="10" infraStructure:EnabledRolesHelper.EnabledRoles="ADMIN,URDZBA" /><!--pristupny pre roly: admin a pre otk-->
   <Button Content="Vsetci-1" Margin="10"/> <!--pristupny pre vsetkych-->
   <Button Content="Vsetci-2" Margin="10" infraStructure:EnabledRolesHelper.EnabledRoles=""/> <!--pristupny pre vsetkych-->
   <Button Content="Vsetci-3" Margin="10" infraStructure:EnabledRolesHelper.EnabledRoles="MAJSTER"/><!--rola majster neexistuje, buton bude nepristupny-->
    */
    }//class RoleConverter3



    //SignalButtonsBaseViewModel.GateLocalControlIsEnabled ak je true, potom je povolene automaticke otvaranie/zatvaranie brany
    //ak GateLocalControlIsEnabled=true, potom nevidno znak R pri brane na mape Stavadla17
    public class GateConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //throw new NotImplementedException();
            if (value == null)
                return null;


            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //throw new NotImplementedException();

            return 1;
        }
    }

    //SignalButtonsBaseViewModel.GateLocalControlIsEnabled ak je true, potom je Miestne  ovladanie (nie z HMI, ale pri brane) brany je povolene vidno znak R
    //ak GateLocalControlIsEnabled=false, Miestne  ovladanie brany nie je povolene (ovlada sa z HMI, nie od brany), potom nevidno znak R pri brane na mape Stavadla17
    public class GateControlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //throw new NotImplementedException();
            if (value == null)
                return null;
            bool status = (bool)value;
            if (status) //Miestne  ovladanie (nie z HMI) brany je povolene
                return System.Windows.Visibility.Visible;
            else
                return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //throw new NotImplementedException();

            return 1;
        }
    }

    //Nastavuje enable na Buttony v login okne
    public class RoleToLoginEnable : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;
            string curRole = (string)App.Current.Resources["currentUserRole"];
            string name = value.ToString();

            //switch (name)
            //{
            //    case "login_prihlasenie":
            //        if (curRole == USER_ROLE.NONE.ToString()) return true;
            //        else return false;
            //    case "login_koniec":
            //    case "login_odhlasenie":
            //    case "login_zmena_hesla":
            //        if (curRole == USER_ROLE.NONE.ToString()) return false;
            //        else return true;
            //    case "login_uzivatela": return false;
            //}
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }

    /// <summary>
    /// MH 25.09.2013: podla dlzky zobrazovaneho textu nastavi lavy okraj pre TextBlock;
    /// Pouziva sa v UC_ElementInfo.xaml
    /// </summary>
    [ValueConversion( /*source*/ typeof(System.String), /*target*/typeof(Thickness))]
    public class VymenaMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Thickness margin = new Thickness(0d, 0d, 0d, 0d);
            if (value == null)
                return margin;
            string s1 = value.ToString();

            if (s1.Length > 15 && s1.Length < 20)
                margin.Left = -30d;
            else if (s1.Length > 20)
                margin.Left = -60d;
            return margin;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// MH 11.10.2013: podla hodnoty value ( Content pre button): AUT, RUC nastavi mode vystup, pouziva sa pre signal button AUT/RUC
    /// Pouziva sa v UC_ElementInfo.xaml
    /// </summary>
    //[ValueConversion( /*source*/ typeof(System.Object), /*target*/typeof(BUTTON_MODE))]
    public class SignalButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null)
                return null;

            //BUTTON_MODE mode = BUTTON_MODE.NONE;

            //string s1 = value.ToString();
            //if (s1.Contains("AUT"))
            //    mode = BUTTON_MODE.RELEASED;
            //if (s1.Contains("RUC"))
            //    mode = BUTTON_MODE.PRESSED;
            //return mode;
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// MH 15.10.2013: podla hodnoty value ( Content pre button): AUT, RUC nastavi Tag, pouziva sa pre signal button AUT/RUC
    /// Pouziva sa v UC_ElementInfo.xaml
    /// </summary>
    //[ValueConversion( /*source*/ typeof(System.Object), /*target*/typeof(BUTTON_MODE))]
    public class SignalButtonConverter2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null)
                return null;

            // BUTTON_MODE mode = BUTTON_MODE.NONE;
            string result = "";
            string s1 = value.ToString();
            if (s1.Contains("AUT"))
                //mode = BUTTON_MODE.RELEASED;
                result = "0";
            if (s1.Contains("RUC"))
                // mode = BUTTON_MODE.PRESSED;
                result = "1";
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    //pre testovanie bindingov, ak dame tu break point mozeme vidiet co ide do nejakej property 
    public class TestConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;
            string myValue = value.ToString();
            // System.Diagnostics.Debug.WriteLine("TestConverter:" + myValue);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //throw new NotImplementedException();

            return 1;
        }
    }

    //MH: september 2018
    /// <summary>
    /// ConcatStringConverter spoji objects (values[i].ToString()) z values do stringu
    /// </summary>
    public class ConcatStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string separator = parameter == null ? ";" : parameter.ToString();
            return String.Join<object>(separator, values);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            //return (value as string).Split(',');
            throw new NotImplementedException();
        }
    }

    public class DictionaryItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //var oc = value as ObservableCollection<PrvokStavadla>;
            //string param = parameter as string;
            //PrvokStavadla p1 = oc.Where(p => p.Nazov == param).FirstOrDefault();
            //return p1;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /*
    public class CollectionItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var oc = value as ObservableCollection<PrvokStavadla>;
            string param = parameter as string;
            if (oc.Count == 0)
               return new BitArray(new int[] {0,0 } );
            PrvokStavadla p1 = oc.Where(p => p.Nazov == param).FirstOrDefault();
            if(p1==null)
                return new BitArray(new int[] { 0, 0 });
            return p1.BityStatus;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    */

    /*
    public class DictionaryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
          object parameter, string language)
        {
            var dictionary = value as Dictionary<string, string>;
            if (dictionary == null || !(parameter is string))
            {
                return null;
            }
            string result;
            dictionary.TryGetValue((string)parameter, out result);
            return result;
        }

        public object ConvertBack(object value, Type targetType,
          object parameter, string language)
        {
            return null;
        }
    }
    */
}
