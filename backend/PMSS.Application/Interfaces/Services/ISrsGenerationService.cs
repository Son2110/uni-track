using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.Srs;

namespace PMSS.Application.Interfaces.Services;

public interface ISrsGenerationService
{
    Task<ApiResponse<SrsDocumentDto>> GenerateSrsAsync(Guid projectId);
}
