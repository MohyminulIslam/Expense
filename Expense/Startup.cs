using Expense.Data;
using Expense.Infrastructure.ApplicationUserClaims;
using Expense.Models.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using Syncfusion.Licensing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Expense.Infrastructure;
using Expense.Infrastructure.Automapper;
using Expense.Infrastructure.AppSettingsModels;
using Expense.Infrastructure.Startup;

namespace Expense
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {

            Configuration = configuration;
            SyncfusionLicenseProvider.RegisterLicense("NTQ0NDQ3QDMxMzkyZTMzMmUzMEtsKzZuRU1ya1QxUTZ0RHhhT2NxUzI5SStwbE5IZXB2cHpLZnN4bHFlUms9");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddRazorPages();
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services.AddMvc(options => options.EnableEndpointRouting = false)
           .AddNewtonsoftJson(options =>
           {
               options.SerializerSettings.ContractResolver = new DefaultContractResolver();
               options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
               //options.SerializerSettings.DateFormatString = "dd/MM/yyyy";
           });


            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed
                // for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
            });



            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // !! Note:
                //
                // If you switch database providers, you might be required to re-create the migrations
                // as they are not always compatible between database systems

                // The easiest option for development outside a container is to use SQLite
                // options.UseSqlite(Configuration.GetConnectionString("SqliteConnection"));
                // Or use this for PostgreSQL:
                //  options.UseNpgsql(Configuration.GetConnectionString("PostgresConnection"));

                // Use this to connect to a MySQL server:
                // options.UseMySQL(Configuration.GetConnectionString("MysqlConnection"));
                // Or use this for SQL Server (if running on Windows):
                options.UseSqlServer(Configuration.GetConnectionString("MsSqlConnection"));
            });

            services.AddDbContext<AppDbContext>();

            services.AddDefaultIdentity<ApplicationUser>()
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>();






            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();






            services.Configure<IdentityOptions>(options =>
            {
                // Default Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
                options.Lockout.MaxFailedAccessAttempts = 50;
                options.Lockout.AllowedForNewUsers = true;

                // Default Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Default SignIn settings.
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;

                // Default User settings.
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "HPL.ERP.AppCookie";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                // You might want to only set the application cookies over a secure connection:
                // options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;
            });



            services.AddAutoMapper(typeof(MappingProfile));

            // As per https://github.com/aspnet/AspNetCore/issues/5828
            // the settings for the cookie would get overwritten if using the default UI so
            // we need to "post-configure" the authentication cookie
            services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
            {
                options.AccessDeniedPath = "/access-denied";
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";

                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
            });

            services.AddDataProtection()
                .PersistKeysToDbContext<ApplicationDbContext>();

            services.AddAntiforgery();

            services.Configure<ScriptTags>(Configuration.GetSection(nameof(ScriptTags)));





            var mvcBuilder = services.AddControllersWithViews(options =>
            {
                // Slugify routes so that we can use /employee/employee-details/1 instead of
                // the default /Employee/EmployeeDetails/1
                //
                // Using an outbound parameter transformer is a better choice as it also allows
                // the creation of correct routes using view helpers
                options.Conventions.Add(
                   new RouteTokenTransformerConvention(
                       new SlugifyParameterTransformer()));

                // Enable Antiforgery feature by default on all controller actions
                // options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

#if DEBUG
            mvcBuilder.AddRazorRuntimeCompilation();
#endif

            services.AddRazorPages(options =>
            {
                options.Conventions.AddAreaPageRoute("Identity", "/Account/Register", "/register");
                options.Conventions.AddAreaPageRoute("Identity", "/Account/Login", "/login");
                options.Conventions.AddAreaPageRoute("Identity", "/Account/Logout", "/logout");
                options.Conventions.AddAreaPageRoute("Identity", "/Account/ForgotPassword", "/forgot-password");
            })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddSessionStateTempDataProvider();

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            // You probably want to use in-memory cache if not developing using docker-compose
            services.AddMemoryCache();
            //  services.AddDistributedRedisCache(action => { action.Configuration = Configuration["Redis:InstanceName"]; });

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.Name = "HPL.ERP.SessionCookie";
                // You might want to only set the application cookies over a secure connection:
                // options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.HttpOnly = true;
                // Make the session cookie essential
                options.Cookie.IsEssential = true;
            });

            // This adds a hosted service that, on application start-up, seeds the database with initial data.
            // You can remove this if you want to prevent the seeding process or you can change the initial data
            // to suit your needs in the IdentityDataSeeder class.
            services.AddHostedService<DbSeederHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
