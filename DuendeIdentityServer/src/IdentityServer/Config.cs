// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using System.Collections.Generic;

namespace IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            { 
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("acquisition", "Acquisition API"),
                new ApiScope("provisioning", "Provisioning API"),
                new ApiScope("publish", "Publish API"),
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                // machine to machine client (api access)
                new Client
                {
                    ClientId = "aqts",
                    ClientSecrets = { new Secret("aqts-secret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = { "acquisition", "provisioning", "publish" }
                },
                // interactive MVC client
                new Client
                {
                    ClientId = "mvc",
                    ClientSecrets = { new Secret("mvc-secret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
                    },
                    RedirectUris = { "https://localhost:5002/signin-oidc" },
                    PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" }
                }
            };
    }
}