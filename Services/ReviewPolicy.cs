using DocumentProcessingPipeline.Models;

namespace DocumentProcessingPipeline.Services;

public sealed class ReviewPolicy
{
    public DocumentResult Apply(DocumentResult result)
    {
        var errors = new ResultValidator().Validate(result);

        var requiresReview =
            result.PruefungErforderlich ||
            errors.Count > 0;

        var reason = result.Begruendung;

        if (errors.Count > 0)
        {
            reason += " Validation issues: " +
                      string.Join(" ", errors);
        }

        return result with
        {
            PruefungErforderlich = requiresReview,
            Begruendung = reason
        };
    }
}