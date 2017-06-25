using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Allax
{
	public delegate void ADDSOLUTIONHANDLER(Solution s);
    public delegate void TASKHANDLER(ITask T);
    public delegate void PROGRESSHANDLER(double Progress);
    [Serializable()]
    public struct DBNote
    {
        public DBNote(List<List<short>> CorMatrix, List<List<short>> DifMatrix)
        {
            this.DStates = new List<List<BlockState>>();
            DStates.AddRange(Enumerable.Range(0, CorMatrix.Count).Select(i=>new List<BlockState>()));
            this.LStates = new List<List<BlockState>>();
            LStates.AddRange(Enumerable.Range(0, CorMatrix.Count).Select(i => new List<BlockState>()));
            //           this.LStates = new List<BlockState>(CorMatrix.Count * CorMatrix.Count);
            for (int row = 1; row < CorMatrix.Count; row++)
            {
                for (int col = 1; col < CorMatrix[0].Count; col++)
                {
                    if (((sbyte)CorMatrix[row][col]) != 0)
                        LStates[col].Add(new BlockState((sbyte)CorMatrix[row][col], col, row, (int)Math.Log(CorMatrix.Count, 2)));
                    if (((sbyte)DifMatrix[row][col]) != 0)
                        DStates[row].Add(new BlockState((sbyte)DifMatrix[row][col], row, col, (int)Math.Log(DifMatrix.Count, 2)));
                }
            }
            for(int i=1;i<CorMatrix.Count;i++)
            {
                LStates[i] = LStates[i].OrderByDescending(o => Math.Abs(o.MatrixValue)).ToList();
                DStates[i] = DStates[i].OrderByDescending(o => Math.Abs(o.MatrixValue)).ToList();
            }
        }
        public DBNote(DBNote N)
        {
            this.DStates = N.DStates;
            this.LStates = N.LStates;
//             this.FuncMatrix = new List<List<bool>>(N.FuncMatrix.Count);
//             this.CorMatrix = new List<List<short>>(N.CorMatrix.Count);
//             this.DifMatrix = new List<List<short>>(N.DifMatrix.Count);
//             //             for (int i = 0; i < N.FuncMatrix.Count; i++)
//             //             {
//             //                 this.FuncMatrix.Add(new List<bool>(N.FuncMatrix[i]));
//             //             }
//             //             for (int i = 0; i < N.CorMatrix.Count; i++)
//             //             {
//             //                 this.CorMatrix.Add(new List<short>(N.CorMatrix[i]));
//             // 
//             //             }
//             //             for (int i = 0; i < N.DifMatrix.Count; i++)
//             //             {
//             //                 this.DifMatrix.Add(new List<short>(N.DifMatrix[i]));
//             //             }
//             this.FuncMatrix = N.FuncMatrix;
//             this.CorMatrix = N.CorMatrix;
//             this.DifMatrix = N.DifMatrix;
        }
//         public List<List<bool>> FuncMatrix;
//         public List<List<short>> CorMatrix;
//         public List<List<short>> DifMatrix;
        public List<List<BlockState>> LStates;
        public List<List<BlockState>> DStates;
    }
    [Serializable()]
    public struct BlockState
    {
        public BlockState(long inputs, int BlockSize)
        {
            MatrixValue = 0;
            this.BlockSize = BlockSize;
            this.CustomInput = WayConverter.ToList(inputs, this.BlockSize);
            this.CustomOutput = new List<bool>(Enumerable.Repeat(false, this.BlockSize));
            this.inputs = 0;
            this.outputs = 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Param">Value from Cor or Dif Matrix</param>
        /// <param name="inputs"></param>
        /// <param name="outputs"></param>
        /// <param name="BlockSize"></param>
        public BlockState(sbyte MatrixValue, long inputs, long outputs, int BlockSize)
        {
            this.BlockSize = BlockSize;
            this.MatrixValue = MatrixValue;
            this.inputs = (byte)inputs;
            this.outputs = (byte)outputs;
            CustomInput = null;
            CustomOutput = null;
            //             _inputs = WayConverter.ToList(inputs, _length);
            //             _outputs = WayConverter.ToList(outputs, _length);
        }
        //         public BlockState(List<bool> Inputs)
        //         {
        //             MatrixValue = 0;
        //             if (Inputs != null)
        //                 BlockSize = Inputs.Count;
        //             else
        //                 BlockSize = 0;
        // //             _inputs = Inputs;
        // //             _outputs = new List<bool>(BlockSize);
        // //             _outputs.AddRange(Enumerable.Repeat<bool>(false, BlockSize));
        //             inputs = WayConverter.ToLong(Inputs);
        //             outputs = 0;
        //         }
        [XmlIgnore()]
        [NonSerialized]
        public List<bool> CustomInput;
        [XmlIgnore()]
        [NonSerialized]
        public List<bool> CustomOutput;
        [XmlElement(ElementName = "length")]
        public int BlockSize;
        [XmlElement(ElementName = "MatrixValue")]
        public sbyte MatrixValue; // value from Matrix
        [XmlElement(ElementName = "Inputs")]
        public byte inputs;
        [XmlElement(ElementName = "Outputs")]
        public byte outputs;
//         [XmlIgnore()]
//         [NonSerialized]
//         public List<bool> _inputs;
//         [XmlIgnore()]
//         [NonSerialized]
//         public List<bool> _outputs;
    }
    public struct Prevalence
    {
        /// <summary>
        /// This is not prevalence, this is probability already.
        /// To get prevalence subtract 0.5.
        /// To get delta multiply doubled prevalence(!) by 2.
        /// </summary>
        /// <param name="P">If P.BlockSize==0||P.ActiveBlocksCount==0||P.Numerator==0 returns 0.5</param>
        public static explicit operator double(Prevalence P)
        {
            if (P.BlockSize == 0 || P.ActiveBlocksCount == 0 || P.Numerator == 0)
            {
                return 0.5;
            }
            var Mul = new BigInteger(1 << P.BlockSize);
            var D = BigInteger.Pow(Mul, P.ActiveBlocksCount);
            var GCD = BigInteger.GreatestCommonDivisor(P.Numerator, D);
            var N = BigInteger.Divide(P.Numerator, GCD);
            D = BigInteger.Divide(D, GCD);
            return 0.5 * ((long)N) / (double)(D);
        }
        /// <summary>
        /// If P.BlockSize==0||P.ActiveBlocksCount==0||P.Numerator==0 returns 0.5.
        /// </summary>
        /// <returns></returns>
        public double ToProbability()
        {
            return 0.5 + 0.5 * ToDelta();
        }
        /// <summary>
        /// If P.BlockSize==0||P.ActiveBlocksCount==0||P.Numerator==0 returns 0.       
        /// Carefully, signed value.
        /// </summary>
        /// <returns></returns>
        public double ToPrevalence()
        {
            return 0.5 * ToDelta();
        }
        /// <summary>
        /// If P.BlockSize==0||P.ActiveBlocksCount==0||P.Numerator==0 returns 0.
        /// Carefully, signed value.
        /// </summary>
        /// <returns></returns>
        public double ToDelta()
        {
            if (BlockSize == 0 || ActiveBlocksCount == 0 || Delta == 0/*Numerator == 0*/)
            {
                return 0;
            }
            return Delta;
            var Mul = 1 << BlockSize;
            var D = BigInteger.Pow(Mul, ActiveBlocksCount);
            //             var GCD = BigInteger.GreatestCommonDivisor(Numerator, D);
            //             var N = BigInteger.Divide(Numerator, GCD);
            //             D = BigInteger.Divide(D, GCD);
            //             var ret = ((double)N) / (double)(D);
            var ret = ((double)Numerator) / (double)(D);
            //Debug.Assert(ret != 0);
            return ret;
        }
        public override string ToString()
        {
            return String.Format("Prevalence: {0}.", ToPrevalence());
        }
        public static Prevalence operator *(long L, Prevalence R) { return R * L; }
        /// <summary>
        /// Adding new Matrix value from new block
        /// </summary>
        /// <param name="L">Current Prevalence</param>
        /// <param name="R">Value from Matrix for new block</param>
        /// <returns></returns>
        public static Prevalence operator *(Prevalence L, long R)
        {
            var ret = L;
            if (L.Delta == 0)
            {
                ret.Delta = (double)R / (double)(1 << L.BlockSize);
            }
            else
            {
                ret.Delta *= ((double)R / (double)(1 << L.BlockSize));
            }
            //             if (L.Numerator==0)
            //             {
            //                 ret.Numerator = R;
            //             }
            //             else
            //             {
            //                 ret.Numerator *= R;
            //             }
            ret.ActiveBlocksCount++;
            return ret;
        }
        public Prevalence(BigInteger Numerator, int ActiveBlocksCount, int BlockSize)
        {
            this.Numerator = Numerator;
            this.ActiveBlocksCount = ActiveBlocksCount;
            this.BlockSize = BlockSize;
            this.Delta = 0;
        }
        public BigInteger Numerator;
        public double Delta;
        public int ActiveBlocksCount;
        public int BlockSize;
        public static bool operator >=(Prevalence L, Prevalence R)
        {
            return Math.Abs(L.ToDelta()) >= Math.Abs(R.ToDelta());
            Debug.Assert(L.BlockSize == R.BlockSize);
            if(L.ActiveBlocksCount==R.ActiveBlocksCount)
            {
                return BigInteger.Abs(L.Numerator) >= BigInteger.Abs(R.Numerator);
            }
            if(R.ActiveBlocksCount == 0)
            {
                return true;
            }
            if(L.ActiveBlocksCount==0)
            {
                return false;
            }
            else
            {
                var tempL = L.Numerator;
                var tempR = R.Numerator;
                var Mul = new BigInteger(1 << L.BlockSize);
                var Diff = L.ActiveBlocksCount - R.ActiveBlocksCount;
                for (int i=0;i<Math.Abs(Diff);i++)
                {
                    if(Diff>0)
                    {
                        tempR *= Mul;
                    }
                    else
                    {
                        tempL *= Mul;
                    }
                }
                var ret = BigInteger.Abs(tempL) >= BigInteger.Abs(tempR);
                //var chk = Math.Abs(L.ToDelta()) >= Math.Abs(R.ToDelta());
                //Debug.Assert(ret == chk);
                return ret;
            }
        }
        public static bool operator <=(Prevalence L, Prevalence R) { return R >= L;  }
        public static bool operator >(Prevalence L, Prevalence R)
        {
            return Math.Abs(L.ToDelta()) > Math.Abs(R.ToDelta());
            Debug.Assert(L.BlockSize == R.BlockSize);
            if (L.ActiveBlocksCount == R.ActiveBlocksCount)
            {
                return BigInteger.Abs(L.Numerator) > BigInteger.Abs(R.Numerator);
            }
            if (R.ActiveBlocksCount == 0)
            {
                return true;
            }
            if (L.ActiveBlocksCount == 0)
            {
                return false;
            }
            else
            {
                var tempL = L.Numerator;
                var tempR = R.Numerator;
                var Mul = new BigInteger(1 << L.BlockSize);
                var Diff = L.ActiveBlocksCount - R.ActiveBlocksCount;
                for (int i = 0; i < Math.Abs(Diff); i++)
                {
                    if (Diff > 0)
                    {
                        tempR *= Mul;
                    }
                    else
                    {
                        tempL *= Mul;
                    }
                }
                return BigInteger.Abs(tempL) > BigInteger.Abs(tempR);
            }
        }
        public static bool operator <(Prevalence L, Prevalence R) { return R > L; }
    }
	public enum AnalisysType
	{
		Linear,
		Differencial
	}
	public enum LayerType
	{
		KLayer,
		SLayer,
		PLayer
	}
	public struct SPNetWayBlock
	{
        public long Inputs;
        public long Outputs;
        public int BlockSize;
	}
	public struct SPNetWayLayer
	{
		public List<SPNetWayBlock> blocks;
		public LayerType type;
	}
	public struct SPNetWay
	{
		public List<SPNetWayLayer> layers;
	}
    public sealed class AvailableSolverTypes
    {

        private readonly String name;
        private readonly int value;

        public static readonly AvailableSolverTypes BruteforceSolver = new AvailableSolverTypes(1, "Bruteforce Solver");
        public static readonly AvailableSolverTypes GreedySolver = new AvailableSolverTypes(2, "Greedy Solver");
        public static readonly AvailableSolverTypes AdvancedSolver = new AvailableSolverTypes(3, "Advanced Solver");
        public static List<AvailableSolverTypes> GetAllTypes()
        {
            return new List<AvailableSolverTypes> { BruteforceSolver, GreedySolver, AdvancedSolver };
        }
        private AvailableSolverTypes(int value, String name)
        {
            this.name = name;
            this.value = value;
        }

        public override String ToString()
        {
            return name;
        }

    }
    public struct AnalisysParams
    {
        public bool ASync;
        public int MaxThreads;
        public Algorithm Alg;
        public AnalisysParams(Algorithm Alg, int MaxThreads = -1)
        {
            this.ASync = true;
            this.Alg = Alg;
            if (MaxThreads == -1)
            {
                this.MaxThreads = System.Environment.ProcessorCount;
            }
            else
            {
                if (MaxThreads > 32)
                {
                    throw new Exception("Seems incorrect processor count value. Maximum is 32 (hardcoded value).");
                }
                this.MaxThreads = MaxThreads;
            }
        }
    }
    public struct SolverInputs
    {
        public SolverInputs(long Input, int length)
        {
            this.Length = length;
            this.Input = Input;
            weight = 0;
        }
        public long Input;
        public int Length;
        public int weight;
    }
    /// <summary>
    /// If UseCustomInput = false then the chosen Solver participates in bruteforce, so Inputs isn't needed.
    /// </summary>
    public struct Rule
    {
        public Rule(AvailableSolverTypes SolverType, int MaxActiveBlocksOnLayer = 2, int MaxStartBlocks=1, bool UseCustomInput=false, SolverInputs Input = new SolverInputs())
        {
            this.MaxActiveBlocksOnLayer = MaxActiveBlocksOnLayer;
            this.SolverType = SolverType;
            this.Input = Input;
            this.UseCustomInput = UseCustomInput;
            this.MaxStartBlocks = MaxStartBlocks;
        }
        public int MaxStartBlocks;
        public int MaxActiveBlocksOnLayer;
        public AvailableSolverTypes SolverType;
        public bool UseCustomInput;
        public SolverInputs Input;
        public override string ToString()
        {
            string ret = "";
            ret += "Solver Type:\t"+SolverType + "\n";
            ret += "Maximum of active S-boxes in one layer:\t" + MaxActiveBlocksOnLayer + "\n";
            ret += "Is custom input used:\t" + ((UseCustomInput) ? "True" : "False") + "\n";
            if (UseCustomInput)
            {
                ret += "Custom input:\t";
                var listedInput = WayConverter.ToList(Input.Input, Input.Length);
                for (int i = 0; i < listedInput.Count; i++)
                {
                    ret += (listedInput[i]) ? "1" : "0";
                }
                ret += "\n";
            }
            return ret;
        }
    }
    public struct Algorithm
    {
        public Algorithm(List<Rule> Rules, AnalisysType Type)
        {
            this.Rules = Rules;
            this.Type = Type;
        }
        public List<Rule> Rules;
        public AnalisysType Type;
    }
    public struct EngineSettings
    {
        public EngineSettings(ADDSOLUTIONHANDLER AddSolutionFunc/*, TaskFinishedHandler TaskFinishedFunc*/)
        {
            this.AddSolutionFunc = AddSolutionFunc;
        }
        public ADDSOLUTIONHANDLER AddSolutionFunc;
    }
    public struct SPNetSettings
    {
//         public static bool operator ==(SPNetSettings L, SPNetSettings R)
//         {
//             if((L.SBoxCount==R.SBoxCount)||(L.WordLength==R.WordLength)||(R.db==L.db))
//             {
//                 return true;
//             }
//             else
//             {
//                 return false;
//             }
//         }
// 
//         public static bool operator !=(SPNetSettings L, SPNetSettings R)
//         {
//             return !(L == R);
//         }
        public SPNetSettings(byte WordLength, byte SBoxSize/*, ISBlockDB DB = null*/)
        {
            this.WordLength = WordLength;
            this.SBoxSize = SBoxSize;
            this.SBoxCount = (byte)(this.WordLength / this.SBoxSize);
            //this.db = DB;
        }
		public byte WordLength;
        //public byte round_count;
        public byte SBoxSize;
		public byte SBoxCount;
		//public ISBlockDB db;
    }
    public struct BlockStateExtrParams
    {
        public AnalisysType Type;
        public BlockStateExtrParams(long Inputs/*, long Outputs*/, Prevalence MIN, Prevalence CurrentPrevalence, AnalisysType Type, bool CheckPrevalence = true)
        {
            this.Inputs = Inputs;
            //this.Outputs = Outputs;
            this.CheckPrevalence = CheckPrevalence;
            this.CurrentPrevalence = CurrentPrevalence;
            this.MIN = MIN;
            this.Type = Type;
        }
        public Prevalence MIN;
        public long Inputs;
        //public long Outputs;
        public bool CheckPrevalence;
        public Prevalence CurrentPrevalence;
    }
    public struct Solution : IComparable<Solution>
	{
        public Solution(Prevalence p, SPNetWay w)
        {
            P = p;
            Way = w;
        }
        public Prevalence P;
		public SPNetWay Way;
        public override string ToString()
        {
            return P.ToString()+string.Format(" Active Boxes Count: {0}.", P.ActiveBlocksCount);
        }

        public int CompareTo(Solution other)
        {
            if (Math.Abs(Math.Abs(this.P.Delta) - Math.Abs(other.P.Delta)) < 1E-200)
            {
                return this.P.ActiveBlocksCount.CompareTo(other.P.ActiveBlocksCount);
            }
            return - Math.Abs(this.P.Delta).CompareTo(Math.Abs(other.P.Delta));
        }
    }
}