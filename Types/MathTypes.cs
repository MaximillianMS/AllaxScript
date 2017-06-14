using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Allax
{
	public delegate bool CallbackAddSolution(Solution s);
    public delegate void TaskFinishedHandler(Task T);
    [Serializable()]
    public struct DBNote
    {
        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="FuncMatrix"></param>
        /// <param name="CorMatrix"></param>
        /// <param name="DifMatrix"></param>
        public DBNote(List<List<bool>> FuncMatrix, List<List<short>> CorMatrix, List<List<short>> DifMatrix)
        {
            /*this.FuncMatrix = new List<List<bool>>(FuncMatrix.Count);
            this.CorMatrix = new List<List<short>>(CorMatrix.Count);
            this.DifMatrix = new List<List<short>>(DifMatrix.Count);
                        for (int i = 0; i < FuncMatrix.Count; i++)
            {
                this.FuncMatrix.Add(new List<bool>(FuncMatrix[i]));
            }
            for (int i = 0; i < CorMatrix.Count; i++)
            {
                this.CorMatrix.Add(new List<short>(CorMatrix[i]));

            }
            for (int i = 0; i < DifMatrix.Count; i++)
            {
                this.DifMatrix.Add(new List<short>(DifMatrix[i]));
            }*/
            this.FuncMatrix = FuncMatrix;
            this.DifMatrix = DifMatrix;
            this.CorMatrix = CorMatrix;
            this.DStates = new List<BlockState>(this.DifMatrix.Count * this.DifMatrix.Count);
            this.LStates = new List<BlockState>(this.CorMatrix.Count * this.CorMatrix.Count);
            for (int row = 1; row < CorMatrix.Count; row++)
            {
                for (int col = 1; col < CorMatrix[0].Count; col++)
                {
                    LStates.Add(new BlockState(CorMatrix[row][col], col, row, (int)Math.Log(CorMatrix.Count, 2)));
                    DStates.Add(new BlockState(DifMatrix[row][col], row, col, (int)Math.Log(DifMatrix.Count, 2)));
                }
            }
            LStates = LStates.OrderByDescending(o => Math.Abs(o.MatrixValue)).ToList();
            DStates = DStates.OrderByDescending(o => Math.Abs(o.MatrixValue)).ToList();
        }
        public DBNote(DBNote N)
        {
            this.DStates = N.DStates;
            this.LStates = N.LStates;
            this.FuncMatrix = new List<List<bool>>(N.FuncMatrix.Count);
            this.CorMatrix = new List<List<short>>(N.CorMatrix.Count);
            this.DifMatrix = new List<List<short>>(N.DifMatrix.Count);
            //             for (int i = 0; i < N.FuncMatrix.Count; i++)
            //             {
            //                 this.FuncMatrix.Add(new List<bool>(N.FuncMatrix[i]));
            //             }
            //             for (int i = 0; i < N.CorMatrix.Count; i++)
            //             {
            //                 this.CorMatrix.Add(new List<short>(N.CorMatrix[i]));
            // 
            //             }
            //             for (int i = 0; i < N.DifMatrix.Count; i++)
            //             {
            //                 this.DifMatrix.Add(new List<short>(N.DifMatrix[i]));
            //             }
            this.FuncMatrix = N.FuncMatrix;
            this.CorMatrix = N.CorMatrix;
            this.DifMatrix = N.DifMatrix;
        }
        public List<List<bool>> FuncMatrix;
        public List<List<short>> CorMatrix;
        public List<List<short>> DifMatrix;
        public List<BlockState> LStates;
        public List<BlockState> DStates;
    }
    [Serializable()]
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
        [XmlElement(ElementName = "length")]
        public int _length;
        [XmlElement(ElementName = "MatrixValue")]
        public Int64 MatrixValue; // Abs(value) from Matrix
        [XmlElement(ElementName = "ListOfInputs")]
        public List<bool> _inputs;
        [XmlElement(ElementName = "ListOfOutputs")]
        public List<bool> _outputs;
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
            if (BlockSize == 0 || ActiveBlocksCount == 0 || Numerator == 0)
            {
                return 0;
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
                if(L.ActiveBlocksCount != 0)
                {
                    ;
                }
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
                        tempL *= Mul;
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
    public sealed class AvailableSolverTypes
    {

        private readonly String name;
        private readonly int value;

        public static readonly AvailableSolverTypes BaseSolver = new AvailableSolverTypes(1, "BaseSolver");
        public static readonly AvailableSolverTypes HeuristicSolver = new AvailableSolverTypes(2, "HeuristicSolver");

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
        public CallbackAddSolution AddSolution;
        public TaskFinishedHandler TaskFinishedFunc;
        public AnalisysParams(Algorithm Alg, CallbackAddSolution AddSolution, TaskFinishedHandler TaskFinishedFunc, int MaxThreads = -1)
        {
            this.TaskFinishedFunc = TaskFinishedFunc;
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
        public Rule(AvailableSolverTypes SolverType, int MaxActiveBlocksOnLayer = 2, bool UseCustomInput=false, SolverInputs Input = new SolverInputs())
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
        public override string ToString()
        {
            string ret = "";
            ret += "Solver Type:\t"+SolverType + "\n";
            ret += "Maximum of active S-boxes in one layer:\t" + MaxActiveBlocksOnLayer + "\n";
            ret += "Is custom input used:\t" + ((UseCustomInput) ? "True" : "False") + "\n";
            if (UseCustomInput)
            {
                ret += "Custom input:\t";
                for (int i = 0; i < Input.input.Count; i++)
                {
                    ret += (Input.input[i]) ? "1" : "0";
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
    public struct SPNetSettings
    {
        public static bool operator ==(SPNetSettings L, SPNetSettings R)
        {
            if((L.SBoxCount==R.SBoxCount)||(L.WordLength==R.WordLength)||(R.db==L.db))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool operator !=(SPNetSettings L, SPNetSettings R)
        {
            return !(L == R);
        }
        public SPNetSettings(byte WordLength, byte SBoxSize, ISBlockDB DB = null)
        {
            this.WordLength = WordLength;
            this.SBoxSize = SBoxSize;
            this.SBoxCount = (byte)(this.WordLength / this.SBoxSize);
            this.db = DB;
        }
		public byte WordLength;
        //public byte round_count;
        public byte SBoxSize;
		public byte SBoxCount;
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