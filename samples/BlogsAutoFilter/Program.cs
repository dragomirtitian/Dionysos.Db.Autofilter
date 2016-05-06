using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogsAutoFilter
{
    class Program
    {
        static void Main(string[] args)
        {
            var configType = args.ElementAtOrDefault(0) ?? "deleteAndSecurity";
            //if (configType == "deleteAndSecurity") AutofilterConfigurationForDeleteAndSecurity.Configure();
            //if (configType == "delete") AutofilterConfigurationForDelete.Configure();
            //if (configType == "deleteWithParams") AutofilterConfigurationForDeleteWithParameters.Configure();
            AutofilterConfiguratinTest.Configure();
            RunQueries();
        }
        static void RunQueries()
        {
            using (var ctx = new BlogsContext())
            {
                ctx.Database.Log = Console.WriteLine;

                ctx.BlogRights.ToArray();

                // Simple Query On Blogs
                ctx.Blogs.Where(_ => _.Name.Contains("x")).ToArray();
                // Include Reference Property 
                ctx.Posts.Include(_ => _.Blog).ToArray();
                // Include Collection Property 
                ctx.Users.Include(_ => _.Blogs).ToArray();
                // Include lazy load collection property
                ctx.Users.ToArray()
                    .SelectMany(_ => _.Blogs).ToArray();

                ctx.Blogs.Include(_ => _.Posts).ToArray();

            }
        }
    }

    public class BlogsContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public bool? IsDeleted { get; set; }
        public int IdCurrentUser { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<BlogRights> BlogRights { get; set; }
    }
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<Blog> Blogs { get; set; }
    }
    public class Blog
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<Post> Posts { get; set; }
        public bool IsDeleted { get; set; }
        public int IdUser { get; set; }

        [ForeignKey("IdUser")]
        public User User { get; set; }

        public virtual List<BlogRights> BlogRights { get; set; }
    }

    public class Post
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int IdBlog { get; set; }

        [ForeignKey("IdBlog")]
        public virtual Blog Blog { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class BlogRights
    {
        [Key]
        public int Id { get; set; }
        public int IdBlog { get; set; }
        public int IdUser { get; set; }

        [ForeignKey("IdUser")]
        public User User { get; set; }

        [ForeignKey("IdBlog")]
        public virtual Blog Blog { get; set; }
    }
}
