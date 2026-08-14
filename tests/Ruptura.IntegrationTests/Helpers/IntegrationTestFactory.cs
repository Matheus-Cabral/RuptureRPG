using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Ruptura.Infrastructure.Data;
using Serilog;
using Serilog.Events;
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

    // Replace the API's production Serilog config (which attaches a rolling FILE sink at
    // `logs/ruptura-.log`) with a quiet, synchronous console-only logger for the test host.
    // This override runs after Program.cs's UseSerilog on the same IHostBuilder, so it wins.
    //
    // Why: every WebApplicationFactory boot re-runs Program.cs, which reconfigures the shared
    // static `Log.Logger` AND points a file sink at a single `logs/ruptura-<date>.log` under
    // the test bin directory. Repeatedly booting/disposing hosts against that shared file
    // (background flush racing host/container teardown, file-lock contention) is the long-standing
    // "Serilog/Testcontainers → re-run once" flake and the source of the `logs/*.log` written
    // during the run. A synchronous console sink with no file sink removes the race and stops
    // writing log files under the test output.
    //
    // EF Core → Fatal: the suite deliberately drives unique-constraint races (e.g. "already has
    // an alive character", duplicate guild membership) that the services CATCH and turn into
    // Result.Failure. EF logs those handled DbUpdateExceptions at Error before the catch,
    // producing alarming-but-expected noise. Suppressing EF's own logging in the TEST host
    // silences only that handled-path noise — a genuinely unhandled DB error still surfaces as
    // a 500 that the assertions catch.
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseSerilog((_, cfg) => cfg
            .MinimumLevel.Warning()
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Fatal)
            .WriteTo.Console());

        return base.CreateHost(builder);
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
