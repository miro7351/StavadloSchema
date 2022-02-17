namespace PA.Stavadlo.MH.Controls
{
    //MH: oktober 2018
    //trieda pouzita pre EventAgrs pre zapnutie/vypnutie kontrolu do stavu blikania/neblikania
    public class BlinkedControl
    {
        public BlinkedControl()
        {

        }
        public BlinkedControl(string nazov, int stavBlikania)
        {
            ControlName = nazov;
            State = stavBlikania;

        }
        public string ControlName
        {
            get; set;
        }

        /// <summary>
        /// 0-neblikajuci stav; 1-blikajuci stav
        /// </summary>
        public int State
        { get; set; }
    }
}
