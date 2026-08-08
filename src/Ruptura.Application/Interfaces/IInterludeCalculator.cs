using Ruptura.Domain.Entities;
using Ruptura.Shared.Guilds;

namespace Ruptura.Application.Interfaces;

public interface IInterludeCalculator
{
    InterludeProjection Project(
        GuildDerivedStats derived,
        IReadOnlyList<ResearchProject> research,
        IReadOnlyList<CraftingOrder> crafting,
        int days);
}
