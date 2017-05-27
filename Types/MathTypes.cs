using System;
using System.Collections.Generic;

namespace Allax
{
	public delegate bool CallbackAddSolution(Solution s);
	public delegate void Log(String s, Int32 error_code);
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
	public struct SPNetSettings
	{
		public byte word_length;
		public byte round_count;
		public byte sblock_count;
		public ISBlockDB db;
		CallbackAddSolution AddSolution;
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
    public struct Solution
	{
		public double prevalence;
		public byte active_blocks_count;
		public SPNetWay way;
	}
}