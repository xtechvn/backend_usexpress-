using System;
using System.Text;
using Caching.RedisWorker;
using Entities.ConfigModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Repositories.IRepositories;
using Repositories.Repositories;

namespace WEB.API
{
    public class Startup
    {
        //https://www.syncfusion.com/blogs/post/how-to-build-crud-rest-apis-with-asp-net-core-3-1-and-entity-framework-core-create-jwt-tokens-and-secure-apis.aspx
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {       

            // khoi tao lan dau tien chuoi config khi ung dung duoc chay.
            // no chi die khi ung ung die
            // Get config to instance model
            services.Configure<DataBaseConfig>(Configuration.GetSection("DataBaseConfig"));

            // Register services   
            services.AddSingleton(Configuration);
            services.AddSingleton<IClientRepository, ClientRepository>();
            services.AddSingleton<IOrderRepository, OrderRepository>();
            services.AddSingleton<IProductRepository, ProductRepository>();
            services.AddSingleton<IOrderItemRepository, OrderItemRepository>();
            services.AddSingleton<IImageProductRepository, ImageProductRepository>();
            services.AddSingleton<IProductRepository, ProductRepository>();
            services.AddSingleton<IAllCodeRepository, AllCodeRepository>();
            services.AddSingleton<IPaymentRepository, PaymentRepository>();
            services.AddSingleton<ICommonRepository, CommonRepository>();
            services.AddSingleton<ILabelRepository, LabelRepository>();
            services.AddSingleton<IVoucherRepository, VoucherRepository>();
            services.AddSingleton<IGroupProductRepository, GroupProductRepository>();
            services.AddSingleton<IGroupProductStoreRepository, GroupProductStoreRepository>();
            services.AddSingleton<IProductClassificationRepository, ProductClassificationRepository>();
            services.AddSingleton<IArticleRepository, ArticleRepository>();
            services.AddSingleton<IOrderProgressRepository, OrderProgessRepository>();
            services.AddSingleton<ICampaignAdsRepository, CampaignAdsRepository>();
            services.AddSingleton<IAffiliateGroupProductRepository, AffiliateGroupProductRepository>();
            services.AddSingleton<ITagRepository, TagRepository>();
            services.AddSingleton<IAutomaticPurchaseAmzRepository, AutomaticPurchaseAmzRepository>();
            services.AddSingleton<IAutomaticPurchaseHistoryRepository, AutomaticPurchaseHistoryRepository>();

            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressConsumesConstraintForFormFileParameters = true;
                    options.SuppressInferBindingSourcesForParameters = true;
                    options.SuppressModelStateInvalidFilter = true;
                    options.SuppressMapClientErrors = true;
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                });

            // Set session
            services.AddDistributedMemoryCache();

            // Setting Redis                     
            services.AddSingleton<RedisConn>();

            services.AddSession(option =>
            {
                // Set a short timeout for easy testing.
                option.IdleTimeout = TimeSpan.FromDays(1);
                option.Cookie.HttpOnly = true;
                // Make the session cookie essential
                option.Cookie.IsEssential = true;
            });


            services.AddCors(o => o.AddPolicy("MyApi", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

           // services.AddResponseCaching();

            //Configure authorization middleware in the startup configureService method.
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = Configuration["Jwt:Audience"],
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                };
            });

           

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, RedisConn redisService)
        {
            
            //app.Run(context => {
            //    return context.Response.WriteAsync("Hello Readers!");
            //});

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }            

            app.UseHttpsRedirection();

            app.UseRouting();

         

            // Inject the authorization middleware into the Request pipeline.
            app.UseAuthentication();
            app.UseAuthorization();

            //Redis conn Call the connect method
            redisService.Connect();
            app.UseCors("MyApi");

            //app.UseResponseCaching();
            //app.Use(async (context, next) =>
            //{
            //    context.Response.GetTypedHeaders().CacheControl =
            //        new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
            //        {
            //            Public = true,
            //            MaxAge = TimeSpan.FromSeconds(10)
            //        };
            //    context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
            //        new string[] { "Accept-Encoding" };

            //    await next();
            //});

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }


    }
}
