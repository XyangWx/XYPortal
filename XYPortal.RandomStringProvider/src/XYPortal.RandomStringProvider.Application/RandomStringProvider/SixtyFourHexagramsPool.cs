using System;
using System.Collections.Generic;
using System.Linq;

namespace XYPortal.RandomStringProvider.RandomStringProvider;

internal class SixtyFourHexagramsPool : SymbolPool
{
    private static readonly char[] Chars = GetSixtyFourHexagrams();
    
    public SixtyFourHexagramsPool(Random random)
        : base(random)
    {
    }
    
    public override char Get(params char[] ignores)
    {
        var chars = Chars.Where(c => !ignores.Contains(c)).ToArray();
        
        var value = chars[Next(chars.Length)];

        return value;
    }

    private static char[] GetSixtyFourHexagrams()
    {
        int start = 0x4cdc0;

        List<char> chars = [];

        for (int i = 0; i < 64; i++)
        {
            chars.Add((char)(start + i));
        }
        
        return  chars.ToArray();
    }
}
