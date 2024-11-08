using System;
using System.Collections.Generic;
using System.Linq;

namespace PageReplacementLab
{
    public class Kernel
    {
        public List<PhysPage> PhysPageFreeList = new List<PhysPage>();
        public List<PhysPage> PhysPageBusyList = new List<PhysPage>();
        public int PageFaultNum { get; private set; }
        private readonly Random random = new Random();

        public MMU Mmu { get; }

        public Kernel(int physPages)
        {
            for (int i = 0; i < physPages; i++)
            {
                PhysPage pp = new PhysPage { PPN = i };
                PhysPageFreeList.Add(pp);
            }
            PageFaultNum = 0;

            Mmu = new MMU(this);
        }

        public void PageFaultHandler(PageTableEntry[] pageTable, int idx, bool useNRU)
        {
            PageFaultNum++;
            PhysPage physPage = PhysPageFreeList.Count > 0 ? GetFreePage() : SelectPageForReplacement(useNRU);

            physPage.PageTable = pageTable;
            physPage.Idx = idx;
            pageTable[idx].P = true;
            pageTable[idx].PPN = physPage.PPN;
        }

        private PhysPage GetFreePage()
        {
            var physPage = PhysPageFreeList[0];
            PhysPageFreeList.RemoveAt(0);
            PhysPageBusyList.Add(physPage);
            return physPage;
        }

        private PhysPage SelectPageForReplacement(bool useNRU)
        {
            return useNRU ? NRUReplacement() : RandomReplacement();
        }

        private PhysPage RandomReplacement()
        {
            int idx = random.Next(PhysPageBusyList.Count);
            var selectedPage = PhysPageBusyList[idx];
            Console.WriteLine($"Page replacement: [PPN: {selectedPage.PPN}] (Random)");
            return selectedPage;
        }

        public PhysPage NRUReplacement()
        {
             List<PhysPage> class0 = new List<PhysPage>(); // R=0, M=0
             List<PhysPage> class1 = new List<PhysPage>(); // R=0, M=1
             List<PhysPage> class2 = new List<PhysPage>(); // R=1, M=0
             List<PhysPage> class3 = new List<PhysPage>(); // R=1, M=1

             foreach (var page in PhysPageBusyList)
            {
                var pageEntry = page.PageTable[page.Idx];

                 if (!pageEntry.R && !pageEntry.M)
                 class0.Add(page); // R=0, M=0
                else if (!pageEntry.R && pageEntry.M)
                 class1.Add(page); // R=0, M=1
                 else if (pageEntry.R && !pageEntry.M)
                 class2.Add(page); // R=1, M=0
                 else
                class3.Add(page); // R=1, M=1
             }

            if (class0.Count > 0) return class0[0];
            if (class1.Count > 0) return class1[0];
            if (class2.Count > 0) return class2[0];
            return class3.Count > 0 ? class3[0] : null;
        }


        public void FreeProc(Proc proc)
        {
            foreach (var entry in proc.PageTable)
            {
                if (entry.P)
                {
                    var physPage = PhysPageBusyList.FirstOrDefault(p => p.PPN == entry.PPN);

                    if (physPage != null)
                    {
                        PhysPageBusyList.Remove(physPage);
                        PhysPageFreeList.Add(physPage);
                    }
                }
            }
        }

        public void UpdatePageStats()
        {
            foreach (var page in PhysPageBusyList)
            {
                if (page.PageTable != null && page.PageTable.Length > page.Idx)
                {
                    page.PageTable[page.Idx].R = false; 
                }
            }
        }
    }
}
