using System;

namespace Stavadlo22.Infrastructure.ObsadenieKolaje
{
    //MH: april 2019
    /*
    * Po rozparsovani telegramu c. 191 by sme mali dostat napr.
    * Len useky zacinajuce znakom K maju user control UC_ObsadenieKolaje!!!!!
       01#998U#01.03.2019#10:37:15#0
       02#891 #01.03.2019#10:36:09#0
       03#804 #01.03.2019#10:23:26#0
       04#887 #01.03.2019#10:13:09#0
       05#383 #01.03.2019#10:02:43#0
       06#383 #01.03.2019#10:01:51#0
       07#870 #01.03.2019#02:15:40#0
       08#891D#01.03.2019#01:36:25#0
       09#892D#01.03.2019#08:23:11#0
       10#384 #01.03.2019#06:58:33#0
       11#894V#01.03.2019#10:37:38#0
       12#998U#01.03.2019#10:37:19#0
       13#870B#01.03.2019#06:51:02#0
       14#893V#01.03.2019#08:38:51#0
       15#384 #01.03.2019#06:59:09#0
       16#870B#01.03.2019#06:50:13#0
       17#870B#01.03.2019#06:50:54#0
       18#888 #01.03.2019#10:43:12#0
       19#863 #01.03.2019#09:39:12#0
       20#972 #20.02.2019#11:25:37#0
       21#972 #25.02.2019#07:54:54#0
    */
    /// <summary>
    /// Udaje pre zobrazenie supravy na kolajovom useku
    /// </summary>
    public sealed class SupravaData
    {
        public UInt16 Cislo_supravy { get; set; }
        public string Kolaj { get; set; }
        public string Datum { get; set; }
        public string Cas { get; set; }
        public UInt16 Priorita { get; set; }
    }
}
