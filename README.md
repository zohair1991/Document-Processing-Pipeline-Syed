# AI-Based Document Verification Pipeline

A C#/.NET console application that classifies document text, extracts relevant information using Claude, validates the structured result, and identifies documents that require human review.

## Requirements

- .NET 10 SDK
- An Anthropic API key

## Run the application

1. Clone the repository and open the project directory.
2. Configure the API key as an environment variable.

**PowerShell:**

```powershell
$env:ANTHROPIC_API_KEY = "your-api-key"
```

3. Optionally configure the model. The default is `claude-sonnet-4-6`.

```powershell
$env:ANTHROPIC_MODEL = "claude-sonnet-4-6"
```

4. Run the application:

```powershell
dotnet restore
dotnet build
dotnet run
```

Place the input `.txt` documents in the `Input/` directory. Results are written to `Output/`, with one JSON result per document and a `summary.json` overview.

## Design decisions

**AI responsibilities:** Claude classifies each document into `hochschulzugangsberechtigung`, `hochschulzeugnis`, `sprachnachweis`, or `sonstiges`, extracts the requested fields, normalizes dates where possible, and provides a justification for its review decision.

**Code responsibilities:** C# handles batch orchestration, JSON serialization, schema validation, and deterministic review checks. Keeping these checks outside the model makes critical rules easier to test and explain.

## Human-review policy

A document is flagged when the model requests review, validation fails, or deterministic checks detect relevant risk signals, including:

- Multiple certificates in one input file.
- An ambiguous numeric completion or examination date.
- Signs of truncation or incomplete content.
- Explicit OCR-quality warnings or strong OCR-corruption signals.

Missing information alone does not automatically require review when the source does not provide that information. For example, a missing overall grade is represented as `null`.

The policy favors escalation when the result may be unreliable. A successful processing status means the pipeline produced an output; it does not mean the document has been independently verified.

## Current limitations

- The current schema produces one result per input file, so multiple certificates in one file cannot be represented independently.
- The system relies on model output and a limited set of deterministic checks. Subtle extraction errors may go undetected.
- OCR is not performed by this application; it processes text already available in the input files.
- The review rules cover known risk patterns and are not a substitute for comprehensive evaluation.

## Next steps for production

1. Enforce a structured output schema and add automated tests for extraction and validation.
2. Build an evaluation set with expected results and measure classification, extraction, and review-decision accuracy.
3. Add retries, timeouts, rate-limit handling, and more detailed error reporting.
4. Track per-document latency and API cost.
5. Support multiple documents within a single file and improve OCR integration.
6. Add observability, data-retention controls, and privacy safeguards before processing real applicant documents.
