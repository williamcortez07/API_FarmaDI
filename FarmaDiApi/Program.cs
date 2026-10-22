using FarmaDi.DataAccess.Repositories;
using FarmaDiApi.Configurations; 
using FarmaDiApi.Middlewares;    
using FarmaDiApi.Services;       
using FarmaDiBusiness.Interfaces;
using FarmaDiBusiness.Services;
using FarmaDiDataAccess.Interfaces;
using FarmaDiDataAccess.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---  Vinculamos Options Pattern y Servicio de Logs MongoDB ---
builder.Services.Configure<MongoLogSettings>(
    builder.Configuration.GetSection("MongoLogSettings"));

builder.Services.AddSingleton<ILogService, MongoLogService>();
// ---------------------------------------------------------------------

// Add services to the container.
var JwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = JwtSettings["SecretKey"];

// Si estamos en entorno de desarrollo local y no se configuró por variable de entorno, proveemos una clave segura para pruebas locales
if (builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(secretKey))
{
    secretKey = "FarmaDiDevelopmentLocalSecretKey_MustBeAtLeast32BytesLong!";
    builder.Configuration["JwtSettings:SecretKey"] = secretKey;
    builder.Configuration["JWTSettings:SecretKey"] = secretKey;
}

if (string.IsNullOrWhiteSpace(secretKey) || Encoding.UTF8.GetByteCount(secretKey) < 32)
{
    throw new InvalidOperationException(
        "JwtSettings:SecretKey must be configured with a signing key of at least 32 bytes.");
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce tu token JWT usando este formato: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});

// Configuracion del servicio de Autenticacion
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = JwtSettings["Issuer"],

        ValidateAudience = true,
        ValidAudience = JwtSettings["Audience"],

        ValidateLifetime = true,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddScoped<IBrandsService, BrandsService>();
builder.Services.AddScoped<IBrandsRepository, BrandsRepository>();

builder.Services.AddScoped<ICategoriesService, CategoriesService>();
builder.Services.AddScoped<ICategoriesRepository, CategoriesRepository>();

builder.Services.AddScoped<IPresentationService, PresentationService>();
builder.Services.AddScoped<IPresentationRepository, PresentationRepository>();

builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IStockRepository, StockRepository>();

builder.Services.AddScoped<IInventoryLossService, InventoryLossService>();
builder.Services.AddScoped<IInventoryLossRepository, InventoryLossRepository>();

builder.Services.AddScoped<IProductBatchesService, ProductBatchesService>();
builder.Services.AddScoped<IProductBatchesRepository, ProductBatchesRepository>();

builder.Services.AddScoped<IRolService, RolesService>();
builder.Services.AddScoped<IRolesRepository, RolesRepository>();

builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<ISuppliersRepository, SuppliersRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();

builder.Services.AddScoped<IProductService, ProductsService>();
builder.Services.AddScoped<IProductsRepository, ProductsRepository>();

builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();

builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();

builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<ISalesRepository, SalesRepository>();

builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();

builder.Services.AddScoped<IKPIDashboardService, KPIDashboardService>();
builder.Services.AddScoped<IKPIDashboardRepository, DashboardRepository>();

builder.Services.AddScoped<IConcentrationServices, ConcentrationServices>();
builder.Services.AddScoped<IConcentrationRepository, ConcentrationRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("PermitirFrontend");
app.UseAuthentication();

// <---Interceptor de Auditora (despus de Auth para leer el User del JWT y antes de los Controladores) --->
app.UseMiddleware<AuditLoggingMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
