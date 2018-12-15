using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace NEODisassembler
{

    public static class ExecutionEngine
    {
        private static int variable_count = -1;
        public static Stack<StackItem> InvocationStack { get; } = new Stack<StackItem>();
        public static Stack<StackItem> EvaluationStack = new Stack<StackItem>();
        public static Stack<StackItem> AltStack { get; } = new Stack<StackItem>();
        private static Queue<string> src_code= new Queue<string>();
        //Count the reference of variables
        private static Dictionary<string, int> variables = new Dictionary<string,int>();

        private static string getVariable()
        {
            variable_count++;
            string vari = "v_" + variable_count;
            variables[vari]=1;
            return vari;
        }
private static void addReference(string name){
     variables[name]= variables[name]+1;
}
        private static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            System.Array.Copy(data, index, result, 0, length);
            return result;
        }
        public static void ExecuteOp(NeoMethod method)
        {
            // TODO: parameter of function
            src_code.Enqueue("function " + method.name + "() {");

            NeoCode param_code = method.neoCodes[0];

            StackItem stackItem;
            string temp = "";
            foreach (NeoCode opcode in method.neoCodes)
            {

                if (opcode.code >= OpCode.PUSHBYTES1 && opcode.code <= OpCode.PUSHBYTES75)
                {
                    stackItem = new StackItem();
                    stackItem.name = temp = getVariable();
                    stackItem.type = Type.bytearray;
                    stackItem.byteArray = opcode.paramData;
                    EvaluationStack.Push(stackItem);

                    src_code.Enqueue("    byte[]" + temp + "= new byte[](\"" + opcode.AsString() + "\");" + "");
                }
                else
                    switch (opcode.code)
                    {
                        // Push value
                        case OpCode.PUSH0:
                            temp = getVariable();

                            stackItem = new StackItem();
                            stackItem.name = temp;
                            stackItem.type = Type.integer;
                            stackItem.integer = 0;
                            EvaluationStack.Push(stackItem);
                            src_code.Enqueue( "    int " + temp + " = 0;");
                            break;
                        case OpCode.PUSHDATA1:
                        case OpCode.PUSHDATA2:
                        case OpCode.PUSHDATA4:
                            temp = getVariable();
                            stackItem = new StackItem();
                            stackItem.name = temp;
                            stackItem.type = Type.bytearray;
                            stackItem.byteArray = opcode.paramData;
                            EvaluationStack.Push(stackItem);
                            src_code.Enqueue("    byte[] " + temp + "= new byte[](\"" + opcode.AsHexString() + "\");" + "");
                            break;
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
                            temp = getVariable();

                            stackItem = new StackItem();
                            stackItem.name = temp;
                            stackItem.type = Type.integer;
                            stackItem.integer = (int)opcode.code - (int)OpCode.PUSH1 + 1;
                            EvaluationStack.Push(stackItem);
                            src_code.Enqueue("    int " + temp + " = "+ ((int)opcode.code - (int)OpCode.PUSH1 + 1)+";");
                            break;

                        // Control
                        case OpCode.NOP:
                            break;
                        case OpCode.JMP:
                        case OpCode.JMPIF:
                        case OpCode.JMPIFNOT:
                            {
                                //src+="    "+
                                //int offset = context.OpReader.ReadInt16();
                                //offset = context.InstructionPointer + offset - 3;
                                //if (offset < 0 || offset > context.Script.Length)
                                //{
                                //    return;
                                //}
                                //bool fValue = true;
                                //if (opcode.code > OpCode.JMP)
                                //{
                                //    fValue = EvaluationStack.Pop().GetBoolean();
                                //    if (opcode == OpCode.JMPIFNOT)
                                //        fValue = !fValue;
                                //}
                                //if (fValue)
                                //    context.InstructionPointer = offset;
                            }
                            break;
                        case OpCode.CALL:

                            //InvocationStack.Push(context.Clone());
                            //context.InstructionPointer += 2;
                            //ExecuteOp(OpCode.JMP, CurrentContext);
                            break;
                        case OpCode.RET:

                            break;
                        case OpCode.APPCALL:
                        case OpCode.TAILCALL:
                            {
                                src_code.Enqueue("    APPCALL(\"" + opcode.paramData.ToString() + "\");");
                            }
                            break;
                        case OpCode.SYSCALL:
                            {
                                //TODO: copmare the function in OpAndSysCall
                                OpAndCall call= OpAndSysCall.sysCall[opcode.AsString()];
                                // param of call funtion
                                if(call.push == 1)
                                {
                                    temp = getVariable();

                                    stackItem = new StackItem();
                                    stackItem.name = temp;
                                    stackItem.type = Type.integer;
                                    stackItem.integer = EvaluationStack.Count;
                                    EvaluationStack.Push(stackItem);
                                    src_code.Enqueue( "    byte[] " + temp + " = " +opcode.AsString()+"(");
                                    EvaluationStack.Push(stackItem);
                                }

                                for (int i = 0; i < call.pop; i++)
                                {
                                    string name = EvaluationStack.Pop().name;
                                    addReference(name);
                                    src_code.Enqueue( "" + name +", ");
                                }
                                src_code.Enqueue( ");");
                            }
                            break;

                        // Stack ops
                        case OpCode.DUPFROMALTSTACK:
                            EvaluationStack.Push(AltStack.Peek());
                            break;
                        case OpCode.TOALTSTACK:
                            AltStack.Push(EvaluationStack.Pop());
                            break;
                        case OpCode.FROMALTSTACK:
                            EvaluationStack.Push(AltStack.Pop());
                            break;
                        case OpCode.XDROP:
                            {
                                //int n = EvaluationStack.Pop().integer;
                                //if (n < 0)
                                //{
                                //    return;
                                //}
                                //EvaluationStack.Remove(n);
                            }
                            break;
                        case OpCode.XSWAP:
                            {
                                //int n = EvaluationStack.Pop().integer;
                                //if (n < 0)
                                //{

                                //    return;
                                //}
                                //if (n == 0) break;

                                //StackItem xn = EvaluationStack.Peek(n);
                                //EvaluationStack.Set(n, EvaluationStack.Peek());
                                //EvaluationStack.Set(0, xn);
                            }
                            break;
                        case OpCode.XTUCK:
                            {
                                //int n = (int)EvaluationStack.Pop().GetBigInteger();
                                //if (n <= 0)
                                //{

                                //    return;
                                //}
                                //EvaluationStack.Insert(n, EvaluationStack.Peek());
                            }
                            break;
                        case OpCode.DEPTH:
                            temp = getVariable();

                            stackItem = new StackItem();
                            stackItem.name = temp;
                            stackItem.type = Type.integer;
                            stackItem.integer = EvaluationStack.Count;
                            EvaluationStack.Push(stackItem);
                            src_code.Enqueue( "    int " + temp + " = " + EvaluationStack.Count + ";");

                            break;
                        case OpCode.DROP:
                            EvaluationStack.Pop();
                            break;
                        case OpCode.DUP:
                            EvaluationStack.Push(EvaluationStack.Peek());
                            break;
                        case OpCode.NIP:
                            {
                                StackItem x2 = EvaluationStack.Pop();
                                EvaluationStack.Pop();
                                EvaluationStack.Push(x2);
                            }
                            break;
                        case OpCode.OVER:
                            {
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Peek();
                                EvaluationStack.Push(x2);
                                EvaluationStack.Push(x1);
                            }
                            break;
                        case OpCode.PICK:
                            {
                                //int n = EvaluationStack.Pop().integer;
                                //if (n < 0)
                                //{

                                //    return;
                                //}
                                //EvaluationStack.Push(EvaluationStack.Peek(n));
                            }
                            break;
                        case OpCode.ROLL:
                            {
                                StackItem n = EvaluationStack.Pop();

                                Stack<StackItem> st = new Stack<StackItem>();

                                for(int i = 0; i < n.integer; i++)
                                {
                                    st.Push(EvaluationStack.Pop());
                                }
                                StackItem target = EvaluationStack.Pop();

                                while (st.Count != 0)
                                {
                                    EvaluationStack.Push(st.Pop());
                                }
                                EvaluationStack.Push(target);
                            }
                            break;
                        case OpCode.ROT:
                            {
                                StackItem x3 = EvaluationStack.Pop();
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                EvaluationStack.Push(x2);
                                EvaluationStack.Push(x3);
                                EvaluationStack.Push(x1);

                            }
                            break;
                        case OpCode.SWAP:
                            {
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                EvaluationStack.Push(x2);
                                EvaluationStack.Push(x1);
                            }
                            break;
                        case OpCode.TUCK:
                            {
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                EvaluationStack.Push(x2);
                                EvaluationStack.Push(x1);
                                EvaluationStack.Push(x2);
                            }
                            break;
                        case OpCode.CAT:
                            {
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();

                                addReference(x2.name);
                                addReference(x1.name);
                                int len = x2.byteArray.Length + x1.byteArray.Length;
                                byte[] x3 = new byte[len];
                                x1.byteArray.CopyTo(x3, 0);
                                x2.byteArray.CopyTo(x3, x1.byteArray.Length);

                                temp = getVariable();
                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.bytearray;
                                stackItem.byteArray = x3;
                                EvaluationStack.Push(stackItem);

                                src_code.Enqueue( "    byte[] " + temp + "= "+ x1.name+ "+"+ x2.name + "");

                                EvaluationStack.Push(stackItem);
                            }
                            break;
                        case OpCode.SUBSTR:
                            {
                                int count = (int)EvaluationStack.Pop().integer;
                                if (count < 0)
                                {

                                    return;
                                }
                                int index = (int)EvaluationStack.Pop().integer;
                                if (index < 0)
                                {

                                    return;
                                }
                                StackItem x = EvaluationStack.Pop();
                                addReference(x.name);
                                src_code.Enqueue( "    " + x.name + " = " + x.name + ".substr(" + index + ", " + count + ");");
                                x.byteArray = SubArray<byte>(x.byteArray, index, count);
                                EvaluationStack.Push(x);
                            }
                            break;
                        case OpCode.LEFT:
                            {
                                int count = (int)EvaluationStack.Pop().integer;
                                if (count < 0)
                                {

                                    return;
                                }
                                StackItem x = EvaluationStack.Pop();
                                addReference(x.name);
                                src_code.Enqueue( "    " + x.name + " = " + x.name + ".substr(0, " + count + ");");
                                x.byteArray = SubArray<byte>(x.byteArray, 0, count);
                                EvaluationStack.Push(x);
                            }
                            break;
                        case OpCode.RIGHT:
                            {
                                //    int count = (int)EvaluationStack.Pop().integer;
                                //    if (count < 0)
                                //    {

                                //        return;
                                //    }
                                //    StackItem x = EvaluationStack.Pop();
                                //    if (x.byteArray.Length < count)
                                //    {

                                //        return;
                                //    }
                                //    src_code.Enqueue( "    " + x.name + "=" + x.name + ".substr(0, " + count + ");");
                                //    x.byteArray = SubArray<byte>(x.byteArray, 0, count);
                                //    EvaluationStack.Push(x);

                                //    EvaluationStack.Push(x.Skip(x.Length - count).ToArray());
                            }
                            break;
                        case OpCode.SIZE:
                            {
                                StackItem x = EvaluationStack.Pop();
                                temp = getVariable();
                                addReference(x.name);
                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.integer;
                                stackItem.integer = x.byteArray.Length;
                                EvaluationStack.Push(stackItem);
                                src_code.Enqueue( "    " + temp + " = " + x.name + ".Length;");

                                EvaluationStack.Push(stackItem);
                            }
                            break;

                        // Bitwise logic
                        case OpCode.INVERT:
                            {
                                StackItem x = EvaluationStack.Pop();
                                addReference(x.name);
                                x.integer = ~x.integer;
                                src_code.Enqueue( "    " + temp + " = ~" + x.name + ";");
                                EvaluationStack.Push(x);
                            }
                            break;
                        case OpCode.AND:
                            {
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.integer;
                                stackItem.integer = x2.integer & x1.integer;
                                EvaluationStack.Push(stackItem);
                                src_code.Enqueue( "    " + temp + " = " + x2.name + "&" + x1.name + ";");

                                EvaluationStack.Push(stackItem);
                            }
                            break;
                        case OpCode.OR:
                            {
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.integer;
                                stackItem.integer = x2.integer | x1.integer;
                                EvaluationStack.Push(stackItem);
                                src_code.Enqueue( "    " + temp + " = " + x2.name + " | " + x1.name + ";");

                                EvaluationStack.Push(stackItem);
                            }
                            break;
                        case OpCode.XOR:
                            {
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.integer;
                                stackItem.integer = x2.integer ^ x1.integer;
                                EvaluationStack.Push(stackItem);
                                src_code.Enqueue( "    " + temp + " = " + x2.name + " ^ " + x1.name + ";");

                                EvaluationStack.Push(stackItem);
                            }
                            break;
                        case OpCode.EQUAL:
                            {
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.boolen;
                                stackItem.flag = x2.integer == x1.integer;
                                EvaluationStack.Push(stackItem);
                                src_code.Enqueue( "    " + temp + " = " + x2.name + " == " + x1.name + ";");

                                EvaluationStack.Push(stackItem);
                            }
                            break;

                        // Numeric
                        case OpCode.INC:
                            {
                                StackItem x2 = EvaluationStack.Pop();
                                addReference(x2.name);
                                x2.integer = x2.integer + 1;
                                src_code.Enqueue( "    " + x2.name + " = " + x2.name + "++;");
                                EvaluationStack.Push(x2);
                            }
                            break;
                        case OpCode.DEC:
                            {
                                StackItem x2 = EvaluationStack.Pop();
                                addReference(x2.name);
                                x2.integer = x2.integer - 1;
                                src_code.Enqueue( "    " + x2.name + " = " + x2.name + "--;");
                                EvaluationStack.Push(x2);
                            }
                            break;
                        case OpCode.SIGN:
                            {
                                //BigInteger x = EvaluationStack.Pop().GetBigInteger();
                                //EvaluationStack.Push(x.Sign);
                            }
                            break;
                        case OpCode.NEGATE:
                            {
                                StackItem x2 = EvaluationStack.Pop();
                                addReference(x2.name);
                                x2.integer = -x2.integer;
                                src_code.Enqueue( "    " + x2.name + " = -" + x2.name + ";");
                                EvaluationStack.Push(x2);
                            }
                            break;
                        case OpCode.ABS:
                            {
                                StackItem x2 = EvaluationStack.Pop();
                                addReference(x2.name);
                                x2.integer = Math.Abs(x2.integer);
                                src_code.Enqueue( "    " + x2.name + " = Math.ABS(" + x2.name + ");");
                                EvaluationStack.Push(x2);
                            }
                            break;
                        case OpCode.NOT:
                            {
                                StackItem x2 = EvaluationStack.Pop();
                                addReference(x2.name);
                                x2.flag = !x2.flag;
                                src_code.Enqueue( "    " + x2.name + " = !" + x2.name + ";");
                                EvaluationStack.Push(x2);
                            }
                            break;
                        case OpCode.NZ:
                            {
                                StackItem x = EvaluationStack.Pop();
                                addReference(x.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.boolen;
                                stackItem.flag = x.integer != 0;

                                src_code.Enqueue( "    " + temp + " = " + x.name + " != 0" + ";");

                                EvaluationStack.Push(stackItem);
                            }
                            break;
                        case OpCode.ADD:
                            {
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.integer;
                                stackItem.integer = x2.integer + x1.integer;
                                EvaluationStack.Push(stackItem);
                                src_code.Enqueue( "    " + temp + " = " + x2.name + " + " + x1.name + ";");

                                EvaluationStack.Push(stackItem);
                            }
                            break;
                        case OpCode.SUB:
                            {
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.integer;
                                stackItem.integer = x1.integer - x2.integer;
                                EvaluationStack.Push(stackItem);
                                src_code.Enqueue( "    " + temp + " = " + x1.name + " - " + x2.name + ";");

                                EvaluationStack.Push(stackItem);

                            }
                            break;
                        case OpCode.MUL:
                            {
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.integer;
                                stackItem.integer = x1.integer * x2.integer;
                                EvaluationStack.Push(stackItem);
                                src_code.Enqueue( "    " + temp + " = " + x1.name + " * " + x2.name + ";");

                                EvaluationStack.Push(stackItem);
                            }
                            break;
                        case OpCode.DIV:
                            {
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.integer;
                                stackItem.integer = x1.integer / x2.integer;
                                EvaluationStack.Push(stackItem);
                                src_code.Enqueue( "    " + temp + "=" + x1.name + "/" + x2.name + ";");

                                EvaluationStack.Push(stackItem);
                            }
                            break;
                        case OpCode.MOD:
                            {
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.integer;
                                stackItem.integer = x1.integer % x2.integer;
                                EvaluationStack.Push(stackItem);
                                src_code.Enqueue( "    " + temp + " = " + x1.name + " % " + x2.name + ";");

                                EvaluationStack.Push(stackItem);
                            }
                            break;
                        case OpCode.SHL:
                            {
                                //int n = (int)EvaluationStack.Pop().GetBigInteger();
                                //BigInteger x = EvaluationStack.Pop().GetBigInteger();
                                //EvaluationStack.Push(x << n);

                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.integer;
                                stackItem.integer = x1.integer << x2.integer;
                                EvaluationStack.Push(stackItem);
                                src_code.Enqueue( "    " + temp + " = " + x1.name + " << " + x2.name + ";");

                                EvaluationStack.Push(stackItem);

                            }
                            break;
                        case OpCode.SHR:
                            {
                                //int n = (int)EvaluationStack.Pop().GetBigInteger();
                                //BigInteger x = EvaluationStack.Pop().GetBigInteger();
                                //EvaluationStack.Push(x >> n);
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.integer;
                                stackItem.integer = x1.integer >> x2.integer;
                                EvaluationStack.Push(stackItem);
                                src_code.Enqueue( "    " + temp + " = " + x1.name + " >> " + x2.name + ";");

                                EvaluationStack.Push(stackItem);
                            }
                            break;
                        case OpCode.BOOLAND:
                            {
                                //bool x2 = EvaluationStack.Pop().GetBoolean();
                                //bool x1 = EvaluationStack.Pop().GetBoolean();
                                //EvaluationStack.Push(x1 && x2);
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.boolen;
                                stackItem.flag = x1.flag && x2.flag;
                                EvaluationStack.Push(stackItem);

                                src_code.Enqueue( "    " + temp + " = " + x1.name + " && " + x2.name + ";");
                            }
                            break;
                        case OpCode.BOOLOR:
                            {
                                //bool x2 = EvaluationStack.Pop().GetBoolean();
                                //bool x1 = EvaluationStack.Pop().GetBoolean();
                                //EvaluationStack.Push(x1 || x2);
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.boolen;
                                stackItem.flag = x1.flag || x2.flag;
                                EvaluationStack.Push(stackItem);

                                src_code.Enqueue( "    " + temp + " = " + x1.name + " || " + x2.name + ";");
                            }
                            break;
                        case OpCode.NUMEQUAL:
                            {
                                //BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                //BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                //EvaluationStack.Push(x1 == x2);

                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.boolen;
                                stackItem.flag = x1.integer == x2.integer;
                                EvaluationStack.Push(stackItem);
                                src_code.Enqueue( "    " + temp + " = " + x1.name + " == " + x2.name + ";");

                                EvaluationStack.Push(stackItem);
                            }
                            break;
                        case OpCode.NUMNOTEQUAL:
                            {
                                //BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                //BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                //EvaluationStack.Push(x1 != x2);
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.boolen;
                                stackItem.flag = x1.integer != x2.integer;
                                EvaluationStack.Push(stackItem);
                                src_code.Enqueue( "    " + temp + " = " + x1.name + " != " + x2.name + ";");

                                EvaluationStack.Push(stackItem);
                            }
                            break;
                        case OpCode.LT:
                            {
                                //BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                //BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                //EvaluationStack.Push(x1 < x2);
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.boolen;
                                stackItem.flag = x1.integer < x2.integer;
                                EvaluationStack.Push(stackItem);
                                src_code.Enqueue( "    " + temp + " = " + x1.name + " < " + x2.name + ";");

                                EvaluationStack.Push(stackItem);
                            }
                            break;
                        case OpCode.GT:
                            {
                                //BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                //BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                //EvaluationStack.Push(x1 > x2);

                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.boolen;
                                stackItem.flag = x1.integer > x2.integer;
                                EvaluationStack.Push(stackItem);
                                src_code.Enqueue( "    " + temp + " = " + x1.name + " > " + x2.name + ";");

                                EvaluationStack.Push(stackItem);
                            }
                            break;
                        case OpCode.LTE:
                            {
                                //BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                //BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                //EvaluationStack.Push(x1 <= x2);
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.boolen;
                                stackItem.flag = x1.integer <= x2.integer;
                                EvaluationStack.Push(stackItem);
                                src_code.Enqueue( "    " + temp + " = " + x1.name + " <= " + x2.name + ";");

                                EvaluationStack.Push(stackItem);
                            }
                            break;
                        case OpCode.GTE:
                            {
                                //BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                //BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                //EvaluationStack.Push(x1 >= x2);
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.boolen;
                                stackItem.flag = x1.integer >= x2.integer;
                                EvaluationStack.Push(stackItem);
                                src_code.Enqueue( "    " + temp + " = " + x1.name + " >= " + x2.name + ";");

                                EvaluationStack.Push(stackItem);
                            }
                            break;
                        case OpCode.MIN:
                            {
                                //BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                //BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                //EvaluationStack.Push(BigInteger.Min(x1, x2));
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.integer;
                                stackItem.integer = Math.Min(x1.integer, x2.integer);
                                EvaluationStack.Push(stackItem);
                                src_code.Enqueue( "    " + temp + "= Math.Min(" + x1.name + "," + x2.name + ");");

                                EvaluationStack.Push(stackItem);
                            }
                            break;
                        case OpCode.MAX:
                            {
                                //BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                //BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                //EvaluationStack.Push(BigInteger.Max(x1, x2));

                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.integer;
                                stackItem.integer = Math.Max(x1.integer, x2.integer);
                                EvaluationStack.Push(stackItem);
                                src_code.Enqueue( "    " + temp + "= Math.Max(" + x1.name + "," + x2.name + ");");

                                EvaluationStack.Push(stackItem);
                            }
                            break;
                        case OpCode.WITHIN:
                            {
                                //BigInteger b = EvaluationStack.Pop().GetBigInteger();
                                //BigInteger a = EvaluationStack.Pop().GetBigInteger();
                                //BigInteger x = EvaluationStack.Pop().GetBigInteger();
                                //EvaluationStack.Push(a <= x && x < b);
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                StackItem x = EvaluationStack.Pop();
                                addReference(x2.name);
                                addReference(x1.name);
                                addReference(x.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.boolen;
                                stackItem.flag = (x1.integer <= x.integer && x.integer < x2.integer);
                                EvaluationStack.Push(stackItem);

                                src_code.Enqueue( "    " + temp + " = (" + x1.name + " <= " + x.name + ") && (" + x.name + " < " + x2.name + ");");

                                EvaluationStack.Push(stackItem);
                            }
                            break;

                        // Crypto
                        case OpCode.SHA1:
                            using (SHA1 sha = SHA1.Create())
                            {
                                StackItem x = EvaluationStack.Pop();
                                addReference(x.name);
                                x.byteArray = sha.ComputeHash(x.byteArray);
                                src_code.Enqueue( "    " + x.name + " = Crypto.SHA256(" + x.name + ");");
                                EvaluationStack.Push(x);
                            }

                            break;
                        case OpCode.SHA256:
                            using (SHA256 sha = SHA256.Create())
                            {
                                StackItem x = EvaluationStack.Pop();
                                addReference(x.name);
                                x.byteArray = sha.ComputeHash(x.byteArray);
                                src_code.Enqueue( "    " + x.name + " = Crypto.SHA256(" + x.name + ");");
                                EvaluationStack.Push(x);
                            }
                            break;
                        case OpCode.HASH160:
                            {
                                //byte[] x = EvaluationStack.Pop().GetByteArray();
                                //EvaluationStack.Push(Crypto.Hash160(x));
                                StackItem x = EvaluationStack.Pop();
                                addReference(x.name);
                                x.byteArray = Crypto.Hash160(x.byteArray);
                                src_code.Enqueue( "    " + x.name + " = Crypto.Hash160(" + x.name + ");");
                                EvaluationStack.Push(x);
                            }
                            break;
                        case OpCode.HASH256:
                            {
                                //byte[] x = EvaluationStack.Pop().GetByteArray();
                                //EvaluationStack.Push(Crypto.Hash256(x));
                                StackItem x = EvaluationStack.Pop();
                                addReference(x.name);
                                x.byteArray = Crypto.Hash256(x.byteArray);
                                src_code.Enqueue( "    " + x.name + " = Crypto.Hash256(" + x.name + ");");
                                EvaluationStack.Push(x);
                            }
                            break;
                        case OpCode.CHECKSIG:
                            {
                                //byte[] pubkey = EvaluationStack.Pop().GetByteArray();
                                //byte[] signature = EvaluationStack.Pop().GetByteArray();
                                //EvaluationStack.Push(Crypto.VerifySignature(ScriptContainer.GetMessage(), signature, pubkey));
                                StackItem pubkey = EvaluationStack.Pop();
                                StackItem signature = EvaluationStack.Pop();
                                // TODO:: VerifySignature
                                temp = "isSignature";

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.boolen;
                                stackItem.flag = false;
                                EvaluationStack.Push(stackItem);

                                src_code.Enqueue(temp +" = Crypto.VerifySignature(ScriptContainer.GetMessage(), "+signature.name+", "+pubkey.name+")");

                            }
                            break;
                        case OpCode.VERIFY:
                            {
                                //byte[] pubkey = EvaluationStack.Pop().GetByteArray();
                                //byte[] signature = EvaluationStack.Pop().GetByteArray();
                                //byte[] message = EvaluationStack.Pop().GetByteArray();
                                //try
                                //{
                                //    EvaluationStack.Push(Crypto.VerifySignature(message, signature, pubkey));
                                //}
                                //catch (ArgumentException)
                                //{
                                //    EvaluationStack.Push(false);
                                //}
                            }
                            break;
                        case OpCode.CHECKMULTISIG:
                            {
                            }
                            break;

                        // Array
                        case OpCode.ARRAYSIZE:
                            {
                                StackItem item = EvaluationStack.Pop();
                                addReference(item.name);
                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.integer;
                                stackItem.integer = item.byteArray.Length;
                                EvaluationStack.Push(stackItem);

                                src_code.Enqueue( "    " + temp + " = " + item.name + ".Length;");
                                EvaluationStack.Push(stackItem);
                            }
                            break;
                        case OpCode.PACK:
                            {
                                StackItem size = EvaluationStack.Pop();
                                addReference(size.name);
                                List<StackItem> items = new List<StackItem>(size.integer);

                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.array;
                                stackItem.arr = items;
                                EvaluationStack.Push(stackItem);

                                src_code.Enqueue( "     List<?> " + temp + "= new List<?>(" + size.name + ");");
                                StackItem it;
                                for (int i = 0; i < size.integer; i++)
                                {
                                    it = EvaluationStack.Pop();
                                    items.Add(it);
                                    src_code.Enqueue( "    " + temp + ".Add(" + it.name + ");");
                                }
                                EvaluationStack.Push(stackItem);
                            }
                            break;
                        case OpCode.UNPACK:
                            {
                                //StackItem item = EvaluationStack.Pop();
                                //if (item is VMArray array)
                                //{
                                //    for (int i = array.Count - 1; i >= 0; i--)
                                //        EvaluationStack.Push(array[i]);
                                //    EvaluationStack.Push(array.Count);
                                //}

                            }
                            break;
                        case OpCode.PICKITEM:
                            {
                                StackItem key = EvaluationStack.Pop();
                                StackItem x = EvaluationStack.Pop();

                                addReference(key.name);
                                addReference(x.name);

                                StackItem v = x.arr[key.integer];
                                src_code.Enqueue( "    " + v.name + " = " + x.name + "[" + key.name + "];");

                                EvaluationStack.Push(v);
                            }
                            break;
                        case OpCode.SETITEM:
                            {
                                StackItem value = EvaluationStack.Pop();
                                //          if (value is Struct s) value = s.Clone();
                                StackItem key = EvaluationStack.Pop();
                                StackItem x = EvaluationStack.Pop();

                                addReference(value.name);
                                addReference(x.name);
                                addReference(key.name);

                                if (x.type == Type.array)
                                {
                                    int index = (int)key.integer;

                                    x.arr[index] = value;
                                    src_code.Enqueue( "    " + x.name + "[" + key.name + "] = " + value.name + ";");
                                }
                                else
                                {
                                    //x.map[key] = value;
                                }
                                EvaluationStack.Push(x);
                            }
                            break;
                        case OpCode.NEWARRAY:
                            {
                                StackItem count = EvaluationStack.Pop();

                                addReference(count.name);

                                List<StackItem> items = new List<StackItem>(count.integer);
                                var temp1 = getVariable();
                                StackItem stackItem1 = new StackItem();
                                stackItem1.name = temp1;
                                stackItem1.type = Type.array;
                                src_code.Enqueue( "    Array " + temp1 + " = new Array<?>(" + count.name + ");");

                                for (var i = 0; i < count.integer; i++)
                                {
                                    temp = getVariable();

                                    stackItem = new StackItem();
                                    stackItem.name = temp;
                                    stackItem.type = Type.boolen;
                                    stackItem.flag = false;
                                    items.Add(stackItem);
                                }
                                stackItem1.arr = items;
                                EvaluationStack.Push(stackItem1);
                            }
                            break;
                        case OpCode.NEWSTRUCT:
                            {
                                //int count = (int)EvaluationStack.Pop().GetBigInteger();
                                //List<StackItem> items = new List<StackItem>(count);
                                //for (var i = 0; i < count; i++)
                                //{
                                //    items.Add(false);
                                //}
                                //EvaluationStack.Push(new VM.Types.Struct(items));
                            }
                            break;
                        case OpCode.NEWMAP:
                            //EvaluationStack.Push(new Map());
                            break;
                        case OpCode.APPEND:
                            {
                                StackItem newItem = EvaluationStack.Pop();
                                StackItem arrItem = EvaluationStack.Pop();
                                addReference(newItem.name);
                                addReference(arrItem.name);

                                arrItem.arr.Add(newItem);

                                src_code.Enqueue( "    " + arrItem.name + ".Add(" + newItem.name + ");");

                                EvaluationStack.Push(arrItem);
                            }
                            break;
                        case OpCode.REVERSE:
                            {
                                StackItem arrItem = EvaluationStack.Pop();

                                addReference(arrItem.name);
                                
                                arrItem.arr.Reverse();
                                src_code.Enqueue( "    " + arrItem.name + ".Reverse();");
                                EvaluationStack.Push(arrItem);
                            }
                            break;
                        case OpCode.REMOVE:
                            {
                                StackItem key = EvaluationStack.Pop();
                                //if (key is ICollection)
                                //{
                                //    State |= VMState.FAULT;
                                //    return;
                                //}
                                //switch (EvaluationStack.Pop())
                                //{
                                //    case VMArray array:
                                //        int index = (int)key.GetBigInteger();
                                //        if (index < 0 || index >= array.Count)
                                //        {
                                //            State |= VMState.FAULT;
                                //            return;
                                //        }
                                //        array.RemoveAt(index);
                                //        break;
                                //    case Map map:
                                //        map.Remove(key);
                                //        break;
                                //    default:
                                //        State |= VMState.FAULT;
                                //        return;
                                //}
                            }
                            break;
                        case OpCode.HASKEY:
                            {
                                StackItem key = EvaluationStack.Pop();
                                StackItem x = EvaluationStack.Pop();

                                addReference(key.name);
                                addReference(x.name);

                                temp = getVariable();

                                stackItem = new StackItem();
                                stackItem.name = temp;
                                stackItem.type = Type.boolen;

                                if (x.type == Type.array)
                                {
                                    int index = (int)key.integer;

                                    stackItem.flag = index < x.arr.Count;
                                }
                                else
                                {
                                    stackItem.flag = x.arr.Contains(key);
                                }

                                src_code.Enqueue( "    " + temp + x.name + ".Contains(" + key.name + ");");
                                EvaluationStack.Push(stackItem);

                            }
                            break;
                        case OpCode.KEYS:
                            //switch (EvaluationStack.Pop())
                            //{
                            //    case Map map:
                            //        EvaluationStack.Push(new VMArray(map.Keys));
                            //        break;
                            //    default:
                            //        State |= VMState.FAULT;
                            //        return;
                            //}
                            break;
                        case OpCode.VALUES:
                            {
                                ICollection<StackItem> values;
                                //switch (EvaluationStack.Pop())
                                //{
                                //    case VMArray array:
                                //        values = array;
                                //        break;
                                //    case Map map:
                                //        values = map.Values;
                                //        break;
                                //    default:
                                //        return;
                                //}
                                //List<StackItem> newArray = new List<StackItem>(values.Count);
                                ////foreach (StackItem item in values)
                                ////    if (item is Struct s)
                                ////        newArray.Add(s.Clone());
                                ////    else
                                ////        newArray.Add(item);
                                //EvaluationStack.Push(new VMArray(newArray));
                            }
                            break;

                        // Exceptions
                        case OpCode.THROW:
                            return;
                        case OpCode.THROWIFNOT:
                            if (!EvaluationStack.Pop().flag)
                            {
                                return;
                            }
                            break;

                        default:
                            return;
                    }
            }
            src_code.Enqueue( "}");

            // REMOVE THOSE VARIABLES THAT ONLY USED ONCE
            foreach( string number in src_code )
            {
                bool print = true;
                foreach(var item in variables)
                {
                    if(item.Value ==1 && number.Contains(item.Key)){
                        print = false;
                    }
                }
                if(print)
                    Console.WriteLine(number);
            }
            //Console.WriteLine(src);
        }

    }
}
