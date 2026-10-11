using System.Text.Json.Serialization;
using CURSOVA.Application;
using CURSOVA.Infrastructure;
using CURSOVA.Infrastructure.Persistence;
using CURSOVA.Infrastructure.Persistence.Seeding;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MagazineDbContext>();
    await MagazineSeeder.InitializeAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/api"), api => api.UseExceptionHandler());
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("TechPulse API"));
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
