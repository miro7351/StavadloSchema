
using System.Windows.Controls;

namespace PA.Stavadlo.MH.UserControls
{
    /// <summary>
    /// SoundControler zapina/vypina pouzitie zvuku pri zobrazeni chybovych stavov
    /// </summary>
    public partial class SoundControler : UserControl
    {
        //StavadloGlobalData globalData;//globalne udaje pre aplikaciu

        public SoundControler()
        {
            InitializeComponent();
            //if (App.AppCommunicator == null)
            //    return;
            //globalData = StavadloGlobalData.Instance;
            CheckSoundButton();
        }

        private void SoundButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckSoundButton();
        }

        private void CheckSoundButton()
        {
            
        }
    }
}
