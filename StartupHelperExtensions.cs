using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using multi_login.Services;
using Newtonsoft.Json.Serialization;
using Serilog;
using System.Text;

namespace multi_login;

internal static class StartupHelperExtensions
{

    // Add services to the container.
    public static WebApplication ConfigureServices(
        this WebApplicationBuilder builder)
    {

        builder.WebHost.UseKestrel(options =>
        {
            options.AddServerHeader = false;
        });

        builder.Host.UseSerilog();

        builder.WebHost.UseSerilog();

        builder.Services.AddAntiforgery();


        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey
                (Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY"))),
                ValidAudiences = Environment.GetEnvironmentVariable("JWT_AUDIENCES").Split(","),
                ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
            };
        });

        builder.Services.AddAuthorization(auth =>
        {
            auth.AddPolicy(JwtBearerDefaults.AuthenticationScheme,
                new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build());
        });

        builder.Services.AddMongoRepository(new MongoRepositoryOptions
        {
            ClientName = Environment.GetEnvironmentVariable("MONGODB_CLIENT_NAME"),
            ConnectionString = Environment.GetEnvironmentVariable("MONGODB_URI")
        });

        builder.Services.AddControllers().AddNewtonsoftJson(setupAction =>
        {
            setupAction.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();
        });


        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(op =>
        {
            op.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Authentication API",
                Version = "v1",
                Description = "API for auth with email/password, Google auth and Microsoft auth, also contains an admin system for handle users",
                Contact = new OpenApiContact
                {
                    Name = "Felipe Hoch",
                    Email = "hochfelipe@gmail.com"
                }
            });

            op.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization Header. \r\n\r\n" +
                    "Type 'Bearer' and your token on the right side, as in the example: Bearer {your token}",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });

            op.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
            });
        });

        builder.Services.AddRateLimiter(_ => _
            .AddFixedWindowLimiter(policyName: "fixed", options =>
            {
                options.PermitLimit = 100;
                options.Window = TimeSpan.FromSeconds(60);
            })
        );

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddScoped<IJwtService, JwtService>();

        builder.Services.AddScoped<IUserRepository, UserRepository>();

        builder.Services.AddAutoMapper(
            AppDomain.CurrentDomain.GetAssemblies());

        return builder.Build();
    }

    // Configure the request/response pipeline.
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseCors(builder =>
                builder
                .SetIsOriginAllowed(s => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin()
                );

            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseSerilogRequestLogging();

	    app.UseCors(builder =>
                builder
                .WithOrigins(Environment.GetEnvironmentVariable("LOGIN_CLIENT"))
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .SetIsOriginAllowed(x => true)
                .AllowAnyHeader()
                .AllowAnyMethod()                
                );

        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseSecurityHeaders();
        app.UseHsts();

        app.MapControllers().RequireAuthorization();

        return app;
    }
}

