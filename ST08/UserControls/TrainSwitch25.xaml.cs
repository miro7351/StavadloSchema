namespace PA.Stavadlo.UserControls
{
    //MH: Januar 2022
    /// <summary>
    /// Graficky objekt pre vyhybku-vymenu s uhlom 25 stupnov medzi ramenami
    /// </summary>
    /// 



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
    public partial class TrainSwitch25 : BaseTrainSwitchControl
    {
        public TrainSwitch25()
        {
            InitializeComponent();
            base.Rameno1 = this.Rameno1_25;
            base.Rameno2 = this.Rameno2_25;
            base.Rozrez1 = this.Rozrez1_25;
            base.Rozrez2 = this.Rozrez2_25;
            base.MainGrid = this.mainGrid_25;
            base.VariantGrid = this.VariantGrid_25; //pozri xaml: <Grid x:Name="VariantGrid_64" .../>

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
