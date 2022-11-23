using multi_login;

var builder = WebApplication.CreateBuilder(args);

var app = builder
            .ConfigureServices()
            .ConfigurePipeline();

// run the app.
app.Run();