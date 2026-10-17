
using System.Text.Json;
using System.Text.Json.Serialization;
using DocumentProcessingPipeline.Models;

namespace DocumentProcessingPipeline.Services;

public sealed class EvaluationRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task RunAsync(
        string projectDirectory,
        CancellationToken cancellationToken = default)
    {
        var expectedPath = Path.Combine(
            projectDirectory, "Evaluation", "expected-results.json");

        var outputDirectory = Path.Combine(projectDirectory, "Output");

        if (!File.Exists(expectedPath))
        {
            Console.Error.WriteLine($"Expected results not found: {expectedPath}");
            return;
        }

        if (!Directory.Exists(outputDirectory))
        {
            Console.Error.WriteLine($"Output directory not found: {outputDirectory}");
            return;
        }

        var json = await File.ReadAllTextAsync(expectedPath, cancellationToken);

        var expectedResults =
            JsonSerializer.Deserialize<List<ExpectedDocumentResult>>(
                json, JsonOptions) ?? [];

        if (expectedResults.Count == 0)
        {
            Console.Error.WriteLine("The evaluation dataset is empty.");
            return;
        }

        var metrics = new Dictionary<string, (int Correct, int Total)>
        {
            ["Classification"] = (0, 0),
            ["Review decision"] = (0, 0),
            ["Holder name"] = (0, 0),
            ["Issuing institution"] = (0, 0),
            ["Completion date"] = (0, 0),
            ["Overall grade"] = (0, 0),
            ["Grading system"] = (0, 0)
        };

        var evaluated = 0;
        var missingOutputs = 0;
        var invalidOutputs = 0;

        Console.WriteLine("Document Evaluation");
        Console.WriteLine(new string('-', 75));

        foreach (var expected in expectedResults)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var outputFileName =
                Path.GetFileNameWithoutExtension(expected.FileName) + ".json";

            var outputPath = Path.Combine(outputDirectory, outputFileName);

            if (!File.Exists(outputPath))
            {
                missingOutputs++;
                Console.WriteLine($"MISSING OUTPUT: {expected.FileName}");
                continue;
            }

            ActualDocumentResult? actual;

            try
            {
                var outputJson = await File.ReadAllTextAsync(
                    outputPath, cancellationToken);

                actual = JsonSerializer.Deserialize<ActualDocumentResult>(
                    outputJson, JsonOptions);

                if (actual is null)
                {
                    invalidOutputs++;
                    Console.WriteLine($"INVALID OUTPUT: {expected.FileName}");
                    continue;
                }
            }
            catch (JsonException ex)
            {
                invalidOutputs++;
                Console.WriteLine(
                    $"INVALID JSON: {expected.FileName} ({ex.Message})");
                continue;
            }

            evaluated++;

            var mismatches = new List<string>();

            Compare(
                "Type",
                expected.ExpectedDocumentType,
                actual.Dokumenttyp,
                "Classification",
                metrics,
                mismatches);

            Compare(
                "Review",
                expected.ExpectedReviewRequired,
                actual.PruefungErforderlich,
                "Review decision",
                metrics,
                mismatches);

            Compare(
                "Holder",
                expected.ExpectedHolderName,
                actual.NameInhaber,
                "Holder name",
                metrics,
                mismatches);

            Compare(
                "Institution",
                expected.ExpectedIssuingInstitution,
                actual.AusstellendeInstitution,
                "Issuing institution",
                metrics,
                mismatches);

            Compare(
                "Completion date",
                expected.ExpectedCompletionDate,
                actual.Abschlussdatum,
                "Completion date",
                metrics,
                mismatches);

            Compare(
                "Grade",
                expected.ExpectedOverallGrade,
                actual.Gesamtnote,
                "Overall grade",
                metrics,
                mismatches);

            Compare(
                "Grading system",
                expected.ExpectedGradingSystem,
                actual.Notensystem,
                "Grading system",
                metrics,
                mismatches);

            var status = mismatches.Count == 0 ? "PASS" : "FAIL";
            Console.WriteLine($"{status}: {expected.FileName}");

            foreach (var mismatch in mismatches)
            {
                Console.WriteLine($"  {mismatch}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("Evaluation Summary");
        Console.WriteLine(new string('-', 75));
        Console.WriteLine($"Expected documents: {expectedResults.Count}");
        Console.WriteLine($"Outputs evaluated:  {evaluated}");
        Console.WriteLine($"Missing outputs:    {missingOutputs}");
        Console.WriteLine($"Invalid outputs:    {invalidOutputs}");
        Console.WriteLine();

        foreach (var metric in metrics)
        {
            var (correct, total) = metric.Value;
            var percentage = total == 0 ? 0 : correct * 100.0 / total;

            Console.WriteLine(
                $"{metric.Key,-22}: {correct}/{total} ({percentage:F1}%)");
        }

        var allCorrect = metrics.Values.Sum(m => m.Correct);
        var allChecks = metrics.Values.Sum(m => m.Total);
        var overallPercentage = allChecks == 0
            ? 0
            : allCorrect * 100.0 / allChecks;

        Console.WriteLine();
        Console.WriteLine(
            $"Overall field/check accuracy: {allCorrect}/{allChecks} " +
            $"({overallPercentage:F1}%)");
    }

    private static void Compare<T>(
        string label,
        T expected,
        T actual,
        string metricName,
        Dictionary<string, (int Correct, int Total)> metrics,
        List<string> mismatches)
    {
        var equal = EqualityComparer<T>.Default.Equals(expected, actual);

        var current = metrics[metricName];
        metrics[metricName] = (
            current.Correct + (equal ? 1 : 0),
            current.Total + 1);

        if (!equal)
        {
            mismatches.Add(
                $"{label}: expected {Format(expected)}, actual {Format(actual)}");
        }
    }

    private static string Format<T>(T value) =>
        value is null ? "null" : $"\"{value}\"";

    private sealed class ActualDocumentResult
    {
        [JsonPropertyName("dokumenttyp")]
        public string Dokumenttyp { get; init; } = "";

        [JsonPropertyName("pruefung_erforderlich")]
        public bool PruefungErforderlich { get; init; }

        [JsonPropertyName("name_inhaber")]
        public string? NameInhaber { get; init; }

        [JsonPropertyName("ausstellende_institution")]
        public string? AusstellendeInstitution { get; init; }

        [JsonPropertyName("abschlussdatum")]
        public string? Abschlussdatum { get; init; }

        [JsonPropertyName("gesamtnote")]
        public string? Gesamtnote { get; init; }

        [JsonPropertyName("notensystem")]
        public string? Notensystem { get; init; }
    }
}
