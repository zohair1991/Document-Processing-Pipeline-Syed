using System.Text.RegularExpressions;
using DocumentProcessingPipeline.Models;

namespace DocumentProcessingPipeline.Services;

public sealed class ReviewPolicy
{
    private readonly ResultValidator _validator;

    public ReviewPolicy(ResultValidator validator)
    {
        _validator = validator;
    }


    public DocumentResult Apply(
        DocumentResult result,
        string documentText,
        IReadOnlyList<string>? validationErrors = null)
    {
        var reasons = new List<string>();

        if (result.PruefungErforderlich)
        {
            reasons.Add("The model requested human review.");
        }

        var errors = validationErrors ?? _validator.Validate(result);
        reasons.AddRange(errors);

        if (ContainsMultipleDocuments(documentText))
        {
            reasons.Add(
                "Multiple document types or certificate headings " +
                "were detected in one input file.");
        }

        var ambiguousDate = ContainsAmbiguousNumericDate(documentText);

        if (ambiguousDate)
        {
            reasons.Add(
                "The completion date is ambiguous and cannot be " +
                "normalized reliably without human review.");
        }

        if (ContainsTruncationSignals(documentText))
        {
            reasons.Add(
                "The source contains signs of an incomplete or truncated document.");
        }

        if (ContainsOcrWarningSignals(documentText))
        {
            reasons.Add(
                "The source contains explicit OCR-quality warnings or " +
                "strong signs of OCR corruption.");
        }

        var distinctReasons = reasons
            .Where(reason => !string.IsNullOrWhiteSpace(reason))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return result with
        {
            // Do not keep a guessed date when the source date is ambiguous.
            Abschlussdatum = ambiguousDate ? null : result.Abschlussdatum,

            PruefungErforderlich = distinctReasons.Count > 0,

            Begruendung = distinctReasons.Count == 0
                ? result.Begruendung
                : string.Join(" ", distinctReasons)
        };
    }


    private static bool ContainsMultipleDocuments(string text)
    {
        var headings = new[]
        {
        "ZEUGNIS DER ALLGEMEINEN HOCHSCHULREIFE",
        "ZEUGNIS DER FACHHOCHSCHULREIFE",
        "BACHELORZEUGNIS",
        "MASTERZEUGNIS",
        "ZEUGNIS ÜBER DIE BACHELORPRÜFUNG",
        "TEST REPORT FORM",
        "LANGUAGE CERTIFICATE",
        "ZERTIFIKAT"
    };

        var headingCount = headings.Count(heading =>
            Regex.IsMatch(
                text,
                $@"(?im)^\s*{Regex.Escape(heading)}\s*$"));

        return headingCount > 1;
    }





    private static bool ContainsAmbiguousNumericDate(string text)
    {
        var lines = text.Split(
            new[] { "\r\n", "\n" },
            StringSplitOptions.None);

        foreach (var line in lines)
        {
            // Ignore dates that are explicitly birth dates.
            if (Regex.IsMatch(
                line,
                @"\b(Date of Birth|Geburtsdatum)\b",
                RegexOptions.IgnoreCase))
            {
                continue;
            }

            // Only consider numeric dates on lines that appear to describe
            // a graduation, completion, examination, or test date.
            if (!Regex.IsMatch(
                line,
                @"\b(Abschluss|Graduation|Completion|Prüfung|Pruefung|Exam|Test)\w*\b",
                RegexOptions.IgnoreCase))
            {
                continue;
            }

            var matches = Regex.Matches(
                line,
                @"(?<!\d)(\d{1,2})/(\d{1,2})/(\d{4})(?!\d)");

            foreach (Match match in matches)
            {
                if (int.TryParse(match.Groups[1].Value, out var first) &&
                    int.TryParse(match.Groups[2].Value, out var second) &&
                    first <= 12 && second <= 12 && first != second)
                {
                    return true;
                }
            }
        }

        return false;
    }




    private static bool ContainsTruncationSignals(string text)
    {
        var normalized = text.ToLowerInvariant();

        if (normalized.Contains(
                "seite 2 von 2 fehlt",
                StringComparison.Ordinal))
        {
            return true;
        }

        if (Regex.IsMatch(
                text,
                @"(?:Note|Gesamtnote|Modulübersicht)[^\r\n]*\d,\s*$",
                RegexOptions.IgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static bool ContainsOcrWarningSignals(string text)
    {
        var normalized = text.ToLowerInvariant();

        return normalized.Contains(
                   "scan-qualitat unzureichend",
                   StringComparison.Ordinal)
               || normalized.Contains(
                   "ocr-qualität unzureichend",
                   StringComparison.Ordinal);
    }
}