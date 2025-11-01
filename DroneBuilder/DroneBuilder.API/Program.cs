using System.Text;
using DroneBuilder.Application;
using DroneBuilder.Application.Mediator.Commands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Options;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DroneBuilder.API;

public abstract class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSwaggerGen();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services
            .AddIdentity<User, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;
        });

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = builder.Configuration["JwtOptions:Issuer"],
                ValidAudience = builder.Configuration["JwtOptions:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey
                    (Encoding.UTF8.GetBytes(builder.Configuration["JwtOptions:Key"]!)),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true
            };
        });

        builder.Services.Configure<JwtOptions>(
            builder.Configuration.GetSection(nameof(JwtOptions)));

        builder.Services.AddAuthorization();

        builder.Services.AddOpenApi();

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapPost("/signup",
            async (IMediator mediator, SignUpModel model, CancellationToken cancellationToken) =>
                await mediator.ExecuteCommandAsync(new SignUpUserCommand(model), cancellationToken));

        app.MapPost("/signin",
            async (IMediator mediator, SignInModel model, CancellationToken cancellationToken) =>
            {
                var result = await mediator.ExecuteCommandAsync<SignInCommand, AuthUserModel>(
                    new SignInCommand(model.Email, model.Password), 
                    cancellationToken);
                return Results.Ok(result);
            });

        app.Run();
    }
}