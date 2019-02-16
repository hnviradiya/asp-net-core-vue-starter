using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Net;
using System.Threading.Tasks;
using VueCliMiddleware;

namespace AspNetCoreVueStarter
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(o =>
            {
                var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();
                o.Filters.Add(new AuthorizeFilter(policy));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            #region Source: https://github.com/IdentityServer/IdentityServer4.Samples/blob/master/Clients/src/MvcHybridAutomaticRefresh/Startup.cs

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "oidc";
            })
             .AddCookie(options =>
             {
                 options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                 options.Cookie.Name = "mvchybridautorefresh";
             })
             .AddOpenIdConnect("oidc", options =>
             {
                 options.Authority = "http://localhost:5000";
                 options.RequireHttpsMetadata = false;

                 options.ClientSecret = "secret";
                 options.ClientId = "mvc.hybrid";

                 options.ResponseType = "code id_token";

                 options.Scope.Clear();
                 options.Scope.Add("openid");
                 options.Scope.Add("profile");
                 options.Scope.Add("email");
                 options.Scope.Add("api1");
                 options.Scope.Add("offline_access");

                 options.ClaimActions.MapAllExcept("iss", "nbf", "exp", "aud", "nonce", "iat", "c_hash");

                 options.GetClaimsFromUserInfoEndpoint = true;
                 options.SaveTokens = true;

                 options.Events.OnRedirectToIdentityProvider = context =>
                 {
                     if (context.Request.Path.StartsWithSegments("/api"))
                     {
                         if (context.Response.StatusCode == (int)HttpStatusCode.OK)
                         {
                             context.ProtocolMessage.State = options.StateDataFormat.Protect(context.Properties);
                             context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                             context.Response.Headers["Location"] = context.ProtocolMessage.CreateAuthenticationRequestUrl();
                         }
                         context.HandleResponse();
                     }
                     return Task.CompletedTask;
                 };

                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     NameClaimType = JwtClaimTypes.Name,
                     RoleClaimType = JwtClaimTypes.Role,
                 };

             });

            #endregion

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    // run npm process with client app
                    spa.Options.StartupTimeout = new TimeSpan(0, 0, 360);
                    spa.UseVueCli(npmScript: "serve", port: 8080, regex: "Compiled ");
                    // if you just prefer to proxy requests from client app, use proxy to SPA dev server instead:
                    // app should be already running before starting a .NET client
                    // spa.UseProxyToSpaDevelopmentServer("http://localhost:8080"); // your Vue app port
                }
            });
        }
    }
}
