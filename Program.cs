using multi_login;
using Serilog.Events;
using Serilog;

Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.MongoDB(
                Environment.GetEnvironmentVariable("MONGODB_URI_LOG"),
                collectionName: "logs",
                restrictedToMinimumLevel: LogEventLevel.Information,
                period: TimeSpan.FromSeconds(1))
            .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

var app = builder
            .ConfigureServices()
            .ConfigurePipeline();

// run the app.
app.Run(); 