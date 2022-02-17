using ST08;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PA.Stavadlo.MH.UserControls
{
    /// <summary>
    /// Interaction logic for UC_BackgroudColorSetter.xaml
    /// </summary>
    public partial class UC_BackgroudColorSetter : UserControl
    {
        public UC_BackgroudColorSetter()
        {
            InitializeComponent();
        }

        // <summary>
        /// nastavenie opacity pre pozadie mapy stavadla
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            byte sytostFarby = (byte)e.NewValue;
            //SolidColorBrush test = new SolidColorBrush(Color.FromArgb(sytostFarby, 0xFF, 0xFF, 0xFF));
            //string newStringBrush = string.Format("#{0:X2}000000", sytostFarby);
            App.Current.Resources["backBrushMapaStavadla"] = new SolidColorBrush(Color.FromArgb(sytostFarby, 0x00, 0x00, 0x00));
            this.SytostFarby = (byte)e.NewValue; 
            //App.Current.Resources["backBrush"] = "#FFFF0000";
            //App.Current.Resources["backBrush"] = newStringBrush;//chyba: {"'#7A000000' is not a valid value for property 'Background'."}
        }
        /// <summary>
        /// handler pre zavretie panelu na nastavenie pozadia
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClosePanel_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Resources["backgroundSetterStavadlo"] = Visibility.Collapsed;
            e.Handled = true;//aby sa event dalej nespracovaval
        }



        public byte SytostFarby
        {
            get { return (byte)GetValue(SytostFarbyProperty); }
            set { SetValue(SytostFarbyProperty, value); }
        }

        public static readonly DependencyProperty SytostFarbyProperty =
            DependencyProperty.Register("SytostFarby", typeof(byte), typeof(UC_BackgroudColorSetter), new PropertyMetadata((byte)50));
    }
}
