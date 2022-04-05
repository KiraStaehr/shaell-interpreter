using System.Collections.Generic;

namespace ShaellLang;

public class NativeTable : ITable
{
    private Dictionary<string, IValue> valueLookup;

    public NativeTable()
    {
        valueLookup = new Dictionary<string, IValue>();
    }

    public void SetValue(string key, IValue value)
    {
        valueLookup[key] = value;
    }

    public virtual RefValue GetValue(IKeyable key)
    {
        if (key is SString)
        {
            if (valueLookup.TryGetValue(key.KeyValue, out IValue val))
            {
                return new RefValue(val);
            }
        }

        return null;
    }

    public virtual void RemoveValue(IKeyable key)
    {
        return;
    }
}