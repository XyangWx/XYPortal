using System;
using System.Collections.Generic;
using System.Linq;

namespace XYPortal.RandomStringProvider.RandomStringProvider;

internal class UnicodeMiscellaneousPool : SymbolPool
{
    private static readonly char[] Chars = "".ToCharArray();
    
    public UnicodeMiscellaneousPool(Random random)
        : base(random)
    {
    }
    
    public override char Get(params char[] ignores)
    {
        var chars = Chars.Where(c => !ignores.Contains(c)).ToArray();
        
        var value = chars[Next(chars.Length)];

        return value;
    }

    private static char[] GetMiscellaneousSymbols()
    {
        int start1 = 0x2600;
        int start2 = 0x2638;

        List<char> chars = [];

        for (int i = 0; i < 0x30; i++)
        {
            chars.Add((char)(start1 + 1));
        }

        // 避开八卦段
        for (int i = 0; i < 0xc8; i++)
        {
            chars.Add((char)(start2 + 1));
        }
        
        return chars.ToArray();
    }
}
