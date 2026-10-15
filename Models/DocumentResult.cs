using System.Text.Json.Serialization;
namespace DocumentProcessingPipeline.Models;

public sealed record DocumentResult
{
    [JsonPropertyName("dokumenttyp")]
    public required string Dokumenttyp { get; init; }

    [JsonPropertyName("name_inhaber")]
    public string? NameInhaber { get; init; }

    [JsonPropertyName("ausstellende_institution")]
    public string? AusstellendeInstitution { get; init; }

    [JsonPropertyName("abschlussdatum")]
    public string? Abschlussdatum { get; init; }

    [JsonPropertyName("gesamtnote")]
    public string? Gesamtnote { get; init; }

    [JsonPropertyName("notensystem")]
    public string? Notensystem { get; init; }

    [JsonPropertyName("pruefung_erforderlich")]
    public bool PruefungErforderlich { get; init; }

    [JsonPropertyName("begruendung")]
    public required string Begruendung { get; init; }
}