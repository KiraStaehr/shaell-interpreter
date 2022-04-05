namespace ShaellLang
{
	public interface ITable : IIterable 
	{
		RefValue GetValue(IKeyable key);
		void RemoveValue(IKeyable key);
	}
}