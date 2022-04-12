using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShaellLang;

public class SString : BaseValue, ITable, IIterable
{
    private string _val;
    private NativeTable _nativeTable;
    
    public SString(string str)
        : base("string")
    {
        _val = str;
        _nativeTable = new NativeTable();
        
        _nativeTable.SetValue("length", new NativeFunc(lengthCallHandler, 0));
        _nativeTable.SetValue("substring", new NativeFunc(SubStringFunc, 2));
    }

    private IValue SubStringFunc(IEnumerable<IValue> argCollection)
    {
        Number[] args = argCollection.ToArray().Select(x => x.ToNumber()).ToArray();
        return new SString(Val.Substring((int) args[0].ToInteger(), (int) args[1].ToInteger()));
    }

    private IValue lengthCallHandler(IEnumerable<IValue> args)
    {
        return new Number(this._val.Length);
    }

    public override bool ToBool() => true;
    public override SString ToSString() => this;
    public override ITable ToTable() => this;
    public override bool IsEqual(IValue other)
    {
        if (other is SString otherString)
        {
            return _val == otherString._val;
        }

        return false;
    }

    public RefValue GetValue(IValue key)
    {
        if (key is Number numberKey)
        {
            if (numberKey.IsInteger)
            {
                var val = numberKey.ToInteger();
                if (val >= 0 && val < _val.Length)
                {
                    //val is less than _val.Length which is an int, therefore val can safely be casted to int
                    return new RefValue(new SString(new string(_val[(int) val], 1)));
                }
            }
        }
        return _nativeTable.GetValue(key);
    }

    public void RemoveValue(IValue key)
    {
        return;
    }

    public override string ToString()
    {
        return _val;
    }

    public IEnumerable<IValue> GetKeys()
    {
        var rv = new List<Number>();
        for (int i = 0; i < _val.Length; i++)
        {
            var n = new Number(i);
            rv.Add(n);
        }
        
        return rv;
    }

    public static SString operator +(SString left, SString right)
    {
        return new SString(left.Val + right.Val);
    }

    public static SString operator *(SString left, Number right)
    {
        StringBuilder sb = new StringBuilder();
        if (right.IsInteger)
        {
            var val = right.ToInteger();
            if (val > int.MaxValue)
                throw new Exception("String multiplication overflow");
            if (val < 0)
                throw new Exception("String cannot be multiplied with less than 0");
            sb.Insert(0, left.Val, (int) val);
        }
        else
        {
            var floored = Math.Floor(right.ToFloating());
            if (floored > int.MaxValue)
                throw new Exception("String multiplication overflow");
            if (floored < 0)
                throw new Exception("String cannot be multiplied with less than 0");
            sb.Insert(0, left.Val, (int) Math.Floor(right.ToFloating()));
        }

        return new SString(sb.ToString());
    }

    public string Val => _val;
    public string KeyValue => _val;
    public string UniquePrefix => "S";

    public override int GetHashCode()
    {
        //This might be wrong but i cant be asked
        return ("S" + Val).GetHashCode();
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is SString str)
        {
            return IsEqual(str);
        }

        return false;
    }
}