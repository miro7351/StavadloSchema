using System;
using System.Reflection;
using Stavadlo22;
using Stavadlo22.Data;
using Stavadlo22.Infrastructure.Communication;
using Stavadlo22.Infrastructure.Enums;
using Stavadlo22.Infrastructure.PathHelperPA;
using Stavadlo22.Infrastructure.PlayMode;

namespace Stavadlo22.Infrastructure
{
    /// <summary>
    /// class pre manazovanie neizolovanych usekov
    /// </summary>
    public class UnIsolatedPathManager //UnIsolatedPathManager
    {
        PlayModeEventDriver PMeventDriver;
        Communicator AppCommunicator;
        StavadloModel stavadloModel;

        public UnIsolatedPathManager()
        {
            AppCommunicator = Communicator.Instance;
            PMeventDriver = PlayModeEventDriver.Instance;
            stavadloModel = StavadloModel.Instance;

            EventHandlerEnabled = false;
            //MH: zaremovane 11.01.2019
            //if (AppCommunicator != null)
            //    AppCommunicator.TlgMessage121Recieved += MessageTlg121Recieved;
            //else
            //{   //PlayMode: eventy odpalovane v PlayMode
            //    if (App.PMeventDriver != null)
            //    {
            //        App.PMeventDriver.PMtlg121DataReceivedEvent += MessageTlg121Recieved;
            //    }
            //}

            //MH: uprava 11.01.2019
            if(!stavadloModel.PlayMode)
                AppCommunicator.TlgMessage121Recieved += MessageTlg121Recieved;
            else
                PMeventDriver.PMtlg121DataReceivedEvent += MessageTlg121Recieved;
        }

        void MessageTlg121Recieved(string tlgData)
        {
            /*
            //if (EventHandlerEnabled && (!string.IsNullOrEmpty(PathName)) )
            if (EventHandlerEnabled && (ClickedPath != null))
            {
                string message = tlgData.ToLower();
                if (message == "ok")
                {
                    App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                    {
                        if ((App.currentMode == CURRENT_MENU_MODE.OZNACENIE_OBSADENIA || App.currentMode == CURRENT_MENU_MODE.SIMULACIA_OBSADENIA) && (bool)(ClickedPath.GetValue(PathHelper.IsUnisolatedPathProperty)))
                        {
                            PATH_MODE curPathMode = (PATH_MODE)ClickedPath.GetValue(PathHelper.ModeProperty);

                            StavadloElements el = StavadloElements.GetInstance();

                            if (el != null)
                            {
                                PropertyInfo propertyInfo = el.GetType().GetProperty(ClickedPath.Name);
                                //if (propertyInfo == null)
                                //{
                                //    MessageBox.Show("Cesta sa neda nastavit! \nPozadovana property sa nenachádza v triede UC_StavadloViewModel!", "Property == null");
                                //    return;
                                //}
                                if (propertyInfo == null)
                                    return;
                                if (curPathMode != PATH_MODE.OBSADENY_USEK)
                                {
                                    propertyInfo.SetValue(el, PATH_MODE.OBSADENY_USEK, null);
                                }
                                else
                                {
                                    propertyInfo.SetValue(el, PATH_MODE.NORMAL, null);
                                }
                            }
                        }
                        ClickedPath = null;
                    }));
                }// if(e.recievedMessage== "ok" )
            }
            */
        }//AppCommunicator_MessageData1Recieved
        
        public System.Windows.Shapes.Path ClickedPath
        {
            get;
            set;
        }

        public string GetPathName()
        {
            return (ClickedPath == null) ? null : ClickedPath.Name;
        }


        public bool EventHandlerEnabled
        {
            get;
            set;
        }
    }
}
