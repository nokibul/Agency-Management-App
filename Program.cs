using System.Net.Cache;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection.Metadata.Ecma335;
using Agency.Models.MyContext;
using Microsoft.EntityFrameworkCore;
using Agency.Models.DTOs;
using FluentValidation;
using FluentValidation.AspNetCore;
using Agency.Models.Validators;
using Agency.Data.Repositories;
using Agency.Interfaces;
using Agency.Models.Entities.Agency;
using Agency.Models.MappingProfile;
using AutoMapper;
using System.Security.Claims;
using Agency.Configuration.Jwt;
using Agency.Services.Authentication.User;
using Agency.Interfaces;
using Agency.Services.Authentication.Jwt;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<MyContext>(DbContextOptionsBuilder =>
{
    DbContextOptionsBuilder.UseNpgsql("Host=localhost;Database=postgres;Username=postgres;Password=nokib");
});

var jwtOptionsSection = builder.Configuration.GetRequiredSection("JwtSettings");
builder.Services.Configure<JwtOptions>(jwtOptionsSection);

// jwt auth configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(jwtOptions =>
{
    var configurationKey = jwtOptionsSection["Key"];
    var key = Encoding.UTF8.GetBytes(configurationKey);
    jwtOptions.SaveToken = true;
    jwtOptions.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = jwtOptionsSection["Issuer"],
        ValidAudience = jwtOptionsSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };

});

builder.Services.AddAuthorization();

builder.Services.AddAutoMapper(typeof(Program));

// repository registration
builder.Services.AddScoped<IAgencyRepository, AgencyRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// service registration
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddFluentValidation(conf =>
{
    conf.RegisterValidatorsFromAssembly(typeof(Program).Assembly);
    conf.AutomaticValidationEnabled = false;
});


// builder.Services.AddTransient<IValidator<CreateAgencyRequestDTO>, CreateAgencyRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
