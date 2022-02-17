namespace PA.Stavadlo.MH.UserControls
{
    #region Documentation
    /// <summary
    /// <remarks>
    /// <para>
    /// Class Info: contains logic for user control TrainSwitch64 (logika pre vyhybku s uhlom ramien 64 stupnov)
    /// <list type="bullet">
    /// <item name="author">Author: RNDr. M. Hrabcak, CSc.</item>
    /// <item name="date">August 2018</item>
    /// <item name="email">hrabcak@procaut.sk</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// </summary>
    #endregion

    public partial class TrainSwitch64 : BaseTrainSwitchControl
    {
        public TrainSwitch64()
        {
            InitializeComponent();
            base.Rameno1 = this.Rameno1_64;
            base.Rameno2 = this.Rameno2_64;
            base.Rozrez1 = this.Rozrez1_64;
            base.Rozrez2 = this.Rozrez2_64;
            base.MainGrid = this.mainGrid_64;
            base.VariantGrid = this.VariantGrid_64; //pozri xaml: <Grid x:Name="VariantGrid_64" .../>

            //tu este Name nie je zname!!!
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
