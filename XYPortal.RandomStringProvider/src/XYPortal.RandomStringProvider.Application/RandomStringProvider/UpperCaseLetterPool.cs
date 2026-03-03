using System;
using System.Linq;

namespace XYPortal.RandomStringProvider.RandomStringProvider;

internal class UpperCaseLetterPool : SymbolPool
{
    private static readonly char[] _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    
    public UpperCaseLetterPool(Random random) : base(random)
    {
    }


    public override string Get(params string[] ignores)
    {
        var chars = _chars.Where(C => !ignores.Any(I => I.Equals($"{C}"))).ToArray();
        
        var value = chars[Next(chars.Length)];

        return $"{value}";
    }
}