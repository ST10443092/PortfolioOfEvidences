using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROGP2.Migrations
{
    /// <inheritdoc />
    public partial class AddDateCreatedToClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if column exists, if not add it
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns 
                               WHERE object_id = OBJECT_ID(N'[dbo].[Claims]') 
                               AND name = 'DateCreated')
                BEGIN
                    ALTER TABLE [Claims] ADD [DateCreated] datetime2 NULL;
                END
            ");
            
            // Update all existing records to use current date
            migrationBuilder.Sql("UPDATE [Claims] SET [DateCreated] = GETDATE() WHERE [DateCreated] IS NULL");
            
            // Make column non-nullable with default constraint
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns 
                          WHERE object_id = OBJECT_ID(N'[dbo].[Claims]') 
                          AND name = 'DateCreated')
                BEGIN
                    -- Remove existing default constraint if any
                    DECLARE @ConstraintName NVARCHAR(200)
                    SELECT @ConstraintName = name 
                    FROM sys.default_constraints 
                    WHERE parent_object_id = OBJECT_ID(N'[dbo].[Claims]') 
                    AND parent_column_id = (SELECT column_id FROM sys.columns 
                                           WHERE object_id = OBJECT_ID(N'[dbo].[Claims]') 
                                           AND name = 'DateCreated')
                    
                    IF @ConstraintName IS NOT NULL
                        EXEC('ALTER TABLE [Claims] DROP CONSTRAINT ' + @ConstraintName)
                    
                    -- Alter column to be non-nullable
                    ALTER TABLE [Claims] ALTER COLUMN [DateCreated] datetime2 NOT NULL;
                    
                    -- Add default constraint
                    ALTER TABLE [Claims] ADD CONSTRAINT DF_Claims_DateCreated DEFAULT GETDATE() FOR [DateCreated];
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "Claims");
        }
    }
}
