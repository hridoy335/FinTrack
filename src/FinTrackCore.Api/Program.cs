using FinTrackCore.Api.Middleware;
using FinTrackCore.Application;
using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Common.Models;
using FinTrackCore.Infrastructure;
using FinTrackCore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var messages = context.HttpContext.RequestServices
            .GetRequiredService<Microsoft.Extensions.Options.IOptions<MessageSettings>>()
            .Value;

        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors.Select(e =>
                string.IsNullOrWhiteSpace(e.ErrorMessage)
                    ? $"{x.Key} is invalid."
                    : e.ErrorMessage))
            .ToList();

        var response = new ApiResponse<object?>
        {
            Success = false,
            StatusCode = StatusCodes.Status400BadRequest,
            Message = messages.ValidationFailed,
            Data = errors,
            Meta = null
        };

        return new BadRequestObjectResult(response);
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "FinTrackCore API",
        Version = "v1",
        Description = "FinTrackCore ASP.NET Core API (Clean Architecture)."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT access token."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("postgresql");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!await db.Database.CanConnectAsync())
    {
        throw new InvalidOperationException("Cannot connect to PostgreSQL. Check ConnectionStrings:DefaultConnection.");
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "FinTrackCore API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
