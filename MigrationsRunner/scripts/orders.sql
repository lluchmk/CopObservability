IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240103154043_Initial'
)
BEGIN
    CREATE TABLE [Orders] (
        [Id] uniqueidentifier NOT NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [ProductId] uniqueidentifier NOT NULL,
        [Amount] int NOT NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240103154043_Initial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Amount', N'CustomerId', N'ProductId') AND [object_id] = OBJECT_ID(N'[Orders]'))
        SET IDENTITY_INSERT [Orders] ON;
    EXEC(N'INSERT INTO [Orders] ([Id], [Amount], [CustomerId], [ProductId])
    VALUES (''0eb4b3a1-080f-430b-a42b-527e2c5607a6'', 13, ''87c77822-d53d-4db1-8e66-468e28102456'', ''e7e45871-5885-462f-b6e7-85dec42e037e''),
    (''52dd8301-69d7-4040-b0bf-3695625a15e2'', 15, ''87c77822-d53d-4db1-8e66-468e28102456'', ''5ba58a44-ead2-4efa-96dd-b789101953e6''),
    (''5497896b-485d-4951-a720-e52826460705'', 2, ''c654b145-1a4a-43b4-a741-87b186554edc'', ''9323c4f1-8a0b-4dda-9272-a96b4c59313f''),
    (''60fd41e5-8b52-4923-b72e-ad9b98f38aed'', 19, ''87c77822-d53d-4db1-8e66-468e28102456'', ''e7e45871-5885-462f-b6e7-85dec42e037e''),
    (''86356385-d6cd-48a7-b185-9187e10e9a2b'', 8, ''3f617303-3844-4403-9017-4fb0bd0ac827'', ''e7e45871-5885-462f-b6e7-85dec42e037e''),
    (''a0140d4d-ca10-4225-8182-7a2defb890a1'', 13, ''3f617303-3844-4403-9017-4fb0bd0ac827'', ''e7e45871-5885-462f-b6e7-85dec42e037e''),
    (''d543a9db-ced4-4d9c-b5ba-06f27f447087'', 64, ''87c77822-d53d-4db1-8e66-468e28102456'', ''e7e45871-5885-462f-b6e7-85dec42e037e''),
    (''fde052ba-6d8c-4d6b-ba9a-121d5e9dde8f'', 98, ''c654b145-1a4a-43b4-a741-87b186554edc'', ''e7e45871-5885-462f-b6e7-85dec42e037e'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Amount', N'CustomerId', N'ProductId') AND [object_id] = OBJECT_ID(N'[Orders]'))
        SET IDENTITY_INSERT [Orders] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240103154043_Initial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240103154043_Initial', N'8.0.0');
END;
GO

COMMIT;
GO

