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
    public static class AutofilterConfigurationForDelete
    {
        public static AutoFilterRegistry AutoFilterRegistry { get; private set; }
        public static void Configure()
        {
            AutoFilterRegistry = new AutoFilterRegistry();

            AutoFilterRegistry.FiltersForContext<BlogsContext>()
                .AddFilter(c => c.Blogs.Where(b => !b.IsDeleted), FilterType.All)
                .AddFilter(c => c.Posts.Where(p=> !p.IsDeleted && !p.Blog.IsDeleted), FilterType.All);

            DbInterception.Add(new AutoFilterTreeInterceptor(AutoFilterRegistry));
            DbInterception.Add(new AutoFilterCommnadInterceptor(AutoFilterRegistry));
        }
    }
}
