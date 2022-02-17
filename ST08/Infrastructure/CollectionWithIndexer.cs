using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Stavadlo22.Infrastructure.MVVM;

namespace Stavadlo22.Infrastructure
{
    /* Pozri aj StavadloModel.cs
     * Pouzitie typu CollectionWithIndexer: Pre prvok stavadla napr. vymenu  mozeme nastavit udaje pre vykreslenie vymeny takto
     * Property  STC typu CollectionWithIndexer a jej polozku kde Nazov="1124" bindujeme v xaml priamo na prvok stavadla!!!!
     * Pri zapise do Dependency Property TsStatus sa spusti funkcia, ktora zabezpeci vykreslenie vymeny
     *  <controls:TrainSwitch64 x:Name="V1124" 
        Margin="425,74,0,0" 
        TsStatus ="{Binding STC[V1124].CombineStatus, Mode=OneWay}"
        InvertArm="False"
        Part01="{Binding ElementName=V1124p1}"
        Part02="{Binding ElementName=V1124p2}"/>
        Pri prijme telegramu sa udaje z telegramu zapisu do kolekcie STC.
     */

    //MH: September 2018
    /// <summary>
    /// Kolekcia s indexerom
    /// </summary>
    public class CollectionWithIndexer
    {
        //private readonly ObservableCollection<PrvokStavadla> localCollection; 
        private readonly List<PrvokStavadla> localCollection;
        public CollectionWithIndexer()
        {
            //localCollection = new ObservableCollection<PrvokStavadla>();  //OK
            localCollection = new List<PrvokStavadla>();    //OK!!!!
        }

        public IEnumerable<PrvokStavadla> GetCollection()
        {
            return localCollection;
        }

        //public List<PrvokStavadla> GetCollection()
        //{
        //    return localCollection;
        //}

        public PrvokStavadla this[string key]
        {
            get
            {
                return localCollection.Where(p => p.Nazov == key).FirstOrDefault();
            }
        }

        public void Add(PrvokStavadla prvok)
        {
            localCollection.Add(prvok);
            return;
        }

        public bool Contains(PrvokStavadla prvok)
        {
            if (localCollection.Contains(prvok))
                return true;
            else
                return false;
        }

        public int Count => localCollection.Count; //expression bodied readonly property
        //{
        //    get
        //    {
        //        return localCollection.Count;
        //    }
        //}
    }

    /// <summary>
    /// Obsahuje udaje pre zobrazenie prvku na mape stavadla
    /// </summary>
    public class PrvokStavadla : BaseMVVM
    {
        public PrvokStavadla()
        {
        }

        public PrvokStavadla(string nazov, char podtyp, Int16 stav, Int16 vyluka, Int16 uvolIzol)
        {
            this.Nazov = nazov;
            this.Podtyp = podtyp;
            this.Stav = stav;
            this.Vyluka = vyluka;
            this.Uvolizol = uvolIzol;
            this.CombineStatus = (Int64)0;
        }

        string nazov;//nazov prvku na mape stavadla, napr. K464, V801, S228, L229c, _800....
        char podtyp; //typ prvku na mape stavadla: n-kolaj. usek pre navestidlo, v-vyhybka(vymena), o-patri k obvodu, O-kolaj. usek neizolovany, s-suhlas, p-pomocny signal ak je typ suhlas

        Int16 stav;
        Int16 vyluka;
        Int16 uvolizol;

        /// <summary>
        /// Nazov prvku na mape stavadla
        /// </summary>
        public string Nazov
        {
            get
            {
                return nazov;
            }
            set
            {
                if (value == nazov)
                    return;
                nazov = value;
                OnPropertyChanged(nameof(Nazov));
            }
        }

        public char Podtyp
        {
            get
            {
                return podtyp;
            }
            set
            {
                if (value == podtyp)
                    return;
                podtyp = value;
                OnPropertyChanged(nameof(Podtyp));
            }
        }

        /// <summary>
        /// Uvolnenie izolacie
        /// </summary>
        public Int16 Uvolizol
        {
            get
            {
                return uvolizol;
            }
            set
            {
                if (value == uvolizol)
                    return;
                uvolizol = value;
                OnPropertyChanged(nameof(Uvolizol));
            }
        }

        /// <summary>
        /// Udaj o vyluke prvku
        /// </summary>
        public Int16 Vyluka
        {
            get
            {
                return vyluka;
            }
            set
            {
                if (value == vyluka)
                    return;
                vyluka = value;
                OnPropertyChanged(nameof(Vyluka));
            }
        }

        /// <summary>
        /// Udaj o stave prvku
        /// </summary>
        public Int16 Stav
        {
            get
            {
                return stav;
            }
            set
            {
                if (value == stav)
                    return;
                stav = value;
                OnPropertyChanged(nameof(Stav));
            }
        }

        long combineStatus;//Int64
        /// <summary>
        /// obsahuje 4 Int16 hodnoty: stav, uvolIzol,  vyluka, typElementu
        /// </summary>
        public long CombineStatus
        {
            get
            {
                return combineStatus;
            }
            set
            {
                if (value == combineStatus)
                    return;
                combineStatus = value;
                OnPropertyChanged(nameof(CombineStatus));
            }
        }

        public override string ToString()
        {
            return $"{Nazov};{Stav};{Podtyp.ToString()};{vyluka};{uvolizol}";
        }
    }
}
