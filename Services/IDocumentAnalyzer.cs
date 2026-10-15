using DocumentProcessingPipeline.Models;

namespace DocumentProcessingPipeline.Services;

public interface IDocumentAnalyzer
{
    Task<DocumentResult> AnalyzeAsync(
        string documentText,
        CancellationToken cancellationToken = default);
}