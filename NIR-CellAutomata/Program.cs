﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Allax;
using Allax.Cryptography;
using System.Threading;
using System.Numerics;
using System.Collections;
using CATesting;
using System.Linq.Expressions;

namespace Allax.Cryptography
{
    public class Cell
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
    class CA : ICloneable, IEnumerable
    {
        public Graph G;
        public LocalFunc F;
        //public LocalFuncByIndex F_Index;
        //public delegate int LocalFunc(List<int> Values);
        //public delegate int LocalFuncByIndex(int Index);
        public IEnumerator GetEnumerator()
        {
            return GetWorkSubgraphs(GetCellCount(), 0);
        }
        public IEnumerator GetWorkSubgraphs(int intStartNodesCount, int intStepsCount, int intCurrentLayerForRecursive=0, List<List<MyCell>> cellsForRecursive = null, bool StartNodesFromOpenText = true, int StartFrom=0)
        {
            if (intCurrentLayerForRecursive == intStepsCount)
            {
                yield return cellsForRecursive;
                yield break;
            }
            if (cellsForRecursive == null)
            {
                int EndCellIndex = (StartNodesFromOpenText) ? 64 : GetCellCount();
                cellsForRecursive = new List<List<MyCell>>();
                var ZeroLayer = new List<MyCell>();
                //
                var precomputedBins = Enumerable.Range(0, EndCellIndex).Select(i => new List<MyCell>(G.Cells.Skip(i).Select(j=>new MyCell(j)))).ToList();
                var bins = new List<List<MyCell>>(Enumerable.Range(0, intStartNodesCount).Select(i=>new List<MyCell>()));
                bins[0] = precomputedBins[StartFrom%precomputedBins.Count];
                Console.WriteLine(string.Format("Warning! Starting from cell with number {0}", StartFrom));
                    //search in cycle
                for (int i = 0; i < bins.Count; i++)
                {
                    var bin = bins[i];
                    if(bin.Count>0)
                    {
                        if (ZeroLayer.Count > i)
                            ZeroLayer.RemoveAt(i);
                        ZeroLayer.Insert(i, bin[0]);
                        if((i<bins.Count-1) && (bin[0].Index + 1 < precomputedBins.Count))
                        {
                            bins[i + 1] = precomputedBins[bin[0].Index + 1];
                        }
                        bin.RemoveAt(0);
                    }
                    else
                    {
                        if (i > 0)
                        {
                            i -= 2;
                            continue;
                        }
                        else
                            break;
                    }
                    if(i==bins.Count-1)
                    {

                        var NewCellsForRecursive = new List<List<MyCell>>(cellsForRecursive);
                        NewCellsForRecursive.Add(ZeroLayer);
                        var it = GetWorkSubgraphs(intStartNodesCount, intStepsCount, 0, NewCellsForRecursive);
                        while (it.MoveNext())
                        {
                            yield return it.Current;
                        }
                        i -= 1;
                    }
                }

                //
                yield break;
            }
            var LastLayer = cellsForRecursive.Last();
            var Candidates = new List<MyCell>();
            var X2CellsFromCandidates = new List<MyCell>();
            if(intCurrentLayerForRecursive==0)
            {
                ;
            }
            LastLayer.ForEach(cell => {
                foreach (var c in cell.Neighbors)
                {
                    var myC = new MyCell(c);
                    myC.VarParInd.Add(cell.Index);
                    if (Candidates.All(i=>i.Index!=myC.Index))
                    {
                        //Candidates does not contain myC
                        Candidates.Add(myC);
                        if (cell.Index == myC.Neighbors[1].Index)
                        {
                                X2CellsFromCandidates.Add(myC);
                        }

                    }
                    else 
                    {
                        //Candidates contains myC
                        myC = Candidates.Find(i => i.Index == myC.Index);
                        myC.VarParInd.Add(cell.Index);
                        if (!X2CellsFromCandidates.All(i => i.Index != myC.Index))
                        {
                            X2CellsFromCandidates.Remove(myC);
                        }
                    }
                }
            });
            foreach (var cell in X2CellsFromCandidates)
            {
                Candidates.Remove(cell);
            }
            //Lets organize depth-first search for the remaining candidates
            for (BigInteger bigInteger = 0; bigInteger < BigInteger.Pow(2, Candidates.Count); bigInteger++)
            {
                var NewLayer = new List<MyCell>();
                //Move X2 Cells to NewLayer hard because of lack of any else edge toward this cell
                foreach (var cell in X2CellsFromCandidates)
                    NewLayer.Add(cell);
                for (int i = 0; i < Candidates.Count; i++)
                {
                    if ((bigInteger & BigInteger.Pow(2, i)) != 0)
                    {
                        NewLayer.Add(Candidates[i]);
                    }
                }
                var NewCellsForRecursive = new List<List<MyCell>>(cellsForRecursive);
                if(NewLayer.Count==0)
                {
                    yield return NewCellsForRecursive;
                    continue;
                }
                NewCellsForRecursive.Add(NewLayer);
                var it =
                GetWorkSubgraphs(intStartNodesCount, intStepsCount, intCurrentLayerForRecursive + 1, NewCellsForRecursive);
                while (it.MoveNext())
                {
                    yield return it.Current;
                }
            }
            yield break;
        }
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
        public void SetConstants(List<List<int>> Constants)
        {
            if (Constants == null || Constants.Count != RoundsCount)
            {
                throw new NotImplementedException();
            }
            else
            {
                if (Constants[0].Count != ConstLength)
                    throw new NotImplementedException();
                this.Constants = new List<List<int>>((from i in Constants select new List<int>(i)));
                FN.RoundConstants = this.Constants;
            }
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
        public static Dictionary<TKey, TValue> ToDictionary<TKey,TValue>(this IEnumerable<KeyValuePair<TKey,TValue>> keyValuePairs)
        {
            return keyValuePairs.ToDictionary(kv => kv.Key, kv => kv.Value);
        }
        public static bool MyContains(this IEnumerable<MyCell> myCells, MyCell cell)
        {
            return !myCells.All(i => i.Index != cell.Index);
        }
        public static bool All(this IEnumerable<bool> vs)
        {
            return vs.All(i => i);
        }
        public static bool BitComparable(this int Int1, int Int2)
        {
            int result = 0;
            var ComparationResult = Enumerable.Range(0, 32).Select(i => (((((1 << i) & Int1) > 0) ? 1 : 0) - ((((1 << i) & Int2) > 0) ? 1 : 0))).ToList();
            if (ComparationResult.All(i => i >= 0))
                result = 1;
            else
            {
                if (ComparationResult.All(i => i <= 0))
                {
                    result = -1;
                }
            }
            return result!=0;
        }
        public static bool BitCompareG(this int Int1, int Int2)
        {
            int result = 0;
            var ComparationResult = Enumerable.Range(0, 32).Select(i => (((((1 << i) & Int1) > 0) ? 1 : 0) - ((((1 << i) & Int2) > 0) ? 1 : 0))).ToList();
            if (ComparationResult.All(i => i >= 0))
            {
                if (Int1 != Int2)
                    result = 1;
            }
            else
            {
                if (ComparationResult.All(i => i <= 0))
                {
                    result = -1;
                }
            }
            return result > 0;
        }
        public static bool BitCompareGE(this int Int1, int Int2)
        {
            int result = 0;
            var ComparationResult = Enumerable.Range(0, 32).Select(i => (((((1 << i) & Int1)>0)?1:0) - ((((1 << i) & Int2)>0)?1:0))).ToList();
            if (ComparationResult.All(i => i >= 0))
                result = 1;
            else
            {
                if (ComparationResult.All(i => i <= 0))
                {
                    result = -1;
                }
            }
            return result>0;
        }
        public static IEnumerator MultiListSerialIterator<T>(this List<List<T>> source)
        {
            var ZeroLayer = new List<T>();
            //
            var k = source.Count;
            var precomputedBins = source;
            var bins = new List<List<T>>(Enumerable.Range(0, k).Select(i => new List<T>()));
            bins[0] = new List<T>(precomputedBins[0]);
            //var IndexDict = new Dictionary<T, int>();
            //Enumerable.Range(0, source.Count).ToList().ForEach(i => IndexDict.Add(source[i], i));
            //search in cycle
            for (int i = 0; i < bins.Count; i++)
            {
                var bin = bins[i];
                if (bin.Count > 0)
                {
                    if (ZeroLayer.Count > i)
                        ZeroLayer.RemoveAt(i);
                    ZeroLayer.Insert(i, bin[0]);
                    if ((i < bins.Count - 1)&&(i+1 < precomputedBins.Count))
                    {
                        bins[i + 1] = new List<T>(precomputedBins[i+1]);
                    }
                    bin.RemoveAt(0);
                }
                else
                {
                    if (i > 0)
                    {
                        //indeed i-=1; continue operator will add extra one
                        i -= 2;
                        continue;
                    }
                    else
                        break;
                }
                if (i == bins.Count - 1)
                {
                    yield return new List<T>(ZeroLayer);
                    i -= 1;
                }
            }

            //
            yield break;
        }
        public static List<List<T>> GetCombinations<T>(this List<T> source, int k)
        {
            var it = source.CombinationIterator(k);
            var res = new List<List<T>>();
            while (it.MoveNext())
            {
                res.Add((List<T>)it.Current);
            }
            return res;
        }
        public static IEnumerator CombinationIterator<T>(this List<T> source, int k)
        {
            var ZeroLayer = new List<T>();
            //
            var precomputedBins = Enumerable.Range(0, source.Count).Select(i => new List<T>(source.Skip(i))).ToList();
            var bins = new List<List<T>>(Enumerable.Range(0, k).Select(i => new List<T>()));
            bins[0] = precomputedBins[0];
            var IndexDict = new Dictionary<T, int>();
            Enumerable.Range(0, source.Count).ToList().ForEach(i => IndexDict.Add(source[i], i));
            //search in cycle
            for (int i = 0; i < bins.Count; i++)
            {
                var bin = bins[i];
                if (bin.Count > 0)
                {
                    if (ZeroLayer.Count > i)
                        ZeroLayer.RemoveAt(i);
                    ZeroLayer.Insert(i, bin[0]);
                    if ((i < bins.Count - 1)&&(IndexDict[bin[0]] + 1< precomputedBins.Count))
                    {
                        bins[i + 1] = precomputedBins[IndexDict[bin[0]] + 1];
                    }
                    bin.RemoveAt(0);
                }
                else
                {
                    if (i > 0)
                    {
                        i -= 2;
                        continue;
                    }
                    else
                        break;
                }
                if (i == bins.Count - 1)
                {

                    yield return new List<T>(ZeroLayer);
                    i -= 1;
                }
            }

            //
            yield break;
        }

        public static List<T> ExtractElements<T>(this List<T> list, List<int> IndexOrder)
        {
            return Enumerable.Range(0, IndexOrder.Count).Select(j=>list[IndexOrder.IndexOf(j)]).ToList();
        }
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

    public static class MyExtenstions
    {}
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
                                        foreach (int I in ResultCells.ElementAt(ind).VarParInd)
                                        {
                                            Cycles.Add(new List<int>(ResultCells.ElementAt(ind).VarParInd));
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
                        var NewParInd = new List<int>(C.VarParInd);
                        NewParInd.Add(C.Index);
                        C.Neighbors.ForEach(c => ResultCells.Enqueue(new MyCell(c) { VarParInd = NewParInd }));
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
    public class MyCell:Allax.Cryptography.Cell
    {
        public List<int> VarParInd = new List<int>();
        public List<MyCell> ConstParrents = new List<MyCell>();
        public List<MyCell> VarParrents = new List<MyCell>();
        public MyCell(Allax.Cryptography.Cell C)
        {
            this.Index = C.Index;
            this.Neighbors = new List<Allax.Cryptography.Cell>(C.Neighbors);
            this.NeighboursIndexes = new List<int>(C.NeighboursIndexes);
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
        static void FindSubCyclesByHeuristicMethod(List<List<int>> Cycles, CA Aut)
        {

            Func<List<int>, int, int, List<int>> GetSubCycle = (Cycle, startInd, Count) => Enumerable.Range(0, Count).Select(i => Cycle[(startInd + i) % Cycle.Count]).ToList();
            foreach (var Cycle in Cycles)

            {
                for (int i = 0; i < Cycle.Count; i++)
                {
                    var Triple = GetSubCycle(Cycle, i, 3).RetReverse();
                    foreach (var Neighbour in Aut[Triple[0]].Neighbors.Where(c => c.Index != Aut[Triple[1]].Index))
                    {
                        if (Neighbour.NeighboursIndexes.Contains(Aut[Triple[2]].Index))
                        {
                            var x2 = Neighbour.Neighbors.First(c => c.NeighboursIndexes[1] == Neighbour.Index);
                            if (Aut[Triple[1]].NeighboursIndexes.Contains(x2.Index))
                            {
                                Console.WriteLine(string.Format("Start={0},Neighbour={1}", Triple[0], Neighbour.Index));
                            }
                        }
                    }
                }
            }
        }
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
            //FindSubCyclesByHeuristicMethod(Cycles, Aut);
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
        static List<List<List<short>>> ShowMatixes(List<List<bool>> Func)
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
            return new List<List<List<short>>> { Res, Res2 };
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
        static void WriteDiffAnalisysToFile(List<int> Key, List<int> Const, List<int> BitsNums, Dictionary<BigInteger, int> Result)
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
        static DiffResult DiffAnalisys(CA NewAut, BigInteger TrCount, List<int> K, List<int> C, Func<BigInteger, BigInteger> OTGenFunc, int StepCount, List<int> BitsNums)
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
                var D = differencialWatcher.GetPairDifference();
                var DiffCount = CurDiff.Select(i => i.Value).Sum();
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
            public int Diff = 91;
            public BigInteger K = 0;
            public BigInteger C = 0;
            public BigInteger OT = 0;
            public List<int> BitsNums = new List<int>();
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
        static Dictionary<BigInteger, DiffResult> BigDiffAnalisys_FAST(CA NewAut, int StepCount, int Log2Treshold, int KeysCount, int ConstsCount, List<int> BitsNums)
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
                    var Result = DiffAnalisys_LastRound(NewAut, BigInteger.Pow(2, Log2Treshold), K_List, C_List, OTGenFunc, StepCount, BitsNums);
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
        static void NewNumerationForCell(Cell C, List<int> NewOrderOfNeighbours)
        {
            C.Neighbors=C.Neighbors.ExtractElements(NewOrderOfNeighbours.Select(i => i - 1).ToList());
            C.NeighboursIndexes = C.Neighbors.Select(i => i.Index).ToList();
        }
        static CA SpecialNumeration(CA A)
        {
            var ret = (CA)A.Clone();
            NewNumerationForCell(ret[28], new List<int> { 1, 2, 4, 3, 5, 6 });
            NewNumerationForCell(ret[164], new List<int> { 1, 2, 6, 4, 5, 3 });
            NewNumerationForCell(ret[83], new List<int> { 6, 2, 4, 1, 5, 3 });
            NewNumerationForCell(ret[173], new List<int> { 1, 2, 3, 4, 6, 5 });
            NewNumerationForCell(ret[108], new List<int> { 4, 2, 3, 1, 5, 6 });
            NewNumerationForCell(ret[177], new List<int> { 4, 2, 3, 1, 5, 6 });
            NewNumerationForCell(ret[94], new List<int> { 1, 2, 3, 4, 6, 5 });
            NewNumerationForCell(ret[45], new List<int> { 1, 2, 4, 5, 3, 6 });
            NewNumerationForCell(ret[114], new List<int> { 1, 6, 2, 3, 4, 5 });
            NewNumerationForCell(ret[11], new List<int> { 5, 2, 3, 4, 1, 6 });
            NewNumerationForCell(ret[142], new List<int> { 1, 2, 6, 4, 5, 3 });
            NewNumerationForCell(ret[60], new List<int> { 1, 2, 3, 5, 4, 6 });
            NewNumerationForCell(ret[1], new List<int> { 1, 2, 3, 5, 4, 6 });


            return ret;
        }
        static CA SpecialNumeration_57(CA A)
        {
            var ret = (CA)A.Clone();
            NewNumerationForCell(ret[57], new List<int> { 4, 2, 3, 1, 5, 6 });
            NewNumerationForCell(ret[162], new List<int> { 4, 2, 3, 1, 5, 6 });
            NewNumerationForCell(ret[132], new List<int> { 3, 2, 4, 1, 6, 5 });
            NewNumerationForCell(ret[150], new List<int> { 4, 2, 1, 3, 5, 6 });
            NewNumerationForCell(ret[72], new List<int> { 1, 2, 3, 5, 4, 6 });
            NewNumerationForCell(ret[113], new List<int> { 1, 2, 3, 5, 6, 4 });
            NewNumerationForCell(ret[167], new List<int> { 4, 2, 3, 1, 5, 6 });
            NewNumerationForCell(ret[62], new List<int> { 1, 2, 6, 3, 4, 5 });
            NewNumerationForCell(ret[140], new List<int> { 1, 2, 4, 5, 3, 6 });
            NewNumerationForCell(ret[53], new List<int> { 4, 2, 5, 1, 6, 3 });
            NewNumerationForCell(ret[118], new List<int> { 1, 2, 3, 4, 6, 5 });
            NewNumerationForCell(ret[164], new List<int> { 1, 2, 3, 4, 6, 5 });
            NewNumerationForCell(ret[160], new List<int> { 3, 2, 4, 1, 6, 5 });
            NewNumerationForCell(ret[81], new List<int> { 4, 2, 6, 3, 5, 1 });
            NewNumerationForCell(ret[66], new List<int> { 4, 2, 6, 3, 1, 5 });
            NewNumerationForCell(ret[169], new List<int> { 1, 2, 4, 3, 5, 6 });
            NewNumerationForCell(ret[88], new List<int> { 3, 2, 1, 4, 5, 6 });
            NewNumerationForCell(ret[161], new List<int> { 3, 2, 4, 1, 5, 6 });
            NewNumerationForCell(ret[18], new List<int> { 3, 2, 4, 5, 6, 1 });
            NewNumerationForCell(ret[14], new List<int> { 1, 2, 3, 5, 4, 6 });
            NewNumerationForCell(ret[167], new List<int> { 1, 2, 3, 5, 4, 6 });
            NewNumerationForCell(ret[53], new List<int> { 4, 2, 3, 1, 5, 6 });
            NewNumerationForCell(ret[24], new List<int> { 4, 2, 3, 1, 6, 5 });


            return ret;
        }
        static bool CheckConst(List<int> Const, Dictionary<int, int> RequiredValues, Dictionary<string, int> RequiredExpressions)
        {
            var ret = true;
            RequiredValues = RequiredValues.Where(kv => kv.Key >= 128).ToDictionary();
            foreach(var K in RequiredValues.Keys)
            {
                if (Const[K - 128] != RequiredValues[K])
                {
                    ret = false;
                    return ret;
                }
            }
            foreach(var strExpr in RequiredExpressions.Keys)
            {
                List<int> listCellId = strExpr.Split('+', '*').Select(i => Convert.ToInt32(i)).ToList();
                if(listCellId.All(i=>i>127))
                {
                    var intOperPos = strExpr.IndexOfAny(new char []{ '+', '*'});
                    switch (strExpr[intOperPos])
                    {
                        case '+':
                            {
                                var sum = listCellId.Select(i => Const[i - 128]).Sum() % 2;
                                if (sum != RequiredExpressions[strExpr])
                                {
                                    ret = false;
                                    return ret;
                                }
                                break;
                            }
                        case '*':
                            {
                                var mul = (listCellId.Select(i => Const[i - 128]).All(i=>i==1))?1:0;
                                if (mul != RequiredExpressions[strExpr])
                                {
                                    ret = false;
                                    return ret;
                                }
                                break;
                            }
                    }
                }
            }
            return ret;
        }
        static bool CrossCheck(List<int> Const, List<int> OT, Dictionary<int, int> RequiredValues, Dictionary<string, int> RequiredExpressions, List<int> Key = null)
        {
            var ret = true;
            foreach (var K in RequiredValues.Keys)
            {
                if (K <64)
                {
                    if (OT != null)
                    {
                        if (OT[K] != RequiredValues[K])
                        {
                            ret = false;
                            return ret;
                        }
                    }

                }
                else
                if(K<128)
                {
                    if (Key != null)
                    {
                        if (Key[K] != RequiredValues[K])
                        {
                            ret = false;
                            return ret;
                        }
                    }
                }
                else
                {
                    if (Const != null)
                    {
                        if (Const[K - 128] != RequiredValues[K])
                        {
                            ret = false;
                            return ret;
                        }
                    }
                }
            }
            
            foreach (var strExpr in RequiredExpressions.Keys)
            {
                List<int> listCellId = strExpr.Split('+', '*').Select(i => Convert.ToInt32(i)).ToList();
                if (!listCellId.TrueForAll(i =>
                {
                    if (i < 64)
                    {
                        return (OT != null);
                    }
                    else
                    {
                        return (Const != null);
                    }
                }
                ))
                {
                    continue;
                }
                var intOperPos = strExpr.IndexOfAny(new char[] { '+', '*' });
                switch (strExpr[intOperPos])
                {
                    case '+':
                        {
                            var sum = listCellId.Select(i => (i<64)?OT[i]:Const[i - 128]).Sum() % 2;
                            if (sum != RequiredExpressions[strExpr])
                            {
                                ret = false;
                                return ret;
                            }
                            break;
                        }
                    case '*':
                        {
                            var mul = (listCellId.Select(i => (i < 64) ? OT[i] : Const[i - 128]).All(i => i == 1)) ? 1 : 0;
                            if (mul != RequiredExpressions[strExpr])
                            {
                                ret = false;
                                return ret;
                            }
                            break;
                        }
                }
            }
            return ret;

        }
        static List<List<int>> GetConsts(Dictionary<int, int> RequiredValues, Dictionary<string, int> RequiredExpressions, int Count = 4)
        {
            var Consts = new List<List<int>>();
            while (Consts.Count < Count)
            {
                var Candidate = CACryptor.GetRandomBitList(54, 27);
                //Check Const operates only with two-operand expressions
                if ((Consts.Count % 2 == 1) || CrossCheck(Candidate,null, RequiredValues, RequiredExpressions))
                {
                    if (Consts.Count == 0)
                    {
                        Consts.Add(Candidate);
                        continue;
                    }
                    var Last = Consts.Last();
                    var Distance = Last.Zip(Candidate, (i, j) => i ^ j).Sum();
                    if (Math.Abs(Distance - 27) < 4)
                    {
                        Consts.Add(Candidate);
                    }
                }
            }
            return Consts;
        }
        class CellSolution
        {
            public MyCell cell;
            public int VarMask;
            public int FaceMask;
            public int ConstMask;
            public CellSolution() { }
            public CellSolution(MyCell myCell, int varMask, int faceMask, int constMask)
            {
                this.cell = myCell;
                this.VarMask = varMask;
                this.FaceMask = faceMask;
                this.ConstMask = constMask;
            }
        }
        static List<(List<bool>, int, int)> GetConstValuesForParrents(MyCell myCell, List<List<(List<bool>, int, int)>> SubFunctions, int PredefinedValue = -1)
        {
            var Neighbors = myCell.Neighbors;
            //var VarParents = myCell.VarParInd;
            var VarParrentsInd = myCell.VarParInd;//VarParents.Select(i => i.Index).ToList();
            //if (!(VarParrentsInd.All(i => myCell.VarParInd.Contains(i)) && (VarParrentsInd.Count == myCell.VarParInd.Count)))
             //   throw new NotImplementedException("I used ParInd for VAr Parrent earlier, but it's not so");
            var ConstParents = myCell.ConstParrents;
            var ConstParrentsInd = ConstParents.Select(i => i.Index).ToList();
            var lVarMask = Neighbors.Select(i => (VarParrentsInd.Contains(i.Index)) ? true : false).ToList();
            var intVarMask = (int)WayConverter.ToLong(lVarMask);
            var FunctionCandidates = SubFunctions[intVarMask];
            /*Trash
            var ConstFunctionsFromFunctionCandidates = FunctionCandidates.Where(i => { var sum = i.ConvertAll(j => Convert.ToInt32(j)).Sum(); return (sum == 0) || (sum == i.Count); }).ToList();
            X2FuncCandidates.Add(CurrentX2Cell, ConstFunctionsFromFunctionCandidates);
            */
            //Choose candidates
            var ConstFunctionsFromFunctionCandidates = FunctionCandidates.Where(i =>
            {
                var intFunc = i.Item1.ConvertAll(j => Convert.ToInt32(j));
                //var sum = Enumerable.Range(0, intFunc.Count).Select(x => (intFunc[(x + VarParrentsInd.Count)%intFunc.Count] ^ intFunc[x])).ToList().Sum();
                if(PredefinedValue!=-1)
                {
                    if (intFunc.Count > 0 && intFunc[0] != PredefinedValue)
                        return false;
                }
                return (intFunc.First() == intFunc.Last());//(sum == 0) || (sum == intFunc.Count);
            }
            ).ToList();
            return ConstFunctionsFromFunctionCandidates;
        }
        static Dictionary<MyCell, (int, List<(int, int)>)> CombineListOfConstsIntoFaces(Dictionary<MyCell, List<(List<bool>, int, int)>> DicCells)
        {
            // cell, varmask, list<( facemask, constmasks)>
            var DicCellsFaceMasked = new Dictionary<MyCell, (int, List<(int, int)>)>();
            foreach (var tCell in DicCells)
            {
                var cell = tCell.Key;
                var intVarMask = tCell.Value[0].Item2;
                DicCellsFaceMasked.Add(cell, (intVarMask, new List<(int, int)>()));
                //operating with const masks,searching subcube faces:
                var lConstMasks = tCell.Value.Select(i => i.Item3).ToList();
                var intLogUpperGroupLimit = Enumerable.Range(0, 6).Last(i => (1 << i) <= lConstMasks.Count);
                Enumerable.Range(0, intLogUpperGroupLimit+1).ToList().ForEach(
                    i =>
                    {
                        var CombinationsK = lConstMasks.CombinationIterator(1<<i);
                        //Is this a face? check func
                        //...
                        while (CombinationsK.MoveNext())
                        {
                            var Combination = (List<int>)CombinationsK.Current;
                            var Count = Combination.Count;
                            if (Count == 1)
                            {
                                DicCellsFaceMasked[cell].Item2.Add((0, Combination[0]));
                                continue;
                            }
                            //var face = Combination.Select(n => n ^ Combination[0]).ToList();
                            //get face mask
                            var FixedBitsInFace = Enumerable.Range(0, 6).Where(bitNum =>
                            {
                                var Mask = 1 << bitNum;
                                var sum = Enumerable.Range(0, Count).Select(vector => ((Combination[vector] & Mask) == 0) ? 0 : 1).Sum();
                                return ((sum == Count) || (sum == 0));
                            }).ToList();
                            var intVarBitsCountInFace = 6 - FixedBitsInFace.Count;
                            if ((1 << intVarBitsCountInFace) != Count)
                                continue;
                            //add comb to dict if all vectors of face in combination
                            var FaceMask = (int)WayConverter.ToLong(Enumerable.Range(0, 6).Select(bit => (FixedBitsInFace.Contains(bit)) ? false : true).ToList().RetReverse());
                            DicCellsFaceMasked[cell].Item2.Add((FaceMask, Combination[0] & ~FaceMask));
                        }
                    });
                //
            }
            return DicCellsFaceMasked;
        }
        static List<Dictionary<MyCell, int>> MergeSolution(List<Dictionary<MyCell, int>> solutions, Dictionary<MyCell, int> solution)
        {
            var ret = new List<Dictionary<MyCell, int>>(solutions);
            for (int SolInd = 0; SolInd < solutions.Count; SolInd++)
            {
                var CurSol = solutions[SolInd];
                if (CurSol.Count == solution.Count)
                {
                    if (CurSol.Zip(solution, (kv1, kv2) => kv1.Key.Index == kv2.Key.Index).All())
                    {
                        //CurSol and TarSol have the same cells
                        var CellIndex = 0;
                        var Sum = CurSol.Zip(solution, (kv1, kv2) => { if (kv1.Value != kv2.Value) { CellIndex = kv1.Key.Index; return 1; } else return 0; }).Sum();
                        if (Sum == 1)
                        {
                            var NewSolution = CurSol.Where(i => i.Key.Index != CellIndex).ToDictionary();
                            var NewSolutions = solutions.Where(i => i != CurSol).ToList();
                            ret = MergeSolution(NewSolutions, NewSolution);
                            return ret;
                        }
                        if (Sum == 0)
                            return ret;
                    }
                }
                if (CurSol.Count < solution.Count)
                {
                    //Same Cells
                    if (CurSol.Keys.All(CurKey => solution.Keys.MyContains(CurKey)))
                    {
                        //check for same values
                        if (CurSol.All(kv => solution.First(i => i.Key.Index == kv.Key.Index).Value == kv.Value))
                            return ret;
                    }
                }
                if (CurSol.Count > solution.Count)
                {
                    if (solution.Keys.All(SolKey => CurSol.Keys.MyContains(SolKey)))
                    {
                        //check for same values
                        if (solution.All(kv => CurSol.First(i => i.Key.Index == kv.Key.Index).Value == kv.Value))
                        {
                            var ExtraRet = Enumerable.Range(SolInd, solutions.Count - SolInd).Select(i => solutions[i]).Where(s =>
                            !((s.Count > solution.Count) &&
                            (solution.Keys.All(SolKey => s.Keys.MyContains(SolKey))) &&
                            (solution.All(kv => s.First(i => i.Key.Index == kv.Key.Index).Value == kv.Value))));
                            ret = ret.Take(SolInd).ToList();
                            ret.AddRange(ExtraRet);
                            ret = MergeSolution(ret, solution);
                            return ret;
                        }
                    }
                }
            }
            ret.Add(solution);
            return ret;
        }
        static List<Dictionary<MyCell, int>> GetSolutionsForListOfCells(List<MyCell> CellList, List<List<(List<bool>, int, int)>> SubFunctions, Dictionary<MyCell, int> predefinedCells = null)
        {
            var FuncCandidates = new Dictionary<MyCell, List<(List<bool>, int, int)>>();
            foreach (var CurrentCell in CellList)
            {
                var Value = -1;
                if (predefinedCells != null)
                {
                    var ResultIndex = predefinedCells.Keys.ToList().FindIndex(i => i.Index == CurrentCell.Index);
                    if (ResultIndex > 0)
                    {
                        Value = predefinedCells[predefinedCells.Keys.ElementAt(ResultIndex)];
                    }
                }
                var ConstFunctionsFromFunctionCandidates = GetConstValuesForParrents(CurrentCell, SubFunctions, Value);
                FuncCandidates.Add(CurrentCell, ConstFunctionsFromFunctionCandidates);
            }
            //Check that every cell have const expression
            foreach (var T in FuncCandidates)
            {
                if (T.Value.Count == 0)
                    //reject current diff graph struct
                    return new List<Dictionary<MyCell, int>>();
            }
            //Extract const values from candidates, convert possible const mask to int
            // cell, varmask, list<( facemask, constmasks)>
            var X2ConstValues = CombineListOfConstsIntoFaces(FuncCandidates);
            // Remove faces wich are already included in other faces
            X2ConstValues.Values.ToList().ForEach(i =>
            {
                var faceMaskList = i.Item2;
                if (i.Item2.Count == 0)
                    return;
                    //throw new NotImplementedException();
                var copyfaceMaskList = new List<(int, int)>(faceMaskList);
                copyfaceMaskList.ForEach(cur =>
                {
                    if (!faceMaskList.All(target => (((target.Item1 == cur.Item1) && (target.Item2 == cur.Item2)) || (!target.Item1.BitCompareG(cur.Item1)) || (((target.Item1.BitCompareG(cur.Item1)) && (target.Item2!=((~target.Item1)&cur.Item2)))))))
                        faceMaskList.Remove(cur);
                });
            });
            //Sort combinations in star count order
            X2ConstValues.Values.ToList().ForEach(i => i.Item2.Sort((comb1, comb2) =>
            {
                var startCountInFace1 = WayConverter.ToIntList(comb1.Item1, 6).Sum();
                var startCountInFace2 = WayConverter.ToIntList(comb2.Item1, 6).Sum();
                //decreasing order
                return startCountInFace2.CompareTo(startCountInFace1);
            }));
            // Drop bad solutions, greedy alg
            //Console.WriteLine("greedy search!");
            X2ConstValues.Values.ToList().ForEach(i =>
            {
                var cellSolutions = i.Item2;
                var startCountInFace = WayConverter.ToIntList(cellSolutions[0].Item1, 6).Sum();
                for (int index=0;index<cellSolutions.Count;index++)
                {
                    var startCountInFaceCur = WayConverter.ToIntList(cellSolutions[index].Item1, 6).Sum();
                    if(true&&(((startCountInFaceCur<startCountInFace-LIMIT_1_STARS)) || (index > LIMIT_2_MAX)))
                    { 
                        i.Item2.RemoveRange(index, i.Item2.Count - index);
                        break;
                    }
                }
                });
            //Combine possible combinations
            var PossibleSolutions = new List<List<CellSolution>>();
            foreach (var myCell in X2ConstValues)
            {
                PossibleSolutions.Add(new List<CellSolution>(myCell.Value.Item2.Select(i => new CellSolution(myCell.Key, myCell.Value.Item1, i.Item1, i.Item2))));
            }
            var solutionsIterator = PossibleSolutions.MultiListSerialIterator();
            //get Solutions for t-1 level
            var Solutions = new List<Dictionary<MyCell, int>>();
            while (solutionsIterator.MoveNext())
            {
                var PossibleSolution = (List<CellSolution>)solutionsIterator.Current;
                //try to combine
                var solution = new Dictionary<MyCell, int>();
                bool RightSolution = true;
                foreach (var cell in PossibleSolution)
                {
                    var ConstMask = cell.ConstMask;
                    var FaceMask = cell.FaceMask;
                    var VarMask = cell.VarMask;
                    var FixedParentsMask = (~VarMask) & (~FaceMask);
                    for (int i = 0; i < 6; i++)
                    {
                        var Neighbour = new MyCell(cell.cell.Neighbors[6 - 1 - i]);
                        //if current neighbout in fixedparrentsmask
                        if (((1 << i) & FixedParentsMask) != 0)
                        {
                            var Value = (((1 << i) & ConstMask) == 0) ? 0 : 1;
                            if (!solution.Keys.All(k => k.Index != Neighbour.Index))
                            {
                                if (solution.First(kv => kv.Key.Index == Neighbour.Index).Value != Value)
                                {
                                    RightSolution = false;
                                    break;
                                }
                            }
                            else
                            {
                                solution.Add(Neighbour, Value);
                            }
                            
                        }
                    }
                    if (!RightSolution)
                        break;
                }
                if (!RightSolution)
                    continue;
                //Search already found solution to merge
                //sort new solution, required for this func
                solution = solution.OrderBy(kv => kv.Key.Index).ToDictionary();
                Solutions = MergeSolution(Solutions, solution);
            }
            if (LIMIT_3_DISCARDBIGSOLUTIONS)
            {
                if (Solutions.Count > 0)
                    Solutions.Sort((b, a) => b.Count.CompareTo(a.Count));
                Solutions = Solutions.TakeWhile(i => i.Count == Solutions[0].Count).ToList();
            }
            return Solutions;
        }
        static (List<MyCell>, List<MyCell>) ExtractConstCells(List<List<MyCell>> DiffStruct, int Step)
        {
            var DiffLayer = DiffStruct[Step];
            //All neighbors of last diff layer
            var CurrentConstLayer = new List<MyCell>();
            var X2CellsFromConstLayer = new List<MyCell>();
            DiffLayer.ForEach(cell => {
                foreach (var c in cell.Neighbors)
                {
                    //Drop cell if it's in next Layer of DiffStruct
                    if(Step<DiffStruct.Count-1)
                    {
                        var DiffNextLayer = DiffStruct[Step + 1];
                        if (!DiffNextLayer.All(i=>(i.Index!=c.Index)))
                        {
                            //drop cell
                            continue;
                        }
                    }
                    //Convert each neighbor of cell from LastDifLayer
                    var myC = new MyCell(c);
                    //Remember parrent of this neighbor - current cell form last dif layer
                    myC.VarParInd.Add(cell.Index);
                    if (CurrentConstLayer.All(j => j.Index != myC.Index))
                    {
                        //if neighbor didn't added to ConstLayerAfterLastDif layer already. i checking that beacuse 
                        //another cells from LastDifLayer may contain that neighbor as own neighbor
                        CurrentConstLayer.Add(myC);
                        //if current cell is X2 neighbor for neighbor
                        if (cell.Index == myC.Neighbors[1].Index)
                        {
                            X2CellsFromConstLayer.Add(myC);
                        }
                    }
                    else
                    {
                        //ConstLayerAfterLast contains myC, find it in CostLayer array
                        myC = CurrentConstLayer.Find(j => j.Index == myC.Index);
                        //add current cell as parent
                        myC.VarParInd.Add(cell.Index);
                        //if X2CellsFromConstLayer contains already this neighbor then remove it from array. It means that it's possible to
                        //create expression for neibors of this neighbor to get const value for neighbor
                        if (!X2CellsFromConstLayer.All(j => j.Index != myC.Index))
                        {
                            X2CellsFromConstLayer.Remove(myC);
                        }
                    }
                }
            });
            return (CurrentConstLayer, X2CellsFromConstLayer);
        }
        static List<Dictionary<MyCell, int>> RemoveRedudantSolutions(List<Dictionary<MyCell, int>> solutions)
        {
            //trash func
            return solutions;
            if (solutions.Count == 0)
                return solutions;
            //Remove equal solutions
            for(int SolInd=0;SolInd<solutions.Count;SolInd++)
            {
                var IndexList = new List<int>();
                if (!IndexList.Contains(SolInd))
                {
                    var CurSol = solutions[SolInd];
                    for (int i = SolInd+1; i < solutions.Count; i++)
                    {
                        var TarSol = solutions[i];
                        if(CurSol.Count==TarSol.Count)
                        {
                            if(CurSol.All(kv=>
                                !TarSol.All(kv2=>(kv.Key.Index!=kv2.Key.Index)||(kv.Value!=kv2.Value))
                            ))
                            {
                                IndexList.Add(i);
                            }
                        }
                    }

                }
                if (IndexList.Count > 0)
                {
                    IndexList.Sort();
                    IndexList.Reverse();
                    IndexList.ForEach(i => solutions.RemoveAt(i));
                }
            }
            //
            //Remove extra solutions
            /*
            // cell, varmask, list<( facemask, constmasks)>
            var DicCellsFaceMasked = new List<Dictionary<MyCell, int>>();
                var lConstMasks = solutions;
                var intLogUpperGroupLimit = Enumerable.Range(0, 32).Last(i => (1 << i) <= lConstMasks.Count);
                Enumerable.Range(0, intLogUpperGroupLimit).ToList().ForEach(
                    i =>
                    {
                        var CombinationsK = lConstMasks.CombinationIterator(1 << i);
                        //Is this a face? check func
                        //...
                        while (CombinationsK.MoveNext())
                        {
                            var Combination = (List<int>)CombinationsK.Current;
                            var Count = Combination.Count;
                            if (Count == 1)
                            {
                                DicCellsFaceMasked[cell].Item2.Add((0, Combination[0]));
                                continue;
                            }
                            //var face = Combination.Select(n => n ^ Combination[0]).ToList();
                            //get face mask
                            var FixedBitsInFace = Enumerable.Range(0, 6).Where(bitNum =>
                            {
                                var Mask = 1 << bitNum;
                                var sum = Enumerable.Range(0, Count).Select(vector => ((Combination[vector] & Mask) == 0) ? 0 : 1).Sum();
                                return ((sum == Count) || (sum == 0));
                            }).ToList();
                            var intVarBitsCountInFace = 6 - FixedBitsInFace.Count;
                            if ((1 << intVarBitsCountInFace) != Count)
                                continue;
                            //add comb to dict if all vectors of face in combination
                            var FaceMask = (int)WayConverter.ToLong(Enumerable.Range(0, 6).Select(bit => (FixedBitsInFace.Contains(bit)) ? false : true).ToList().RetReverse());
                            DicCellsFaceMasked[cell].Item2.Add((FaceMask, Combination[0] & ~FaceMask));
                        }
                    });
                //
            return DicCellsFaceMasked;
            */
            for (int SolInd = 0; SolInd < solutions.Count; SolInd++)
            {
                var IndexList = new List<int>();
                if (!IndexList.Contains(SolInd))
                {
                    var CurSol = solutions[SolInd];
                    for (int i = SolInd + 1; i < solutions.Count; i++)
                    {
                        var TarSol = solutions[i];
                        if (CurSol.Count == TarSol.Count)
                        {
                            
                        }
                    }

                }
                if (IndexList.Count > 0)
                {
                    IndexList.Sort();
                    IndexList.Reverse();
                    IndexList.ForEach(i => solutions.RemoveAt(i));
                }
            }
            throw new NotImplementedException();
        }
        static IEnumerator SUPERDIFFERENCIALCRYPTOANALISYS(List<List<MyCell>> DiffStruct, List<List<(List<bool>, int, int)>> SubFunctions, Dictionary<MyCell,int> ConstCellsAtStep=null, int InverseStep = 0)
        {
            //get Solutions for X2 at t-1 level
            // Solution -> (Fixed Cell at t-1 level, Value for Cell)
            var Deep = DiffStruct.Count - 1 - InverseStep;
            if (Deep < 0)
            {
                yield return ConstCellsAtStep;
                yield break;
            }   

            var CurrentConstAndX2Layer = ExtractConstCells(DiffStruct, Deep);
            var CurrentConstLayer = CurrentConstAndX2Layer.Item1;
            var X2CellsFromConstLayer = CurrentConstAndX2Layer.Item2;
            if(InverseStep==0)
            {
                if (X2CellsFromConstLayer.Count != 0)
                {
                    yield break; //skip analisys beacuse of X2 Neighbours on next layer which must be const
                }
            }
            Console.WriteLine();
            if(ConstCellsAtStep!=null)
            {
                foreach(var cell in ConstCellsAtStep)
                {
                    if(CurrentConstLayer.All(i=>i.Index!=cell.Key.Index))
                    {
                        CurrentConstLayer.Add(cell.Key);
                    }
                }
            }
            Console.Write(string.Format("Current deep:{0} ", Deep+1));
            CurrentConstLayer.ForEach(i => Console.Write(string.Format("{0}-", i.Index)));
            var LayerSolutions = GetSolutionsForListOfCells(CurrentConstLayer, SubFunctions, ConstCellsAtStep);
            //Combine Solutions
            //LayerSolutions = RemoveRedudantSolutions(LayerSolutions);
            //
            foreach (var Solution in LayerSolutions)
            {
                var It = SUPERDIFFERENCIALCRYPTOANALISYS(DiffStruct, SubFunctions, Solution, InverseStep + 1);
                while(It.MoveNext())
                {
                    var NextSolution = It.Current;
                    yield return NextSolution;
                }
            }
            yield break;
            //Lets organaize bruteforce among all solutions
            throw new NotImplementedException();
        }
        static List<List<bool>> ExtractFuncFromCACryptor(CACryptor CACr)
        {
            return Enumerable.Range(0, 1 << 6).Select(i => CACr.FN.Automata.F.GetResult(WayConverter.ToList(i, 6).ConvertAll(k => (k == false) ? 0 : 1))).ToList().ConvertAll(l => (l == 0) ? new List<bool> { false } : new List<bool> { true });
        }
        static List<List<(List<bool>, int, int)>> GetSubFunctions(List<List<bool>> Func)
        {
            return
                Enumerable.Range(0, 1 << 6).Select(intVarMask => //VarMask
                {
                    var ConstCount = 6 - WayConverter.ToIntList(intVarMask, 6).Sum();
                    var lstVarMask = WayConverter.ToList(intVarMask, 6);
                    return Enumerable.Range(0, 1 << ConstCount).Select(ConstNum => //ConstValues
                    {
                        var ConstBits = WayConverter.ToIntList(ConstNum, ConstCount);
                        //Get required indexes for Func and then get all func

                        var iterConstBits = ConstBits.GetEnumerator();
                        //Set Const values on their places in EnumMask
                        var EnumMask = lstVarMask.Select(varMaskBit =>
                        {
                            if (!varMaskBit)
                                iterConstBits.MoveNext();
                            return (!varMaskBit) ? ((iterConstBits.Current == 1) ? true : false) : false;
                        }).ToList();
                        var intEnumMask = (int)WayConverter.ToLong(EnumMask);
                        return (Enumerable.Range(0, 1 << 6).Where(m => //Func Arg Enum
                        {
                            //xor const values in m, make them zero. Make and with constmask (~i) so var bits become zero. 
                            //If all result is zero then const bits in m are the same as required.
                            return (((~intVarMask) & (intEnumMask ^ m)) == 0);
                        }

                            ).Select(k => Func[k][0]).ToList(), intVarMask, intEnumMask);
                    }
                    ).ToList();
                }
                      ).ToList();
        }
        static bool IsNextLayerConst(List<List<MyCell>> sg)
        {
            //Check that on next step there will be no X2 neighbours with one edge

            var LastDifLayer = sg.Last();
            //All neighbors of last diff layer
            var ConstLayerAfterLast = new List<MyCell>();
            var X2CellsFromConstLayer = new List<MyCell>();
            LastDifLayer.ForEach(cell => {
                foreach (var c in cell.Neighbors)
                {
                    //Convert each neighbor of cell from LastDifLayer
                    var myC = new MyCell(c);
                    //Remember parrent of this neighbor - current cell form last dif layer
                    myC.VarParInd.Add(cell.Index);
                    if (ConstLayerAfterLast.All(i => i.Index != myC.Index))
                    {
                        //if neighbor didn't added to ConstLayerAfterLastDif layer already. i checking that beacuse 
                        //another cells from LastDifLayer may contain that neighbor as own neighbor
                        ConstLayerAfterLast.Add(myC);
                        //if current cell is X2 neighbor for neighbor
                        if (cell.Index == myC.Neighbors[1].Index)
                        {
                            X2CellsFromConstLayer.Add(myC);
                        }
                    }
                    else
                    {
                        //ConstLayerAfterLast contains myC, find it in CostLayer array
                        myC = ConstLayerAfterLast.Find(i => i.Index == myC.Index);
                        //add current cell as parent
                        myC.VarParInd.Add(cell.Index);
                        //if X2CellsFromConstLayer contains already this neighbor then remove it from array. It means that it's possible to
                        //create expression for neibors of this neighbor to get const value for neighbor
                        if (!X2CellsFromConstLayer.All(i => i.Index != myC.Index))
                        {
                            X2CellsFromConstLayer.Remove(myC);
                        }
                    }
                }
            });
            return (X2CellsFromConstLayer.Count == 0);
        }
        static void PrintStruct(List<List<MyCell>> sg)
        {
            //Print Struct
            Console.WriteLine("\nStruct:\n");
            sg.ForEach(i => {
                i.ForEach(j => Console.Write(string.Format("{0}{1}-", j.Index, "(" + string.Join(",", j.VarParInd.ToArray()) + ")")));
                Console.Write(";");
            });
            Console.WriteLine();
        }
        static int LIMIT_1_STARS=0;
        static int LIMIT_2_MAX = 0;
        static bool LIMIT_3_DISCARDBIGSOLUTIONS = true;
        static void Main(string[] args)
        {

            //Создаю стандартный шифр на 128
            var CACr = new CACryptor(7, 128);
            //Граф на 182 вершины выбран. Извлечем Функцию в нужном нам формате
            var Func = ExtractFuncFromCACryptor(CACr);
            var SubFunctions = GetSubFunctions(Func);
            //var MX = ShowMatixes(Func);
            Console.WriteLine("2-factor:");
            // Нумеруем 2-фактор
            var NewAut_before = Numerate((CA)CACr.FN.Automata.Clone());
            //посмотрим proof of concept 

            //NewAut_before = SpecialNumeration_57(NewAut_before);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Lets find required subgraphs of Cellular Automata's work.");
            Console.WriteLine("Start cells Count:");
            var SNCount = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine();
            Console.WriteLine("Max Deep of Structs:");
            var Steps = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine();
            Console.WriteLine("Cells have numerous variants of neighbours's cells values for making current cell stay const.\n" +
                "Variants are represented as binary vectors and boolean cube's faces and sorted in decreasing order of stars count in face. \n" +
                "Put a difference of stars in such faces for every cell to limit brute force (0-5):");
            LIMIT_1_STARS = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine();
            Console.WriteLine("And the max of possible variants for every cell is:");
            LIMIT_2_MAX = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine();
            Console.WriteLine("Start cell:");
            var StartCell = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine();
            Console.WriteLine("Discard big solutions? (y\\N)");
            LIMIT_3_DISCARDBIGSOLUTIONS = "yesYes".Contains(Console.ReadLine());
            Console.WriteLine();
            var sgItr = NewAut_before.GetWorkSubgraphs(SNCount, Steps,0,null,true,StartCell);
            while (sgItr.MoveNext())
            {
                var sg = (List<List<MyCell>>)sgItr.Current; // Get current struct

                if (!IsNextLayerConst(sg))
                    continue;
                PrintStruct(sg);
                
                var Solutions = SUPERDIFFERENCIALCRYPTOANALISYS(sg, SubFunctions);
                while (Solutions.MoveNext())
                {
                    var Solution = (Dictionary<MyCell, int>)Solutions.Current;
                    var intDicSolution = Solution.ToDictionary(i => i.Key.Index, j => j.Value);
                    Console.WriteLine("Solution:");
                    intDicSolution.ToList().ForEach(j => Console.Write(string.Format("{0}{1}-", j.Key, "(" + j.Value+ ")")));
                    Console.WriteLine();
                    //Check existance of solution with supercheck

                    //CrossCheck(null, null, intDicSolution, null);


                    var Key = CACryptor.GetRandomBitList(128);
                    while(!CrossCheck(null, null,intDicSolution, new Dictionary<string, int>(), Key))
                    {
                        Key = CACryptor.GetRandomBitList(128);
                    }
                    Key = Key.Skip(64).ToList();
                    var RequiredOTValues = intDicSolution.Where(kv => kv.Key < 64).ToDictionary();
                    var RequiredCValues = intDicSolution.Where(kv => kv.Key >= 128).ToDictionary();
                    var Converted = MyIterator.ListToBigInteger(Key).ToByteArray().Take(8).Reverse().ToArray();
                    Console.WriteLine(string.Format("Key:{0}", BitConverter.ToString(Converted, 0, Converted.Length)));

                    var Consts = GetConsts(intDicSolution, new Dictionary<string, int>());

                    foreach (var C in Consts)
                    {
                        Console.WriteLine(string.Format("Const:{0}, Checked: {1}", BitConverter.ToString(MyIterator.ListToBigInteger(C).ToByteArray().Reverse().ToArray(), 0, MyIterator.ListToBigInteger(C).ToByteArray().Reverse().ToArray().Length), CheckConst(C, RequiredCValues, new Dictionary<string, int>())));
                    }
            ;
                    CACr.SetConstants(Consts);
                    Func<BigInteger, BigInteger> OTGenFunc = (I) => {
                        while (true)
                        {
                            var ret = CACryptor.GetRandomBitList(64);
                            foreach (var Id in RequiredOTValues.Keys)
                            {
                                ret[Id] = RequiredOTValues[Id];
                            }
                            if (CrossCheck(Consts.First(), ret, RequiredCValues.Concat(RequiredOTValues).ToDictionary(i => i.Key, i => i.Value), new Dictionary<string, int>()))
                                return MyIterator.ListToBigInteger(ret);
                        }
                    };
                    var BitsNums = new List<int> { 28 }; //new List<int> { 57 };
                    var Result = DiffAnalisys(NewAut_before, 1, Key.Take(64).ToList(), Consts.First(), OTGenFunc, 7, sg.First().Select(i=>i.Index).ToList());
                    ;
                    Console.WriteLine(string.Format("OT:{0}", BitConverter.ToString(Result.OT.ToByteArray().Take(8).Reverse().ToArray(), 0, Result.OT.ToByteArray().Take(8).Reverse().ToArray().Length)));

                    //

                    //Print success probability
                    if (Result.Diff == 0)
                        Console.WriteLine(string.Format("Success prob: {0}", Math.Pow(.5, intDicSolution.Where(i => (i.Key > 63) && (i.Key < 128)).Count())));
                }

                continue; //skip analisys
                //Make choise take it or drop
            }
            Console.WriteLine("\n\nFinished.");
            Console.ReadLine();
        }
        static void Main2(string[] args)
        {
            //Создаю стандартный шифр на 128
           var CACr = new Allax.Cryptography.CACryptor(7, 128);
            //var CACr_2 = new CACryptor(7, 128, "x3x4+x1x3+x1+x2+1", 4);
            

            //throw new NotImplementedException("Subfunctions have non-unique constmasks for same varmask");
            //var MX = ShowMatixes(Func);
            //var Func_2 = Enumerable.Range(0, 1 << 4).Select(i => CACr_2.FN.Automata.F.GetResult(WayConverter.ToList(i, 4).ConvertAll(k => (k == false) ? 0 : 1))).ToList().ConvertAll(l => (l == 0) ? new List<bool> { false } : new List<bool> { true });
            //ShowMatixes(Func_2);
            // Нумеруем 2-фактор
            var NewAut_before = Numerate((CA)CACr.FN.Automata.Clone());
            //посмотрим proof of concept 

            NewAut_before = SpecialNumeration_57(NewAut_before);
            //Get All Structures - Differencials
           
                //TODO: Lets do ditry hacks for checking my super-collision-differencial analisys
                //First, Find expressions
                /*
                var Expressions = new Dictionary<string, int>();
                //Lets find nodes In ContLayer After last differencial layer that have X2 edge to nodes from last differencial layer
                var X2fromConstLayerAfterLast = new List<MyCell>();
                foreach(var node in ConstLayerAfterLast)
                {
                    //if last dif layer have cell that X2 for this cell
                    if(!LastDifLayer.All(i=>i.Index!=node.Neighbors[1].Index))
                    {
                        X2fromConstLayerAfterLast.Add(node);
                    }
                    //Find parrents for node inside last dif layer. Note that ParInd contains only var parrents
                    node.ConstParrents = node.Neighbors.Where(i => !node.VarParInd.Contains(i.Index)).ToList().ConvertAll(i => new MyCell(i));
                    node.VarParrents = node.Neighbors.Where(i => node.VarParInd.Contains(i.Index)).ToList().ConvertAll(i => new MyCell(i));
                }6

                */
                /*
                foreach(var node in X2fromConstLayerAfterLast)
                {
                    var VarMask = (short)WayConverter.ToLong(node.Neighbors.Select(i => (!node.VarParrents.All(j => j.Index != i.Index)) ? true: false).ToList());
                    var ExpressionsCandidates = SubFunctions[VarMask];

                }
                */
                





            //My proof of concept
            var NewAut = SpecialNumeration(NewAut_before);
            //запихиваем граф в шифратор
            CACr.FN.AssignAutomata(NewAut);

            //SearchCollision((CA)NewAut.Clone());
            /*114, 118, 80, 61, 174, 115, 81, 172, 47, 136, 104, 100, 91, 23, 86, 144, 51, 22, 44, 74, 66, 13, 7, 46,
10, 78, 112, 129, 163, 171, 60, 102, 101, 116, 82, 133, 165, 125, 142, 8, 25, 166, 37, 0, 138, 6, 67, 76, 92, 169, 95, 111, 84, 34, 126, 62, 85, 4, 58, 9, 128,*/

            Console.WriteLine();
            /*
             var RequiredOTValues = new Dictionary<int, int> {
                { 2, 1}, { 14, 1}, { 18, 1}, { 19, 1}, { 39, 1}, { 62, 1}, { 53, 1}, { 24, 1}, { 11, 1}, { 21, 1},
                { 38, 1}, { 6, 1}, { 36, 1}, { 7, 1}, { 33, 1}, { 42, 1}, { 59, 1}, { 54, 1}, { 20, 1},
                { 26, 1},  {25, 0},
                { 45, 0}, { 1, 0}, { 30, 0}, };
            //Биты констант, необходимые для создания коллизии
            var RequiredCValues = new Dictionary<int, int> { { 148, 1 }, { 161, 1 }, { 158, 1 }, { 159, 1 }, { 169, 1 }, { 140, 1 }, { 167, 1 }, { 146, 0 } };
            //Соотношения между битами, необходимые для создания коллизии
            var RequiredExpressions = new Dictionary<string, int> { { "28+149", 1 }
                , {"125+5", 1 }, {"86+149", 1 }, {"32+133", 1 }, {"61+131", 1 }, { "145+15", 1 }
        };*/
            //Биты открытого текста, необходимые для создания коллизии
            var RequiredOTValues = new Dictionary<int, int> { { 0, 0 }, { 6, 0 }, { 8, 0 }, { 14, 0 }, { 19, 0 }, { 33, 0 }, { 37, 0 }, { 40, 0 },{ 41, 1 }, { 45, 0 }, { 46, 0 }, { 52, 1 }, { 54, 0 }, { 60, 1 } };
            //Биты констант, необходимые для создания коллизии
            var RequiredCValues = new Dictionary<int, int> { { 180, 0 }, { 160, 0 }, { 142, 0 }, { 154, 0 }, { 162, 0 }, { 175, 0 }, { 176, 1 }, { 147, 0 }, };
            //Соотношения между битами, необходимые для создания коллизии
            var RequiredExpressions = new Dictionary<string, int> { { "26+138", 1 }
                , {"11+34", 1 }, {"178*129", 0 }, {"159*144", 0 }, {"158+156", 1 } };
            var Key = CACryptor.GetRandomBitList(128);
            Console.WriteLine(string.Format("Key:{0}", BitConverter.ToString(MyIterator.ListToBigInteger(Key).ToByteArray().Reverse().ToArray(), 0, 16)));

            var Consts = //GetConsts(RequiredCValues, RequiredExpressions);
                (new List<BigInteger> {
                    new BigInteger((new byte[] { 0x00, 0x08, 0x85, 0x79, 0xF4, 0x87, 0xBC, 0xB8 }).Reverse().ToArray()),
                    new BigInteger((new byte[] { 0x00, 0x04, 0x28, 0x2F, 0x6E, 0x24, 0xF3, 0x7E}).Reverse().ToArray()),
                    new BigInteger((new byte[] { 0x00, 0x07, 0xF1, 0x49, 0x36, 0x17, 0xC9, 0xAC, }).Reverse().ToArray()),
                    new BigInteger((new byte[] { 0x00, 0x33, 0x0A, 0xEE, 0x16, 0x3A, 0xE3, 0x13 }).Reverse().ToArray()),

                }).Select(i => MyIterator.BigIntegerToList(i, 54)).ToList();
            
            foreach (var C in Consts)
            {
                Console.WriteLine(string.Format("Const:{0}, Checked: {1}", BitConverter.ToString(MyIterator.ListToBigInteger(C).ToByteArray().Reverse().ToArray(), 0, 7), CheckConst(C, RequiredCValues, RequiredExpressions)));
            }
            ;
            CACr.SetConstants(Consts);
            Func<BigInteger, BigInteger> OTGenFunc = (I) => { while (true)
                {
                    var ret = CACryptor.GetRandomBitList(64);
                    foreach(var Id in RequiredOTValues.Keys)
                    {
                        ret[Id] = RequiredOTValues[Id];
                    }
                if (CrossCheck(Consts.First(), ret, RequiredCValues.Concat(RequiredOTValues).ToDictionary(i =>  i.Key, i=>i.Value ), RequiredExpressions))
                    return MyIterator.ListToBigInteger(ret);
                }
            };
            var BitsNums = new List<int> { 28 }; //new List<int> { 57 };
            var Result = DiffAnalisys(NewAut, 1, Key.Take(64).ToList(), Consts.First(), OTGenFunc, 7, BitsNums);
            ;
            //BigDiffAnalisys_FAST(NewAut, 8, 7, 100, 100, new List<int> { 28 });

            // int ultramin = 91;
            // DiffResult UltraMinR = new DiffResult();;
            // for(int i=0;i<64;i++)
            // {
            //     var BitsNums = new List<int> { i};
            //     var Result = BigDiffAnalisys_FAST(NewAut, 7, 4, 100, 100, BitsNums);
            //     foreach (var r in Result)
            //     {
            //         if (Math.Abs(91 - r.Value.Diff) > Math.Abs(91 - ultramin))
            //         {
            //             ultramin = r.Value.Diff;
            //             UltraMinR = r.Value;
            //         }
            //     }
            // }
            //Console.WriteLine(Math.Abs(91-ultramin));
        }
    }
}
