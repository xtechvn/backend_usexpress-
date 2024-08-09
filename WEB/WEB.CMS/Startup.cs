using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caching.RedisWorker;
using Entities.ConfigModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using Repositories.IRepositories;
using Repositories.Repositories;

namespace WEB.CMS
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
            services.AddControllersWithViews().AddNewtonsoftJson();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Authenticate/Login");
                options.LoginPath = new PathString("/Authenticate/Login");
                options.ReturnUrlParameter = "RequestPath";
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
                options.SlidingExpiration = true;
                options.Cookie = new CookieBuilder
                {
                    HttpOnly = true,
                    Name = "Net.Security.Cookie",
                    Path = "/",
                    SameSite = SameSiteMode.Lax,
                    SecurePolicy = CookieSecurePolicy.SameAsRequest
                };
            });

            // services.AddRazorPages().AddRazorRuntimeCompilation();

            // Get config to instance model
            services.Configure<DataBaseConfig>(Configuration.GetSection("DataBaseConfig"));
            services.Configure<MailConfig>(Configuration.GetSection("MailConfig"));
            services.Configure<DomainConfig>(Configuration.GetSection("DomainConfig"));

            // Register services
            services.AddSingleton<IAllCodeRepository, AllCodeRepository>();
            services.AddSingleton<ICommonRepository, CommonRepository>();
            services.AddSingleton<IMenuRepository, MenuRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IRoleRepository, RoleRepository>();
            services.AddTransient<IPermissionRepository, PermissionRepository>();
            services.AddTransient<IGroupProductRepository, GroupProductRepository>();
            services.AddTransient<ILabelRepository, LabelRepository>();
            services.AddTransient<IPositionRepository, PositionRepository>();
            services.AddTransient<IProductClassificationRepository, ProductClassificationRepository>();
            services.AddTransient<ICampaignAdsRepository, CampaignAdsRepository>();
            services.AddTransient<IClientRepository, ClientRepository>();
            services.AddTransient<INoteRepository, NoteRepository>();
            services.AddTransient<IAllCodeRepository, AllCodeRepository>();
            services.AddTransient<IAttachFileRepository, AttachFileRepository>();
            services.AddTransient<IOrderRepository, OrderRepository>();
            services.AddTransient<IProductRepository, ProductRepository>();
            services.AddTransient<ICashbackRepository, CashbackRepository>();
            services.AddTransient<IPaymentRepository, PaymentRepository>();
            services.AddTransient<IArticleRepository, ArticleRepository>();
            services.AddTransient<IProvinceRepository, ProvinceRepository>();
            services.AddTransient<IDistrictRepository, DistrictRepository>();
            services.AddTransient<IWardRepository, WardRepository>();
            services.AddTransient<IMFARepository, MFARepository>();
            services.AddTransient<IOrderProgressRepository, OrderProgessRepository>();
            services.AddTransient<ILocationProductRepository, LocationProductRepository>();
            services.AddTransient<IAutomaticPurchaseAmzRepository, AutomaticPurchaseAmzRepository>();
            services.AddTransient<IAutomaticPurchaseHistoryRepository, AutomaticPurchaseHistoryRepository>();

            // Setting Redis                     
            services.AddSingleton<RedisConn>();
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
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    name: "Account",
                    pattern: "{controller=Account}/{action=Login}/{requestPath?}");
                endpoints.MapControllerRoute(name: "setupManual",
                  pattern: "/product/setup-manual",
                  defaults: new { controller = "product", action = "SetupManual" });
            });
        }
    }
}
