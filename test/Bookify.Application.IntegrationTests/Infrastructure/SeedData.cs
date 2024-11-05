using Bookify.Application.Abstractions.Data;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace Bookify.Application.IntegrationTests.Infrastructure;

public static class SeedData
{
    public static async Task SeedTestData(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<ISqlConnectionFactory>();
        using var connection = connectionFactory.CreateConnection();

        // Seed test-specific data here
        const string userSql = """
                                   INSERT INTO public.users
                                   (id, identity_id, first_name, last_name, email)
                                   VALUES('00000000-0000-0000-0000-000000000001', 'test-keycloak-id-1', 'John', 'Doe', 'john.doe@test.com')
                                   ON CONFLICT DO NOTHING;
                               """;
        await connection.ExecuteAsync(userSql);
    }
}