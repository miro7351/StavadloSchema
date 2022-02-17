
using System;
using System.Windows;
using System.Windows.Controls;
using PA.Stavadlo.MH.Data;
using PA.Stavadlo.MH.Communication;

namespace PA.Stavadlo.MH.UserControls
{
    /// <summary>
    /// UC_ElementInfo2 obsahuje udaje z tlg. c. 131.
    /// PFunkcie-Informacie-O prvku.
    /// User Control pre zobrazenie informacii o prvku na ktory sa kliklo.
    /// DataContext nastaveny v xaml.
    /// </summary>
    public partial class UC_ElementInfo2 : UserControl
    {
        /// <summary>
        /// User Control pre zobrazenie informacii o prvku na ktory sa kliklo.
        /// Pouziva viewModel ElementInfoWindowViewModel.
        /// Po kliku na prvok sa posle do servera telegram c. 121.
        /// Server vrati tel. c. 131. Ten sa spracuje v ElementInfoWindowViewModel a nastavi 
        /// GlobalData.ElementInfoVisibility  = Visibility.Visisble;
        /// </summary>

        StavadloGlobalData GlobalData;
        Communicator AppCommunicator;

        bool ucCanBeMinimalized;//priznak pre  zmenu Visibility Property Panelu
        public UC_ElementInfo2()
        {
            InitializeComponent();
            GlobalData = StavadloGlobalData.Instance;
            AppCommunicator = Communicator.Instance;

            //handler pre event ak sa prijme telegram c. 141 zo servera, aby okno automaticky zavrelo
            if (AppCommunicator != null)
                AppCommunicator.TlgMessage141Recieved += AppCommunicator_Tlg141Recieved;
            ucCanBeMinimalized = false;
        }

        //ak sa prijme telegram c. 141, potom Info panel sa skryje/neskryje podla nastavenia z PFunkcie->Nastavenie->info panel
        void AppCommunicator_Tlg141Recieved(object sender, Infrastructure.Communication.EventArgs2 e)
        {
            bool status = GlobalData.HideOpenPanel;// (bool)App.Current.Resources["hideOpenPanel"];//
            //System.Diagnostics.Debug.WriteLine("+++++++++++++++UC_ElementInfo status="+status.ToString() + "canBeMinimal=" + ucCanBeMinimalized.ToString() );
            //PFunkcie - Nastavenia-Info panel:CheckBox: Skryt panel pri zmene
            //true pri prijme telegramu c.141 zavrie okno
            //false pri prijme telegramu c.141 nezavri okno
            if (status)
            {
                if (ucCanBeMinimalized)
                    App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                    {
                        //App.Current.Resources["visibilityElementInfo"] = System.Windows.Visibility.Collapsed;
                        GlobalData.ElementInfoVisibility = Visibility.Collapsed;
                    }));
            }
        }

        //az po otvoreni Info panelu sa moze panel minimlizovat
        /// <summary>
        /// handler pre UserControl event IsVisibilityChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ucCanBeMinimalized = (bool)e.NewValue;
        }
    }
}
