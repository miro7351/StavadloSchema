
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace PA.Stavadlo.MH.UserControls
{
    /// <summary>
    /// Interaction logic for UClocomotive.xaml
    /// </summary>
    public partial class UClocomotive : UserControl
    {
        //StavadloGlobalData globalData;
        
        //private readonly log4net.ILog Log;// = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public UClocomotive()
        {
            InitializeComponent();
            //globalData = StavadloGlobalData.Instance;
            
            //Log = globalData.Log;

            this.Loaded += UClocomotive_Loaded;
        }

        private void UClocomotive_Loaded(object sender, RoutedEventArgs e)
        {
            double height = this.Height;
            txbNumber.FontSize = height / 4.0;//velkost fontu pre cislo lokomotivy
            //Pre kazdu lokomotivu nastavime binding <controls:UClocomotive   Visibility="{Binding GlobalData.MovableElementVisibility, Mode=OneWay}".../>
            //Je to vyhodne, lebo pre vsetky lokomotivy DP Visibility sa binduje na jeden zdroj v DataContexte!!!
            //"MovableElementVisibility" je  property z DataContextu, riadi visibility prvkov stavadla ako su lokomotivy, panaci, vykolajky

            Binding VisibilityBinding = new Binding("GlobalData.MovableElementVisibility") { Mode = BindingMode.OneWay, Source = this.DataContext };
            BindingOperations.SetBinding(this, VisibilityProperty, VisibilityBinding);
        }

        public Brush LocomotiveBrush
        {
            get { return (Brush)GetValue(LocomotiveBrushProperty); }
            set { SetValue(LocomotiveBrushProperty, value); }
        }

       /// <summary>
       /// Brush pre farbu locomotivy
       /// </summary>
        public static readonly DependencyProperty LocomotiveBrushProperty =
            DependencyProperty.Register("LocomotiveBrush", typeof(Brush), typeof(UClocomotive), new PropertyMetadata(Brushes.Transparent));


        /// <summary>
        /// Cislo-popis zobrazene na lokomotive
        /// </summary>
        public string LocNumber
        {
            get { return (string)GetValue(LocNumberProperty); }
            set { SetValue(LocNumberProperty, value); }
        }

        public static readonly DependencyProperty LocNumberProperty =
            DependencyProperty.Register("LocNumber", typeof(string), typeof(UClocomotive), new UIPropertyMetadata("I"));



        /// <summary>
        /// uhol otocenia lokomotivy okolo osi
        /// </summary>
        public double RotAngle
        {
            get { return (double)GetValue(RotAngleProperty); }
            set { SetValue(RotAngleProperty, value); }
        }

        public static readonly DependencyProperty RotAngleProperty =
            DependencyProperty.Register("RotAngle", typeof(double), typeof(UClocomotive), new UIPropertyMetadata(0.0));

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
            DependencyProperty.Register("Speed", typeof(double), typeof(UClocomotive), new PropertyMetadata(5.0));
        //--


        //otacanie - kolieskom mysi
        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
                double speed = Speed; 

                if (e.Delta > 0)
                    RotAngle += speed;
                else
                    RotAngle -= speed;

                e.Handled = true;
        }


        /// <summary>
        /// vrati na povodne miesto
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.RenderTransform = new MatrixTransform();
            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            RotateTransform rotate = new RotateTransform(0);
            RotAngle = 0;
            TranslateTransform translate = new TranslateTransform();
            SkewTransform skew = new SkewTransform();
            TransformGroup tgroup = new TransformGroup();
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
