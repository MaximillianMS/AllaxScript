using System;
using System.Collections.Generic;
using System.Linq;

namespace Allax
{
	public delegate bool CallbackAddSolution(Solution s);
    public struct Prevalence
    {
        public Prevalence(long Mul, int ActiveBlocksCount)
        {
            this.Mul = Mul;
            this.ActiveBlocksCount = ActiveBlocksCount;
        }
        public long Mul;
        public int ActiveBlocksCount;
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
        public Algorithm Alg;
        public CallbackAddSolution AddSolution;
        AnalisysParams(Algorithm Alg, CallbackAddSolution AddSolution)
        {
            ASync = true;
            this.Alg = Alg;
            this.AddSolution = AddSolution;
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
    public struct Rule
    {
        public Rule(AvailableSolverTypes SolverType, SolverInputs Input=new SolverInputs(), bool UseCustomInput=false)
        {
            this.SolverType = SolverType;
            this.Input = Input;
            this.UseCustomInput = UseCustomInput;
        }
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
		public byte word_length;
		public byte round_count;
		public byte sblock_count;
		public ISBlockDB db;
    }
    public struct BlockStateExtrParams
    {
        public BlockStateExtrParams(List<bool> Inputs, List<bool> Outputs, Prevalence MIN, Prevalence CurrentPrevalence, bool CheckCorrelation=true)
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
            this.CheckCorrelation = CheckCorrelation;
            this.CurrentPrevalence = CurrentPrevalence;
            this.MIN = MIN;
        }
        public Prevalence MIN;
        public List<bool> Inputs;
        public List<bool> Outputs;
        public bool CheckCorrelation;
        public Prevalence CurrentPrevalence;
    }
    public struct BlockState
    {
        public BlockState(Int64 cor, int inputs, int outputs, int length)
        {
            _length = length;
            _cor = cor;
            _inputs = WayConverter.ToList(inputs, _length);
            _outputs = WayConverter.ToList(outputs, _length);
        }
        public BlockState(List<bool> Inputs)
        {
            _cor = 0;
            if (Inputs != null)
                _length = Inputs.Count;
            else
                _length = 0;
            _inputs = Inputs;
            _outputs = new List<bool>(_length);
            _outputs.AddRange(Enumerable.Repeat<bool>(false, _length));
        }
        public int _length;
        public Int64 _cor; // Abs(value) from Matrix
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