namespace ShaellLang
{
	public interface ITable
	{
		RefValue GetValue(IKeyable key);
		void RemoveValue(IValue key);
	}
}