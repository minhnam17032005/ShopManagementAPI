using System;
using Microsoft.EntityFrameworkCore;
using ShopManagementAPI.Data;
using ShopManagementAPI.Services;
using ShopManagementAPI.Middleware;
using System.Text.Json.Serialization;
using System.Text.Json;
using ShopManagementAPI.Data.Seeding;
using ShopManagementAPI.Repositories;
using ShopManagementAPI.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;  
using StackExchange.Redis;
using Microsoft.OpenApi.Models;
using System.Reflection;
using ShopManagementAPI.Authorization;
using ShopManagementAPI.Configurations;


var builder = WebApplication.CreateBuilder(args);

//configurations
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.Configure<RedisSettings>(
    builder.Configuration.GetSection("Redis"));

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.Configure<OtpSettings>(
    builder.Configuration.GetSection("OtpSettings"));

// ===== Thêm DbContext để EF Core biết kết nối DB bên appsettings.Development.json=====
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDatabase")));

var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>()
    ?? throw new Exception("Jwt configuration is missing.");
//===== bộ não xử lý JWT → biến token thành HttpContext.User =====
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Key)
        ),

        RoleClaimType = ClaimTypes.Role,
        NameClaimType = "username"
    };
    options.MapInboundClaims = false;
    options.EventsType = typeof(JwtAuthEvents);
});

// ===== Thêm controller support =====
builder.Services.AddControllers().AddJsonOptions(options =>{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});

//===Cho phép service đọc HttpContext===
builder.Services.AddHttpContextAccessor();

//====Repository====
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<PermissionRepository>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddScoped<RolePermissionRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserRoleRepository>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<OrderItemRepository>();
builder.Services.AddScoped<EmailOtpRepository>();
builder.Services.AddScoped<OtpVerificationRepository>();
builder.Services.AddScoped<UnitOfWork>();

//====Service====
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ProductService>();

builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<UserDataScopeService>();
builder.Services.AddScoped<DashboardService>();

builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<JwtAuthEvents>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<OtpService>();

// redis connection
var redisSettings = builder.Configuration
    .GetSection("Redis")
    .Get<RedisSettings>()
    ?? throw new Exception("Redis configuration is missing.");
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var options = ConfigurationOptions.Parse(
        redisSettings.ConnectionString);

    options.AbortOnConnectFail = false;

    return ConnectionMultiplexer.Connect(options);
});

// Redis services :jwt blacklist --- permission cache
builder.Services.AddSingleton<JwtBlacklistService>();
builder.Services.AddScoped<PermissionCacheService>();

builder.Services.AddEndpointsApiExplorer();
// ===== Thêm Swagger/OpenAPI để test API =====
builder.Services.AddSwaggerGen(c =>
{   //JWT ACCESS TOKEN
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ShopManagementAPI",
        Version = "v1",
        Description = "Mini Ecommerce API với JWT + Refresh Token"
    });

    // JWT ACCESS TOKEN
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description =
        "JWT Access Token\n\n" +
        "Format: Bearer {your_access_token}\n\n" +
        "Refresh token được gửi tự động bằng HttpOnly Cookie."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    // XML COMMENT
    var xmlFile =$"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

    var xmlPath =Path.Combine(AppContext.BaseDirectory, xmlFile);

    c.IncludeXmlComments(xmlPath);

    // tránh duplicate schema
    c.CustomSchemaIds(x => x.FullName);
});

var app = builder.Build();
//==== DbSeeder run thông qua RunSeeder ở appsettings.Development.json====
var runSeeder = builder.Configuration.GetValue<bool>("RunSeeder");

if (runSeeder){
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(context);
}

// ===== WARM UP APP =====
// Khởi tạo sẵn SQL Server, EF Core và Redis khi app startup giúp giảm độ trễ request đầu tiên .
app.Lifetime.ApplicationStarted.Register(() =>
{
    Task.Run(async () =>
    {
        try
        {
            using var scope = app.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            await db.Database.CanConnectAsync();
            await db.Permissions.AnyAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARMUP] {ex.Message}");
        }
    });
});

// ===== Middleware =====
if (app.Environment.IsDevelopment()){
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // giữ token khi refresh browser
        c.ConfigObject.PersistAuthorization = true;

        c.DocumentTitle = "ShopManagementAPI Docs";
    });
}
app.UseStatusCodePages(async context =>{
    var response = context.HttpContext.Response;

    if (response.StatusCode == 404)
    {
        response.ContentType = "application/json";

        await response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
        {
            message = "API not found or not exist",
            path = context.HttpContext.Request.Path,
        }));
    }
});
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<JwtSecurityMiddleware>();

app.UseAuthorization();
app.UseMiddleware<PermissionMiddleware>();

app.MapControllers();
app.Run();
