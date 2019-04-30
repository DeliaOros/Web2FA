using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;


namespace WebApplication3
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new RequireHttpsAttribute());
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<IdentityUser>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.Configure<LdapConfig>(Configuration.GetSection("ldap"));
            services.AddScoped<IAuthenticationService, LdapAuthenticationService>();
            //public static AuthenticationBuilder AddCookie(this AuthenticationBuilder builder, string authenticationScheme, Action<CookieAuthenticationOptions> configureOptions);
            // public static AuthenticationBuilder AddCookie(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<CookieAuthenticationOptions> configureOptions);
            services.AddAuthentication().AddCookie("app", configureOptions => new CookieAuthenticationOptions

            {
                Events = new CookieAuthenticationEvents
                {
                    // You will need this only if you use Ajax calls with a library not compatible with IsAjaxRequest
                    // More info here: https://github.com/aspnet/Security/issues/1056
                    OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return Task.CompletedTask;
                    }
                },
                LoginPath = new PathString("/login"),


            });

            // default access requires admin access
            var isAdminUserPolicy = new AuthorizationPolicyBuilder().RequireRole("Admin").Build();
            services.AddMvc(options =>
            {
                options.Filters.Add(new ApplyPolicyOrAuthorizeFilter(isAdminUserPolicy));
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "login",
                    template: "login",
                    defaults: new { controller = "Account", action = "Login" }
                );
                routes.MapRoute(
                    name: "logout",
                    template: "logout",
                    defaults: new { controller = "Account", action = "Logout" }
                );
            });
        }
    }
}