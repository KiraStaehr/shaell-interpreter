using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Atn;

namespace ShaellLang
{
	public class UserTable : BaseTable, IIterable
	{
		public UserTable MetaTable { get; set; }
		
		public UserTable() : base("usertable")
		{
			SetValue(new SString("length"), new NativeFunc(LengthFunc, 0));
			SetValue(new SString("insert"), new NativeFunc(InsertFunc, 1));
			SetValue(new SString("set_meta_table"), new NativeFunc(SetMetaTableFunc, 1));
		}

		/// <summary>
		/// Returns the value indexed by the given key. If the key is not found, the key is tied to a SNull value.
		/// </summary>
		/// <param name="key">The key to search for.</param>
		/// <returns>The value that is tied to the key</returns>
		public override RefValue GetValue(IKeyable key)
		{
			RefValue value;
			
			if (ContainsKey(key))
				 return base.GetValue(key);

			if (MetaTable?.ContainsKey(key) != null) 
				 return MetaTable.GetValue(key);
		

			return InsertEmpty(key);
		}

		public IValue SetMetaTableFunc(IEnumerable<IValue> args)
		{
			var table = args.First() as UserTable;
			MetaTable = table;
			return MetaTable;
		}
		
	}
}