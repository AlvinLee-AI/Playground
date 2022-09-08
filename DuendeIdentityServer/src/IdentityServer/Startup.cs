// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.IdentityServer;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Reflection;
using System.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace IdentityServer
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }

        public Startup(IWebHostEnvironment environment)
        {
            Environment = environment;
        }

        // Configures DI system
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            const string dbConnectionString = @"Data Source=Duende.IdentityServer.Quickstart.EntityFramework.db";

            services
                .AddIdentityServer(options =>
                {
                    // https://docs.duendesoftware.com/identityserver/v5/fundamentals/resources/
                    options.EmitStaticAudienceClaim = true;
                })
                // Ignore in-memory resources in favour of EF store provider
                //.AddInMemoryIdentityResources(Config.IdentityResources)
                //.AddInMemoryApiScopes(Config.ApiScopes)
                //.AddInMemoryClients(Config.Clients)
                .AddTestUsers(TestUsers.Users)
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b => 
                        b.UseSqlite(dbConnectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b => 
                        b.UseSqlite(dbConnectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                });

            var googleClientId = System.Environment.GetEnvironmentVariable("OIDC_GOOGLE_CLIENTID");
            var googleClientSecret = System.Environment.GetEnvironmentVariable("OIDC_GOOGLE_SECRET");
            if (string.IsNullOrEmpty(googleClientId) || string.IsNullOrEmpty(googleClientSecret))
            {
                Console.WriteLine("Google OIDC credentials missing.");
                return;
            }

            services.AddAuthentication()
                .AddGoogle("Google", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret;
                })
                .AddOpenIdConnect("oidc", "Demo IdentityServer", options => 
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SignOutScheme = IdentityServerConstants.SignoutScheme;
                    options.SaveTokens = true;

                    options.Authority = "https://demo.duendesoftware.com";
                    options.ClientId = "interactive.confidential";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enables basic MVC UI
            app.UseStaticFiles();
            app.UseRouting();
            
            app.UseIdentityServer();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
