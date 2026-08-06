using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ruptura.Infrastructure.Data;
using Testcontainers.PostgreSql;

namespace Ruptura.IntegrationTests.Helpers;

public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private readonly string _mediaRoot =
        Path.Combine(Path.GetTempPath(), "ruptura-test-media-" + Guid.NewGuid());

    public string MediaRoot => _mediaRoot;

    public async Task InitializeAsync() => await _db.StartAsync();

    public new async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        if (Directory.Exists(_mediaRoot))
            Directory.Delete(_mediaRoot, recursive: true);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, cfg) =>
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MediaSettings:RootPath"] = _mediaRoot
            }));

        builder.ConfigureServices(services =>
        {
            // Replace the real DB with the Testcontainers one
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseNpgsql(_db.GetConnectionString()));
        });
    }
}
