using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Handlers;
using SimpleChat.Controllers.Validators;
using SimpleChat.Hubs;
using SimpleChat.Infrastructure.Data;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Services;

namespace SimpleChat
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }

        public IHostingEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = Configuration.GetConnectionString("SqliteConnection");
            connectionString = connectionString.Replace("{DataDirectory}",
                Path.Combine(Environment.ContentRootPath, "Data"));

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(connectionString);
            });

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new JwtHelper().GetValidationParameters();
                    // We have to hook the OnMessageReceived event in order to
                    // allow the JWT authentication handler to read the access
                    // token from the query string when a WebSocket or 
                    // Server-Sent Events request comes in.
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/api/chat")))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            AddUserServices(services);
            
            services.AddDistributedMemoryCache();
            services.AddSession();

            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = Environment.IsDevelopment();
                options.ClientTimeoutInterval = TimeSpan.FromMinutes(1);
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        private void AddUserServices(IServiceCollection services)
        {
            services.AddSingleton<IUserCollection, UserCollection>();
            services.AddSingleton<IGuard, Guard>();
            services.AddScoped<IFacebookOAuth2Service, FacebookOAuth2Service>();
            services.AddScoped<IVKontakteOAuth2Service, VKontakteOAuth2Service>();
            services.AddScoped<IOdnoklassnikiOAuth2Service, OdnoklassnikiOAuth2Service>();
            services.AddScoped<IJwtHelper, JwtHelper>();
            services.AddScoped<IJsonHelper, JsonHelper>();
            services.AddScoped<ISessionHelper, SessionHelper>();
            services.AddScoped<ISessionWrapper, SessionWrapper>();
            services.AddScoped<IUriHelper, UriHelper>();
            services.AddScoped<IMD5Hasher, MD5Hasher>();

            // Flows
            services.AddScoped<IAuthenticationFlow, AuthenticationFlow>();
            services.AddScoped<ISignInFlow, SignInFlow>();
            services.AddScoped<IConfirmSignInFlow, ConfirmSignInFlow>();
            services.AddScoped<IContextBuilder, ContextBuilder>();

            // Handlers
            services.AddTransient<AddExternalLogin>();
            services.AddTransient<AddUserClaims>();
            services.AddTransient<ConfigureOAuth2ServiceForAuthentication>();
            services.AddTransient<ConfigureOAuth2ServiceForSignIn>();
            services.AddTransient<CreateAuthenticatedResult>();
            services.AddTransient<CreateConfirmSignInResult>();
            services.AddTransient<CreateIdentityUserIfNotExist>();
            services.AddTransient<CreateSuccessResult>();
            services.AddTransient<ExternalLoginSignIn>();
            services.AddTransient<FetchIdentityUser>();
            services.AddTransient<FetchUserClaims>();
            services.AddTransient<PickUpProviderData>();
            services.AddTransient<PrepareForConfirmSignInReturn>();
            services.AddTransient<PrepareForSignInReturn>();
            services.AddTransient<RefreshUserClaims>();
            services.AddTransient<RequestAccessToken>();
            services.AddTransient<RequestUserInfo>();
            services.AddTransient<ValidateAuthorizationCode>();
            services.AddTransient<ValidateConfirmationCode>();
            services.AddTransient<ValidateEmailAddress>();
            services.AddTransient<ValidateRequestUser>();
            services.AddTransient<ValidateSession>();
            services.AddTransient<ValidateState>();

            // Validators
            services.AddTransient<AccessTokenClaimTypeValidator>();
            services.AddTransient<AuthorizationCodeValidator>();
            services.AddTransient<AvatarClaimTypeValidator>();
            services.AddTransient<ConfirmationCodeValidator>();
            services.AddTransient<IdentityUserValidator>();
            services.AddTransient<NameClaimTypeValidator>();
            services.AddTransient<NullableIdentityUserValidator>();
            services.AddTransient<OAuth2ServiceValidator>();
            services.AddTransient<ProviderValidator>();
            services.AddTransient<RequestUserValidator>();
            services.AddTransient<SessionIdValidator>();
            services.AddTransient<SignInResultValidator>();
            services.AddTransient<StateValidator>();
            services.AddTransient<UserClaimsValidator>();
            services.AddTransient<UserInfoValidator>();
            services.AddTransient<UserNameValidator>();

            if (Environment.IsDevelopment())
                services.AddTransient<IEmailService, MockEmailService>();
            else
                services.AddTransient<IEmailService, EmailService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    HotModuleReplacementClientOptions = new Dictionary<string, string>
                    {
                        ["timeout"] = "60000",
                        ["reload"] = "true"
                    },
                });
                
            }
            if (env.IsProduction())
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseSession();
            app.UseSignalR(configure =>
            {
                configure.MapHub<ChatHub>("/api/chat");
            });
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
