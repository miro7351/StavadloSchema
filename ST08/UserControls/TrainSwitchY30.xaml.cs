
namespace PA.Stavadlo.UserControls
{
   

    #region Documentation
    /// <summary
    /// <remarks>
    /// <para>
    /// Class Info: contains logic for user control TrainSwitchY30 (logika pre vyhybku s uhlom ramien 30 stupnov a zahnutym ramenom1)
    /// <list type="bullet">
    /// <item name="author">Author: RNDr. M. Hrabcak, CSc.</item>
    /// <item name="date">Marec 2022</item>
    /// <item name="email">hrabcak@procaut.sk</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// </summary>
    #endregion

    public partial class TrainSwitchY30 : BaseTrainSwitchControl
    {
        public TrainSwitchY30()
        {
            InitializeComponent();
            base.Rameno1 = this.Rameno1_30;
            base.Rameno2 = this.Rameno2_30;
            base.Rozrez1 = this.Rozrez1_30;
            base.Rozrez2 = this.Rozrez2_30;
            base.MainGrid = this.mainGrid_30;
            base.VariantGrid = this.VariantGrid_30; //pozri xaml: <Grid x:Name="VariantGrid_30" .../>

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
