namespace ShaellLang;

public class RawKeyable : IKeyable
{
    public string KeyValue { get; private set; }
    public string UniquePrefix { get; }

    public RawKeyable(string s)
    {
        KeyValue = s;
        UniquePrefix = "";
    }

  
}