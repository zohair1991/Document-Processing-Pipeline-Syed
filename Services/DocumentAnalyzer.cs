using DocumentProcessingPipeline.Models;

namespace DocumentProcessingPipeline.Services;

public sealed class DocumentAnalyzer : IDocumentAnalyzer
{
    public Task<DocumentResult> AnalyzeAsync(
        string documentText,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentText);

        // Temporary implementation.
        // We will replace this with the actual LLM API call.

        throw new NotImplementedException(
            "LLM integration has not been configured yet.");
    }
}