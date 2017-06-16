using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
        List<byte> FourierTransform(List<byte> func)
        {
            if (func.Count > 1)
            {
                var left = new List<byte>();
                var right = new List<byte>();
                left.AddRange(Enumerable.Repeat<byte>(0, func.Count / 2));
                right.AddRange(Enumerable.Repeat<byte>(0, func.Count / 2));
                for (int i = 0; i < func.Count / 2; i++)
                {
                    left[i] = (byte)(func[i] + func[i + func.Count / 2]);
                    right[i] = (byte)(func[i] - func[i + func.Count / 2]);
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
        List<byte> GetLinCombo(List<List<bool>> funcMatrix, int LinCombo)
        {
            if (funcMatrix.Count > 0)
            {
                var combo = new List<byte>();
                combo.AddRange(Enumerable.Repeat<byte>(0, funcMatrix.Count));
                for (int i = 0; i < funcMatrix.Count; i++)
                {
                    for (int j = 0; j < funcMatrix[0].Count; j++)
                    {
                        if ((LinCombo & (1 << (funcMatrix[0].Count - 1 - j))) != 0)
                            combo[i] += (byte)((funcMatrix[i][j]) ? 1 : 0);
                    }
                    combo[i] = (byte)(combo[i] & 1);
                }
                return combo;
            }
            return null;
        }
        List<byte> GetFuncAnalog(List<byte> func)
        {
            var ret = new List<byte>(func.Count);
            for(int i=0;i<func.Count;i++)
            {
                ret.Add((byte)((func[i] == 0) ? 1 : -1));
            }
            return ret;
        }
        public List<List<byte>> GetCorMatrix(List<List<bool>> funcMatrix)
        {
            var CorMatrix = new List<List<byte>>(funcMatrix.Count);
            CorMatrix.AddRange(Enumerable.Range(0, funcMatrix.Count).Select(i=>new List<byte>()).ToList());
            for (int LinCombo = 0; LinCombo < funcMatrix.Count; LinCombo++)
            {
                CorMatrix[LinCombo] = GetLinCombo(funcMatrix, LinCombo);
                var f_spectrum = FourierTransform(GetFuncAnalog(CorMatrix[LinCombo]));
                CorMatrix[LinCombo] = f_spectrum;
            }
            return CorMatrix;
        }

        public List<List<byte>> GetDifMatrix(List<List<bool>> funcMatrix)
        {
            var funcList = WayConverter.MatrixToList(funcMatrix);
            var ret = new List<List<byte>>();
            ret.AddRange(Enumerable.Range(0, funcMatrix.Count).Select(i=>new List<byte>(funcMatrix.Count).ToList()));
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
                    ret[a].Add((byte)Counter);
                }
            }
            return ret;
        }

        public DBNote GetNoteFromDB(List<byte> funcMatrix, int VarCount)
        {
            return GetNoteFromDB(WayConverter.ListToMatrix(funcMatrix, VarCount));
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
        List<List<bool>> FuncMatrix;
        public List<BlockState> LStates;
        public List<BlockState> DStates;
        int VarCount;
        byte _length;
        public List<List<byte>> GetCorMatrix()
        {
            return _database.GetCorMatrix(FuncMatrix);
        }
        public List<List<bool>> GetFuncMatrix()
        {
            return FuncMatrix;
        }
        public List<List<byte>> GetDifMatrix()
        {
            return _database.GetDifMatrix(FuncMatrix);
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
//                     CorMatrix = Note.CorMatrix;
//                     DifMatrix = Note.DifMatrix;
                    LStates = Note.LStates;
                    DStates = Note.DStates;
                }
                else
                {
                    throw new Exception("Argument length error");
                }
            }
        }
        void SetDB(ref SBlockDB database)
        {
            _database = database;
        }
        public SBlock()
        {
            _database = null;
            FuncMatrix.Clear();
            VarCount = 0;
        }
        public SBlock(ref ISBlockDB database, byte block_length) //!NOW USED
        {
            _database = database;
            if(FuncMatrix!=null)
            FuncMatrix.Clear();
            else
            {
                FuncMatrix = new List<List<bool>>();
            }
            VarCount = 0;
            _length = block_length;
        }
        public SBlock(List<List<bool>> Matrix, SBlockDB database)
        {
            _database = database;
            FuncMatrix = Matrix;
            VarCount = 0;
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
        protected List<IBlock> Blocks;
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
        public SLayer(ref ISBlockDB db, byte block_length, byte blocks_count)
        {
            type = LayerType.SLayer;
            Blocks = new List<IBlock>(blocks_count);
            foreach(var i in Enumerable.Range(0, blocks_count))
            {
                Blocks.Add(new SBlock(ref db, block_length));
            }
        }
    }
    class PLayer : Layer
    {
        public PLayer(byte word_length)
        {
            type = LayerType.PLayer;
            Blocks = new List<IBlock>(1);
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
        ISBlockDB DB;
        List<ILayer> Layers;
        SPNetSettings _settings;
        public SPNetSettings GetSettings()
        {
            return _settings;
        }
        public void AddLayer(Layer layer)
        {
            Layers.Add(layer);
        }
        public void SetSBlockDB(ISBlockDB db)
        {
            DB = db;
        }
        public List<ILayer> GetLayers()
        {
            return Layers.ConvertAll(x => ((ILayer)x));
        }
        public SPNet(SPNetSettings settings)
        {
            _settings = settings;
            Layers = new List<ILayer>();
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
                        var layer = new SLayer(ref DB, _settings.SBoxSize, _settings.SBoxCount);
                        AddLayer(layer);
                        break;
                    }
                case LayerType.PLayer:
                    {
                        var layer = new PLayer(_settings.WordLength);
                        AddLayer(layer);
                        break;
                    }
            }
        }
    }

    public class Engine : IEngine
    {
        public event TASKDONEHANDLER TASKDONE;
        IWorker TheWorker;
        ISPNet TheNet;
        ISBlockDB TheDB;
        ITasker TheTasker;
        EngineSettings Settings;
        private static Prevalence MultiThreadParam1; //Prevalence
        private static readonly object syncRoot = new object();
        public Engine(EngineSettings Settings)
        {
            TheWorker = null;
            TheNet = null;
            TheDB = null;
            Init(Settings);
        }
        public void Init(EngineSettings Settings)
        {
            this.Settings = Settings;
        }

        public Prevalence GetMultiThreadPrevalence()
        {
            lock (syncRoot)
            {
                return MultiThreadParam1;
            }
        }
        public void SetMultiThreadPrevalence(Prevalence P)
        {
            lock (syncRoot)
            {
                MultiThreadParam1 = P;
            }
        }
        public void PerformAnalisys(AnalisysParams Params)
        {
            try
            {
                this.SetMultiThreadPrevalence(new Prevalence(0, 0, TheNet.GetSettings().SBoxSize));
                var TaskerParams = new TaskerParams(this, Params.Alg);
                TheTasker = new Tasker(TaskerParams);
                var WorkerParams = new WorkerParams(this, Params.MaxThreads);
                TheWorker = new Worker(WorkerParams);
                TheWorker.TASKDONE += TheWorker_TASKDONE;
                {
                    if (!Params.ASync)
                    {
                        TheWorker.Run();
                    }
                    else
                    {
                        TheWorker.AsyncRun();
                    }
                }
            }
            catch
            {
                Logger.UltraLogger.Instance.ExportToFile();
                throw new NotImplementedException();
            }
        }

        private void TheWorker_TASKDONE(Task T)
        {
            TASKDONE.BeginInvoke(T, null, null);
        }

        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            return Zip(bytes);
        }
        public static byte[] Zip(byte[] bytes)
        {
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
        public static byte[] Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                return mso.ToArray();
            }
        }
        public void SerializeDB(FileStream FS, bool xml=false)
        {
            if (TheDB == null)
                return;
            if (xml)
            {
                var xmlSerializer = new XmlSerializer(typeof(SBlockDB));
                xmlSerializer.Serialize(FS, TheDB);
            }
            else
            {
                using (var stream = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, TheDB);
                    stream.Flush();
                    stream.Position = 0;
                    var arr = stream.ToArray();
                    arr = Zip(Zip(arr));
                    FS.Write(arr, 0, arr.Length);
                }
            }
        }
        public virtual ISPNet GetSPNetInstance()
        {
            return TheNet;
        }
        public virtual ISPNet CreateSPNetInstance(SPNetSettings settings) //from interfaces
        {
            TheNet = new SPNet(settings);
            if (TheDB ==null)
            {
                GetSBlockDBInstance();
            }
            UpdateSBlockDB();
            return TheNet;
        }
        private void UpdateSBlockDB()
        {
            TheNet.SetSBlockDB(TheDB);
        }
        public virtual ISBlockDB GetSBlockDBInstance()
        {
            if(TheDB == null)
            {
                TheDB = new SBlockDB();
                if(TheNet!=null)
                {
                    UpdateSBlockDB();
                }
            }
            return TheDB;
        }
        public virtual ISBlockDB InjectSBlockDB(FileStream FS, bool xml = false) //from interfaces
        {
            var arr = new byte[FS.Length - FS.Position];
            FS.Read(arr, (int)FS.Position, arr.Length);
            arr = Unzip(Unzip(arr));
            if (xml)
            {
                    var xmlSerializer = new XmlSerializer(typeof(SBlockDB));
                    TheDB = (SBlockDB)xmlSerializer.Deserialize(FS);
            }
            else
            {
                using (var stream = new MemoryStream(arr))
                {
                    var formatter = new BinaryFormatter();
                    TheDB = (SBlockDB) formatter.Deserialize(stream);
                    //UnpackDB();
                }
            }
            UpdateSBlockDB();
            return TheDB;
        }
        void UnpackDB()
        {
            foreach(var N in ((SBlockDB)TheDB).FuncDB)
            {
                
                for(int i=0;i<N.Value.DStates.Count;i++)
                {
                    N.Value.DStates[i];
                }
            }
        }

        public IWorker GetWorkerInstance()
        {
            return TheWorker;
        }

        public ITasker GetTaskerInstance()
        {
            return TheTasker;
        }

        public EngineSettings GetSettings()
        {
            return Settings;
        }
    }
}