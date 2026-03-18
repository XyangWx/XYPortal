using System;
using System.Collections.Generic;
using System.Linq;

namespace XYPortal.RandomStringProvider.RandomStringProvider;

internal class UnicodeMiscellaneousPool : SymbolPool
{
    private static readonly char[] Chars = GetMiscellaneousSymbols();
    
    public UnicodeMiscellaneousPool(Random random)
        : base(random)
    {
    }
    
    public override char Get(params char[] ignores)
    {
        var chars = Chars.Where(c => !ignores.Contains(c)).ToArray();
        
        if (chars.Length == 0)
        {
            throw new InvalidOperationException("No available characters after filtering ignores");
        }
        
        var value = chars[Next(chars.Length)];

        return value;
    }

    private static char[] GetMiscellaneousSymbols()
    {
        int start1 = 0x2600;  // Miscellaneous Symbols (☀ to ⛰)
        int start2 = 0x2638;  // Resume and Select symbols (☸ to ⛿)

        List<char> chars = [];

        // Miscellaneous Symbols range (0x2600 - 0x26FF)
        for (int i = 0; i < 0x30; i++)
        {
            chars.Add((char)(start1 + i));
        }

        // Resume and Select symbols (0x2638 - 0x27BF)
        for (int i = 0; i < 0x88; i++)
        {
            chars.Add((char)(start2 + i));
        }
        
        return chars.ToArray();
    }
}
