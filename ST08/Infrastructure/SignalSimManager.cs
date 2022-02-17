
using Stavadlo22.Infrastructure.Communication;

namespace Stavadlo22.Infrastructure
{
    /// <summary>
    /// Simulovanie stlacenia (zapinania a vypinania) buttonov: AUT/RUC Den/Noc RZV VPP SPR VR
    /// Stav buttonov je od 15.10.2013 nastavovany z telegramu, potom sa pouzivaju objekty: NAPAJ a KONTR
    /// AUT/RUC nestlaceny stav:AUT    RUC stlaceny stav - cervene pozadie:
    /// RUC - ak je stav RUC, potom menu buttony Udrzba, Vymeny, Vcesta, Pcesta Zrus, Suhlas, STOP su nepristupne!! PFunkcie je pristupny
    /// Den/Noc nemeni farbu pozadia pri stlaceni!!
    /// </summary>
    public class SignalSimManager
    {
        public SignalSimManager()
        {
            EventHandlerEnabled = false;
            //napojenie na event ak Logic vyslal telegram 121
            //App.AppCommunicator.MessageData1Recieved += new Communication.ServerData1Delegate(AppCommunicator_MessageData1Recieved);

            //napojenie na event ak Logic vyslal spravu v telegrame 161
            //App.AppCommunicator.LogicMessageReceived += AppCommunicator_LogicMessageReceived;
        }

        void AppCommunicator_LogicMessageReceived(object sender, EventErrorArgsLogic e)
        {
            string message = e.message;
        }



        private void AppCommunicator_MessageData1Recieved(object sender, EventArgs1 e)
        {
            string message = e.recievedMessage.ToLower();
        }

        /// <summary>
        /// button na ktory sa kliklo
        /// </summary>
        public System.Windows.Controls.Button ClickedButton
        {
            get;
            set;
        }

        public string GetButtonName()
        {
            return (ClickedButton == null) ? null : ClickedButton.Name;
        }

        /// <summary>
        /// povoluje/zakazuje zmenu stavu buttonov
        /// </summary>
        public bool EventHandlerEnabled
        {
            get;
            set;
        }
    }
}
