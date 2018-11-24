using System;
using System.Collections.Generic;
using System.Text;

namespace NEODisassembler
{
    public enum ParamType
    {
        None,
        ByteArray,
        String,
        Addr,
    }

    public class NeoModule
    {
        public NeoModule()
        {
        }

        //小蚁没类型，只有方法
        public SortedDictionary<int, NeoCode> total_Codes = new SortedDictionary<int, NeoCode>();

        public string mainMethod;
        public Dictionary<string, NeoMethod> mapMethods = new Dictionary<string, NeoMethod>();
        public Dictionary<string, NeoEvent> mapEvents = new Dictionary<string, NeoEvent>();


        public Dictionary<string, object> staticfields = new Dictionary<string, object>();
    }
    public class NeoMethod
    {
        public string name = "";
        public int addr = 0;
        public string displayName = "";
        public List<NeoParam> paramtypes = new List<NeoParam>();
        // Store the opcode of current method
        public List<NeoCode> neoCodes = new List<NeoCode>();
        public string returntype;
        public bool inSmartContract;
        //临时变量
        public List<NeoParam> body_Variables = new List<NeoParam>();

        //临时记录在此，会合并到一起
        public SortedDictionary<int, NeoCode> body_Codes = new SortedDictionary<int, NeoCode>();
        public int funcaddr;

        public string lastsfieldname = null;//最后一个加载的静态成员的名字，仅event使用

        public int lastparam = -1;//最后一个加载的参数对应
        public int lastCast = -1;

        public string toSrcCode()
        {
            string src = "function ";
            src += this.name + "(){";
            int addr = 0;
            //if (opcode > OpCode.PUSH16 && opcode != OpCode.RET && context.PushOnly)
            //{
            //    State |= VMState.FAULT;
            //    return;
            //}
            //if (opcode >= OpCode.PUSHBYTES1 && opcode <= OpCode.PUSHBYTES75)
            //    EvaluationStack.Push(context.OpReader.ReadBytes((byte)opcode));
    
            return src;
        }
    }

    public class NeoEvent
    {
        public string _namespace;
        public string name;
        public string displayName;
        public List<NeoParam> paramtypes = new List<NeoParam>();
        public string returntype;
    }

    public class NeoCode
    {
        public OpCode code = OpCode.NOP;
        public int addr;
        public byte[] bytes;
        public bool error;//bool
        public byte[] paramData;
        public ParamType paramType;
        public bool needfix = false;//lateparse tag
        public bool needfixfunc = false;
        public int srcaddr;
        public int[] srcaddrswitch;
        public string srcfunc;

        public string name()
        {
            return Enum.GetName(typeof(OpCode), code);
        }

        public string toString()
        {
            var name = this.getCodeName();
            if (this.paramType == ParamType.ByteArray)
            {
                name += "[" + this.AsHexString() + "]" + " (" + this.AsString() + ")";
            }
            else if (this.paramType == ParamType.String)
            {
                name += "[" + this.AsString() + "]";
            }
            else if (this.paramType == ParamType.Addr)
            {
                name += "[" + this.AsAddr() + "]";
            }
            return this.addr.ToString("x02") + ":" + name;
        }

        public string AsHexString()
        {
            var str = "0x";
            for (var i = 0; i < this.paramData.Length; i++)
            {
                var s = this.paramData[i].ToString("x02");
                str += s;
            }

            return str;
        }
        public string AsString()
        {
            return Encoding.UTF8.GetString(this.paramData);
        }
        public int AsAddr()
        {
            return BitConverter.ToInt16(this.paramData, 0);
        }
        public string getCodeName()
        {
            var name = "";
            if (this.error)
                name = "[E]";
            if (this.code == OpCode.PUSHT)
                return "PUSH1(true)";
            if (this.code == OpCode.PUSHF)
                return "PUSH0(false)";

            if (this.code > OpCode.PUSHBYTES1 && this.code < OpCode.PUSHBYTES75)
                return name + "PUSHBYTES" + (this.code - OpCode.PUSHBYTES1 + 1);
            else
                return name + Enum.GetName(typeof(OpCode), this.code);
        }
    }
    public class NeoParam
    {
        public NeoParam(string name, string type)
        {
            this.name = name;
            this.type = type;
        }
        public string name
        {
            get;
            private set;
        }
        public string type
        {
            get;
            private set;
        }
        public override string ToString()
        {
            return type + " " + name;
        }
    }
}
