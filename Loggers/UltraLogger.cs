﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allax.Logger
{
    public class UltraLogger
    {
        static int MAX_NOTES = 100000;
        static bool TurnedOn = false;
        const string PredefinedPath = "C:\\Windows\\Temp\\";
        System.Collections.Concurrent.ConcurrentBag<Note> Notes = new System.Collections.Concurrent.ConcurrentBag<Note>();
        private static volatile UltraLogger instance;
        private UltraLogger() { }
        private static readonly object syncRoot = new object();
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
            if (!TurnedOn)
                return;
            var N = new Note();
            N.Thread = System.Threading.Thread.CurrentThread.ManagedThreadId;
            N.Time = DateTime.Now;
            N.Type = Type;
            N.Msg = Msg;
            N.MsgLevel = 0;
            Notes.Add(N);
            if (Notes.Count > MAX_NOTES)
            {
                //AddToLog("Logger: MAX_NOTES Reached", MsgType.Warning);
                //lock(syncRoot)
                //{
                    ExportToFile();
                //}
                return;
                //throw new Exception("Logger: Over 100000 notes in Logger");
            }
        }
        public void ExportToFile(string Path = PredefinedPath)
        {
            AddToLog("Logger: Exporting Notes from UltraLogger started.");
            if (!System.IO.Directory.Exists(Path))
            {
                AddToLog("Logger: No such directory exists: \"" + Path+"\"");
                return;
            }
            Path+= "Allax_"+DateTime.Now.ToShortDateString()+".log";
            AddToLog("Logger: File: "+Path);
            if(Notes.Count!=0)
            using (var aFile = new System.IO.FileStream(Path, System.IO.FileMode.Append, System.IO.FileAccess.Write))
            using (var sw = new System.IO.StreamWriter(aFile))
            {
                lock (syncRoot)
                {

                    var Notes = GetNotes();
                    string Buffer = "";
                    for (int i = 0; i < Notes.Count; i++)
                    {
                        if (i % (MAX_NOTES / 100) != 0)
                        {
                            Buffer += Notes[i] + "\n";
                        }
                        else
                        {
                            sw.WriteLine(Buffer);
                            Buffer = "";
                        }
                    }
                    if (Buffer != "")
                    {
                        sw.WriteLine(Buffer);
                        Buffer = "";
                    }
                    this.Notes = new System.Collections.Concurrent.ConcurrentBag<Note>();
                }
            }
               // AddToLog("Logger: File creation failed through exporting UltraLogger notes.");
        }
        public List<Note> GetNotes(bool SortByTime = false)
        {
            AddToLog("Logger: Getting Notes from UltraLogger started.");
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
