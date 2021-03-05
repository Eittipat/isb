using Xunit;
using ISB.Runtime;

namespace ISB.Tests
{
    public class EngineTest
    {
        [Fact]
        public void TestNop()
        {
            Engine engine = new Engine("Program");
            engine.ParseAssembly(@"nop");
            Assert.Equal(0, engine.IP);
            Assert.Equal(0, engine.StackCount);
            engine.Run(true);
            Assert.Equal(1, engine.IP);
            Assert.Equal(0, engine.StackCount);
        }

        [Fact]
        public void TestPushValue()
        {
            Engine engine = new Engine("Program");
            engine.ParseAssembly(@"push 3.14");
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.IP);
            Assert.Equal(1, engine.StackCount);
            NumberValue value = (NumberValue)engine.StackTop;
            Assert.Equal((decimal)3.14, value.ToNumber());

            engine.ParseAssembly(@"pushs ""3.14""");
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.IP);
            Assert.Equal(1, engine.StackCount);
            StringValue str = (StringValue)engine.StackTop;
            Assert.Equal("3.14", str.ToString());
        }

        [Fact]
        public void TestBranch()
        {
            Engine engine = new Engine("Program");
            engine.ParseAssembly(@"br label2
            label1:
            br label3
            label2:
            br label1
            label3:
            nop");
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(4, engine.IP);

            engine.ParseAssembly(@"push 1
            br_if label1 label2
            label1:
            push 1
            br label3
            label2:
            push 2
            label3:
            nop");
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(6, engine.IP);
            Assert.Equal("1", engine.StackTop.ToString());
        }

        [Fact]
        public void TestRegisters()
        {
            Engine engine = new Engine("Program");
            engine.ParseAssembly(@"push 10
            set 0
            push 20
            set 1
            get 0
            get 1
            add");
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(7, engine.IP);
            Assert.Equal("30", engine.StackTop.ToString());
        }

        [Fact]
        public void TestMemoryVariables()
        {
            Engine engine = new Engine("Program");
            engine.ParseAssembly(@"push 10
            store a
            push 20
            store b
            load a
            load b
            sub");
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(7, engine.IP);
            Assert.Equal("-10", engine.StackTop.ToString());
        }

        [Fact]
        public void TestBinaryOprations()
        {
            Engine engine = new Engine("Program");
            engine.ParseAssembly(@"push 120
            push 20
            push 30
            push 40
            push 50
            add
            sub
            mul
            div");
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(9, engine.IP);
            Assert.Equal("-0.1", engine.StackTop.ToString());

            engine.ParseAssembly(@"push 10
            push 20
            ge");
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(3, engine.IP);
            Assert.False(engine.StackTop.ToBoolean());
        }

        [Fact]
        public void TestArrayOperations()
        {
            Engine engine = new Engine("Program");
            engine.Compile(@"a[0] = 3
            a[1] = ""Hello""
            a[1]", true);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal("Hello", engine.StackTop.ToString());

            engine.Compile(@"a[0] = 3
            a[1] = ""Hello""
            a[0]", true);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal("3", engine.StackTop.ToString());

            engine.Compile(@"a[0] = 3
            a[1] = ""Hello""
            a[""unknown""]", true);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal("", engine.StackTop.ToString());

            engine.Compile(@"a[1][2] = 1
            a[8][0] = 2
            a[""a""][1][""b""] = 3
            a[8][0]", true);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal("2", engine.StackTop.ToString());

            engine.Compile(@"a[1][2] = 1
            a[8][0] = 2
            a[""a""][1][""b""] = 3
            a[""a""][1][""b""]", true);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal("3", engine.StackTop.ToString());

            engine.Compile(@"a[1][2] = 1
            a[8][0] = 2
            a[""a""][1][""b""] = 3
            a[""a""][1][""c""]", true);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal("", engine.StackTop.ToString());
        }

        [Fact]
        public void TestFebonacci()
        {
            const string code =
                @"NUM = 20
                  Fib[0] = 0
                  Fib[1] = 1
                  For i = 2 to NUM
                      Fib[i] = Fib[i - 1] + Fib[i - 2]
                  EndFor
                  Fib[20]";
            Engine engine = new Engine("Program");
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal("6765", engine.StackTop.ToString());
        }

        [Fact]
        public void TestIsPrime()
        {
            const string code =
                @"n = 1000117 ' number to be test.
                  IsPrime = 0
                  if n <= 3 then
                    if n > 1 then
                      IsPrime = 1
                      goto TheEnd
                    else
                      IsPrime = 0
                      goto TheEnd
                    endif
                  elseif n mod 2 = 0 or n mod 3 = 0 then
                    IsPrime = 0
                    goto TheEnd
                  else
                    i = 5
                    while i * i <= n
                      if n mod i = 0 or n mod (i + 2) = 0 then
                        IsPrime = 0
                        goto TheEnd
                      endif
                      i = i + 6
                    endwhile
                    IsPrime = 1
                  endif
                  TheEnd:
                  IsPrime";
            Engine engine = new Engine("Program");
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.True(engine.StackTop.ToBoolean());
        }


        [Theory]

        [InlineData (@"br abc",
        @"Runtime error: Undefined assembly label, abc (0:     br abc)")]

        [InlineData (@"push 0
        add",
        @"Runtime error: Unexpected empty stack. (1:     add)")]

        [InlineData (@"push 3
        push 0
        div",
        @"Runtime error: Division by zero. (2:     div)")]
        public void TestRuntimeErrors(string code, string error)
        {
            Engine engine = new Engine("Program");
            engine.ParseAssembly(code);
            engine.Run(true);
            Assert.True(engine.HasError);
            Assert.Single(engine.ErrorInfo.Contents);
            System.Console.WriteLine(engine.ErrorInfo.Contents[0].ToDisplayString());
            Assert.Equal(error, engine.ErrorInfo.Contents[0].ToDisplayString());
        }
    }
}