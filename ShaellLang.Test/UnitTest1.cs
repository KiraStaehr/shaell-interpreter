using Xunit;

namespace ShaellLang.Test;

public class UnitTest1
{
    private ShaellLang shaellLang = new ShaellLang();
    
    [Fact]
    public void Test1()
    {
        // Arrange
        ShaellLang shaellLang = new ShaellLang();
        
        // Act  
        bool testFailed = shaellLang.Run("../../../test.æ", new string[]{"test.æ", ""});

        // Assert
        Assert.False(testFailed);
    }
    
    [Fact]
    public void TestMetaTables()
    {   
        // Act  
        bool testFailed = shaellLang.Run("../../../metatable.æ", new string[]{"metatable.æ", ""});

        // Assert
        Assert.False(testFailed);
    }
}