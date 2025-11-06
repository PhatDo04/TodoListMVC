-- Tạo hoặc Sửa một Trigger tên là trg_Tasks_UpdatedAt
CREATE TRIGGER trg_Tasks_UpdatedAt
ON Tasks   -- Gắn Trigger này vào bảng "Tasks"
AFTER INSERT, UPDATE -- Nó sẽ tự động chạy SAU KHI một lệnh INSERT hoặc UPDATE hoàn tất
AS
BEGIN
    -- Dòng này để tăng tốc, không bắt buộc nhưng nên có
    SET NOCOUNT ON; 

    -- Cập nhật cột "UpdatedAt" của (các) dòng VỪA BỊ thay đổi
    -- (Bảng "inserted" là một bảng "ảo" chứa các dòng vừa được thêm/sửa)
    UPDATE T
    SET UpdatedAt = GETDATE()
    FROM Tasks AS T
    INNER JOIN inserted AS I ON T.Id = I.Id;
END