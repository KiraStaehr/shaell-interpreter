using System;
using System.Collections.Generic;
using System.Collections;

namespace ShaellLang 
{
	public delegate IValue NativeFuncCallHandler(IEnumerable<IValue> args);
	public class NativeFunc : BaseValue, IFunction 
	{
		private NativeFuncCallHandler _callHandler;

		public NativeFunc(NativeFuncCallHandler callHandler, uint argumentCount)
			: base("nativefunc")
		{
			_callHandler = callHandler;
			ArgumentCount = argumentCount;
		}

		//Implement IFunction
		
		public IValue Call(IEnumerable<IValue> args) => new JobObject(_callHandler(args));
		public override SString ToSString()
		{
			return new SString("nativeFunc");
		}

		public uint ArgumentCount {  get; private set; }
		
		public override bool ToBool() => true;
		
		public override IFunction ToFunction() => this;
		
		public override bool IsEqual(IValue other)
		{
			return other == this;
		}
	}
}