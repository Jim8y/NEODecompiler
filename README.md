# NEODecompiler
This is a NEO smart contract decompiler, still under developing.

本项目尚处于开发的初级阶段，目前基本可以完成对整个项目的字节码进行反编译，然而，由于开发时间尚短，项目中依然存在很多还没完成的地方。 尤其是还有几个指令的
解析由于存在理解问题，还没有对其进行解析，因此如果合约代码中使用到了这些指令，反编译将会失败。

#如何使用

在Program.cs中可以指定avm的文件名，然后运行程序，程序就会首先输出反编译代码，然后输出指令码。

例如对于如下合约：
```
        public static void Main()
        {
            int a = 2;
            string aa="hello";
            string bb = "world";
            string cc = aa + bb;
           // uint b = Blockchain.GetHeight();
        }
```

反编译如下：
```
function Main() {
    int v_0 = 4;
    Array<?> v_1 = new Array<?>(v_0);
    int v_6 = 2;
    int v_7 = 0;
    v_1[v_7] = v_6;
    byte[] v_9= new byte[]("hello");
    int v_10 = 1;
    v_1[v_10] = v_9;
    byte[] v_12= new byte[]("world");
    int v_13 = 2;
    v_1[v_13] = v_12;
    int v_15 = 1;
    v_9 = v_1[v_15];
    int v_16 = 2;
    v_12 = v_1[v_16];
    byte[] v_17= v_9+v_12
    int v_18 = 3;
    v_1[v_18] = v_17;
}

00:PUSH4
01:NEWARRAY
02:TOALTSTACK
03:NOP
04:PUSH2
05:FROMALTSTACK
06:DUP
07:TOALTSTACK
08:PUSH0(false)
09:PUSH2
0a:ROLL
0b:SETITEM
0c:PUSHBYTES5[0x68656c6c6f] (hello)
12:FROMALTSTACK
13:DUP
14:TOALTSTACK
15:PUSH1(true)
16:PUSH2
17:ROLL
18:SETITEM
19:PUSHBYTES5[0x776f726c64] (world)
1f:FROMALTSTACK
20:DUP
21:TOALTSTACK
22:PUSH2
23:PUSH2
24:ROLL
25:SETITEM
26:FROMALTSTACK
27:DUP
28:TOALTSTACK
29:PUSH1(true)
2a:PICKITEM
2b:FROMALTSTACK
2c:DUP
2d:TOALTSTACK
2e:PUSH2
2f:PICKITEM
30:CAT
31:FROMALTSTACK
32:DUP
33:TOALTSTACK
34:PUSH3
35:PUSH2
36:ROLL
37:SETITEM
38:NOP
39:FROMALTSTACK
3a:DROP
3b:RET
```
