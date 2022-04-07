namespace ShaellLang
{
	public interface IKeyable : IValue
	{
		string KeyValue { get; }
		string UniquePrefix { get; }
		public string ToString() => UniquePrefix + KeyValue;
	}
}