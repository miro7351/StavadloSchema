using System;
using System.Linq;
using System.Collections.Generic;

namespace Stavadlo22.Infrastructure.PlayMode
{
    public class PlayBuffer
    {
        private int maxRecords;
        private List<String> list;
        //private int currentRecord;
        private int backwardOffset; //indikujue ako hlboko sme pri citani zo zasobnika

        public int BackwadOffset
        {
            get { return backwardOffset; }
        }

        public PlayBuffer(int maxRecords)
        {
            this.maxRecords = maxRecords;
            list = new List<string>(maxRecords);
            backwardOffset = 0;
        }

        public void Push(string record)
        {
            if (list.Count == maxRecords)
                list.RemoveAt(0);
            list.Add(record);
        }

        public string Pop()
        {
            if (CanPop)
            {
                backwardOffset++;
                string record = list.ElementAt(list.Count - 1 - backwardOffset);
                //list.RemoveAt(list.Count - 1);    //nemozeme odstranovat, lebo potom by sme nevedeli ist dobredu..ako sa pojde 5krat spat a tieto zaznamy sa odstrania stream je 5 zaznamov vpredu  
                return record;
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// Ak pouzivatel isiel dozadu, dopredu musi ist cez tuto metodu... inak stream vrati zaznam tam kde skoncil (bez ohladu na posun spat)
        /// </summary>
        /// <returns></returns>
        public string GetFromOffset()
        {
            if (backwardOffset > 0)
            {
                string record = list.ElementAt(list.Count - backwardOffset);
                backwardOffset--;
                return record;
            }
            return null;
        }

        public bool CanPop
        {
            get { return (list.Count - 1 - backwardOffset) != 0; }
        }

        public void Flush()
        {
            list.Clear();
            backwardOffset = 0;
        }

    }
}
