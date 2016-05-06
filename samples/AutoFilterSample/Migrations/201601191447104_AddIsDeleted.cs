namespace AutoFilterSample.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsDeleted : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Students", "IsDeleted", c => c.Boolean(nullable: false));
            this.Sql(@"
CREATE FUNCTION IsDeleted (@p1 bit) RETURNS bit
AS
BEGIN
	RETURN 1
END
");
        }
        
        public override void Down()
        {
            DropColumn("dbo.Students", "IsDeleted");
            this.Sql(@"
DROP FUNCTION IsDeleted 
");

        }
    }
}
