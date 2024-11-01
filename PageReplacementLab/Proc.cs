using System;
using System.Collections.Generic;

namespace PageReplacementLab
{
    public class Proc
    {
        public PageTableEntry[] PageTable;
        private int[] workingSet;
        private int reqCount;
        private readonly int reqLimit;
        private readonly Random rnd = new Random();

        public bool Completed => reqCount >= reqLimit;

        public Proc()
        {
            int tableSize = rnd.Next(10, 31); 
            PageTable = new PageTableEntry[tableSize];
            for (int i = 0; i < tableSize; i++) PageTable[i] = new PageTableEntry();

            workingSet = GenerateWorkingSet((int)(tableSize * 0.3), tableSize); 
            reqLimit = rnd.Next(1000, 1501); 
        }

        public void AccessPages(int minReq, int maxReq, MMU mmu)
        {
            int req = rnd.Next(minReq, maxReq + 1);
            for (int i = 0; i < req; i++)
            {
                int idx = (rnd.Next(100) < 90) ? workingSet[rnd.Next(workingSet.Length)] : rnd.Next(PageTable.Length);
                bool isWrite = rnd.Next(100) < 30;
                mmu.AccessPage(PageTable, idx, isWrite);
                reqCount++;
            }
        }

        private int[] GenerateWorkingSet(int size, int maxIdx)
        {
            var set = new HashSet<int>();
            while (set.Count < size) set.Add(rnd.Next(0, maxIdx));
            return new List<int>(set).ToArray();
        }
    }
}
