﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PA.Stavadlo.MH.UserControls
{
    /// <summary>
    /// Interaction logic for UC_MapaST08.xaml
    /// </summary>
    public partial class UC_MapaST08 : UserControl
    {
        public UC_MapaST08()
        {
            InitializeComponent();
        }

        private void MapaST08_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void MapaST08_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        //Spracovanie udalosti pomocou routed eventu; Event zavedeny v xaml subore v Style pre Path
        //Odchytava sa v StavadloPage.xaml

        /// <summary>
        /// Handler pre zachytenie kliku na usek;
        /// Pozri Style pre Path;
        /// Odpali routed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PathMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }//PathMouseLeftButtonDown
    }
}
