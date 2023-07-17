using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Neo4jClient;
using System;

namespace Series.GraphDatabase
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

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Series.GraphDatabase", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Series.GraphDatabase v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private IServiceCollection RegisterNeo4jDatabase(IServiceCollection services)
        {
            services.AddSingleton(typeof(IDriver), resolver =>
            {
                var uri = Environment.GetEnvironmentVariable("NEO4J_HOST");
                var user = Environment.GetEnvironmentVariable("NEO4J_USER");
                var password = Environment.GetEnvironmentVariable("NEO4J_PASSWORD");

                var authToken = AuthTokens.Basic(user, password);
                var driver = GraphDatabase.Driver(uri, authToken);
                return driver;
            });

            services.AddSingleton(typeof(IBoltGraphClient), resolver =>
            {
                var client = new BoltGraphClient(services.BuildServiceProvider().GetService<IDriver>());

                if (!client.IsConnected)
                {
                    client.ConnectAsync().GetAwaiter().GetResult();
                }

                return client;
            });

            //services.AddScoped<Neo4jContext>(sp =>
            //{
            //    var driver = sp.GetRequiredService<IDriver>();
            //    var boltGraphClient = sp.GetRequiredService<IBoltGraphClient>();
            //    return new Neo4jContext(driver, boltGraphClient, DatabaseName);
            //});

            return services;
        }

        private void AddNeo4jDAL(IServiceCollection services)
        {
            //services.AddScoped<ICountryDAL, CountryDAL>();
            //services.AddScoped<ILangDAL, LangDAL>();
            //services.AddScoped<IUserDAL, UserDAL>();
            //services.AddScoped<IPostDAL, PostDAL>();
            //services.AddScoped<IVocabPackageDAL, VocabPackageDAL>();
        }
    }
}
