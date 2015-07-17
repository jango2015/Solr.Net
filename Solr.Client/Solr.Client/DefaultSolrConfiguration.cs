using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;

namespace Solr.Client
{
    public class DefaultSolrConfiguration : ISolrConfiguration
    {
        private const string SettingCollectionUrl = "solr:collectionUrl";
        private const string SettingCollectionUpdateUrl = "solr:collectionUpdateUrl";
        private const string SettingCollectionQueryUrl = "solr:collectionQueryUrl";

        public DefaultSolrConfiguration(string collectionUrl)
        {
            var appSettings = ConfigurationManager.AppSettings;
            SetDefaultRequestUrls(appSettings, collectionUrl);
        }

        public DefaultSolrConfiguration()
        {
            var appSettings = ConfigurationManager.AppSettings;
            var collectionUrl = GetValueFromAppSettings(appSettings, SettingCollectionUrl, null);
            SetDefaultRequestUrls(appSettings, collectionUrl);
        }

        private void SetDefaultRequestUrls(NameValueCollection appSettings, string collectionUrl)
        {
            collectionUrl = string.IsNullOrWhiteSpace(collectionUrl) ? null : collectionUrl.TrimEnd('/');
            var defaultUpdateUrl = collectionUrl == null ? null : string.Format("{0}/update", collectionUrl);
            var defaultQueryUrl = collectionUrl == null ? null : string.Format("{0}/query", collectionUrl);
            UpdateUrl = GetValueFromAppSettings(appSettings, SettingCollectionUpdateUrl, defaultUpdateUrl);
            QueryUrl = GetValueFromAppSettings(appSettings, SettingCollectionQueryUrl, defaultQueryUrl);
            if (UpdateUrl == null || QueryUrl == null)
            {
                throw new TypeInitializationException(GetType().FullName,
                    new ArgumentNullException(SettingCollectionUpdateUrl,
                        string.Format("The app setting {0} should be configured to the base url of a solr core/collection, e.g.: http://localhost:8983/solr/collectionName, which supports both the /update and /query request handlers. Alternatively, explicit urls can be set for query and update handlers with the app settings {1} and {2}, respectively", SettingCollectionUrl, SettingCollectionQueryUrl, SettingCollectionUpdateUrl)));
            }
        }

        private static string GetValueFromAppSettings(NameValueCollection appSettings, string key, string defaultValue)
        {
            var values = appSettings.GetValues(key);
            if (values == null) return defaultValue;
            return values.FirstOrDefault() ?? defaultValue;
        }

        public string UpdateUrl { get; set; }
        public string QueryUrl { get; set; }
    }
}
