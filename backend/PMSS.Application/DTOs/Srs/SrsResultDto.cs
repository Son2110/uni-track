namespace PMSS.Application.DTOs.Srs;

public class SrsResultDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string SrsContent { get; set; } = string.Empty;
    public string ModelUsed { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string? FileName { get; set; }
    public string? DownloadUrl { get; set; }
}
