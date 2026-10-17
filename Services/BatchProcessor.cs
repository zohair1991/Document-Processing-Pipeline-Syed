using System.Text.Json;
using DocumentProcessingPipeline.Models;

namespace DocumentProcessingPipeline.Services;

public sealed class BatchProcessor
{
    private readonly IDocumentAnalyzer _analyzer;
    private readonly ResultValidator _validator;
    private readonly ReviewPolicy _reviewPolicy;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public BatchProcessor(
        IDocumentAnalyzer analyzer,
        ResultValidator validator,
        ReviewPolicy reviewPolicy)
    {
        _analyzer = analyzer;
        _validator = validator;
        _reviewPolicy = reviewPolicy;
    }

    public async Task RunAsync(
        string inputDirectory,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(inputDirectory))
        {
            throw new DirectoryNotFoundException(
                $"Input directory not found: {inputDirectory}");
        }

        Directory.CreateDirectory(outputDirectory);

        var files = Directory.GetFiles(
        inputDirectory,
        "*.txt",
        SearchOption.TopDirectoryOnly)
            .Where(file =>
                !Path.GetFileName(file).Equals(
                    "README.txt",
                    StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Array.Sort(files, StringComparer.OrdinalIgnoreCase);

        var summary = new List<object>();

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileName = Path.GetFileName(file);

            try
            {
                var documentText = await File.ReadAllTextAsync(
                    file,
                    cancellationToken);

                var result = await _analyzer.AnalyzeAsync(
                    documentText,
                    cancellationToken);

                var validationErrors = _validator.Validate(result);
                result = _reviewPolicy.Apply(
                    result,
                    documentText,
                    validationErrors);


                var resultPath = Path.Combine(
                    outputDirectory,
                    $"{Path.GetFileNameWithoutExtension(file)}.json");

                await File.WriteAllTextAsync(
                    resultPath,
                    JsonSerializer.Serialize(result, JsonOptions),
                    cancellationToken);

                summary.Add(new
                {
                    fileName,
                    success = true,
                    reviewRequired = result.PruefungErforderlich,
                    validationErrors,
                    resultFile = Path.GetFileName(resultPath)
                });

                Console.WriteLine(
                    $"Processed: {fileName} | " +
                    $"Review required: {result.PruefungErforderlich}");
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                var errorPath = Path.Combine(
                    outputDirectory,
                    $"{Path.GetFileNameWithoutExtension(file)}.error.json");

                var error = new
                {
                    fileName,
                    success = false,
                    error = ex.Message,
                    reviewRequired = true
                };

                await File.WriteAllTextAsync(
                    errorPath,
                    JsonSerializer.Serialize(error, JsonOptions),
                    cancellationToken);

                summary.Add(error);

                Console.Error.WriteLine(
                    $"Failed: {fileName} | {ex.Message}");
            }
        }

        var summaryPath = Path.Combine(
            outputDirectory,
            "summary.json");

        await File.WriteAllTextAsync(
            summaryPath,
            JsonSerializer.Serialize(summary, JsonOptions),
            cancellationToken);

        Console.WriteLine();
        Console.WriteLine($"Total documents: {files.Length}");
        Console.WriteLine($"Summary: {summaryPath}");
    }
}