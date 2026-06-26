using AgileFlow.Application.Interfaces;
using AgileFlow.Domain.Entities;
using AgileFlow.Infrastructure.Persistence.Data;
using AgileFlow.Infrastructure.Repositories;
using AgileFlow.Infrastructure.Services;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Mappings;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ── MVC + Swagger ─────────────────────────────────────────────────────────────
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste your JWT here. Example: eyJhbGci..."
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                    { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
            });
        });

        // ── Database ──────────────────────────────────────────────────────────────────
        builder.Services.AddDbContext<AgileFlowDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // ── Identity ──────────────────────────────────────────────────────────────────
        builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AgileFlowDbContext>()
        .AddDefaultTokenProviders();

        // ── JWT ───────────────────────────────────────────────────────────────────────
        var jwt = builder.Configuration.GetSection("Jwt");

        builder.Services.AddAuthentication(options =>
        {
            // Fix: both scheme defaults must be JwtBearer, not default Identity cookies
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt["Issuer"],
                ValidAudience = jwt["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                                               Encoding.UTF8.GetBytes(jwt["Key"] ?? string.Empty)),
                ClockSkew = TimeSpan.FromSeconds(30),
            };
        });

        // ── Authorization policies ────────────────────────────────────────────────────
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
            options.AddPolicy("TeamLeadPlus", p => p.RequireRole("Admin", "TeamLead"));
        });
        // ── AutoMapper ──────────────────────────────────────────────────────
        builder.Services.AddAutoMapper(typeof(WorkspaceProfile).Assembly);

        // ──  Repositories ──────────────────────────────────────────────────────
        builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
        builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
        builder.Services.AddScoped<ITaskRepository, TaskRepository>();

        // ── Application services ──────────────────────────────────────────────────────
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IWorkspaceAuthorizationService, WorkspaceAuthorizationService>();
        builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
        builder.Services.AddScoped<IProjectService, ProjectService>();
        builder.Services.AddScoped<ITaskService, TaskService>();

        // ── CORS (Vite dev server) ────────────────────────────────────────────────────
        builder.Services.AddCors(options =>
            options.AddPolicy("DevFrontend", policy =>
                policy.WithOrigins("http://127.0.0.1:5500/frontend/AuthTest.html")
                      .AllowAnyHeader()
                      .AllowAnyMethod()));

        // ─────────────────────────────────────────────────────────────────────────────
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "SolKey API v1");
                options.RoutePrefix = string.Empty;
            });
            app.UseCors("DevFrontend");
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();   // must be before UseAuthorization
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}       
