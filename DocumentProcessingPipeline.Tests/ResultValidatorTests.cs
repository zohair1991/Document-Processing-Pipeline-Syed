
using DocumentProcessingPipeline.Models;
using DocumentProcessingPipeline.Services;

namespace DocumentProcessingPipeline.Tests;

public class ResultValidatorTests
{
    private readonly ResultValidator _validator = new();

    [Fact]
    public void Validate_ValidDate_DoesNotReturnDateError()
    {
        var result = CreateResult(completionDate: "2024-06-17");

        var errors = _validator.Validate(result);

        Assert.DoesNotContain(
            errors,
            error => error.Contains("Completion date"));
    }

    [Fact]
    public void Validate_InvalidCalendarDate_ReturnsDateError()
    {
        var result = CreateResult(completionDate: "2024-02-30");

        var errors = _validator.Validate(result);

        Assert.Contains(
            errors,
            error => error.Contains("Completion date"));
    }

    [Fact]
    public void Validate_UnsupportedDocumentType_ReturnsError()
    {
        var result = CreateResult(documentType: "unknown");

        var errors = _validator.Validate(result);

        Assert.Contains(
            errors,
            error => error.Contains("document type"));
    }

    [Fact]
    public void Validate_EmptyJustification_ReturnsError()
    {
        var result = CreateResult(justification: "");

        var errors = _validator.Validate(result);

        Assert.Contains(
            errors,
            error => error.Contains("justification"));
    }

    private static DocumentResult CreateResult(
        string documentType = DocumentType.Hochschulzeugnis,
        string? completionDate = null,
        string justification = "Document fields were extracted.")
    {
        return new DocumentResult
        {
            Dokumenttyp = documentType,
            NameInhaber = "Anna Beispiel",
            AusstellendeInstitution = "Universität Beispielstadt",
            Abschlussdatum = completionDate,
            Gesamtnote = "1,7",
            Notensystem = null,
            PruefungErforderlich = false,
            Begruendung = justification
        };
    }
}
