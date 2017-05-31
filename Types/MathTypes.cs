using System;
using System.Collections.Generic;
using System.Linq;

namespace Allax
{
	public delegate bool CallbackAddSolution(Solution s);
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
    public enum AvailableSolverTypes { BaseSolver }
    public struct LinearAnalisysParams
    {
        public bool ASync;
        public LinearAlg Alg;
        public CallbackAddSolution AddSolution;
        LinearAnalisysParams(LinearAlg Alg, CallbackAddSolution AddSolution)
        {
            ASync = true;
            this.Alg = Alg;
            this.AddSolution = AddSolution;
        }
    }
    public struct LinearAlg
    {

    }
    public struct DifferAlg
    {

    }
    public struct SPNetSettings
	{
		public byte word_length;
		public byte round_count;
		public byte sblock_count;
		public ISBlockDB db;
    }
    public struct SBlockState
    {
        public SBlockState(Int64 cor, int inputs, int outputs)
        {
            _cor = cor;
            _inputs = inputs;
            _outputs = outputs;
        }
        public Int64 _cor; // Abs(value) from Matrix
        public int _inputs;
        public int _outputs;
    }
    public struct BlockStateExtrParams
    {
        public BlockStateExtrParams(List<bool> Inputs, List<bool> Outputs, Int64 MIN, Int64 CurrentCorrelation, bool CheckCorrelation=true)
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
            this.CurrentCorrelation = CurrentCorrelation;
            this.MIN = MIN;
        }
        public Int64 MIN;
        public List<bool> Inputs;
        public List<bool> Outputs;
        public bool CheckCorrelation;
        public Int64 CurrentCorrelation;
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
		public double prevalence;
		public byte active_blocks_count;
		public SPNetWay way;
	}
}