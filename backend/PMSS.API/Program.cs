using PMSS.Infrastructure.DependencyInjection;
using PMSS.Infrastructure.Middleware;
using PMSS.Infrastructure.Data;
using PMSS.API.GraphQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

try
{
    Log.Information("Starting PMSS API application");

    builder.Host.UseSerilog();

    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "PMSS API", Version = "v1" });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Paste JWT token only (without 'Bearer ' prefix)"
        });
        options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", doc),
                new List<string>()
            }
        });
    });

    builder.Services.AddInfrastructure(builder.Configuration);

    // Add GraphQL with query-only support
    builder.Services
        .AddGraphQLServer()
        .AddQueryType<Query>()
        .AddProjections()
        .AddFiltering()
        .AddSorting()
        .SetPagingOptions(new HotChocolate.Types.Pagination.PagingOptions { MaxPageSize = 200, DefaultPageSize = 50 })
        .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = builder.Environment.IsDevelopment());

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            await dbContext.Database.MigrateAsync();
        }
        catch (InvalidOperationException ex) when (app.Environment.IsDevelopment() && ex.Message.Contains("PendingModelChangesWarning"))
        {
            Log.Warning(ex, "Skipping auto-migration in Development due to pending model changes. API will continue to run.");
        }
        catch (Exception ex) when (app.Environment.IsDevelopment())
        {
            Log.Warning(ex, "Auto-migration failed in Development. API will continue to run.");
        }
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseHttpsRedirection();

    app.UseCors("AllowAll");

    // Map GraphQL endpoint (GET only for queries)
    app.MapGraphQL("/graphql");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
