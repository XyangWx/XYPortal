using System;
using System.Linq;

namespace XYPortal.RandomStringProvider.RandomStringProvider;

internal class EarthlyBranchesPool : SymbolPool
{
    private static readonly char[] Chars = "子丑寅卯辰巳午未申酉戌亥".ToCharArray();
    
    public EarthlyBranchesPool(Random random)
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
