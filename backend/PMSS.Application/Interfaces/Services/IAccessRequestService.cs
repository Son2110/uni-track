using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.AccessRequest;

namespace PMSS.Application.Interfaces.Services;

public interface IAccessRequestService
{
    Task<ApiResponse<PagedResult<AccessRequestDto>>> GetAllRequestsAsync(AccessRequestFilterParams filterParams);
    Task<ApiResponse<AccessRequestDto>> GetRequestByIdAsync(Guid id);
    Task<ApiResponse<AccessRequestDto>> CreateRequestAsync(CreateAccessRequestDto dto);
    Task<ApiResponse<AccessRequestDto>> UpdateRequestStatusAsync(Guid id, UpdateAccessRequestStatusDto dto);
    Task<ApiResponse<bool>> DeleteRequestAsync(Guid id);
}
