using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Allax;
using Allax.Cryptography;
using System.Threading;
using System.Numerics;

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
        public int GetResult(int Index)
        {
            return FuncMatrix[Index];
        }
        Dictionary<long, int> FuncMatrix;
    }
    class CA : ICloneable
    {
        public Graph G;
        public LocalFunc F;
        //public LocalFuncByIndex F_Index;
        //public delegate int LocalFunc(List<int> Values);
        //public delegate int LocalFuncByIndex(int Index);
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
            int Index = 0;
            for(int i=0;i< G.Cells[CellIndex].Neighbors.Count;i++)
            {
                Index = (Index << 1) + G.Cells[CellIndex].Neighbors[i].Value;
            }
            return F.GetResult(Index);
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
            //this.G = New.G;
        }
        public static CA NextStep(CA Orig, int Steps = 1, bool MultiThread = false)
        {
            for (int s = 0; s < Steps; s++)
            {
                //var NextStepCA = Orig/*.Clone()*/;
                if (MultiThread)
                {
                /*    var E = new EngineForTaskerAndWorker();
                    E.TheTasker = new NextStepTasker();
                    ((NextStepTasker)E.TheTasker).CellCount = Orig.G.Cells.Count;
                    ((NextStepTasker)E.TheTasker).OrigAutomata = Orig;
                    ((NextStepTasker)E.TheTasker).NewAutomata = NextStepCA;
                    E.TheWorker = new SignalWorker(new WorkerParams(E, Environment.ProcessorCount));
                    E.TheWorker.AsyncRun();
                    ((SignalWorker)E.TheWorker).oSignalEvent.WaitOne();
                    E.TheWorker.Dispose();*/
                }
                else
                {
                    List<int> NextStep = new List<int>(Enumerable.Repeat(0, Orig.GetCellCount()));
                    for (int i = 0; i < Orig.G.Cells.Count; i++)
                    {
                        NextStep[i] = Orig.GetNextStepCellValue(i);
                    }
                    for (int i = 0; i < Orig.G.Cells.Count; i++)
                    {
                        Orig[i].Value = NextStep[i];
                    }
                }
                //Orig = NextStepCA;
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
        public static List<int> GetRandomBitList(int Length, int WeightRequirement = -1)
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
            var OrigCA = new CA(G, LF);
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
        public static List<T> RetReverse<T>(this List<T> list)
        {
            list.Reverse();
            return list;
        }
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
    public interface IIterator
    {
        bool IsFinished();
        List<int> GetNext();
        void Reset();
    }
    public class MyIterator : IIterator
    {
        public MyIterator(Dictionary<BigInteger, int>.KeyCollection list) { SeenN = list; }
        public void SetKey(List<int> Key)
        {
            this.Key = Key;
        }
        public void SetConst(List<int> Const)
        {
            this.Const = Const;
        }
        public List<int> Key=Enumerable.Repeat(0, 64).ToList();
        public List<int> Const=Enumerable.Repeat(0, 54).ToList();
        public static List<int> BigIntegerToList(BigInteger BI, int ListLength = 182)
        {
            return Enumerable.Range(0, ListLength).Select(i => (int)((BI >> i) & 1)).Reverse().ToList();
        }
        public static BigInteger ListToBigInteger(List<int> L)
        {
            BigInteger ret = 0;
            foreach (var Value in L)
            {
                ret = (ret << 1) + Value;
            }
            return ret;
        }
        public BigInteger N = 0;
        int Log2Limit = 64;
        Func<BigInteger, BigInteger> F = ((I) => { return ++I; });
        public void SetOpenTextGenFunc(Func<BigInteger, BigInteger> func)
        {
            this.F = func;
        }
        public IEnumerable<BigInteger> SeenN = new HashSet<BigInteger>();
        public bool IsFinished()
        {
            if (N < BigInteger.Pow(2, Log2Limit))
                return false;
            else
                return true;
        }
        public List<int> GetNext()
        {
            N=F(N);
            var N_List = BigIntegerToList(N, 64);
            while (SeenN.Contains(N))
            {
                N = F(N);
            }
            return N_List.Concat(Key).Concat(Const).ToList();
        }
        public void Reset()
        {
            N = 0;
        }
    }
    class DifferencialWatcher
    {
        List<List<Graph>> States=new List<List<Graph>>();
        CA OriginAutomata;
        List<CA> Automatas = new List<CA>();
        List<int> Mask;
        public MyIterator It;
        List<List<int>> States_Fast = new List<List<int>>();
        public static List<int> CreateDiffMask(CA Aut, List<int> BitsNumsToChange)
        {
            var Mask = Enumerable.Repeat(0, Aut.GetCellCount()).ToList();
            foreach (var b in BitsNumsToChange)
            {
                Mask[b] = 1;
            }
            return Mask;
        }
        public DifferencialWatcher(CA Aut)
        {
            this.OriginAutomata = Aut;
        }
        void Update()
        {
            States.Add(new List<Graph>());
            for (int i = 0; i < Automatas.Count; i++)
            {
                var Automata = Automatas[i];
                if (Automata != null)
                {
                    States.Last().Add((Graph)Automata.G.Clone());
                }
            }
        }
        public void SetIterator(MyIterator iterator)
        {
            this.It = iterator;
        }
        public void Step(bool UpdateStates=true)
        {
            foreach (var Automata in Automatas)
            {
                Automata.NextStep();
            }
            if (UpdateStates)
                Update();
        }
        public void Step(int Count, bool UpdateStates = true)
        {
            for (int i = 0; i < Count; i++)
            {
                Step(UpdateStates);
            }
        }
        public void SetAutomatas(List<List<int>> InitValues)
        {
            Automatas.Clear();
            States.Clear();
            for (int i = 0; i < InitValues.Count; i++)
            {
                Automatas.Add((CA)OriginAutomata.Clone());
                Automatas.Last().SetValues(InitValues[i]);
            }
        }
        public void SetDiffMask(List<int> Mask)
        {
            this.Mask = new List<int>(Mask);
        }
        public BigInteger N_ext = 0;
        public bool NextPair_Fast(int Step)
        {
            if (!It.IsFinished())
            {
                var Input = It.GetNext();
                var SecondInput = Input.Zip(Mask, (x, y) => x ^ y).ToList();
                Automatas.Clear();
                States_Fast.Clear();
                OriginAutomata.SetValues(Input);
                Automatas.Add(OriginAutomata);
                this.Step(Step, false);
                States_Fast.Add(OriginAutomata.GetValues());
                OriginAutomata.SetValues(SecondInput);
                this.Step(Step, false);
                States_Fast.Add(OriginAutomata.GetValues());
                return true;
            }
            else
                return false;
        }
        public bool NextPair(int Step)
        {
            if (!It.IsFinished())
            {
                var Input = It.GetNext();
                var SecondInput = Input.Zip(Mask, (x, y) => x ^ y).ToList();
                SetAutomatas(new List<List<int>> { Input, SecondInput });
                this.Step(Step);
                return true;
            }
            else
                return false;
        }
        public List<int> GetPairLastDifference()
        {
            //List<Graph> ret = new List<Graph>();
            {
                var CellsDiff = States_Fast[0].Zip(States_Fast[1], (i, j) => i ^ j).ToList();
                /*var GraphDiff = (Graph)StepStates[0].Clone();
                for (int i = 0; i < GraphDiff.Cells.Count; i++)
                {
                    GraphDiff.Cells[i].Value = CellsDiff[i];
                }
                ret.Add(GraphDiff);*/
                return CellsDiff;
            }
        }
        public List<Graph> GetPairDifference()
        {
            List<Graph> ret = new List<Graph>();
            foreach(var StepStates in States)
            {
                var CellsDiff = StepStates[0].Cells.Select(i => i.Value ^ StepStates[1].Cells[i.Index].Value).ToList();
                var GraphDiff = (Graph)StepStates[0].Clone();
                for(int i =0;i< GraphDiff.Cells.Count;i++)
                {
                    GraphDiff.Cells[i].Value = CellsDiff[i];
                }
                ret.Add(GraphDiff);
            }
            return ret;
        }
    }
    class Program
    {
        static CA Numerate(CA Aut, int LinearArgInd = 1)
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
        static void ShowMatixes(List<List<bool>> Func)
        {
            var DB = new SBlockDB();
            //Выведем матрицы корреляции и разности
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

        }
        static void SearchCollision(CA A)
        {

            var MaxStarts = 182;
            var CollisionResult = new List<List<Cell>>(Enumerable.Range(0, A.GetCellCount()).Select(a => new List<Cell>()));
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
                                        !cell.Neighbors.All(cell2 => !C.Where(cell3 => cell3 != c).Contains(cell2)));
                            //Ci-NewBi
                            var Candidates3 = c.Neighbors.Where(cell => cell != c.Neighbors[1]);
                            Candidates1 = new Queue<Cell>(Candidates1.Where(cell => !Candidates0.Contains(cell)));
                            Candidates2 = new Queue<Cell>(Candidates2.Where(cell => !Candidates0.Contains(cell) && !Candidates1.Contains(cell)));
                            Candidates3 = new Queue<Cell>(Candidates3.Where(cell => !Candidates0.Contains(cell) && !Candidates1.Contains(cell) && !Candidates2.Contains(cell)));
                            for (int i = Candidates0.Count(); i < 2; i++)
                            {
                                if (Candidates1.Count() != 0)
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
            var BestCollision = CollisionResult.Find(a => a.Count == CollisionResult.Select(b => b.Count).Min());
            Console.WriteLine("B:");
            foreach (var b in BestCollision)
            {
                Console.Write(string.Format("{0}-", b.Index));
            }
            Console.WriteLine("\nC:");
            foreach (var b in BestCollision)
            {
                Console.Write(string.Format("{0}-", b.Neighbors.Find(c => c.Neighbors[1] == b).Index));
            }

            /*foreach (var B2 in A.G.Cells)
            {
                var C2 = B2.Neighbors.Find(c => c.Neighbors[1] == B2);
                foreach (var B1 in C2.Neighbors.Where(c => c != B2))
                {
                    foreach (var B3 in C2.Neighbors.Where(c => c != B2 && c != B1))
                    {
                        var C1 = B1.Neighbors.Find(c => c.Neighbors[1] == B1);
                        var C3 = B3.Neighbors.Find(c => c.Neighbors[1] == B3);
                        if (C1.Neighbors.Contains(B2) && C1.Neighbors.Contains(B3))
                        {
                            if (C3.Neighbors.Contains(B1) && C3.Neighbors.Contains(B2))
                            {
                                ;
                            }
                        }
                    }
                }

            }*/
        }
        static void TestCrypt(CACryptor CACr)
        {

            var OT = (from i in WayConverter.ToList(0xDDAABBAAC, 64).Concat(WayConverter.ToList(0xCACACADAB, 64)) select Convert.ToInt32(i)).ToList().ConvertAll(i => Convert.ToBoolean(i));
            var CT = CACr.Encrypt(OT);
            Console.WriteLine("{0,16:X}, {1,16:X}", WayConverter.ToLong((from i in Enumerable.Range(0, CT.Count / 2) select (CT[i])).ToList()), WayConverter.ToLong((from i in Enumerable.Range(CT.Count / 2, CT.Count / 2) select (CT[i])).ToList()));
            var OT2 = CACr.Decrypt(CT);
            Console.WriteLine("{0,16:X}, {1,16:X}", WayConverter.ToLong((from i in Enumerable.Range(0, OT2.Count / 2) select (OT2[i])).ToList()), WayConverter.ToLong((from i in Enumerable.Range(OT2.Count / 2, OT2.Count / 2) select (OT2[i])).ToList()));
            Console.ReadLine();
        }
        static void TestLinAnalisys(CACryptor CACr)
        {

            int[] Sum = new int[182];
            for (int st = 0; st < 182; st++)
            {
                for (int i = 0; i < 182; i++)
                {
                    if (i != st)
                    {
                        var Result = CALinearAnalisys((CA)CACr.FN.Automata.Clone(), new List<int> { st });
                        Result.Reverse();
                        Sum[st] = Result.Skip(1).Sum(c => c.Count);
                        break;
                    }
                }
            }
            var Best = Sum.Where(c => c != 0).Min();
        }
        static void WriteDiffAnalisysToFile(List<int> Key,List<int>Const,List<int>BitsNums, Dictionary<BigInteger, int> Result)
        {

            using (FileStream aFile = new FileStream(string.Format(@"out_{0}_{1}.txt", string.Join("", Key), string.Join("", Const)), FileMode.Append, FileAccess.ReadWrite))
            {
                using (StreamWriter sr = new StreamWriter(aFile))
                {
                    sr.WriteLine(string.Format("Key: {0}", string.Join("", Key)));

                    sr.WriteLine(string.Format("Const: {0}", string.Join("", Const)));
                    sr.WriteLine(string.Format("Mask: {0}", string.Join("-", BitsNums)));
                    StringBuilder sb = new StringBuilder();

                    foreach (var r in Result)
                    {
                        sb.Append(string.Format("{0};{1}", string.Join("", MyIterator.BigIntegerToList(r.Key)), r.Value));
                    }
                    sr.WriteLine(sb);
                }
            }
        }
        static DiffResult DiffAnalisys(CA NewAut, BigInteger TrCount, List<int> K, List<int> C, Func<BigInteger, BigInteger> OTGenFunc, int StepCount, List<int>BitsNums)
        {
            DifferencialWatcher differencialWatcher = new DifferencialWatcher((CA)NewAut.Clone());
            var Result = new Dictionary<BigInteger, int>();
            var iterator = new MyIterator(Result.Keys);
            iterator.Reset();
            iterator.SetOpenTextGenFunc(OTGenFunc);
            iterator.SetKey(K);
            iterator.SetConst(C);
            differencialWatcher.SetIterator(iterator);
            var Mask = DifferencialWatcher.CreateDiffMask(NewAut, BitsNums);
            differencialWatcher.SetDiffMask(Mask);
            DiffResult Min = new DiffResult() { Diff = 91 };
            for (BigInteger j = 0; differencialWatcher.NextPair(StepCount) && (j < TrCount); j++)
            {
                var CurDiff = differencialWatcher.GetPairDifference().Last().Cells;
                var DiffCount = CurDiff.Select(i=>i.Value).Sum();
                Result.Add(iterator.N, DiffCount);
                if (Math.Abs(91 - DiffCount) > Math.Abs(91 - Min.Diff))
                {
                    Min.Diff = DiffCount;
                    Min.OT = differencialWatcher.It.N;
                }
            }
            //WriteDiffAnalisysToFile(iterator.Key,iterator.Const,BitsNums,Result);
            return Min;
        }
        static DiffResult DiffAnalisys_LastRound(CA NewAut, BigInteger TrCount, List<int> K, List<int> C, Func<BigInteger, BigInteger> OTGenFunc, int StepCount, List<int> BitsNums)
        {
            DifferencialWatcher differencialWatcher = new DifferencialWatcher((CA)NewAut.Clone());
            var Result = new Dictionary<BigInteger, int>();
            var iterator = new MyIterator(Result.Keys);
            iterator.Reset();
            iterator.SetOpenTextGenFunc(OTGenFunc);
            iterator.SetKey(K);
            iterator.SetConst(C);
            differencialWatcher.SetIterator(iterator);
            var Mask = DifferencialWatcher.CreateDiffMask(NewAut, BitsNums);
            differencialWatcher.SetDiffMask(Mask);
            DiffResult Min = new DiffResult() { Diff = 91 };
            for (BigInteger j = 0; differencialWatcher.NextPair_Fast(StepCount) && (j < TrCount); j++)
            {
                var CurDiff = differencialWatcher.GetPairLastDifference();
                var DiffCount = CurDiff.Sum();
                Result.Add(iterator.N, DiffCount);
                if (Math.Abs(91 - DiffCount) > Math.Abs(91 - Min.Diff))
                {
                    Min.Diff = DiffCount;
                    Min.OT = differencialWatcher.It.N;
                }
            }
            //WriteDiffAnalisysToFile(iterator.Key,iterator.Const,BitsNums,Result);
            return Min;
        }

        class DiffResult
        {
            public int Diff=91;
            public BigInteger K=0;
            public BigInteger C=0;
            public BigInteger OT=0;
            public List<int> BitsNums=new List<int>();
        }
        static Dictionary<BigInteger, DiffResult> BigDiffAnalisys(CA NewAut, int StepCount, int Log2Treshold, int KeysCount, int ConstsCount, List<int> BitsNums)
        {
            var Keys = new Dictionary<BigInteger, DiffResult>(KeysCount);
            while (Keys.Count != KeysCount)
            {
                var Candidate = MyIterator.ListToBigInteger(CACryptor.GetRandomBitList(64));
                if (!Keys.Keys.Contains(Candidate))
                {
                    Keys.Add(Candidate, new DiffResult() { K = Candidate });
                }
            }
            var Consts = new Dictionary<BigInteger, DiffResult>(ConstsCount);
            while (Consts.Count != ConstsCount)
            {
                var Candidate = MyIterator.ListToBigInteger(CACryptor.GetRandomBitList(54, 27));
                if (!Consts.Keys.Contains(Candidate))
                {
                    Consts.Add(Candidate, new DiffResult() { C = Candidate });
                }
            }
            Func<BigInteger, BigInteger> OTGenFunc = (I) => { return MyIterator.ListToBigInteger(CACryptor.GetRandomBitList(64)); };

            foreach (var K in Keys)
            {
                {
                    var R = new Random();
                    var C = Consts.ElementAt(R.Next(Consts.Count));
                    var K_List = MyIterator.BigIntegerToList(K.Key, 64);
                    var C_List = MyIterator.BigIntegerToList(C.Key, 54);
                    var Result = DiffAnalisys(NewAut, BigInteger.Pow(2, Log2Treshold), K_List, C_List, OTGenFunc, StepCount, BitsNums);
                    if (Math.Abs(91 - Result.Diff) > Math.Abs(91 - K.Value.Diff))
                    {
                        K.Value.Diff = Result.Diff;
                        K.Value.C = C.Key;
                        K.Value.BitsNums = BitsNums;
                        K.Value.OT = Result.OT;
                    }
                    if (Math.Abs(91 - Result.Diff) > Math.Abs(91 - C.Value.Diff))
                    {
                        C.Value.Diff = Result.Diff;
                        C.Value.K = K.Key;
                        C.Value.BitsNums = BitsNums;
                        K.Value.OT = Result.OT;
                    }
                }
            }
            return Keys;
        }
        static Dictionary<BigInteger, DiffResult> BigDiffAnalisys_FAST(CA NewAut,int StepCount, int Log2Treshold, int KeysCount, int ConstsCount, List<int> BitsNums)
        {
            var Keys = new Dictionary<BigInteger, DiffResult>(KeysCount);
            while(Keys.Count != KeysCount)
            {
                var Candidate = MyIterator.ListToBigInteger(CACryptor.GetRandomBitList(64));
                if(!Keys.Keys.Contains(Candidate))
                {
                    Keys.Add(Candidate, new DiffResult(){ K = Candidate });
                }
            }
            var Consts = new Dictionary<BigInteger, DiffResult>(ConstsCount);
            while (Consts.Count != ConstsCount)
            {
                var Candidate = MyIterator.ListToBigInteger(CACryptor.GetRandomBitList(54, 27));
                if (!Consts.Keys.Contains(Candidate))
                {
                    Consts.Add(Candidate, new DiffResult() { C = Candidate });
                }
            }
            Func<BigInteger, BigInteger> OTGenFunc = (I) => { return MyIterator.ListToBigInteger(CACryptor.GetRandomBitList(64)); };

            foreach (var K in Keys)
            {
                {
                    var R = new Random();
                    var C = Consts.ElementAt(R.Next(Consts.Count));
                    var K_List = MyIterator.BigIntegerToList(K.Key, 64);
                    var C_List = MyIterator.BigIntegerToList(C.Key, 54);
                    var Result = DiffAnalisys_LastRound(NewAut, BigInteger.Pow(2, Log2Treshold), K_List, C_List, OTGenFunc, StepCount, BitsNums);
                    if(Math.Abs(91-Result.Diff)>Math.Abs(91-K.Value.Diff))
                    {
                        K.Value.Diff = Result.Diff;
                        K.Value.C = C.Key;
                        K.Value.BitsNums = BitsNums;
                        K.Value.OT = Result.OT;
                    }
                    if (Math.Abs(91-Result.Diff)>Math.Abs(91-C.Value.Diff))
                    {
                        C.Value.Diff = Result.Diff;
                        C.Value.K = K.Key;
                        C.Value.BitsNums = BitsNums;
                        K.Value.OT = Result.OT;
                    }
                }
            }
            return Keys;
        }
        static void Main(string[] args)
        {
            //Создаю стандартный шифр на 128
            var CACr = new Allax.Cryptography.CACryptor(7, 128);
            var CACr_2 = new CACryptor(7, 128, "x3x4+x1x3+x1+x2+1", 4);
            //Граф на 182 вершины выбран. Извлечем Функцию в нужном нам формате
            var Func = Enumerable.Range(0, 1 << 6).Select(i => CACr.FN.Automata.F.GetResult(WayConverter.ToList(i, 6).ConvertAll(k => (k == false) ? 0 : 1))).ToList().ConvertAll(l => (l == 0) ? new List<bool> { false } : new List<bool> { true });
            ShowMatixes(Func);
            var Func_2 = Enumerable.Range(0, 1 << 4).Select(i => CACr_2.FN.Automata.F.GetResult(WayConverter.ToList(i, 4).ConvertAll(k => (k == false) ? 0 : 1))).ToList().ConvertAll(l => (l == 0) ? new List<bool> { false } : new List<bool> { true });
            ShowMatixes(Func_2);
            // Нумеруем 2-фактор
            var NewAut = Numerate((CA)CACr.FN.Automata.Clone());
            //запихиваем граф в шифратор
            CACr.FN.AssignAutomata(NewAut);

            //SearchCollision((CA)NewAut.Clone());
            /*114, 118, 80, 61, 174, 115, 81, 172, 47, 136, 104, 100, 91, 23, 86, 144, 51, 22, 44, 74, 66, 13, 7, 46,
10, 78, 112, 129, 163, 171, 60, 102, 101, 116, 82, 133, 165, 125, 142, 8, 25, 166, 37, 0, 138, 6, 67, 76, 92, 169, 95, 111, 84, 34, 126, 62, 85, 4, 58, 9, 128,*/

            Console.WriteLine();
            BigDiffAnalisys(NewAut, 7, 4, 1, 1, new List<int> { 28 });

            int ultramin = 91;
            DiffResult UltraMinR = new DiffResult();;
            for(int i=0;i<64;i++)
            {
                var BitsNums = new List<int> { i};
                var Result = BigDiffAnalisys_FAST(NewAut, 7, 4, 100, 100, BitsNums);
                foreach (var r in Result)
                {
                    if (Math.Abs(91 - r.Value.Diff) > Math.Abs(91 - ultramin))
                    {
                        ultramin = r.Value.Diff;
                        UltraMinR = r.Value;
                    }
                }
            }
           Console.WriteLine(Math.Abs(91-ultramin));
        }
    }
}
