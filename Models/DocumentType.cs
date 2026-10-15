namespace DocumentProcessingPipeline.Models;

public static class DocumentType
{
    public const string Hochschulzugangsberechtigung =
        "hochschulzugangsberechtigung";

    public const string Hochschulzeugnis = "hochschulzeugnis";

    public const string Sprachnachweis = "sprachnachweis";

    public const string Sonstiges = "sonstiges";

    public static bool IsValid(string? value) =>
        value is Hochschulzugangsberechtigung
            or Hochschulzeugnis
            or Sprachnachweis
            or Sonstiges;
}