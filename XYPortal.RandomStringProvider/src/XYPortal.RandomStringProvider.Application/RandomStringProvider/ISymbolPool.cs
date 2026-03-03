namespace XYPortal.RandomStringProvider.RandomStringProvider;

internal interface ISymbolPool
{
    string Get(params string[] ignores);
}