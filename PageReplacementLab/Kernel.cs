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

        public void PageFaultHandler(PageTableEntry[] pageTable, int idx)
        {
            PageFaultNum++;
            PhysPage physPage = PhysPageFreeList.Count > 0 ? GetFreePage() : SelectPageForReplacement();

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

        private PhysPage SelectPageForReplacement()
        {
            return random.Next(2) == 0 ? RandomReplacement() : NRUReplacement();
        }

        private PhysPage RandomReplacement()
        {
            int idx = random.Next(PhysPageBusyList.Count);
            var selectedPage = PhysPageBusyList[idx];
            Console.WriteLine($"Page replacement: [PPN: {selectedPage.PPN}] (Random)");
            return selectedPage;
        }

        private PhysPage NRUReplacement()
        {
            var candidates = PhysPageBusyList
                .Where(page => page.PageTable != null && page.PageTable.Length > page.Idx && !page.PageTable[page.Idx].R)
                .ToList();

            PhysPage selectedPage;
            if (candidates.Count > 0)
            {
                selectedPage = candidates[random.Next(candidates.Count)];
                Console.WriteLine($"Page replacement: [PPN: {selectedPage.PPN}] (NRU)");
            }
            else
            {
                selectedPage = RandomReplacement();
            }

            return selectedPage;
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
