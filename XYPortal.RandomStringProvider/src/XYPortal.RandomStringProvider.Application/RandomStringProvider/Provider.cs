using System;
using System.Collections.Generic;

namespace XYPortal.RandomStringProvider.RandomStringProvider;

public static class Provider
{
    private static readonly Random _random = new Random(DateTime.Now.Millisecond);
    private static readonly IReadOnlyDictionary<RandomCategory, ISymbolPool> _pools;
    static Provider()
    {
        _pools = new Dictionary<RandomCategory, ISymbolPool>()
        {
            { RandomCategory.LowercaseLetters, new LowerCaseLetterPool(_random) },
            { RandomCategory.UppercaseLetters, new UpperCaseLetterPool(_random) },
            ////TODO: 补全逻辑
        };
    }
    
    public static string MakeRandomString(RandomStringInput input)
    {
        ////TODO: 补全逻辑
        throw new System.NotImplementedException();
    }
}