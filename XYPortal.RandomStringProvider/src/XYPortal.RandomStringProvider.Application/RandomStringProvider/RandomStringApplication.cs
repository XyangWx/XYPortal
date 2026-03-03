using System.Threading.Tasks;

namespace XYPortal.RandomStringProvider.RandomStringProvider;

public class RandomStringApplication : RandomStringProviderAppService, IRandomStringApplication
{
    public string MakeRandomString(RandomStringInput input)
    {
        return Provider.MakeRandomString(input);
    }

    public async Task<string> MakeRandomStringAsync(RandomStringInput input)
    {
        string value = Provider.MakeRandomString(input);
        
        return await Task.FromResult(value);
    }
}