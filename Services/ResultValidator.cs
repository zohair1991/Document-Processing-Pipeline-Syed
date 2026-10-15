using System.Globalization;
using DocumentProcessingPipeline.Models;

namespace DocumentProcessingPipeline.Services;

public sealed class ResultValidator
{
    public IReadOnlyList<string> Validate(DocumentResult result)
    {
        var errors = new List<string>();

        if (!DocumentType.IsValid(result.Dokumenttyp))
            errors.Add("Invalid document type.");

        if (string.IsNullOrWhiteSpace(result.Begruendung))
            errors.Add("Review justification is missing.");

        if (result.Abschlussdatum is not null &&
            !DateTime.TryParseExact(
                result.Abschlussdatum,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _))
        {
            errors.Add("Completion date must be a valid YYYY-MM-DD date.");
        }

        return errors;
    }
}