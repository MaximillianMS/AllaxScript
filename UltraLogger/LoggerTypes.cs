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
        public override String ToString()
        {
            String ret = "";
            ret += Time.ToShortDateString() + ":\t";
            ret += String.Format("Thread:\t{0}\t", Thread);
            ret += String.Format("MsgLevel:\t{0}\t", MsgLevel);
            ret += String.Format("MsgType:\t{0}\t", Type);
            ret += String.Format(Msg+"\t");
            return ret;
        }
    }
}
