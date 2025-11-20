using AzureSearch.WebApp.Publico.Utils.Config;
using AzureSearch.WebApp.Publico.Utils.Query;
using CEAM.AzureSearch.WebApp.FileManager;
using CEAM.AzureSearch.WebApp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace CEAM.AzureSearch.WebApp
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
            services.AddDistributedMemoryCache();

            //services.AddSession(options =>
            //{
            //    options.IdleTimeout = TimeSpan.FromSeconds(36000);
            //    options.Cookie.HttpOnly = true;
            //    options.Cookie.IsEssential = true;
            //});

            services.AddControllersWithViews();
            services.AddScoped<IExcelBase, ExcelBase>();
            services.AddScoped<IExcelService, ExcelService>();
            services.AddScoped<IAzureSearchService, AzureSearchService>();
            services.AddApplicationInsightsTelemetry("5c754488-0842-4151-8e6e-7693a85334bd");

            // Registros para JSON y normalización de consulta
            services.AddSingleton<IConfigLoader, ConfigLoader>();
            services.AddSingleton<IQueryNormalizer, QueryNormalizer>();

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
                app.UseExceptionHandler("/Search/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            //app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Search}/{action=Index}/{id?}");
            });
        }
    }
}
