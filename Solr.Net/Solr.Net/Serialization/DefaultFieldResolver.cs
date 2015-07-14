using System;
using System.Reflection;

namespace Solr.Net.Serialization
{
    public class DefaultFieldResolver : IFieldResolver
    {
        public virtual string GetFieldName(MemberInfo memberInfo)
        {
            return memberInfo.Name;
        }
        
        protected static Type GetMemberType(MemberInfo memberInfo)
        {
            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null) return propertyInfo.PropertyType;
            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null) return fieldInfo.FieldType;
            return null;
        }
    }
}