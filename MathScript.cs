using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
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
            var Note = new DBNote(CM, DM);
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
            for (int i = 0; i < func.Count; i++)
            {
                ret.Add((short)((func[i] == 0) ? 1 : -1));
            }
            return ret;
        }
        public List<List<short>> GetCorMatrix(List<List<bool>> funcMatrix)
        {
            var CorMatrix = new List<List<short>>(funcMatrix.Count);
            CorMatrix.AddRange(Enumerable.Range(0, funcMatrix.Count).Select(i => new List<short>()).ToList());
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
            ret.AddRange(Enumerable.Range(0, funcMatrix.Count).Select(i => new List<short>(funcMatrix.Count).ToList()));
            for (int a = 0; a < funcMatrix.Count; a++)
            {
                for (int b = 0; b < funcMatrix.Count; b++)
                {
                    int Counter = 0;
                    foreach (var x in Enumerable.Range(0, funcMatrix.Count))
                    {
                        if ((funcList[x] ^ funcList[x ^ a]) == b)
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
    }
    abstract class Block : IBlock
    {
        public abstract void Init(List<byte> arg); //SBlock size:4-8. Whole word: up to 64bit.
        public abstract List<BlockState> ExtractStates(BlockStateExtrParams Params);
        public byte BlockSize;
    };
    class PBlock : Block
    {
        List<int> outputNumber;
        public int GetOutputNumber(int InputNumber) // 
        {
            return outputNumber[InputNumber];
        }
        public PBlock(byte word_length)
        {
            BlockSize = word_length;
            outputNumber = new List<int>(BlockSize);
        }
        public override void Init(List<byte> arg) // from interfaces
        {
            const int bias = 0; //P-Block record type: 'some args, Output1, Output1, ..., OutputN'. No some args => Bias=0. Output value starts with 1.
            if (arg.Count >= bias)
            {
                if (arg.Count - bias == BlockSize)
                {
                    outputNumber.Clear();
                    outputNumber.AddRange(Enumerable.Repeat(0, BlockSize));
                    for (int i = bias; i < arg.Count; i++)
                    {
                        outputNumber[i - bias] = arg[i] - 1; // Convert from global numeration
                    }
                }
            }
        }
        public override List<BlockState> ExtractStates(BlockStateExtrParams Params)
        {
            var State = new BlockState(Params.Inputs, this.BlockSize);
            var ret = new List<BlockState>(1);
            //          var Inputs = WayConverter.ToList(State.inputs, State.BlockSize);
            //          var Outputs = new List<bool>(Enumerable.Repeat(false, State.BlockSize));
            /*
                        for(int i= State.BlockSize-1; i>=0;i--)
                        {
                            if ((State.inputs & (1 << i)) != 0)
                            {
                                var Num = GetOutputNumber(State.BlockSize - 1 - i);
                                if (Num < BlockSize)
                                {
                                    State.outputs = (byte)(State.outputs | ((long)1 << (State.BlockSize - 1 - Num)));
                                }
                                else
                                {
                                    Logger.UltraLogger.Instance.AddToLog("Net: Wrong pblock initializaion", Logger.MsgType.Error);
                                    throw new NotImplementedException();
                                }
                            }
                        }*/
            foreach (var j in Enumerable.Range(0, BlockSize))
            {
                if (State.CustomInput[j] != false)
                {
                    var Num = GetOutputNumber(j);
                    if (Num < BlockSize)
                    {
                        State.CustomOutput[Num] = true;
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
            //throw new NotImplementedException("KBlock do not needed to init");
        }
        public override List<BlockState> ExtractStates(BlockStateExtrParams Params)
        {
            //throw new NotImplementedException();
            return null;
        }
    }
    class SBlock : Block
    {
        ISBlockDB _database;
        List<List<bool>> FuncMatrix;
        DBNote Note;
        int VarCount;
        public override List<BlockState> ExtractStates(BlockStateExtrParams Params)// MIN prevalence, current inputs
        {
            var ret = new List<BlockState>();
            var States = (Params.Type == AnalisysType.Linear) ? Note.LStates : Note.DStates;
            var Inputs = (int)Params.Inputs;
            var Count = States[Inputs].Count;
            for (int i = 0; i < Count; i++)
            {
                var state = States[Inputs][i];
                if (state.MatrixValue == 0)
                {
                    break;
                }
                var P = state.MatrixValue * Params.CurrentPrevalence;
                if ((P >= Params.MIN) || (!Params.CheckPrevalence))
                {

                    ret.Add(state);
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
                if (VarCount == this.BlockSize)
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
                    Note = _database.GetNoteFromDB(FuncMatrix);
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
            if (FuncMatrix != null)
                FuncMatrix.Clear();
            else
            {
                FuncMatrix = new List<List<bool>>();
            }
            VarCount = 0;
            BlockSize = block_length;
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
            return new List<IBlock> { new KBlock() };//throw new NotImplementedException();
        }
        public override void DeleteBlock(byte number)
        {
            //throw new NotImplementedException();
        }
        public override IBlock GetBlock(byte number)
        {
            return new KBlock();// throw new NotImplementedException();
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
            foreach (var i in Enumerable.Range(0, blocks_count))
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
        static Dictionary<AvailableSolverTypes, ISolver> Solvers = new Dictionary<AvailableSolverTypes, ISolver> {
                { AvailableSolverTypes.GreedySolver, new GreedySolver() },
                {AvailableSolverTypes.AdvancedSolver, new AdvancedSolver() },
                { AvailableSolverTypes.BruteforceSolver, new BaseSolver() } };
        public event TASKHANDLER ONTASKDONE;
        public event TASKHANDLER ONALLTASKSDONE;
        public event PROGRESSHANDLER ONPROGRESSCHANGED;
        public event ADDSOLUTIONHANDLER ONSOLUTIONFOUND;
        IWorker TheWorker;
        ISPNet TheNet;
        ISBlockDB TheDB;
        ITasker TheTasker;
        EngineSettings Settings;
        ulong AllTasksCount;
        private static long TaskCounter;
        bool ASync;
        private static Prevalence MultiThreadParam1; //Prevalence
        private static readonly object syncRoot = new object();
        private static readonly object syncRoot2 = new object();
        public Engine(/*EngineSettings Settings*/)
        {
            TheWorker = null;
            TheNet = null;
            TheDB = null;
            Init(new EngineSettings(PublishSolution));
        }
        private void PublishSolution(Solution S)
        {
            if(!ASync)
            {
                ONSOLUTIONFOUND(S);
            }
            else
            {
                ONSOLUTIONFOUND.BeginInvoke(S, null, null);
            }
        }
        public void Init(EngineSettings Settings)
        {
            this.Settings = Settings;
        }
        private long IncrementTaskCounter()
        {
            lock (syncRoot2)
            {
                TaskCounter++;
                return TaskCounter;
            }
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
        private void Reset()
        {
            this.SetMultiThreadPrevalence(new Prevalence(0, 0, TheNet.GetSettings().SBoxSize));
            TaskCounter = 0;
            Solvers = new Dictionary<AvailableSolverTypes, ISolver> {
                { AvailableSolverTypes.GreedySolver, new GreedySolver() },
                {AvailableSolverTypes.AdvancedSolver, new AdvancedSolver() },
                { AvailableSolverTypes.BruteforceSolver, new BaseSolver() } };
        }
        public void PerformAnalisys(AnalisysParams Params)
        {
            try
            {
                Reset();
                ASync = Params.ASync;
                var TaskerParams = new TaskerParams(this, Params.Alg);
                TheTasker = new Tasker(TaskerParams);
                AllTasksCount = TheTasker.GetTasksCount();
                if(AllTasksCount>((ulong)1<<32))
                {
                    throw new Exception("Tasks count is above 4 billions!!!");
                }
                var WorkerParams = new WorkerParams(this, Params.MaxThreads, Params.ASync);
                TheWorker = new Worker(WorkerParams);
                TheWorker.TASKDONE += TheWorker_TASKDONE;
                TheWorker.ALLTASKSDONE += TheWorker_ALLTASKSDONE;
                {
                    if (!Params.ASync)
                    {
                        if (ONPROGRESSCHANGED.GetInvocationList().Length != 0)
                            ONPROGRESSCHANGED(0);
                        TheWorker.Run();
                    }
                    else
                    {
                        if (ONPROGRESSCHANGED.GetInvocationList().Length != 0)
                            ONPROGRESSCHANGED.BeginInvoke(0, null, null);
                        TheWorker.AsyncRun();
                    }
                }
            }
            catch(Exception e)
            {
                Logger.UltraLogger.Instance.ExportToFile();
                throw new Exception(e.Message);
            }
        }
        private void TheWorker_ALLTASKSDONE(ITask T)
        {
            if (!ASync)
            {
                if (ONALLTASKSDONE.GetInvocationList().Length != 0)
                    ONALLTASKSDONE(T);
            }
            else
            {
                System.Threading.Thread.Sleep(50); // some workaround
                if (ONALLTASKSDONE.GetInvocationList().Length != 0)
                    ONALLTASKSDONE.BeginInvoke(T, null, null);
            }
        }
        private void TheWorker_TASKDONE(ITask T)
        {
            var Value = IncrementTaskCounter() / (double)AllTasksCount;
            if (!ASync)
            {
                if (ONTASKDONE.GetInvocationList().Length != 0)
                    ONTASKDONE.BeginInvoke(T, null, null);
                if (ONPROGRESSCHANGED.GetInvocationList().Length != 0)
                    ONPROGRESSCHANGED.BeginInvoke(Value, null, null);
            }
            {
                if (ONTASKDONE.GetInvocationList().Length != 0)
                    ONTASKDONE(T);
                if (ONPROGRESSCHANGED.GetInvocationList().Length != 0)
                    ONPROGRESSCHANGED(Value);
            }
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
            if (TheNet != null)
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
        public void AbortAnalisys()
        {
            if(TheWorker!=null)
            {
                TheWorker.Dispose();
                TheWorker = null;
                TheTasker = null;
                GC.Collect();
            }
        }
        public Dictionary<AvailableSolverTypes, ISolver> GetSolvers()
        {
            return Solvers;
        }
    }
}