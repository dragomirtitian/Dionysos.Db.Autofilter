using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dionysos.Db.Autofilter.Providers
{
    public class GlobalParameterProvider : IGlobalParameterProvider
    {
        private string pameterName;
        private Func<DbContext, object> valueGetter;

        public GlobalParameterProvider(string parameterName, Func<DbContext, object> valueGetter)
        {
            this.pameterName = parameterName;
            this.valueGetter = valueGetter;
        }
        public IEnumerable<string> Parameter
        {
            get
            {
                yield return this.pameterName;
            }
        }

        public object GetParameter(DbContext context, string name)
        {
            if (this.pameterName == name) return this.valueGetter(context) ?? DBNull.Value;

            return null;
        }
    }

    public static class GlobalParameterProviderExtensions
    {
        public static AutoFilterRegistry AddGlobalParameter(this AutoFilterRegistry @this, string name, Func<object> valueGetter)
        {
            @this.AddGlobalParameterProvider(new GlobalParameterProvider(name, c => valueGetter()));
            return @this;
        }
        public static AutoFilterRegistry AddGlobalParameter(this AutoFilterRegistry @this, string name, Func<DbContext, object> valueGetter)
        {
            @this.AddGlobalParameterProvider(new GlobalParameterProvider(name, valueGetter));
            return @this;
        }
    }
}
