
namespace DocumentProcessingPipeline.Models;

public sealed class ExpectedDocumentResult
{
    public required string FileName { get; init; }

    public required string ExpectedDocumentType { get; init; }

    public required bool ExpectedReviewRequired { get; init; }

    public string? ExpectedHolderName { get; init; }

    public string? ExpectedIssuingInstitution { get; init; }

    public string? ExpectedCompletionDate { get; init; }

    public string? ExpectedOverallGrade { get; init; }

    public string? ExpectedGradingSystem { get; init; }
}
