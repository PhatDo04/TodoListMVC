-- =============================================
-- Script: T?o b?ng Users và d? li?u m?u
-- Database: TodoListDB
-- Author: PhatDo
-- =============================================

USE TodoListDB;
GO

-- T?o b?ng Users
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Email NVARCHAR(255) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
    PRINT 'B?ng Users ?ã ???c t?o thành công!';
END
ELSE
BEGIN
    PRINT 'B?ng Users ?ã t?n t?i!';
END
GO

-- Thêm user m?u
-- Password: password123
-- Hash ???c t?o b?ng BCrypt v?i work factor 11
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'test@example.com')
BEGIN
    INSERT INTO Users (Email, PasswordHash, CreatedAt)
    VALUES (
        'test@example.com',
        '$2a$11$rGfJXwV8z5F5g5g5g5g5g5u7KqQqQqQqQqQqQqQqQqQqQqQqQq', -- ? Thay b?ng hash th?c t?
        GETDATE()
    );
    PRINT 'User test@example.com ?ã ???c t?o!';
END
GO

-- Thêm user admin
-- Password: admin123
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'admin@example.com')
BEGIN
    INSERT INTO Users (Email, PasswordHash, CreatedAt)
    VALUES (
        'admin@example.com',
        '$2a$11$anotherHashHereForAdminPassword1234567890', -- ? Thay b?ng hash th?c t?
        GETDATE()
    );
    PRINT 'User admin@example.com ?ã ???c t?o!';
END
GO

-- Ki?m tra d? li?u
SELECT * FROM Users;
GO

-- =============================================
-- H??NG D?N T?O PASSWORD HASH
-- =============================================
-- B?n c?n hash password tr??c khi insert vào database.
-- 
-- **Cách 1: Dùng C# Console App**
-- 
-- using BCrypt.Net;
-- 
-- class Program
-- {
--     static void Main()
--     {
--         string password = "password123";
--         string hash = BCrypt.Net.BCrypt.HashPassword(password);
--         Console.WriteLine("Hash: " + hash);
--     }
-- }
--
-- **Cách 2: Dùng Online Tool**
-- https://bcrypt-generator.com/
-- Nh?p password ? Ch?n Rounds: 11 ? Copy hash
--
-- **Cách 3: Dùng PowerShell**
-- Install-Package BCrypt.Net-Next
-- [BCrypt.Net.BCrypt]::HashPassword("password123")
-- =============================================
