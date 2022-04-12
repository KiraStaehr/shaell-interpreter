using System;
using System.Collections.Generic;

namespace ShaellLang;

public class BaseTable : BaseValue, ITable, IIterable
{
    //Store all values that cannot be stored in an array
    private Dictionary<IValue, RefValue> _values = new();
    //Store the integer keys that are not consecutive
    private SortedDictionary<int, RefValue> _sortedValues = new();
    //Store the integer keys in an array
    private List<RefValue> _consecutiveValues = new();
    public int ArrayLength => _consecutiveValues.Count;
    public BaseTable(string typeName) : base(typeName)
    { }

    /// <summary>
    /// Sets a value into the table.
    /// </summary>
    /// <param name="key">The key to the value.</param>
    /// <param name="value">The value to insert.</param>
    /// <returns>The value that was inserted.</returns>
    public RefValue SetValue(IValue key, IValue value)
    {
        var val = GetValue(key);
        if (val != null)
        {
            val.Set(value);
            return val;
        }

        return SetNewValue(key, value);
    }
    
    //Should not be called if it already exists
    protected RefValue SetNewValue(IValue key, IValue value)
    {
        if (key is Number {IsInteger: true} numberKey)
        {
            var n = numberKey.ToInteger();
            if (n >= 0 && n <= int.MaxValue)
            {
                return SetNewValue((int) n, value);
            }
        }

        var newVal = new RefValue(value);
        _values[key] = newVal;
        return newVal;
    }

    //Kan kun blive kaldet hvis man ved key ikke eksistere
    private RefValue SetNewValue(int key, IValue value)
    {
        var newVal = new RefValue(value);
        _sortedValues[key] = newVal;
        MoveConsecutiveValues();

        return newVal;
    }
    
    /// <summary>
    /// Retrieves a value from the table.
    /// </summary>
    /// <param name="key">The key for the value to retrieve.</param>
    /// <returns>The value.</returns>
    public virtual RefValue GetValue(IValue key)
    {
        if (key is Number {IsInteger: true} numberKey)
        {
            var x = numberKey.ToInteger();
            if (x >= 0 && x <= int.MaxValue)
                return GetValue((int) x);
        }
        
        if (_values.TryGetValue(key, out var value))
            return value;

        return null;
    }
    
    private RefValue GetValue(int key)
    {
        if (key >= _consecutiveValues.Count)
        {
            if (_sortedValues.TryGetValue(key, out var value))
            {
                return value;
            }

            return null;
        }

        return _consecutiveValues[key];
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
    public IValue LengthFunc(IEnumerable<IValue> args)
    {
        return new Number(_consecutiveValues.Count);
    }

    public void RemoveValue(IValue key)
    {
        _values.Remove(key);
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

    public IEnumerable<IValue> GetKeys()
    {
        var rv = new List<IValue>();
        foreach (var key in _values)   
        {
            rv.Add(key.Key);
        }

        for(int i = 0; i < _consecutiveValues.Count; i++)
        {
            var v = new Number(i);
            rv.Add(v);
        }

        foreach (var key in _sortedValues)
        {
            var v = new Number(key.Key);
            rv.Add(v);
        }
        return rv;  
    }
}