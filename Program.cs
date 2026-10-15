using DocumentProcessingPipeline.Services;

var projectDirectory = Directory.GetCurrentDirectory();

var inputDirectory = Path.Combine(
    projectDirectory,
    "Input");

var outputDirectory = Path.Combine(
    projectDirectory,
    "Output");

var analyzer = new DocumentAnalyzer();
var validator = new ResultValidator();
var reviewPolicy = new ReviewPolicy();

var processor = new BatchProcessor(
    analyzer,
    validator,
    reviewPolicy);

await processor.RunAsync(
    inputDirectory,
    outputDirectory);