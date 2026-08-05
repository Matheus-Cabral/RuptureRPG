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
using Ruptura.Infrastructure.Data;
using Ruptura.Infrastructure.Identity;
using Ruptura.Infrastructure.Repositories;
using Ruptura.Infrastructure.Services;
using Ruptura.Infrastructure.Settings;
using Ruptura.Shared.Auth;
using Ruptura.Shared.Catalog;
using Ruptura.Shared.Campaigns;

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

        // Core services
        services.AddSingleton<JwtService>();
        services.AddSingleton<ICharacterStatsCalculator, CharacterStatsCalculator>();

        // Application services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IInviteCodeService, InviteCodeService>();
        services.AddScoped<ICampaignService, CampaignService>();
        services.AddScoped<ICatalogEntryService, CatalogEntryService>();

        // Repositories
        services.AddScoped<IInviteCodeRepository, InviteCodeRepository>();
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<ICampaignMembershipRepository, CampaignMembershipRepository>();
        services.AddScoped<ICatalogEntryRepository, CatalogEntryRepository>();
        services.AddScoped<ICharacterSheetRepository, CharacterSheetRepository>();

        // Validators
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
        services.AddScoped<IValidator<RegisterPlayerRequest>, RegisterPlayerRequestValidator>();
        services.AddScoped<IValidator<CreateCampaignRequest>, CreateCampaignRequestValidator>();
        services.AddScoped<IValidator<AssignMemberRequest>, AssignMemberRequestValidator>();
        services.AddScoped<IValidator<CreateCatalogEntryRequest>, CreateCatalogEntryRequestValidator>();
        services.AddScoped<IValidator<UpdateCatalogEntryRequest>, UpdateCatalogEntryRequestValidator>();

        return services;
    }
}
