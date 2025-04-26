// Program.cs
using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using StackExchange.Redis;
using renework.Helpers;
using renework.MongoDB.Context;
using renework.MongoDB.Collections;
using renework.Repositories;
using renework.Repositories.Interfaces;
using Microsoft.OpenApi.Models;
using renework.MongoDB;

var builder = WebApplication.CreateBuilder(args);

// 1) Bind Settings
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("MongoSettings"));
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<RedisSettings>(
    builder.Configuration.GetSection("RedisSettings"));

// 2) MongoDB
var mongoSettings = builder.Configuration
    .GetSection("MongoSettings")
    .Get<MongoSettings>();

builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(mongoSettings.ConnectionString));
builder.Services.AddSingleton<MongoDbContext>();

// 3) Mongo Collections for DI
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<MongoDbContext>().Users);
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<MongoDbContext>().Courses);
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<MongoDbContext>().AppliedCourses);
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<MongoDbContext>().Applications);
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<MongoDbContext>().CourseReviews);

// 4) Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IAppliedCourseRepository, AppliedCourseRepository>();
builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddScoped<ICourseReviewRepository, CourseReviewRepository>();

// 5) JWT
builder.Services.AddSingleton<JwtTokenGenerator>();
var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>();
var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.Key);

builder.Services
  .AddAuthentication(options =>
  {
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
          ValidIssuer = jwtSettings.Issuer,
          ValidAudience = jwtSettings.Audience,
          IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
      };
  });
builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    opts.AddPolicy("UserOnly", p => p.RequireRole("User"));
});

// 6) Redis
var redisSettings = builder.Configuration
    .GetSection("RedisSettings")
    .Get<RedisSettings>();

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(redisSettings.Configuration));

builder.Services.AddStackExchangeRedisCache(opts =>
{
    opts.Configuration = redisSettings.Configuration;
    opts.InstanceName = redisSettings.InstanceName;
});
builder.Services.AddScoped<renework.Services.ICacheService,
                           renework.Services.RedisCacheService>();

// 7) MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Renework API",
        Version = "v1",
        Description = "Course & application platform"
    });

    // XML docs
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    // JWT in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme, Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Renework API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
