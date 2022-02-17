using PA.Stavadlo.MH.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PA.Stavadlo.MH.UserControls
{
    //MH: uprava semtember 2018, pouzitie ARROW_SYMBOL_MODE
    /// <summary>
    /// Sipka pre zobrazenie ziadosti/suhlasu pouzivana v mape stavadla. 
    /// Blikanie riadi UC_StavadloViewModel 
    /// </summary>
    public partial class UCArrow : UserControl
    {
        public UCArrow()
        {
            InitializeComponent();
        }

       
        /* PA.Stavadlo.MH.Enum
         * Enums01.cs
         public enum ARROW_SYMBOL_MODE
         {
             INVISIBLE = 0,
             ZIADOST_VCHOD,  //blika Transparent<->Yellow kazdu sekundu
             SUHLAS_VCHOD,   //zlty trvalo svieti
             ZIADOST_ODCHOD, //blika Transparent<->Green kazdu sekundu
             SUHLAS_ODCHOD,  //zeleny trvalo svieti
             NONE
         }
         */

        public ARROW_MODE Mode
        {
            get { return (ARROW_MODE)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        /// <summary>
        /// mod pre UserControl, pre nastavenie farby a blikania
        /// </summary>
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(ARROW_MODE), typeof(UCArrowSymbol), new PropertyMetadata(ARROW_MODE.NONE));



        public SolidColorBrush MyStroke
        {
            get { return (SolidColorBrush)GetValue(MyStrokeProperty); }
            set
            {
                SetValue(MyStrokeProperty, value);
            }
        }

        public static readonly DependencyProperty MyStrokeProperty =
            DependencyProperty.Register("MyStroke", typeof(SolidColorBrush), typeof(UCArrow), new UIPropertyMetadata( new SolidColorBrush(Colors.Transparent)));

        public SolidColorBrush MyFill
        {
            get { return (SolidColorBrush)GetValue(MyFillProperty); }
            set
            {
                // ZK_NazVas
                SetValue(MyFillProperty, value);
                this.SetValue(BackgroundProperty, value);
            }
        }

        public static readonly DependencyProperty MyFillProperty =
            DependencyProperty.Register("MyFill", typeof(SolidColorBrush), typeof(UCArrow), new UIPropertyMetadata( new SolidColorBrush(Colors.Transparent))  );
    }
}
