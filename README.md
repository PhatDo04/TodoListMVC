# TodoListMVC - ASP.NET MVC + Web API Application

## 📋 Tổng quan dự án

Ứng dụng quản lý công việc (Todo List) full-stack được xây dựng bằng **ASP.NET MVC** (server-side rendering) và **ASP.NET Web API** (RESTful API), áp dụng các design patterns hiện đại: **Repository Pattern**, **Unit of Work**, **Dependency Injection**, và **DTO Pattern**.

### 🎯 Mục tiêu
- Học cách xây dựng ứng dụng web với kiến trúc phân tầng rõ ràng
- Hiểu và áp dụng các Design Patterns trong thực tế
- Quản lý database transactions với Unit of Work
- Xây dựng RESTful API với CORS support
- Sử dụng AutoMapper cho object mapping
- Dependency Injection với Unity Container

---

## 🏗️ Kiến trúc tổng quan

```
┌─────────────────────────────────────────────────────────────────────┐
│        CLIENT LAYER                                                 │
│  ┌──────────────────┐               ┌──────────────────┐            │
│  │  Browser (MVC)   │               │  JavaScript App  │            │
│  │  Razor Views     │               │  (External)      │            │
│  └────────┬─────────┘               └────────┬─────────┘            │
└───────────┼──────────────────────────────────┼──────────────────────┘
            │                                  │
            │ HTTP                             │ HTTP (CORS)
            ▼                                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│         PRESENTATION LAYER                                          │
│  ┌──────────────────────────────────────────────────────────┐       │
│  │        ASP.NET MVC + Web API Application                 │       │
│  │  ┌────────────────┐          ┌──────────────────────┐    │       │
│  │  │ Global.asax.cs │────────> │  Configuration Layer │    │       │
│  │  │ Application    │          │  - WebApiConfig      │    │       │
│  │  │ Entry Point    │          │  - UnityConfig       │    │       │
│  │  └────────────────┘          │  - MappingProfile    │    │       │
│  │                              └──────────────────────┘    │       │
│  │                                                          │       │
│  │  ┌──────────────────────────────────────────────────┐    │       │
│  │  │         Middleware/Handlers                      │    │       │
│  │  │  - CorsHandler (OPTIONS preflight)               │    │       │
│  │  └──────────────────────────────────────────────────┘    │       │
│  │                                                          │       │
│  │  ┌──────────────────────────────────────────────────┐    │       │
│  │  │            Controllers Layer                     │    │       │
│  │  │  ┌──────────────────┐  ┌───────────────────┐     │    │       │
│  │  │  │ TasksController  │  │ TasksApiController│     │    │       │
│  │  │  │   (MVC)          │  │   (Web API)       │     │    │       │
│  │  │  │ - Index()        │  │ - GetTasks()      │     │    │       │
│  │  │  │ - Create()       │  │ - GetTask(id)     │     │    │       │
│  │  │  │ - Edit()         │  │ - PostTask()      │     │    │       │
│  │  │  │ - Delete()       │  │ - PutTask()       │     │    │       │
│  │  │  │                  │  │ - DeleteTask()    │     │    │       │
│  │  │  │ Direct ADO.NET   │  │ Uses UoW + DI     │     │    │       │
│  │  │  └──────────────────┘  └───────────────────┘     │    │       │
│  │  └──────────────────────────────────────────────────┘    │       │
│  └──────────────────────────────────────────────────────────┘       │
└─────────────────────────────────────────────────────────────────────┘
                  │                        │
                  ▼                        ▼
┌─────────────────────────────────────────────────────────────────────┐
│              DTO LAYER (Data Transfer)                              │
│  ┌──────────────┐  ┌────────────────┐  ┌────────────────┐           │
│  │   TaskDto    │  │ TaskCreateDto  │  │ TaskUpdateDto  │           │
│  │              │  │                │  │                │           │
│  │ + Id         │  │ + Title        │  │ + Title        │           │
│  │ + Title      │  │ [Required]     │  │ + IsCompleted  │           │
│  │ + IsCompleted│  │ [MinLength(3)] │  │ [Required]     │           │
│  │ + CreatedAt  │  │                │  │                │           │
│  │ + UpdatedAt  │  │                │  │                │           │
│  └──────────────┘  └────────────────┘  └────────────────┘           │
└─────────────────────────────────────────────────────────────────────┘
                            │
                            ▼ AutoMapper
┌─────────────────────────────────────────────────────────────────────┐
│          BUSINESS LOGIC LAYER                                       │
│                                                                     │
│  ┌────────────────────────────────────────────────────────────┐     │
│  │      Unit of Work Pattern                                  │     │
│  │  ┌──────────────────────────────────────────────────────┐  │     │
│  │  │              UnitOfWork : IUnitOfWork                │  │     │
│  │  │  ┌──────────────────────────────────────────────┐    │  │     │
│  │  │  │  - SqlConnection _connection                 │    │  │     │
│  │  │  │  - SqlTransaction _transaction               │    │  │     │
│  │  │  │  - ITaskRepository Tasks { get; }            │    │  │     │
│  │  │  │  + SaveChanges() → Commit/Rollback           │    │  │     │
│  │  │  │  + Dispose() → Clean up resources            │    │  │     │
│  │  │  └──────────────────────────────────────────────┘    │  │     │
│  │  └──────────────────────────────────────────────────────┘  │     │
│  └────────────────────────────────────────────────────────────┘     │
│                            │                                        │
│                            │ Creates & Manages                      │
│                            ▼                                        │
│  ┌────────────────────────────────────────────────────────────┐     │
│  │              Repository Pattern                            │     │
│  │  ┌──────────────────────────────────────────────────────┐  │     │
│  │  │     SqlTaskRepository : ITaskRepository              │  │     │
│  │  │  ┌────────────────────────────────────────────────┐  │  │     │
│  │  │  │  Receives connection & transaction from UoW    │  │  │     │
│  │  │  │                                                │  │  │     │
│  │  │  │  + GetTasks()                                  │  │  │     │
│  │  │  │    → SELECT Id, Title, IsCompleted,            │  │  │     │
│  │  │  │       CreatedAt, UpdatedAt FROM Tasks          │  │  │     │
│  │  │  │                                                │  │  │     │
│  │  │  │  + GetTask(id)                                 │  │  │     │
│  │  │  │    → SELECT * FROM Tasks WHERE Id = @Id        │  │  │     │
│  │  │  │                                                │  │  │     │
│  │  │  │  + PostTask(task)                              │  │  │     │
│  │  │  │    → INSERT INTO Tasks ...                     │  │  │     │
│  │  │  │ → SCOPE_IDENTITY()                             │  │  │     │
│  │  │  │                                                │  │  │     │
│  │  │  │  + PutTask(id, task)                           │  │  │     │
│  │  │  │    → UPDATE Tasks SET ...                      │  │  │     │
│  │  │  │                                                │  │  │     │
│  │  │  │  + DeleteTask(id)                              │  │  │     │ 
│  │  │  │    → DELETE FROM Tasks WHERE Id = @Id          │  │  │     │
│  │  │  └────────────────────────────────────────────────┘  │  │     │
│  │  └──────────────────────────────────────────────────────┘  │     │
│  └────────────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────────────┘
                            │
                            │ ADO.NET
                            ▼
┌─────────────────────────────────────────────────────────────────────┐
│           DATA LAYER                                                │  
│  ┌────────────────────────────────────────────────────────────┐     │
│  │          SQL Server Database                               │     │
│  │  ┌──────────────────────────────────────────────────────┐  │     │
│  │  │  Table: Tasks                                        │  │     │
│  │  │  ─────────────────────────────────────────────────── │  │     │
│  │  │  Id         INT PRIMARY KEY IDENTITY(1,1)            │  │     │
│  │  │  Title     NVARCHAR(200) NOT NULL                    │  │     │
│  │  │  IsCompleted  BIT NOT NULL DEFAULT 0                 │  │     │
│  │  │  CreatedAt    DATETIME NOT NULL DEFAULT GETDATE()    │  │     │
│  │  │  UpdatedAt  DATETIME NOT NULL DEFAULT GETDATE()      │  │     │
│  │  └──────────────────────────────────────────────────────┘  │     │
│  │                                                            │     │
│  │  Connection String: TodoListDBConnection                   │     │
│  │  Server: (localdb)\MSSQLLocalDB                            │     │
│  │  Database: TodoListDB                                      │     │ 
│  └────────────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│            CROSS-CUTTING CONCERNS                                   │
│                                                                     │
│  ┌──────────────────────┐  ┌─────────────────────────────────┐      │
│  │  Unity Container     │  │  AutoMapper                     │      │
│  │  (DI Container)      │  │  (Object Mapping)               │      │
│  │                      │  │                                 │      │
│  │  Register:           │  │  MappingProfile:                │      │ 
│  │  • IUnitOfWork       │  │  • TaskModel → TaskDto          │      │
│  │    → UnitOfWork      │  │  • TaskCreateDto → TaskModel    │      │
│  │  • IMapper           │  │  • TaskUpdateDto → TaskModel    │      │
│  │    → Mapper Instance │  │                                 │      │
│  │                      │  │  Automatic property mapping     │      │
│  │  Lifetime:           │  │  Reduces boilerplate code       │      │  
│  │  Hierarchical        │  │                                 │      │
│  │  (Per Request)       │  │                                 │      │
│  └──────────────────────┘  └─────────────────────────────────┘      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 📊 Luồng dữ liệu (Data Flow)

### 🔵 GET Tasks - Lấy danh sách
```
┌─────────────┐
│   Client    │
│ JavaScript  │
└──────┬──────┘
       │ HTTP GET /api/TasksApi
       ▼
┌──────────────────┐
│   CorsHandler    │ ← Handle CORS preflight (OPTIONS)
└────────┬─────────┘
         │ Pass through
         ▼
┌─────────────────────────┐
│  TasksApiController     │
│  .GetTasks()            │
└──────┬──────────────────┘
       │ Request UoW
       ▼
┌─────────────────────────┐
│  UnitOfWork.Tasks       │ ← SqlConnection + Transaction active
│  .GetTasks()            │
└──────┬──────────────────┘
       │ Execute SQL
       ▼
┌─────────────────────────┐
│  SqlTaskRepository      │
│  .GetTasks()            │
│                         │
│  SELECT Id, Title,      │
│    IsCompleted,         │
│    CreatedAt,           │
│    UpdatedAt            │
│  FROM Tasks             │
│  ORDER BY Id DESC       │
└──────┬──────────────────┘
       │ SqlDataReader
       ▼
┌─────────────────────────┐
│  List<TaskModel>        │ ← Domain Models
└──────┬──────────────────┘
       │ AutoMapper
       ▼
┌─────────────────────────┐
│  IEnumerable<TaskDto>   │ ← DTOs
└──────┬──────────────────┘
       │ Return JSON
       ▼
┌─────────────────────────┐
│  HTTP 200 OK            │
│  [                      │
│   {                     │
│    "Id": 1,             │
│    "Title": "Task 1",   │
│    "IsCompleted": false,│
│    "CreatedAt": "...",  │
│    "UpdatedAt": "..."   │
│   }                     │
│  ]                      │
└─────────────────────────┘
```

### 🟢 POST Task - Tạo mới
```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │ HTTP POST /api/TasksApi
       │ Body: { "Title": "New Task" }
       ▼
┌──────────────────┐
│   CorsHandler    │ ← OPTIONS preflight → 200 OK
└────────┬─────────┘
         │
         ▼
┌─────────────────────────────────┐
│  TasksApiController.PostTask()  │
│  (TaskCreateDto taskDto)        │
└──────┬──────────────────────────┘
       │ [Required, MinLength(3)]
       │ ModelState.IsValid?
       ▼
┌──────────────────────┐
│AutoMapper            │
│  TaskCreateDto →     │
│  TaskModel           │
└──────┬───────────────┘
       │
       ▼
┌───────────────────────────────┐
│  taskModel.IsCompleted = false│
│  taskModel.CreatedAt = Now    │
│  taskModel.UpdatedAt = Now    │
└──────┬────────────────────────┘
       │
       ▼
┌──────────────────────────┐
│  UnitOfWork.Tasks        │
│  .PostTask(taskModel)    │
└──────┬───────────────────┘
       │
       ▼
┌─────────────────────────────────────────┐
│  SqlTaskRepository                      │
│  .PostTask()                            │
│                                         │
│  INSERT INTO Tasks                      │
│  (Title, IsCompleted)                   │
│  VALUES (@Title, @IsCompleted)          │
│  SELECT SCOPE_IDENTITY()                │ ← Get new ID 
└──────┬──────────────────────────────────┘
       │ Return taskModel with Id
       ▼
┌──────────────────────────┐
│  UnitOfWork              │
│  .SaveChanges()          │
│  → Transaction.Commit()  │ ← Finalize transaction
└──────┬───────────────────┘
       │
       ▼
┌──────────────────────────┐
│  AutoMapper              │
│  TaskModel → TaskDto     │
└──────┬───────────────────┘
       │
       ▼
┌──────────────────────────────┐
│  HTTP 201 Created            │
│  Location: .../api/TasksApi/1│
│  {                           │
│    "Id": 1,                  │
│    "Title": "New Task",      │
│    "IsCompleted": false,     │
│    ...                       │
│  }                           │
└──────────────────────────────┘
```

### 🟡 PUT Task - Cập nhật
```
Client
  │ HTTP PUT /api/TasksApi/1
  │ Body: { "Title": "Updated", "IsCompleted": true }
  ▼
TasksApiController.PutTask(1, TaskUpdateDto)
  │ ModelState.IsValid?
  ▼
UnitOfWork.Tasks.GetTask(1)  ← Check existence
  │ taskModelFromDb exists?
  ▼
AutoMapper.Map(taskDto, taskModelFromDb)  ← Update properties
  ▼
UnitOfWork.Tasks.PutTask(1, taskModelFromDb)
  │ UPDATE Tasks SET Title=..., IsCompleted=...
  ▼
UnitOfWork.SaveChanges()  ← Commit
  ▼
HTTP 200 OK
```

### 🔴 DELETE Task - Xóa
```
Client
  │ HTTP DELETE /api/TasksApi/1
  ▼
TasksApiController.DeleteTask(1)
  │
  ▼
UnitOfWork.Tasks.GetTask(1)  ← Check existence
  │ exists?
  ▼
UnitOfWork.Tasks.DeleteTask(1)
  │ DELETE FROM Tasks WHERE Id = @Id
  ▼
UnitOfWork.SaveChanges()  ← Commit
  ▼
HTTP 200 OK
```

---

## 🎨 Design Patterns

### 1️⃣ Repository Pattern
**Mục đích**: Tách biệt logic truy cập dữ liệu khỏi business logic

```csharp
// Interface
public interface ITaskRepository
{
    IEnumerable<TaskModel> GetTasks();
  TaskModel GetTask(int id);
    TaskModel PostTask(TaskModel task);
    void PutTask(int id, TaskModel task);
 void DeleteTask(int id);
}

// Implementation
public class SqlTaskRepository : ITaskRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction _transaction;
    
    // ... implementations
}
```

**Lợi ích**:
- ✅ Dễ dàng thay đổi cơ sở dữ liệu (SQL → MongoDB, etc.)
- ✅ Dễ test (mock ITaskRepository)
- ✅ Tập trung logic truy cập dữ liệu

### 2️⃣ Unit of Work Pattern
**Mục đích**: Quản lý transactions và đảm bảo consistency

```csharp
public interface IUnitOfWork : IDisposable
{
    ITaskRepository Tasks { get; }
    int SaveChanges();  // Commit hoặc Rollback
}

public class UnitOfWork : IUnitOfWork
{
  private readonly SqlConnection _connection;
    private SqlTransaction _transaction;
    
    public UnitOfWork()
    {
        _connection = new SqlConnection(connectionString);
        _connection.Open();
        _transaction = _connection.BeginTransaction();
        
     Tasks = new SqlTaskRepository(_connection, _transaction);
    }
    
    public int SaveChanges()
    {
        try
        {
         _transaction.Commit();
            return 1;
        }
        catch
        {
 _transaction.Rollback();
 throw;
    }
    }
}
```

**Lợi ích**:
- ✅ Tất cả thay đổi commit/rollback cùng lúc
- ✅ Đảm bảo ACID properties
- ✅ Quản lý connection tập trung

### 3️⃣ Dependency Injection
**Mục đích**: Giảm coupling, tăng testability

```csharp
// Unity Container Registration
public static void RegisterComponents()
{
    var container = new UnityContainer();
    
    // Register AutoMapper
    var mapperConfig = new MapperConfiguration(cfg => {
        cfg.AddProfile<MappingProfile>();
    });
    container.RegisterInstance<IMapper>(mapperConfig.CreateMapper());
    
    // Register UnitOfWork
  container.RegisterType<IUnitOfWork, UnitOfWork>(
        new HierarchicalLifetimeManager()  // Per request
    );
    
    GlobalConfiguration.Configuration.DependencyResolver = 
  new UnityDependencyResolver(container);
}

// Controller với DI
public class TasksApiController : ApiController
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    
    // Unity tự động inject
    public TasksApiController(IUnitOfWork uow, IMapper mapper)
    {
     _uow = uow;
        _mapper = mapper;
    }
}
```

### 4️⃣ DTO Pattern
**Mục đích**: Bảo vệ domain model, tối ưu data transfer

```csharp
// Domain Model (Internal)
public class TaskModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// DTOs (External API)
public class TaskDto  // Read
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TaskCreateDto  // Create
{
 [Required]
    [MinLength(3)]
    public string Title { get; set; }
}

public class TaskUpdateDto  // Update
{
    [Required]
    [MinLength(3)]
    public string Title { get; set; }
    
    [Required]
    public bool IsCompleted { get; set; }
}
```

**Mapping với AutoMapper**:
```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<TaskModel, TaskDto>();
        CreateMap<TaskCreateDto, TaskModel>();
      CreateMap<TaskUpdateDto, TaskModel>();
    }
}
```

---

## 📡 API Documentation

### Base URL
```
https://localhost:44348/api/TasksApi
```

### Endpoints

| Method | Endpoint | Description | Request Body | Response |
|--------|----------|-------------|--------------|----------|
| **GET** | `/api/TasksApi` | Lấy tất cả tasks | - | `200 OK` + TaskDto[] |
| **GET** | `/api/TasksApi/{id}` | Lấy task theo ID | - | `200 OK` + TaskDto hoặc `404` |
| **POST** | `/api/TasksApi` | Tạo task mới | TaskCreateDto | `201 Created` + TaskDto |
| **PUT** | `/api/TasksApi/{id}` | Cập nhật task | TaskUpdateDto | `200 OK` hoặc `404` |
| **DELETE** | `/api/TasksApi/{id}` | Xóa task | - | `200 OK` hoặc `404` |

### Request/Response Examples

#### GET /api/TasksApi
**Response** `200 OK`:
```json
[
    {
        "Id": 1,
        "Title": "Học ASP.NET MVC",
        "IsCompleted": false,
        "CreatedAt": "2024-01-15T10:30:00",
        "UpdatedAt": "2024-01-15T10:30:00"
    },
    {
        "Id": 2,
     "Title": "Học Design Patterns",
        "IsCompleted": true,
        "CreatedAt": "2024-01-15T11:00:00",
    "UpdatedAt": "2024-01-15T14:20:00"
    }
]
```

#### POST /api/TasksApi
**Request**:
```json
{
    "Title": "Học Unit of Work Pattern"
}
```

**Response** `201 Created`:
```json
{
    "Id": 3,
    "Title": "Học Unit of Work Pattern",
    "IsCompleted": false,
    "CreatedAt": "2024-01-15T15:00:00",
    "UpdatedAt": "2024-01-15T15:00:00"
}
```

#### PUT /api/TasksApi/3
**Request**:
```json
{
    "Title": "Học Unit of Work Pattern - Completed",
    "IsCompleted": true
}
```

**Response** `200 OK`

#### DELETE /api/TasksApi/3
**Response** `200 OK`

---

## 🔧 Technologies & Versions

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET Framework | 4.7.2 | Platform |
| C# | 7.3 | Programming Language |
| ASP.NET MVC | 5.2.9 | Web Framework (Server-side) |
| ASP.NET Web API | 5.3.0 | RESTful API Framework |
| ADO.NET | Built-in | Database Access |
| SQL Server LocalDB | - | Database |
| Unity Container | 5.11.1 | Dependency Injection |
| AutoMapper | 10.1.1 | Object-Object Mapping |
| Microsoft.AspNet.WebApi.Cors | 5.3.0 | CORS Support |
| Bootstrap | 5.3.3 | UI Framework |

---

## 🚀 Hướng dẫn cài đặt

### 1. Yêu cầu hệ thống
- Visual Studio 2019/2022
- SQL Server hoặc SQL Server LocalDB
- .NET Framework 4.7.2 SDK

### 2. Clone Repository
```bash
git clone https://github.com/PhatDo04/TodoListMVC.git
cd TodoListMVC
```

### 3. Restore NuGet Packages
Trong Visual Studio:
```
Tools > NuGet Package Manager > Restore NuGet Packages
```

Hoặc Package Manager Console:
```powershell
Update-Package -reinstall
```

### 4. Tạo Database

Mở SQL Server Management Studio hoặc Visual Studio SQL Server Object Explorer và chạy:

```sql
CREATE DATABASE TodoListDB;
GO

USE TodoListDB;
GO

CREATE TABLE Tasks (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(200) NOT NULL,
    IsCompleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Insert sample data
INSERT INTO Tasks (Title, IsCompleted, CreatedAt, UpdatedAt)
VALUES 
    ('Học ASP.NET MVC', 0, GETDATE(), GETDATE()),
    ('Học Repository Pattern', 1, GETDATE(), GETDATE()),
    ('Học Unit of Work', 0, GETDATE(), GETDATE());
GO
```

### 5. Cấu hình Connection String

Kiểm tra `Web.config`:
```xml
<connectionStrings>
    <add name="TodoListDBConnection" 
         connectionString="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TodoListDB;Integrated Security=True" 
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

### 6. Chạy ứng dụng

Trong Visual Studio: nhấn `F5` hoặc `Ctrl+F5`

Ứng dụng sẽ chạy tại:
- **MVC**: `https://localhost:44348/Tasks`
- **API**: `https://localhost:44348/api/TasksApi`

---

## 🌐 CORS Configuration

### Vấn đề
Khi gọi API từ domain khác (ví dụ: `http://localhost:5500`), browser sẽ chặn request do CORS policy.

### Giải pháp

**1. CorsHandler** - Xử lý OPTIONS preflight:
```csharp
public class CorsHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
  {
      if (request.Method == HttpMethod.Options)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.Add("Access-Control-Allow-Origin", "*");
response.Headers.Add("Access-Control-Allow-Methods", 
            "GET, POST, PUT, DELETE, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", 
  "Content-Type, Authorization");
   return Task.FromResult(response);
        }
        return base.SendAsync(request, cancellationToken);
    }
}
```

**2. WebApiConfig** - Enable CORS:
```csharp
public static void Register(HttpConfiguration config)
{
    // Add CORS handler
    config.MessageHandlers.Add(new CorsHandler());
    
    // Enable CORS globally
    var cors = new EnableCorsAttribute("*", "*", "*");
    config.EnableCors(cors);
    
    // ... other config
}
```

**3. Web.config** - Remove WebDAV:
```xml
<system.webServer>
    <modules>
        <remove name="WebDAVModule" />
    </modules>
</system.webServer>
```

---

## 🗂️ Cấu trúc thư mục

```
TodoListMVC/
├── App_Start/
│   ├── BundleConfig.cs
│   ├── FilterConfig.cs
│   ├── MappingProfile.cs          # AutoMapper configuration
│   ├── RouteConfig.cs
│   ├── UnityConfig.cs             # DI configuration
│ └── WebApiConfig.cs            # Web API + CORS config
├── Controllers/
│   ├── HomeController.cs
│   ├── TasksController.cs         # MVC Controller (ADO.NET)
│   └── TasksApiController.cs      # API Controller (UoW + DI)
├── DTOs/
│   ├── TaskDto.cs      # Read DTO
│   ├── TaskCreateDto.cs           # Create DTO
│   └── TaskUpdateDto.cs     # Update DTO
├── Handlers/
│   └── CorsHandler.cs    # CORS preflight handler
├── Models/
│   └── TaskModel.cs   # Domain model
├── Repositories/
│   ├── ITaskRepository.cs       # Repository interface
│   ├── SqlTaskRepository.cs       # Repository implementation
│├── IUnitOfWork.cs             # UoW interface
│   └── UnitOfWork.cs              # UoW implementation
├── Views/
│   └── Tasks/
│       ├── Index.cshtml       # Task list view
│└── Edit.cshtml        # Edit task view
├── Global.asax.cs           # Application entry point
├── Web.config              # App configuration
└── packages.config     # NuGet packages
```

---

## 🧪 Testing API với cURL

### GET All Tasks
```bash
curl -X GET https://localhost:44348/api/TasksApi
```

### GET Single Task
```bash
curl -X GET https://localhost:44348/api/TasksApi/1
```

### POST New Task
```bash
curl -X POST https://localhost:44348/api/TasksApi \
  -H "Content-Type: application/json" \
  -d '{"Title":"Học LINQ"}'
```

### PUT Update Task
```bash
curl -X PUT https://localhost:44348/api/TasksApi/1 \
  -H "Content-Type: application/json" \
  -d '{"Title":"Học LINQ - Completed","IsCompleted":true}'
```

### DELETE Task
```bash
curl -X DELETE https://localhost:44348/api/TasksApi/1
```

---

## 🐛 Troubleshooting

### ❌ Lỗi CORS
**Triệu chứng**: 
```
Access to XMLHttpRequest has been blocked by CORS policy
```

**Giải pháp**:
1. Kiểm tra `CorsHandler` đã được add vào `MessageHandlers`
2. `EnableCors()` phải gọi TRƯỚC `MapHttpAttributeRoutes()`
3. Remove `WebDAVModule` trong `Web.config`

### ❌ Lỗi Connection String
**Triệu chứng**:
```
NullReferenceException at UnitOfWork constructor
```

**Giải pháp**: 
Kiểm tra tên connection string khớp:
```csharp
// UnitOfWork.cs
.ConnectionStrings["TodoListDBConnection"]  // ← Tên phải khớp Web.config
```

### ❌ Lỗi AutoMapper
**Triệu chứng**:
```
MapperConfiguration constructor error
```

**Giải pháp**: Downgrade về version 10.1.1:
```powershell
Uninstall-Package AutoMapper
Install-Package AutoMapper -Version 10.1.1
```

### ❌ Lỗi Unity PerRequestLifetimeManager
**Triệu chứng**:
```
PerRequestLifetimeManager does not exist
```

**Giải pháp**: Dùng `HierarchicalLifetimeManager`:
```csharp
container.RegisterType<IUnitOfWork, UnitOfWork>(
    new HierarchicalLifetimeManager()
);
```

---

## 💡 Best Practices Được Áp Dụng

### 1. Security
✅ **Parameterized Queries** - Chống SQL Injection:
```csharp
cmd.Parameters.AddWithValue("@Id", id);
```

✅ **Input Validation** - Data Annotations:
```csharp
[Required]
[MinLength(3)]
public string Title { get; set; }
```

### 2. Resource Management
✅ **Using statements** - Tự động dispose:
```csharp
using (SqlCommand cmd = CreateCommand())
{
    // ...
}
```

✅ **IDisposable Pattern**:
```csharp
public class UnitOfWork : IUnitOfWork, IDisposable
{
    public void Dispose()
    {
        _transaction?.Dispose();
_connection?.Dispose();
    }
}
```

### 3. Transaction Management
✅ **ACID compliance** với Unit of Work:
```csharp
try
{
    _transaction.Commit();
}
catch
{
    _transaction.Rollback();
    throw;
}
```

### 4. Separation of Concerns
✅ Phân tầng rõ ràng:
- Controllers → Business Logic
- Repositories → Data Access
- DTOs → Data Transfer
- Models → Domain

---

## 📚 Tài liệu tham khảo

- [ASP.NET MVC Documentation](https://docs.microsoft.com/en-us/aspnet/mvc/)
- [ASP.NET Web API](https://docs.microsoft.com/en-us/aspnet/web-api/)
- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [Unit of Work Pattern](https://www.martinfowler.com/eaaCatalog/unitOfWork.html)
- [AutoMapper](https://docs.automapper.org/)
- [Unity Container](https://github.com/unitycontainer/unity)
- [CORS in ASP.NET Web API](https://docs.microsoft.com/en-us/aspnet/web-api/overview/security/enabling-cross-origin-requests-in-web-api)

---

## 📝 Notes

### MVC vs Web API trong dự án này

| Aspect | TasksController (MVC) | TasksApiController (Web API) |
|--------|----------------------|------------------------------|
| **Pattern** | Direct ADO.NET | Repository + UoW + DI |
| **Data Access** | SqlConnection trực tiếp | Qua IUnitOfWork |
| **Response** | Views (Razor) | JSON |
| **Use Case** | Server-side rendering | RESTful API cho clients |
| **Transaction** | Mỗi action riêng biệt | Managed by UnitOfWork |

### Tại sao có 2 cách?
- **MVC Controller**: Ví dụ về cách truyền thống (ADO.NET thuần)
- **API Controller**: Ví dụ về kiến trúc hiện đại (Clean Architecture)

### Production Recommendations
Khi deploy production, nên:
1. ✅ Chỉ định CORS origins cụ thể (không dùng "*")
2. ✅ Thêm Authentication/Authorization
3. ✅ Implement logging (Serilog, NLog)
4. ✅ Error handling tốt hơn
5. ✅ Add caching (Redis, MemoryCache)
6. ✅ Unit tests & Integration tests
7. ✅ API versioning
8. ✅ Rate limiting

---

## 👤 Author

**Phat Do**
- GitHub: [@PhatDo04](https://github.com/PhatDo04)
- Repository: [TodoListMVC](https://github.com/PhatDo04/TodoListMVC)

---

## 📄 License

This project is for educational purposes.

---

**⚠️ Lưu ý**: Đây là project học tập, không sử dụng cho production mà không có thêm các security measures và optimizations cần thiết!
