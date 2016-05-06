namespace AutoFilterSample.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsDeletedToStandard : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Standards", "IsDeleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Standards", "IsDeleted");
        }
    }
}
