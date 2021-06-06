using Downtown.Data;
using Downtown.Data.Repositories;
using Downtown.Soap.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SoapCore;
using System;
using System.ServiceModel;
using System.Xml;

namespace Downtown.Soap
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
            var connectionString = "server=downtown-db.mariadb.database.azure.com;user=todor_andonov@downtown-db;password=password1!; database =downtown_db";
            var serverVersion = new MariaDbServerVersion(new Version(10, 3));

            services.AddDbContext<DowntownDbContext>(options =>
                options.UseMySql(connectionString, serverVersion, mysqlOptions => mysqlOptions.MigrationsAssembly("Downtown.Data")));

            services.AddSoapCore();
            services.AddMvc();

            services.AddScoped<IUnitOfWork, DowntownDbContext>();
            services.AddScoped<ICityRepository, CityRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IEventService, EventService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.UseSoapEndpoint<IEventService>("/ServicePath.asmx", new BasicHttpBinding
                {
                    MaxReceivedMessageSize = long.MaxValue,
                    ReaderQuotas = XmlDictionaryReaderQuotas.Max
                },
                SoapSerializer.XmlSerializer);
            });
        }
    }
}