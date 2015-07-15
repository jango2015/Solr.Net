using System.Reflection;

namespace Solr.Client.Serialization
{
    public interface IFieldResolver
    {
        string GetFieldName(MemberInfo memberInfo);
    }
}
