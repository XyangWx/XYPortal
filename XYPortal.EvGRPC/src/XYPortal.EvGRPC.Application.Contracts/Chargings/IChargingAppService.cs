using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace XYPortal.EvGRPC.Chargings;

public interface IChargingAppService : IApplicationService
{
    Task<ChargingDto> GetAsync(string id);

    Task<List<ChargingDto>> GetListAsync(string vehicleId, int pageSize = 50, string? pageToken = null);

    Task<ChargingDto> CreateAsync(CreateUpdateChargingDto input);

    Task<ChargingDto> UpdateAsync(string id, CreateUpdateChargingDto input);

    Task DeleteAsync(string id);

    Task<CurrentBatteryDto> GetCurrentBatteryAsync(string vehicleId);
}
