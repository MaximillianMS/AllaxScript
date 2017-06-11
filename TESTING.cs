﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Allax;
namespace AllaxScript
{
    public class OutPut
    {
        private object syncRoot = new object();
        public bool MyAddSolution(Solution S)
        {
            lock (syncRoot)
            {
                var W = S.Way;
                var P = S.P;
                return true;
            }
        }
    }
    public class Program
    {
        static void CheckSMTH()
        {
            int MaxIter = 0;
            foreach (var bl in Enumerable.Range(4, 5))
            {
                var Iter1 = new InputsIterator(64/bl, bl);
                int CurIter = 0;
                while(!Iter1.IsFinished())
                {
                    Iter1.NextState();
                    CurIter++;
                }
                Console.WriteLine(String.Format("Block Length: {0}. Block Count: {1}. Total Iterations: {2}.", bl, 64 / bl, CurIter));
                if(MaxIter<CurIter)
                {
                    MaxIter = CurIter;
                }

            }
            Console.WriteLine(MaxIter);
            return;
            var Iter = new InputsIterator(4, 4);
            while (!Iter.IsFinished())
            {
                var I = Iter.NextState();
                for (int i = 0; i < I.input.Count; i++)
                {
                    Console.Write((I.input[i]) ? 1 : 0);
                }
                Console.WriteLine();
                Console.ReadLine();
            }
            ;
        }
        public static void Main()
        {
            CheckSMTH();
            Allax.IEngine E= new Engine();
            var OUT = new OutPut();
            var SBDB = E.GetSBlockDBInstance();
            var Settings = new Allax.SPNetSettings(16, 4, SBDB);
            var Net = E.GetSPNetInstance(Settings);
            AddFullRound(Net);
            AddLastRound(Net);
            List<byte> SBlockInit = new List<byte>{ 14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7 };
            foreach(var B in Net.GetLayers()[1].GetBlocks())
            {
                B.Init(SBlockInit);
            }
            var PBlockInit = new List<byte> { 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15, 4, 8, 12, 16 };
            Net.GetLayers()[2].GetBlocks()[0].Init(PBlockInit);
            var R1 = new Rule(AvailableSolverTypes.BaseSolver);
            var R2 = new Rule(AvailableSolverTypes.HeuristicSolver);
            var F = new Allax.CallbackAddSolution(OUT.MyAddSolution);
            var AP = new AnalisysParams(new Algorithm(new List<Rule> { R1, R2 }, AnalisysType.Linear), F, 1);
            Net.PerformAnalisys(AP);
            Console.ReadLine();
        }

        static void AddFullRound(Allax.ISPNet Net)
        {
            Net.AddLayer(Allax.LayerType.KLayer);
            Net.AddLayer(Allax.LayerType.SLayer);
            Net.AddLayer(Allax.LayerType.PLayer);
        }
        static void AddLastRound(Allax.ISPNet Net)
        {
            Net.AddLayer(Allax.LayerType.KLayer);
            Net.AddLayer(Allax.LayerType.SLayer);
            Net.AddLayer(Allax.LayerType.KLayer);
        }
    }
}
