using System;
using System.Globalization;
using System.Reflection;
using Solr.Client.Serialization;

namespace Solr.EPiServer
{
    public class EpiSolrFieldResolver : DefaultSolrFieldResolver
    {
        private readonly string _languageName;
        private readonly string _cultureName;

        public EpiSolrFieldResolver(CultureInfo cultureInfo)
        {
            _cultureName = cultureInfo.Name;
            _languageName = cultureInfo.TwoLetterISOLanguageName;
        }

        public override string GetFieldName(MemberInfo memberInfo)
        {
            var memberType = GetMemberType(memberInfo);
            return GetFieldName(memberInfo.Name, memberType);
        }

        public string GetFieldName(string memberName, Type memberType)
        {
            string suffix;
            if (memberType == typeof (DateTime) || memberType == typeof (DateTime?)) suffix = "dt";
            else suffix = string.Format("txt_{0}", _languageName);
            return string.Format("{0}_{1}_{2}", memberName, _cultureName, suffix);
        }

        public string GetDefaultFieldName()
        {
            return string.Format("_default_{0}_txts_{1}", _cultureName, _languageName);
        }
    }
}
