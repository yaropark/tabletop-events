using TabletopEvents.Api.Models.Dtos.Responses;

namespace TabletopEvents.Api.Services.Contracts;

public interface ITemplateService
{
    IReadOnlyList<GameTemplateResponse> GetAll();

    /// <summary>Validates game-templates.json and upserts it, so adding a game is a data change only.</summary>
    void SyncFromFile(string path);
}
