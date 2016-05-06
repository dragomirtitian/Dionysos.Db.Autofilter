using Dionysos.Db.Autofilter.Providers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dionysos.Db.Autofilter.Config
{
    public class AutofilterProviderConfiguration<TContext>
        where TContext : DbContext
    {
        public AutoFilterRegistry AutoFilter
        {
            get; set;
        }

        public AutofilterProviderConfiguration(AutoFilterRegistry autoFilter)
        {
            this.AutoFilter = autoFilter;
        }
        
        public AutofilterProviderConfiguration<TContext> AddFilter<T>(
            Func<TContext, IQueryable<T>> filter, FilterType type, params LinqFilterParamter[] linqParameters)
        {
            this.AutoFilter.AddFilterProvider(new LinqFilterProvider<TContext, T>(type, filter, linqParameters));
            return this;
        }

        public AutofilterProviderConfiguration<TContext> EnableSoftDelete(string isDeletedProperty, string globalIsDeletedParameter)
        {
            this.AutoFilter
                .AddFilterProvider(new SoftDeleteProvider<TContext>(isDeletedProperty, null, null, globalIsDeletedParameter));
            return this;
        }

        public AutofilterProviderConfiguration<TContext> EnableSoftDelete(string isDeletedProperty, Func<DbContext, object> getter)
        {
            this.AutoFilter
                .AddFilterProvider(new SoftDeleteProvider<TContext>(isDeletedProperty, getter, null));
            return this;
        }
    }

    public static class AutofilterProviderConfigurationExtensions
    {
        public static AutofilterProviderConfiguration<TContext> FiltersForContext<TContext>(this AutoFilterRegistry @this)
            where TContext : DbContext
        {
            return new AutofilterProviderConfiguration<TContext>(@this);
        }
    }

}