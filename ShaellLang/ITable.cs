namespace ShaellLang
{
	public interface ITable
	{
		RefValue GetValue(IKeyable key);
		void RemoveValue(IKeyable key);
	}
}