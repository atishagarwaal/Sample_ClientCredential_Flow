using Duende.IdentityServer.Models;

namespace IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId()
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new[]
        {
            new ApiResource("api-weather", "Weather API")
            {
                Scopes = { "api-weather" } // Audience for the API resource
            }
        };


    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
            {
                new ApiScope("api-weather", "Weather API")
            };

    public static IEnumerable<Client> Clients =>
        new Client[]
            {
                new Client
                {
                    ClientId = "XCJHDJHGDSYGYW",

                    ClientSecrets = { new Secret("Pass@word123".Sha256()) },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    AllowedScopes = { "api-weather" },

                    Claims = new List<ClientClaim>
                    {
                        new ClientClaim("audience", "api-weather")
                    }
                }
            };
}
