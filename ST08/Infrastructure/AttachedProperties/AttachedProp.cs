
using System.Windows;
using Stavadlo22.Infrastructure.Enums;


namespace Stavadlo22.Infrastructure.AttachedProperties
{
    /// <summary>
    /// attached property pre Signalizacny button
    /// </summary>
    public class SignalButtonAttProp : DependencyObject
    {

        public static BUTTON_MODE GetMode(DependencyObject obj)
        {
            return (BUTTON_MODE)obj.GetValue(ModeProperty);
        }

        public static void SetMode(DependencyObject obj, BUTTON_MODE value)
        {
            obj.SetValue(ModeProperty, value);
        }

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.RegisterAttached("Mode", typeof(BUTTON_MODE), typeof(SignalButtonAttProp), new PropertyMetadata(BUTTON_MODE.RELEASED));
    }


    public class CurrentMode : DependencyObject
    {

        public static CURRENT_MENU_MODE GetMode(DependencyObject obj)
        {
            return (CURRENT_MENU_MODE)obj.GetValue(ModeProperty);
        }

        public static void SetMode(DependencyObject obj, CURRENT_MENU_MODE value)
        {
            obj.SetValue(ModeProperty, value);
        }

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.RegisterAttached("Mode", typeof(CURRENT_MENU_MODE), typeof(CurrentMode), new PropertyMetadata(CURRENT_MENU_MODE.NONE));
    }


    /// <summary>
    /// pouziva sa pre Controly (Buttony); AttachedProperty obsahuje string; zoznam roli pre ktore je Control.IsEnabled=true
    /// POZN: string: ADMIN; control pristupny len pre rolu ADMIN
    /// POZN: string: UDRZBA,ADMIN; control pristupny len pre UDRZBA A ADMIN
    /// POZN: string: DISPECER;control pristupny len pre DISPECER
    /// POZN: string.Empty
    /// v Style je Control.Visibility bindovana na EnabledRoles a je vyhodnotena Converterom
    /// </summary>
    public class EnabledRolesHelper : DependencyObject
    {

        public static readonly DependencyProperty EnabledRolesProperty;
        static EnabledRolesHelper()
        {
            EnabledRolesProperty = DependencyProperty.RegisterAttached("EnabledRoles", typeof(string),
               typeof(EnabledRolesHelper), new PropertyMetadata(string.Empty));
        }

        public static string GetEnabledRoles(DependencyObject target)
        {
            return (string)target.GetValue(EnabledRolesProperty);
        }

        public static void SetEnabledRoles(DependencyObject target, string value)
        {
            target.SetValue(EnabledRolesProperty, value);
        }

    }//class EnabledRolesHelper

    //<Style TargetType="Button">
    //       <Setter Property="Visibility" 
    //               Value="{Binding RelativeSource={RelativeSource Mode=Self}, Path=(infraStructure:EnabledRolesHelper.EnabledRoles), Mode=OneWay, Converter={StaticResource roleConv3}}"/>
    //   </Style>
    /*
     * <Button Content="ADMIN, UDRZBA" Margin="10" infraStructure:EnabledRolesHelper.EnabledRoles="ADMIN,UDRZBA" /><!--pristupny pre roly: ADMIN, UDRZBA-->
  <Button Content="Vsetci-1" Margin="10"/> <!--pristupny pre vsetkych-->
  <Button Content="Vsetci-2" Margin="10" infraStructure:EnabledRolesHelper.EnabledRoles=""/> <!--pristupny pre vsetkych-->
  <Button Content="Vsetci-3" Margin="10" infraStructure:EnabledRolesHelper.EnabledRoles="MAJSTER"/><!--rola MAJTER neexistuje, buton bude nepristupny-->
     * */


}
