# 🔐 Hướng Dẫn Cấu Hình JWT & SSO Toàn Diện

## 📋 Tổng Quan

Dự án này sử dụng **2 phương thức xác thực song song**:

1. **JWT (JSON Web Tokens)** - Cho API authentication
2. **SSO (Single Sign-On)** với OpenID Connect - Cho web authentication

---

## 🎯 Kiến Trúc Tổng Quan

```
┌─────────────────────────────────────────────────────────────────────┐
│      AUTHENTICATION ARCHITECTURE           │
└─────────────────────────────────────────────────────────────────────┘

┌──────────────┐      ┌──────────────────────────┐
│   Browser    │     │   External Client/App    │
│  (MVC Views) │           │   (Mobile, SPA, etc.)    │
└──────┬───────┘              └──────────┬───────────────┘
       │           │
       │ Cookie-based             │ Token-based
     │ SSO Auth        │ JWT Auth
       ▼         ▼
┌─────────────────────────────────────────────────────────────────────┐
│        OWIN STARTUP (Startup.cs)      │
│  ┌───────────────────────┐         ┌──────────────────────────┐     │
│  │  SSO Middleware       │      │  JWT Middleware     │     │
│  │  ─────────────        │       │  ──────────────          │     │
│  │  ✓ Cookie Auth        │         │  ✓ Bearer Token Auth  │     │
│  │  ✓ OpenID Connect     │    │  ✓ Token Validation    │     │
│  │  ✓ Auth0 Integration  │   │  ✓ Claim-based Identity  │     │
│  └───────────────────────┘         └──────────────────────────┘     │
└─────────────────────────────────────────────────────────────────────┘
          │
        ▼
┌─────────────────────────────────────────────────────────────────────┐
│             CONTROLLERS   │
│  ┌─────────────────────┐     ┌──────────────────────────┐     │
│  │  MVC Controllers    │           │  API Controllers       │     │
│  │  ─────────────────  │           │  ───────────────────     │     │
│  │[Authorize]        │   │  [Authorize]             ││
│  │  → Cookie check     │           │  → JWT Bearer check  │     │
│  │  → Redirect to login│           │  → Return 401    │     │
│  │         │ │   │     │
│  │  TasksController    │      │  TasksApiController      │     │
│  │  │      │  AuthController          │     │
│  └─────────────────────┘└──────────────────────────┘     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 🔑 JWT Authentication

### 1. JWT là gì?

JWT (JSON Web Token) là một chuẩn mở (RFC 7519) để truyền thông tin an toàn giữa các bên dưới dạng JSON object.

**Cấu trúc JWT**:
```
xxxxx.yyyyy.zzzzz
  │     │     │
  │     │     └─── Signature (chữ ký)
  │     └───────── Payload (dữ liệu)
  └─────────────── Header (metadata)
```

### 2. Luồng Hoạt Động JWT

```mermaid
sequenceDiagram
    participant Client as Client App
participant API as API Server
    participant Auth as AuthController
    participant UoW as UnitOfWork
    participant DB as Database

    Note over Client,DB: 🔐 REGISTRATION FLOW
    Client->>Auth: POST /api/auth/register
    Note right of Client: Body: {<br/>  email: "user@example.com",<br/>  password: "Pass123",<br/>  confirmPassword: "Pass123"<br/>}
    
    Auth->>Auth: Validate ModelState
    Auth->>UoW: Users.GetByEmail(email)
    UoW->>DB: SELECT * FROM Users WHERE Email = @Email
    DB-->>UoW: null (email chưa tồn tại)
    UoW-->>Auth: null
    
    Auth->>Auth: Hash password với BCrypt
    Note right of Auth: passwordHash = BCrypt.HashPassword(password)
    
    Auth->>UoW: Users.Create(userModel)
    UoW->>DB: INSERT INTO Users (Email, PasswordHash, CreatedAt)
  DB-->>UoW: New User ID
    UoW-->>Auth: UserModel with ID
  
    Auth->>UoW: SaveChanges()
    Note right of UoW: Transaction.Commit()
    
    Auth->>Auth: CreateJwtToken(user)
    Note right of Auth: Generate JWT with:<br/>- Claims (userId, email)<br/>- Secret Key<br/>- Expiration (1 hour)
    
    Auth-->>Client: HTTP 201 Created
    Note left of Auth: {<br/>  "id": 1,<br/>  "email": "user@example.com",<br/>  "access_token": "eyJhbG...",<br/>  "token_type": "bearer",<br/>  "expires_in": 3600<br/>}
 
    Note over Client,DB: 🔓 LOGIN FLOW
    Client->>Auth: POST /api/auth/login
    Note right of Client: Body: {<br/>  email: "user@example.com",<br/>  password: "Pass123"<br/>}
    
    Auth->>Auth: Validate ModelState
    Auth->>UoW: Users.GetByEmail(email)
 UoW->>DB: SELECT * FROM Users WHERE Email = @Email
    DB-->>UoW: UserModel
    UoW-->>Auth: UserModel
    
    alt User not found
 Auth-->>Client: HTTP 401 Unauthorized
  else User exists
        Auth->>Auth: VerifyPassword(password, user.PasswordHash)
        Note right of Auth: BCrypt.Verify(password, hash)
        
    alt Password incorrect
        Auth-->>Client: HTTP 401 Unauthorized
        else Password correct
  Auth->>Auth: CreateJwtToken(user)
  Auth-->>Client: HTTP 200 OK + JWT Token
        end
    end
    
    Note over Client,DB: 📝 PROTECTED API ACCESS
    Client->>API: GET /api/TasksApi
    Note right of Client: Headers:<br/>  Authorization: Bearer eyJhbG...
    
    API->>API: JWT Middleware validates token
    Note right of API: 1. Decode token<br/>2. Verify signature<br/>3. Check expiration<br/>4. Extract claims
    
    alt Token invalid/expired
        API-->>Client: HTTP 401 Unauthorized
    else Token valid
        API->>API: Set User.Identity from claims
        API->>API: Execute [Authorize] attribute
 API->>UoW: Tasks.GetTasks()
        UoW->>DB: SELECT * FROM Tasks
  DB-->>UoW: List<TaskModel>
UoW-->>API: List<TaskModel>
        API-->>Client: HTTP 200 OK + Tasks data
    end
```

### 3. Cấu Hình JWT (Step by Step)

#### Bước 1: Cài đặt NuGet Packages

```powershell
# Package Manager Console
Install-Package System.IdentityModel.Tokens.Jwt -Version 8.15.0
Install-Package Microsoft.Owin.Security.Jwt -Version 4.2.3
Install-Package BCrypt.Net-Next -Version 4.0.3
```

#### Bước 2: Tạo JwtConfig.cs

```csharp
// TodoListMVC/App_Start/JwtConfig.cs
using System;

namespace TodoListMVC.App_Start
{
    public class JwtConfig
    {
        // Issuer: Người phát hành token (application name)
     public static string Issuer = "TodoListMVC";

        // Audience: Đối tượng sử dụng token (service name)
        public static string Audience = "todolist";

        // Secret: Khóa bí mật để ký token (PHẢI đổi trong production!)
      // Tối thiểu 32 ký tự, nên dùng random string
        public static string Secret = "slkajdflkjl12kj3l13908a0s9cdaolidkaldje212_l23n1l";

        // TokenLifetime: Thời gian sống của token
        public static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);
    }
}
```

**⚠️ BẢO MẬT**: Trong production:
- Đổi `Secret` thành random string mạnh
- Lưu trong Environment Variables hoặc Azure Key Vault
- **KHÔNG COMMIT** secret vào Git

#### Bước 3: Tạo UserModel và UserRepository

```csharp
// TodoListMVC/Models/UserModel.cs
using System;

namespace TodoListMVC.Models
{
    public class UserModel
    {
        public int Id { get; set; }
   public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
```

```csharp
// TodoListMVC/Repositories/IUserRepository.cs
using TodoListMVC.Models;

namespace TodoListMVC.Repositories
{
    public interface IUserRepository
    {
  UserModel GetByEmail(string email);
 UserModel Create(UserModel user);
    }
}
```

```csharp
// TodoListMVC/Repositories/SqlUserRepository.cs
using System;
using System.Data.SqlClient;
using TodoListMVC.Models;

namespace TodoListMVC.Repositories
{
    public class SqlUserRepository : IUserRepository
{
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;

      public SqlUserRepository(SqlConnection connection, SqlTransaction transaction)
  {
      _connection = connection;
            _transaction = transaction;
        }

        public UserModel GetByEmail(string email)
{
            var query = "SELECT Id, Email, PasswordHash, CreatedAt FROM Users WHERE Email = @Email";
  
     using (var cmd = new SqlCommand(query, _connection, _transaction))
      {
 cmd.Parameters.AddWithValue("@Email", email);
       
  using (var reader = cmd.ExecuteReader())
     {
        if (reader.Read())
            {
          return new UserModel
    {
        Id = Convert.ToInt32(reader["Id"]),
       Email = reader["Email"].ToString(),
   PasswordHash = reader["PasswordHash"].ToString(),
          CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
    };
     }
       }
      }
          
            return null;
  }

        public UserModel Create(UserModel user)
        {
   var query = @"
   INSERT INTO Users (Email, PasswordHash, CreatedAt)
  VALUES (@Email, @PasswordHash, @CreatedAt);
          SELECT SCOPE_IDENTITY();";
   
        using (var cmd = new SqlCommand(query, _connection, _transaction))
            {
    cmd.Parameters.AddWithValue("@Email", user.Email);
      cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
    
        user.Id = Convert.ToInt32(cmd.ExecuteScalar());
       }
            
      return user;
        }
    }
}
```

#### Bước 4: Cập nhật IUnitOfWork

```csharp
// TodoListMVC/Repositories/IUnitOfWork.cs
using System;

namespace TodoListMVC.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ITaskRepository Tasks { get; }
        IUserRepository Users { get; } // ← Thêm dòng này
        int SaveChanges();
    }
}
```

```csharp
// TodoListMVC/Repositories/UnitOfWork.cs
public class UnitOfWork : IUnitOfWork
{
    // ...existing code...
    
 public IUserRepository Users { get; private set; }

    public UnitOfWork()
    {
        // ...existing connection setup...
        
      Tasks = new SqlTaskRepository(_connection, _transaction);
      Users = new SqlUserRepository(_connection, _transaction); // ← Thêm
    }
    
    // ...rest of code...
}
```

#### Bước 5: Tạo DTOs cho Authentication

```csharp
// TodoListMVC/DTOs/LoginDto.cs
using System.ComponentModel.DataAnnotations;

namespace TodoListMVC.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password là bắt buộc")]
        [MinLength(6, ErrorMessage = "Password phải có ít nhất 6 ký tự")]
        public string Password { get; set; }
    }
}
```

```csharp
// TodoListMVC/DTOs/RegisterDto.cs
using System.ComponentModel.DataAnnotations;

namespace TodoListMVC.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password là bắt buộc")]
        [MinLength(6, ErrorMessage = "Password phải có ít nhất 6 ký tự")]
   public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }
    }
}
```

#### Bước 6: Tạo AuthController

```csharp
// TodoListMVC/Controllers/AuthController.cs
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web.Http;
using TodoListMVC.App_Start;
using TodoListMVC.DTOs;
using TodoListMVC.Models;
using TodoListMVC.Repositories;

namespace TodoListMVC.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly IUserRepository _userRepository;
     private readonly IUnitOfWork _unitOfWork;
      private readonly IMapper _mapper;

        public AuthController(IUnitOfWork unitOfWork, IMapper mapper)
        {
      _unitOfWork = unitOfWork;
  _mapper = mapper;
            _userRepository = _unitOfWork.Users;
      }

/// <summary>
        /// Đăng ký user mới
   /// </summary>
        [AllowAnonymous]
      [HttpPost]
        [Route("register")]
        public IHttpActionResult Register(RegisterDto model)
  {
 if (!ModelState.IsValid)
 return BadRequest(ModelState);

       // Kiểm tra email đã tồn tại
            var existing = _userRepository.GetByEmail(model.Email);
        if (existing != null)
     {
 return Content(System.Net.HttpStatusCode.Conflict, 
              new { message = "Email đã tồn tại" });
            }

         // Hash password với BCrypt
   var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

         var user = new UserModel
       {
     Email = model.Email,
     PasswordHash = passwordHash,
       CreatedAt = DateTime.UtcNow
            };

     // Tạo user trong database
        var created = _userRepository.Create(user);
      _unitOfWork.SaveChanges();

            // Tạo JWT token ngay sau khi đăng ký
            var token = CreateJwtToken(created);
         
         return Content(System.Net.HttpStatusCode.Created, new
            {
         id = created.Id,
      email = created.Email,
     access_token = token,
 token_type = "bearer",
      expires_in = (int)JwtConfig.TokenLifetime.TotalSeconds
     });
        }

        /// <summary>
        /// Đăng nhập
      /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
   public IHttpActionResult Login(LoginDto model)
     {
    if (!ModelState.IsValid)
          return BadRequest(ModelState);

        var user = _userRepository.GetByEmail(model.Email);
  if (user == null)
         return Unauthorized();

            // Verify password
  if (!VerifyPassword(model.Password, user.PasswordHash))
    return Unauthorized();

        var token = CreateJwtToken(user);

          return Ok(new
         {
    id = user.Id,
          email = user.Email,
       access_token = token,
     token_type = "bearer",
         expires_in = (int)JwtConfig.TokenLifetime.TotalSeconds
       });
  }

      /// <summary>
  /// Verify password với BCrypt
     /// </summary>
     private bool VerifyPassword(string password, string passwordHash)
        {
     return BCrypt.Net.BCrypt.Verify(password, passwordHash);
     }

 /// <summary>
   /// Tạo JWT Token
        /// </summary>
        private string CreateJwtToken(UserModel user)
    {
            var key = Encoding.UTF8.GetBytes(JwtConfig.Secret);

            // Claims: Thông tin user được nhúng vào token
            var claims = new[]
 {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
       new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
    };

            // Signing credentials: Chữ ký của token
          var creds = new SigningCredentials(
           new SymmetricSecurityKey(key),
    SecurityAlgorithms.HmacSha256);

            // Tạo token
       var token = new JwtSecurityToken(
       issuer: JwtConfig.Issuer,
  audience: JwtConfig.Audience,
 claims: claims,
       expires: DateTime.UtcNow.Add(JwtConfig.TokenLifetime),
          signingCredentials: creds
            );

    return new JwtSecurityTokenHandler().WriteToken(token);
        }
 }
}
```

#### Bước 7: Cấu hình JWT trong Startup.cs

```csharp
// TodoListMVC/Startup.cs
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Microsoft.IdentityModel.Tokens;
using Owin;
using System.Text;
using System.Web.Http;
using TodoListMVC.App_Start;

[assembly: OwinStartup(typeof(TodoListMVC.Startup))]

namespace TodoListMVC
{
    public class Startup
  {
        public void Configuration(IAppBuilder app)
      {
  // Cấu hình JWT Bearer Authentication
            ConfigureJWT(app);

       // Cấu hình Web API
     app.Map("/api", api =>
          {
    var config = new HttpConfiguration();
         WebApiConfig.Register(config);
    api.UseWebApi(config);
         });
        }

        private void ConfigureJWT(IAppBuilder app)
    {
            var key = Encoding.UTF8.GetBytes(JwtConfig.Secret);

   var tokenValidationParameters = new TokenValidationParameters
       {
       // Validate Issuer (người phát hành)
     ValidateIssuer = true,
     ValidIssuer = JwtConfig.Issuer,

     // Validate Audience (đối tượng sử dụng)
    ValidateAudience = true,
     ValidAudience = JwtConfig.Audience,

                // Validate chữ ký
     ValidateIssuerSigningKey = true,
           IssuerSigningKey = new SymmetricSecurityKey(key),

             // Validate thời gian hết hạn
        ValidateLifetime = true,
  ClockSkew = TimeSpan.FromMinutes(2) // Cho phép sai lệch 2 phút
            };

       // Sử dụng JWT Bearer Authentication middleware
 app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
   {
  AuthenticationMode = AuthenticationMode.Active,
       TokenValidationParameters = tokenValidationParameters
         });
        }
 }
}
```

#### Bước 8: Tạo Database Table

```sql
-- Run trong SQL Server Management Studio
USE TodoListDB;
GO

CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Thêm index cho Email (để tìm kiếm nhanh)
CREATE INDEX IX_Users_Email ON Users(Email);
GO
```

#### Bước 9: Test JWT API

**Test với Postman/cURL:**

```bash
# 1. Register
curl -X POST https://localhost:44348/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Pass123456",
    "confirmPassword": "Pass123456"
  }'

# Response:
# {
#   "id": 1,
#   "email": "test@example.com",
#   "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
#   "token_type": "bearer",
#   "expires_in": 3600
# }

# 2. Login
curl -X POST https://localhost:44348/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Pass123456"
  }'

# 3. Access Protected API
curl -X GET https://localhost:44348/api/TasksApi \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## 🌐 SSO (Single Sign-On) với OpenID Connect

###1. SSO là gì?

SSO cho phép người dùng đăng nhập một lần và truy cập nhiều ứng dụng mà không cần đăng nhập lại.

**Ưu điểm**:
- ✅ Trải nghiệm người dùng tốt hơn
- ✅ Bảo mật tập trung
- ✅ Quản lý user dễ dàng
- ✅ Hỗ trợ MFA (Multi-Factor Authentication)

---

###2. Luồng Hoạt Động SSO (sau khi đã cấu hình thành công)

```mermaid
sequenceDiagram
 participant User as Browser
 participant App as TodoListMVC
 participant Auth0 as Auth0
 participant Cookie as Cookie Store

 User->>App: GET /Tasks (protected)
 App->>App: OWIN Cookie check
 alt No cookie
 App-->>User:302 -> /Account/Login (OWIN Challenge)
 User->>Auth0: Redirect to Auth0 /authorize
 Auth0->>User: Show login UI
 User->>Auth0: Submit credentials
 Auth0-->>User:302 -> returnTo app with code/id_token
 User->>App: GET /?code=...&id_token=...
 App->>App: OIDC middleware validates id_token, creates claims principal
 App->>Cookie: Issue cookie `TodoListMVC.Auth`
 App-->>User:302 -> original URL (/Tasks)
 User->>App: GET /Tasks with cookie
 App->>App: Serve page
 end

 Note over User,Auth0: Logout flow
 User->>App: GET /Account/Logout
 App->>App: Clear local cookie
 App->>Auth0: Redirect to https://<your-domain>/v2/logout?client_id=...&returnTo=https://localhost:44348/
 Auth0->>User: Clears SSO session and redirects back to app
 User->>App: GET / (not authenticated)
```

---

###3. Những thay đổi chính đã áp dụng trong dự án (SSO hoàn thiện)

- `Startup.cs`:
 - `app.UseCookieAuthentication(...)` đã cấu hình `CookieName = "TodoListMVC.Auth"`, `ExpireTimeSpan`, `SlidingExpiration`, `CookieHttpOnly`.
 - `app.UseOpenIdConnectAuthentication(...)` đã đặt `RedirectUri` và `PostLogoutRedirectUri` phù hợp.
 - Trong `RedirectToIdentityProvider` notification, logout request được build thành URL `https://{authority}v2/logout?client_id={clientId}&returnTo={postLogoutUri}` để tương thích với Auth0 v2 logout.
 - `AuthenticationFailed` redirect về `/Error/ShowError` với thông báo rõ ràng.

- `AccountController`:
 - `Login(string returnUrl)` → gọi `Authentication.Challenge(...)` để kích hoạt OIDC login flow.
 - `Logout()` → xóa cookie local và gọi SignOut OIDC; middleware redirect đến Auth0 logout endpoint.
 - `PostLogout()` → action để nhận redirect sau khi Auth0 logout và chuyển về Home.
 - `Profile()` → hiển thị claims lấy từ cookie (SSO).

- Views:
 - `Views/Account/Profile.cshtml` hiển thị claims.
 - `Views/Account/AccessDenied.cshtml` và `Views/Error/ShowError.cshtml` để hiển thị lỗi rõ ràng.
 - Layout đã được cập nhật để hiển thị trạng thái đăng nhập và menu login/logout.

- `Web.config`:
 - `oidc:ClientId`, `oidc:Authority` (phải có dấu `/` ở cuối), `oidc:RedirectUri` được cấu hình và khớp với các values trong Auth0 Allowed Callback / Logout URLs.

---

###4. Cấu hình Auth0 (tóm tắt - cần có để logout hoạt động)

- Allowed Callback URLs:
 - `https://localhost:44348/`
 - `https://localhost:44348/signin-oidc` (nếu sử dụng)

- Allowed Logout URLs:
 - `https://localhost:44348/`
 - `https://localhost:44348/Account/PostLogout`

- Authority phải dạng: `https://<your-tenant>.us.auth0.com/` (có `/` cuối)

---

###5. Kiểm tra sau khi cấu hình thành công

1. Chạy ứng dụng, truy cập `https://localhost:44348/` → click "Go to Tasks" → sẽ redirect tới Auth0.
2. Đăng nhập trên Auth0 → quay lại ứng dụng, confirm cookie `TodoListMVC.Auth` được tạo.
3. Truy cập `Profile` để xem claims.
4. Click `Logout` → ứng dụng sẽ:
 - Xóa cookie `TodoListMVC.Auth` cục bộ
 - Redirect tới `https://<your-tenant>.us.auth0.com/v2/logout?client_id={clientId}&returnTo={https://localhost:44348/}`
 - Auth0 xóa session SSO và redirect về `https://localhost:44348/` (hoặc `PostLogout` URL)
5. Sau logout, truy cập `/Tasks` sẽ redirect về login.

---

###6. Lưu ý & Troubleshooting cụ thể (logout)

- Nếu xuất hiện thông báo `Oops!, something went wrong` trong Auth0 khi logout:
 - Kiểm tra **Allowed Logout URLs** trong Auth0 và đảm bảo `returnTo` URL có trong danh sách.
 - Kiểm tra `client_id` gửi đến endpoint logout là Client ID của Application.
 - Kiểm tra `authority` value trong `Web.config` có đúng domain và có dấu `/` ở cuối.

- Nếu cookie không bị xóa sau logout:
 - Clear cookie thủ công từ browser DevTools để debug.
 - Kiểm tra domain và path cookie. Trong môi trường development, `CookieSecureOption.SameAsRequest` cho phép HTTP nhưng production nên dùng `Always`.

---

###7. Tài liệu tham khảo nhanh

- Auth0 logout: https://auth0.com/docs/authenticate/login/logout
- Auth0 OIDC Quickstart (ASP.NET OWIN): https://auth0.com/docs/quickstart/webapp/aspnet-owin

---

## 🔄 So Sánh JWT vs SSO

| Tiêu Chí | JWT (API Auth) | SSO (Web Auth) |
|----------|----------------|----------------|
| **Use Case** | Mobile apps, SPAs, API clients | Web browsers, MVC applications |
| **Storage** | Client lưu token (localStorage, memory) | Server-side session cookie |
| **State** | Stateless (self-contained) | Stateful (session on server) |
| **Expiration** | Token expires, client phải refresh | Cookie expires, auto redirect to SSO |
| **User Info** | Trong token claims | Trong cookie + claims |
| **Logout** | Client xóa token | Server invalidates session |
| **Cross-Domain** | Dễ dàng (gửi token qua header) | Khó hơn (cookie restrictions) |
| **Security** | Token có thể bị lộ nếu không cẩn thận | Cookie có HttpOnly, Secure flags |

---

## 🛡️ Best Practices Bảo Mật

### 1. JWT Security

```csharp
// ✅ DO:
// - Use strong secret (min 32 chars, random)
public static string Secret = "your-very-long-random-secret-key-here-min-32-chars";

// - Set appropriate expiration
public static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);

// - Validate all parameters
var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateIssuerSigningKey = true,
    ValidateLifetime = true,
    ClockSkew = TimeSpan.FromMinutes(2)
};

// ❌ DON'T:
// - Store sensitive data in JWT (it's base64, not encrypted)
// - Use weak secrets
// - Set expiration too long
// - Skip validation
```

### 2. Password Hashing

```csharp
// ✅ DO: Use BCrypt with salt
var hash = BCrypt.Net.BCrypt.HashPassword(password);
var isValid = BCrypt.Net.BCrypt.Verify(password, hash);

// ❌ DON'T: Use plain MD5, SHA1, or store plain text
var hash = MD5.Hash(password); // ❌ INSECURE!
```

### 3. HTTPS

```csharp
// ✅ Production: Always use HTTPS
RequireHttpsMetadata = true // In OpenID Connect config

// ❌ Development only:
RequireHttpsMetadata = false
```

### 4. Secrets Management

```csharp
// ✅ DO: Use environment variables or Azure Key Vault
var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
var clientSecret = Configuration["Auth0:ClientSecret"];

// ❌ DON'T: Hardcode secrets in code
public static string Secret = "hardcoded-secret"; // ❌ DON'T!
```

---

## 🧪 Testing

### Test JWT Flow

```javascript
// test-jwt.js
const API_BASE = 'https://localhost:44348/api';

// 1. Register
async function register() {
    const response = await fetch(`${API_BASE}/auth/register`, {
   method: 'POST',
      headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
       email: 'test@example.com',
       password: 'Test123456',
   confirmPassword: 'Test123456'
        })
    });
    
  const data = await response.json();
    console.log('Register response:', data);
    return data.access_token;
}

// 2. Login
async function login() {
    const response = await fetch(`${API_BASE}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
       email: 'test@example.com',
       password: 'Test123456'
        })
    });
    
    const data = await response.json();
    console.log('Login response:', data);
    return data.access_token;
}

// 3. Get Tasks (Protected)
async function getTasks(token) {
    const response = await fetch(`${API_BASE}/TasksApi`, {
        headers: { 'Authorization': `Bearer ${token}` }
    });
    
    const data = await response.json();
    console.log('Tasks:', data);
    return data;
}

// Run tests
(async () => {
    try {
  const token = await register();
    // Or: const token = await login();
 
        await getTasks(token);
    } catch (error) {
console.error('Error:', error);
    }
})();
```

### Test SSO Flow

1. Navigate to `https://localhost:44348/Tasks`
2. If not logged in → Redirect to Auth0
3. Login with Auth0 credentials
4. Redirect back to app
5. Access protected pages without re-login
6. Check cookie in browser DevTools
7. Test logout

---

## 🚀 Deployment Checklist

### Trước khi deploy Production:

- [ ] Đổi JWT Secret thành random string mạnh
- [ ] Lưu secrets trong Azure Key Vault / Environment Variables
- [ ] Enable `RequireHttpsMetadata = true`
- [ ] Cấu hình CORS cho specific origins (không dùng `*`)
- [ ] Set cookie `Secure = true`, `HttpOnly = true`
- [ ] Giảm `ClockSkew` xuống `TimeSpan.Zero`
- [ ] Thêm rate limiting cho login endpoints
- [ ] Enable logging và monitoring
- [ ] Test với production Auth0 tenant
- [ ] Cập nhật Allowed Callback URLs trong Auth0

---

## 📖 Tài Liệu Tham Khảo

- [JWT.io](https://jwt.io/) - JWT Introduction & Debugger
- [Auth0 Docs](https://auth0.com/docs) - SSO Configuration
- [OpenID Connect Spec](https://openid.net/connect/) - OIDC Protocol
- [OWIN Documentation](http://www.asp.net/aspnet/overview/owin-and-katana) - OWIN Middleware
- [BCrypt.Net](https://github.com/BcryptNet/bcrypt.net) - Password Hashing

---

## ❓ FAQ

### Q: Tại sao cần cả JWT và SSO?

**A**: 
- **JWT**: Cho external clients (mobile apps, SPAs) - stateless, dễ scale
- **SSO**: Cho web browsers (MVC views) - tốt hơn cho user experience, bảo mật hơn với cookies

### Q: JWT token bị lộ thì sao?

**A**:
- Token hết hạn sau 1 giờ
- Implement token refresh mechanism
- Store token securely (không lưu trong localStorage nếu lo ngại XSS)
- Use short expiration times

### Q: SSO session hết hạn thì sao?

**A**: 
- Cookie middleware tự động redirect về Auth0 login
- User đăng nhập lại
- Hoặc Auth0 có thể auto-renew nếu session còn valid

### Q: Làm sao để revoke JWT token?

**A**:
- JWT là stateless, không thể revoke trực tiếp
- Solutions:
  1. Maintain token blacklist (in Redis)
  2. Use short expiration + refresh tokens
  3. Add version field in claims, increment on logout

---

## 👨‍💻 Author

**Phat Do**
- GitHub: [@PhatDo04](https://github.com/PhatDo04)

---

**📅 Last Updated**: 2024

**📄 License**: Educational Purpose

---

**⚠️ LƯU Ý**: Đây là hướng dẫn chi tiết cho mục đích học tập. Khi deploy production, cần thêm nhiều biện pháp bảo mật và tối ưu hóa!
