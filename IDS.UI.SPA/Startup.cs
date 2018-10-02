using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;

namespace IDS.UI.SPA
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
            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                    options.Cookie.Name = "authcookie";
                    //options.AccessDeniedPath = "/Authorization/AccessDenied";
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = Configuration.GetSection("auth:oidc:authority").Value;
                    options.ClientId = Configuration.GetSection("auth:oidc:clientid").Value;
                    options.RequireHttpsMetadata = false;
                    options.ResponseType = "code id_token";
                    options.ClientSecret = "secret";

                    options.Events.OnRedirectToIdentityProvider = n =>
                    {
                        if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.Authentication)
                        {
                            //string urlHost = n.HttpContext.Request.Host.Value;

                            //urlHost = urlHost.Remove(urlHost.IndexOf(":"), urlHost.Length - urlHost.IndexOf(":")).ToLower().Trim();

                            //string subdomainName = urlHost.Split('.')[0];

                            //if (!string.IsNullOrWhiteSpace(urlHost) && subdomainName != "www" && subdomainName.ToUpperInvariant() != "AMS")
                            //{
                            //    n.ProtocolMessage.AcrValues = $"tenant:{subdomainName}";
                            //}
                            //else if (!string.IsNullOrWhiteSpace(urlHost) && subdomainName == "www" && subdomainName.ToUpperInvariant() != "AMS")
                            //{
                            //    n.ProtocolMessage.AcrValues = $"tenant:{urlHost.Split('.')[1]}";
                            //}
                            n.ProtocolMessage.AcrValues = $"tenant:api1";
                        }

                        return Task.FromResult(0);
                    };

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    //options.Scope.Add("email");
                    //options.Scope.Add("tenantId");

                    options.SaveTokens = true;
                    options.SignInScheme = "Cookies";

                    //options.TokenValidationParameters = new TokenValidationParameters
                    //{
                    //    NameClaimType = JwtClaimTypes.Name,
                    //    RoleClaimType = JwtClaimTypes.Role,
                    //};

                    //options.GetClaimsFromUserInfoEndpoint = true;

                    //options.ClaimActions.Remove("amr");
                    //options.ClaimActions.DeleteClaim("sid");
                    //options.ClaimActions.DeleteClaim("idp");

                    //options.ClaimActions.MapUniqueJsonKey("tenant", "tenantid");
                });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
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
            }

            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

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
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
