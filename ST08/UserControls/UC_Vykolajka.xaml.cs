
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace PA.Stavadlo.MH.UserControls
{
    /// <summary>
    /// User control Vykolajka (cerveny trojuholnik), predmet pouzivany na kolajisku.
    /// </summary>
    public partial class UC_Vykolajka : UserControl
    {
        //StavadloGlobalData globalData;
       

        //ILog Log;
        //private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public UC_Vykolajka()
        {
            InitializeComponent();
            //globalData = StavadloGlobalData.Instance;
           
            //Log = globalData.Log;
            this.Loaded += UC_Vykolajka_Loaded;
        }

        private void UC_Vykolajka_Loaded(object sender, RoutedEventArgs e)
        {
            //Pre kazdu vykolajku nastavime binding <controls:UC_Vykolajka   Visibility="{Binding MovableElementVisibility, Mode=OneWay}".../>
            //Je to vyhodne, lebo pre vsetky vykolajky DP Visibility sa binduje na jeden zdroj v DataContexte!!!
            //"MovableElementVisibility" je  property z DataContextu, ktora riadi Visibility controlu.

            Binding VisibilityBinding = new Binding("GlobalData.MovableElementVisibility") { Mode = BindingMode.OneWay, Source = this.DataContext };
            BindingOperations.SetBinding(this, VisibilityProperty, VisibilityBinding);
        }


        #region == Dependency properties ==



        public Brush VykolajkaBrush
        {
            get { return (Brush)GetValue(VykolajkaBrushProperty); }
            set { SetValue(VykolajkaBrushProperty, value); }
        }

        /// <summary>
        /// farba vykolajky
        /// </summary>
        public static readonly DependencyProperty VykolajkaBrushProperty =
            DependencyProperty.Register("VykolajkaBrush", typeof(Brush), typeof(UC_Vykolajka), new PropertyMetadata(Brushes.Red));

        public string Typ
        {
            get { return (string)GetValue(TypProperty); }
            set { SetValue(TypProperty, value); }
        }

        /// <summary>
        ///  Typ=L pre lavu vykolajku, Typ=P pre pravu vykolajku
        /// </summary>
        public static readonly DependencyProperty TypProperty =
            DependencyProperty.Register("Typ", typeof(string), typeof(UC_Vykolajka), new PropertyMetadata(string.Empty));

        
        public double RotAngle
        {
            get { return (double)GetValue(RotAngleProperty); }
            set { SetValue(RotAngleProperty, value); }
        }
       
        public static readonly DependencyProperty RotAngleProperty =
            DependencyProperty.Register("RotAngle", typeof(double), typeof(UC_Vykolajka), new UIPropertyMetadata(0.0));

        //MH: pridane august 2018

        public double Speed
        {
            get { return (double)GetValue(SpeedProperty); }
            set { SetValue(SpeedProperty, value); }
        }

        /// <summary>
        /// zmena uhla pri otoceni kolieska mysi
        /// </summary>
        public static readonly DependencyProperty SpeedProperty =
            DependencyProperty.Register("Speed", typeof(double), typeof(UC_Vykolajka), new PropertyMetadata(5.0));
        //--


        #endregion == Dependency properties ==

        /// <summary>
        /// otacanie vykolajky okolo osi - krutenie kolieskom mysi
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //Otacanie vykolajky okolo osi pomocou otacania kolieska mysi
            // if (e.LeftButton == MouseButtonState.Pressed)//musi byt stlacene lave tlacidlo mysi!!!
            {
                //double speed = Convert.ToDouble(App.Current.Resources["RotationSpeed"]);
                double speedMH = Speed;

                if (e.Delta > 0)
                    RotAngle += speedMH;
                else
                    RotAngle -= speedMH;

                e.Handled = true;
            }
        }

        /// <summary>
        /// vrati vykolajku na povodne miesto na displaji
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.RenderTransform = new MatrixTransform();
            //upravi velkost otocienia
            RotAngle = 0d;
            
            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            RotateTransform rotate = new RotateTransform(0);
            TranslateTransform translate = new TranslateTransform();
            SkewTransform skew     = new SkewTransform();
            TransformGroup tgroup  = new TransformGroup();
            tgroup.Children.Add(scale);
            tgroup.Children.Add(rotate);
            tgroup.Children.Add(translate);
            tgroup.Children.Add(skew);
            this.RenderTransform = tgroup;
            UserControl_ManipulationComplete(null, null);       //zapis do logu o zmene polohy
        }

        //zapis do  logu
        private void UserControl_ManipulationComplete(object sender, MouseButtonEventArgs e)
        {
           // Point locationFromScreen = this.PointToScreen(new Point(0, 0));
           // log.Info(string.Format("{0}{1},{2},{3}", App.LogHeaders["MOVEABLE-ELEMENT"], locationFromScreen.X, locationFromScreen.Y, this.Name ));
             
            //ulozi posun pre X a Y + meno elementu
            //Log.Info(string.Format("{0}{1};{2};{3}", globalData.LogHeaders["MOVEABLE-ELEMENT"], this.RenderTransform.Value.OffsetX, this.RenderTransform.Value.OffsetY, this.Name ));
            //Log?.Info($"{globalData.LogHeaders["MOVEABLE-ELEMENT"]}{this.RenderTransform.Value.OffsetX};{this.RenderTransform.Value.OffsetY};{this.Name}");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class TypConverter : IValueConverter
    {
        //hodnoty value:prazdny string, L, P; ak je L vrati -1 ak je P, alebo string.Empty vrati 1
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;
            string s1 = value.ToString();
            if (s1 == "L")
                return -1;
            else return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
  
}
