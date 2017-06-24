using System;
using System.Collections.Generic;
using System.IO;

namespace Allax
{
    //Yuri. To get ISPNet and ISBlockDB use "IEngine E=new Engine();" or "Engine E=new Engine();"
    public interface IEngine
    {
        event EventHandler ALLTASKSDONE;
        event TASKHANDLER TASKDONE;
        void AbortAnalisys();
        void Init(EngineSettings Settings);
        EngineSettings GetSettings(); 
        IWorker GetWorkerInstance();
        ITasker GetTaskerInstance();
        Dictionary<AvailableSolverTypes, Solver> GetSolvers();
        Prevalence GetMultiThreadPrevalence();
        void SetMultiThreadPrevalence(Prevalence P);
        void PerformAnalisys(AnalisysParams Params);
        /// <summary>
        /// Engine will contains ISPNet instance
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        ISPNet CreateSPNetInstance(SPNetSettings Settings);
        ISPNet GetSPNetInstance();
        /// <summary>
        /// Engine will contains SBLockDB. Automatically ISPNet must know about new DB
        /// </summary>
        /// <param name="FS">FS with file to read</param>
        /// <param name="xml">If xml format must be used</param>
        /// <returns></returns>
        ISBlockDB InjectSBlockDB(FileStream FS, bool xml = false);
        ISBlockDB GetSBlockDBInstance();
        void SerializeDB(FileStream FS, bool xml=false);
    }
    public interface IBlock
	{
		void Init(List<byte> arg); // P-Block: "1, 8 , 4, 3, 5, 7, 6, 2" array of char where char is num, not text.
                                   // S-Block. First block 4x4: " 0x0A, 0x03, 0x03, ... , 0x01" (Length=16). Second block 4x4: "0x01, 0x02, 0xF, ..., 0x02, 0x01" (Length=16)
        List<BlockState> ExtractStates(BlockStateExtrParams Params);
    }
	public interface ILayer
	{
        LayerType GetLayerType();
		List<IBlock> GetBlocks(); //
		IBlock GetBlock(byte number); // 1..N
		void DeleteBlock(byte number); //!! (note for me)
	}
    public interface ISBlockDB
    {
        DBNote GetNoteFromDB(List<List<bool>> funcMatrix);
        DBNote GetNoteFromDB(List<byte> funcMatrix, int VarCount);
        List<List<short>> GetCorMatrix(List<List<bool>> funcMatrix);
        List<List<short>> GetDifMatrix(List<List<bool>> funcMatrix);
    }
	public interface ISPNet
	{
		void AddLayer(LayerType Type);
		void DeleteLayer(byte Number);
		List<ILayer> GetLayers();	
        SPNetSettings GetSettings();
        void SetSBlockDB(ISBlockDB DB);
    }
}