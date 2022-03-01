// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
using Catalog.Repositories;
using Catalog.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

      app.UseHttpsRedirection();

      app.UseRouting();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
    }
  }
}