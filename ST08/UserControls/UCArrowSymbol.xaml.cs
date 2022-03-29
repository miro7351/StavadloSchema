using System.Windows;
using System.Windows.Controls;

using PA.Stavadlo.Infrastructure.Enums;


namespace PA.Stavadlo.UserControls
{
    //MH: semptember 2018 uprava Mode, pridanie ARROW_SYMBOL_MODE do Infrastructure.Enums
    /// <summary>
    /// Sipka pre zobrazenie suhlasu.
    /// Pouziva sa v okne OSymbolochWindow. Riadenie blikania pomocov DataTrigerov
    /// </summary>
    public partial class UCArrowSymbol : UserControl
    {
        public UCArrowSymbol()
        {
            InitializeComponent();
        }


        public ARROW_SYMBOL_MODE ARROW_SYMBOL
        {
            get { return (ARROW_SYMBOL_MODE)GetValue(ARROW_SYMBOLProperty); }
            set { SetValue(ARROW_SYMBOLProperty, value); }
        }

        /// <summary>
        /// pre nastavenie modu v akom je zobrazeny UserControl
        /// </summary>
        public static readonly DependencyProperty ARROW_SYMBOLProperty =
            DependencyProperty.Register("ARROW_SYMBOL", typeof(ARROW_SYMBOL_MODE), typeof(UCArrowSymbol), new PropertyMetadata(ARROW_SYMBOL_MODE.NONE));



    }

    //pozri Infrastructure.Enums Enums01.cs
    //public enum ARROW_SYMBOL_MODE
    //{
    //    INVISIBLE = 0,
    //    ZIADOST_VCHOD,  //blika Transparent<->Yellow kazdu sekundu
    //    SUHLAS_VCHOD,   //zlty trvalo svieti
    //    ZIADOST_ODCHOD, //blika Transparent<->Green kazdu sekundu
    //    SUHLAS_ODCHOD,  //zeleny trvalo svieti
    //    NONE
    //}
}
