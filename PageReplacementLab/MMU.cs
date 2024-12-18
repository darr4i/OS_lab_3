namespace PageReplacementLab
{
    public class MMU
    {
        private readonly Kernel kernel;
        public int PageAccessNum { get; private set; }

        public MMU(Kernel kernel)
        {
            this.kernel = kernel;
            PageAccessNum = 0;
        }

      public void AccessPage(PageTableEntry[] pageTable, int idx, bool isWrite, bool useNRU)
    {
        PageAccessNum++;
        if (!pageTable[idx].P)
        {
         kernel.PageFaultHandler(pageTable, idx, useNRU);
        }

        pageTable[idx].R = true;
     if (isWrite) pageTable[idx].M = true;
    }


        }
    }

