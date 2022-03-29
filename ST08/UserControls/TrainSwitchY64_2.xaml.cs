

namespace PA.Stavadlo.UserControls
{
    /// <summary>
    /// Interaction logic for TrainSwitchY64_2.xaml
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
    public partial class TrainSwitchY64_2 : BaseTrainSwitchControl
    {
        public TrainSwitchY64_2()
        {
            InitializeComponent();
            base.Rameno1 = this.RamenoY1_64_2;
            base.Rameno2 = this.RamenoY2_64_2;
            base.Rozrez1 = this.RozrezY1_64_2;
            base.Rozrez2 = this.RozrezY2_64_2;
            base.MainGrid = this.mainGridY_64_2;
            base.VariantGrid = this.VariantGridY_64_2; //pozri xaml: <Grid x:Name="VariantGrid_64_2" .../>

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
