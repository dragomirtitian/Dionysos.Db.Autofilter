using Dionysos.Db.Autofilter.Config;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dionysos.Db.Autofilter.Providers
{
    public class SoftDeleteProvider<TContext> : IFilterProvider, IGlobalParameterProvider
        where TContext: DbContext
    {
        public SoftDeleteProvider(string prop, Func<TContext, object> getParameterValue, Type type = null, string globalParameterName = null)
        {
            this.PropertyName = prop;
            this.Type = type;
            this.GlobalParameterName = globalParameterName ?? ("IsDeleted" + DateTime.Now.Ticks);
            this.GetParameterValue = getParameterValue;
        }

        public Func<TContext, object> GetParameterValue { get; private set; }
        public string GlobalParameterName { get; private set; }

        public IEnumerable<string> Parameter
        {
            get
            {
                if (this.GetParameterValue != null) yield return this.GlobalParameterName;
            }
        }
            

        public string PropertyName { get; private set; }
        public FilterType SupportedFilterTypes { get; set; }
        public Type Type { get; private set; }
        
        public DbFilterExpression GetFilter(DbExpression originalExpression, DbContext context, EdmType edmType, FilterType filterType)
        {
            if (!(context is TContext)) return null;

            EdmMember prop;
            if (edmType is CollectionType) edmType = ((CollectionType)edmType).TypeUsage.EdmType;


            if(!((StructuralType)edmType).Members.TryGetValue(this.PropertyName, true, out prop))
            {
                return null;
            }

            var objContext = ((IObjectContextAdapter)context).ObjectContext;
            var metadata = objContext.MetadataWorkspace;
            var entitySet = metadata.GetEntityContainer(objContext.DefaultContainerName, DataSpace.CSpace)
                .BaseEntitySets
                .First(_=> _.ElementType == edmType);
            var bind = DbExpressionBuilder.Scan(entitySet).Bind();

            var nullableBool = DbExpression.FromBoolean(true).ResultType;
            var parameter = DbExpressionBuilder.Parameter(nullableBool, this.GlobalParameterName);
            return DbExpressionBuilder
                .Filter
                (
                    bind,
                    
                    DbExpressionBuilder.Property(bind.Variable, (EdmProperty)prop)
                        .Equal(parameter)
                    .Or(parameter.IsNull())
                );
        }

        public object GetParameter(DbContext context, string name)
        {
            if (name == this.GlobalParameterName)
            {
                return this.GetParameterValue != null ? this.GetParameterValue((TContext)context) : false;
            }
            return null;
        }
        
    }
}
