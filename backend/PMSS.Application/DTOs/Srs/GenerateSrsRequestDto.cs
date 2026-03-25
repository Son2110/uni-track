using System.ComponentModel.DataAnnotations;

namespace PMSS.Application.DTOs.Srs;

/// <summary>
/// Optional request body for SRS generation with custom parameters
/// </summary>
public class GenerateSrsRequestDto
{
    /// <summary>
    /// Optional custom project name override. If not provided, uses the project name from PMSS.
    /// </summary>
    public string? ProjectName { get; set; }

    /// <summary>
    /// Optional additional context or instructions for the AI generator
    /// Example: "Focus on mobile features", "Include API integration requirements"
    /// </summary>
    [MaxLength(1000)]
    public string? AdditionalContext { get; set; }
}
