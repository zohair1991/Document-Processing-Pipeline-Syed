// using DocumentProcessingPipeline.Services;

// var projectDirectory = Directory.GetCurrentDirectory();

// var inputDirectory = Path.Combine(
//     projectDirectory,
//     "Input");

// var outputDirectory = Path.Combine(
//     projectDirectory,
//     "Output");

// var analyzer = new DocumentAnalyzer();
// var validator = new ResultValidator();
// var reviewPolicy = new ReviewPolicy(validator);

// var processor = new BatchProcessor(
//     analyzer,
//     validator,
//     reviewPolicy);

// await processor.RunAsync(
//     inputDirectory,
//     outputDirectory);


using DocumentProcessingPipeline.Services;

var projectDirectory = Directory.GetCurrentDirectory();

if (args.Contains("--evaluate", StringComparer.OrdinalIgnoreCase))
{
    var evaluator = new EvaluationRunner();
    await evaluator.RunAsync(projectDirectory);
    return;
}

var inputDirectory = Path.Combine(projectDirectory, "Input");
var outputDirectory = Path.Combine(projectDirectory, "Output");

var analyzer = new DocumentAnalyzer();
var validator = new ResultValidator();
var reviewPolicy = new ReviewPolicy(validator);
var processor = new BatchProcessor(analyzer, validator, reviewPolicy);

await processor.RunAsync(inputDirectory, outputDirectory);
