using TabletopEvents.Api.Models.Entities;

namespace TabletopEvents.Api.Repositories.Contracts;

public interface ITemplateRepository
{
    IReadOnlyList<GameTemplate> GetAll();

    GameTemplate? Get(string key);

    /// <summary>Inserts new templates and updates existing ones by key, in one transaction.</summary>
    void Upsert(IEnumerable<GameTemplate> templates);
}
