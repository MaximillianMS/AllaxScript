using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllaxScript.Logger
{
    public enum MsgType { Action, Warning, Error }
    public struct Note
    {
        public int Thread;
        public DateTime Time;
        public int MsgLevel;
        public MsgType Type;
        public String Msg;
    }
}
