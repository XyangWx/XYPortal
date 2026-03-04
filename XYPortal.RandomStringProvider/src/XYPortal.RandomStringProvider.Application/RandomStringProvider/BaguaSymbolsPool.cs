using System;
using System.Collections.Generic;
using System.Linq;

namespace XYPortal.RandomStringProvider.RandomStringProvider;

internal class BaguaSymbolsPool : SymbolPool
{
    private static readonly char[] Chars = GetBaguaSymbols();
    
    public BaguaSymbolsPool(Random random)
        : base(random)
    {
    }
    
    public override char Get(params char[] ignores)
    {
        var chars = Chars.Where(c => !ignores.Contains(c)).ToArray();
        
        var value = chars[Next(chars.Length)];

        return value;
    }

    private static char[] GetBaguaSymbols()
    {
        int start = 0x2630;
        
        List<char> list = new List<char>();

        for (int i = 0; i < 8; i++)
        {
            list.Add((char)(start + i));
        }
        
        return list.ToArray();
    }
}
