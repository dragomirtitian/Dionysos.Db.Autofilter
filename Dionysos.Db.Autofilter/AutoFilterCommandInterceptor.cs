using System;
using System.Collections.Generic;
using System.Data.Common;
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
    public class AutoFilterCommnadInterceptor : IDbCommandInterceptor
    {
        private readonly AutoFilterRegistry registry;
        
        public AutoFilterCommnadInterceptor(AutoFilterRegistry reg)
        {
            this.registry = reg;
            
        }
        
        private void SetGlobalParameters(DbCommand command, DbContext context)
        {
            if (context == null) return;
            foreach (DbParameter param in command.Parameters)
            {
                var provider = this.registry.GetParameterProvider(param.ParameterName);
                if (provider == null) continue;

                param.Value = provider.GetParameter(context, param.ParameterName);
            }
        }

        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            
        }

        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            this.SetGlobalParameters(command, interceptionContext.DbContexts.FirstOrDefault());
        }

        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
        }

        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            this.SetGlobalParameters(command, interceptionContext.DbContexts.FirstOrDefault());
        }

        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            
        }

        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            this.SetGlobalParameters(command, interceptionContext.DbContexts.FirstOrDefault());
        }
    }
}
