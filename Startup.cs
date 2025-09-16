using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.IO.Compression;
using ReportServices.Models;
using Microsoft.AspNetCore.ResponseCompression;
using System.Text;
using Bold.Licensing;
using BoldReports.Web;
using Newtonsoft.Json;
using System.Reflection;
using Samples.Core.Logger;

using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.CookiePolicy;

namespace ReportServices
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment _hostingEnvironment)
        {
            log4net.GlobalContext.Properties["LogPath"] = _hostingEnvironment.ContentRootPath;
            LogExtension.RegisterLog4NetConfig();
            string License = File.ReadAllText(Path.Combine(_hostingEnvironment.ContentRootPath, "BoldLicense.txt"), Encoding.UTF8);
            BoldLicenseProvider.RegisterLicense(License, bool.Parse(configuration.GetSection("appSettings").GetSection("IsOfflineLicense").Value), bool.Parse(configuration.GetSection("appSettings").GetSection("EnableLicenseLog").Value));
            ReportConfig.DefaultSettings = new ReportSettings()
            {
                MapSetting = this.GetMapSettings(_hostingEnvironment)
            }.RegisterExtensions(this.GetDataExtension(configuration.GetSection("appSettings").GetSection("ExtAssemblies").Value));

            Configuration = configuration;
            env = _hostingEnvironment;
        }

        private List<string> GetDataExtension(string ExtAssemblies)
        {
            var extensions = !string.IsNullOrEmpty(ExtAssemblies) ? ExtAssemblies : string.Empty;
            try
            {
                var ExtNames = new List<string>(extensions.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
                List<string> ExtCollections = new List<string>();
                ExtNames.ForEach(Extension => ExtCollections.Add(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, Extension + ".dll")));
                return ExtCollections;
            }
            catch (Exception ex)
            {
                LogExtension.LogError("Failed to Load Data Extension", ex, MethodBase.GetCurrentMethod());
            }
            return null;
        }

        public IWebHostEnvironment env { get; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddMemoryCache();
            services.AddResponseCompression();
            services.AddCors(o => o.AddPolicy("AllowAllOrigins", builder =>
            {
                builder.AllowAnyOrigin();
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();

            }));
            services.AddControllers(); // Add MVC services
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true; // Enable compression for HTTPS requests
                options.Providers.Add<GzipCompressionProvider>();
            });
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest; // Adjust compression level as needed
            });
        }

        private BoldReports.Web.MapSetting GetMapSettings(IWebHostEnvironment _hostingEnvironment)
        {
            try
            {
                string basePath = _hostingEnvironment.WebRootPath;
                string mapShapePath = Path.Combine(basePath, "ShapeData");
                var mapFileInfo = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(mapShapePath).GetFileInfo("mapshapes.txt");
                using var mapReader = new System.IO.StreamReader(mapFileInfo.CreateReadStream());
                return new MapSetting()
                {
                    ShapePath = mapShapePath + '/',
                    MapShapes = JsonConvert.DeserializeObject<List<MapShape>>(mapReader.ReadToEnd())
                };
            }
            catch (Exception ex) { Console.WriteLine(ex); }
            return null;
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
                // In production, handle errors more gracefully, e.g., log and display a generic error page.
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.Use(async (context, next) =>
            {
                context.Response.Headers["Referrer-Policy"] = "no-referrer-when-downgrade";
                await next();
            });
            app.UseCookiePolicy();
            app.UseRouting();
            app.UseCors("AllowAllOrigins");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                name: "default",
                pattern: "api/{controller}/{id?}");
            });
            app.UseResponseCompression();
        }
    }
}
