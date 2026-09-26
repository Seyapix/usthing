using Microsoft.EntityFrameworkCore;
using Timetable.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TimetableContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Timetable")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TimetableContext>();
    context.Database.Migrate();
}

app.MapGet("/health", () => new { status = "ok" });
app.MapControllers();

app.Run();
