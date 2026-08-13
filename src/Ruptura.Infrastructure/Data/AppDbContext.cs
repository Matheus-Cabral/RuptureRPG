using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Identity;

namespace Ruptura.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<InviteCode> InviteCodes => Set<InviteCode>();
    public DbSet<CharacterSheet> CharacterSheets => Set<CharacterSheet>();
    public DbSet<GuildSheet> GuildSheets => Set<GuildSheet>();
    public DbSet<GuildBuilding> GuildBuildings => Set<GuildBuilding>();
    public DbSet<GuildStaff> GuildStaff => Set<GuildStaff>();
    public DbSet<ResearchProject> ResearchProjects => Set<ResearchProject>();
    public DbSet<CraftingOrder> CraftingOrders => Set<CraftingOrder>();
    public DbSet<Expedition> Expeditions => Set<Expedition>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignMembership> CampaignMemberships => Set<CampaignMembership>();
    public DbSet<CatalogEntry> CatalogEntries => Set<CatalogEntry>();
    public DbSet<CharacterJournalEntry> CharacterJournalEntries => Set<CharacterJournalEntry>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Creature> Creatures => Set<Creature>();
    public DbSet<Npc> Npcs => Set<Npc>();
    public DbSet<Encounter> Encounters => Set<Encounter>();
    public DbSet<Reward> Rewards => Set<Reward>();
    public DbSet<CombatSession> CombatSessions => Set<CombatSession>();
    public DbSet<Arc> Arcs => Set<Arc>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<SessionLog> SessionLogs => Set<SessionLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
