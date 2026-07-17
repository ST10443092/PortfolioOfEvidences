using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CLDV6212P1.Migrations
{
    /// <inheritdoc />
    public partial class AddManagerUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update or insert the manager user
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM Users WHERE Email = 'manager@gmail.com' OR UserId = 2)
                BEGIN
                    UPDATE [Users] 
                    SET [Name] = N'Manager',
                        [Surname] = N'User',
                        [PasswordHash] = N'pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=',
                        [Role] = N'Manager',
                        [DateCreated] = '2024-01-01T00:00:00.0000000'
                    WHERE Email = 'manager@gmail.com' OR UserId = 2;
                END
                ELSE
                BEGIN
                    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserId', N'DateCreated', N'Email', N'Name', N'PasswordHash', N'Role', N'Surname') AND [object_id] = OBJECT_ID(N'[Users]'))
                        SET IDENTITY_INSERT [Users] ON;
                    INSERT INTO [Users] ([UserId], [DateCreated], [Email], [Name], [PasswordHash], [Role], [Surname])
                    VALUES (2, '2024-01-01T00:00:00.0000000', N'manager@gmail.com', N'Manager', N'pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=', N'Manager', N'User');
                    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserId', N'DateCreated', N'Email', N'Name', N'PasswordHash', N'Role', N'Surname') AND [object_id] = OBJECT_ID(N'[Users]'))
                        SET IDENTITY_INSERT [Users] OFF;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [Users] WHERE Email = 'manager@gmail.com' AND UserId = 2");
        }
    }
}
