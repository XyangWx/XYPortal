using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace XYPortal.EvGRPC.Vehicles;

public interface IVehicleAppService : IApplicationService
{
    Task<VehicleDto> GetAsync(string id);

    Task<List<VehicleDto>> GetListAsync(int pageSize = 50, string? pageToken = null);

    Task<VehicleDto> CreateAsync(CreateUpdateVehicleDto input);

    Task<VehicleDto> UpdateAsync(string id, CreateUpdateVehicleDto input);

    Task DeleteAsync(string id);
}
