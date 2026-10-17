# AI-Based Document Verification Pipeline

A C#/.NET 10 console application that classifies document text, extracts structured information using Claude, validates the results, and identifies documents requiring human review.

## Requirements

- .NET 10 SDK
- Anthropic API key for AI-powered processing

## Setup and execution

Configure the API key in PowerShell:

```powershell
$env:ANTHROPIC_API_KEY = "your-api-key"
```

Optionally configure the model. The default is `claude-sonnet-4-6`.

```powershell
$env:ANTHROPIC_MODEL = "claude-sonnet-4-6"
```

Build and run the application:

```powershell
dotnet restore
dotnet build
dotnet run
```

Place input `.txt` documents in `Input/`. The application writes one JSON result per document to `Output/`, along with `summary.json`. If processing an individual document fails, an `.error.json` file is written and processing continues with the remaining documents.

## Evaluation and tests

Evaluate the existing outputs against `Evaluation/expected-results.json`:

```powershell
dotnet run -- --evaluate
```

The evaluator compares classification, review decisions, and extracted fields against the expected records. It does not call the AI API; outputs must already exist in `Output/`.

Run the automated unit tests:

```powershell
dotnet test .\DocumentProcessingPipeline.Tests\DocumentProcessingPipeline.Tests.csproj
```

The seven unit tests cover validation rules and human-review policy behavior without API calls.

## Design decisions

**AI responsibilities:** Claude classifies documents as `hochschulzugangsberechtigung`, `hochschulzeugnis`, `sprachnachweis`, or `sonstiges`; extracts the requested fields; and provides a justification for its review decision.

**Code responsibilities:** C# handles batch orchestration, JSON serialization, validation, and deterministic review checks. This separation makes critical rules more predictable and testable while allowing the model to handle varied document wording and formats.

**Human-review policy:** Review is required when requested by the model, when validation fails, or when deterministic checks detect risk signals such as multiple certificates in one file, ambiguous numeric completion dates, truncation, or OCR-quality warnings. Missing information alone does not automatically trigger review.

A successfully processed document is not necessarily a verified document. Human review remains important when extracted information is uncertain.

## Limitations

- The schema supports one result per input file, so multiple certificates in one file cannot be represented independently.
- Deterministic checks cover known risk patterns and cannot detect every extraction error.
- The application processes text files; OCR is not performed by the pipeline.
- Evaluation quality depends on the accuracy and consistency of the expected records. Exact string comparisons may count formatting differences as mismatches.
- The current implementation does not provide comprehensive production-grade monitoring or API cost tracking.

## Future improvements

1. Enforce structured model output and strengthen automated tests.
2. Expand the independently verified evaluation dataset.
3. Add retries, timeouts, and rate-limit handling.
4. Track API latency and cost per document.
5. Support multiple documents per file and integrate OCR.
6. Add observability, data-retention controls, and privacy safeguards before processing real applicant documents.
