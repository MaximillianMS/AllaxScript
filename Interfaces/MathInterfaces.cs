using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace Allax
{
    //Yuri. To get ISPNet and ISBlockDB use "IEngine E=new Engine();" or "Engine E=new Engine();"
    public interface IEngine
    {
        ISPNet GetSPNetInstance(SPNetSettings settings, bool NewNet=false);
        ISBlockDB GetSBlockDBInstance(string xmlSerializedDB="");
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
    }
	public interface ISPNet
	{
		void AddLayer(LayerType type);
		void DeleteLayer(byte number);
		List<ILayer> GetLayers();	
		void PerformAnalisys(AnalisysParams Params);
        SPNetSettings GetSettings();
        CallbackAddSolution GetCallbackAddSolution();
        Prevalence GetMultiThreadPrevalence();
        void SetMultiThreadPrevalence(Prevalence P);
	}
}