using System;
using System.Text;
using Caching.RedisWorker;
using Entities.ConfigModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Repositories.IRepositories;
using Repositories.Repositories;

using WEB.UI.Common;
using WEB.UI.Service;

//using WEB.UI.Validation;

namespace WEB.UI
{
    public class Startup
    {
        //public static string API_GEN_TOKEN = ReadFile.LoadConfig().API_GEN_TOKEN;
        // public static int TimeOutVerifyEmail = Convert.ToInt32(ReadFile.LoadConfig().timeout_verify_email); // phut
        //public static string KEY_PRIVATE_TOKEN = "YourKey-2374-OFFKDI940NG7:56753253-tyuw-5769-0921-kfirox29zoxv";
        private readonly IConfiguration Configuration;//
        public Startup(IConfiguration configuration)
        {

            Configuration = configuration;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {


            services.AddSession(options =>
            {
                options.Cookie.Name = Constants.CartSessionKey;
                options.IdleTimeout = TimeSpan.FromDays(30);
            });

            // Cấu hình trang mặc định với những action gắn authent 
            //services.ConfigureApplicationCookie(config =>
            //{
            //    config.Cookie.Name = Constants.JWToken;
            //    config.LoginPath = "/Client/Login";
            //    config.ExpireTimeSpan = TimeSpan.FromSeconds(3);
            //    config.SlidingExpiration = true;
            //});

            services.AddMvc();
            //services.AddControllersWithViews();            
            services.AddRazorPages();
            //services.AddUrlHelper();
            // Get config to instance model
            services.Configure<DataBaseConfig>(Configuration.GetSection("DataBaseConfig"));
            services.Configure<MailConfig>(Configuration.GetSection("MailConfig"));


            // Register services                      
            services.AddSingleton<IClientRepository, ClientRepository>();
            services.AddSingleton<IProductRepository, ProductRepository>();
            services.AddSingleton<IGroupProductRepository, GroupProductRepository>();
            services.AddSingleton<IArticleRepository, ArticleRepository>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IViewRenderService, ViewRenderService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            /// Thêm nén các file css/js/... theo chuẩn Br và Gzip --> Web load nhanh hơn
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });
            // Setting Redis                     
            services.AddSingleton<RedisConn>();

            #region SETTING JWT
            //Provide a secret key to Encrypt and Decrypt the Token 
            var SecretKey = Encoding.ASCII.GetBytes(Constants.Secret); // KHÓA PRIVATE
            //Configure JWT Token Authentication - JRozario
            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(token =>
            {
                token.RequireHttpsMetadata = false;
                token.SaveToken = true; // Lưu trữ mã thông báo trong http context để truy cập mã thông báo khi cần thiết trong controller
                token.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(SecretKey), // private key
                    ValidateIssuer = true,
                    //Usually this is your application base URL - JRozario
                    ValidIssuer = Configuration["API_GEN_TOKEN"], // địa chỉ nơi cung cấp dịch vụ
                    ValidateAudience = true,
                    //Here we are creating and using JWT within the same application. In this case base URL is fine 
                    //If the JWT is created using a web service then this could be the consumer URL 
                    ValidAudience = Configuration["API_GEN_TOKEN"], // nơi sử dụng dịch vụ
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
            #endregion END SETTING JWT            

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, RedisConn redisService)
        {

            if (env.IsDevelopment())
            {
                //app.UseStatusCodePagesWithRedirects("/Error/{0}");
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePagesWithRedirects("/Error/{0}");
            }


            //Addd User session  - JWT
            app.UseSession();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            /// Sử dụng các file như css/js/img --> Set cache với thời gian là 1 d
            /// GoLive mới nhảy vào đây
            if (!env.IsDevelopment())
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    OnPrepareResponse = ctx =>
                    {
                        const int durationInSeconds = 60 * 60 * 24;// 60 * 60 * 24 * 30;
                        ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                                "public,max-age=" + durationInSeconds;
                    }
                });
            }

            //Add JWToken to all incoming HTTP Request Header - JWT
            // Nạp lại token đã lưu từ Cookie
            app.Use(async (context, next) =>
            {
                var JWToken = context.Request.Cookies[Constants.USEXPRESS_ACCESS_TOKEN];//Lấy ra token lưu dưới máy client                                                                     
                if (!string.IsNullOrEmpty(JWToken))
                {
                    context.Request.Headers.Add("Authorization", "Bearer " + JWToken);                 
                }
                await next();
            });
            app.UseAuthentication();

            //Redis conn Call the connect method
            redisService.Connect();

            //app.UseEndpoints(endpoints =>
            //{
            //    //endpoints.MapControllerRoute(name: "client",
            //    //pattern: "client/login",
            //    //defaults: new { controller = "Client", action = "Login" });

            //    endpoints.MapControllerRoute(
            //        name: "default",
            //        pattern: "{controller=Home}/{action=Index}/{id?}");
            //});
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("Default", "{controller=Home}/{action=Index}/{id?}");
            });



        }
    }
}
