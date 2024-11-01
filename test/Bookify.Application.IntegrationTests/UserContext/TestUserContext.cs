using Bookify.Application.Abstractions.Authentication;

namespace Bookify.Application.IntegrationTests.UserContext;

public class TestUserContext : IUserContext
{
    public Guid UserId { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Use the seeded user's ID
    public string IdentityId => "test-keycloak-id-1"; // Matches the seeded IdentityId
}