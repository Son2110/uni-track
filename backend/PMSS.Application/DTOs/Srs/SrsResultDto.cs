namespace PMSS.Application.DTOs.Srs;

/// <summary>
/// Response DTO containing the generated SRS document
/// </summary>
public class SrsResultDto
{
    /// <summary>
    /// The project ID this SRS was generated for
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// The project name
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// The generated SRS content in Markdown format
    /// </summary>
    public string SrsContent { get; set; } = string.Empty;

    /// <summary>
    /// AI model used for generation
    /// </summary>
    public string ModelUsed { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp of generation
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}
