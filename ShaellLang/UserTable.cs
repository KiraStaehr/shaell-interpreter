using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ShaellLang
{
	public class UserTable : BaseValue, ITable, IIterable
	{
		private Dictionary<IKeyable, RefValue> values;
		//Store the array values in order
		private SortedDictionary<int, RefValue> _sortedValues;
		//Store the integer keys in an array
		private List<RefValue> _consecutiveValues;
		
		private string KeyValue(IKeyable key) => key.UniquePrefix + key.KeyValue;
		public UserTable() 
			: base("usertable")
		{
			values = new Dictionary<string, RefValue>();
			_consecutiveValues = new List<RefValue>();
			_sortedValues = new SortedDictionary<int, RefValue>();
			SetValue(new SString("length"), new NativeFunc(LengthFunc, 0));
			SetValue(new SString("insert"), new NativeFunc(InsertFunc, 1));
		}

		public IValue InsertFunc(IEnumerable<IValue> args)
		{
			foreach (var arg in args)
			{
				var newValue = this.GetValue(new Number(_consecutiveValues.Count));
				newValue.Set(arg);
			}

			return new SNull();
		}

		public IValue LengthFunc(IEnumerable<IValue> args)
		{
			return new Number(_consecutiveValues.Count);
		}

		private RefValue SetValue(IKeyable key, IValue value)
		{
			RefValue refValue = new RefValue(value);
			values.Add(KeyValue(key), refValue);
			return refValue;
		}

		private void MoveConsecutiveValues()
		{
			var targetValues = new List<int>();
			foreach (var val in _sortedValues)
			{
				if (val.Key == _consecutiveValues.Count)
				{
					_consecutiveValues.Add(val.Value);
					targetValues.Add(val.Key);
				}
				else
				{
					break;
				}
			}

			foreach (var targetValue in targetValues)
			{
				_sortedValues.Remove(targetValue);
			}
		}
		
		public RefValue GetValue(IKeyable key)
		{
			if (key is Number {IsInteger: true} numberKey)
			{
				var number = numberKey.ToInteger();
				if (number >= 0)
				{
					if (number < _consecutiveValues.Count)
					{
						//We can cast to int since we have already bounds checked the number
						return _consecutiveValues[(int) number];
					}
					if (number == _consecutiveValues.Count)
					{
						var newVal = new RefValue(new SNull());
						_consecutiveValues.Add(newVal);
						MoveConsecutiveValues();
						return newVal;
					}
					if (number <= int.MaxValue)
					{
						if (_sortedValues.TryGetValue((int) number, out RefValue thing))
						{
							return thing;
						}

						var newVal = new RefValue(new SNull());
						_sortedValues[(int) number] = newVal;
						return newVal;
					}
				} 
					
			}
			
			bool exists = values.TryGetValue(KeyValue(key), out RefValue value);
			if (exists)
			{
				return value;
			}
			
			//Since keys should be implicitly defined we just add the key and return it
			return SetValue(key, new SNull());
		}

		public void RemoveValue(IKeyable key)
		{
			values.Remove(KeyValue(key));
		}

		public override bool ToBool()
		{
			return true;
		}
		
		public override ITable ToTable()
		{
			return this;
		}

		public override bool IsEqual(IValue other)
		{
			return other == this;
		}

		public IEnumerable<IKeyable> GetKeys()
		{
			
			var rv = new List<IKeyable>();
			for (int i = 0; i < _consecutiveValues.Count; i++)
			{
				Number n = new Number(i);
				rv.Add(n);
			}

			foreach (var key in _sortedValues.Keys)
			{
				rv.Add(new Number(key));
			}

			foreach (var key in values.Keys)
			{
				var k = new RawKeyable(key);
				rv.Add(k);
			}
			return rv;
		}
	}
}