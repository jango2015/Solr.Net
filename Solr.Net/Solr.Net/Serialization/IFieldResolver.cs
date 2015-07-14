using System;
using System.Reflection;

namespace Solr.Net.Serialization
{
    public interface IFieldResolver
    {
        string GetFieldName(MemberInfo memberInfo);
    }
}
