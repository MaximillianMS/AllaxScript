using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Allax;
using System.Threading;

namespace CellAllax
{
    class Cell
    {
        public int Value;
        public int Index;
        public List<int> NeighboursIndexes = new List<int>();
        public List<Cell> Neighbours = new List<Cell>();
    }
    class Graph : ICloneable
    {
        public Graph()
        {

        }
        public Graph(string[] SplittedNodeList)
        {
            Cells = new List<Cell>(SplittedNodeList.Length);
            Cells.AddRange(Enumerable.Range(0, SplittedNodeList.Length).Select(i => new Cell() { Index = i }));
            for (int i = 0; i < SplittedNodeList.Length; i++)
            {
                var S = SplittedNodeList[i];
                var splittedValues = S.Split(',');
                foreach (var strIndex in splittedValues)
                {
                    var intIndex = Convert.ToInt32(strIndex);
                    Cells[i].Neighbours.Add(Cells[intIndex]);
                    Cells[i].NeighboursIndexes.Add(intIndex);
                }
            }
        }
        public Cell this[int i]
        {
            get { return Cells[i]; }
            set { Cells[i] = value; }
        }
        public List<Cell> Cells = new List<Cell>();

        public object Clone()
        {
            var NewCells = Cells.Select(i => new Cell() { Value = i.Value, Index = i.Index, NeighboursIndexes = new List<int>(i.NeighboursIndexes) }).ToList();
            foreach (var Cell in NewCells)
            {
                Cell.Neighbours = new List<Cell>();
                for (int i = 0; i < Cell.NeighboursIndexes.Count; i++)
                {
                    Cell.Neighbours.Add(NewCells[Cell.NeighboursIndexes[i]]);
                }
            }
            return new Graph() { Cells = NewCells };
        }
    }
    class ANF
    {
        int VarCount;
        public ANF(int VarCount)
        {
            this.VarCount = VarCount;
        }
        List<Conjunction> Conjunctions = new List<Conjunction>();
        class Conjunction
        {
            public List<int> VarLogPows;
            public int GetConjunction(List<int> VarValues)
            {
                return Enumerable.Range(0, VarLogPows.Count).Aggregate(1, (acc, x) => (acc * ((VarLogPows[x] == 1) ? VarValues[x] : 1)));
            }
        }
        public int GetResult(List<int> VarValues)
        {
            return Enumerable.Range(0, Conjunctions.Count).Aggregate((acc, x) => ((acc + Conjunctions[x].GetConjunction(VarValues)) & 1));
        }
        public void AddConjuction(List<int> VarLogPows)
        {
            Conjunctions.Add(new Conjunction() { VarLogPows = VarLogPows });
        }
    }
    class LocalFunc
    {
        ANF Func;
        int VarCount;
        public LocalFunc(int VarCount)
        {
            this.VarCount = VarCount;
            Func = new ANF(VarCount);
            FuncMatrix = new Dictionary<long, int>(Enumerable.Range(0, 1 << VarCount).Select(i => new KeyValuePair<long, int>(i, 0)).ToDictionary(i => i.Key, i => i.Value));
        }
        public LocalFunc(int VarCount, string strANF) : this(VarCount)
        {
            var strConjunctions = strANF.Trim().Replace(" ", "").Split('+');
            foreach (var strConjunction in strConjunctions)
            {
                if (strConjunction != "")
                {
                    if (strConjunction.Length > 1)
                    {
                        var strVars = strConjunction.Split('x', 'X');
                        var VarLogPows = new List<int>(Enumerable.Repeat(0, VarCount));
                        foreach (var strVar in strVars)
                        {
                            if (strVar != "")
                            {
                                var Var = Convert.ToInt32(strVar);
                                VarLogPows[Var - 1] = 1;
                            }
                        }
                        Func.AddConjuction(new List<int>(VarLogPows));
                    }
                    else
                    {
                        Func.AddConjuction(new List<int>(Enumerable.Repeat(0, VarCount)));
                    }
                }
            }
            FuncMatrix = new Dictionary<long, int>(1 << VarCount);
            for (long i = 0; i < ((long)1 << VarCount); i++)
            {
                FuncMatrix.Add(i, Func.GetResult(WayConverter.ToList(i, VarCount).Select(j => (j) ? 1 : 0).ToList()));
            }
        }
        public int GetResult(List<int> Values)
        {
            if (Values.Count == VarCount)
            {
                var index = (int)WayConverter.ToLong(Values.Select(i => (i == 1) ? true : false).ToList());
                return FuncMatrix[index];
            }
            else
                throw new NotImplementedException();
        }
        Dictionary<long, int> FuncMatrix;
    }
    class CA : ICloneable
    {
        Graph G;
        LocalFunc F;
        public delegate int LocalFunc(List<int> Values);
        public void ResetValues()
        {
            foreach(var N in G.Cells)
            {
                N.Value = 0;
            }
        }
        public CA(Graph G, LocalFunc F)
        {
            this.G = G;
            this.F = F;
        }
        public void SetValues(List<int> Values)
        {
            for (int i = 0; i < Values.Count; i++)
                G.Cells[i].Value = Values[i];
        }
        public void AppendValues(List<int> Values)
        {
            for (int i = 0; i < Values.Count; i++)
                G.Cells[i].Value = (G.Cells[i].Value + Values[i]) & 1;
        }
        public List<int> GetValues()
        {
            return G.Cells.Select(i => i.Value).ToList();
        }
        public int GetNextStepCellValue(int CellIndex)
        {
            return F(G.Cells[CellIndex].Neighbours.Select(i => i.Value).ToList());
        }
        public object Clone()
        {
            var newG = G.Clone();
            return new CA((Graph)newG, F);
        }
        public Cell this[int i]
        {
            get
            {
                return G[i];
            }
            set
            {
                G[i] = value;
            }
        }
        public void NextStep(int Steps = 1, bool MultiThread = false)
        {
            var New = NextStep(this, Steps, MultiThread);
            this.G = New.G;
        }
        public static CA NextStep(CA Orig, int Steps = 1, bool MultiThread = false)
        {
            for (int s = 0; s < Steps; s++)
            {
                var NextStepCA = (CA)Orig.Clone();
                if (MultiThread)
                {
                    var E = new EngineForTaskerAndWorker();
                    E.TheTasker = new NextStepTasker();
                    ((NextStepTasker)E.TheTasker).CellCount = Orig.G.Cells.Count;
                    ((NextStepTasker)E.TheTasker).OrigAutomata = Orig;
                    ((NextStepTasker)E.TheTasker).NewAutomata = NextStepCA;
                    E.TheWorker = new SignalWorker(new WorkerParams(E, Environment.ProcessorCount));
                    E.TheWorker.AsyncRun();
                    ((SignalWorker)E.TheWorker).oSignalEvent.WaitOne();
                    E.TheWorker.Dispose();
                }
                else
                {
                    for (int i = 0; i < Orig.G.Cells.Count; i++)
                    {
                        NextStepCA[i].Value = Orig.GetNextStepCellValue(i);
                    }
                }
                Orig = NextStepCA;
            }
            return Orig;
        }
        public int GetCellCount()
        {
            return G.Cells.Count;
        }
    }
    class FeistelNet
    {
        class FeistelNetRound
        {
            public List<int> InL;
            public List<int> InR;
            public List<int> OutL;
            public List<int> OutR;
            public FeistelNetRound(CA Automata, int AutomataSteps, List<int> InL, List<int> InR, List<int> Key, List<int> Constant)
            {
                this.InL = new List<int>((InL != null) ? InL : new List<int>());
                this.InR = new List<int>((InR != null) ? InR : new List<int>());
                this.Key = new List<int>((Key != null) ? Key : new List<int>());
                this.Constant = new List<int>((Constant != null) ? Constant : new List<int>());
                this.Automata = (CA)Automata.Clone();
                this.AutomataSteps = AutomataSteps;
            }
            public void ProcessRound(bool LastRound = false)
            {
                if ((InR.Count + Key.Count + Constant.Count) > Automata.GetCellCount())
                    throw new Exception("Input length for CA more than cells it contains.");
                Automata.ResetValues();
                OutL = new List<int>(InR);
                Automata.SetValues(InR.Concat(Key).Concat(Constant).ToList());
                Automata.NextStep(AutomataSteps);
                var automataResult = Automata.GetValues();
                var automataResultCutted = automataResult.Skip(automataResult.Count - InL.Count).ToList();
                OutR = new List<int>(Enumerable.Range(0, InL.Count).Select(i => ((InL[i] + automataResultCutted[i]) & 1)));
                if (LastRound)
                {
                    var temp = OutL;
                    OutL = OutR;
                    OutR = temp;
                }
            }
            public CA Automata;
            public List<int> Key;
            public List<int> Constant;
            public int AutomataSteps;

        }
        public CA Automata; // Origin automata
        public List<int> MasterKey;
        public List<List<int>> RoundConstants;
        List<List<int>> RoundKeys;
        List<FeistelNetRound> Rounds;
        private int RoundsCount;
        private int AutomataSteps;
        private void GetRoundKeys()
        {
            RoundKeys = new List<List<int>>
            {
                (from i in Enumerable.Range(0, MasterKey.Count / 2) select MasterKey[i]).ToList(),
                (from i in Enumerable.Range(MasterKey.Count / 2, MasterKey.Count / 2) select MasterKey[i]).ToList(),
                (from i in Enumerable.Range(0, MasterKey.Count/2) select MasterKey[(i+MasterKey.Count/4)%(MasterKey.Count/2)]).ToList(),
                (from i in Enumerable.Range(0, MasterKey.Count/2) select MasterKey[(i+MasterKey.Count/4)%(MasterKey.Count/2)+MasterKey.Count/2]).ToList(),
            };
        }
        public FeistelNet(CA Automata, int AutomataSteps, int RoundsCount, List<int> MasterKey, List<List<int>> RoundConstants)
        {
            this.Automata = (Automata!=null)?(CA)Automata.Clone():null;
            this.MasterKey = (MasterKey!=null)?new List<int>(MasterKey):new List<int>();
            this.RoundConstants = (RoundConstants!=null)?new List<List<int>>(RoundConstants.Select(i => new List<int>(i))): new List<List<int>>();
            this.RoundsCount = RoundsCount;
            this.AutomataSteps = AutomataSteps;
            GetRoundKeys();
        }
        public void SetMasterKey(List<int> Key)
        {
            this.MasterKey = new List<int>(Key);
            GetRoundKeys();
        }
        private void ProcessRounds()
        {
            for (int i = 0; i < RoundsCount; i++)
            {
                if (i != RoundsCount - 1)
                {
                    Rounds[i].ProcessRound();
                    Rounds[i + 1].InL = Rounds[i].OutL;
                    Rounds[i + 1].InR = Rounds[i].OutR;
                }
                else
                {
                    Rounds[i].ProcessRound(true);
                }
            }
        }
        private void PrepareRounds(List<int> OpenText, bool Encrypt)
        {
            Rounds = new List<FeistelNetRound>(RoundsCount);
            Rounds.AddRange(from i in Enumerable.Range(0, RoundsCount) select new FeistelNetRound(Automata, AutomataSteps, null, null, 
                RoundKeys[(Encrypt) ? i : (RoundsCount - i - 1)], RoundConstants[(Encrypt) ? i : (RoundsCount - i - 1)]));
            Rounds[0].InL = (from i in Enumerable.Range(0, OpenText.Count / 2) select OpenText[i]).ToList();
            Rounds[0].InR = (from i in Enumerable.Range(OpenText.Count / 2, OpenText.Count / 2) select OpenText[i]).ToList();
        }
        public List<int> Encrypt(List<int> OpenText)
        {
            PrepareRounds(OpenText, true);
            ProcessRounds();
            return Rounds[RoundsCount-1].OutL.Concat(Rounds[RoundsCount-1].OutR).ToList();
        }
        public List<int> Decrypt(List<int> CipherText)
        {
            PrepareRounds(CipherText, false);
            ProcessRounds();
            return Rounds[RoundsCount - 1].OutL.Concat(Rounds[RoundsCount - 1].OutR).ToList();
        }
    }
    class CACryptor
    {
        FeistelNet FN;
        List<List<int>> Constants;
        List<int> MasterKey;
        int BlockLength = 128;
        int KeyLength;
        int RoundsCount;
        int ConstLength;
        int AutomataSteps;
        static StringBuilder ReadFromFile(string GraphsPath, int nodes)
        {
            var sb = new StringBuilder();
            using (FileStream aFile = new FileStream(GraphsPath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(aFile))
                {
                    while (!sr.EndOfStream)
                    {
                        var temp = sr.ReadLine();
                        if (temp.Contains(':'))
                        {
                            if (!temp.Contains("diameter"))
                            {
                                if (Convert.ToInt32(temp.Substring(0, temp.IndexOf(':'))) == nodes)
                                {
                                    sb.AppendLine(temp); // nodeCount:
                                    sr.ReadLine();
                                    sb.AppendLine(sr.ReadLine()); // diameter:
                                    sr.ReadLine();
                                    sb.AppendLine(sr.ReadLine()); // NodeList
                                }
                            }
                        }
                    }
                }
            }
            return sb;
        }
        static List<int> GetRandomBitList(int Length, int WeightRequirement = -1)
        {
            if (Length <= 0)
                return new List<int>();
            var ret = new List<int>(Enumerable.Repeat(0, Length));
            var retIndex = new List<int>(Enumerable.Range(0, Length));
            var IntRangom = new Random();
            WeightRequirement = (WeightRequirement == -1 || WeightRequirement > Length) ? IntRangom.Next(Length) + 1 : WeightRequirement;
            for (int i = 0; i < WeightRequirement; i++)
            {
                var Index = IntRangom.Next(retIndex.Count);
                ret[retIndex[Index]] = 1;
                retIndex.RemoveAt(Index);
            }
            if (ret.Sum() == WeightRequirement)
                return ret;
            else
                throw new Exception();
        }
        public CACryptor(int AutomataSteps, int KeyLength, string strLocalFunc= "x1x3x5+x3x4+x5x6+x3x5+x1x5+x1+x2+1", int LocalFuncVarCount = 6 , int RoundsCount = 4, string GraphsPath = @"..\..\graphs-diam.txt")
        {
            this.AutomataSteps = AutomataSteps;
            this.RoundsCount = RoundsCount;
            this.KeyLength = KeyLength;
            int nodes;
            if (this.KeyLength == 128)
                nodes = 182;
            else
            {
                if (this.KeyLength == 256)
                    nodes = 282;
                else
                    throw new NotImplementedException();
            }
            var NodeList = ReadFromFile(GraphsPath, nodes);
            if (NodeList.ToString() == "")
                throw new NotImplementedException();
            var SplittedStr = NodeList.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)[2].Trim().TrimStart('[').TrimEnd(']').Replace(" ", string.Empty).Split(new string[] { "],[" }, StringSplitOptions.None);
            var G = new Graph(SplittedStr);
            var LF = new LocalFunc(LocalFuncVarCount, strLocalFunc);
            var OrigCA = new CA(G, LF.GetResult);
            this.ConstLength = nodes - this.KeyLength / 2 - BlockLength / 2;
            FN = new FeistelNet(OrigCA, AutomataSteps, RoundsCount, MasterKey, Constants);
            SetRandomConstants();
            SetRandomKey();
        }
        public void SetKey(List<bool> Key)
        {
            if (Key==null || Key.Count != KeyLength)
                throw new NotImplementedException();
            MasterKey = Key.ConvertAll(i => Convert.ToInt32(i));
            FN.SetMasterKey(MasterKey);
        }
        public void SetRandomKey()
        {
            SetKey(GetRandomBitList(KeyLength).ConvertAll(i => Convert.ToBoolean(i)));
        }
        public void SetConstants(List<List<bool>> Constants)
        {
            if(Constants==null|| Constants.Count!=RoundsCount)
            {
                throw new NotImplementedException();
            }
            else
            {
                if (Constants[0].Count != ConstLength)
                    throw new NotImplementedException();
                this.Constants = new List<List<int>>((from i in Constants select i.ConvertAll(j => Convert.ToInt32(j))));
                FN.RoundConstants = this.Constants;
            }
        }
        public void SetRandomConstants()
        {
            SetConstants(new List<List<bool>>(Enumerable.Range(0, RoundsCount).Select(i => GetRandomBitList(ConstLength, (((ConstLength & 1) == 0) ? (ConstLength / 2) : (ConstLength / 2 + 1))).ConvertAll(j=>Convert.ToBoolean(j)))));
        }
        public List<bool> Encrypt(List<bool> OpenText)
        {
            return FN.Encrypt(OpenText.ConvertAll(i=>Convert.ToInt32(i))).ConvertAll(j=>Convert.ToBoolean(j));
        }
        public List<bool> Decrypt(List<bool> CipherText)
        {
            return FN.Decrypt(CipherText.ConvertAll(i => Convert.ToInt32(i))).ConvertAll(i=>Convert.ToBoolean(i));
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var CACr = new CACryptor(4, 128);
            var OT = (from i in WayConverter.ToList(0xDDAABBAAC, 64).Concat(WayConverter.ToList(0xCACACADAB, 64)) select Convert.ToInt32(i)).ToList().ConvertAll(i => Convert.ToBoolean(i));
            var CT = CACr.Encrypt(OT);
            Console.WriteLine("{0,16:X}, {1,16:X}", WayConverter.ToLong((from i in Enumerable.Range(0, CT.Count / 2) select (CT[i])).ToList()), WayConverter.ToLong((from i in Enumerable.Range(CT.Count / 2, CT.Count / 2) select (CT[i])).ToList()));
            var OT2 = CACr.Decrypt(CT);
            Console.WriteLine("{0,16:X}, {1,16:X}", WayConverter.ToLong((from i in Enumerable.Range(0, OT2.Count / 2) select (OT2[i])).ToList()), WayConverter.ToLong((from i in Enumerable.Range(OT2.Count / 2, OT2.Count / 2) select (OT2[i])).ToList()));
            Console.ReadLine();
        }
    }
}
