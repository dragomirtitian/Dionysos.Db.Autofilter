using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Core.Objects.ELinq;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dionysos.Db.Autofilter
{
    public class AutoFilterRegistry
    {
        Dictionary<Type, List<IFilterProvider>> providersByType = new Dictionary<Type, List<IFilterProvider>>();
        List<IFilterProvider> forAllTypes = new List<IFilterProvider>();
        Dictionary<string, IGlobalParameterProvider> globalParameters = new Dictionary<string, IGlobalParameterProvider>();

        public IGlobalParameterProvider GetParameterProvider(string name)
        {
            IGlobalParameterProvider result;
            this.globalParameters.TryGetValue(name, out result);
            return result;
        }

        public void AddGlobalParameterProvider(IGlobalParameterProvider globalParameterProvider)
        {
            foreach (var param in globalParameterProvider.Parameter)
            {
                this.globalParameters[param] = globalParameterProvider;
            }
        }

        public void AddFilterProvider(IFilterProvider provider)
        {
            List<IFilterProvider> providers;
            if (provider.Type != null)
            {
                if (!providersByType.TryGetValue(provider.Type, out providers))
                {
                    providersByType.Add(provider.Type, providers = new List<IFilterProvider>());
                }
                providers.Add(provider);
            }
            else
            {
                forAllTypes.Add(provider);
            }
            if(provider is IGlobalParameterProvider)
            {
                this.AddGlobalParameterProvider((IGlobalParameterProvider)provider);
            }
        }

        public IEnumerable<IFilterProvider> GetFilterProvider(Type type)
        {
            List<IFilterProvider> providers;
            providersByType.TryGetValue(type, out providers);

            if (providers != null && forAllTypes.Count != 0) return providers.Concat(forAllTypes);
            if (providers != null) return providers;
            return forAllTypes;
        }
    }

    [Flags]
    public enum FilterType
    {
        TableScan = 0x01,
        CollectionProperty = 0x02,
        Property = 0x04,
        All = TableScan | CollectionProperty | Property
    }

    public interface IGlobalParameterProvider
    {
        IEnumerable<string> Parameter { get; }
        object GetParameter(DbContext context, string name);
        
    }

    public interface IFilterProvider
    {
        Type Type { get; }
        FilterType SupportedFilterTypes { get; }
        DbFilterExpression GetFilter(DbExpression originalExpression, DbContext context, EdmType type, FilterType filterType);
    }
    
    public static class EfExtensions
    {
        public static Type GetClrTypeFromCSpaceType(DbContext context, EdmType cType)
        {
            return GetClrTypeFromCSpaceType(((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace, cType);
        }
        public static Type GetClrTypeFromCSpaceType(MetadataWorkspace workspace, EdmType cType)
        {
            var itemCollection = (ObjectItemCollection)workspace.GetItemCollection(DataSpace.OSpace);

            if (cType is StructuralType)
            {
                var osType = workspace.GetObjectSpaceType((StructuralType)cType);
                return itemCollection.GetClrType(osType);
            }
            else if (cType is EnumType)
            {
                var osType = workspace.GetObjectSpaceType((EnumType)cType);
                return itemCollection.GetClrType(osType);
            }
            else if (cType is PrimitiveType)
            {
                return ((PrimitiveType)cType).ClrEquivalentType;
            }
            else if (cType is CollectionType)
            {
                return GetClrTypeFromCSpaceType(workspace, ((CollectionType)cType).TypeUsage.EdmType);
            }
            else if (cType is RefType)
            {
                return GetClrTypeFromCSpaceType(workspace, ((RefType)cType).ElementType);
            }
            else if (cType is EdmFunction)
            {
                return GetClrTypeFromCSpaceType(workspace, ((EdmFunction)cType).ReturnParameter.TypeUsage.EdmType);
            }
            return null;
        }
    }

}
