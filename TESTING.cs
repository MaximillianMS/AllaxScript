﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Allax;

namespace Allax
{

}
namespace AllaxScript
{
    public class Program
    {
        public class OutPut
        {
            private object syncRoot = new object();
            //int SolveCounter = 0;
            int TaskCounter = 0;
            private System.Collections.Concurrent.ConcurrentBag<Solution> Solutions = new System.Collections.Concurrent.ConcurrentBag<Solution>();
            public void TaskFinished(Task T)
            {
                lock (syncRoot)
                {
                    var Time = (T.Params.EndTime - T.Params.StartTime);
                    Console.WriteLine("Task {0} has been finished. Exec time: {1}.", ++TaskCounter, Time.ToString());
                    //Console.WriteLine(PrintWay(T.GetWay(), 2));
                }
            }
            public void ClearCounters()
            {
                Solutions = new System.Collections.Concurrent.ConcurrentBag<Solution>();
                //SolveCounter = 0;
                TaskCounter = 0;
            }
            public string PrintWay(SPNetWay W, int MaxLayers = -1)
            {
                if (MaxLayers == -1)
                {
                    MaxLayers = W.layers.Count;
                }
                var FullOut = "";
                for (int i = 0; i < W.layers.Count; i++)
                {
                    if (i > MaxLayers)
                    {
                        break;
                    }
                    string O = "";
                    string I = "";

                    switch (W.layers[i].type)
                    {
                        case LayerType.KLayer:
                            {
                                I += string.Format("K {0, 2}  IN:\t", i / 3 + 1);
                                O += string.Format("K {0, 2} OUT:\t", i / 3 + 1);
                                break;
                            }
                        case LayerType.SLayer:
                            {
                                I += string.Format("S {0, 2}  IN:\t", i / 3 + 1);
                                O += string.Format("S {0, 2} OUT:\t", i / 3 + 1);
                                break;
                            }
                        case LayerType.PLayer:
                            {
                                I += string.Format("P {0, 2}  IN:\t", i / 3 + 1);
                                O += string.Format("P {0, 2} OUT:\t", i / 3 + 1);
                                break;
                            }
                    }
                    for (int j = 0; j < W.layers[i].blocks.Count; j++)
                    {
                        var i_ = WayConverter.ToList(W.layers[i].blocks[j].Inputs, W.layers[i].blocks[j].BlockSize);
                        var o_ = WayConverter.ToList(W.layers[i].blocks[j].Outputs, W.layers[i].blocks[j].BlockSize);
                        for (int k = 0; k < i_.Count; k++)
                        {
                            I += (i_[k]) ? '1' : '0';
                            if ((k + 1) % W.layers[1].blocks[0].BlockSize == 0)
                                I += "\t";
                        }
                        for (int k = 0; k < i_.Count; k++)
                        {
                            O += (o_[k]) ? '1' : '0';
                            if ((k + 1) % W.layers[1].blocks[0].BlockSize == 0)
                                O += "\t";
                        }
                    }
                    FullOut += I + "\n" + O + "\n\n";
                }
                return FullOut;
            }
            public bool MyAddSolution(Solution S)
            {
                Solutions.Add(S);
                Console.WriteLine("Solution has been found. Prevalence: {1}. Active blocks count: {2}. Total solutions: {0}.", Solutions.Count, S.P.ToPrevalence(), S.P.ActiveBlocksCount);
                return true;
            }
            public void PrintSolution(Solution S, int SolveCounter)
            {
                var W = S.Way;
                var P = S.P;
                var FullOut = String.Format("Solution {0}. Prevalence: {1}. Active blocks count: {2}.", ++SolveCounter, S.P.ToPrevalence(), S.P.ActiveBlocksCount);
                FullOut += "\n\n";
                FullOut += PrintWay(W);
                Console.WriteLine(FullOut);
            }
            public void GetSolutions()
            {
                if (Solutions != null)
                {
                    if (Solutions.Count > 0)
                    {
                        Console.WriteLine("Solutions count: {0}", Solutions.Count);
                        var SortedSolutions = Solutions.ToList().OrderByDescending(x => Math.Abs(x.P.ToDelta())).ToList();
                        for (int i = 0; i < SortedSolutions.Count()&&i<3; i++)
                        {
                            var Sol = SortedSolutions[i];
                            PrintSolution(Sol, i);
                        }
                    }
                }
            }
            public void ExportDB()
            {
                if (E != null && SBDB != null)
                {
                    Console.WriteLine("Enter the file name:");
                    var Path = Console.ReadLine();
                    Path = "SBOXDB_" + Path;
                    Path = @"D:\" + Path + ".TimVoiMaxDB";
                    using (var aFile = new System.IO.FileStream(Path, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                    {
                        lock (syncRoot)
                        {
                            E.SerializeDB(aFile);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Engine and DB have not been created.");
                }
            }
            public void ImportDB()
            {
                if (E != null)
                {
                    Console.WriteLine("Enter the file name:");
                    var Path = Console.ReadLine();
                    Path = "SBOXDB_" + Path;
                    Path = @"D:\" + Path + ".TimVoiMaxDB";
                    using (var aFile = new System.IO.FileStream(Path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        lock (syncRoot)
                        {
                            SBDB = E.InjectSBlockDB(aFile);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Engine and DB have not been created.");
                }
            }
            public void PrintByteArray(byte[] bytes)
            {
                var sb = new StringBuilder("{ ");
                foreach (var b in bytes)
                {
                    sb.Append(b + ", ");
                }
                sb.Append("}");
                Console.WriteLine(sb.ToString());
            }

        }
        private static string ReadLine()
        {
            int READLINE_BUFFER_SIZE = 2048;
            var inputStream = Console.OpenStandardInput(READLINE_BUFFER_SIZE);
            byte[] bytes = new byte[READLINE_BUFFER_SIZE];
            int outputLength = inputStream.Read(bytes, 0, READLINE_BUFFER_SIZE);
            //Console.WriteLine(outputLength);
            char[] chars = Encoding.UTF7.GetChars(bytes, 0, outputLength);
            return new string(chars);
        }
        public static List<byte> GetInitialSequence(int length)
        {
            var ret = new List<byte>();
            while (ret.Count != length)
            {
                ret.Clear();
                Console.WriteLine("Enter initial sequence: ");
                var Seq = ReadLine();
                Console.WriteLine();
                Seq = Seq.Trim();
                string[] Values;
                if (Seq.Contains(","))
                {
                    Seq.Replace(" ", string.Empty);
                    Values = Seq.Split(',');
                }
                else
                {
                    if (Seq.Contains(";"))
                    {
                        Seq.Replace(" ", string.Empty);
                        Values = Seq.Split(';');
                    }
                    else
                        Values = Seq.Split(' ');
                }
                for (int i = 0; i < Values.Length; i++)
                {
                    if (Values[i] != "")
                        ret.Add(Convert.ToByte(Values[i]));
                }
            }
            return ret;
        }
        public struct PredefinedInitKey
        {
            public override bool Equals(object obj)
            {
                var R = (PredefinedInitKey)obj;
                return (R.Length == Length) && (R.BoxType == BoxType);
            }
            public override int GetHashCode()
            {
                return Length.GetHashCode() ^ BoxType.GetHashCode();
            }
            public int Length;
            public Allax.LayerType BoxType;
        }
        static public Dictionary<PredefinedInitKey, List<byte>> PredefinedInits = new Dictionary<PredefinedInitKey, List<byte>> {
            { new PredefinedInitKey {Length = 16, BoxType = LayerType.SLayer }, new List<byte> { 14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7 } },
            { new PredefinedInitKey {Length = 256, BoxType = LayerType.SLayer },  new List<byte> { 35, 183, 46, 247, 18, 34, 78, 125, 81, 21, 198, 128, 200, 117, 52, 195, 174, 85, 86, 248, 141, 13, 227, 5, 40, 149, 178, 224, 134, 65, 249, 24, 142, 132, 173, 169, 138, 235, 214, 193, 108, 3, 79, 176, 166, 225, 102, 194, 20, 140, 47, 103, 70, 208, 95, 241, 152, 171, 88, 187, 137, 26, 181, 167, 153, 157, 9, 201, 17, 146, 73, 123, 93, 58, 53, 242, 226, 206, 160, 188, 243, 75, 237, 16, 66, 139, 236, 59, 136, 175, 252, 83, 10, 42, 190, 147, 251, 131, 0, 221, 203, 14, 104, 151, 150, 165, 62, 69, 61, 255, 124, 25, 158, 7, 38, 122, 97, 29, 60, 170, 106, 189, 156, 155, 45, 196, 2, 64, 1, 145, 50, 23, 240, 216, 213, 63, 87, 22, 186, 68, 27, 28, 191, 82, 118, 244, 8, 228, 101, 230, 209, 233, 12, 44, 182, 133, 211, 115, 94, 161, 67, 105, 154, 98, 109, 121, 177, 4, 33, 48, 253, 111, 56, 32, 205, 49, 218, 54, 127, 204, 210, 71, 120, 185, 250, 114, 223, 254, 231, 219, 15, 113, 238, 163, 207, 234, 245, 179, 143, 212, 107, 19, 77, 43, 55, 246, 239, 215, 37, 57, 126, 164, 41, 168, 30, 172, 110, 232, 90, 202, 192, 220, 76, 39, 148, 84, 130, 229, 11, 96, 112, 100, 217, 6, 116, 31, 119, 91, 144, 199, 36, 89, 99, 180, 222, 197, 135, 92, 51, 162, 129, 159, 72, 80, 184, 74 } },
            {new PredefinedInitKey {Length = 16, BoxType = LayerType.PLayer }, new List<byte> { 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15, 4, 8, 12, 16 } },
            {new PredefinedInitKey {Length = 64, BoxType=LayerType.PLayer }, new List<byte> { 7, 62, 57, 55, 37, 30, 31, 10, 14, 59, 16, 58, 29, 53, 8, 48, 49, 26, 32, 54, 13, 4, 1, 2, 43, 33, 40, 24, 39, 36, 12, 50, 42, 22, 21, 64, 63, 51, 6, 3, 46, 61, 5, 27, 28, 60, 15, 41, 23, 17, 11, 45, 52, 9, 20, 19, 44, 47, 34, 18, 35, 25, 56, 38 } }
        };
        public static void Menu1_2_2_1(LayerType Type)
        {
            var BL = (Type == LayerType.SLayer) ? 1 << Net.GetSettings().SBoxSize : Net.GetSettings().WordLength;
            var BlockInit = new List<byte>();
            int mode = 0;
            while (mode != 1 && mode != 2)
            {
                Console.WriteLine("Make all boxes the same? [1 - Yes, 2 - Nope]");
                mode = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine();
            }
            if (mode == 1)
            {
                int predefined = 0;
                var Key = new PredefinedInitKey { Length = BL, BoxType = Type };
                if (PredefinedInits.ContainsKey(Key))
                {
                    while (predefined != 1 && predefined != 2)
                    {
                        Console.WriteLine("Use predefined init? [1 - Yes, 2 - Nope]");
                        predefined = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine();
                    }
                }
                if (predefined == 1)
                {
                    PredefinedInits.TryGetValue(Key, out BlockInit);
                }
                else
                {
                    BlockInit = GetInitialSequence(BL);
                }
            }
            for (int i = 0; i < (Net.GetLayers().Count / 3 - 1); i++)
            {
                var L = Net.GetLayers()[3 * i + ((Type == LayerType.SLayer) ? 1 : 2)];
                if (L.GetLayerType() == Type)
                {
                    for (int j = 0; j < L.GetBlocks().Count; j++)
                    {
                        Console.WriteLine(String.Format("{1}-Layer {0, 2}:", i + 1, (((Type == LayerType.SLayer) ? "S" : "P"))));
                        L.GetBlocks()[j].Init((mode == 1) ? BlockInit : GetInitialSequence(BL));
                        if (mode == 1)
                        {
                            output.PrintByteArray(BlockInit.ToArray());
                        }
                    }
                }
            }
        }
        public static void Menu1_2_2()
        {
            int AT = 0;
            while (AT != 1 && AT != 2)
            {
                Console.WriteLine("Which type of layer you wanna init? [1 - S-box, 2 - P-box]");
                AT = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine();
            }
            Menu1_2_2_1((AT == 1) ? LayerType.SLayer : LayerType.PLayer);

        }
        public static AnalisysParams GetAnalisysParams(AnalisysType Type)
        {
            var ret = new AnalisysParams();
            ret.MaxThreads = 0;
            while(!((ret.MaxThreads>0)&&(ret.MaxThreads<32)))
            {
                Console.WriteLine("Enter threads count: ");
                ret.MaxThreads = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine();
            }
            ret.Alg = new Algorithm();
            ret.Alg.Type = Type;
            ret.Alg.Rules = new List<Rule>();
            bool exit = false;
            while(!exit)
            {
                Console.WriteLine("1 - Add Rule\n" + "2 - Print Rules\n" + "3 - Delete Rule\n" + "4 - Finish\n");
                var A = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine();
                switch (A)
                {
                    case 1:
                        {
                            ret.Alg.Rules.Add(GetRule());
                            break;
                        }
                    case 2:
                        {
                            for(int i=0;i<ret.Alg.Rules.Count;i++)
                            {
                                Console.WriteLine("Rule #{0}\r\n"+ret.Alg.Rules[i], i);
                            }
                            break;
                        }
                    case 3:
                        {
                            int ind = -1;
                            while (!(ind < ret.Alg.Rules.Count && ind >= 0))
                            {
                                Console.WriteLine("Enter Rule index: ");
                                ind = Convert.ToInt32(Console.ReadLine());
                            }
                            ret.Alg.Rules.RemoveAt(ind);
                            break;
                        }
                    case 4:
                        {
                            exit = true;
                            break;
                        }
                }

            }
            return ret;
        }

        public static Rule GetRule()
        {
            var R = new Allax.Rule(((Func<AvailableSolverTypes>)(
                () =>
            {
                int ind = 0;
                while (ind != 1 && ind != 2)
                {
                    Console.WriteLine("Choose Solver: \n1 - {0}\n2 - {1}", AvailableSolverTypes.BaseSolver, AvailableSolverTypes.HeuristicSolver);
                    ind = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine();
                }
                return (ind == 1) ? AvailableSolverTypes.BaseSolver : AvailableSolverTypes.HeuristicSolver;
            }))(), ((Func<int>)(
            () =>
            {
                int MaxActive = 0;
                while (!(MaxActive > 0 && MaxActive < (Net.GetSettings().SBoxCount+1)))
                {
                    Console.WriteLine("Enter maximum of active S-boxes in one layer:");
                    MaxActive = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine();
                }
                return MaxActive;
            }))());
            int mode = 0;
            while (mode != 1 && mode != 2)
            {
                Console.WriteLine("Do you wanna use custom input? [1 - Yes, 2 - Nope]");
                mode = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine();
            }
            if(mode==1)
            {
                R.UseCustomInput = true;
                R.Input = new SolverInputs(WayConverter.ToLong(GetInitialSequence(Net.GetSettings().WordLength).ConvertAll(x=>(x==1))), Net.GetSettings().WordLength);
            }
            return R;
        }

        public static void Menu1_2()
        {
            string M = "1 - Add round\n" +
                    "2 - Init Layer\n" +
                    "3 - Show Net Settings\n" +
                    "4 - Perform linear analysis\n" +
                    "5 - Perform differential analysis\n";
            Console.WriteLine(M);
            var K = Console.ReadLine();
            Console.WriteLine();
            switch (K)
            {
                case "1":
                    {
                        int count = 0;
                        while (!(count > 0 && count <= 64))
                        {
                            Console.WriteLine("Enter how much rounds you want to add:");
                            count = Convert.ToInt16((Console.ReadLine()));
                        }
                        foreach (var i in Enumerable.Range(0, count))
                            AddRound(Net);
                        break;
                    }
                case "2":
                    {
                        Menu1_2_2();
                        break;
                    }
                case "3":
                    {
                        Console.WriteLine("Word length: {1}. S-box size: {2}. S-box count: {0}. Full rounds : {3}.\n", Net.GetSettings().SBoxCount, Net.GetSettings().WordLength, Net.GetSettings().SBoxSize, Net.GetLayers().Count/3-1);
                        Console.WriteLine("Layer stack:");
                        for(int i=0;i<Net.GetLayers().Count;i++)
                        {
                            switch(Net.GetLayers()[i].GetLayerType())
                            {
                                case LayerType.KLayer:
                                    {
                                        Console.WriteLine("K-Layer");
                                        break;
                                    }
                                case LayerType.SLayer:
                                    {
                                        Console.WriteLine("S-Layer");
                                        break;
                                    }
                                case LayerType.PLayer:
                                    {
                                        Console.WriteLine("P-Layer");
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                case "4":
                    {
                        output.ClearCounters();
                        E.PerformAnalisys(GetAnalisysParams(AnalisysType.Linear));
                        break;
                    }
                case "5":
                    {
                        output.ClearCounters();
                        E.PerformAnalisys(GetAnalisysParams(AnalisysType.Differencial));
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

        }
        public static void Menu1_CreateNet()
        {
            int SL = 0;
            while (!((SL <= 8) && (SL >= 4)))
            {
                Console.WriteLine("Enter S-box length [4-8]: ");
                SL = Convert.ToInt32(Console.ReadLine());
            }
            int WL = 1;
            while (!((WL/SL == WL/(double)SL)&&(WL<=64)))
            {
                Console.WriteLine("Enter whole word length which must be divisible by S-box length: ");
                WL = Convert.ToInt32(Console.ReadLine());
            }
            var Settings = new Allax.SPNetSettings((byte)WL, (byte)SL);
            Net = E.CreateSPNetInstance(Settings);
            SBDB = E.GetSBlockDBInstance();
            AddFullRound(Net);
            AddLastRound(Net);
        }
        public static void Menu1()
        {
            bool exit = false;
            while (!exit)
            {
                try
                {
                    string M =  "1 - Create Net\n" +
                                "2 - Manage Net\n" +
                                "3 - Export DB to file\n" +
                                "4 - Import DB\n"+
                                "5 - Exit\n" +
                                "6 - Get results\n" +
                                "8 - Abort analisys\n";
                    Console.WriteLine(M);
                    var K = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine();
                    switch (K)
                    {
                        case 1:
                            {
                                Menu1_CreateNet();
                                break;
                            }
                        case 2:
                            {
                                if (Net != null)
                                    Menu1_2();
                                break;
                            }
                        case 3:
                            {
                                output.ExportDB();
                                break;
                            }
                        case 4:
                            {
                                output.ImportDB();
                                break;
                            }
                        case 5:
                            {
                                exit = true;
                                break;
                            }
                        case 6:
                            {
                                output.GetSolutions();
                                break;
                            }
                        case 8:
                            {
                                E.AbortAnalisys();
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                    Console.WriteLine();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine();
                }
            }

        }
        public static IEngine E;
        public static ISPNet Net;
        public static ISBlockDB SBDB;
        public static OutPut output;
        public static void Main()
        {
            /*
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
                        var AP = new AnalisysParams(new Algorithm(new List<Rule> { / *R1,* / R2 }, AnalisysType.Linear), F, 1);
                        //Net.PerformAnalisys(AP);*/
            output = new OutPut();
            E = new Engine(new EngineSettings(output.MyAddSolution));
            E.TASKDONE += output.TaskFinished;
            Menu1();
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
        static void AddRound(Allax.ISPNet Net)
        {
            DelLastRound(Net);
            AddFullRound(Net);
            AddLastRound(Net);
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
        static void DelLastRound(Allax.ISPNet Net)
        {
            Net.DeleteLayer((byte)(Net.GetLayers().Count-1));
            Net.DeleteLayer((byte)(Net.GetLayers().Count - 1));
            Net.DeleteLayer((byte)(Net.GetLayers().Count - 1));
        }
    }
}
