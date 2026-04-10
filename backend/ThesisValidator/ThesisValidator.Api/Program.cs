using System.Text.Json.Serialization;
using ThesisValidator.Api.Configuration;
using ThesisValidator.BLL.Docx;
using ThesisValidator.BLL.Interfaces;
using ThesisValidator.BLL.Services;
using ThesisValidator.DAL;
using ThesisValidator.DAL.Docx;
using ThesisValidator.DAL.JSON;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DAL
builder.Services.AddScoped<IDocumentParsingService, DocxParsingService>();
builder.Services.AddScoped<IRuleRepository, JsonRuleRepository>();

// BLL
builder.Services.AddScoped<IRuleEvaluator, RuleEvaluator>();
builder.Services.AddScoped<IDocumentValidator, DocxValidator>();
builder.Services.AddScoped<IValidationEngine, ValidationEngine>();

// CORS
var corsOptions = builder.Configuration.GetSection("Cors").Get<CorsOptions>();
if (corsOptions?.AllowedOrigins == null)
{
    throw new InvalidOperationException("Cors:AllowedOrigins is not configured.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(corsOptions!.AllowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Swagger
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
