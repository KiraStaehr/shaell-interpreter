using System;
using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;

namespace ShaellLang
{
	public class ShaellLang
	{
		//Runs the test script and returns whether it failed or not
		public bool Run(string file, string[] args)
		{
			string fileContent = File.ReadAllText(file);

			try
			{
				AntlrInputStream inputStream = new AntlrInputStream(fileContent);
				ShaellLexer shaellLexer = new ShaellLexer(inputStream);
				shaellLexer.AddErrorListener(new ShaellLexerErrorListener());
				CommonTokenStream commonTokenStream = new CommonTokenStream(shaellLexer);
				ShaellParser shaellParser = new ShaellParser(commonTokenStream);

				ShaellParser.ProgContext progContext = shaellParser.prog();
				var executer = new ExecutionVisitor(args[1..]);
				executer.SetGlobal("print", new NativeFunc(delegate(IEnumerable<IValue> args)
				{
					foreach (var value in args)
					{
						Console.Write(value.ToSString().Val);
					}

					Console.WriteLine();

					return new SNull();
				}, 0));

				executer.SetGlobal("T", TableLib.CreateLib());

				executer.SetGlobal("A", TestLib.CreateLib());

				executer.SetGlobal("debug_break", new NativeFunc(delegate(IEnumerable<IValue> args)
				{
					Console.WriteLine("Debug break");
					return new SNull();
				}, 0));

				executer.Visit(progContext);
			}
			catch (SyntaxErrorException e)
			{
				Console.WriteLine(e.Message);
				return TestLib.testFailed;

			}
			return TestLib.testFailed;
		}
	}
}