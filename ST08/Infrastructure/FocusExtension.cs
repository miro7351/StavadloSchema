using System.Windows;

namespace Stavadlo22.Infrastructure
{
    /// <summary>
    /// pre nastavenie focusu na control pomocou kodu,
    /// ak je to Button potom bude mat IsDefault = true
    /// </summary>
    public static class FocusExtension
    {
        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }


        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }


        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
             "IsFocused", typeof(bool), typeof(FocusExtension),
             new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));


        private static void OnIsFocusedPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var uie = (UIElement)d;
            if ((bool)e.NewValue)
            {
                uie.Focus(); // Don't care about false values.
                System.Windows.Controls.Button button = uie as System.Windows.Controls.Button;
                if (button != null)
                {
                    button.IsDefault = true;
                }
            }
        }
    }
}
