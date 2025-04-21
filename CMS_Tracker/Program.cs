using System.Text;
using CMS_Tracker.Email;
using CMS_Tracker.Middleware;
using CMS_Tracker.Models;
using CMS_Tracker.Repositories;
using CMS_Tracker.Repositories.Interfaces;
using CMS_Tracker.Security;
using CMS_Tracker.Services;
using CMS_Tracker.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace CMS_Tracker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CMS API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' followed by your JWT token.",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
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

            builder.Services.AddDbContext<CMS_DBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            builder.Services.AddScoped<IMenuService, MenuService>();
            builder.Services.AddScoped<IPermissionService, PermissionService>();
            builder.Services.AddScoped<AdminSeeder>();

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ITokenRepository, TokenRepository>();
            builder.Services.AddScoped<IBlacklistRepository, BlacklistRepository>();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogWarning("JWT Auth Failed: {Message}", context.Exception.Message);
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogInformation("JWT Challenge Triggered");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogInformation("JWT Token Validated Successfully");
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
            var startupLogger = loggerFactory.CreateLogger<Program>();
            startupLogger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError(error, "Global Exception");
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync(app.Environment.IsDevelopment()
                        ? error?.Message ?? "An error occurred."
                        : "An unexpected error occurred.");
                });
            });

            using (var scope = app.Services.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
                await seeder.SeedAsync();
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CMS API V1");
                    c.RoutePrefix = "swagger";
                });
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMiddleware<TokenBlacklistMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
