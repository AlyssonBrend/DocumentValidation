using DocumentValidator.Application.Services;
using DocumentValidator.Application.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<CpfValidator>();
builder.Services.AddScoped<PassportMrzValidator>();
builder.Services.AddScoped<ValidationService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();