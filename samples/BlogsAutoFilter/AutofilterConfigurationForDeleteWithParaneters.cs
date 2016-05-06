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
    public static class AutofilterConfigurationForDeleteWithParameters
    {
        public static AutoFilterRegistry AutoFilterRegistry { get; private set; }
        public static void Configure()
        {
            AutoFilterRegistry = new AutoFilterRegistry();

            AutoFilterRegistry.AddGlobalParameter("IsDeleted", c => ((BlogsContext)c).IsDeleted);

            bool? isDeleted = null;
            AutoFilterRegistry.FiltersForContext<BlogsContext>()
                .AddFilter(
                    c => c.Blogs.Where(b => isDeleted == null || b.IsDeleted == isDeleted),
                    FilterType.All, "IsDeleted", "IsDeleted")
                .AddFilter(
                    c => c.Posts.Where(p => isDeleted == null || (p.IsDeleted == isDeleted && p.Blog.IsDeleted == isDeleted)),
                    FilterType.All, "IsDeleted", "IsDeleted", "IsDeleted");

            DbInterception.Add(new AutoFilterTreeInterceptor(AutoFilterRegistry));
            DbInterception.Add(new AutoFilterCommnadInterceptor(AutoFilterRegistry));
        }
    }
}
