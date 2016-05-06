using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dionysos.Db.Autofilter
{
    public class AutoFilterTreeInterceptor : IDbCommandTreeInterceptor
    {
        private readonly AutoFilterRegistry registry;

        public AutoFilterTreeInterceptor(AutoFilterRegistry reg)
        {
            this.registry = reg;
        }
        public void TreeCreated(DbCommandTreeInterceptionContext interceptionContext)
        {
            if (interceptionContext.OriginalResult.DataSpace == DataSpace.CSpace)
            {
                var objectContexts = interceptionContext.ObjectContexts.First();
                var dbContexts = interceptionContext.DbContexts.First();
                var queryCommand = interceptionContext.Result as DbQueryCommandTree;
                if (queryCommand != null)
                {
                    Console.WriteLine(queryCommand.Parameters.ToString());
                    var newQuery = queryCommand.Query.Accept(new AutoFilterExpressionVisitor(dbContexts, objectContexts.MetadataWorkspace, this.registry));
                    interceptionContext.Result = new DbQueryCommandTree(queryCommand.MetadataWorkspace, queryCommand.DataSpace, newQuery);
                }
            }   
        }
    }
    
    class AutoFilterExpressionVisitor : DefaultExpressionVisitor
    {
        private AutoFilterRegistry registry;
        private MetadataWorkspace workspace;
        private DbContext dbContext;

        public AutoFilterExpressionVisitor(DbContext dbContext, MetadataWorkspace workspace, AutoFilterRegistry registry)
        {
            this.workspace = workspace;
            this.registry = registry;
            this.dbContext = dbContext;
        }

        private static DbExpression MergeCollectionFilter(DbFilterExpression filterExpression, DbExpression targetExpression)
        {
            var bind = DbExpressionBuilder.Bind(targetExpression);
            return DbExpressionBuilder.Filter
            (
                bind,
                filterExpression.Predicate.Accept(new VariableVisitor(bind.Variable, filterExpression.Input.Variable))
            );
        }

        private DbExpression ForEachFilter(DbExpression targetExpression, FilterType filterType, Func<DbFilterExpression, DbExpression, DbExpression> func)
        {
            var edmType = targetExpression.ResultType.EdmType;
            var objType = EfExtensions.GetClrTypeFromCSpaceType(this.workspace, edmType);
            foreach (var filter in this.registry.GetFilterProvider(objType))
            {
                if (!filter.SupportedFilterTypes.HasFlag(FilterType.TableScan)) continue;

                var filterExpression = filter.GetFilter(targetExpression, this.dbContext, edmType, filterType);

                if (filterExpression == null) continue;

                targetExpression = func(filterExpression, targetExpression);
            }

            return targetExpression;
        }

        public override DbExpression Visit(DbScanExpression expression)
        {
            return ForEachFilter(base.Visit(expression), FilterType.TableScan, MergeCollectionFilter);
        }
        
        public override DbExpression Visit(DbPropertyExpression expression)
        {
            var resultType = expression.ResultType.EdmType;
            if (resultType is CollectionType)
            {
                return ForEachFilter(base.Visit(expression), FilterType.CollectionProperty, MergeCollectionFilter);
            }
            else if (resultType is StructuralType)
            {
                var cond =  ForEachFilter(expression, FilterType.Property, (dbF, t) =>
                {
                    var c = dbF.Predicate.Accept(new VariableVisitor(expression, dbF.Input.Variable));
                    if (t == expression) return c;

                    return DbExpressionBuilder.And(t, c);
                });
                if (cond == expression) return expression;

                return DbExpressionBuilder.Case
                (
                    new[] { cond },
                    new[] { expression },
                    DbExpressionBuilder.Null(expression.ResultType) 
                );
            }

            return base.Visit(expression);
        }


    }

    public class VariableVisitor : DefaultExpressionVisitor
    {
        private DbExpression replaceWith;
        private DbVariableReferenceExpression searchFor;

        public VariableVisitor(DbExpression replaceWith, DbVariableReferenceExpression searchFor)
        {
            this.replaceWith = replaceWith;
            this.searchFor = searchFor;
        }

        public override DbExpression Visit(DbVariableReferenceExpression expression)
        {
            if (expression == this.searchFor) return this.replaceWith;
            return base.Visit(expression);
        }
    }


}
