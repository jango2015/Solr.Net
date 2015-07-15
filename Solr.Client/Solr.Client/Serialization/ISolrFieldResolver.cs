using System.Reflection;

namespace Solr.Client.Serialization
{
    public interface ISolrFieldResolver
    {
        string GetFieldName(MemberInfo memberInfo);
    }
}
