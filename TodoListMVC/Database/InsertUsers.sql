-- =============================================
-- Script: Insert Users v?i BCrypt hashed passwords
-- Database: TodoListDB
-- =============================================
-- Passwords:
-- test@example.com    ? password123
-- admin@example.com   ? admin123
-- user@example.com    ? user123
-- =============================================

USE TodoListDB;
GO

-- Xóa d? li?u c? (n?u test l?i)
-- DELETE FROM Users;
-- GO

-- Insert user: test@example.com (password: password123)
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'test@example.com')
BEGIN
    INSERT INTO Users (Email, PasswordHash, CreatedAt)
    VALUES (
        'test@example.com',
        '$2a$11$YourBCryptHashHereForPassword123XXXXXXXXXXXXXXXXXXXXXXXXX',
        GETDATE()
    );
END
GO

-- Insert user: admin@example.com (password: admin123)
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'admin@example.com')
BEGIN
    INSERT INTO Users (Email, PasswordHash, CreatedAt)
    VALUES (
        'admin@example.com',
        '$2a$11$YourBCryptHashHereForAdmin123XXXXXXXXXXXXXXXXXXXXXXXXXXXX',
        GETDATE()
    );
END
GO

-- Ki?m tra users ?ã insert
SELECT 
    Id,
    Email,
    LEFT(PasswordHash, 20) + '...' AS PasswordHash_Preview,
    CreatedAt
FROM Users;
GO

-- =============================================
-- H??NG D?N L?Y HASH TH?T
-- =============================================
-- 
-- **B??c 1**: M? Package Manager Console trong Visual Studio
-- 
-- **B??c 2**: Ch?y l?nh sau ?? hash password
--
-- C# Interactive ho?c t?o Console App nh?:
--
-- using BCrypt.Net;
-- var hash = BCrypt.Net.BCrypt.HashPassword("password123");
-- Console.WriteLine(hash);
--
-- **B??c 3**: Copy hash và thay th? vào script trên
--
-- **Ho?c dùng online tool**:
-- https://bcrypt-generator.com/
-- - Nh?p password
-- - Ch?n Rounds: 11
-- - Copy hash (b?t ??u b?ng $2a$11$...)
--
-- =============================================
