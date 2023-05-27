using System;
using System.IO;
using System.Security.Claims;
using atakafe_api.Controllers;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace atakafe_api
{
    public class Startup
    {
        private const string AuthHost = "https://atakafe.com/auth";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.RegisterDataServices();
            services.RegisterServices();
            services.AddSingleton<IChannelQueueService<FbUpdateToken>, ChannelQueueService<FbUpdateToken>>();
            services.AddSingleton<IChannelQueueService<FbUpdateObject>, ChannelQueueService<FbUpdateObject>>();
            services.AddSingleton<IChannelQueueService<UserActivity>, ChannelQueueService<UserActivity>>();
            services.AddSingleton<IChannelQueueService<HookObject>, ChannelQueueService<HookObject>>();
            services.AddHostedService<FbMessageBackgroundService>();
            services.AddHostedService<FbTokenBackgroundService>();
            services.AddHostedService<ActivityBackgroundService>();
            services.AddHostedService<ZaloMessageBackgroundService>();
            services.AddHttpClient<FbMessageBackgroundService>();
            services.AddHttpClient<FbTokenBackgroundService>();
            services.AddHttpClient<ZaloMessageBackgroundService>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "ISale API"
                });
            });
            services.AddMvcCore()
                .AddAuthorization()
                .AddJsonFormatters();

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = AuthHost;
                    //options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false;

                    options.ApiName = "atakafe.api";
                    options.RoleClaimType = ClaimTypes.Role;
                });
            services.AddSession(options =>
            {
                options.Cookie.Name = ".ISale.Session";
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.IsEssential = true;
            });

            services.AddCors();
            services.AddTransient <DataController, DataController>();
            services.AddTransient <AccountController, AccountController>();

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
                app.UseHsts();
            }

            app.UseStaticFiles();

            // Shows UseCors with CorsPolicyBuilder.
            app.UseCors(builder =>
                builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseSession();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ISale API V1");
            });

            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Firebase.json")),
            });
        }
    }
}
