using System;
using System.Linq;

namespace XYPortal.RandomStringProvider.RandomStringProvider;

internal class HeavenlyStemsPool : SymbolPool
{
    private static readonly char[] Chars = "甲乙丙丁戊己庚辛壬癸".ToCharArray();
    
    public HeavenlyStemsPool(Random random)
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
