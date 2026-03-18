using System;
using System.Linq;

namespace XYPortal.RandomStringProvider.RandomStringProvider;

internal class ArabicNumeralPool : SymbolPool
{
    private static readonly char[] Chars = "0123456789".ToCharArray();
    
    public ArabicNumeralPool(Random random) : base(random)
    {
    }

    public override char Get(params char[] ignores)
    {
        var chars = Chars.Where(c => !ignores.Contains(c)).ToArray();
        
        var value = chars[Next(chars.Length)];

        return value;
    }
}