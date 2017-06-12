using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Allax;
namespace Allax
{

}
namespace AllaxScript
{
    public class OutPut
    {
        private object syncRoot = new object();
        int counter = 0;
        public bool MyAddSolution(Solution S)
        {
            lock (syncRoot)
            {
                var W = S.Way;
                var P = S.P;
                Console.WriteLine(String.Format("Solution {0}. Prevalence: {1}. Active blocks count: {2}.",++counter, S.P.ToPrevalence(), S.P.ActiveBlocksCount));
                Console.WriteLine();
                for (int i = 0; i < W.layers.Count; i++)
                {
                    string O = "";
                    string I = "";
                    for (int j=0;j<W.layers[i].blocks.Count;j++)
                    {
                        var i_ = W.layers[i].blocks[j].active_inputs;
                        var o_ = W.layers[i].blocks[j].active_outputs;
                        for (int k=0;k<i_.Count;k++)
                        {
                            I += (i_[k]) ? '1' : '0';
                        }
                        for (int k = 0; k < i_.Count; k++)
                        {
                            O += (o_[k]) ? '1' : '0';
                        }
                    }
                    Console.WriteLine(I);
                    Console.WriteLine(O);
                }
                Console.WriteLine();
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
                    var I = Iter1.NextState();
//                     for (int i = 0; i < I.input.Count; i++)
//                     {
//                         Console.Write((I.input[i]) ? 1 : 0);
//                     }
//                     Console.WriteLine();
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
//             var Iter = new InputsIterator(4, 4);
//             while (!Iter.IsFinished())
//             {
//                 var I = Iter.NextState();
//                 for (int i = 0; i < I.input.Count; i++)
//                 {
//                     Console.Write((I.input[i]) ? 1 : 0);
//                 }
//                 Console.WriteLine();
//                 Console.ReadLine();
//             }
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
            AddFullRound(Net);
            AddLastRound(Net);
            List<byte> SBlockInit = new List<byte>{ 14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7 };
            var PBlockInit = new List<byte> { 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15, 4, 8, 12, 16 };
            InitSLayer(1, SBlockInit, Net);
            InitSLayer(4, SBlockInit, Net);
            InitPLayer(2, PBlockInit, Net);
            InitPLayer(5, PBlockInit, Net);
            var R1 = new Rule(AvailableSolverTypes.BaseSolver);
            var R2 = new Rule(AvailableSolverTypes.HeuristicSolver);
            var F = new Allax.CallbackAddSolution(OUT.MyAddSolution);
            var AP = new AnalisysParams(new Algorithm(new List<Rule> { /*R1,*/ R2 }, AnalisysType.Linear), F, 1);
            Net.PerformAnalisys(AP);
            Console.ReadLine();
        }
        static void InitPLayer(int L, List<byte> PBlockInit, ISPNet Net)
        {
            Net.GetLayers()[L].GetBlocks()[0].Init(PBlockInit);
        }
        static void InitSLayer(int L, List<byte> SBlockInit, ISPNet Net)
        {

            foreach (var B in Net.GetLayers()[L].GetBlocks())
            {
                B.Init(SBlockInit);
            }
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
