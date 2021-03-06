using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PA.Stavadlo.UserControls
{
    /// <summary>
    /// Interaction logic for TrainSwitchY64.xaml
    /// </summary>
    #region Documentation
    /// <summary
    /// <remarks>
    /// <para>
    /// Class Info: contains logic for user control TrainSwitch45 (logika pre vyhybku s uhlom ramien 45 stupnov)
    /// <list type="bullet">
    /// <item name="author">Author: RNDr. M. Hrabcak, CSc.</item>
    /// <item name="date">Februar  2022</item>
    /// <item name="email">hrabcak@procaut.sk</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// </summary>
    #endregion
    public partial class TrainSwitchY64 : BaseTrainSwitchControl
    {
        public TrainSwitchY64()
        {
            InitializeComponent();
            base.Rameno1 = this.RamenoY1_64;
            base.Rameno2 = this.RamenoY2_64;
            base.Rozrez1 = this.RozrezY1_64;
            base.Rozrez2 = this.RozrezY2_64;
            base.MainGrid = this.mainGridY_64;
            base.VariantGrid = this.VariantGridY_64; //pozri xaml: <Grid x:Name="VariantGrid_64" .../>

        }

        private void TrainSwitchControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //tu Name, Part01.Name, Part02.Name je zname!!!

            if (Part01 != null)//Part01 zdedene z BaseTrainSwitchControl
            {
                base.Part01 = Part01; //prenos lokalnej hodnoty do parent class
            }
            if (Part02 != null)
            {
                base.Part02 = Part02;
            }
        }
    }
}
