using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Diagnostics;
namespace Allax
{
	public delegate bool CallbackAddSolution(Solution s);
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
            if (BlockSize == 0 || ActiveBlocksCount == 0 || Numerator == 0)
            {
                return 0.5;
            }
            var Mul = new BigInteger(1 << BlockSize);
            var D = BigInteger.Pow(Mul, ActiveBlocksCount);
            var GCD = BigInteger.GreatestCommonDivisor(Numerator, D);
            var N = BigInteger.Divide(Numerator, GCD);
            D = BigInteger.Divide(D, GCD);
            return ((long)N) / (double)(D);
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
            if (L.Numerator==0)
            {
                ret.Numerator = R;
                Debug.Assert(L.ActiveBlocksCount == 0);
            }
            else
            {
                ret.Numerator *= R;
            }
            ret.ActiveBlocksCount++;
            return ret;
        }
        public Prevalence(BigInteger Numerator, int ActiveBlocksCount, int BlockSize)
        {
            this.Numerator = Numerator;
            this.ActiveBlocksCount = ActiveBlocksCount;
            this.BlockSize = BlockSize;
        }
        public BigInteger Numerator;
        public int ActiveBlocksCount;
        public int BlockSize;
        public static bool operator >=(Prevalence L, Prevalence R)
        {
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
                for (int i=0;i<Diff;i++)
                {
                    if(Diff>0)
                    {
                        tempR *= Mul;
                    }
                    else
                    {
                        tempR *= Mul;
                    }
                }
                return BigInteger.Abs(tempL) >= BigInteger.Abs(tempR);
            }
        }
        public static bool operator <=(Prevalence L, Prevalence R) { return R >= L;  }
        public static bool operator >(Prevalence L, Prevalence R)
        {
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
                for (int i = 0; i < Diff; i++)
                {
                    if (Diff > 0)
                    {
                        tempR *= Mul;
                    }
                    else
                    {
                        tempR *= Mul;
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
		public List<bool> active_inputs;
		public List<bool> active_outputs;
	}
	public struct SPNetWayLayer
	{
		//public Int64 active_inputs;
		//public Int64 active_outputs;
		public List<SPNetWayBlock> blocks;
		public LayerType type;
	}
	public struct SPNetWay
	{
		public List<SPNetWayLayer> layers; // Input-Output on every layer 
	}
    public enum AvailableSolverTypes { BaseSolver, HeuristicSolver }
    public struct AnalisysParams
    {
        public bool ASync;
        public int MaxThreads;
        public Algorithm Alg;
        public CallbackAddSolution AddSolution;
        public AnalisysParams(Algorithm Alg, CallbackAddSolution AddSolution, int MaxThreads = -1)
        {
            this.ASync = true;
            this.Alg = Alg;
            this.AddSolution = AddSolution;
            if (MaxThreads == -1)
            {
                this.MaxThreads = System.Environment.ProcessorCount;
                if (MaxThreads > 32)
                {
                    throw new Exception("Seems incorrect processor count value. Maximum is 32 (hardcoded value).");
                }
            }
            else
            {
                this.MaxThreads = MaxThreads;
            }
        }
    }
    public struct SolverInputs
    {
        public SolverInputs(List<bool> Input)
        {
            input = Input;
            weight = 0;
        }
        public SolverInputs(long Input, int length)
        {
            input = WayConverter.ToList(Input, length);
            weight = 0;
        }
        public List<bool> input;
        public int weight;
    }
    /// <summary>
    /// If UseCustomInput = false then the chosen Solver participates in bruteforce, so Inputs isn't needed.
    /// </summary>
    public struct Rule
    {
        public Rule(AvailableSolverTypes SolverType, SolverInputs Input=new SolverInputs(), int MaxActiveBlocksOnLayer = 2, bool UseCustomInput=false)
        {
            this.MaxActiveBlocksOnLayer = MaxActiveBlocksOnLayer;
            this.SolverType = SolverType;
            this.Input = Input;
            this.UseCustomInput = UseCustomInput;
        }
        public int MaxActiveBlocksOnLayer;
        public AvailableSolverTypes SolverType;
        public bool UseCustomInput;
        public SolverInputs Input;
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
    public struct SPNetSettings
	{
        public SPNetSettings(byte WordLength, byte SBlockCount, ISBlockDB DB)
        {
            this.word_length = WordLength;
            this.sblock_count = SBlockCount;
            this.db = DB;
        }
		public byte word_length;
		//public byte round_count;
		public byte sblock_count;
		public ISBlockDB db;
    }
    public struct BlockStateExtrParams
    {
        public AnalisysType Type;
        public BlockStateExtrParams(List<bool> Inputs, List<bool> Outputs, Prevalence MIN, Prevalence CurrentPrevalence, AnalisysType Type, bool CheckPrevalence = true)
        {
            if (Inputs == null)
            {
                Inputs = new List<bool>();
            }
            this.Inputs = Inputs;
            if (Outputs == null)
            {
                Outputs = new List<bool>();
            }
            this.Outputs = Outputs;
            this.CheckPrevalence = CheckPrevalence;
            this.CurrentPrevalence = CurrentPrevalence;
            this.MIN = MIN;
            this.Type = Type;
        }
        public Prevalence MIN;
        public List<bool> Inputs;
        public List<bool> Outputs;
        public bool CheckPrevalence;
        public Prevalence CurrentPrevalence;
    }
    public struct BlockState
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Param">Value from Cor or Dif Matrix</param>
        /// <param name="inputs"></param>
        /// <param name="outputs"></param>
        /// <param name="length"></param>
        public BlockState(Int64 MatrixValue, int inputs, int outputs, int length)
        {
            _length = length;
            this.MatrixValue = MatrixValue;
            _inputs = WayConverter.ToList(inputs, _length);
            _outputs = WayConverter.ToList(outputs, _length);
        }
        public BlockState(List<bool> Inputs)
        {
            MatrixValue = 0;
            if (Inputs != null)
                _length = Inputs.Count;
            else
                _length = 0;
            _inputs = Inputs;
            _outputs = new List<bool>(_length);
            _outputs.AddRange(Enumerable.Repeat<bool>(false, _length));
        }
        public int _length;
        public Int64 MatrixValue; // Abs(value) from Matrix
        public List<bool> _inputs;
        public List<bool> _outputs;
    }
    public struct Solution
	{
        public Solution(Prevalence p, SPNetWay w)
        {
            P = p;
            Way = w;
        }
        public Prevalence P;
		public SPNetWay Way;
	}
}