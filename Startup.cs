// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
using System;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using Catalog.Repositories;
using Catalog.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.HttpsPolicy;
// using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Catalog
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
      // Enable Cors
      services.AddCors(c =>
      {
        c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
      });

      // Following line will serialize the Guid type to String in db and DateTimeOffset as well
      BsonSerializer.RegisterSerializer(new GuidSerializer(MongoDB.Bson.BsonType.String));
      BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(MongoDB.Bson.BsonType.String));
      // *Singleton* makes one copy for the entire life time
      var mongoDbSettings = Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();

      services.AddSingleton<IMongoClient>(serviceProvider =>
      {
        // I've moved this line above
        // var settings = Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
        return new MongoClient(mongoDbSettings.ConnectionString);
      });

      // services.AddSingleton<IItemsRepository, InMemoryRepository>();
      services.AddSingleton<IItemsRepository, MongoDbItemsRepository>();

      services.AddControllers(options =>
      {
        options.SuppressAsyncSuffixInActionNames = false;
      });
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog", Version = "v1" });
      });

      services.AddHealthChecks()
      .AddMongoDb(
        mongoDbSettings.ConnectionString,
        name: "Mongodb",
        timeout: TimeSpan.FromSeconds(3),
        tags: new[] { "ready" }
      );
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      // Enable cors
      app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog v1"));
      }

      if (env.IsDevelopment()) {
        app.UseHttpsRedirection();
      }

      app.UseRouting();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
        // this is for mongodb, as mongodb has ready tag
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
          Predicate = (check) => check.Tags.Contains("ready"),
          ResponseWriter = async (context, report) =>
          {
            var result = JsonSerializer.Serialize(
             new
             {
               status = report.Status.ToString(),
               checks = report.Entries.Select(entry => new
               {
                 name = entry.Key,
                 status = entry.Value.Status.ToString(),
                 exception = entry.Value.Exception != null ?
                   entry.Value.Exception.Message : "none",
                 duration = entry.Value.Duration.ToString()
               })
             }
           );
            context.Response.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsync(result);
          }
        });
        // Following health check is only for apis
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
          Predicate = (_) => false
        });
      });
    }
  }
}
