using System;

namespace PA.Stavadlo.Infrastructure
{
    //MH: Januar  2019
    //pomocne funkcie pre aplikaciu
    public class StavadloHelper
    {
        /// <summary>
        /// Uprava nazvu premennej z Logicu;
        /// Ak sa nameFromLogic zacina cislom, potom sa prida znak _, aby control mal ten isty name ako pouzivame vo VS, vo VS sa control Name nemoze zacinat cislom;
        /// </summary>
        /// <param name="nameFromLogic"></param>
        /// <returns>upraveny, alebo povodny nazov kontrolu</returns>
        public static string GetElementRealName(string nameFromLogic)  //v prijatom telegrame sa name moze zacinat cislom, napr. 860
        {
            if (string.IsNullOrEmpty(nameFromLogic))
                return string.Empty;

            if (char.IsDigit(nameFromLogic[0]))//ak name zacina cislom, pridam znak _, aby som mal ten isty name ako pouzivame my 
                nameFromLogic = "_" + nameFromLogic;
            return nameFromLogic;
        }

        /// <summary>
        /// Int16 zmeni na char
        /// </summary>
        /// <param name="podTyp"></param>
        /// <returns>char pre vstupny podtTyp</returns>
        public static char GetPodTypZnak(Int16 podTyp)
        {
            char podtypZnak = 'x';
            switch (podTyp)
            {
                case 0:
                    podtypZnak = 'N'; //Navestidlo
                    break;
                case 1:
                    podtypZnak = 'n'; //izolovany kolaj. usek
                    break;
                case 2:
                    podtypZnak = '0'; //neizolovany kolaj. usek
                    break;
                case 3:
                    podtypZnak = 'v'; //kolaj. usek patriaci vyhybke
                    break;
                case 4:
                    podtypZnak = 's'; //suhlas
                    break;
                case 5:
                    podtypZnak = 'p';   //pomocny signal
                    break;
                default: break;
            }
            return podtypZnak;
        }

        /// <summary>
        /// char podTyp zmeni na Int16
        /// </summary>
        /// <param name="podtyp"></param>
        /// <returns></returns>
        public static short GetPodTyp(char podtyp)
        {
            short typElementu = 255;
            switch (podtyp) //podtyp potrebujeme preniest do CombineStatus, preto to transformujem do Int16
            {
                case 'N'://Navestidlo
                    typElementu = (Int16)0;
                    break;
                case 'n': //izolovany kolaj. usek
                    typElementu = (Int16)1;
                    break;
                case '0': //neizolovany kolaj. usek
                    typElementu = (Int16)2;
                    break;
                case 'v': //kolaj. usek patriaci vyhybke
                    typElementu = (Int16)3;
                    break;
                case 's': //suhlas
                    typElementu = (Int16)4;
                    break;
                case 'p':   //pomocny signal pre KONTR.NAPAJ
                    typElementu = (Int16)5;
                    break;
                default: break;
            }
            return typElementu;
        }

        //status    stav    uvolIzol   vyluka       podtyp
        //bity:     63-48    47-32      31-16        15-0
        /// <summary>
        /// Vrati kombinovane stavove slovo typu Int64 pre prvok stavadla, ktore je vytvorene zo styroch Int16;
        /// </summary>
        /// <param name="stav"></param>
        /// <param name="uvolIzol"></param>
        /// <param name="vyluka"></param>
        /// <param name="podTyp"></param>
        /// <returns>kombinovane stavove slovo</returns>
        public static Int64 CreateCombineStatus(Int16 stav, Int16 uvolIzol, Int16 vyluka, Int16 podTyp)
        {
            Int64 s1 = (Int64)stav << 48;
            Int64 s2 = (Int64)uvolIzol << 32;
            Int64 s3 = (Int64)vyluka << 16;
            Int64 s4 = (Int64)podTyp;
            Int64 status = s1 | s2 | s3 | s4;
            return status;
        }

        /*
        /// <summary>
        /// Rozparsuje Int64 na 4 Int16 hodnoty
        /// </summary>
        /// <param name="combineStatus"></param>
        /// <returns>new Tuple<Int16, Int16, Int16, Int16>(stav, uvolIzol, vyluka, podTyp)</returns>
        public static Tuple<Int16,  Int16, Int16, Int16> GetStatusValues(Int64 combineStatus)
        {
            Int16 stav=0;
            Int16 uvolIzol=0;
            Int16 vyluka=0;
            Int16 podTyp=0;

            stav = (Int16)(combineStatus >> 48);
            uvolIzol = (Int16)((combineStatus & 0x0000_FFFF_0000_0000) >> 32);
            vyluka = (Int16)((combineStatus & 0x0000_0000_FFFF_0000) >> 16);

            podTyp = (Int16)(combineStatus & 0x0000_0000_0000_FFFF);
            //return new Tuple<Int16, Int16, Int16, Int16>(stav, uvolIzol, vyluka, podTyp);
            return  Tuple.Create<Int16, Int16, Int16, Int16>(stav, uvolIzol, vyluka, podTyp);
        }
        */
        /* Pouzitie
         *  Tuple<Int16, Int16, Int16, Int16> t1 = StavadloHelper.GetStatusValues(status);
            Int16 s = t1.Item1; //stav
            Int16 u = t1.Item2; //uvolIzol
            Int16 v = t1.Item3; //vyluka
            Int16 p = t1.Item4; //podtyp
         */

        //Nuget: treba pridat System.ValueTuple
        public static (Int16 stav, Int16 uvolIzol, Int16 vyluka, Int16 podTyp) GetStatusValues2(Int64 combineStatus)
        {
            Int16 stav = 0;
            Int16 uvolIzol = 0;
            Int16 vyluka = 0;
            Int16 podTyp = 0;
            stav = (Int16)(combineStatus >> 48);
            uvolIzol = (Int16)((combineStatus & 0x0000_FFFF_0000_0000) >> 32);

            vyluka = (Int16)((combineStatus & 0x0000_0000_FFFF_0000) >> 16);
           
            podTyp = (Int16)(combineStatus & 0x0000_0000_0000_FFFF);

            return (stav, uvolIzol, vyluka, podTyp);
        }
        /*  Pouzitie
         *  (Int16 stav, Int16 uvolIzol, Int16 vyluka, Int16 podTyp) = StavadloHelper.GetStatusValues2(status);
         */
    }
}
