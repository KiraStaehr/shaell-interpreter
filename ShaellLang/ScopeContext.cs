using System;
using System.Collections.Generic;

namespace ShaellLang;

public class ScopeContext
{
    private Dictionary<string, RefValue> _values;

    public ScopeContext()
    {
        _values = new Dictionary<string, RefValue>();
    }
    
    public RefValue GetValue(string key)
    {
        bool found = _values.TryGetValue(key, out RefValue foundValue);

        return found ? foundValue : null;
    }

    public RefValue NewValue(string key, IValue val)
    {
        RefValue rv = new RefValue(val);
        if (_values.ContainsKey(key))
        {
            throw new Exception($"Variable {key} already declared name");
        }
        
        _values[key] = rv;
        return rv;
    }
}