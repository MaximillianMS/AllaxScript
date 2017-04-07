using System;
using System.Collections.Generic;
using System.Linq;
namespace Allax
{
    class SBlockDB : ISBlockDB
    {
        //Fourier Transform DB
        Dictionary<List<short>, List<short>> FuncDB;
        public Dictionary<List<short>, List<short>> Export()
        {
            return FuncDB;
        }
        public SBlockDB(Dictionary<List<short>, List<short>> FourierTransformDB)
        {
            if (FourierTransformDB != null)
            {
                FuncDB = FourierTransformDB;
            }
            else
            {
                FuncDB = new Dictionary<List<short>, List<short>>();
            }
        }
        void AddToFuncDB(List<short> func, List<short> f_spectrum)
        {
            FuncDB.Add(func, f_spectrum);
        }
        List<short> FourierTransform(List<short> func)
        {
            if (func.Count > 1)
            {
                List<short> left = new List<short>();
                List<short> right = new List<short>();
                left.AddRange(Enumerable.Repeat<short>(0, func.Count / 2));
                right.AddRange(Enumerable.Repeat<short>(0, func.Count / 2));
                for (int i = 0; i < func.Count / 2; i++)
                {
                    left[i] = (short)(func[i] + func[i + func.Count / 2]);
                    right[i] = (short)(func[i] - func[i + func.Count / 2]);
                }
                left = FourierTransform(left);
                right = FourierTransform(right);
                for (int i = 0; i < func.Count / 2; i++)
                {
                    func[i] = left[i];
                    func[i + func.Count / 2] = right[i];
                }
            }
            return func;
        }
        List<short> GetLinCombo(List<List<byte>> func_table, int LinCombo)
        {
            if (func_table.Count > 0)
            {
                List<short> combo = new List<short>();
                combo.AddRange(Enumerable.Repeat<short>(0, func_table.Count));
                for (int i = 0; i < func_table.Count; i++)
                {
                    for (int j = 0; j < func_table[0].Count; j++)
                    {
                        if ((LinCombo & (1 << (func_table.Count - 1 - j))) != 0)
                            combo[i] += func_table[i][j];
                    }
                    combo[i] = (short)(combo[i] & 1);
                }
                return combo;
            }
            return null;
        }
        public List<List<short>> GetCorMatrix(List<List<byte>> func_table)
        {
            int LinCombo = 0;
            List<List<short>> CorMatrix = new List<List<short>>();
            CorMatrix.AddRange(Enumerable.Repeat<List<short>>(new List<short>(), func_table.Count));
            for (LinCombo = 0; LinCombo < func_table.Count; LinCombo++)
            {
                CorMatrix[LinCombo] = GetLinCombo(func_table, LinCombo);
                List<short> f_spectrum = new List<short>();
                if (!FuncDB.TryGetValue(CorMatrix[LinCombo], out f_spectrum))
                {
                    f_spectrum = FourierTransform(CorMatrix[LinCombo]);
                    AddToFuncDB(CorMatrix[LinCombo], f_spectrum);
                }
                CorMatrix[LinCombo] = f_spectrum;
            }
            return CorMatrix;
        }
    }
    abstract class Block : IBlock
    {
        public abstract void Init(List<byte> arg); //SBlock size:4-8. Whole word: up to 64bit.
    };
    class PBlock : Block
    {
        int VarCount;
        int _length;
        List<int> outputNumber;
        public int GetOutputNumber(int InputNumber) // 
        {
            return outputNumber[InputNumber];
        }
        public PBlock(byte word_length)
        {
            _length = word_length;
            outputNumber.Clear();
        }
        public override void Init(List<byte> arg) // from interfaces
        {
            const int bias = 0; //P-Block record type: 'some args, Output1, Output1, ..., OutputN'. No some args => Bias=0. Output value starts with 1.
            if (arg.Count >= bias)
            {
                if (arg.Count - bias == _length)
                {
                    outputNumber.AddRange(Enumerable.Repeat<int>(0, _length));
                    for (int i = bias; i < arg.Count; i++)
                    {
                        outputNumber[i - bias] = arg[i] - 1; // Convert from global numeration
                    }
                }
            }
        }
    }
    class KBlock : Block
    {
        public override void Init(List<byte> arg)
        {
            throw new NotImplementedException("KBlock do not needed to init");
        }
    }
    class SBlock : Block
    {
        ISBlockDB _database;
        List<List<short>> CorTable;
        List<List<byte>> FuncTable;
        public List<SBlockState> _states;
        int VarCount;
        byte _length;
        string BlockID = "";
        public List<SBlockState> ExtractStates(Int64 MIN, Int64 CurrentCor, int inputs)// MIN prevalence, current inputs
        {
            var States = new List<SBlockState>();
            foreach (SBlockState state in _states)
            {
                if (state._cor * CurrentCor > MIN)
                {
                    if (state._inputs == inputs)
                    {
                        States.Add(state);
                    }
                }
                else
                {
                    break;
                }
            }
            return States;
        }
        public void Sort()
        {
            _states = new List<SBlockState>(1 << (2 * VarCount));
            for (int row = 1; row < CorTable.Count; row++)
            {
                for (int col = 1; col < CorTable[0].Count; col++)
                {
                    _states.Add(new SBlockState(Math.Abs(CorTable[row][col]), col, row));
                }
            }
            _states = _states.OrderByDescending(o => Math.Abs(o._cor)).ToList();
        }
        short GetCorrelation(int inputs, int outputs)
        {
            return CorTable[outputs][inputs];
        }
        public override void Init(List<byte> arg) // from interfaces
        {
            const int bias = 0; //S-Block record type: 'some args, Line1, Line2, ..., LineN'. No some args => Bias=0.
            if (arg.Count >= bias)
            {
                if (arg.Count - bias != 0)
                    VarCount = (int)System.Math.Log(arg.Count - bias, 2); // Table: 2**n x n
                if (VarCount == _length)
                {
                    FuncTable.AddRange(Enumerable.Repeat<List<byte>>(new List<byte>(), arg.Count - bias));
                    //Line0: 		y0..yn
                    //Line1: 		y0..yn
                    //....................
                    //Line2**n-1: 	y0..yn
                    for (int i = bias; i < arg.Count; i++)
                    {
                        FuncTable[i - bias].AddRange(Enumerable.Repeat<byte>(0, VarCount));
                        for (int j = VarCount - 1; j >= 0; j--)
                        {
                            if ((arg[i] & (1 << j)) == 0)
                            {
                                CorTable[i - bias].Add(0);
                            }
                            else
                            {
                                CorTable[i - bias].Add(1);
                            }
                        }
                    }
                    if (_database != null)
                        CorTable = _database.GetCorMatrix(FuncTable);
                    Sort();
                }
                else
                {
                    throw new Exception("Argument length error");
                }
            }
        }

        void SetDB(SBlockDB database)
        {
            _database = database;
        }
        public SBlock()
        {
            _database = null;
            BlockID = "";
            CorTable.Clear();
            FuncTable.Clear();
            VarCount = 0;
        }
        public SBlock(ISBlockDB database, byte block_length) //!NOW USED
        {
            _database = database;
            BlockID = "";
            CorTable.Clear();
            FuncTable.Clear();
            VarCount = 0;
            _length = block_length;
        }
        public SBlock(List<List<byte>> table, SBlockDB database)
        {
            _database = database;
            BlockID = "";
            CorTable.Clear();
            FuncTable.Clear();
            FuncTable = table;
            VarCount = 0;
            CorTable = _database.GetCorMatrix(FuncTable);
        }
    }
    abstract class Layer : ILayer
    {
        public LayerType GetLayerType()
        {
            return type;
        }
        public abstract List<IBlock> GetBlocks();
        public abstract void DeleteBlock(byte number);
        public abstract IBlock GetBlock(byte number);
        protected LayerType type;
        protected List<Block> Blocks;
    }
    class KLayer : Layer
    {
        List<int> Key;
        public KLayer(List<int> key)
        {
            Key = key;
        }
        public KLayer()
        {
            type = LayerType.KLayer;
        }
        public override List<IBlock> GetBlocks()
        {
            throw new NotImplementedException();
        }
        public override void DeleteBlock(byte number)
        {
            throw new NotImplementedException();
        }
        public override IBlock GetBlock(byte number)
        {
            throw new NotImplementedException();
        }
    }
    class SLayer : Layer
    {
        public override List<IBlock> GetBlocks()
        {
            return Blocks.ConvertAll(x => ((IBlock)x));
        }
        public override IBlock GetBlock(byte number)// from interfaces
        {
            if (number - 1 < Blocks.Count)
                return Blocks[number - 1];
            else
                return null;
        }
        public override void DeleteBlock(byte number)
        {
            throw new NotImplementedException();
        }
        public SLayer(ISBlockDB db, byte block_length, byte blocks_count)
        {
            type = LayerType.SLayer;
            Blocks.AddRange(Enumerable.Repeat<SBlock>(new SBlock(db, block_length), blocks_count));
        }
        ushort ActiveBlocks; // Active Block bit mask. 16 block support in one layer. 
    }
    class PLayer : Layer
    {
        public PLayer(byte word_length)
        {
            type = LayerType.PLayer;
            Blocks.Add(new PBlock(word_length));
        }
        int GetOutputNumber(int InputNumber) // Global Numeration 1-16
        {
            return ((PBlock)(Blocks[0])).GetOutputNumber(InputNumber);
        }
        int[] GetOutputNumber(int[] inputNums) // Global Numeration 1-16
        {
            return (from num in inputNums select GetOutputNumber(num)).ToArray();
        }
        public override List<IBlock> GetBlocks()
        {
            return new List<IBlock> { Blocks[0] };
        }
        public override void DeleteBlock(byte number)
        {
            throw new NotImplementedException();
        }
        public override IBlock GetBlock(byte number)
        {
            throw new NotImplementedException();
        }
    }
    struct SBlockState
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
    class Worker
    {

    }
    class SPNet : ISPNet
    {
        List<Layer> Layers;
        SPNetSettings _settings;
        public SPNetSettings GetSettings()
        {
            return _settings;
        }
        static Int64 MIN; // CURRENT MIN FOR THREADS
        public void AddLayer(Layer layer)
        {
            Layers.Add(layer);
        }
        public virtual void SetSBlockDB(SBlockDB db)
        {
            _settings.db = db;
        }
        public virtual List<ILayer> GetLayers()
        {
            return Layers.ConvertAll(x => ((ILayer)x));
        }

        public virtual void PerformLinearAnalisys()
        {
            //!!
            //NOT IMPLEMENTED

        }
        public SPNet(SPNetSettings settings)
        {
            _settings = settings;
        }
        public virtual void DeleteLayer(byte number)
        {
            Layers.RemoveAt(number);
        }
        public virtual void AddLayer(LayerType type) // from interfaces
        {
            switch (type)
            {
                case LayerType.KLayer:
                    {
                        KLayer layer = new KLayer();
                        AddLayer(layer);
                        break;
                    }
                case LayerType.SLayer:
                    {
                        SLayer layer = new SLayer(_settings.db, (byte)(_settings.word_length / _settings.sblock_count), _settings.sblock_count);
                        AddLayer(layer);

                        break;
                    }
                case LayerType.PLayer:
                    {
                        PLayer layer = new PLayer(_settings.word_length);
                        AddLayer(layer);
                        break;
                    }
            }
        }
    }
    public class WayConverter
    {
        bool CheckStandartNetCondition(ISPNet Net)
        {
            return true;
        }
        public SPNetWay ToEmptyWay(ISPNet Net)
        {
            var NetWay = new SPNetWay();
            if(CheckStandartNetCondition(Net))
            {
                var Layers = Net.GetLayers();
                var Settings = Net.GetSettings();
                var LayerCount = Layers.Count;
                NetWay.layers = new List<SPNetWayLayer>();
                for(int i=0;i<LayerCount;i++)
                {
                    var tmp = new SPNetWayLayer();
                    tmp.type=Layers[i].GetLayerType();
                    if (tmp.type != LayerType.SLayer)
                    {
                        tmp.blocks = new List<SPNetWayBlock>();
                        var tmp_block = new SPNetWayBlock();
                        tmp_block.active_inputs = new List<bool>();
                        tmp_block.active_outputs = new List<bool>();
                        tmp_block.active_inputs.AddRange(Enumerable.Repeat<bool>(false, Settings.word_length));
                        tmp_block.active_outputs.AddRange(Enumerable.Repeat<bool>(false, Settings.word_length));
                        tmp.blocks.Add(tmp_block);
                    }
                    else
                    {
                        foreach (var j in Enumerable.Range(0, Settings.sblock_count))
                        {
                            var tmp_sblock = new SPNetWayBlock();
                            tmp_sblock.active_inputs = new List<bool>();
                            tmp_sblock.active_outputs = new List<bool>();
                            tmp_sblock.active_inputs.AddRange(Enumerable.Repeat<bool>(false, Settings.word_length / Settings.sblock_count));
                            tmp_sblock.active_outputs.AddRange(Enumerable.Repeat<bool>(false, Settings.word_length / Settings.sblock_count));
                            tmp.blocks.Add(tmp_sblock);
                        }
                    }
                    NetWay.layers.Add(tmp);

                }
                return NetWay;
            }
            else
            {
                throw new Exception("Azaza. Wrong net.");
            }
        }
    }
    public class TaskConstructor
    {

    }
    public class Engine : IEngine
    {
        public virtual ISPNet GetSPNetInstance(SPNetSettings settings) //from interfaces
        {
            return new SPNet(settings);
        }
        public virtual ISBlockDB GetSBlockDBInstance(Dictionary<List<short>, List<short>> db) //from interfaces
        {
            return new SBlockDB(db);
        }
    }
}