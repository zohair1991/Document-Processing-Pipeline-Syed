
using DocumentProcessingPipeline.Models;
using DocumentProcessingPipeline.Services;

namespace DocumentProcessingPipeline.Tests;

public class ReviewPolicyTests
{
    private readonly ReviewPolicy _policy =
        new(new ResultValidator());

    [Fact]
    public void Apply_AmbiguousGraduationDate_ClearsDateAndRequiresReview()
    {
        var result = CreateResult(
            completionDate: "2024-04-03");

        var text = """
            MASTERZEUGNIS
            Date of graduation: 03/04/2024
            """;

        var actual = _policy.Apply(result, text);

        Assert.Null(actual.Abschlussdatum);
        Assert.True(actual.PruefungErforderlich);
        Assert.Contains("ambiguous", actual.Begruendung);
    }

    [Fact]
    public void Apply_TruncatedDocument_RequiresReview()
    {
        var result = CreateResult();

        var text = """
            BACHELORZEUGNIS
            Gesamtnote: 2,
            """;

        var actual = _policy.Apply(result, text);

        Assert.True(actual.PruefungErforderlich);
        Assert.Contains("truncated", actual.Begruendung);
    }

    [Fact]
    public void Apply_MultipleCertificates_RequiresReview()
    {
        var result = CreateResult();

        var text = """
            ZEUGNIS DER ALLGEMEINEN HOCHSCHULREIFE

            BACHELORZEUGNIS
            """;

        var actual = _policy.Apply(result, text);

        Assert.True(actual.PruefungErforderlich);
        Assert.Contains("Multiple document types", actual.Begruendung);
    }


    private static DocumentResult CreateResult(
        string? completionDate = null)
    {
        return new DocumentResult
        {
            Dokumenttyp = DocumentType.Hochschulzeugnis,
            NameInhaber = "Anna Beispiel",
            AusstellendeInstitution = "Universität Beispielstadt",
            Abschlussdatum = completionDate,
            Gesamtnote = "1,7",
            Notensystem = null,
            PruefungErforderlich = false,
            Begruendung = "Document fields were extracted."
        };
    }

}
