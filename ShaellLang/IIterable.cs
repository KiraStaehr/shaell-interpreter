using System.Collections.Generic;

namespace ShaellLang; 


public interface IIterable 
{
    public IEnumerable<IKeyable> GetKeys(); 
    

}