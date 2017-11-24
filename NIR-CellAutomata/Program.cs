using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Allax;
using Allax.Cryptography;
using System.Threading;

namespace Allax.Cryptography
{
    class Cell
    {
        public int Value;
        public int Index;
        public List<int> NeighboursIndexes = new List<int>();
        public List<Cell> Neighbors = new List<Cell>();
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
                    Cells[i].Neighbors.Add(Cells[intIndex]);
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
                Cell.Neighbors = new List<Cell>();
                for (int i = 0; i < Cell.NeighboursIndexes.Count; i++)
                {
                    Cell.Neighbors.Add(NewCells[Cell.NeighboursIndexes[i]]);
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
        public Graph G;
        public LocalFunc F;
        public delegate int LocalFunc(List<int> Values);
        public void ResetValues()
        {
            foreach (var N in G.Cells)
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
            return F(G.Cells[CellIndex].Neighbors.Select(i => i.Value).ToList());
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
            this.Automata = (Automata != null) ? (CA)Automata.Clone() : null;
            this.MasterKey = (MasterKey != null) ? new List<int>(MasterKey) : new List<int>();
            this.RoundConstants = (RoundConstants != null) ? new List<List<int>>(RoundConstants.Select(i => new List<int>(i))) : new List<List<int>>();
            this.RoundsCount = RoundsCount;
            this.AutomataSteps = AutomataSteps;
            GetRoundKeys();
        }
        public void SetMasterKey(List<int> Key)
        {
            this.MasterKey = new List<int>(Key);
            GetRoundKeys();
        }
        public void AssignAutomata(CA Aut)
        {
            this.Automata = Aut;
            if (Rounds != null)
                Rounds.ForEach(r => r.Automata = Aut);
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
            return Rounds[RoundsCount - 1].OutL.Concat(Rounds[RoundsCount - 1].OutR).ToList();
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
        public FeistelNet FN;
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
        public CACryptor(int AutomataSteps, int KeyLength, string strLocalFunc = "x1x3x5+x3x4+x5x6+x3x5+x1x5+x1+x2+1", int LocalFuncVarCount = 6, int RoundsCount = 4, string GraphsPath = @"..\..\graphs-diam.txt")
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
            var SplittedStr = NodeList.ToString().Split(
                new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries
                )[2].Trim().TrimStart('[').TrimEnd(']').Replace(" ", string.Empty).Split(new string[] { "],[" }, StringSplitOptions.None);
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
            if (Key == null || Key.Count != KeyLength)
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
            if (Constants == null || Constants.Count != RoundsCount)
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
            SetConstants(new List<List<bool>>(Enumerable.Range(0, RoundsCount).Select(i => GetRandomBitList(ConstLength, (((ConstLength & 1) == 0) ? (ConstLength / 2) : (ConstLength / 2 + 1))).ConvertAll(j => Convert.ToBoolean(j)))));
        }
        public List<bool> Encrypt(List<bool> OpenText)
        {
            return FN.Encrypt(OpenText.ConvertAll(i => Convert.ToInt32(i))).ConvertAll(j => Convert.ToBoolean(j));
        }
        public List<bool> Decrypt(List<bool> CipherText)
        {
            return FN.Decrypt(CipherText.ConvertAll(i => Convert.ToInt32(i))).ConvertAll(i => Convert.ToBoolean(i));
        }
    }
    class HK
    {
        static int MAX = 100001;
        static int NIL = 0;
        static int INF = (1 << 28);
        public List<List<int>> G;
        public int n, m; public int[] match = new int[MAX], dist=new int[MAX];
        // n - узлов слева (1..n)
        // m - узлов справа (n+1..n+m)
        bool bfs()
        {
            int i, u, v, len;
            Queue<int> Q=new Queue<int>();

            for (i = 1; i <= n; i++)
            {
                if (match[i] == NIL)
                {
                    dist[i] = 0;
                    Q.Enqueue(i);
                }
                else
                    dist[i] = INF;
            }

            dist[NIL] = INF;

            while (Q.Count!=0)
            {
                u = Q.Dequeue();
                if (u != NIL)
                {
                    len = G[u].Count;
                    for (i = 0; i < len; i++)
                    {
                        v = G[u][i];
                        if (dist[match[v]] == INF)
                        {
                            dist[match[v]] = dist[u] + 1;
                            Q.Enqueue(match[v]);
                        }
                    }
                }
            }
            return (dist[NIL] != INF);
        }
        bool dfs(int u)
        {
            int i, v, len;
            if (u != NIL)
            {
                len = G[u].Count;
                for (i = 0; i < len; i++)
                {
                    v = G[u][i];
                    if (dist[match[v]] == dist[u] + 1)
                    {
                        if (dfs(match[v]))
                        {
                            match[v] = u;
                            match[u] = v;
                            return true;
                        }
                    }
                }
                dist[u] = INF;
                return false;
            }
            return true;
        }
        public int hopcroft_karp()
        {
            int matching = 0, i;

            while (bfs())
                for (i = n; i >= 1; i--)
                    if (match[i] == NIL && dfs(i))
                        matching++;
            return matching;
        }
    }
    public static class Ext
    {
        public static void Add<T>(this Queue<T> q, T el)
        {
            q.Enqueue(el);
        }
        public static void ReInsert<T>(this List<T> list, int Src, int Dest)
        {

            var C2 = list[Src];
            list.Remove(C2);
            list.Insert(Dest, C2);
        }
        public static void Swap<T>(this List<T> list, int Ind1, int Ind2)
        {
            var C1 = list[Ind1];
            var C2 = list[Ind2];
            list[Ind2] = C2;
            list[Ind1] = C1;
        }
    }
}
namespace CATesting
{
    class CycleFinder
    {
        void Find(Allax.Cryptography.CACryptor CACr)
        {
            var Aut = CACr.FN.Automata;
            var UnusedCells = Enumerable.Range(0, Aut.GetCellCount()).ToList();
            var Cycles = new List<List<int>>();
            foreach (var i in Enumerable.Range(0, Aut.GetCellCount()))
            {
                //Console.WriteLine(String.Format("\n\nStart cell #{0,3}", i));
                var Aut2 = (Allax.Cryptography.CA)Aut.Clone();
                var Cell = Aut2[i];
                var ResultCells = new Queue<MyCell>();
                ResultCells.Enqueue(new MyCell(Cell));
                var PrevIter = new List<MyCell>();
                var PrevPrevIter = new List<MyCell>();
                for (int iteration = 0; iteration < 6; iteration++)
                {
                    //Console.WriteLine(String.Format("\nIteration #{0}", iteration));

                    foreach (var pc in PrevPrevIter)
                    {

                        var L = ResultCells.ToList();
                        L.RemoveAll(c => c.Index == pc.Index);
                        ResultCells = new Queue<MyCell>(L);
                    }
                    if (iteration != 0)
                    {
                        PrevPrevIter = new List<MyCell>(PrevIter);
                        if (ResultCells.All(c => c.Index != i))
                        {
                            ;
                        }
                        else
                        {
                            ;
                            if (iteration > 3)
                                foreach (var ind in Enumerable.Range(0, ResultCells.Count))
                                {
                                    if (ResultCells.ElementAt(ind).Index == i)
                                    {
                                        //Console.WriteLine("Found cycle:");
                                        foreach (int I in ResultCells.ElementAt(ind).ParInd)
                                        {
                                            Cycles.Add(new List<int>(ResultCells.ElementAt(ind).ParInd));
                                            //Console.Write(string.Format("{0, 3} ", I));
                                        }
                                        //Console.WriteLine(i);
                                    }
                                }
                        }
                    }
                    PrevIter = ResultCells.ToList();
                    //ResultCells.ToList().ForEach(c => Console.WriteLine(String.Format("Cell #{0,3}", c.Index)));

                    var CellsCount = ResultCells.Count;
                    for (int CellIt = 0; CellIt < CellsCount; CellIt++)
                    {
                        MyCell C = ResultCells.Dequeue();
                        var NewParInd = new List<int>(C.ParInd);
                        NewParInd.Add(C.Index);
                        C.Neighbors.ForEach(c => ResultCells.Enqueue(new MyCell(c) { ParInd = NewParInd }));
                    }
                }
            }
            var UsedCycles = new List<List<int>>();
            while (UnusedCells.Count != 0)
            {
                var C = UnusedCells[0];
                var Found = false;
                foreach (var Cycle in Cycles)
                {
                    if (Cycle.All(c => c != C))
                    {
                        continue;
                    }
                    else
                    {
                        if (Cycle.All(c => UnusedCells.Contains(c)))
                        {
                            UsedCycles.Add(Cycle);
                            Cycle.ForEach(c => UnusedCells.Remove(c));
                            Cycles.Remove(Cycle);
                            Found = true;
                            break;
                        }
                    }
                }
                if (!Found)
                {
                    UnusedCells.Remove(C);
                    //break;// throw new NotImplementedException();
                }
            }
        }
    }
    class MyCell:Allax.Cryptography.Cell
    {
        public List<int> ParInd = new List<int>();
        public MyCell(Allax.Cryptography.Cell C)
        {
            this.Index = C.Index;
            this.Neighbors = new List<Allax.Cryptography.Cell>(C.Neighbors);
        }
    }
    class Program
    {
        static CA Numerate(CA Aut, int LinearArgInd=1)
        {
            Aut = (CA)Aut.Clone();
            var hk = new Allax.Cryptography.HK();
            hk.G = new List<List<int>>(Enumerable.Range(0, 2 * Aut.GetCellCount() + 1).Select(i => new List<int>()));
            hk.n = Aut.GetCellCount();
            hk.m = 2 * hk.n;
            Aut.G.Cells.ForEach(i =>
            {
                hk.G[i.Index + 1] = new List<int>(i.Neighbors.Select(c => c.Index + 1 + hk.n));
                hk.G[i.Index + 1 + hk.n] = new List<int>(i.Neighbors.Select(c => c.Index + 1));
            });
            hk.hopcroft_karp();
            var V = new Queue<int>(Enumerable.Range(0, Aut.GetCellCount()));
            var Cycles = new List<List<int>>();
            while (V.Count != 0)
            {
                var v = V.Dequeue();
                Console.WriteLine();
                Console.WriteLine();
                Console.Write("{0}", v);
                Cycles.Add(new List<int>() { v, });
                while (true)
                {
                    var vN = hk.match[v + 1] - hk.n - 1;

                    Console.Write("-{0}", vN);
                    Cycles[Cycles.Count - 1].Add(vN);
                    if (!V.Contains(vN))
                        break;
                    V = new Queue<int>(V.Where(i => i != vN));
                    v = vN;
                }
            }

            foreach (var Cycle in Cycles)
            {
                for (int i = 0; i < Cycle.Count - 1; i++)
                {
                    var C = Aut.G[Cycle[i]];
                    var Neighbours = C.Neighbors;
                    Neighbours.ReInsert(Neighbours.FindIndex(c => (c.Index == Cycle[i + 1])), LinearArgInd);
                    C.NeighboursIndexes = Neighbours.Select(c => c.Index).ToList();
                }
            }
            return Aut;
        }

        static List<Queue<Allax.Cryptography.Cell>> CALinearAnalisys(CA Aut, List<int> EndCellInd)
        {
            var UsedIndexes = new List<int>();
            var V = new List<Queue<Allax.Cryptography.Cell>>();
            V.Add(new Queue<Cell>(EndCellInd.Select(i => Aut[i])));
            for (int i = 0; i < 7; i++)
            {
                V.Add(new Queue<Cell>());
                for (int j = 0; j < V[i].Count; j++)
                {
                    var C = V[i].ElementAt(j);
                    if (!UsedIndexes.Contains(C.Index))
                    {
                        var Found = false;
                        foreach (var CN in C.Neighbors.Where(c => c.Index != C.Neighbors[1].Index))
                        {
                            if (UsedIndexes.Contains(CN.Index))
                            {
                                C.Neighbors.Swap(0, C.Neighbors.IndexOf(CN));
                                Found = true;
                                break;
                            }
                        }
                        if (!Found)
                        {
                            foreach (var CN in C.Neighbors.Where(c => c.Index != C.Neighbors[1].Index))
                            {
                                if (!CN.Neighbors.Where(c => c.Index != CN.Neighbors[1].Index).All(el => !UsedIndexes.Contains(el.Index)))
                                {
                                    C.Neighbors.Swap(0, C.Neighbors.IndexOf(CN));
                                    break;
                                }
                            }
                        }
                        UsedIndexes.Add(C.Index);
                    }
                    if (!V[i + 1].Contains(C.Neighbors[0]))
                        V[i + 1].Enqueue(C.Neighbors[0]);
                    if (!V[i + 1].Contains(C.Neighbors[1]))
                        V[i + 1].Enqueue(C.Neighbors[1]);
                }
            }
            return V;
        }

        static void Main(string[] args)
        {
            var CACr = new Allax.Cryptography.CACryptor(7, 128);
            var Func = Enumerable.Range(0, 1 << 6).Select(i => CACr.FN.Automata.F(WayConverter.ToList(i, 6).ConvertAll(k => (k == false) ? 0 : 1))).ToList().ConvertAll(l => (l == 0) ? new List<bool> { false } : new List<bool> { true });
            var DB = new SBlockDB();
            var Res = DB.GetCorMatrix(Func);
            foreach (var Ind in Enumerable.Range(0, Res[1].Count))
            {
                WayConverter.ToList(Ind, 6).ForEach(X => Console.Write(string.Format("{0} ", Convert.ToInt32(X))));
                Console.WriteLine(string.Format("{0}", Res[1][Ind]));
            }
            var Res2 = DB.GetDifMatrix(Func);
            foreach (var Ind in Enumerable.Range(0, Res2[1].Count))
            {
                WayConverter.ToList(Ind, 6).ForEach(X => Console.Write(string.Format("{0} ", Convert.ToInt32(X))));
                Console.WriteLine(string.Format("{0}", Res2[Ind][1]));
            }
            var NewAut = Numerate(CACr.FN.Automata);

            CACr.FN.AssignAutomata(NewAut);
            int[] Sum = new int[182];
            for (int st = 0; st < 182; st++)
            {
                for (int i = 0; i < 182; i++)
                {
                    if (i != st)
                    {
                        var Result = CALinearAnalisys((CA)CACr.FN.Automata.Clone(), new List<int> { st});
                        Result.Reverse();
                        Sum[st] = Result.Skip(1).Sum(c => c.Count);
                        break;
                    }
                }
            }
            var Best = Sum.Where(c => c != 0).Min();
            var A = (CA)NewAut.Clone();
            var MaxStarts = 182;
            var CollisionResult = new List<List<Cell>> (Enumerable.Range(0, A.GetCellCount()).Select(a=>new List<Cell>()));
            foreach (var start in A.G.Cells)
            {
                var B = new List<Cell>();
                var C = new List<Cell>();
                B.Add(start);
                    while (B.Count < MaxStarts)
                    {
                        //Add C(x2)-B
                        var AddedToB = false;
                        var AddedToC = false;
                        foreach (var b in B)
                        {
                            var C2 = b.Neighbors.Find(c => c.Neighbors[1] == b);
                            if (!C.Contains(C2))
                            {
                                C.Add(C2);
                                AddedToC = true;
                            }
                        }
                        //Add B-(x2)C
                        foreach (var c in C)
                        {
                            if (!B.Contains(c.Neighbors[1]))
                            {
                                B.Add(c.Neighbors[1]);
                                AddedToB = true;
                            }
                        }
                        if (AddedToB || AddedToC)
                            continue;
                        //Check Ci
                        foreach (var c in C)
                        {
                            //Ci-Bi
                            var Candidates0 = c.Neighbors.Where(cell => cell != c.Neighbors[1] && B.Contains(cell));
                            if (Candidates0.Count() >= 2)
                            {
                                continue;
                            }
                            else
                            {
                                //search
                                //Ci-NewBi-(x2)Cj
                                var Candidates1 = c.Neighbors.Where(cell => cell != c.Neighbors[1] && 
                                            C.Contains(cell.Neighbors.Find(cell2 => cell2.Neighbors[1] == cell)));
                                //Ci-NewBi-(xi)Cj
                                var Candidates2 = c.Neighbors.Where(cell => cell != c.Neighbors[1] &&
                                            !cell.Neighbors.All(cell2 => !C.Where(cell3=>cell3!=c).Contains(cell2)));
                                //Ci-NewBi
                                var Candidates3 = c.Neighbors.Where(cell => cell != c.Neighbors[1]);
                                Candidates1 = new Queue<Cell> (Candidates1.Where(cell => !Candidates0.Contains(cell)));
                                Candidates2 = new Queue<Cell>(Candidates2.Where(cell => !Candidates0.Contains(cell) && !Candidates1.Contains(cell)));
                                Candidates3 = new Queue<Cell>(Candidates3.Where(cell => !Candidates0.Contains(cell) && !Candidates1.Contains(cell) && !Candidates2.Contains(cell)));
                                for (int i = Candidates0.Count();i<2;i++)
                                {
                                    if(Candidates1.Count()!=0)
                                    {
                                        B.Add(((Queue<Cell>)Candidates1).Dequeue());
                                        AddedToB = true;
                                        continue;
                                    }
                                    if (Candidates2.Count() != 0)
                                    {
                                        B.Add(((Queue<Cell>)Candidates2).Dequeue());
                                        AddedToB = true;
                                        continue;
                                    }
                                    if (Candidates3.Count() != 0)
                                    {
                                        B.Add(((Queue<Cell>)Candidates3).Dequeue());
                                        AddedToB = true;
                                        continue;
                                    }
                                }
                            }
                        }
                        if (AddedToB || AddedToC)
                            continue;
                        Console.WriteLine("\nB:");
                        B.ForEach(c => Console.Write(string.Format("{0}-", c.Index)));
                        Console.WriteLine("\nC:");
                        C.ForEach(c =>
                        
                            Console.WriteLine(string.Format("C{0} (X1:{1},X2:{2},X6:{3})", c.Index, c.Neighbors.Where(cell => cell != c.Neighbors[1] && B.Contains(cell)).ToList()[0].Index, c.Neighbors[1].Index, c.Neighbors.Where(cell => cell != c.Neighbors[1] && B.Contains(cell)).ToList()[1].Index))
                            
                            );
                    CollisionResult[start.Index] = B;
                    break;
                    
                }
            }
            var BestCollision = CollisionResult.Find(a=>a.Count==CollisionResult.Select(b=>b.Count).Min());
            Console.WriteLine("B:");
            foreach (var b in BestCollision)
            {
                Console.Write(string.Format("{0}-", b.Index));
            }
            Console.WriteLine("\nC:");
            foreach (var b in BestCollision)
            {
                Console.Write(string.Format("{0}-", b.Neighbors.Find(c=>c.Neighbors[1]==b).Index));
            }
            foreach (var B2 in A.G.Cells)
            {
                var C2 = B2.Neighbors.Find(c => c.Neighbors[1] == B2);
                foreach(var B1 in C2.Neighbors.Where(c=>c!=B2))
                {
                    foreach(var B3 in C2.Neighbors.Where(c=>c!=B2&&c!=B1))
                    {
                        var C1 = B1.Neighbors.Find(c => c.Neighbors[1] == B1);
                        var C3 = B3.Neighbors.Find(c => c.Neighbors[1] == B3);
                        if(C1.Neighbors.Contains(B2)&& C1.Neighbors.Contains(B3))
                        {
                            if(C3.Neighbors.Contains(B1)&& C3.Neighbors.Contains(B2))
                            {
                                ;
                            }
                        }
                    }
                }

            }
            var OT = (from i in WayConverter.ToList(0xDDAABBAAC, 64).Concat(WayConverter.ToList(0xCACACADAB, 64)) select Convert.ToInt32(i)).ToList().ConvertAll(i => Convert.ToBoolean(i));
            var CT = CACr.Encrypt(OT);
            Console.WriteLine("{0,16:X}, {1,16:X}", WayConverter.ToLong((from i in Enumerable.Range(0, CT.Count / 2) select (CT[i])).ToList()), WayConverter.ToLong((from i in Enumerable.Range(CT.Count / 2, CT.Count / 2) select (CT[i])).ToList()));
            var OT2 = CACr.Decrypt(CT);
            Console.WriteLine("{0,16:X}, {1,16:X}", WayConverter.ToLong((from i in Enumerable.Range(0, OT2.Count / 2) select (OT2[i])).ToList()), WayConverter.ToLong((from i in Enumerable.Range(OT2.Count / 2, OT2.Count / 2) select (OT2[i])).ToList()));
            Console.ReadLine();
        }
    }
}
