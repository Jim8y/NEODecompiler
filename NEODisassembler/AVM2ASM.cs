using System;
using System.Collections.Generic;
using System.Text;

namespace NEODisassembler
{
   class Avm2Asm
    {
        public static NeoCode[] Trans(byte[] script)
        {
            NeoModule neoModule = new NeoModule();
            NeoMethod method = new NeoMethod();

            method.addr = 0;
            method.name = "Main";
            neoModule.mapMethods[method.name] = method;

            var breader = new ByteReader(script);
            List<NeoCode> arr = new List<NeoCode>();
            while (breader.End() == false)
            {
                var o = new NeoCode();
                o.addr = breader.addr;
                o.code = breader.ReadOP();
                try
                {
                    //push 特别处理
                    if (o.code >= OpCode.PUSHBYTES1 && o.code <= OpCode.PUSHBYTES75)
                    {
                        o.paramType = ParamType.ByteArray;
                        var _count = (int)o.code;
                        o.paramData = breader.ReadBytes(_count);
                    }
                    else
                    {
                        switch (o.code)
                        {
                            case OpCode.PUSH0:
                            case OpCode.PUSHM1:
                            case OpCode.PUSH1:
                            case OpCode.PUSH2:
                            case OpCode.PUSH3:
                            case OpCode.PUSH4:
                            case OpCode.PUSH5:
                            case OpCode.PUSH6:
                            case OpCode.PUSH7:
                            case OpCode.PUSH8:
                            case OpCode.PUSH9:
                            case OpCode.PUSH10:
                            case OpCode.PUSH11:
                            case OpCode.PUSH12:
                            case OpCode.PUSH13:
                            case OpCode.PUSH14:
                            case OpCode.PUSH15:
                            case OpCode.PUSH16:
                                o.paramType = ParamType.None;
                                break;
                            case OpCode.PUSHDATA1:
                                {
                                    o.paramType = ParamType.ByteArray;
                                    var _count = breader.ReadByte();
                                    o.paramData = breader.ReadBytes(_count);
                                }
                                break;
                            case OpCode.PUSHDATA2:
                                {
                                    o.paramType = ParamType.ByteArray;
                                    var _count = breader.ReadUInt16();
                                    o.paramData = breader.ReadBytes(_count);
                                }
                                break;
                            case OpCode.PUSHDATA4:
                                {
                                    o.paramType = ParamType.ByteArray;
                                    var _count = breader.ReadInt32();
                                    o.paramData = breader.ReadBytes(_count);
                                }
                                break;
                            case OpCode.NOP:
                                o.paramType = ParamType.None;
                                break;
                            case OpCode.JMP:
                            case OpCode.JMPIF:
                            case OpCode.JMPIFNOT:
                                o.paramType = ParamType.Addr;
                                o.paramData = breader.ReadBytes(2);
                                break;
         
                            case OpCode.CALL:
                                o.paramType = ParamType.Addr;
                                o.paramData = breader.ReadBytes(2);
                                // need re-calculate the address
                                o.needfixfunc = true;
                                break;
                            case OpCode.RET:
                                ExecutionEngine.ExecuteOp(method);
                                o.paramType = ParamType.None;
                                // If it is not the end, create a new method and store it into the neomodule
                                // The name of new method is sub_ and the address
                                if(o.addr< script.Length-1)
                                {
                                    method = new NeoMethod();
                                    method.name = "sub_" + (o.addr + 1);
                                    method.addr = o.addr + 1;
                                }

                                break;
                            case OpCode.APPCALL:
                            case OpCode.TAILCALL:
                                o.paramType = ParamType.ByteArray;
                                o.paramData = breader.ReadBytes(20);
                                break;
                            case OpCode.SYSCALL:
                                o.paramType = ParamType.String;
                                o.paramData = breader.ReadVarBytes();
                                break;
                            case OpCode.DUPFROMALTSTACK:
                            case OpCode.TOALTSTACK:
                            case OpCode.FROMALTSTACK:
                            case OpCode.XDROP:
                            case OpCode.XSWAP:
                            case OpCode.XTUCK:
                            case OpCode.DEPTH:
                            case OpCode.DROP:
                            case OpCode.DUP:
                            case OpCode.NIP:
                            case OpCode.OVER:
                            case OpCode.PICK:
                            case OpCode.ROLL:
                            case OpCode.ROT:
                            case OpCode.SWAP:
                            case OpCode.TUCK:
                                o.paramType = ParamType.None;
                                break;

                            case OpCode.CAT:
                            case OpCode.SUBSTR:
                            case OpCode.LEFT:
                            case OpCode.RIGHT:
                            case OpCode.SIZE:
                                o.paramType = ParamType.None;
                                break;

                            case OpCode.INVERT:
                            case OpCode.AND:
                            case OpCode.OR:
                            case OpCode.XOR:
                            case OpCode.EQUAL:
                                o.paramType = ParamType.None;
                                break;

                            case OpCode.INC:
                            case OpCode.DEC:
                            case OpCode.SIGN:
                            case OpCode.NEGATE:
                            case OpCode.ABS:
                            case OpCode.NOT:
                            case OpCode.NZ:
                            case OpCode.ADD:
                            case OpCode.SUB:
                            case OpCode.MUL:
                            case OpCode.DIV:
                            case OpCode.MOD:
                            case OpCode.SHL:
                            case OpCode.SHR:
                            case OpCode.BOOLAND:
                            case OpCode.BOOLOR:
                            case OpCode.NUMEQUAL:
                            case OpCode.NUMNOTEQUAL:
                            case OpCode.LT:
                            case OpCode.GT:
                            case OpCode.LTE:
                            case OpCode.GTE:
                            case OpCode.MIN:
                            case OpCode.MAX:
                            case OpCode.WITHIN:
                                o.paramType = ParamType.None;
                                break;

                            // Crypto
                            case OpCode.SHA1:
                            case OpCode.SHA256:
                            case OpCode.HASH160:
                            case OpCode.HASH256:
                            case OpCode.CHECKSIG:
                            case OpCode.CHECKMULTISIG:
                                o.paramType = ParamType.None;
                                break;

                            // Array
                            case OpCode.ARRAYSIZE:
                            case OpCode.PACK:
                            case OpCode.UNPACK:
                            case OpCode.PICKITEM:
                            case OpCode.SETITEM:
                            case OpCode.NEWARRAY:
                            case OpCode.NEWSTRUCT:
                                o.paramType = ParamType.None;
                                break;

                            // Exceptions
                            case OpCode.THROW:
                            case OpCode.THROWIFNOT:
                                o.paramType = ParamType.None;
                                break;
                            default:
                                //Console.WriteLine("error"+ Enum.GetName(typeof(OpCode),o.code));
                                break;
                        }
                    }
                }
                catch(Exception err)
                {
                    o.error = true;
                    Console.WriteLine(err.Data);
                }
                arr.Add(o);
                // Store the neo code into the method
                method.neoCodes.Add(o);
                if (o.error)
                    break;
            }

            return arr.ToArray();
        }
    }
}
