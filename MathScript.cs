using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Allax
{
    [Serializable()]
    public class SBlockDB : ISBlockDB
    {
        //Fourier Transform DB
        [XmlElement(ElementName = "FuncDB")]
        public SerializableDictionary<string, DBNote> FuncDB;
        public DBNote GetNoteFromDB(List<List<bool>> funcMatrix)
        {
            DBNote Note;
            var key = WayConverter.MatrixToString(funcMatrix);
            if (!FuncDB.TryGetValue(key, out Note))
            {
                Note = AddToFuncDB(key, funcMatrix);
            }
            else
            {
                ;
            }
            return Note;
        }
        public SBlockDB()
        {
            FuncDB = new SerializableDictionary<string, DBNote>();
        }
        public SBlockDB(SerializableDictionary<string, DBNote> DB=null)
        {
            if (DB != null)
            {
                FuncDB = DB;
            }
            else
            {
                FuncDB = new SerializableDictionary<string, DBNote>();
            }
        }
        DBNote AddToFuncDB(string key, List<List<bool>> funcMatrix)
        {
            var CM = GetCorMatrix(funcMatrix);
            var DM = GetDifMatrix(funcMatrix);
            var Note = new DBNote(funcMatrix, CM, DM);
            FuncDB.Add(key, Note);
            return Note;
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
                        if ((LinCombo & (1 << (funcMatrix[0].Count - 1 - j))) != 0)
                            combo[i] += (short)((funcMatrix[i][j]) ? 1 : 0);
                    }
                    combo[i] = (short)(combo[i] & 1);
                }
                return combo;
            }
            return null;
        }
        List<short> GetFuncAnalog(List<short> func)
        {
            var ret = new List<short>(func.Count);
            for(int i=0;i<func.Count;i++)
            {
                ret.Add((short)((func[i] == 0) ? 1 : -1));
            }
            return ret;
        }
        public List<List<short>> GetCorMatrix(List<List<bool>> funcMatrix)
        {
            var CorMatrix = new List<List<short>>(funcMatrix.Count);
            CorMatrix.AddRange(Enumerable.Range(0, funcMatrix.Count).Select(i=>new List<short>()).ToList());
            for (int LinCombo = 0; LinCombo < funcMatrix.Count; LinCombo++)
            {
                CorMatrix[LinCombo] = GetLinCombo(funcMatrix, LinCombo);
                var f_spectrum = FourierTransform(GetFuncAnalog(CorMatrix[LinCombo]));
                CorMatrix[LinCombo] = f_spectrum;
            }
            return CorMatrix;
        }

        public List<List<short>> GetDifMatrix(List<List<bool>> funcMatrix)
        {
            var funcList = WayConverter.MatrixToList(funcMatrix);
            var ret = new List<List<short>>();
            ret.AddRange(Enumerable.Range(0, funcMatrix.Count).Select(i=>new List<short>(funcMatrix.Count).ToList()));
            for(int a=0; a<funcMatrix.Count;a++)
            {
                for(int b=0;b<funcMatrix.Count;b++)
                {
                    int Counter = 0;
                    foreach(var x in Enumerable.Range(0, funcMatrix.Count))
                    { 
                        if((funcList[x] ^ funcList[x^a])==b)
                        {
                            Counter++;
                        }
                    }
                    ret[a].Add((short)Counter);
                }
            }
            return ret;
        }

        public DBNote GetNoteFromDB(List<byte> funcMatrix, int VarCount)
        {
            return GetNoteFromDB(WayConverter.ListToMatrix(funcMatrix, VarCount));
        }
        public string Serialize()
        {
            var xmlSerializer = new XmlSerializer(typeof(SBlockDB));
            StringWriter stringWriter = new StringWriter();
            xmlSerializer.Serialize(stringWriter, this);
            return stringWriter.ToString();
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
                    outputNumber.Clear();
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
        List<List<short>> DifMatrix;
        List<List<bool>> FuncMatrix;
        public List<BlockState> LStates;
        public List<BlockState> DStates;
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
        public List<List<short>> GetDifMatrix()
        {
            return DifMatrix;
        }
        public override List<BlockState> ExtractStates(BlockStateExtrParams Params)// MIN prevalence, current inputs
        {
            var ret = new List<BlockState>();
            var States = (Params.Type == AnalisysType.Linear) ? LStates : DStates;
            var Count = States.Count;
            for (int i = 0; i < Count; i++)
            {
                var state = States[i];
                var P = state.MatrixValue * Params.CurrentPrevalence;
                if (((P >= Params.MIN)&&(P.Numerator!=0)) || (!Params.CheckPrevalence))
                {
                    if (Enumerable.Range(0, state._inputs.Count).All(x => (state._inputs[x] == Params.Inputs[x])))
                    {
                        ret.Add(state);
                    }
                }
                else
                {
                    break;
                }
            }
            return ret;
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
                    FuncMatrix = WayConverter.ListToMatrix(arg.Skip(bias).ToList(), VarCount);
                    //Line0: 		y0..yn
                    //Line1: 		y0..yn
                    //....................
                    //Line2**n-1: 	y0..yn
                    if (_database == null)
                    {
                        _database = new SBlockDB();
                    }
                    var Note = _database.GetNoteFromDB(FuncMatrix);
                    CorMatrix = Note.CorMatrix;
                    DifMatrix = Note.DifMatrix;
                    LStates = Note.LStates;
                    DStates = Note.DStates;
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
            if(CorMatrix!=null)
            CorMatrix.Clear();
            else
            {
                CorMatrix = new List<List<short>>();
            }
            if(FuncMatrix!=null)
            FuncMatrix.Clear();
            else
            {
                FuncMatrix = new List<List<bool>>();
            }
            if(DifMatrix!=null)
            DifMatrix.Clear();
            else
            {
                DifMatrix = new List<List<short>>();
            }
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
            var Note = _database.GetNoteFromDB(FuncMatrix);
            CorMatrix = Note.CorMatrix;
            DifMatrix = Note.DifMatrix;
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
            Blocks = new List<Block>(blocks_count);
            Blocks.AddRange(Enumerable.Range(0, blocks_count).Select(i=>new SBlock(db, block_length)).ToList());
        }
    }
    class PLayer : Layer
    {
        public PLayer(byte word_length)
        {
            type = LayerType.PLayer;
            Blocks = new List<Block>(1);
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
        private static readonly object syncRoot = new object();
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
        public void PerformAnalisys(AnalisysParams Params)
        {
            //throw new NotImplementedException();
            try
            {
                this.SetMultiThreadPrevalence(new Prevalence(0, 0, this.GetSettings().word_length / this.GetSettings().sblock_count));
                this.AddSolution = Params.AddSolution;
                var TaskerParams = new TaskerParams(this, Params.Alg);
                var WP = new WorkerParams(Params.MaxThreads, TaskerParams, Params.TaskFinishedFunc);
                _worker = new Worker(WP);
                {
                    if (!Params.ASync)
                    {
                        _worker.Run();
                    }
                    else
                    {
                        _worker.AsyncRun();
                    }
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
        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }

                return mso.ToArray();
            }
        }
        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
        public virtual ISPNet GetSPNetInstance(SPNetSettings settings) //from interfaces
        {
            if (TheNet == null||(TheNet.GetSettings()!=settings))
                TheNet = new SPNet(settings);
            return TheNet;
        }
        public virtual ISBlockDB GetSBlockDBInstance(string xmlSerializedDB="") //from interfaces
        {
            if (TheDB == null||xmlSerializedDB!="")
            {
                if (xmlSerializedDB == "")
                    TheDB = new SBlockDB();
                else
                {
                    var xmlSerializer = new XmlSerializer(typeof(SBlockDB));
                    var stringReader = new StringReader(xmlSerializedDB);
                    TheDB = (SBlockDB) xmlSerializer.Deserialize(stringReader);
                }
            }
            return TheDB;
        }
    }
}