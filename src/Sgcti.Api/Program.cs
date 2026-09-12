using Microsoft.EntityFrameworkCore;
using Sgcti.Core.Abstractions;
using Sgcti.Core.Services;
using Sgcti.Infrastructure.Persistence;
using Sgcti.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SgctiDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SgctiDB")));

builder.Services.AddScoped<IImpresoraRepositorio, ImpresoraRepositorio>();
builder.Services.AddScoped<ServicioAnalisisPredictivo>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();

    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();