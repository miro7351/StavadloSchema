using Stavadlo22.Data.Telegrams;
using Stavadlo22.Infrastructure.Communication;
using System;


namespace Stavadlo22.Infrastructure.PlayMode
{
    /// <summary>
    /// Pouziva sa v PlayMode;
    /// Odpaluje jednotlive eventy pre udaje nacitane z log suboru,
    /// akoby to boli eventy po prijme telegramu zo servera;
    /// </summary>
    public class PlayModeEventDriver
    {
        private static readonly PlayModeEventDriver _instance = new PlayModeEventDriver();

        public static PlayModeEventDriver Instance => _instance; //readonly property: expression bodied property

        /// <summary>
        /// event odpaleny pre nastavenie polohy pre "virtualCursor"
        /// </summary>
        public event Action<System.Windows.Point> PlayModeElementPositionEvent;

        /// <summary>
        /// pri simulacii kliknutia - zmena pozicie kurzora
        /// </summary>
        public event Action<String> PlayModeClickEvent;

        /// <summary>
        /// event pri nacitani udajov z logu pre telegram c. 141
        /// </summary>
        public event Action<Elements> PMtlg141RecievedEvent;//

        /// <summary>
        /// event pri nacitani udajov z logu pre telegram c. 151
        /// </summary>
        public event Action<EventArgs3> PMtlg151ReceivedEvent;
        /// <summary>
        /// event pri nacitani udajov z logu pre telegram c. 161
        /// </summary>
        public event Action<LogicMessage161> PMtlg161DataReceivedEvent;//

        /// <summary>
        ///  event pri nacitani udajov z logu pre telegram c. 191
        /// </summary>
        public event Action<Sets> PMtlg191ReceivedEvent;//

        /// <summary>
        ///  event pri nacitani udajov z logu pre telegram c. 121
        /// </summary>
        public event Action<string> PMtlg121DataReceivedEvent;

        /// <summary>
        /// simuluje event pred vyslanimtlg.120
        /// </summary>
        public event Action PMtlg120SendingEvent;

        /// <summary>
        /// event pri ukonceni presunu prvku vykolajka - moze sa spravit aj pre vlacik a pracovnikov
        /// </summary>
        public event Action<MoveableData> ElementMovedEvent;

        //funkcia pre odpalenie eventu pre tlg.141
        public void RaisePMtlg141DataRecieved(Elements element)
        {
            PMtlg141RecievedEvent?.Invoke(element);
        }

        //funkcia pre odpalenie eventu pre tlg.151
        public void RaisePMtlg151DataReceived(EventArgs3 e)
        {
            PMtlg151ReceivedEvent?.Invoke(e);
        }

        //funkcia pre odpalenie eventu pre tlg.161
        public void RaisePMtlg161DataReceived(LogicMessage161 data)
        {
            PMtlg161DataReceivedEvent?.Invoke(data);
        }

        //funkcia pre odpalenie eventu pre tlg.121
        public void RaisePMtlg121DataReceived(string tlgString)
        {
            PMtlg121DataReceivedEvent?.Invoke(tlgString);
        }

        //funkcia pre odpalenie eventu, ktroy sa odpaluje pred vyslanim tlg.120
        public void RaisePMtlg120SendingEvent()
        {
            PMtlg120SendingEvent?.Invoke();
        }

        //funkcia pre odpalenie eventu pre tlg.191
        public void RaisePMtlg191DataRecieved(Sets sets)
        {
            PMtlg191ReceivedEvent?.Invoke(sets);
        }

        //funkcia pre odpalenie eventu, ktory presuva objekty na mape stavadla - vykolajka a podobne
        public void RaiseElementMovedEvent(MoveableData data)
        {
            ElementMovedEvent?.Invoke(data);
        }

        public void RaisePlayModeClickEvent(String elemName)
        {
            PlayModeClickEvent?.Invoke(elemName);
        }

        /// <summary>
        /// Event odpaleny ak sa spracuvava zaznam pre CLICK
        /// </summary>
        public event Action<System.Drawing.Point> CursorMoveEvent;//pozri MainWindowStavadlo.xaml.cs
        public void RaiseCursorMoveEvent(System.Drawing.Point curPoint)
        {
            CursorMoveEvent?.Invoke(curPoint);
        }


        public void RaiseElementPositionEvent(System.Windows.Point curPoint)
        {
            PlayModeElementPositionEvent?.Invoke(curPoint);
        }


    }
}
