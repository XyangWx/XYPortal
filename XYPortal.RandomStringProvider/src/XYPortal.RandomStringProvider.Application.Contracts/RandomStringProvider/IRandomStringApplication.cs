using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace XYPortal.RandomStringProvider.RandomStringProvider;

public interface IRandomStringApplication : IApplicationService
{
    string MakeRandomString(RandomStringInput input);
    Task<string> MakeRandomStringAsync(RandomStringInput input);
}