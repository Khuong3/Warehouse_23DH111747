USE master;
GO

IF DB_ID(N'Warehouse_23DH111747') IS NOT NULL
BEGIN
    ALTER DATABASE Warehouse_23DH111747 SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE Warehouse_23DH111747;
END
GO

CREATE DATABASE Warehouse_23DH111747;
GO

USE Warehouse_23DH111747;
GO

CREATE TABLE dbo.Categories (
    CategoryID   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CategoryName NVARCHAR(100)     NOT NULL
);
GO

CREATE TABLE dbo.Products (
    ProductID   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ProductName NVARCHAR(100)     NOT NULL,
    Category    NVARCHAR(100)     NOT NULL,
    Price       DECIMAL(18,2)     NOT NULL,
    Quantity    INT               NOT NULL
);
GO

INSERT INTO dbo.Categories (CategoryName)
VALUES
    (N'Thiết bị điện tử - Bảo Khương'),
    (N'Phụ kiện âm thanh - Bảo Khương'),
    (N'Nội thất văn phòng - Bảo Khương');
GO

INSERT INTO dbo.Products (ProductName, Category, Price, Quantity)
VALUES
    (N'Bàn phím Bảo Khương',           N'Thiết bị điện tử - Bảo Khương',   1250000.00, 10),
    (N'Chuột không dây Bảo Khương',    N'Thiết bị điện tử - Bảo Khương',    650000.00,  5),
    (N'Tai nghe Bluetooth Bảo Khương', N'Phụ kiện âm thanh - Bảo Khương',  1890000.00,  3),
    (N'Loa di động Bảo Khương',        N'Phụ kiện âm thanh - Bảo Khương',   950000.00,  0),
    (N'Màn hình LED Bảo Khương',       N'Thiết bị điện tử - Bảo Khương',   3450000.00,  8),
    (N'Ghế văn phòng Bảo Khương',      N'Nội thất văn phòng - Bảo Khương', 2100000.00,  2);
GO

SELECT * FROM dbo.Products;
GO
