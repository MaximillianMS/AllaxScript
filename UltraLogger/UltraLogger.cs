using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllaxScript.Logger
{
    public class UltraLogger
    {
        static int MAX_NOTES = 100000;
        System.Collections.Concurrent.ConcurrentBag<Note> Notes = new System.Collections.Concurrent.ConcurrentBag<Note>();
        private static volatile UltraLogger instance;
        private UltraLogger() { }
        private static object syncRoot = new object();
        public static UltraLogger Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new UltraLogger();
                        }
                    }
                }

                return instance;
            }
        }
        public void AddToLog(String Msg, Logger.MsgType Type = MsgType.Action, int MsgLevel = 0)
        {
            var N = new Note();
            N.Thread = System.Threading.Thread.CurrentThread.ManagedThreadId;
            N.Time = DateTime.Now;
            N.Type = Type;
            N.Msg = Msg;
            N.MsgLevel = 0;
            Notes.Add(N);
            if (Notes.Count > MAX_NOTES)
            {
                throw new Exception("Over 100000 notes in Logger");
            }
        }
        public List<Note> GetNotes(bool SortByTime = false)
        {
            AddToLog("Exporting Notes from UltraLogger");
            List<Note> ret = new List<Note>(228);
            if (!SortByTime)
            {
                ret = new List<Note>(Notes.Count);
                lock (syncRoot)
                {
                    foreach (var N in Notes)
                    {
                        ret.Add(N);
                    }
            }
            }
            else
            {
                throw new NotImplementedException();
            }
            return ret;
        }
    }
}
