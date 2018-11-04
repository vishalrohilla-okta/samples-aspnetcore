using System.Collections.Generic;

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using Okta.AspNetCore;
using okta_aspnetcore_mvc_example.Models;

namespace okta_aspnetcore_mvc_example
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
            services.Configure<OktaSettings>(Configuration.GetSection("Okta"));         
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme;
            })

  .AddCookie()
  .AddOpenIdConnect(options =>
  {

      // Configuration pulled from appsettings.json by default

      options.ClientId = Configuration.GetSection("Okta").GetValue<string>("ClientId"); 
      options.ClientSecret = Configuration.GetSection("Okta").GetValue<string>("ClientSecret");
      options.Authority = Configuration.GetSection("Okta").GetValue<string>("Authority");
      options.CallbackPath = "/authorization-code/callback";
      options.ResponseType = "code";
      options.SaveTokens = true;
      options.UseTokenLifetime = false;
      options.GetClaimsFromUserInfoEndpoint = true;
      options.Scope.Add("openid");
      options.Scope.Add("profile");
      options.TokenValidationParameters = new TokenValidationParameters
      {
          NameClaimType = "name"
      };

      options.Events = new OpenIdConnectEvents

      {

          OnRedirectToIdentityProvider = ApplyCustomParameters,

          OnUserInformationReceived = CopyCustomClaims

      };

  });
            services.AddMvc();           
        }
        // When we want to log in with a specific provider, we have
        // to pass some additional parameters to the /authorize endpoint.

        private Task ApplyCustomParameters(RedirectContext context)
        {
            if (context.Properties.Items.TryGetValue("idp", out var idpId))
            {
                context.ProtocolMessage.SetParameter("idp", idpId);
            }
            if (context.Properties.Items.TryGetValue("sessionToken", out var sessionToken))
            {
                context.ProtocolMessage.SetParameter("sessionToken", sessionToken);
            }
            if (context.Properties.Items.TryGetValue("login_hint", out var loginHint))
            {
                context.ProtocolMessage.LoginHint = loginHint;
            }
            return Task.CompletedTask;

        }
        // Custom claims in the ID token or userinfo endpoint aren't automatically
        // copied into the ClaimsPrincipal, so it must be done manually here.
        // (This method is used by both OpenIdConnectMiddleware definitions in Events.OnUserInformationReceived)

        private Task CopyCustomClaims(UserInformationReceivedContext context)
        {
            if (context.User.TryGetValue("rewardsNumber", out var rewardsNumber))
            {
                var claimsIdentity = (ClaimsIdentity)context.Principal.Identity;
                claimsIdentity.AddClaim(new Claim("rewardsNumber", rewardsNumber?.ToString()));
            }
            return Task.CompletedTask;
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
