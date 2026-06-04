# ShopManagementAPI

![.NET](https://img.shields.io/badge/.NET-8/9-purple)
![ASP.NET Core](https://img.shields.io/badge/WebAPI-ASP.NET%20Core-blue)
![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-red)
![Redis](https://img.shields.io/badge/Cache-Redis-orange)
![JWT](https://img.shields.io/badge/Auth-JWT-green)
![Swagger](https://img.shields.io/badge/API-Swagger-brightgreen)

---

# 1. Giới thiệu dự án

**ShopManagementAPI** là hệ thống backend quản lý bán hàng được xây dựng bằng ASP.NET Core Web API, mô phỏng một hệ thống thực tế trong doanh nghiệp.

Dự án tập trung vào các core backend quan trọng:

* Xác thực người dùng bằng **JWT + Refresh Token**
* Phân quyền theo **Role + Permission**
* Quản lý sản phẩm / danh mục / đơn hàng
* Cache dữ liệu bằng **Redis**
* Logging hệ thống bằng **Serilog**
* Kiến trúc backend theo hướng **Clean Architecture**

---

# 2. Mục tiêu học tập của dự án

Thông qua dự án này, tôi học được:

## Authentication & Security

* JWT Access Token & Refresh Token
* Logout thật sự bằng **Blacklist Token (Redis)**
* Hash mật khẩu bằng BCrypt
* Middleware xử lý xác thực

## Authorization nâng cao

* Role-based (ADMIN, STAFF, CUSTOMER)
* Permission-based ([RequirePermission])
* DataScope filter dữ liệu theo role

## Performance

* Redis cache permission
* Giảm truy vấn DB
* Tối ưu truy xuất user permission

## Backend Architecture

* Service – Repository pattern
* Middleware pipeline
* Exception handling global
* DTO separation

## Tư duy thực tế

* Validate business logic chặt chẽ
* Soft delete / restore
* FSM trạng thái đơn hàng
* Tách domain rõ ràng

---

# 3. Kiến trúc hệ thống

```text id="arch"
Client
  ↓
Controller
  ↓
Service (Business Logic)
  ↓
Repository
  ↓
Entity Framework Core
  ↓
SQL Server
```

---

# 4. Authentication Flow

```text id="auth"
Login
 → Validate user
 → Generate JWT Access Token
 → Generate Refresh Token
 → Store DB + Cookie

Request API
 → JWT Middleware
 → Check Redis blacklist
 → Validate user active
 → Inject CurrentUserService
```

---

# 5. Authorization Flow

```text id="auth2"
JWT Claims
   ↓
Roles
   ↓
Permissions
   ↓
RequirePermission Attribute
   ↓
Redis Permission Cache
   ↓
DataScope Service
```

---

# 6. Swagger API

- [http://localhost:5013/swagger/index.html](http://localhost:5013/swagger/index.html)

### Cách sử dụng:

1. Chạy project
2. Truy cập Swagger
3. Login API để lấy token
4. Click **Authorize**
5. Nhập:

```text id="token"
Bearer {access_token}
```

---

# 7. Cấu hình hệ thống (appsettings.json)

## Ví dụ cấu hình chuẩn

```json id="config"
{
  "ConnectionStrings": {
    "MyDatabase": "Server=YOUR_SERVER_NAME;Database=ShopManagementAPI;Trusted_Connection=True;TrustServerCertificate=True;"
  },

  "RunSeeder": false,

  "Jwt": {
    "Key": "your-secret-key-here",
    "Issuer": "YourIssuer",
    "Audience": "YourAudience",
    "AccessTokenExpirationMinutes": 10,
    "RefreshTokenExpirationDays": 1
  },

  "Redis": {
    "ConnectionString": "localhost:6379,connectTimeout=5000,syncTimeout=5000",
    "PermissionCacheExpirationHours": 1
  },

  "Serilog": {
    "Using": [ "Serilog.Sinks.MSSqlServer" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ],
    "WriteTo": [
      {
        "Name": "MSSqlServer",
        "Args": {
          "connectionString": "MyDatabase",
          "tableName": "Logs",
          "autoCreateSqlTable": true,
          "restrictedToMinimumLevel": "Information"
        }
      }
    ]
  }
}
```

---

## Ví dụ thực tế khi chạy

### 1. Database local:

```json
"Server=DESKTOP-ABC123\\SQLEXPRESS;Database=ShopManagementAPI;Trusted_Connection=True;"
```

---

### 2. JWT thực tế:

```json
"Key": "shop-api-2026-super-secret-key-123456"
```

---

### 3. Redis local:

```json
"ConnectionString": "localhost:6379"
```

---

# 8. Seeder dữ liệu

## ⚡ Bật seed lần đầu

```json
"RunSeeder": true
```

### Dữ liệu tạo ra:

* ADMIN role
* STAFF role
* CUSTOMER role
* Permission hệ thống
* Admin account

---

## Admin mặc định

```csharp id="admin"
Username = "admin",
Password = "123456",
FullName = "Super admin",
Email = "admin2005@gmail.com",
IsActive = true
```

---

## Sau khi chạy xong

```json
"RunSeeder": false
```

- Tránh duplicate dữ liệu

---

# 9. Database Design

## ER Diagram

- ./Images/Diagram.png

```text id="erd"
[INSERT ER DIAGRAM IMAGE HERE]
```

---

## Quan hệ chính

* User ↔ Role (N-N)
* Role ↔ Permission (N-N)
* Category → Product (1-N)
* User → Order (1-N)
* Order → OrderItem (1-N)

---

# 10. Redis Usage

## Permission Cache

```text id="redis1"
permissions:{userId}
```

## Blacklist Token

```text id="redis2"
blacklist:{jti}
```

---

# 11. Modules chính

## User

* CRUD user
* Gán role
* Lock / unlock user

## Auth

* Login / Register
* Refresh token
* Logout (revoke JWT)

## Product

* CRUD product
* Check duplicate category
* Update stock
* Soft delete

## Category

* CRUD category
* Không cho xóa nếu còn product active

## Order

```text id="order"
PENDING → CONFIRMED → SHIPPING → COMPLETED
```

---

# 12. Điểm nổi bật

* JWT + Refresh Token rotation
* Redis caching system
* Permission-based authorization
* Role-based data filtering
* Soft delete toàn hệ thống
* Middleware pipeline rõ ràng
* Clean architecture chuẩn backend

---

# 13. Hướng phát triển

* Dashboard thống kê doanh thu
* Audit log hệ thống
* Notification realtime (SignalR)
* Background job (Hangfire)
* Rate limiting API
* Docker + CI/CD deploy
* Microservices architecture

---

# 14. Kết luận

Dự án này giúp tôi hiểu sâu hơn về:

* Cách xây dựng backend production-ready
* Thiết kế hệ thống phân quyền thực tế
* Tối ưu hiệu năng với Redis
* Quản lý authentication/authorization nâng cao
* Tư duy clean architecture trong .NET


