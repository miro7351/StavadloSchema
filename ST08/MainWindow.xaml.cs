using System.Windows;
using System.Windows.Input;

namespace ST08
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            msgWritter.labelLeft.Content = "Oznam nalavo";
            msgWritter.labelRight.Content = "Oznam napravo";
            //msgWritter.label3.
        }

        #region ---------Obsluha buttonov: nastavovanie velkosti okna, pohyb okna, ukoncenie aplikacie-----------

        /// <summary>
        /// handler pre button na minimalizovanie okna
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowMinimalize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// handler pre button na ukoncenie aplikacie
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowClose(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// handler pre button na maximalizovanie okna
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowMaximalize(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        /// <summary>
        /// handler pre tahanie okna a dvojklik na maximalizaciu okna
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DragStart(object sender, MouseButtonEventArgs e)
        {
            //if (e.ClickCount == 2)
            //{
            //    WindowMaximalize(sender, e);
            //    return;
            //}

            DragMove();
        }
        #endregion ---------Nastavovanie velkosti okna, ukoncenie aplikacie-----------
    }
}
