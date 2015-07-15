using System.Globalization;
using System.Reflection;
using Solr.Client.Serialization;

namespace Solr.EPiServer
{
    public class EpiSolrFieldResolver : DefaultSolrFieldResolver
    {
        private readonly string _languageName;

        public EpiSolrFieldResolver(CultureInfo cultureInfo)
        {
            _languageName = cultureInfo.TwoLetterISOLanguageName;
        }

        public override string GetFieldName(MemberInfo memberInfo)
        {
            return string.Format("{0}_txt_{1}", memberInfo.Name, _languageName);
        }
    }
}
