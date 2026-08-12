using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ruptura.Application.Interfaces;
using Ruptura.Application.Services;
using Ruptura.Application.Validators.Auth;
using Ruptura.Application.Validators.Catalog;
using Ruptura.Application.Validators.Campaigns;
using Ruptura.Application.Validators.CharacterSheets;
using Ruptura.Application.Validators.Guilds;
using Ruptura.Application.Validators.Journal;
using Ruptura.Infrastructure.Data;
using Ruptura.Infrastructure.Identity;
using Ruptura.Infrastructure.Repositories;
using Ruptura.Infrastructure.Services;
using Ruptura.Infrastructure.Settings;
using Ruptura.Shared.Auth;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Catalog;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Guilds;
using Ruptura.Shared.Journal;

namespace Ruptura.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Identity
        services.AddIdentityCore<ApplicationUser>(opts =>
            {
                opts.Password.RequireDigit = true;
                opts.Password.RequireLowercase = true;
                opts.Password.RequireUppercase = true;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequiredLength = 8;
                opts.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // Settings
        services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
        services.Configure<MediaSettings>(configuration.GetSection(nameof(MediaSettings)));

        // Core services
        services.AddSingleton<JwtService>();
        services.AddSingleton<ICharacterStatsCalculator, CharacterStatsCalculator>();
        services.AddSingleton<IGuildStatsCalculator, GuildStatsCalculator>();  // pure & stateless, like CharacterStatsCalculator
        services.AddSingleton<IInterludeCalculator, InterludeCalculator>();    // pure & stateless
        services.AddSingleton<ICreatureStatsCalculator, CreatureStatsCalculator>(); // pure & stateless
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();

        // Application services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IInviteCodeService, InviteCodeService>();
        services.AddScoped<ICampaignService, CampaignService>();
        services.AddScoped<ICatalogEntryService, CatalogEntryService>();
        services.AddScoped<ICharacterSheetService, CharacterSheetService>();
        services.AddScoped<IGuildSheetService, GuildSheetService>();
        services.AddScoped<IJournalEntryService, JournalEntryService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICampaignDashboardService, CampaignDashboardService>();
        services.AddScoped<ICreatureService, CreatureService>();
        services.AddScoped<INpcService, NpcService>();

        // Repositories
        services.AddScoped<IInviteCodeRepository, InviteCodeRepository>();
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<ICampaignMembershipRepository, CampaignMembershipRepository>();
        services.AddScoped<ICatalogEntryRepository, CatalogEntryRepository>();
        services.AddScoped<ICharacterSheetRepository, CharacterSheetRepository>();
        services.AddScoped<ICharacterJournalEntryRepository, CharacterJournalEntryRepository>();
        services.AddScoped<IGuildSheetRepository, GuildSheetRepository>();
        services.AddScoped<IGuildBuildingRepository, GuildBuildingRepository>();
        services.AddScoped<IGuildStaffRepository, GuildStaffRepository>();
        services.AddScoped<IResearchProjectRepository, ResearchProjectRepository>();
        services.AddScoped<ICraftingOrderRepository, CraftingOrderRepository>();
        services.AddScoped<IExpeditionRepository, ExpeditionRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ICreatureRepository, CreatureRepository>();
        services.AddScoped<INpcRepository, NpcRepository>();

        // Validators
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
        services.AddScoped<IValidator<RegisterPlayerRequest>, RegisterPlayerRequestValidator>();
        services.AddScoped<IValidator<CreateCampaignRequest>, CreateCampaignRequestValidator>();
        services.AddScoped<IValidator<AssignMemberRequest>, AssignMemberRequestValidator>();
        services.AddScoped<IValidator<CreateCatalogEntryRequest>, CreateCatalogEntryRequestValidator>();
        services.AddScoped<IValidator<UpdateCatalogEntryRequest>, UpdateCatalogEntryRequestValidator>();
        services.AddScoped<IValidator<GrantCharacterSheetRequest>, GrantCharacterSheetRequestValidator>();
        services.AddScoped<IValidator<UpdateCharacterSheetRequest>, UpdateCharacterSheetRequestValidator>();
        services.AddScoped<IValidator<CreateJournalEntryRequest>, CreateJournalEntryRequestValidator>();
        services.AddScoped<IValidator<UpdateJournalEntryRequest>, UpdateJournalEntryRequestValidator>();
        services.AddScoped<IValidator<UpdateGuildSheetRequest>, UpdateGuildSheetRequestValidator>();

        return services;
    }
}
