using System;
using System.Collections.Generic;

namespace PageReplacementLab
{
    public class Program
    {
        public const int PHYS_PAGES = 100; 
        public const int INIT_PROCS = 3;   
        public const int MAX_PROCS = 10;   
        public const int MIN_REQ = 30;     
        public const int MAX_REQ = 50;     

        static readonly Kernel kernel = new Kernel(PHYS_PAGES); 
        static readonly List<Proc> procQueue = new List<Proc>();

        static void Main(string[] args)
        {
            CreateInitialProcs(INIT_PROCS);

            while (true)
            {
                ProcessProcs();
            }
        }

        static void CreateInitialProcs(int count)
        {
            for (int i = 0; i < count; i++)
            {
                CreateNewProc();
            }
        }

        static void ProcessProcs()
        {
            List<Proc> completedProcs = new List<Proc>();

            foreach (var proc in procQueue)
            {
                proc.AccessPages(MIN_REQ, MAX_REQ, kernel.Mmu);

                if (proc.Completed)
                {
                    kernel.FreeProc(proc);
                    completedProcs.Add(proc);
                }
            }

                foreach (var proc in completedProcs)
            {
                procQueue.Remove(proc);
            }


            if (procQueue.Count < MAX_PROCS)
            {
                CreateNewProc();
            }

            kernel.UpdatePageStats();
        }

        static void CreateNewProc()
        {
            Proc newProc = new Proc();
            procQueue.Add(newProc);
        }
    }
}