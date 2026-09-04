# Sample Client Credentials Flow

This repository demonstrates a client credentials flow using IdentityServer to issue a JWT for a specific API resource and how a downstream Web API validates the token and enforces scope-based authorization.

## Overview

The client requests a token from IdentityServer using client credentials and requests one or more scopes. IdentityServer validates that the client is allowed the requested scopes; if valid, it issues a JWT. IdentityServer derives the token audience (aud) from how scopes are configured and mapped to ApiResources — clients do not normally include an explicit aud parameter in the client credentials request.

## How IdentityServer resolves the audience (aud)

1) Scope request: the client requests a token with a Scope, for example "weather.read".

2) Scope-to-ApiResource mapping: IdentityServer looks up configured ApiResources and their Scopes. If it finds an ApiResource that contains the requested scope, that ApiResource's name is used as the token audience.

   Example configuration:

```csharp
new ApiResource("api-weather", "Weather API")
{
	Scopes = { "weather.read" } // links the scope to this resource
}
```

3) Audience injection: because "weather.read" belongs to the "api-weather" resource, IdentityServer automatically adds "api-weather" to the token's aud claim list.

4) API validation: downstream APIs should validate the token's aud claim (for example, with ValidateAudience and ValidAudiences). If the API expects "api-weather" and the aud claim contains that value, audience validation succeeds.

What if a scope isn't mapped to any ApiResource?

If you request a scope that is not listed inside any ApiResource, IdentityServer will still issue a token but the aud claim will default to the identity server's resources endpoint (for example, https://localhost:5001/resources). In that case, an API that expects a specific resource name (like "api-weather") will fail audience validation.

## Step 1: Token Request & Issuance (IdentityServer)

When the console application executes this block:

```csharp
var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
{
	Address = disco.TokenEndpoint,
	ClientId = "weather-client-app",
	ClientSecret = "Pass@word123",
	Scope = "weather.read",
});
```

Behavior:
- Scope validation: IdentityServer checks that the client `weather-client-app` is allowed to request `weather.read` (client's AllowedScopes).
- Audience resolution: IdentityServer determines the aud value from the ApiResource that contains the requested scope (see previous section). It injects that value into the JWT it issues.
- Token claims: the issued JWT will include the scope claim (`weather.read`) and the aud claim (for example, `api-weather`).

## Step 2: Token Verification & Authorization

When the API receives the bearer token, standard authentication and authorization middleware handle the request:

1) Authentication (AddJwtBearer)

```csharp
options.TokenValidationParameters.ValidateAudience = true;
options.TokenValidationParameters.ValidAudiences = a new List<string> { "api-weather" };
```

Result: The middleware validates the token's aud claim contains `api-weather` and authenticates the request.

2) Authorization (AddAuthorization)

```csharp
options.AddPolicy("WeatherReadPolicy", policy =>
	policy.RequireClaim("scope", "weather.read"));
```

Result: After authentication, the authorization layer checks the token's scope claim. If `weather.read` is present the policy is satisfied and the API request is authorized.

## Useful rule of thumb

- Clients are granted scopes (permissions).
- Scopes are defined as ApiScopes and are associated with ApiResources via the ApiResource.Scopes collection.
- IdentityServer determines the token audience from the ApiResource that includes the requested scope. The `Resource` parameter in a ClientCredentialsTokenRequest is an optional routing hint (RFC 8707) and not a required way to set aud.

## Notes and recommendations

- Keep clients' AllowedScopes and ApiResource configuration in sync so IdentityServer can resolve and issue tokens with the correct audience and scopes.
- Always map ApiScopes to an ApiResource when those scopes are intended for a specific API; otherwise audience validation by the API may fail.
- Prefer validating both audience and scope in your API to enforce least privilege.