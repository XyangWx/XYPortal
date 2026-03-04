using System;
using System.Linq;

namespace XYPortal.RandomStringProvider.RandomStringProvider;

internal class ChineseCapitalNumbersPool : SymbolPool
{
    private static readonly char[] Chars = "零壹贰叁肆伍陆柒捌玖拾佰仟萬亿兆".ToCharArray();
    
    public ChineseCapitalNumbersPool(Random random)
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
