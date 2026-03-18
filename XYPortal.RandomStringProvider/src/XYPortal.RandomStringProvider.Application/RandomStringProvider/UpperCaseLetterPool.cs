using System;
using System.Linq;

namespace XYPortal.RandomStringProvider.RandomStringProvider;

internal class UpperCaseLetterPool : SymbolPool
{
    private static readonly char[] Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    
    public UpperCaseLetterPool(Random random) : base(random)
    {
    }


    public override char Get(params char[] ignores)
    {
        var chars = Chars.Where(c => !ignores.Contains(c)).ToArray();
        
        var value = chars[Next(chars.Length)];

        return value;
    }
}