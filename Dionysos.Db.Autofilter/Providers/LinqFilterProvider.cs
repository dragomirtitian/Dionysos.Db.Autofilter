using Dionysos.Db.Autofilter.Config;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.ELinq;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dionysos.Db.Autofilter.Providers
{
    public class LinqFilterParamter
    {

        private static int id;

        public LinqFilterParamter(Func<DbContext, object> getter)
            :this(null, getter)
        {
        }

        public LinqFilterParamter(string name, Func<DbContext, object> getter = null)
        {
            this.GlobalName = name ?? "linqFilterProvider" + (id++);
            this.ValueGetter = getter;
        }
        public string GlobalName { get; private set; }
        public Func<DbContext, object> ValueGetter { get; private set; }

        public static implicit operator LinqFilterParamter (string name)
        {
            return new LinqFilterParamter(name);
        }
    }

    public class LinqFilterProvider<TContext, TElement> : IFilterProvider, IGlobalParameterProvider
        where TContext : DbContext
    {
        private Dictionary<string, LinqFilterParamter> linqParameters;
        private string[] paramNames;

        public LinqFilterProvider(FilterType supportedFilterTypes, Func<TContext, IQueryable<TElement>> filter, IEnumerable<LinqFilterParamter> parameters)
        {
            this.SupportedFilterTypes = supportedFilterTypes;
            this.Filter = filter;
            this.linqParameters = parameters
                .Where(_=> _.ValueGetter != null)
                .ToDictionary(_=> _.GlobalName);

            this.paramNames = parameters
                .Select(_ => _.GlobalName)
                .ToArray();

        }
        public Func<TContext, IQueryable<TElement>> Filter { get; private set; }

        public IEnumerable<string> Parameter
        {
            get
            {
                return this.linqParameters.Keys;
            }
        }

        public FilterType SupportedFilterTypes { get; private set; }
        public Type Type
        {
            get
            {
                return typeof(TElement);
            }
        }


        public DbFilterExpression GetFilter(DbExpression originalExpression, DbContext context, EdmType type, FilterType filterType)
        {
            var objectContext = ((IObjectContextAdapter)context).ObjectContext;
            var query = this.Filter((TContext)context);
            var expression = LinqFilterProviderExtensions.ExpressionConverter(objectContext, query.Expression);

            return (DbFilterExpression)expression.Accept(new ReplaceParameters(this.paramNames));
        }

        public object GetParameter(DbContext context, string name)
        {
            return this.linqParameters[name].ValueGetter(context);
        }
        
        class ReplaceParameters:DefaultExpressionVisitor
        {
            public ReplaceParameters(string[] newParameters)
            {
                this.newParameters = newParameters;
            }
            Regex regex = new Regex("p__linq__(?<Index>[0-9]*)");
            private string[] newParameters;
            public override DbExpression Visit(DbParameterReferenceExpression expression)
            {
                var m = regex.Match(expression.ParameterName);
                if (!m.Success) return expression;
                var paramIndex = int.Parse(m.Groups["Index"].Value);
                return DbExpressionBuilder.Parameter(expression.ResultType, this.newParameters[paramIndex]);
            }
            
        }
    }

    

    public static class LinqFilterProviderExtensions
    {

        public static readonly Func<ObjectContext, Expression, DbFilterExpression> ExpressionConverter = CreateExpressionConverter();
        private static Func<ObjectContext, Expression, DbFilterExpression> CreateExpressionConverter()
        {
            var dm = new DynamicMethod("expressionConverter", typeof(DbFilterExpression), new[] { typeof(ObjectContext), typeof(Expression) }, true);
            var il = dm.GetILGenerator();
            var bFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var efAssembly = typeof(ObjectContext).Assembly;
            var funcletizer = efAssembly.GetType("System.Data.Entity.Core.Objects.ELinq.Funcletizer");
            var createQueryFuncletizer = funcletizer.GetMethod("CreateQueryFuncletizer", bFlags | BindingFlags.Static);

            var expressionConverter = efAssembly.GetType("System.Data.Entity.Core.Objects.ELinq.ExpressionConverter");
            var ctor = expressionConverter.GetConstructor(bFlags , null, new[] { funcletizer, typeof(Expression) }, null);
            var convert = expressionConverter.GetMethod("Convert", bFlags);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, createQueryFuncletizer);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Callvirt, convert);
            il.Emit(OpCodes.Castclass, typeof(DbFilterExpression));
            il.Emit(OpCodes.Ret);

            return (Func<ObjectContext, Expression, DbFilterExpression>)dm.CreateDelegate(typeof(Func<ObjectContext, Expression, DbFilterExpression>));
        }
    }
}
