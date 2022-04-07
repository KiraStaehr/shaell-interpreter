using System;
using System.Collections.Generic;

namespace ShaellLang;

public class BaseTable : BaseValue, ITable
{
    private Dictionary<IValue, RefValue> _values = new();
    //Store the array values in order
    private SortedDictionary<int, RefValue> _sortedValues = new();
    //Store the integer keys in an array
    private List<RefValue> _consecutiveValues = new();
    public int ArrayLength => _consecutiveValues.Count;
    public BaseTable(string typeName) : base(typeName)
    { }

    /// <summary>
    /// Check if the table contains a value with the given key.
    /// </summary>
    /// <param name="key">The key to check for.</param>
    /// <returns>Whether or not the key exists. </returns>
    public bool ContainsKey(IValue key)
    {
        if (key is Number {IsInteger: true} numberKey)
            return ContainsKey(numberKey.ToInt32());
        return _values.ContainsKey(key);
    }
    protected bool ContainsKey(int key) => _sortedValues.ContainsKey(key);

    /// <summary>
    /// Inserts a value into the table.
    /// </summary>
    /// <param name="key">The key to the value.</param>
    /// <param name="value">The value to insert.</param>
    /// <returns>The value that was inserted.</returns>
    /// <exception cref="Exception">The Key already exists.</exception>
    public RefValue Insert(IKeyable key, RefValue value)
    {
        if (ContainsKey(key))
            throw new Exception("Key already exists");
			
        if (key is Number {IsInteger: true} numberKey)
        {
            return Insert(numberKey.ToInt32(), value);
        }
			
        _values.Add(key, value);
        return value;
    }

    protected RefValue Insert(int key, RefValue value)
    {
        if (key == ArrayLength)
        {
            _consecutiveValues.Add(value);
            MoveConsecutiveValues();
            return value;
        }

        _sortedValues.Add(key, value);
        return value;
    }
		
    protected RefValue InsertEmpty(IKeyable key) => Insert(key, new RefValue(new SNull()));

    /// <summary>
    /// Retrieves a value from the table.
    /// </summary>
    /// <param name="key">The key for the value to retrieve.</param>
    /// <returns>The value.</returns>
    public virtual RefValue GetValue(IKeyable key)
    {
        if (key is Number {IsInteger: true} numberKey)
        {
            var number = numberKey.ToInt32();
            return GetValue(number);
        }

        return GetValue(key);
    }

    private RefValue GetValue(int key)
    {
        if (key < 0)
            throw new Exception("Negative index");
        if (key < ArrayLength)	
            return _consecutiveValues[key];
        if(_sortedValues.TryGetValue(key, out var value))
            return value;
        throw new Exception("Index out of bounds");
    }
    private RefValue GetValue(IValue key)
    {
        if (_values.TryGetValue(key, out var value))
            return value;
        throw new Exception("Key not found");
    }
		
    
    protected RefValue SetValue(IKeyable key, IValue value)
    {
        RefValue refValue = new RefValue(value);
        _values.Add(key, refValue);
        return refValue;
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
        for (int i = 0; i < _consecutiveValues.Count; i++)
        {
            rv.Add(new Number(i));
        }
        foreach (var key in _sortedValues.Keys)
        {
            rv.Add(new Number(key));
        }
        foreach (var key in _values.Keys)
        {
            rv.Add(key);
        }
        return rv;
    }
}