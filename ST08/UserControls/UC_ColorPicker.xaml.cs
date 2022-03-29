using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PA.Stavadlo.UserControls
{
    //MH: Februar 2019
    /// <summary>
    /// Pre vyber SolidColorBrush z nastavenej palety
    /// </summary>
    public partial class UC_ColorPicker : UserControl
    {
        public UC_ColorPicker()
        {
            InitializeComponent();
        }


        public SolidColorBrush SelectedBrush
        {
            get { return (SolidColorBrush)GetValue(SelectedBrushProperty); }
            set { SetValue(SelectedBrushProperty, value); }
        }

        public static readonly DependencyProperty SelectedBrushProperty =
            DependencyProperty.Register("SelectedBrush", typeof(SolidColorBrush), typeof(UC_ColorPicker), new PropertyMetadata(new SolidColorBrush(Color.FromArgb((byte)0xFF, (byte)0xCB, (byte)0xCB, (byte)0xCB))));

       
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //#FFCBCBCB
            //SolidColorBrush defaultBrush = new SolidColorBrush(Color.FromArgb((byte)0xFF, (byte)0xCB, (byte)0xCB, (byte)0xCB));
            ListBox lb = sender as ListBox;
            ListBoxItem lbi = lb.SelectedItem as ListBoxItem;
            SelectedBrush = lbi.Background as SolidColorBrush;
        }
    }
}
