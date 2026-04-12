using System;
using Microsoft.EntityFrameworkCore;
using Demo_Course_Management.Data;
using Demo_Course_Management.Services;
using Demo_Course_Management.Middleware;
using System.Text.Json.Serialization;
using System.Text.Json;
using Demo_Course_Management.Data.Seeding; // namespace DbContext của bạn

var builder = WebApplication.CreateBuilder(args);

// ===== 1️⃣ Thêm DbContext để EF Core biết kết nối DB bên appsettings.Development.json=====
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDatabase")));

// ===== 2️⃣ Thêm controller support =====
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});
//====Service====
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<RoleService>();


// ===== 3️⃣ Thêm Swagger/OpenAPI để test API =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//==== DbSeeder run thông qua RunSeeder ở appsettings.Development.json====
var runSeeder = builder.Configuration.GetValue<bool>("RunSeeder");

if (runSeeder)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(context);
}

// ===== 4️⃣ Middleware =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStatusCodePages(async context =>
{
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

app.UseAuthorization(); // nếu muốn dùng [Authorize], bỏ nếu chưa cần

app.MapControllers(); // map tất cả controller

app.Run();
