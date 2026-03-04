using System;

namespace XYPortal.RandomStringProvider.RandomStringProvider;

internal abstract class SymbolPool : ISymbolPool
{
    private readonly Random _random;
    
    protected SymbolPool(Random random)
    {
        _random = random;
    }

    protected int Next(int min, int max)
    {
        return _random.Next(min, max);
    }

    protected int Next(int max)
    {
        return _random.Next(max);
    }

    public abstract char Get(params char[] ignores);
}