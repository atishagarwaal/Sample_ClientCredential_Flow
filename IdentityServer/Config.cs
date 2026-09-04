using Duende.IdentityServer.Models;

namespace IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId()
        };

    // Defines the physical API
    public static IEnumerable<ApiResource> ApiResources =>
        new[]
        {
            new ApiResource("api-weather", "Weather API")
            {
                // Add permissions (scopes) to the API resource
                Scopes = { "weather.read" }
            }
        };


    // Defines the permissions (scopes) that can be requested by clients
    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
            {
                new ApiScope("weather.read", "Read Weather Data"),
                new ApiScope("weather.write", "Modify Weather Data")
            };

    public static IEnumerable<Client> Clients =>
        new Client[]
            {
                new Client
                {
                    ClientId = "weather-client-app",

                    ClientSecrets = { new Secret("Pass@word123".Sha256()) },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    // Set the pemissions (scopes)
                    AllowedScopes = { "weather.read" },
                }
            };
}
