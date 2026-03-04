using System;
using System.Linq;

namespace XYPortal.RandomStringProvider.RandomStringProvider;

internal class LowerCaseLetterPool : SymbolPool
{
    private static readonly char[] Chars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
    
    public LowerCaseLetterPool(Random random)
        : base(random)
    {
    }
    
    public override char Get(params char[] ignores)
    {
        var chars = Chars.Where(c => !ignores.Contains(c)).ToArray();
        
        var value = chars[Next(chars.Length)];

        return value;
    }
}