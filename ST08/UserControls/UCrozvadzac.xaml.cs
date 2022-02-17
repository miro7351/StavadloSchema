
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;



namespace PA.Stavadlo.MH.UserControls
{
    /// <summary>
    /// UserContol znazornuje rozvadzac
    /// </summary>
    public partial class UCrozvadzac : UserControl
    {
        public UCrozvadzac()
        {
            InitializeComponent();
            this.Loaded += UCrozvadzac_Loaded;
        }

        private void UCrozvadzac_Loaded(object sender, RoutedEventArgs e)
        {
            //Pre kazdy rozvadzac nastavime binding <controls:UCrozvadzac   Visibility="{Binding GlobalData.RozvadzacVisibility, Mode=OneWay}".../>
            //Je to vyhodne, lebo pre vsetky rovadzace DP Visibility sa binduje na jeden zdroj v DataContexte: GlobalData.RozvadzacVisibility!!!
            //GlobalData je property vo ViewModeli, RozvadzacVisibility je property v GlobalData  "RozvadzacVisibility" je meno property z DataContextu

            Binding VisibilityBinding = new Binding("GlobalData.RozvadzacVisibility") { Mode = BindingMode.OneWay, Source = this.DataContext };
            BindingOperations.SetBinding(this, VisibilityProperty, VisibilityBinding);

            //Binding VisibilityBinding = new Binding("RozvadzacVisibility") { Mode = BindingMode.OneWay, Source = this.DataContext };
            //BindingOperations.SetBinding(this, VisibilityProperty, VisibilityBinding);

        }

        //handler pre lavy klik mysou: xaml: MouseLeftButtonDown="Rozvadzac_MouseLeftButtonDown" 
        private void Rozvadzac_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //odpalenie Routed eventu; zachyti sa v StavadloPage01.xaml
            //Tag sa pouziva pre Tooltip, lebo viac rozvadzacov moze mat rovnake oznacenie; 
            //Name sa pouziva pre nazov Info suboru
            RaiseRozvadzacKlikAttachedEvent(this.Name);
        }



        //attached event pre lavy klik na rozvadzac
        public static readonly RoutedEvent RozvadzacKlikAttachedEvent = EventManager.RegisterRoutedEvent("RozvadzacKlikAttachedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UCrozvadzac));

        private void RaiseRozvadzacKlikAttachedEvent(string elementName)
        {
            //string rozvadzacName = this.Name;
            //RoutedEventArgs newEventArgs = new RoutedEventArgs(UCrozvadzac.RozvadzacKlikAttachedEvent, new RozvadzacEventArgs(){ sender=this});
            //RaiseEvent(newEventArgs);
        }

        public static void AddRozvadzacKlikAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ((UIElement)d).AddHandler(UCrozvadzac.RozvadzacKlikAttachedEvent, handler);
        }

        public static void RemoveRozvadzacKlikAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ((UIElement)d).RemoveHandler(UCrozvadzac.RozvadzacKlikAttachedEvent, handler);
        }

    }//class UCrozvadzac
}
