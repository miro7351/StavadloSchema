
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace PA.Stavadlo.UserControls
{
    /// <summary>
    /// Interaction logic for UC_PanakUss.xaml
    /// </summary>
    public partial class UC_PanakUss : UserControl
    {
        //StavadloGlobalData globalData;
       

        //private readonly log4net.ILog Log;

        //private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public UC_PanakUss()
        {
            InitializeComponent();
            //globalData = StavadloGlobalData.Instance;
           
            //Log = globalData.Log;
            this.Loaded += UC_PanakUss_Loaded;
        }

        private void UC_PanakUss_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //Pre kazdeho panaka nastavime binding <controls:UC_PanakUss   Visibility="{Binding GlobalData.MovableElementVisibility, Mode=OneWay}".../>
            //Je to vyhodne, lebo pre vsetky panaky DP Visibility sa binduje na jeden zdroj v DataContexte!!!
            //"MovableElementVisibility" je meno property z DataContextu

            Binding VisibilityBinding = new Binding("GlobalData.MovableElementVisibility") { Mode = BindingMode.OneWay, Source = this.DataContext };
            BindingOperations.SetBinding(this, VisibilityProperty, VisibilityBinding);
        }


        /// <summary>
        /// vrati na povodne miesto
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            TranslateTransform translate = new TranslateTransform();
            RotateTransform rotate = new RotateTransform(0);
            SkewTransform skew     = new SkewTransform();
            TransformGroup tgroup  = new TransformGroup();
            tgroup.Children.Add(scale);
            tgroup.Children.Add(rotate);
            tgroup.Children.Add(translate);
            tgroup.Children.Add(skew);
            this.RenderTransform = tgroup;
            UserControl_ManipulationComplete(null, null);       //zapis do logu o zmene polohy
        }

        private void UserControl_ManipulationComplete(object sender, MouseButtonEventArgs e)
        {
            //Log.Info(string.Format("{0}{1};{2};{3}", globalData.LogHeaders["MOVEABLE-ELEMENT"], this.RenderTransform.Value.OffsetX, this.RenderTransform.Value.OffsetY, this.Name));
        }

    }
}
