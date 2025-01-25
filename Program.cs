using minesweeperAPI.ApplicationCore.DomModels;
using minesweeperAPI.ApplicationCore.Interfaces.Services;
using minesweeperAPI.Infrastructure.BLL.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});


// Add services to the container.

builder.Services.AddDbContext<MinesweeperContext>();
builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler
                                                                        = ReferenceHandler.IgnoreCycles);
builder.Services.AddScoped<IGameService, GameService>();



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
