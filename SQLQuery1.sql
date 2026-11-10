CREATE TABLE [SuspendedSales] (
    [Id] int IDENTITY(1,1) PRIMARY KEY,
    [SuspendedNo] nvarchar(max) NULL,
    [DateSuspended] datetime2 NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL
);

CREATE TABLE [SuspendedSaleItems] (
    [Id] int IDENTITY(1,1) PRIMARY KEY,
    [SuspendedSaleId] int NOT NULL,
    [ProductId] int NOT NULL,
    [ProductName] nvarchar(max) NULL,
    [Quantity] int NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    FOREIGN KEY ([SuspendedSaleId]) REFERENCES [SuspendedSales] ([Id]) ON DELETE CASCADE
);