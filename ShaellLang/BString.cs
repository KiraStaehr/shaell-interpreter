using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShaellLang;

public class BString : BaseValue, ITable, IIterable
{
    private string _val;
    private byte[] _buffer;
    private NativeTable _table;

    public BString(byte[] buf)
        : base("bstring")
    {
        _buffer = buf;
        _val = Encoding.Default.GetString(buf);
        _table = new NativeTable();
        _table.SetValue("toSString", new NativeFunc(ToSStringFunc, 1));
        _table.SetValue("substring", new NativeFunc(SubStringFunc, 2));
    }

    private IValue ToSStringFunc(IEnumerable<IValue> argCollection)
    {
        Encoding e;
        switch (argCollection.ToArray()[0].ToSString().Val.ToLower())
        {
            case "ascii":
                e = new ASCIIEncoding();
                break;
            case "utf-8":
                e = new UTF8Encoding();
                break;
            case "utf-16":
                e = new UnicodeEncoding();
                break;
            case "utf-32":
                e = new UTF32Encoding();
                break;
            default:
                throw new Exception("Wrong encoding");
        }

        return new SString(e.GetString(_buffer));
    }

    private IValue SubStringFunc(IEnumerable<IValue> argCollection)
    {
        Number[] args = argCollection.ToArray().Select(x => x.ToNumber()).ToArray();
        return new SString(_val.Substring((int) args[0].ToInteger(), (int) args[1].ToInteger()));
    }

    public BString(string val) : this(Encoding.Default.GetBytes(val)) { }
    public override bool ToBool() => true;

    public override Number ToNumber() => new(int.Parse(_val));

    public override SString ToSString() => new(_val);

    public override ITable ToTable() => this;
    public override bool IsEqual(IValue other)
    {
        if (other is BString unpackedBString)
        {
            return unpackedBString._buffer.SequenceEqual(_buffer);
        }

        return false;
    }

    public RefValue GetValue(IValue key)
    {
        if (key is Number numberKey)
        {
            if (numberKey.IsInteger)
            {
                long val = numberKey.ToInteger();
                if (val >= 0 && val < _buffer.Length)
                {
                    //val is less than _val.Length which is an int, therefore val can safely be casted to int
                    return new RefValue(new Number(_buffer[val]));
                }
            }
        }
        return _table.GetValue(key);
    }

    public void RemoveValue(IValue key)
    {
        return;
    }

    public string Val => _val;
    public byte[] Buffer => _buffer;
    public string KeyValue => _val;
    public string UniquePrefix => "S";

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
}