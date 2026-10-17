using System.Globalization;
using DocumentProcessingPipeline.Models;

namespace DocumentProcessingPipeline.Services;

public sealed class ResultValidator
{
    public IReadOnlyList<string> Validate(DocumentResult result)
    {
        var errors = new List<string>();

        if (!DocumentType.IsValid(result.Dokumenttyp))
        {
            errors.Add("Invalid or unsupported document type.");
        }

        if (string.IsNullOrWhiteSpace(result.Begruendung))
        {
            errors.Add("Review justification is missing.");
        }

        if (result.Abschlussdatum is not null)
        {
            var validDate = DateTime.TryParseExact(
                result.Abschlussdatum,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _);

            if (!validDate)
            {
                errors.Add(
                    "Completion date must be a real date in YYYY-MM-DD format.");
            }
        }

        if (result.NameInhaber is not null &&
            string.IsNullOrWhiteSpace(result.NameInhaber))
        {
            errors.Add("Holder name is empty.");
        }

        if (result.AusstellendeInstitution is not null &&
            string.IsNullOrWhiteSpace(result.AusstellendeInstitution))
        {
            errors.Add("Issuing institution is empty.");
        }

        if (result.Gesamtnote is not null &&
            string.IsNullOrWhiteSpace(result.Gesamtnote))
        {
            errors.Add("Overall grade is empty.");
        }

        if (result.Notensystem is not null &&
            string.IsNullOrWhiteSpace(result.Notensystem))
        {
            errors.Add("Grading system is empty.");
        }

        return errors;
    }
}