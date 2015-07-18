using System.Reflection;
using Solr.Client.Serialization;

namespace Solr.Client.Test
{
    public class TechProductFieldResolver : DefaultSolrFieldResolver
    {
        public override string GetFieldName(MemberInfo memberInfo)
        {
            switch (memberInfo.Name)
            {
                case "Category":
                    return "cat";
                case "ContentType":
                    return "content_type";
                case "LastModified":
                    return "last_modified";
                case "Source":
                    return "_src_";
            }
            return memberInfo.Name.ToLowerInvariant();
        }
    }
}