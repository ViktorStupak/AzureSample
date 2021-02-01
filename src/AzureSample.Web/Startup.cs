using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AzureSample.Context.Infrastructure;
using AzureSample.Context.Options;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace AzureSample.Web
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
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"));
            services.AddControllersWithViews(
                options =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                });
            services.AddRazorPages()
                .AddMicrosoftIdentityUI();
            services.AddDbContext<DataContext>(opts =>
            {
                opts.UseSqlServer(Configuration["ConnectionStrings:DefaultConnection"]);
                opts.EnableSensitiveDataLogging();
            });
            services.AddScoped<IRepository, Repository>();
            services.Configure<BlobStorageOptions>(Configuration.GetSection(BlobStorageOptions.Position));
            services.Configure<QueuesOptions>(Configuration.GetSection(QueuesOptions.Position));
            services.Configure<TablesOptions>(Configuration.GetSection(TablesOptions.Position));
            services.AddSingleton<BlobStorageService>();
            services.AddSingleton<QueuesService>();
            services.AddSingleton<TablesService>();
            services.Configure<MvcNewtonsoftJsonOptions>(opts =>
            {
                opts.SerializerSettings.NullValueHandling
                    = Newtonsoft.Json.NullValueHandling.Ignore;
            });

            services.Configure<MvcOptions>(opts =>
            {
                opts.RespectBrowserAcceptHeader = true;
                opts.ReturnHttpNotAcceptable = true;
            });
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
