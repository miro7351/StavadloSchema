namespace Stavadlo22.Infrastructure
{
    #region Documentation
    /// <summary
    /// Obsahuje udaje pre 1 rezim
    /// <remarks>
    /// <para>
    /// Class Info: udaje pre popis rezimu
    /// <list type="bullet">
    /// <item name="author">Author: RNDr. M. Hrabcak, CSc.</item>
    /// <item name="date">July 2013</item>
    /// <item name="email">hrabcak@procaut.sk</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// </summary>
    #endregion
    public class Rezim
    {

        public Rezim(string name, string kodRezimu, char level, char kod)
        {
            this.Name = name;
            this.KodRezimu = kodRezimu;
            this.Level = level;
            this.Kod = kod;

        }

        /// <summary>
        /// nazov rezimu; napr.VYLUKA_PRESRAVENIA
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// text pre telegram do servera; napr. Vyluka prestavenia
        /// </summary>
        public string KodRezimu
        {
            get;
            private set;
        }

        /// <summary>
        /// uroven-level: level pre Admina-2, level pre Dispecera-0, level pre Udrzbu-1
        /// </summary>
        public char Level
        {
            get;
            private set;
        }

        /// <summary>
        /// Kod rezimu
        /// </summary>
        public char Kod
        {
            get;
            private set;
        }

    }//class Rezim
}
