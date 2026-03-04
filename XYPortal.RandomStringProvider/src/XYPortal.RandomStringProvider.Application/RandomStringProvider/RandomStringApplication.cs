using System;
using System.Text;
using System.Threading.Tasks;

namespace XYPortal.RandomStringProvider.RandomStringProvider;

public class RandomStringApplication : RandomStringProviderAppService, IRandomStringApplication
{
    public string MakeRandomString(RandomStringInput input)
    {
        ArgumentNullException.ThrowIfNull(input, nameof(input));
        
        StringBuilder sb = new StringBuilder();

        if (!string.IsNullOrEmpty(input.Prefix))
        {
            sb.Append(input.Prefix);
        }
        
        sb.Append(Provider.MakeRandomString(input));

        if (!string.IsNullOrEmpty(input.Suffix))
        {
            sb.Append(input.Suffix);
        }
        
        return sb.ToString();
    }

    public async Task<string> MakeRandomStringAsync(RandomStringInput input)
    {
        string value = MakeRandomString(input);
        
        return await Task.FromResult(value);
    }
}