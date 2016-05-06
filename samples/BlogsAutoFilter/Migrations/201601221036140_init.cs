namespace BlogsAutoFilter.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BlogRights",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdBlog = c.Int(nullable: false),
                        IdUser = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Blogs", t => t.IdBlog, cascadeDelete: false)
                .ForeignKey("dbo.Users", t => t.IdUser, cascadeDelete: false)
                .Index(t => t.IdBlog)
                .Index(t => t.IdUser);
            
            CreateTable(
                "dbo.Blogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                        IdUser = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.IdUser, cascadeDelete: true)
                .Index(t => t.IdUser);
            
            CreateTable(
                "dbo.Posts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Content = c.String(),
                        IdBlog = c.Int(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Blogs", t => t.IdBlog, cascadeDelete: true)
                .Index(t => t.IdBlog);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BlogRights", "IdUser", "dbo.Users");
            DropForeignKey("dbo.Blogs", "IdUser", "dbo.Users");
            DropForeignKey("dbo.Posts", "IdBlog", "dbo.Blogs");
            DropForeignKey("dbo.BlogRights", "IdBlog", "dbo.Blogs");
            DropIndex("dbo.Posts", new[] { "IdBlog" });
            DropIndex("dbo.Blogs", new[] { "IdUser" });
            DropIndex("dbo.BlogRights", new[] { "IdUser" });
            DropIndex("dbo.BlogRights", new[] { "IdBlog" });
            DropTable("dbo.Users");
            DropTable("dbo.Posts");
            DropTable("dbo.Blogs");
            DropTable("dbo.BlogRights");
        }
    }
}
