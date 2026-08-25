using System.Text;
using InventoryManager.API.Data;
using InventoryManager.API.Interfaces;
using InventoryManager.API.Middleware;
using InventoryManager.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ==============================
// قاعدة البيانات
// ==============================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ==============================
// الخدمات
// ==============================

builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<IStockMovementService, StockMovementService>();

builder.Services.AddScoped<IAuthService, AuthService>();

// ==============================
// المصادقة باستخدام JWT
// ==============================

var jwtSettings = builder.Configuration.GetSection("Jwt");

var jwtKey = jwtSettings["Key"]
             ?? throw new InvalidOperationException(
                 "JWT key is missing.");

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey))
            };
    });

builder.Services.AddAuthorization();

// ==============================
// Controllers
// ==============================

builder.Services.AddControllers();

// ==============================
// Swagger
// ==============================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Enter: Bearer {your JWT token}"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});

// ==============================
// بناء التطبيق
// ==============================

var app = builder.Build();

// ==============================
// معالجة الأخطاء
// ==============================

app.UseMiddleware<ExceptionHandlingMiddleware>();

// ==============================
// Swagger
// ==============================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ==============================
// HTTPS
// ==============================

app.UseHttpsRedirection();

// ==============================
// Authentication
// ==============================

app.UseAuthentication();

// ==============================
// Authorization
// ==============================

app.UseAuthorization();

// ==============================
// Controllers
// ==============================

app.MapControllers();

app.Run();