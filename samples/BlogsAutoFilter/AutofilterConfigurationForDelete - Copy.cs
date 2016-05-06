using Dionysos.Db.Autofilter.Providers;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dionysos.Db.Autofilter.Config;
using Dionysos.Db.Autofilter;

namespace BlogsAutoFilter
{
    public static class AutofilterConfiguratinTest
    {
        public static AutoFilterRegistry AutoFilterRegistry { get; private set; }
        public static void Configure()
        {
            AutoFilterRegistry = new AutoFilterRegistry();

            var arr = new[] { 1, 2, 3, 4 };
            AutoFilterRegistry.FiltersForContext<BlogsContext>()
                //.AddFilter(c => c.Blogs.Where(b => a.Contains(b.IdUser)), FilterType.All, "a")
                .AddFilter(c => c.Blogs.Where(b => arr.Any(i=> i > b.IdUser)), FilterType.All)
                ;
            DbInterception.Add(new AutoFilterTreeInterceptor(AutoFilterRegistry));
            DbInterception.Add(new AutoFilterCommnadInterceptor(AutoFilterRegistry));
        }
    }
}
