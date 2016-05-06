using CodeFirstStoreFunctions;
using Dionysos.Db.Autofilter;
using Dionysos.Db.Autofilter.Providers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFilterSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var autoFilter = new AutoFilterRegistry();
            bool? isDeleted = false;
            autoFilter.AddFilterProvider(new SoftDeleteProvider<SchoolContext>("IsDeleted", c=> isDeleted.HasValue ? (object)isDeleted.Value : DBNull.Value)
            {
                SupportedFilterTypes = FilterType.All
            });

            var intercept = new AutoFilterTreeInterceptor(autoFilter);
            DbInterception.Add(intercept);
            DbInterception.Add(new AutoFilterCommnadInterceptor(autoFilter));

            using (var ctx = new SchoolContext())
            {
                ctx.Database.Log = Console.WriteLine;
                //ctx.Database.CreateIfNotExists();
                
                ctx.Students.ToArray();

                isDeleted = null;

                ctx.Students.Select(_ => _.Standard).ToArray();
            }
        }
    }

    public class SchoolContext : DbContext, IDbModelCacheKeyProvider
    {
        public SchoolContext() : base()
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add(new FunctionsConvention<SchoolContext>("dbo"));

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Standard> Standards { get; set; }

        [DbFunction("CodeFirstDatabaseSchema", "IsDeleted")]
        public static bool IsDeleted(bool isDeleted)
        {
            throw new NotImplementedException();
        }

        public string CacheKey
        {
            get
            {
                return "A";
            }
        }
    }

    public class Student
    {
        public Student()
        {

        }
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public byte[] Photo { get; set; }
        public decimal Height { get; set; }
        public float Weight { get; set; }

        public Standard Standard { get; set; }

        public bool IsDeleted { get; set; }
    }

    public class Standard
    {
        public Standard()
        {

        }
        public int StandardId { get; set; }
        public string StandardName { get; set; }

        public ICollection<Student> Students { get; set; }

        public bool IsDeleted { get; set; }

    }

}