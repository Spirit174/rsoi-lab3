using Booking.System.ReservationService.Core.Interfaces;
using Booking.System.ReservationService.Core.Services;
using Booking.System.ReservationService.DataBase.Repositories;
using Microsoft.OpenApi.Models;
using Booking.System.ReservationService.Extensions;

namespace Booking.System.ReservationService;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers().AddNewtonsoftJson();
        
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Person.Server", Version = "v1" });

        });
        services.AddSwaggerGenNewtonsoftSupport();
        
        services.AddDbContext(Configuration);
        
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IHotelRepository, HotelRepository>();
        
        services.AddScoped<IReservationService, Core.Services.ReservationService>();
        services.AddScoped<IHotelService, HotelService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSwagger(c =>
        {
            c.RouteTemplate = "/api/v1/swagger/{documentName}/swagger.json";
        });
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/api/v1/swagger/v1/swagger.json", "Person.Server.Http v1");
            c.RoutePrefix = "api/v1/swagger";
        });
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/manage/health", () => Results.Ok(new { status = "Healthy", service = "reservation" }));
        });
    }
}