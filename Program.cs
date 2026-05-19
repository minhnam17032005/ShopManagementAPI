using System;
using Microsoft.EntityFrameworkCore;
using Demo_Course_Management.Data;
using Demo_Course_Management.Services;
using Demo_Course_Management.Middleware;
using System.Text.Json.Serialization;
using System.Text.Json;
using Demo_Course_Management.Data.Seeding;
using Demo_Course_Management.Repositories;
using Demo_Course_Management.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ===== Thêm DbContext để EF Core biết kết nối DB bên appsettings.Development.json=====
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDatabase")));

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

        ValidIssuer =builder.Configuration["Jwt:Issuer"],
        ValidAudience =builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        builder.Configuration["Jwt:Key"]!
                    )),
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
//====Service====
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<PermissionService>();

builder.Services.AddScoped<CurrentUserService>();

builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<JwtAuthEvents>();

//=== dùng reddis để lưu blacklist accesstoken
builder.Services.AddSingleton<JwtBlacklistService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(
        builder.Configuration["Redis:ConnectionString"]!
    )
);

// ===== Thêm Swagger/OpenAPI để test API =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập token dạng: Bearer {your_token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();
//==== DbSeeder run thông qua RunSeeder ở appsettings.Development.json====
var runSeeder = builder.Configuration.GetValue<bool>("RunSeeder");

if (runSeeder){
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(context);
}

// ===== Middleware =====
if (app.Environment.IsDevelopment()){
    app.UseSwagger();
    app.UseSwaggerUI();
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

app.UseAuthorization();
// permission check nên sau authorization
//app.UseMiddleware<PermissionMiddleware>();
app.MapControllers(); // map tất cả controller
app.Run();
