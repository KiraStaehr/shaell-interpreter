namespace ShaellLang;

public class RefValue : BaseValue
{
    private IValue _realValue;
    public RefValue(IValue val)
        : base("refvalue")
    {
        _realValue = val;
    }
    
    public IValue Set(IValue newValue)
    {
        _realValue = newValue;
        return _realValue;
    }

    public IValue Get()
    {
        return _realValue;
        
    }

    public override IValue Unpack()
    {
        if (_realValue is RefValue realRefValue)
        {
            return realRefValue.Unpack();
        }
        else
        {
            return _realValue;
        }
    }

    public override bool ToBool() => _realValue.ToBool();

    public override Number ToNumber() => _realValue.ToNumber();

    public override IFunction ToFunction() => _realValue.ToFunction();

    public override SString ToSString() => _realValue.ToSString();

    public override ITable ToTable() => _realValue.ToTable();

    public override bool IsEqual(IValue other)
    {
        return _realValue.IsEqual(other);
    }

    public override string ToString()
    {
        return _realValue.ToString();
    }
}