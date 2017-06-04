using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
        public SBlockDB(Dictionary<List<short>, List<short>> FourierTransformDB=null)
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
                var left = new List<short>();
                var right = new List<short>();
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
        List<short> GetLinCombo(List<List<bool>> funcMatrix, int LinCombo)
        {
            if (funcMatrix.Count > 0)
            {
                var combo = new List<short>();
                combo.AddRange(Enumerable.Repeat<short>(0, funcMatrix.Count));
                for (int i = 0; i < funcMatrix.Count; i++)
                {
                    for (int j = 0; j < funcMatrix[0].Count; j++)
                    {
                        if ((LinCombo & (1 << (funcMatrix.Count - 1 - j))) != 0)
                            combo[i] += (short)((funcMatrix[i][j]) ? 1 : 0);
                    }
                    combo[i] = (short)(combo[i] & 1);
                }
                return combo;
            }
            return null;
        }
        public List<List<short>> GetCorMatrix(List<List<bool>> funcMatrix)
        {
            int LinCombo = 0;
            var CorMatrix = new List<List<short>>();
            CorMatrix.AddRange(Enumerable.Repeat<List<short>>(new List<short>(), funcMatrix.Count));
            for (LinCombo = 0; LinCombo < funcMatrix.Count; LinCombo++)
            {
                CorMatrix[LinCombo] = GetLinCombo(funcMatrix, LinCombo);
                var f_spectrum = new List<short>();
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
        public abstract List<BlockState> ExtractStates(BlockStateExtrParams Params);
    };
    class PBlock : Block
    {
        int _length;
        List<int> outputNumber=new List<int>();
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
                    outputNumber.AddRange(Enumerable.Repeat(0, _length));
                    for (int i = bias; i < arg.Count; i++)
                    {
                        outputNumber[i - bias] = arg[i] - 1; // Convert from global numeration
                    }
                }
            }
        }
        public override List<BlockState> ExtractStates(BlockStateExtrParams Params)
        {
            var State = new BlockState(Params.Inputs);
            var ret = new List<BlockState>(1);
            foreach (var j in Enumerable.Range(0, _length))
            {
                if (State._inputs[j] != false)
                {
                    var Num = GetOutputNumber(j);
                    if (Num < _length)
                    {
                        State._outputs[Num] = true;
                    }
                    else
                    {
                        Logger.UltraLogger.Instance.AddToLog("Net: Wrong pblock initializaion", Logger.MsgType.Error);
                        throw new NotImplementedException();
                    }
                }
            }
            ret.Add(State);
            return ret;
        }
    }
    class KBlock : Block
    {
        public override void Init(List<byte> arg)
        {
            throw new NotImplementedException("KBlock do not needed to init");
        }
        public override List<BlockState> ExtractStates(BlockStateExtrParams Params)
        {
            throw new NotImplementedException();
        }
    }
    class SBlock : Block
    {
        ISBlockDB _database;
        List<List<short>> CorMatrix;
        List<List<bool>> FuncMatrix;
        public List<BlockState> _states;
        int VarCount;
        byte _length;
        public List<List<short>> GetCorMatrix()
        {
            return CorMatrix;
        }
        public List<List<bool>> GetFuncMatrix()
        {
            return FuncMatrix;
        }
        public override List<BlockState> ExtractStates(BlockStateExtrParams Params)// MIN prevalence, current inputs
        {
            var States = new List<BlockState>();
            var MIN = Params.MIN.ActiveBlocksCount / ((double)Params.MIN.ActiveBlocksCount);
            for (int i = 0; i < _states.Count; i++)
            {
                var state = States[i];
                var P = (state._cor * Params.CurrentPrevalence.Mul) / ((double)Params.CurrentPrevalence.ActiveBlocksCount + 1);
                if ((Params.CurrentPrevalence.Mul <= 0) || (P > MIN))
                {
                    if (Enumerable.Range(0, state._inputs.Count).All(x => (state._inputs[x] == Params.Inputs[x])))
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
            _states = new List<BlockState>(1 << (2 * VarCount));
            for (int row = 1; row < CorMatrix.Count; row++)
            {
                for (int col = 1; col < CorMatrix[0].Count; col++)
                {
                    _states.Add(new BlockState(Math.Abs(CorMatrix[row][col]), col, row, _length));
                }
            }
            _states = _states.OrderByDescending(o => Math.Abs(o._cor)).ToList();
        }
        short GetCorrelation(int inputs, int outputs)
        {
            return CorMatrix[outputs][inputs];
        }
        public override void Init(List<byte> arg) // from interfaces
        {
            const int bias = 0; //S-Block record type: 'some args, Line1, Line2, ..., LineN'. No some args => Bias=0.
            if (arg.Count >= bias)
            {
                if (arg.Count - bias != 0)
                    VarCount = (int)Math.Log(arg.Count - bias, 2); // Matrix: 2**n x n
                if (VarCount == this._length)
                {
                    FuncMatrix.AddRange(Enumerable.Repeat(new List<bool>(), arg.Count - bias));
                    //Line0: 		y0..yn
                    //Line1: 		y0..yn
                    //....................
                    //Line2**n-1: 	y0..yn
                    for (int i = bias; i < arg.Count; i++)
                    {
                        //FuncMatrix[i - bias].AddRange(Enumerable.Repeat<byte>(0, VarCount));
                        for (int j = VarCount - 1; j >= 0; j--)
                        {
                            if ((arg[i] & (1 << j)) == 0)
                            {
                                FuncMatrix[i - bias].Add(false);
                            }
                            else
                            {
                                FuncMatrix[i - bias].Add(true);
                            }
                        }
                    }
                    if (_database != null)
                        CorMatrix = _database.GetCorMatrix(FuncMatrix);
                    else
                    {
                        _database = new SBlockDB();
                    }
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
            CorMatrix.Clear();
            FuncMatrix.Clear();
            VarCount = 0;
        }
        public SBlock(ISBlockDB database, byte block_length) //!NOW USED
        {
            _database = database;
            CorMatrix.Clear();
            FuncMatrix.Clear();
            VarCount = 0;
            _length = block_length;
        }
        public SBlock(List<List<bool>> Matrix, SBlockDB database)
        {
            _database = database;
            CorMatrix.Clear();
            FuncMatrix.Clear();
            FuncMatrix = Matrix;
            VarCount = 0;
            CorMatrix = _database.GetCorMatrix(FuncMatrix);
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
            Blocks.AddRange(Enumerable.Repeat(new SBlock(db, block_length), blocks_count));
        }
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
    class SPNet : ISPNet
    {
        private static Prevalence MultiThreadParam1; //correlation
        private static long MultiThreadParam2;
        private static double MultiThreadParam3;
        private object syncRoot = new object();
        public Prevalence GetMultiThreadPrevalence()
        {
            lock(syncRoot)
            {
                return MultiThreadParam1;
            }
        }
        public void SetMultiThreadPrevalence(Prevalence P)
        {
            lock(syncRoot)
            {
                MultiThreadParam1 = P;
            }
        }
        List<Layer> Layers;
        SPNetSettings _settings;
        IWorker _worker;
        CallbackAddSolution AddSolution; 
        public SPNetSettings GetSettings()
        {
            return _settings;
        }
        public void AddLayer(Layer layer)
        {
            Layers.Add(layer);
        }
        public void SetSBlockDB(SBlockDB db)
        {
            _settings.db = db;
        }
        public List<ILayer> GetLayers()
        {
            return Layers.ConvertAll(x => ((ILayer)x));
        }
        public SPNet(SPNetSettings settings)
        {
            _settings = settings;
            Layers = new List<Layer>();
        }
        public void DeleteLayer(byte number)
        {
            Layers.RemoveAt(number);
        }
        public void AddLayer(LayerType type) // from interfaces
        {
            switch (type)
            {
                case LayerType.KLayer:
                    {
                        var layer = new KLayer();
                        AddLayer(layer);
                        break;
                    }
                case LayerType.SLayer:
                    {
                        var layer = new SLayer(_settings.db, (byte)(_settings.word_length / _settings.sblock_count), _settings.sblock_count);
                        AddLayer(layer);

                        break;
                    }
                case LayerType.PLayer:
                    {
                        var layer = new PLayer(_settings.word_length);
                        AddLayer(layer);
                        break;
                    }
            }
        }
        public CallbackAddSolution GetCallbackAddSolution()
        {
            return AddSolution;
        }
        public void PerformLinearAnalisys(AnalisysParams Params)
        {
            //throw new NotImplementedException();
            try
            {
                this.AddSolution = Params.AddSolution;
                var TaskerParams = new TaskerParams(this, Params.Alg);
                _worker = new Worker(new WorkerParams(Environment.ProcessorCount, TaskerParams));
                if (!Params.ASync)
                {
                    _worker.Run();
                }
                else
                {
                    _worker.AsyncRun();
                }
            }
            catch
            {
                Logger.UltraLogger.Instance.ExportToFile();
            }
        }
    }

    public class Engine : IEngine
    {
        ISPNet TheNet;
        ISBlockDB TheDB;
        public virtual ISPNet GetSPNetInstance(SPNetSettings settings) //from interfaces
        {
            if (TheNet == null)
                TheNet = new SPNet(settings);
            return TheNet;
        }
        public virtual ISBlockDB GetSBlockDBInstance(Dictionary<List<short>, List<short>> db=null) //from interfaces
        {
            if (TheDB == null)
                TheDB = new SBlockDB(db);
            return TheDB;
        }
    }
}