
using System;
using System.Windows.Media;

using Stavadlo22.UserControls;
using Stavadlo22.Infrastructure.Enums;
using System.Windows.Controls;

namespace Stavadlo22.Infrastructure
{
    //MH: oktober 2018
    /// <summary>
    /// Trieda pre vypis oznamov do spodneho riadku aplikacie
    /// </summary>
    public class MessageWriter
    {
        private static readonly MessageWriter _instance = new MessageWriter();

        private MessageWriter()//place for instance initialization code
        {
            Clear();
        }

        public static MessageWriter Instance
        {
            get
            {
                return _instance;
            }
        }

        UC_MessageWriter ucWriter;

        #region ------Methods-------------

        public void SetMessageWriterControl(Control  control)
        {
            ucWriter = (UC_MessageWriter)control;
        }

        /// <summary>
        /// vypise lavy oznam; mod je INFO
        /// </summary>
        /// <param name="text"></param>
        public void WriteLeft(string text)
        {
            Write(1, "");
            Write(1, text, INFO_MODE.INFO);
        }

        /// <summary>
        /// vypise lavy oznam podla nastaveneho modu
        /// </summary>
        /// <param name="text"></param>
        /// <param name="mode"></param>
        public void WriteLeft(string text, INFO_MODE mode)
        {
            Write(1, "");
            Write(1, text, mode);
        }

        /// <summary>
        /// vypise pravy oznam, mod je INFO
        /// </summary>
        /// <param name="text"></param>
        public void WriteRight(string text)
        {
           // System.Diagnostics.Debug.WriteLine($"///////// WriteRight text: {text}");
           //text: TEST_VYPISU_MH na displaji bude: TESTVYPISU_MH!!!!
            text = text.Replace('_', '-');
           // System.Diagnostics.Debug.WriteLine($"///////// WriteRight text: {text}");
            Write(2, "");
            Write(2, text, INFO_MODE.INFO);
        }

        /// <summary>
        /// vypise pravy oznam podla nastaveneho modu
        /// </summary>
        /// <param name="text"></param>
        /// <param name="mode"></param>
        public void WriteRight(string text, INFO_MODE mode)
        {
            //System.Diagnostics.Debug.WriteLine($"///////// WriteRight text: {text}");
            Write(2, "");
            Write(2, text, mode);
        }

        private void Write(int position, string text)
        {
            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
            {
                if (position == 1)
                {
                    ucWriter.labelLeft.Content = text;
                    ucWriter.labelLeft.Background = new SolidColorBrush(Colors.White);
                    ucWriter.labelLeft.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if (position == 2)
                {
                    ucWriter.labelRight.Content = text;
                    ucWriter.labelRight.Background = new SolidColorBrush(Colors.White);
                    ucWriter.labelRight.Foreground = new SolidColorBrush(Colors.Black);
                }
                //else if (position == 3)
                //{
                //    label3.Content = text;
                //    label3.Background = new SolidColorBrush(Colors.White);
                //    label3.Foreground = new SolidColorBrush(Colors.Black);
                //}
            }));
        }

        private void Write(int position, string text, INFO_MODE mode)
        {
            //System.Diagnostics.Debug.WriteLine($"///////// Write text: {text}");
            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
            {
                switch (mode)
                {
                    case INFO_MODE.NONE:
                        if (position == 1)
                        {
                            ucWriter.labelLeft.Background = new SolidColorBrush(Colors.White);
                            //labelLeft.Foreground = new SolidColorBrush(Colors.Black);
                            ucWriter.labelLeft.Foreground = new SolidColorBrush(Colors.Blue);
                            ucWriter.labelLeft.Content = text;
                        }
                        else if (position == 2)
                        {
                            ucWriter.labelRight.Background = new SolidColorBrush(Colors.White);
                            ucWriter.labelRight.Foreground = new SolidColorBrush(Colors.Black);
                            ucWriter.labelRight.Content = text;
                        }
                        //else if (position == 3)
                        //{
                        //    label3.Background = new SolidColorBrush(Colors.White);
                        //    label3.Foreground = new SolidColorBrush(Colors.Black);
                        //    label3.Content = text;
                        //}
                        break;
                    case INFO_MODE.INFO:
                        if (position == 1)
                        {
                            ucWriter.labelLeft.Background = new SolidColorBrush(Colors.White);
                            ucWriter.labelLeft.Foreground = new SolidColorBrush(Colors.Blue);
                            ucWriter.labelLeft.Content = text;
                        }
                        else if (position == 2)
                        {
                            ucWriter.labelRight.Background = new SolidColorBrush(Colors.White);
                            ucWriter.labelRight.Foreground = new SolidColorBrush(Colors.Black);
                            ucWriter.labelRight.Content = text;
                        }
                        //else if (position == 3)
                        //{
                        //    label3.Background = new SolidColorBrush(Colors.White);
                        //    label3.Foreground = new SolidColorBrush(Colors.Black);
                        //    label3.Content = text;
                        //}

                        break;
                    case INFO_MODE.ERROR:
                        if (position == 1)
                        {
                            ucWriter.labelLeft.Background = new SolidColorBrush(Colors.Yellow);
                            ucWriter.labelLeft.Foreground = new SolidColorBrush(Colors.Red);
                            ucWriter.labelLeft.Content = text;
                        }
                        else if (position == 2)
                        {
                            ucWriter.labelRight.Background = new SolidColorBrush(Colors.Yellow);
                            ucWriter.labelRight.Foreground = new SolidColorBrush(Colors.Red);
                            ucWriter.labelRight.Content = text;
                        }
                        //else if (position == 3)
                        //{
                        //    label3.Background = new SolidColorBrush(Colors.Yellow);
                        //    label3.Foreground = new SolidColorBrush(Colors.Red);
                        //    label3.Content = text;
                        //}
                        break;
                    case INFO_MODE.EXCEPTION:
                        if (position == 1)
                        {
                            ucWriter.labelLeft.Background = new SolidColorBrush(Colors.White);
                            ucWriter.labelLeft.Foreground = new SolidColorBrush(Colors.Red);
                            ucWriter.labelLeft.Content = text;
                        }
                        else if (position == 2)
                        {
                            ucWriter.labelRight.Background = new SolidColorBrush(Colors.White);
                            ucWriter.labelRight.Foreground = new SolidColorBrush(Colors.Red);
                            ucWriter.labelRight.Content = text;
                        }
                        //else if (position == 3)
                        //{
                        //    label3.Background = new SolidColorBrush(Colors.White);
                        //    label3.Foreground = new SolidColorBrush(Colors.Red);
                        //    label3.Content = text;
                        //}
                        break;
                    default:
                        if (position == 1)
                        {
                            ucWriter.labelLeft.Background = new SolidColorBrush(Colors.White);
                            ucWriter.labelLeft.Foreground = new SolidColorBrush(Colors.Black);
                            ucWriter.labelLeft.Content = text;
                        }
                        else if (position == 2)
                        {
                            ucWriter.labelRight.Background = new SolidColorBrush(Colors.White);
                            ucWriter.labelRight.Foreground = new SolidColorBrush(Colors.Black);
                            ucWriter.labelRight.Content = text;
                        }
                        //else if (position == 3)
                        //{
                        //    label3.Background = new SolidColorBrush(Colors.White);
                        //    label3.Foreground = new SolidColorBrush(Colors.Black);
                        //    label3.Content = text;
                        //}
                        break;
                }//switch
            }));
        }

        /// <summary>
        /// vymaze lavy oznam
        /// </summary>
        public void ClearLeft()
        {
            Write(1, string.Empty);
        }
        /// <summary>
        /// vymaze pravy oznam
        /// </summary>
        public void ClearRight()
        {
            Write(2, string.Empty);
        }
        /// <summary>
        /// vymaze pravy oznam
        /// </summary>
        public void Clear3()
        {
            Write(3, string.Empty);
        }
        /// <summary>
        /// vymaze lavy a pravy oznam;
        /// vymaze vypisany text, nastavi mod na INFO
        /// </summary>
        public void Clear()
        {
            Write(1, string.Empty, INFO_MODE.INFO);
            Write(2, string.Empty, INFO_MODE.INFO);
            Write(3, string.Empty, INFO_MODE.INFO);
        }

        #endregion ------Methods-------------
    }
}
