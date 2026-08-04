using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ruptura.API.Middleware;
using Ruptura.Infrastructure.Data;
using Ruptura.Infrastructure.Extensions;
using Ruptura.Infrastructure.Settings;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Logging ───────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/ruptura-.log", rollingInterval: RollingInterval.Day));

    // ── Controllers + Localization ────────────────────────────────────────────
    builder.Services.AddControllers()
        .ConfigureApiBehaviorOptions(opts => opts.SuppressModelStateInvalidFilter = true);
    builder.Services.AddLocalization(opts => opts.ResourcesPath = "Resources");

    // ── Infrastructure (EF, Identity, JWT settings, services, repos, validators)
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── Authentication: JWT Bearer ────────────────────────────────────────────
    var jwtSettings = builder.Configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>()!;
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opts =>
        {
            // Prevent the handler from renaming "role" → long URI claim type,
            // so [Authorize(Roles = "GameMaster")] matches the short-form claim.
            opts.MapInboundClaims = false;

            opts.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                // Use short claim names ("role", "name") instead of long URI forms
                RoleClaimType = "role",
                NameClaimType = "name"
            };
        });

    builder.Services.AddAuthorization();

    // ── CORS ─────────────────────────────────────────────────────────────────
    // Cors:AllowedOrigin supports comma-separated origins for multi-environment use.
    // Example: "http://localhost:80,http://localhost:5186"
    var allowedOrigins = (builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost")
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    builder.Services.AddCors(opts =>
        opts.AddPolicy("BlazorClient", policy =>
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));

    // ── Swagger ───────────────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(opts =>
    {
        opts.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Ruptura API",
            Version = "v1",
            Description = "API for managing Ruptura RPG campaigns and character sheets."
        });
        opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter JWT access token"
        });
        opts.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                        { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                []
            }
        });
    });

    var app = builder.Build();

    // ── Auto-migrate on startup ───────────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    // ── Middleware pipeline ───────────────────────────────────────────────────
    app.UseMiddleware<ExceptionMiddleware>();
    app.UseSerilogRequestLogging();

    var supportedCultures = new[] { "en", "pt-BR" };
    app.UseRequestLocalization(opts =>
        opts.SetDefaultCulture("en")
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures)
            .SetDefaultCulture("en"));

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("BlazorClient");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
