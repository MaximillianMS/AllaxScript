using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Allax;
using System.IO;

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
            int counter = 0;
            int TaskCounter = 0;
            public void TaskFinished(Task T)
            {
                lock(syncRoot)
                {
                    Console.WriteLine("Task {0} has been finished.", ++TaskCounter);
                    //Console.WriteLine(PrintWay(T.GetWay(), 2));
                }
            }
            public void ClearCounter()
            {
                counter = 0;
                TaskCounter = 0;
            }
            public string PrintWay(SPNetWay W, int MaxLayers=-1)
            {
                if (MaxLayers==-1)
                {
                    MaxLayers = W.layers.Count;
                }
                var FullOut = "";
                for (int i = 0; i < W.layers.Count; i++)
                {
                    if(i>MaxLayers)
                    {
                        break;
                    }
                    string O = "";
                    string I = "";

                    switch (W.layers[i].type)
                    {
                        case LayerType.KLayer:
                            {
                                I += string.Format("K-Layer {0, 2}  IN:\t", i/3 + 1);
                                O += string.Format("K-Layer {0,2} OUT:\t", i/3 + 1);
                                break;
                            }
                        case LayerType.SLayer:
                            {
                                I += string.Format("S-Layer {0, 2}  IN:\t", i/3 + 1);
                                O += string.Format("S-Layer {0, 2} OUT:\t", i/3 + 1);
                                break;
                            }
                        case LayerType.PLayer:
                            {
                                I += string.Format("P-Layer {0, 2}  IN:\t", i/3 + 1);
                                O += string.Format("P-Layer {0, 2} OUT:\t", i/3 + 1);
                                break;
                            }
                    }
                    for (int j = 0; j < W.layers[i].blocks.Count; j++)
                    {
                        var i_ = W.layers[i].blocks[j].active_inputs;
                        var o_ = W.layers[i].blocks[j].active_outputs;
                        for (int k = 0; k < i_.Count; k++)
                        {
                            I += (i_[k]) ? '1' : '0';
                            if ((k + 1) % W.layers[1].blocks[0].active_inputs.Count == 0)
                                I += "\t";
                        }
                        for (int k = 0; k < i_.Count; k++)
                        {
                            O += (o_[k]) ? '1' : '0';
                            if ((k + 1) % W.layers[1].blocks[0].active_outputs.Count == 0)
                                O += "\t";
                        }
                    }
                    FullOut += I + "\n" + O + "\n\n";
                }
                return FullOut;
            }
            public bool MyAddSolution(Solution S)
            {
                lock (syncRoot)
                {
                    var W = S.Way;
                    var P = S.P;
                    var FullOut = String.Format("Solution {0}. Prevalence: {1}. Active blocks count: {2}.", ++counter, S.P.ToPrevalence(), S.P.ActiveBlocksCount);
                    FullOut += "\n\n";
                    FullOut += PrintWay(W);
                    Console.WriteLine(FullOut);
                    return true;
                }
            }
        }
        private static string ReadLine()
        {
            int READLINE_BUFFER_SIZE = 2048;
            Stream inputStream = Console.OpenStandardInput(READLINE_BUFFER_SIZE);
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
        public static void Menu1_2_2_1()
        {
            var SBL= Net.GetSettings().word_length / Net.GetSettings().sblock_count;
            List<byte> SBlockInit = new List<byte> { 14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7 };
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
                if ((1<<SBL)==SBlockInit.Count)
                {
                    while (predefined != 1 && predefined != 2)
                    {
                        Console.WriteLine("Use predefined init? [1 - Yes, 2 - Nope]");
                        predefined = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine();
                    }
                }
                SBlockInit=(predefined==1)?SBlockInit:GetInitialSequence(1<<SBL);
            }
            for (int i = 0; i < (Net.GetLayers().Count / 3 - 1); i++)
            {
                for(int j=0;j<Net.GetLayers()[3*i+1].GetBlocks().Count;j++)
                {
                    Net.GetLayers()[3 * i + 1].GetBlocks()[j].Init((mode==1)?SBlockInit:GetInitialSequence(SBL));
                }
            }

        }
        public static void Menu1_2_2_2()
        {
            var SBL = Net.GetSettings().word_length;
            List<byte> BlockInit = new List<byte> { 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15, 4, 8, 12, 16 };
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
                if (SBL == BlockInit.Count)
                {
                    while (predefined != 1 && predefined != 2)
                    {
                        Console.WriteLine("Use predefined init? [1 - Yes, 2 - Nope]");
                        predefined = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine();
                    }
                }
                BlockInit = (predefined == 1) ? BlockInit : GetInitialSequence(SBL);
            }
            for (int i = 0; i < (Net.GetLayers().Count / 3 - 1); i++)
            {
                for (int j = 0; j < Net.GetLayers()[3 * i + 2].GetBlocks().Count; j++)
                {
                    Net.GetLayers()[3 * i + 2].GetBlocks()[j].Init((mode == 1) ? BlockInit : GetInitialSequence(SBL));
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
            switch(AT)
            {
                case 1:
                    {
                        Menu1_2_2_1();
                        break;
                    }
                case 2:
                    {
                        Menu1_2_2_2();
                        break;
                    }
            }

        }
        public static AnalisysParams GetAnalisysParams(AnalisysType Type)
        {
            var ret = new AnalisysParams();
            ret.TaskFinishedFunc = output.TaskFinished;
            ret.AddSolution = output.MyAddSolution;
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
                while (!(MaxActive > 0 && MaxActive < (Net.GetSettings().sblock_count+1)))
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
                R.Input = new SolverInputs(GetInitialSequence(Net.GetSettings().word_length).ConvertAll(x=>(x==1)));
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
                        Console.WriteLine("Word length: {1}. S-box size: {2}. S-box count: {0}. Full rounds : {3}.\n", Net.GetSettings().sblock_count, Net.GetSettings().word_length, Net.GetSettings().word_length / Net.GetSettings().sblock_count, Net.GetLayers().Count/3-1);
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
                        output.ClearCounter();
                        Net.PerformAnalisys(GetAnalisysParams(AnalisysType.Linear));
                        break;
                    }
                case "5":
                    {
                        output.ClearCounter();
                        Net.PerformAnalisys(GetAnalisysParams(AnalisysType.Differencial));
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
            E = new Engine();
            SBDB = new SBlockDB();
            var Settings = new Allax.SPNetSettings((byte)WL, (byte)(WL/SL), SBDB);
            Net = E.GetSPNetInstance(Settings);
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
                    string M = "1 - Create Net\n" +
                                "2 - Manage Net\n" +
                                "3 - Exit\n";
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
                                exit = true;
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
        public static ISBlockDB SBDB;
        public static ISPNet Net;
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
