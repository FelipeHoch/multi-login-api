using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using multi_login.Services;
using System.Text;

namespace multi_login;

internal static class StartupHelperExtensions
{
    // Add services to the container.
    public static WebApplication ConfigureServices(
        this WebApplicationBuilder builder)
    {
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
                (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
            };
        });

        builder.Services.AddAuthorization();

        builder.Services.AddMongoRepository(
            builder.Configuration.GetSection(
                nameof(MongoRepositoryOptions)).Get<MongoRepositoryOptions>());

        builder.Services.AddControllers();

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
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers().RequireAuthorization();

        return app;
    }
}

