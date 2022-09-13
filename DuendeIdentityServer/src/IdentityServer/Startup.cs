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
using System.Linq;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
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
            // Duende EF integration provides template db
            const string dbConnectionString = @"Data Source=Duende.IdentityServer.Quickstart.EntityFramework.db";

            services
                .AddIdentityServer(options =>
                {
                    // https://docs.duendesoftware.com/identityserver/v5/fundamentals/resources/
                    options.EmitStaticAudienceClaim = true;
                    
                    // AQ-28852 Cache dynamic, custom IdentityProvider configurations to improve performance
                    //          Refreshes configuration store data from database after specified time
                    options.Caching.IdentityProviderCacheDuration = TimeSpan.FromMinutes(5);
                    options.Caching.ClientStoreExpiration = TimeSpan.FromMinutes(5);
                })
                // Ignore in-memory resources in favour of EF store provider
                //.AddInMemoryIdentityResources(Config.IdentityResources)
                //.AddInMemoryApiScopes(Config.ApiScopes)
                //.AddInMemoryClients(Config.Clients)
                //.AddTestUsers(TestUsers.Users)

                // AQ-28852 Dynamic IdentityProvider configurations stored in DB, rather than statically
                //          loading configurations via .AddAuthentication() calls below
                .AddConfigurationStore(options =>
                {
                    options.DefaultSchema = "myConfigurationSchema";
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

            // Statically loads external IdentityProvider configurations
            services.AddAuthentication()
                .AddGoogle("Google", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ClientId = "796649104179-3d4pd2avcaepuvrnh9ob9cb50ggsu9b9.apps.googleusercontent.com";
                    options.ClientSecret = "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";
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

            // Seed database with in-memory configuration data. Used only for demo purposes
            InitializeDatabase(app);

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

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (!context.Clients.Any())
                {
                    foreach (var client in Config.Clients)
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.IdentityResources)
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiScopes.Any())
                {
                    foreach (var resource in Config.ApiScopes)
                    {
                        context.ApiScopes.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }

    }
}
