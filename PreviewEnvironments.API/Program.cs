using PreviewEnvironments.API.Endpoints;
using PreviewEnvironments.API.Hosting;
using PreviewEnvironments.Application.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication(builder.Configuration);

builder.Services.Configure<HostOptions>(x =>
{
    x.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
    x.ServicesStartConcurrently = true;
    x.ServicesStopConcurrently = false;
});

builder.Services.AddHostedService<AppLifetimeService>();
builder.Services.AddHostedService<ContainerCleanUpBackgroundService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapEndpoints();

app.Run();
