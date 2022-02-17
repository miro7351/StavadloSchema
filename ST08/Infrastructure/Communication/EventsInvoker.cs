using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Stavadlo22.Infrastructure.AppEventArgs;
using Stavadlo22.UserControls;
using Stavadlo22.Infrastructure.Controls;

namespace Stavadlo22.Infrastructure.Communication
{
    /// <summary>
    /// Pomocna trieda. Obsahuje funkcie, ktore odpaluju eventy
    /// </summary>
    public class AppEventsInvoker
    {

        private static readonly AppEventsInvoker _instance = new AppEventsInvoker();

        public static AppEventsInvoker Instance => _instance;//expression bodied readonly property
        //public static AppEventsInvoker Instance
        //{
        //    get
        //    {
        //        return _instance;
        //    }
        //}

        //-------------------MH:13.12.2013-------------------
        /// <summary>
        /// Event odpaleny pri kliku (lavy klik) na prvok stavadla: semafor, vymenu, usek, branu
        /// </summary>
        public event Action<string> StavadloElementClickedEvent;  //pre InfoZariadenieViewModel.cs

        /// <summary>
        /// funkcia na odpalenie eventu
        /// </summary>
        /// <param name="elName"></param>
        public void InvokeElementInfoEvent(string elName)
        {
            StavadloElementClickedEvent?.Invoke(elName);
        }
        //--------------------------------------------------

        //----MH: 10.01.2019 -----------------

        /// <summary>
        /// Event odpaleny pri kliku (lavy klik) na prvok stavadla: semafor, vymenu, usek, branu
        /// </summary>
        public event Action<FrameworkElement> StavadloElementClickedEventFE;  //pre InfoZariadenieViewModel.cs

        /// <summary>
        /// funkcia na odpalenie eventu
        /// </summary>
        /// <param name="elName"></param>
        public void InvokeElementInfoEventFE(FrameworkElement element)
        {
            StavadloElementClickedEventFE?.Invoke(element);
        }

        //-------------------------------

        //---------MH:23.12.2013-------------------

            /* MH zaremovane 21.02.2019
        public event Action<SelectedPath> UserSelectedPcestaEvent;
        public event Action<SelectedPath> UserSelectedVcestaEvent;

        /// <summary>
        /// odpali event UserSelectedPcestaEvent;
        /// pouziva sa po kliku na usek pri stavani Posunovej cesty, alebo pri vyluke posunovej cesty;
        /// pozri StavadloPage01.xaml.cs
        /// </summary>
        /// <param name="selPath"></param>
        public void InvokeUserSelectedPcestaEvent(SelectedPath selPath)
        {
            if (UserSelectedPcestaEvent != null)
                UserSelectedPcestaEvent(selPath);
        }

        /// <summary>
        /// odpali event UserSelectedVcestaEvent;
        /// pouziva sa po kliku na usek pri stavani vlakovej cesty, alebo pri vyluke vlakovej cesty
        /// pozri StavadloPage01.xaml.cs
        /// </summary>
        /// <param name="selPath"></param>
        public void InvokeUserSelectedVcestaEvent(SelectedPath selPath)
        {
            if (UserSelectedVcestaEvent != null)
                UserSelectedVcestaEvent(selPath);
        }
        */
        /// <summary>
        /// event odpaleny ak uzivatel klikol na platny pociatocny usek posunovej cesty
        /// </summary>
        public event Action<string> UserSelStartPathPcestaValidEvent;
        
        /// <summary>
        /// event odpaleny ak uzivatel klikol na platny pociatocny usek vlakovej cesty
        /// </summary>
        public event Action<string> UserSelStartPathVcestaValidEvent;

        public void InvokeStartPathPcesta_IsValisEvent(string pathName)
        {
             UserSelStartPathPcestaValidEvent?.Invoke(pathName);
        }

        public void InvokeStartPathVcesta_IsValidEvent(string pathName)
        {
            UserSelStartPathVcestaValidEvent?.Invoke(pathName);
        }

        //=== MH: 10.01.2019 ====

        /// <summary>
        /// event odpaleny ak uzivatel klikol na platny pociatocny usek posunovej cesty
        /// </summary>
        public event Action<Path> UserSelStartPathPcestaValidEventFE;

        /// <summary>
        /// event odpaleny ak uzivatel klikol na platny pociatocny usek vlakovej cesty
        /// </summary>
        //public event Action<Path> UserSelStartPathVcestaValidEventFE;
        public event Action<FrameworkElement> UserSelStartPathVcestaValidEventFE;

        public void InvokeStartPathPcesta_IsValidEvent(Path path)
        {
            UserSelStartPathPcestaValidEventFE?.Invoke(path);
        }

        public void InvokeStartPathVcesta_IsValidEvent(FrameworkElement el)
        {
            UserSelStartPathVcestaValidEventFE?.Invoke(el);
        }

        //MH: pridane 21.03.2019
        /// <summary>
        /// event odpaleny ak uzivatel klikol na Path alebo Semafor a je to platny pociatocny usek posunovej cesty
        /// </summary>
        public event Action<FrameworkElement> UserSelStartPathPcestaValidEventFE2;
        public void InvokeStartPathPcesta_IsValidEvent2(FrameworkElement fe)
        {
            UserSelStartPathPcestaValidEventFE2?.Invoke(fe);
        }

        //----------------------------

        /// <summary>
        /// event odpaleny, ak uzivatel klikol na platny koncovy usek posunovej cesty a cesta nema varianty
        /// </summary>
        public event Action<string> UserSelEndPathPcestaIsValidEvent_Variant0;
        
        
        /// <summary>
        /// event odpaleny, ak uzivatel klikol na platny koncovy usek vlakovej cesty a cesta nema varianty
        /// </summary>
        public event Action<string> UserSelEndPathVcestaIsValidEvent_Variant0;

        /// <summary>
        /// event odpaleny, ak uzivatel klikol na platny koncovy usek posunovej cesty a cesta ma varianty
        /// </summary>
        public event Action<string> UserSelEndPathPcestaIsValidEvent;
        /// <summary>
        ///  event odpaleny, ak uzivatel klikol na platny koncovy usek vlakovej cesty a cesta ma varianty
        /// </summary>
        public event Action<string> UserSelEndPathVcestaIsValidEvent;


        //======= MH: 10.01.2019 ============

        /// <summary>
        /// event odpaleny, ak uzivatel klikol na platny koncovy usek posunovej cesty a cesta nema varianty
        /// </summary>
        public event Action<Path> UserSelEndPathPcestaIsValidEvent_Variant0FE;


        /// <summary>
        /// event odpaleny, ak uzivatel klikol na platny koncovy usek vlakovej cesty a cesta nema varianty
        /// </summary>
        public event Action<FrameworkElement> UserSelEndPathVcestaIsValidEvent_Variant0FE;

        /// <summary>
        /// event odpaleny, ak uzivatel klikol na platny koncovy usek posunovej cesty a cesta ma varianty
        /// </summary>
        //public event Action<Path> UserSelEndPathPcestaIsValidEventFE;
        public event Action<FrameworkElement> UserSelEndPathPcestaIsValidEventFE;
        /// <summary>
        ///  event odpaleny, ak uzivatel klikol na platny koncovy usek vlakovej cesty a cesta ma varianty
        /// </summary>
        public event Action<FrameworkElement> UserSelEndPathVcestaIsValidEventFE;


        public event Action<FrameworkElement> UserSelEndPathPcestaIsValidEvent_Variant0FE2;//MH: 22.03.2019
        //---------------------------

        /// <summary>
        /// odpali event, ak uzivatel klikol na platny koncovy usek posunovej cesty, pri stavani cesty, alebo vyluke cesty
        /// ak cesta ma varianty
        /// </summary>
        /// <param name="pathName"></param>
        public void InvokeEndPathPcesta_IsValidEvent(string pathName)
        {
             UserSelEndPathPcestaIsValidEvent?.Invoke(pathName);
        }

        //public void InvokeEndPathPcesta_IsValidEvent(Path path)
        //{
        //    UserSelEndPathPcestaIsValidEventFE?.Invoke(path);
        //}

        public void InvokeEndPathPcesta_IsValidEvent(FrameworkElement el)
        {
            UserSelEndPathPcestaIsValidEventFE?.Invoke(el);
        }

        /// <summary>
        /// odpali event ak uzivatel klikol na platny koncovy usek vlakovej cesty, pri stavani cesty, alebo vyluke cesty
        /// ak cesta ma varianty
        /// </summary>
        /// <param name="pathName"></param>
        public void InvokeEndPathVcesta_IsValidEvent(string pathName)
        {
             UserSelEndPathVcestaIsValidEvent?.Invoke(pathName);
        }

        public void InvokeEndPathVcesta_IsValidEvent(FrameworkElement el)
        {
            UserSelEndPathVcestaIsValidEventFE?.Invoke(el);
        }

        public void InvokeEndPathPcesta_Variant0_IsValidEvent(string pathName)
        {
            UserSelEndPathPcestaIsValidEvent_Variant0?.Invoke(pathName);
        }

        public void InvokeEndPathPcesta_Variant0_IsValidEvent(Path path)
        {
            UserSelEndPathPcestaIsValidEvent_Variant0FE?.Invoke(path);
        }

        //MH: 22.03.2019
        public void InvokeEndPathPcesta_Variant0_IsValidEvent(FrameworkElement el)
        {
            UserSelEndPathPcestaIsValidEvent_Variant0FE2?.Invoke(el);
        }



        public void InvokeEndPathVcesta_Variant0_IsValidEvent(string pathName)
        {
                UserSelEndPathVcestaIsValidEvent_Variant0?.Invoke(pathName);
        }

        public void InvokeEndPathVcesta_Variant0_IsValidEvent(FrameworkElement el)
        {
            UserSelEndPathVcestaIsValidEvent_Variant0FE?.Invoke(el);
        }
        //-------------
        /// <summary>
        /// event odpaleny na konci postavenia Posunovej/Vlakovej cesty, ak sa vybral posledny usek, ktory jednoznacne urcuje cestu
        /// </summary>
        public event Action TrainPathIsValidEvent;

        public void InvokeTrainPath_IsValidEvent()
        {
            TrainPathIsValidEvent?.Invoke();
        }


        //-----------------------------------------

        /// <summary>
        /// event odpaleny po prijati tlg.161 v mode CURRENT_MODE.ZRUSENIE_CESTY, CURRENT_MODE.VYLUKA_POSUNOVEJ_CESTY alebo CURRENT_MODE.VYLUKA_VLAKOVEJ_CESTY 
        /// </summary>
        public event Action<string> Tlg161PathNameEvent;//Tlg161PathNameEvent
        public void InvokeTlg161PathNameEvent(string fullPathName)
        {
             Tlg161PathNameEvent?.Invoke(fullPathName);
        }

       

        //zmena MH: september 2018
        public event Action<Path> NastavUsekSucastCesty_Event;

        public void Invoke_NastavUsekSucastCesty_Event(Path myPath)
        {
            NastavUsekSucastCesty_Event?.Invoke(myPath);
        }

        //public event Action<string> NastavVymenuSucastCesty_Event;
        //public void Invoke_NastavVymenuSucastCesty_Event(string switchName)
        //{

        //    if (NastavVymenuSucastCesty_Event != null)
        //        NastavVymenuSucastCesty_Event(switchName);

        //}

        //MH: zmena oktober 2018
        public event Action<BaseTrainSwitchControl> NastavVymenuSucastCesty_Event;
        public void Invoke_NastavVymenuSucastCesty_Event(BaseTrainSwitchControl control)
        {
            NastavVymenuSucastCesty_Event?.Invoke(control);
        }
        //-------------------------------

        /// <summary>
        /// event odpaleny pri zavreti User Controlu UC_LogsPlayerControl
        /// </summary>
        public event Action LogsPlayerControl_ClosedEvent;

        /// <summary>
        /// odpali event LogsPlayerControl_ClosedEvent
        /// </summary>
        public void Invoke_LogsPlayerControl_ClosedEvent()
        {
                LogsPlayerControl_ClosedEvent?.Invoke();
        }

        //--------------------------------------------

        public event Action StartVymenaTimer_Event;
        /// <summary>
        /// odpali event StartVymenaTimer_Event
        /// </summary>
        public void Invoke_StartVymenaTimer_Event()
        {
                StartVymenaTimer_Event?.Invoke();
        }

        public event Action StopVymenaTimer_Event;
        /// <summary>
        /// odpali event StopVymenaTimer_Event
        /// </summary>
        public void Invoke_StopVymenaTimer_Event()
        {
            StopVymenaTimer_Event?.Invoke();
        }
        //------------------------------------------

        public event Action<string> LogPlayerControl_RoleChangedEvent;
        public void Invoke_LogPlayerControl_RoleChangedEvent(string newRoleName)
        {
             LogPlayerControl_RoleChangedEvent?.Invoke(newRoleName);
        }

        public event Action<DateTime> LogPlayerControl_SelectedDateFromChanged;
        public void Invoke_LogPlayerControl_SelectedDateFromChanged(DateTime dateFrom)
        {
            LogPlayerControl_SelectedDateFromChanged?.Invoke(dateFrom);
        }

        public event Action LogPlayer_DisposeEvent;
        public void Invoke_LogPlayer_DisposeEvent()
        {
            LogPlayer_DisposeEvent?.Invoke();
        }


        //MH: pridane oktober 2018

        public event Action ControlBlinkedEvent;

        /// <summary>
        /// Odpali event ControlBlinkedEvent
        /// </summary>
        public void Invoke_ControlBlinkedEvent()
        {
            ControlBlinkedEvent?.Invoke();
        }


        public event Action<BlinkedControl> BlinkedControlEvent;

        public void Invoke_BlinkedControlEvent(BlinkedControl bc)
        {
            BlinkedControlEvent.Invoke(new BlinkedControl(bc.ControlName, bc.State));
        }

        //MH: pridane februar 2019
        //odpaluje sa v ServerWatchDogControler ak sa prerusi/obnovi spojenie so serverom
        public event Action<string> ServerConnectionEvent;
        public void Invoke_ServerConnectionEvent(string msg)
        {
            ServerConnectionEvent?.Invoke(msg);
        }

        public event Action PrevozOceleEvent;

        public void PrevozOcele_IsValidEvent()
        {
            PrevozOceleEvent?.Invoke();
        }

    }

   
}
