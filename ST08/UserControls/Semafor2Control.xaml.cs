using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PA.Stavadlo.UserControls
{

    // 9.11.2015 Pozn. Semafor2 sa na MapeStavadla uz nepouziva, namiesto neho je semafor1 s attached property s1,s2,s3, tento objekt sa pouziva len v okne oSymbolochWindow
    public enum SEMAFOR2_MODE
    {
        //STOJ,
        //VOLNO,
        //POSUN_NEZABEZP_DOVOLENY,
        POSUN_ZABEZP_DOVOLENY, // biela - siva
        POSUN_NEDOVOLENY,      // siva - modra
        VYLUKA_NAVESTIDLA,     // siva - siva
        //PORUCHA_NAVESTIDLA
    }
    /// <summary>
    /// Navestidlo pre stavadlo
    /// </summary>
    public partial class Semafor2Control : UserControl
    {
        private SolidColorBrush prewTopLightBrush;
        /// <summary>
        /// Previous top light brush; minula farba horneho svetla
        /// </summary>
        public SolidColorBrush PrewTopLightBrush
        {
            get { return prewTopLightBrush; }
            set { prewTopLightBrush = value; }
        }

        private SolidColorBrush prewBottomLightBrush;
        /// <summary>
        /// Previous bottom light brush; minula farba spodneho svetla
        /// </summary>
        public SolidColorBrush PrewBottomLightBrush
        {
            get { return prewBottomLightBrush; }
            set { prewBottomLightBrush = value; }
        }

        public Semafor2Control()
        {
            InitializeComponent();
            PrewTopLightBrush = (SolidColorBrush)this.Resources["redLightBrush"];
            PrewBottomLightBrush = (SolidColorBrush)this.Resources["lightOffBrush"];
        }


        #region ========== RoutedEvents =============

        //lavy klik na semafor
        public static readonly RoutedEvent Semafor2LightModeChangedAttachedEvent = EventManager.RegisterRoutedEvent("Semafor2LightModeChangedAttachedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Semafor2Control));

        //pravy klik na semafor
        public static readonly RoutedEvent Semafor2RightClickAttachedEvent = EventManager.RegisterRoutedEvent("Semafor2RightClickAttachedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Semafor2Control));

        // 9.11.2015 Pridane lavy klik na semafor v suvislosti so stavanim P/V Ciest
        public static readonly RoutedEvent Semafor2LeftClickAttachedEvent = EventManager.RegisterRoutedEvent("Semafor2LeftClickAttachedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Semafor2Control));
        private void RaiseSemafor2LeftClickAttachedEvent()
        {
            //RoutedEventArgs newEventArgs = new RoutedEventArgs(Semafor2Control.Semafor2LeftClickAttachedEvent, new SemaphoreEventArgs() { Name = "Left Click", sender = this });
            //RaiseEvent(newEventArgs);
        }
        public static void AddSemafor2LeftClickAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ((UIElement)d).AddHandler(Semafor2Control.Semafor2LeftClickAttachedEvent, handler);
        }
        // Koniec pridaneho


        private void RaiseSemafor2LightModeChangedAttachedEvent()
        {

            //RoutedEventArgs newEventArgs = new RoutedEventArgs(Semafor2Control.Semafor2LightModeChangedAttachedEvent, new SemaphoreEventArgs() { Name = "zmena", sender = this });
            //RaiseEvent(newEventArgs);
        }

        public static void AddSemafor2LightModeChangedAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ((UIElement)d).AddHandler(Semafor2Control.Semafor2LightModeChangedAttachedEvent, handler);
        }

        public static void RemoveSemafor2LightModeChangedAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ((UIElement)d).RemoveHandler(Semafor2Control.Semafor2LightModeChangedAttachedEvent, handler);
        }


        private void RaiseSemafor2RightClickAttachedEvent()
        {
            
        }

        public static void AddSemafor2RightClickAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ((UIElement)d).AddHandler(Semafor2Control.Semafor2RightClickAttachedEvent, handler);
        }

        public static void RemoveSemafor2RightClickAttachedEventHandler(DependencyObject d, RoutedEventHandler handler)
        {
            ((UIElement)d).RemoveHandler(Semafor2Control.Semafor2RightClickAttachedEvent, handler);
        }


        #endregion ========== RoutedEvents =============

        /// <summary>
        /// nastavi mod-farbu semaforu
        /// </summary>
        /// <param name="mode"></param>
        public void SetMode(SEMAFOR2_MODE mode)
        {
            LightMode = mode;
        }

        public SEMAFOR2_MODE LightMode
        {
            get { return (SEMAFOR2_MODE)GetValue(LightModeProperty); }
            set { SetValue(LightModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LightMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LightModeProperty =
            DependencyProperty.Register("LightMode", typeof(SEMAFOR2_MODE), typeof(Semafor2Control), new UIPropertyMetadata(SEMAFOR2_MODE.VYLUKA_NAVESTIDLA, new PropertyChangedCallback(OnLightModeChanged)));

        private void SemaforLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string name = ((UserControl)sender).Name;
            RaiseSemafor2LightModeChangedAttachedEvent();
        }

        private void SemaforRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine(name);
            RaiseSemafor2RightClickAttachedEvent(); //odpalenie Routed eventu
        }

       
        private static void OnLightModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Semafor2Control control = (Semafor2Control)d;
            SEMAFOR2_MODE prewMode = (SEMAFOR2_MODE)e.OldValue;
            SEMAFOR2_MODE newMode = (SEMAFOR2_MODE)e.NewValue;

            switch (prewMode)
            {
                case SEMAFOR2_MODE.POSUN_NEDOVOLENY:
                    control.PrewTopLightBrush = (SolidColorBrush)control.Resources["redLightBrush"];
                    control.PrewBottomLightBrush = (SolidColorBrush)control.Resources["lightOffBrush"];
                    break;
                case SEMAFOR2_MODE.POSUN_ZABEZP_DOVOLENY:
                    control.PrewTopLightBrush = (SolidColorBrush)control.Resources["redLightBrush"];
                    control.PrewBottomLightBrush = (SolidColorBrush)control.Resources["lightOffBrush"];
                    break;
                case SEMAFOR2_MODE.VYLUKA_NAVESTIDLA:
                    control.PrewTopLightBrush = (SolidColorBrush)control.Resources["redLightBrush"];
                    control.PrewBottomLightBrush = (SolidColorBrush)control.Resources["lightOffBrush"];
                    break;
                default:
                    break;
            }

            switch (newMode)
            {
                case SEMAFOR2_MODE.POSUN_NEDOVOLENY:
                    control.SetPosunNedovoleny();
                    break;
                case SEMAFOR2_MODE.POSUN_ZABEZP_DOVOLENY:
                    control.SetPosunZabezpDovol();
                    break;
                case SEMAFOR2_MODE.VYLUKA_NAVESTIDLA:
                    break;
                default:
                    break;
            }
        }

        private void SetNormalStav()
        {
            mainBorder.BorderBrush = this.Resources["blackBrush"] as SolidColorBrush;
            RedLight.Stroke = this.Resources["blackBrush"] as SolidColorBrush;
            GreenLight.Stroke = this.Resources["blackBrush"] as SolidColorBrush;
            RedLight.Fill = this.Resources["lightOffBrush"] as SolidColorBrush;
            GreenLight.Fill = this.Resources["lightOffBrush"] as SolidColorBrush;
            Nozicka1.Stroke = this.Resources["blackBrush"] as SolidColorBrush;
            Nozicka1.Fill = this.Resources["blackBrush"] as SolidColorBrush;
            SetOppacity(1);
        }
        public void SetOppacity(double oppacity)
        {
            RedLight.Opacity = oppacity;
            GreenLight.Opacity = oppacity;
            if (oppacity == 1)
            {
                mainBorder.Opacity = Nozicka1.Opacity = oppacity;
            }
        }

        public void SetPosunNedovoleny()
        {
            SetNormalStav();
            GreenLight.Fill = this.Resources["blueLightBrush"] as SolidColorBrush;
        }

        public void SetPosunZabezpDovol()
        {
            SetNormalStav();
            RedLight.Fill = this.Resources["whiteLightBrush"] as SolidColorBrush;
        }

        // End ZK_NazVas

    }//class Semafor2Control
}
