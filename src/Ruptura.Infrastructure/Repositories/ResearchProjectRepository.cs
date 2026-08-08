using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class ResearchProjectRepository(AppDbContext db)
    : BaseRepository<ResearchProject>(db), IResearchProjectRepository
{
    public async Task<IEnumerable<ResearchProject>> GetByGuildAsync(Guid guildSheetId, CancellationToken ct = default) =>
        await Set.Where(r => r.GuildSheetId == guildSheetId).ToListAsync(ct);
}
