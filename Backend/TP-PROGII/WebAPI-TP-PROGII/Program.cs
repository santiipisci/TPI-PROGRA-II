using Microsoft.EntityFrameworkCore;
using tpRepository.Models;
using tpRepository.Repository.Implementations;
using tpRepository.Repository.Interfaces;
using tpRepository.Services.Implementations;
using tpRepository.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//DbContext
builder.Services.AddDbContext<AutopartesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//Inyecciones
builder.Services.AddScoped<IAutopartesRepository, AutopartesRepository>();
builder.Services.AddScoped<IAutopartesService, AutopartesService>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
