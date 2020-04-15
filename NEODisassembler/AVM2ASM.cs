using System;
using System.Collections.Generic;
using System.Text;

namespace NEODisassembler
{
   class Avm2Asm
    {
        public static Dictionary<int, NeoCode> Trans(byte[] script)
        {
            NeoModule neoModule = new NeoModule();
            NeoMethod method = new NeoMethod();

            method.addr = 0;
            method.name = "Main";
            neoModule.mapMethods[method.name] = method;

            var breader = new ByteReader(script);
            Dictionary<int, NeoCode> arr = new Dictionary<int, NeoCode>();
            while (breader.End() == false)
            {
                var o = new NeoCode();
                o.addr = breader.addr;
                o.code = breader.ReadOP();
                try
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
                            case OpCode.PUSHINT8:
                            case OpCode.PUSHINT16:
                            case OpCode.PUSHINT32:
                            case OpCode.PUSHINT64:
                            case OpCode.PUSHINT128:
                            case OpCode.PUSHINT256:
                            case OpCode.PUSHNULL:
                                o.paramType = ParamType.None;
                                break;
                            case OpCode.PUSHA:
                                o.paramType = ParamType.Addr;
                                //var _count = breader.ReadByte();
                                o.paramData = breader.ReadBytes(4);
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
                            case OpCode.JMPEQ:
                            case OpCode.JMPNE:
                            case OpCode.JMPGT:
                            case OpCode.JMPGE:
                            case OpCode.JMPLT:
                            case OpCode.JMPLE:
                                {
                                // Set loop symbol
                                o.paramType = ParamType.Addr;
                                o.paramData = breader.ReadBytes(1);
                                //if (o.AsAddr() < 0)
                                //{
                                //    var target = o.addr + o.AsAddr() - 3;
                                //    NeoCode targetCode = arr[target];
                                //    targetCode.beginOfLoop = true;
                                //   o.endOfLoop = true;
                                //}
                            }
                            break;
                            case OpCode.JMPIF_L:
                            case OpCode.JMPIFNOT_L:
                            case OpCode.JMPEQ_L:
                            case OpCode.JMPNE_L:
                            case OpCode.JMP_L:
                            case OpCode.JMPLT_L:
                            case OpCode.JMPGT_L:
                            case OpCode.JMPLE_L:
                            case OpCode.JMPGE_L:
                                {
                                    // Set loop symbol
                                    o.paramType = ParamType.Addr;
                                    o.paramData = breader.ReadBytes(4);
                                    //if (o.AsAddr() < 0)
                                    //{
                                    //    var target = o.addr + o.AsAddr() - 3;
                                    //    NeoCode targetCode = arr[target];
                                    //    targetCode.beginOfLoop = true;
                                    //   o.endOfLoop = true;
                                    //}
                                }
                                break;

                            case OpCode.CALL:
                                o.paramType = ParamType.Addr;
                                o.paramData = breader.ReadBytes(1);
                                // need re-calculate the address
                                o.needfixfunc = true;
                                break;
                            case OpCode.CALL_L:
                                o.paramType = ParamType.Addr;
                                o.paramData = breader.ReadBytes(4);
                                // need re-calculate the address
                                o.needfixfunc = true;
                                break;
                            case OpCode.CALLA:
                            //{
                                //o.paramType = ParamType.None;
                            //    o.paramType = ParamType.Addr;
                            //    var _count = breader.ReadVarInt();
                            //    o.paramData = breader.ReadBytes(_count);
                            //}
                           
                            //break;

                        case OpCode.ABORT:
                                //o.paramType = ParamType.None;
                            case OpCode.ASSERT:
                                //{
                                //    if (!TryPop(out bool x)) return false;
                                //    if (!x) return false;
                                //    break;
                                //}
                            case OpCode.THROW:
                                //{
                                //    return false;
                                //}
                            case OpCode.RET:
                                //ExecutionEngine.ExecuteOp(method);
                                o.paramType = ParamType.None;
                                // If it is not the end, create a new method and store it into the neomodule
                                // The name of new method is sub_ and the address
                                //if(o.addr< script.Length-1)
                                //{
                                //    method = new NeoMethod();
                                //    method.name = "sub_" + (o.addr + 1);
                                //    method.addr = o.addr + 1;
                                //}

                                break;
          
                            case OpCode.SYSCALL:
                                o.paramType = ParamType.String;
                                o.paramData = breader.ReadBytes(4);
                                break;
                            case OpCode.DEPTH:
                            case OpCode.DROP:
                            case OpCode.NIP:
                            case OpCode.XDROP:
                            case OpCode.CLEAR:

                            case OpCode.DUP:
                            case OpCode.OVER:
                            case OpCode.PICK:
                            case OpCode.ROLL:
                            case OpCode.ROT:
                            case OpCode.SWAP:
                            case OpCode.TUCK:
                            case OpCode.REVERSE3:
                            case OpCode.REVERSEN:
                            case OpCode.INITSSLOT:
                            case OpCode.INITSLOT:
                                o.paramType = ParamType.ByteArray;
                                o.paramData = breader.ReadBytes(1);
                                break;
                            case OpCode.LDSFLD0:
                            case OpCode.LDSFLD1:
                            case OpCode.LDSFLD2:
                            case OpCode.LDSFLD3:
                            case OpCode.LDSFLD4:
                            case OpCode.LDSFLD5:
                            case OpCode.LDSFLD6:
                            case OpCode.LDSFLD:
                                o.paramType = ParamType.ByteArray;
                                o.paramData = breader.ReadBytes(1);
                                break;

                            case OpCode.STSFLD0:
                            case OpCode.STSFLD1:
                            case OpCode.STSFLD2:
                            case OpCode.STSFLD3:
                            case OpCode.STSFLD4:
                            case OpCode.STSFLD5:
                            case OpCode.STSFLD6:
                            case OpCode.STSFLD:
                                o.paramType = ParamType.ByteArray;
                                o.paramData = breader.ReadBytes(1);
                                break;

                            case OpCode.LDLOC0:
                            case OpCode.LDLOC1:
                            case OpCode.LDLOC2:
                            case OpCode.LDLOC3:
                            case OpCode.LDLOC4:
                            case OpCode.LDLOC5:
                            case OpCode.LDLOC6:
                                o.paramType = ParamType.None;
                                break;

                            case OpCode.LDLOC:
                                o.paramType = ParamType.ByteArray;
                                o.paramData = breader.ReadBytes(1);
                                break;

                            case OpCode.STARG0:
                            case OpCode.STARG1:
                            case OpCode.STARG2:
                            case OpCode.STARG3:
                            case OpCode.STARG4:
                            case OpCode.STARG5:
                            case OpCode.STARG6:
                                o.paramType = ParamType.None;
                                break;
                            case OpCode.STARG:
                                o.paramType = ParamType.ByteArray;
                                o.paramData = breader.ReadBytes(1);
                                break;

                            case OpCode.NEWBUFFER:
                            case OpCode.MEMCPY:
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
                            case OpCode.NOTEQUAL:
                            case OpCode.SIGN:
                                o.paramType = ParamType.None;
                                break;

                            case OpCode.INC:
                            case OpCode.DEC:
                          
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
                            case OpCode.PACK:
                            case OpCode.UNPACK:
                            case OpCode.NEWARRAY0:
                            case OpCode.NEWARRAY:
                                o.paramType = ParamType.None;
                                break;
                            case OpCode.NEWARRAY_T:
                                o.paramType = ParamType.ByteArray;
                                o.paramData = breader.ReadBytes(1);
                                break;

                            case OpCode.MIN:
                            case OpCode.MAX:
                            case OpCode.WITHIN:
                            case OpCode.NEWSTRUCT0:
                            case OpCode.NEWSTRUCT:
                            case OpCode.NEWMAP:
                            case OpCode.KEYS:
                            case OpCode.VALUES:
                            case OpCode.APPEND:
                                o.paramType = ParamType.None;
                                break;

                            // Crypto
                            case OpCode.HASKEY:
                                o.paramType = ParamType.None;
                                break;

                            // Array
                            case OpCode.PICKITEM:
                            case OpCode.SETITEM:
                            case OpCode.REVERSEITEMS:
                            case OpCode.REMOVE:
                            case OpCode.CLEARITEMS:
                            case OpCode.ISNULL:
                            case OpCode.ISTYPE:
                            case OpCode.CONVERT:
                                o.paramType = ParamType.None;
                                break;

                            default:
                                //Console.WriteLine("error"+ Enum.GetName(typeof(OpCode),o.code));
                                break;
                        }
                  
                }
                catch(Exception err)
                {
                    o.error = true;
                    Console.WriteLine("Open File Error:" + err.ToString());
                }
                arr.Add(o.addr,o);
                // Store the neo code into the method
                method.neoCodes.Add(o);
                if (o.error)
                    break;
            }

            return arr;
        }
    }
}
