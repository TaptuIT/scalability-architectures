using CentralCommand.Api.DataAccess;
using CentralCommand.Api.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


public class Startup
{

    public IConfiguration Configuration { get; }

    public Startup(IWebHostEnvironment env)
    {
        // Build Configuration from 3 files that each override previous values. 
        var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                // base prod appsettings (pipelines should replace values here)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                // dev specific app settings that are stored in the repository
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                // local appsettings file for secrets
                .AddJsonFile("appsettings.Local.json", optional: true)
                .AddEnvironmentVariables();

        Configuration = builder.Build();
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add services to the container.
        services.AddControllers();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // DI
        services.AddScoped<ICentralCommandDataAccess, CentralCommandDataAccess>();
        services.AddSingleton(Configuration.GetSection(DBSettings.Name).Get<DBSettings>());
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
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