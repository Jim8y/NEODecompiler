using System;
using System.Collections.Generic;
using System.Text;
using VMArray = NEODisassembler.Array;
namespace NEODisassembler
{

        public class ExecutionStackRecord : RandomAccessStack<StackItem>
        {
            public enum OpType
            {
                Non,
                Clear,
                Insert,
                Peek,
                Pop,
                Push,
                Remove,
                Set,
            }
            public struct Op
            {
                public Op(OpType type, int ind = -1)
                {
                    this.type = type;
                    this.ind = ind;
                }
                public OpType type;
                public int ind;
            }

            public List<Op> record = new List<Op>();
            public void ClearRecord()
            {
                record.Clear();
            }
            public OpType GetLastRecordType()
            {
                if (record.Count == 0)
                    return OpType.Non;
                else
                    return record.Last().type;
            }
            public new void Clear()
            {
                record.Add(new Op(OpType.Clear));
                base.Clear();
            }
            public new void Insert(int index, StackItem item)
            {
                record.Add(new Op(OpType.Insert, index));
                base.Insert(index, item);
            }
            public new StackItem Peek(int index = 0)
            {
                record.Add(new Op(OpType.Peek, index));
                return base.Peek(index);
            }
            public StackItem PeekWithoutLog(int index = 0)
            {
                return base.Peek(index);
            }
            public new StackItem Pop()
            {
                record.Add(new Op(OpType.Pop));
                return base.Remove(0);
            }

            public new void Push(StackItem item)
            {
                record.Add(new Op(OpType.Push));
                base.Push(item);
            }

            public new StackItem Remove(int index)
            {
                if (index == 0)
                    return Pop();

                record.Add(new Op(OpType.Remove, index));
                return base.Remove(index);
            }

            public new void Set(int index, StackItem item)
            {
                record.Add(new Op(OpType.Set, index));
                base.Set(index, item);
            }
        }


        public class ExecutionEngine
        {
            public RandomAccessStack<ExecutionContext> InvocationStack { get; } = new RandomAccessStack<ExecutionContext>();
            public ExecutionStackRecord EvaluationStack = new ExecutionStackRecord();
            public RandomAccessStack<StackItem> AltStack { get; } = new RandomAccessStack<StackItem>();
            public ExecutionContext CurrentContext => InvocationStack.Peek();
            public ExecutionContext CallingContext => InvocationStack.Count > 1 ? InvocationStack.Peek(1) : null;
            public ExecutionContext EntryContext => InvocationStack.Peek(InvocationStack.Count - 1);

            public ExecutionEngine()
            {

            }


        private void ExecuteOp(NeoMethod method, ExecutionContext context)
        {
            // TODO: parameter of function
            string src = "function " + method.name + "() {";

            NeoCode param_code = method.neoCodes[0];

            //// Get the number of temp variable inside of the funtion
            //if(param_code.code != OpCode.PUSH0)
            //{
            //    var num_of_param = param_code.code - 0x50; // PHSH1 =>0x51
            //    // Create an array to store those variables and put the array into the altstack

            //}
            int addr = 0;
            int variable_count = 0;
            while (addr <= method.neoCodes.Count)
            {
                NeoCode opcode = method.neoCodes[addr];

                if (opcode.code >= OpCode.PUSHBYTES1 && opcode.code <= OpCode.PUSHBYTES75)
                    EvaluationStack.Push(context.OpReader.ReadBytes((byte)opcode.code));
                else
                    switch (opcode.code)
                    {
                        // Push value
                        case OpCode.PUSH0:

                            EvaluationStack.Push(new byte[0]);
                            break;
                        case OpCode.PUSHDATA1:
                            EvaluationStack.Push(context.OpReader.ReadBytes(context.OpReader.ReadByte()));
                            break;
                        case OpCode.PUSHDATA2:
                            EvaluationStack.Push(context.OpReader.ReadBytes(context.OpReader.ReadUInt16()));
                            break;
                        case OpCode.PUSHDATA4:
                            EvaluationStack.Push(context.OpReader.ReadBytes(context.OpReader.ReadInt32()));
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
                            EvaluationStack.Push((int)opcode - (int)OpCode.PUSH1 + 1);
                            break;

                        // Control
                        case OpCode.NOP:
                            break;
                        case OpCode.JMP:
                        case OpCode.JMPIF:
                        case OpCode.JMPIFNOT:
                            {
                                int offset = context.OpReader.ReadInt16();
                                offset = context.InstructionPointer + offset - 3;
                                if (offset < 0 || offset > context.Script.Length)
                                {
                                    return;
                                }
                                bool fValue = true;
                                if (opcode > OpCode.JMP)
                                {
                                    fValue = EvaluationStack.Pop().GetBoolean();
                                    if (opcode == OpCode.JMPIFNOT)
                                        fValue = !fValue;
                                }
                                if (fValue)
                                    context.InstructionPointer = offset;
                            }
                            break;
                        case OpCode.CALL:
                            InvocationStack.Push(context.Clone());
                            context.InstructionPointer += 2;
                            ExecuteOp(OpCode.JMP, CurrentContext);
                            break;
                        case OpCode.RET:

                            break;
                        case OpCode.APPCALL:
                        case OpCode.TAILCALL:
                            {

                            }
                            break;
                        case OpCode.SYSCALL:
                            {

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
                                int n = (int)EvaluationStack.Pop().GetBigInteger();
                                if (n < 0)
                                {
                                    return;
                                }
                                EvaluationStack.Remove(n);
                            }
                            break;
                        case OpCode.XSWAP:
                            {
                                int n = (int)EvaluationStack.Pop().GetBigInteger();
                                if (n < 0)
                                {

                                    return;
                                }
                                if (n == 0) break;
                                StackItem xn = EvaluationStack.Peek(n);
                                EvaluationStack.Set(n, EvaluationStack.Peek());
                                EvaluationStack.Set(0, xn);
                            }
                            break;
                        case OpCode.XTUCK:
                            {
                                int n = (int)EvaluationStack.Pop().GetBigInteger();
                                if (n <= 0)
                                {

                                    return;
                                }
                                EvaluationStack.Insert(n, EvaluationStack.Peek());
                            }
                            break;
                        case OpCode.DEPTH:
                            EvaluationStack.Push(EvaluationStack.Count);
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
                                int n = (int)EvaluationStack.Pop().GetBigInteger();
                                if (n < 0)
                                {

                                    return;
                                }
                                EvaluationStack.Push(EvaluationStack.Peek(n));
                            }
                            break;
                        case OpCode.ROLL:
                            {
                                int n = (int)EvaluationStack.Pop().GetBigInteger();
                                if (n < 0)
                                {

                                    return;
                                }
                                if (n == 0) break;
                                EvaluationStack.Push(EvaluationStack.Remove(n));
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
                                //StackItem x2 = EvaluationStack.Pop();
                                //StackItem x1 = EvaluationStack.Pop();
                                //EvaluationStack.Push(x2);
                                //EvaluationStack.Push(x1);
                                //EvaluationStack.Push(x2);
                            }
                            break;
                        case OpCode.CAT:
                            {
                                //byte[] x2 = EvaluationStack.Pop().GetByteArray();
                                //byte[] x1 = EvaluationStack.Pop().GetByteArray();
                                //EvaluationStack.Push(x1.Concat(x2).ToArray());
                            }
                            break;
                        case OpCode.SUBSTR:
                            {
                                //int count = (int)EvaluationStack.Pop().GetBigInteger();
                                //if (count < 0)
                                //{

                                //    return;
                                //}
                                //int index = (int)EvaluationStack.Pop().GetBigInteger();
                                //if (index < 0)
                                //{

                                //    return;
                                //}
                                //byte[] x = EvaluationStack.Pop().GetByteArray();
                                //EvaluationStack.Push(x.Skip(index).Take(count).ToArray());
                            }
                            break;
                        case OpCode.LEFT:
                            {
                                int count = (int)EvaluationStack.Pop().GetBigInteger();
                                if (count < 0)
                                {

                                    return;
                                }
                                byte[] x = EvaluationStack.Pop().GetByteArray();
                                EvaluationStack.Push(x.Take(count).ToArray());
                            }
                            break;
                        case OpCode.RIGHT:
                            {
                                int count = (int)EvaluationStack.Pop().GetBigInteger();
                                if (count < 0)
                                {

                                    return;
                                }
                                byte[] x = EvaluationStack.Pop().GetByteArray();
                                if (x.Length < count)
                                {

                                    return;
                                }
                                EvaluationStack.Push(x.Skip(x.Length - count).ToArray());
                            }
                            break;
                        case OpCode.SIZE:
                            {
                                byte[] x = EvaluationStack.Pop().GetByteArray();
                                EvaluationStack.Push(x.Length);
                            }
                            break;

                        // Bitwise logic
                        case OpCode.INVERT:
                            {
                                BigInteger x = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(~x);
                            }
                            break;
                        case OpCode.AND:
                            {
                                BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(x1 & x2);
                            }
                            break;
                        case OpCode.OR:
                            {
                                BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(x1 | x2);
                            }
                            break;
                        case OpCode.XOR:
                            {
                                BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(x1 ^ x2);
                            }
                            break;
                        case OpCode.EQUAL:
                            {
                                StackItem x2 = EvaluationStack.Pop();
                                StackItem x1 = EvaluationStack.Pop();
                                EvaluationStack.Push(x1.Equals(x2));
                            }
                            break;

                        // Numeric
                        case OpCode.INC:
                            {
                                BigInteger x = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(x + 1);
                            }
                            break;
                        case OpCode.DEC:
                            {
                                BigInteger x = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(x - 1);
                            }
                            break;
                        case OpCode.SIGN:
                            {
                                BigInteger x = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(x.Sign);
                            }
                            break;
                        case OpCode.NEGATE:
                            {
                                BigInteger x = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(-x);
                            }
                            break;
                        case OpCode.ABS:
                            {
                                BigInteger x = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(BigInteger.Abs(x));
                            }
                            break;
                        case OpCode.NOT:
                            {
                                bool x = EvaluationStack.Pop().GetBoolean();
                                EvaluationStack.Push(!x);
                            }
                            break;
                        case OpCode.NZ:
                            {
                                BigInteger x = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(x != BigInteger.Zero);
                            }
                            break;
                        case OpCode.ADD:
                            {
                                BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(x1 + x2);
                            }
                            break;
                        case OpCode.SUB:
                            {
                                BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(x1 - x2);
                            }
                            break;
                        case OpCode.MUL:
                            {
                                BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(x1 * x2);
                            }
                            break;
                        case OpCode.DIV:
                            {
                                BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(x1 / x2);
                            }
                            break;
                        case OpCode.MOD:
                            {
                                BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(x1 % x2);
                            }
                            break;
                        case OpCode.SHL:
                            {
                                int n = (int)EvaluationStack.Pop().GetBigInteger();
                                BigInteger x = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(x << n);
                            }
                            break;
                        case OpCode.SHR:
                            {
                                int n = (int)EvaluationStack.Pop().GetBigInteger();
                                BigInteger x = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(x >> n);
                            }
                            break;
                        case OpCode.BOOLAND:
                            {
                                bool x2 = EvaluationStack.Pop().GetBoolean();
                                bool x1 = EvaluationStack.Pop().GetBoolean();
                                EvaluationStack.Push(x1 && x2);
                            }
                            break;
                        case OpCode.BOOLOR:
                            {
                                bool x2 = EvaluationStack.Pop().GetBoolean();
                                bool x1 = EvaluationStack.Pop().GetBoolean();
                                EvaluationStack.Push(x1 || x2);
                            }
                            break;
                        case OpCode.NUMEQUAL:
                            {
                                BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(x1 == x2);
                            }
                            break;
                        case OpCode.NUMNOTEQUAL:
                            {
                                BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(x1 != x2);
                            }
                            break;
                        case OpCode.LT:
                            {
                                BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(x1 < x2);
                            }
                            break;
                        case OpCode.GT:
                            {
                                BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(x1 > x2);
                            }
                            break;
                        case OpCode.LTE:
                            {
                                BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(x1 <= x2);
                            }
                            break;
                        case OpCode.GTE:
                            {
                                BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(x1 >= x2);
                            }
                            break;
                        case OpCode.MIN:
                            {
                                BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(BigInteger.Min(x1, x2));
                            }
                            break;
                        case OpCode.MAX:
                            {

                                BigInteger x2 = EvaluationStack.Pop().GetBigInteger();
                                BigInteger x1 = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(BigInteger.Max(x1, x2));
                            }
                            break;
                        case OpCode.WITHIN:
                            {
                                BigInteger b = EvaluationStack.Pop().GetBigInteger();
                                BigInteger a = EvaluationStack.Pop().GetBigInteger();
                                BigInteger x = EvaluationStack.Pop().GetBigInteger();
                                EvaluationStack.Push(a <= x && x < b);
                            }
                            break;

                        // Crypto
                        case OpCode.SHA1:
                            //using (SHA1 sha = SHA1.Create())
                            //{
                            //    byte[] x = EvaluationStack.Pop().GetByteArray();
                            //    EvaluationStack.Push(sha.ComputeHash(x));
                            //}
                            break;
                        case OpCode.SHA256:
                            //using (SHA256 sha = SHA256.Create())
                            //{
                            //    byte[] x = EvaluationStack.Pop().GetByteArray();
                            //    EvaluationStack.Push(sha.ComputeHash(x));
                            //}
                            break;
                        case OpCode.HASH160:
                            {
                                //byte[] x = EvaluationStack.Pop().GetByteArray();
                                //EvaluationStack.Push(Crypto.Hash160(x));
                            }
                            break;
                        case OpCode.HASH256:
                            {
                                //byte[] x = EvaluationStack.Pop().GetByteArray();
                                //EvaluationStack.Push(Crypto.Hash256(x));
                            }
                            break;
                        case OpCode.CHECKSIG:
                            {
                                //byte[] pubkey = EvaluationStack.Pop().GetByteArray();
                                //byte[] signature = EvaluationStack.Pop().GetByteArray();
                                //try
                                //{
                                //    EvaluationStack.Push(Crypto.VerifySignature(ScriptContainer.GetMessage(), signature, pubkey));
                                //}
                                //catch (ArgumentException)
                                //{
                                //    EvaluationStack.Push(false);
                                //}
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
                                if (item is ICollection collection)
                                    EvaluationStack.Push(collection.Count);
                                else
                                    EvaluationStack.Push(item.GetByteArray().Length);
                            }
                            break;
                        case OpCode.PACK:
                            {
                                int size = (int)EvaluationStack.Pop().GetBigInteger();

                                List<StackItem> items = new List<StackItem>(size);
                                for (int i = 0; i < size; i++)
                                    items.Add(EvaluationStack.Pop());
                                EvaluationStack.Push(items);
                            }
                            break;
                        case OpCode.UNPACK:
                            {
                                StackItem item = EvaluationStack.Pop();
                                if (item is VMArray array)
                                {
                                    for (int i = array.Count - 1; i >= 0; i--)
                                        EvaluationStack.Push(array[i]);
                                    EvaluationStack.Push(array.Count);
                                }

                            }
                            break;
                        case OpCode.PICKITEM:
                            {
                                StackItem key = EvaluationStack.Pop();

                                switch (EvaluationStack.Pop())
                                {
                                    case VMArray array:
                                        int index = (int)key.GetBigInteger();

                                        EvaluationStack.Push(array[index]);
                                        break;
                                    case Map map:
                                        if (map.TryGetValue(key, out StackItem value))
                                        {
                                            EvaluationStack.Push(value);
                                        }

                                        break;
                                    default:
                                        return;
                                }
                            }
                            break;
                        case OpCode.SETITEM:
                            {
                                StackItem value = EvaluationStack.Pop();
                                if (value is Struct s) value = s.Clone();
                                StackItem key = EvaluationStack.Pop();

                                switch (EvaluationStack.Pop())
                                {
                                    case VMArray array:
                                        int index = (int)key.GetBigInteger();

                                        array[index] = value;
                                        break;
                                    case Map map:
                                        map[key] = value;
                                        break;
                                    default:
                                        return;
                                }
                            }
                            break;
                        case OpCode.NEWARRAY:
                            {
                                int count = (int)EvaluationStack.Pop().GetBigInteger();
                                List<StackItem> items = new List<StackItem>(count);
                                for (var i = 0; i < count; i++)
                                {
                                    items.Add(false);
                                }
                                EvaluationStack.Push(new Array(items));
                            }
                            break;
                        case OpCode.NEWSTRUCT:
                            {
                                int count = (int)EvaluationStack.Pop().GetBigInteger();
                                List<StackItem> items = new List<StackItem>(count);
                                for (var i = 0; i < count; i++)
                                {
                                    items.Add(false);
                                }
                                EvaluationStack.Push(new VM.Types.Struct(items));
                            }
                            break;
                        case OpCode.NEWMAP:
                            EvaluationStack.Push(new Map());
                            break;
                        case OpCode.APPEND:
                            {
                                StackItem newItem = EvaluationStack.Pop();
                                if (newItem is Types.Struct s)
                                {
                                    newItem = s.Clone();
                                }
                                StackItem arrItem = EvaluationStack.Pop();
                                if (arrItem is VMArray array)
                                {
                                    array.Add(newItem);
                                }
                                else
                                {
                                    State |= VMState.FAULT;
                                    return;
                                }
                            }
                            break;
                        case OpCode.REVERSE:
                            {
                                StackItem arrItem = EvaluationStack.Pop();
                                if (arrItem is VMArray array)
                                {
                                    array.Reverse();
                                }
                                else
                                {
                                    State |= VMState.FAULT;
                                    return;
                                }
                            }
                            break;
                        case OpCode.REMOVE:
                            {
                                StackItem key = EvaluationStack.Pop();
                                if (key is ICollection)
                                {
                                    State |= VMState.FAULT;
                                    return;
                                }
                                switch (EvaluationStack.Pop())
                                {
                                    case VMArray array:
                                        int index = (int)key.GetBigInteger();
                                        if (index < 0 || index >= array.Count)
                                        {
                                            State |= VMState.FAULT;
                                            return;
                                        }
                                        array.RemoveAt(index);
                                        break;
                                    case Map map:
                                        map.Remove(key);
                                        break;
                                    default:
                                        State |= VMState.FAULT;
                                        return;
                                }
                            }
                            break;
                        case OpCode.HASKEY:
                            {
                                StackItem key = EvaluationStack.Pop();
                                if (key is ICollection)
                                {
                                    State |= VMState.FAULT;
                                    return;
                                }
                                switch (EvaluationStack.Pop())
                                {
                                    case VMArray array:
                                        int index = (int)key.GetBigInteger();
                                        if (index < 0)
                                        {
                                            State |= VMState.FAULT;
                                            return;
                                        }
                                        EvaluationStack.Push(index < array.Count);
                                        break;
                                    case Map map:
                                        EvaluationStack.Push(map.ContainsKey(key));
                                        break;
                                    default:
                                        State |= VMState.FAULT;
                                        return;
                                }
                            }
                            break;
                        case OpCode.KEYS:
                            switch (EvaluationStack.Pop())
                            {
                                case Map map:
                                    EvaluationStack.Push(new VMArray(map.Keys));
                                    break;
                                default:
                                    State |= VMState.FAULT;
                                    return;
                            }
                            break;
                        case OpCode.VALUES:
                            {
                                ICollection<StackItem> values;
                                switch (EvaluationStack.Pop())
                                {
                                    case VMArray array:
                                        values = array;
                                        break;
                                    case Map map:
                                        values = map.Values;
                                        break;
                                    default:
                                        return;
                                }
                                List<StackItem> newArray = new List<StackItem>(values.Count);
                                foreach (StackItem item in values)
                                    if (item is Struct s)
                                        newArray.Add(s.Clone());
                                    else
                                        newArray.Add(item);
                                EvaluationStack.Push(new VMArray(newArray));
                            }
                            break;

                        // Exceptions
                        case OpCode.THROW:
                            return;
                        case OpCode.THROWIFNOT:
                            if (!EvaluationStack.Pop().GetBoolean())
                            {
                                return;
                            }
                            break;

                        default:
                            return;
                    }
            }
        }
               
        }
}
