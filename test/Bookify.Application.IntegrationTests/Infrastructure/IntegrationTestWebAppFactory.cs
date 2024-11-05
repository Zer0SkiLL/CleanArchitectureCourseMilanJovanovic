using Bookify.Application.Abstractions.Authentication;
using Bookify.Application.Abstractions.Data;
using Bookify.Application.IntegrationTests.UserContext;
using Bookify.Infrastructure;
using Bookify.Infrastructure.Authentication;
using Bookify.Infrastructure.Data;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.Keycloak;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Bookify.Application.IntegrationTests.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("bookify")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:latest")
        .Build();

    private readonly KeycloakContainer _keycloakContainer = new KeycloakBuilder()
        .WithResourceMapping(
            new FileInfo(".files/bookify-realm-export.json"),
            new FileInfo("/opt/keycloak/data/import/realm.json"))
        .WithCommand("--import-realm")
        .Build();

    // private readonly SeedData _seedData = new SeedData();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));

            services.AddDbContext<ApplicationDbContext>(options =>
                options
                    .UseNpgsql(_dbContainer.GetConnectionString())
                    .UseSnakeCaseNamingConvention());

            services.AddSingleton<ISqlConnectionFactory>(_ =>
                new SqlConnectionFactory(_dbContainer.GetConnectionString()));

            services.Configure<RedisCacheOptions>(redisCacheOptions =>
                redisCacheOptions.Configuration = _redisContainer.GetConnectionString());

            var keycloakAddress = _keycloakContainer.GetBaseAddress();

            services.Configure<KeycloakOptions>(o =>
            {
                o.AdminUrl = $"{keycloakAddress}admin/realms/bookify/";
                o.TokenUrl = $"{keycloakAddress}realms/bookify/protocol/openid-connect/token";
            });
            
            // set the usercontext for tests to the user that has been seeded to the database with a fixed userId
            services.AddScoped<IUserContext, TestUserContext>();
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _redisContainer.StartAsync();
        await _keycloakContainer.StartAsync();
        await SeedData.SeedTestData(Services);
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _redisContainer.StopAsync();
        await _keycloakContainer.StopAsync();
    }
}