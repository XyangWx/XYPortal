using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XYPortal.RandomStringProvider.RandomStringProvider;

public static class Provider
{
    private static readonly Random Random = new Random(DateTime.Now.Millisecond);
    private static readonly IReadOnlyDictionary<RandomCategory, ISymbolPool> Pools;
    static Provider()
    {
        Pools = new Dictionary<RandomCategory, ISymbolPool>()
        {
            { RandomCategory.LowercaseLetters, new LowerCaseLetterPool(Random) },
            { RandomCategory.UppercaseLetters, new UpperCaseLetterPool(Random) },
            { RandomCategory.ArabicNumerals, new ArabicNumeralPool(Random) },
            ////TODO: 补全逻辑
        };
    }
    
    public static string MakeRandomString(RandomStringInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        
        var pools = GetPools(input.SymbolCategories);

        if (pools.Length >= input.Length)
        {
            throw new InvalidOperationException("输出字符串类型数量大于等于字符串长度");
        }

        List<char> list = [];
        var ignores = input.IgnoreChars?.ToList() ?? [];

        foreach (var pool in pools)
        {
            char @char = pool.Get(ignores.ToArray());

            if (input.IsOnlyOnce)
            {
                if (!ignores.Contains(@char))
                {
                    ignores.Add(@char);
                }
            }
            
            list.Add(@char);
        }

        int leftLength = input.Length - pools.Length;
        
        for (int i = leftLength; i > 0; i--)
        {
            int point = Random.Next(pools.Length);
            
            char @char = pools[point].Get(ignores.ToArray());

            if (input.IsOnlyOnce)
            {
                if (!ignores.Contains(@char))
                {
                    ignores.Add(@char);
                }
            }
            
            list.Add(@char);
        }
        
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < list.ToArray().Length; i++)
        {
            var point =  Random.Next(list.ToArray().Length);
            var @char = list.ToArray()[point];
            sb.Append(@char);
            list.Remove(@char);
        }

        return sb.ToString();
    }

    private static ISymbolPool[] GetPools(RandomCategory category)
    {
        var categories = GetCategories(category);

        List<ISymbolPool> pools = [];
        pools.AddRange(from item in categories where Pools.ContainsKey(item) select Pools[item]);

        return pools.ToArray();
    }

    private static RandomCategory[] GetCategories(RandomCategory category)
    {
        List<RandomCategory> list = [];
        list.AddRange(Enum.GetValues(typeof(RandomCategory))
            .Cast<RandomCategory>()
            .Where(item => item != RandomCategory.All && item != RandomCategory.PasswordOptions)
            .Where(item => (item & category) == item));

        return list.ToArray();
    }
}