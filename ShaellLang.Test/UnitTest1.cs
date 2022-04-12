using Xunit;

namespace ShaellLang.Test;

public class UnitTest1
{
    private ShaellLang shaellLang = new ShaellLang();
    
    [Fact]
    public void TestOperators()
    {
        // Act  
        bool testFailed = shaellLang.Run("../../../OperatorTest.æ", new string[]{"OperatorTest.æ", ""});

        // Assert
        Assert.False(testFailed);
    }
    
    [Fact]
    public void TestMetaTables()
    {   
        // Act  
        bool testFailed = shaellLang.Run("../../../MetatableTest.æ", new string[]{"MetatableTest.æ", ""});

        // Assert
        Assert.False(testFailed);
    }

    [Fact]
    public void TestStringInterpolation()
    {
        // Act
        bool testFailed = shaellLang.Run("../../../StringInterpolationTest.æ", new string[]{"StringInterpolationTest.æ", ""});
        
        // Assert
        Assert.False(testFailed);
    }
    
    [Fact]
    public void TestScope()
    {
        // Act
        bool testFailed = shaellLang.Run("../../../ScopeTest.æ", new string[]{"ScopeTest.æ", ""});
        
        // Assert
        Assert.False(testFailed);
    }
    
}