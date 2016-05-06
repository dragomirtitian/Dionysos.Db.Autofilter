namespace BlogsAutoFilter.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BlogsAutoFilter.BlogsContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(BlogsAutoFilter.BlogsContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //

            User titian, lasting;
            context.Users.AddOrUpdate(
                _ => _.Name,
                titian = new User
                {
                    Name = "Titian Cernicova Dragomir",
                },
                lasting = new User
                {
                    Name = "Lasting Software",
                }
            );
            context.Blogs.AddOrUpdate(
              p => p.Name,
              new Blog
              {
                  Name = "Programming with Lasting Software",
                  User  = lasting,

              },
              new Blog
              {
                  Name =  "XProgramming",
                  User = titian,
              },
              new Blog
              {
                  Name = "The cooking blog",
                  IsDeleted = true,
                  User = titian
              }
            );
            //
        }
    }
}
