using System;
using System.Collections.Generic;
using System.Linq;

namespace ShaellLang;

public static class TestLib
{
    public static bool testFailed { get; private set; }
    
    public static IValue CreateLib()
    {
        testFailed = false;
        var userTable = new UserTable();
        userTable
            .GetValue(new SString("assert"))
            .Set(new NativeFunc(AssertFunc, 2));
        userTable
            .GetValue(new SString("assertType"))
            .Set(new NativeFunc(AssertTypeFunc, 2));
        userTable
            .GetValue(new SString("describe"))
            .Set(new NativeFunc(DescribeFunc, 2));
        return userTable;
    }

    private static IValue AssertFunc(IEnumerable<IValue> args)
    {
        var argArr = args.ToArray();
        if (argArr.Length < 2)
        {
            throw new Exception("assert: too few arguments");
        }
        
        if (argArr[0].ToBool() == false)
        {
            throw new Exception("assert: " + argArr[1].ToSString().Val);
        }

        return new SNull();
    }
    
    private static IValue AssertTypeFunc(IEnumerable<IValue> args)
    {
        var argArr = args.ToArray();
        if (argArr.Length < 2)
        {
            throw new Exception("assert: too few arguments");
        }
        
        if (argArr[0].GetTypeName() != argArr[1].ToSString().Val)
        {
            throw new Exception($"assert: expected type {argArr[1].ToSString().Val} but got {argArr[0].GetTypeName()}");
        }

        return new SNull();
    }

    private static IValue DescribeFunc(IEnumerable<IValue> args)
    {
        var argArr = args.ToArray();
        if (argArr.Length < 2)
        {
            throw new Exception("describe: too few arguments");
        }

        var name = argArr[0].ToSString().Val;
        var func = argArr[1].ToFunction();
        
        try
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Testing \"{name}\"");
            Console.ResetColor();

            func.Call(Enumerable.Empty<IValue>());

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Test \"{name}\" succeeded");
            Console.ResetColor();
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Test \"{name}\" failed");
            Console.ResetColor();
            Console.WriteLine(e);
            testFailed = true;
        }
        
        return new SNull();
    }
    
}