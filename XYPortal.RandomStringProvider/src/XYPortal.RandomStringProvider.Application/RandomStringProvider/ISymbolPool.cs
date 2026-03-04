namespace XYPortal.RandomStringProvider.RandomStringProvider;

internal interface ISymbolPool
{
    char Get(params char[] ignores);
}