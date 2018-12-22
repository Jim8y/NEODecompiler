# NEODecompiler
This is a NEO smart contract decompiler, still under developing.

本项目尚处于开发的初级阶段，目前基本可以完成对整个项目的字节码进行反编译，然而，由于开发时间尚短，项目中依然存在很多还没完成的地方。 尤其是还有几个指令的
解析由于存在理解问题，还没有对其进行解析，因此如果合约代码中使用到了这些指令，反编译将会失败。

Feel free to try, use, and submit your commit. Contribution is warmly welcomed.

#如何使用

在Program.cs中可以指定avm的文件名，然后运行程序，程序就会首先输出反编译代码，然后输出指令码。

Assign the avm file path in Program.cs, then run this project in visual studio, you will get the decompiled result.

例如对于如下合约:
Sample contract:

```c#
        public static void Main()
        {
            string a = "hello ";
            string b = "world";
            for (int i = 0; i < 10; i++)
            {
                string c = a + b;
            }
        }
```

反编译如下:
Decompiled result:

```C#
00 static void Main(string[] args) {
01    Array v_array_0 = new Array<?>(5);
04    byte[] v_array_1 = new byte[]("hello ");
11    v_array_0[0] = v_array_1;
12    byte[] v_array_2 = new byte[]("world");
1e    v_array_0[1] = v_array_2;
26    v_array_0[2] = 0;
27 Jump 49
2f    v_array_1 = v_array_0[0];
34    v_array_2 = v_array_0[1];
35    v_array_1 = v_array_1 + v_array_2     // (hello world)
3c    v_array_0[3] = v_array_1;
42    int v_int_0 = v_array_0[2];
44    v_int_0 = 1 + v_int_0;
4b    v_array_0[2] = v_int_0;
50    v_int_0 = v_array_0[2];
52    bool v_bool_0 = v_int_0 < 10;
59    v_array_0[4] = v_bool_0;
5e    v_bool_0 = v_array_0[4];
5f Jump 27
  }

00:PUSH5
01:NEWARRAY
02:TOALTSTACK
03:NOP
04:PUSHBYTES6[0x68656c6c6f20] (hello )
0b:FROMALTSTACK
0c:DUP
0d:TOALTSTACK
0e:PUSH0(false)
0f:PUSH2
10:ROLL
11:SETITEM
12:PUSHBYTES5[0x776f726c64] (world)
18:FROMALTSTACK
19:DUP
1a:TOALTSTACK
1b:PUSH1(true)
1c:PUSH2
1d:ROLL
1e:SETITEM
1f:PUSH0(false)
20:FROMALTSTACK
21:DUP
22:TOALTSTACK
23:PUSH2
24:PUSH2
25:ROLL
26:SETITEM
27:JMP[37]
2a:NOP
2b:FROMALTSTACK
2c:DUP
2d:TOALTSTACK
2e:PUSH0(false)
2f:PICKITEM
30:FROMALTSTACK
31:DUP
32:TOALTSTACK
33:PUSH1(true)
34:PICKITEM
35:CAT
36:FROMALTSTACK
37:DUP
38:TOALTSTACK
39:PUSH3
3a:PUSH2
3b:ROLL
3c:SETITEM
3d:NOP
3e:FROMALTSTACK
3f:DUP
40:TOALTSTACK
41:PUSH2
42:PICKITEM
43:PUSH1(true)
44:ADD
45:FROMALTSTACK
46:DUP
47:TOALTSTACK
48:PUSH2
49:PUSH2
4a:ROLL
4b:SETITEM
4c:FROMALTSTACK
4d:DUP
4e:TOALTSTACK
4f:PUSH2
50:PICKITEM
51:PUSH10
52:LT
53:FROMALTSTACK
54:DUP
55:TOALTSTACK
56:PUSH4
57:PUSH2
58:ROLL
59:SETITEM
5a:FROMALTSTACK
5b:DUP
5c:TOALTSTACK
5d:PUSH4
5e:PICKITEM
5f:JMPIF[-53]
62:NOP
63:FROMALTSTACK
64:DROP
65:RET
```
