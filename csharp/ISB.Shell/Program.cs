using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine;
using CommandLine.Text;
using ISB.Runtime;
using ISB.Utilities;

namespace ISB.Shell
{
    class Program
    {
        public class Options
        {
            [Option('i', "input", Required=false,
                HelpText="BASIC file (*.bas) to run/compile, or ISB assembly file (*.asm) to run. " +
                    "If not set, the interactive shell mode will start.")]
            public string InputFile { get; set; }

            [Option('c', "compile", Required=false,
                HelpText="Compile BASIC code to ISB assembly, without running it.")]
            public bool Compile { get; set; }

            [Option('o', "output", Required=false,
                HelpText="Output file path when --compile is set. " +
                    "If not set, the output assembly will be written to stdout.")]
            public string OutputFile { get; set; }
        }

        private const string BasicExtension = ".bas";
        private const string AssemblyExtension = ".asm";

        static void Main(string[] args)
        {
            var parserResult = Parser.Default.ParseArguments<Options>(args);
            var helpText = HelpText.AutoBuild<Options>(parserResult, h => h, e => e);
            parserResult.WithParsed(options =>
            {
                Console.Error.WriteLine(helpText.Heading);
                Console.Error.WriteLine(helpText.Copyright);
                Console.Error.WriteLine();
                RunOptions(options);
            });
        }

        private static void RunOptions(Options opts)
        {
            if (!String.IsNullOrWhiteSpace(opts.InputFile))
            {
                string fileName = Path.GetFileName(opts.InputFile);
                string ext = Path.GetExtension(opts.InputFile);
                string code = File.ReadAllText(opts.InputFile);
                if (ext != null && ext.ToLower() == BasicExtension)
                {
                    if (opts.Compile)
                    {
                        if (!String.IsNullOrWhiteSpace(opts.OutputFile))
                        {
                            using (StreamWriter output = new StreamWriter(opts.OutputFile))
                            {
                                CompileToTextFormat(fileName, code, output);
                            }
                        }
                        else
                        {
                            CompileToTextFormat(fileName, code, Console.Out);
                        }
                    }
                    else
                    {
                        RunProgram(fileName, code);
                    }
                }
                else if (ext != null && ext.ToLower() == AssemblyExtension)
                {
                    RunAssembly(fileName, code);
                }
                else
                {
                    Console.Error.WriteLine($"Only {BasicExtension} or {AssemblyExtension} file is supported.");
                }
            }
            else
            {
                // Starts the interactive shell if not input file is provided.
                StartREPL();
            }
        }

        private static bool CompileToTextFormat(string fileName, string code, TextWriter output)
        {
            Engine engine = new Engine(fileName);
            if (!engine.Compile(code, true))
            {
                ErrorReport.Report(code, engine.ErrorInfo, Console.Error);
                return false;
            }

            string commentLine = ';' + new String('-', 99);
            output.WriteLine(commentLine);
            output.WriteLine($"; The ISB Assembly code generated from {fileName}");
            output.WriteLine($"; The code can be parsed and run by the shell tool of ISB (Interactive Small Basic).");
            output.WriteLine($"; See https://github.com/wixette/isb for more details.");
            output.WriteLine(commentLine);
            output.WriteLine(engine.AssemblyInTextFormat);
            return true;
        }

        private static bool RunAssembly(string fileName, string code)
        {
            Engine engine = new Engine(fileName);
            engine.ParseAssembly(code);
            if (!engine.Run(true))
            {
                ErrorReport.Report(engine.ErrorInfo, Console.Error);
                return false;
            }
            if (engine.StackCount > 0)
            {
                Console.WriteLine(engine.StackTop.ToDisplayString());
            }
            return true;
        }

        private static bool RunProgram(string fileName, string code)
        {
            Engine engine = new Engine(fileName);
            if (!engine.Compile(code, true))
            {
                ErrorReport.Report(code, engine.ErrorInfo, Console.Error);
                return false;
            }
            if (!engine.Run(true))
            {
                ErrorReport.Report(engine.CodeLines, engine.ErrorInfo, Console.Error);
                return false;
            }
            if (engine.StackCount > 0)
            {
                Console.WriteLine(engine.StackTop.ToDisplayString());
            }
            return true;
        }

        private class Evaluator : REPL.IEvaluator
        {
            private Engine engine;
            private List<string> multiLineCode;

            private const string emptyPattern = @"^\s*$";
            private Regex emptyRegex = new Regex(emptyPattern);

            private const string exitCommandPattern = @"^\s*[Qq][Uu][Ii][Tt]\s*(\(\s*\))*\s*$";
            private Regex exitCommandRegex = new Regex(exitCommandPattern);

            private const string listCommandPattern = @"^\s*[Ll][Ii][Ss][Tt]\s*(\(\s*\))*\s*$";
            private Regex listCommandRegex = new Regex(listCommandPattern);

            private const string clearCommandPattern = @"^\s*[Cc][Ll][Ee][Aa][Rr]\s*(\(\s*\))*\s*$";
            private Regex clearCommandRegex = new Regex(clearCommandPattern);

            public Evaluator()
            {
                this.engine = new Engine("Program");
                multiLineCode = new List<string>();
            }

            public REPL.EvalResult Eval(string line)
            {
                Debug.Assert(line != null);

                if (emptyRegex.IsMatch(line))
                {
                    return REPL.EvalResult.OK;
                }

                if (exitCommandRegex.IsMatch(line))
                {
                    return REPL.EvalResult.Exit;
                }

                if (listCommandRegex.IsMatch(line))
                {
                    Console.WriteLine(String.Join('\n', engine.CodeLines));
                    return REPL.EvalResult.OK;
                }

                if (clearCommandRegex.IsMatch(line))
                {
                    engine.Reset();
                    return REPL.EvalResult.OK;
                }

                string code = (multiLineCode.Count > 0) ? code = String.Join('\n', multiLineCode) + "\n" + line : line;
                if (!engine.Compile(code, false))
                {
                    if (engine.ErrorInfo.Contents.Count > 0 &&
                        engine.ErrorInfo.Contents.Last().Code == Diagnostic.ErrorCode.UnexpectedEndOfStream)
                    {
                        multiLineCode.Add(line);
                        return REPL.EvalResult.NeedMoreLines;
                    }
                    else
                    {
                        ErrorReport.Report(code, engine.ErrorInfo, Console.Error);
                        return REPL.EvalResult.OK;
                    }
                }

                multiLineCode.Clear();
                if (!engine.Run(false))
                {
                    ErrorReport.Report(engine.CodeLines, engine.ErrorInfo, Console.Error);
                    return REPL.EvalResult.OK;
                }
                if (engine.StackCount > 0)
                {
                    // Remaining results in the stack are always popped out in the interactive env.
                    BaseValue value = engine.StackPop();
                    Console.WriteLine(value.ToDisplayString());
                }

                return REPL.EvalResult.OK;
            }
        }

        private static void StartREPL()
        {
            Evaluator evaluator = new Evaluator();
            REPL repl = new REPL("] ", "> ", evaluator);
            Console.WriteLine("Type \"quit\" to exit, \"list\" to show the code, \"clear\" to clear the code.");
            repl.Loop();
        }
    }
}
