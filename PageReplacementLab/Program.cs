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
        public const int MAX_CYCLES = 100; 

        static readonly Kernel kernel = new Kernel(PHYS_PAGES); 
        static readonly List<Proc> procQueue = new List<Proc>();

        static void Main(string[] args)
        {
            CreateInitialProcs(INIT_PROCS);
            int cycle = 0;

            while (cycle < MAX_CYCLES)
            {
                ProcessProcs();
                cycle++;

                if (cycle % 10 == 0)
                {
                    PrintStatistics(cycle);
                }
            }

            Console.WriteLine("Програма завершила виконання.");
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
                Console.WriteLine($"Процес завершено та видалено. Загальна кількість процесів: {procQueue.Count}");
            }

            if (procQueue.Count < MAX_PROCS)
            {
                CreateNewProc();
                Console.WriteLine($"Новий процес створено. Загальна кількість процесів: {procQueue.Count}");
            }

            kernel.UpdatePageStats();
        }

        static void CreateNewProc()
        {
            Proc newProc = new Proc();
            procQueue.Add(newProc);
            Console.WriteLine($"Створено новий процес. Загальна кількість процесів: {procQueue.Count}");
        }

        public static void PrintStatistics(int cycle)
        {
            double pageFaultRatio = kernel.PageFaultNum > 0
                ? (double)kernel.PageFaultNum / (cycle * procQueue.Count * MAX_REQ) * 100
                : 0;

            Console.WriteLine($"\n--- Статистика після циклу {cycle} ---");
            Console.WriteLine($"Загальна кількість сторінкових промахів: {kernel.PageFaultNum}");
            Console.WriteLine($"Співвідношення сторінкових промахів: {pageFaultRatio:F2}%");
            Console.WriteLine($"Кількість вільних фізичних сторінок: {kernel.PhysPageFreeList.Count}");
            Console.WriteLine($"Кількість зайнятих фізичних сторінок: {kernel.PhysPageBusyList.Count}\n");
        }
    }
}
