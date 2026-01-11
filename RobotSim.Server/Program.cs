using Microsoft.AspNetCore.Hosting.Server.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Register the simulator as a singleton so state persists for the session
builder.Services.AddSingleton<RobotSim.Server.Services.RobotSimulator>();

// Add CORS to allow requests from Vite dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVite", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "https://localhost:5173",
            "http://127.0.0.1:5173",
            "https://127.0.0.1:5173"
        )
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Logging for diagnosis ---
var loggerFactory = app.Services.GetService<ILoggerFactory>();
var logger = loggerFactory?.CreateLogger("Startup") ?? app.Logger;
logger.LogInformation("===== RobotSim.Server startup log BEGIN =====");
logger.LogInformation("Environment: {env}", app.Environment.EnvironmentName);
logger.LogInformation("ContentRootPath: {path}", app.Environment.ContentRootPath);
logger.LogInformation("WebRootPath: {path}", app.Environment.WebRootPath ?? "<null>");
logger.LogInformation("Command line args: {args}", string.Join(' ', args ?? Array.Empty<string>()));
logger.LogInformation("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES: {val}", Environment.GetEnvironmentVariable("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES") ?? "<null>");
logger.LogInformation("DOTNET_RUNNING_IN_CONTAINER: {val}", Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") ?? "<null>");

// Log launchSettings.json contents if present
try
{
    var launchPath = Path.Combine(app.Environment.ContentRootPath, "Properties", "launchSettings.json");
    if (File.Exists(launchPath))
    {
        var txt = File.ReadAllText(launchPath);
        var snippet = txt.Length > 2000 ? txt[..2000] + "..." : txt;
        logger.LogInformation("Found launchSettings.json: {path}\n{content}", launchPath, snippet);
    }
    else
    {
        logger.LogInformation("launchSettings.json not found at {path}", launchPath);
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "Error reading launchSettings.json");
}

// Log csproj SpaProxy settings if present
try
{
    var csprojPath = Path.Combine(app.Environment.ContentRootPath, "RobotSim.Server.csproj");
    if (File.Exists(csprojPath))
    {
        var csproj = File.ReadAllText(csprojPath);
        var snippet = csproj.Length > 2000 ? csproj[..2000] + "..." : csproj;
        logger.LogInformation("Found csproj: {path}\n{content}", csprojPath, snippet);
    }
    else
    {
        logger.LogInformation("csproj not found at {path}", csprojPath);
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "Error reading csproj");
}

// Log environment variables that affect SPA proxy
try
{
    var keys = new[] { "ASPNETCORE_URLS", "ASPNETCORE_ENVIRONMENT", "ASPNETCORE_HOSTINGSTARTUPASSEMBLIES" };
    foreach (var k in keys)
    {
        logger.LogInformation("ENV {key}={val}", k, Environment.GetEnvironmentVariable(k) ?? "<null>");
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "Error reading environment variables");
}

string addressSeparator = ", ";

// Register application lifetime events to log addresses and shutdown reasons
app.Lifetime.ApplicationStarted.Register(() =>
{
    try
    {
        // Log server addresses
        var addressesFeature = app.Services.GetService<IServerAddressesFeature>();
        if (addressesFeature != null)
        {
            logger.LogInformation("Server listening on: {addresses}", string.Join(addressSeparator, addressesFeature.Addresses));
        }
        else
        {
            logger.LogInformation("IServerAddressesFeature not available. WebApplication.Urls: {urls}", string.Join(addressSeparator, app.Urls));
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error logging server addresses on ApplicationStarted");
    }
});

app.Lifetime.ApplicationStopping.Register(() => logger.LogWarning("ApplicationStopping called"));
app.Lifetime.ApplicationStopped.Register(() => logger.LogWarning("ApplicationStopped called"));
logger.LogInformation("===== RobotSim.Server startup log END =====");
// --- End logging ---

app.UseDefaultFiles();
app.UseStaticFiles();

// Use CORS
app.UseCors("AllowVite");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global exception handler - return JSON error responses
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var errorResponse = new
        {
            success = false,
            message = "Internal server error",
            error = app.Environment.IsDevelopment() ? exception?.Error?.Message : null
        };

        await context.Response.WriteAsJsonAsync(errorResponse);
    });
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
