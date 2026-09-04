using System.Text.Json;
using Duende.IdentityModel.Client;

Console.WriteLine("Hello, World!");

// Discover endpoints from metadata
var client = new HttpClient();

var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
if (disco.IsError)
{
    Console.WriteLine(disco.Error);
    return 1;
}

// Request token
var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
{
    Address = disco.TokenEndpoint,
    ClientId = "weather-client-app",
    ClientSecret = "Pass@word123",
    Scope = "weather.read", // Permission scopes
});

if (tokenResponse.IsError)
{
    Console.WriteLine(tokenResponse.Error);
    Console.WriteLine(tokenResponse.ErrorDescription);
    return 1;
}

Console.WriteLine(tokenResponse.Json);
Console.WriteLine("\n\n");

// Call API
var apiClient = new HttpClient();
apiClient.SetBearerToken(tokenResponse.AccessToken!);

var response = await apiClient.GetAsync("http://localhost:5182/weatherforecast");
if (!response.IsSuccessStatusCode)
{
    Console.WriteLine(response.StatusCode);
    Console.ReadLine();
    return 1;
}
else
{
    var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
    Console.WriteLine(JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true }));
    Console.ReadLine();
    return 0;
}