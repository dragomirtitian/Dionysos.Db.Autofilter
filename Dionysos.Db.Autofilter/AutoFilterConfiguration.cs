using Dionysos.Db.Autofilter.Providers;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dionysos.Db.Autofilter.Config;
using Dionysos.Db.Autofilter;

namespace Dionysos.Db.Autofilter
{
    public static class AutoFilterConfiguration
    {
        public static AutoFilterRegistry AutoFilterRegistry { get; private set; }
        public static void Configure()
        {
            AutoFilterRegistry = new AutoFilterRegistry();

            DbInterception.Add(new AutoFilterTreeInterceptor(AutoFilterRegistry));
            DbInterception.Add(new AutoFilterCommnadInterceptor(AutoFilterRegistry));
        }
    }
}
