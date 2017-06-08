using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Allax;
namespace AllaxScript
{
    public class Program
    {
        public static void Main()
        {
            Allax.IEngine E= new Engine();
            var SBDB = E.GetSBlockDBInstance();
            var Settings = new Allax.SPNetSettings(16, 4, SBDB);
            var Net = E.GetSPNetInstance(Settings);
            AddFullRound(Net);
            AddLastRound(Net);
            List<byte> SBlockInit = new List<byte>{ 14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7 };
            foreach(var B in Net.GetLayers()[1].GetBlocks())
            {
                B.Init(SBlockInit);
            }
        }
        static void AddFullRound(Allax.ISPNet Net)
        {
            Net.AddLayer(Allax.LayerType.KLayer);
            Net.AddLayer(Allax.LayerType.SLayer);
            Net.AddLayer(Allax.LayerType.PLayer);
        }
        static void AddLastRound(Allax.ISPNet Net)
        {
            Net.AddLayer(Allax.LayerType.KLayer);
            Net.AddLayer(Allax.LayerType.SLayer);
            Net.AddLayer(Allax.LayerType.KLayer);
        }
    }
}
