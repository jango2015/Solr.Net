using System.Linq;
using System.Threading.Tasks;
using log4net.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Solr.Client.Linq;

namespace Solr.Client.Test
{
    [TestClass]
    public class IntegrationTest
    {
        private readonly DefaultSolrRepository _repository;

        public IntegrationTest()
        {
            // load log4net
            BasicConfigurator.Configure();
            // load solr
            var configuration = new DefaultSolrConfiguration("http://localhost:8983/solr/techproducts");
            _repository = new DefaultSolrRepository(configuration, new TechProductFieldResolver());
        }
        
        [TestMethod]
        public async Task RemoveDocument()
        {
            // ensure deleted
            await _repository.Remove("test1");
            // verify
            var r1 = _repository.Search<TechProduct>().For(x => x.Id == "test1").ToList();
            Assert.AreEqual(0, r1.Count());
            // add
            await _repository.Add(new TechProduct
            {
                Id = "test1",
                Title = new[]{"UnitTest2"}
            });
            // verify
            var r2 = _repository.Search<TechProduct>().For(x => x.Id == "test1").ToList();
            Assert.AreEqual(1, r2.Count());
            // delete
            await _repository.Remove("test1");
            // verify
            var r3 = _repository.Search<TechProduct>().For(x => x.Id == "test1").Take(10).ToList();
            Assert.AreEqual(0, r3.Count());
        }

        [TestMethod]
        public async Task Filter1()
        {
            var r3 = _repository.Search<TechProduct>().For("*", "lucene").Filter(x => x.Title.Contains("volapyk"));
            Assert.AreEqual(0, r3.ToList().Count());
        }

        [TestMethod]
        public async Task Facet1()
        {
            //var facetC = new SolrQueryStatisticsFacet<TechProduct>("unique(cat)");
            //var facetA = new SolrQueryTermsFacet<TechProduct>(x => x.Category).Facet("D", facetC);
            //var facetB = new SolrQueryTermsFacet<TechProduct>(x => x.LastModified).Facet("D", facetC);
            //var query = new SolrQuery<TechProduct>().Take(0).Facet("A", facetA).Facet("B", facetB).Facet("C", facetC);
            //var r3 = await _repository.Get(query);
            //Assert.AreEqual(0, r3.Response.Documents.Count());
            //var facets = r3.GetFacets();
            //Assert.AreEqual(37, (long)r3.Facets["count"]);
            //var facetObject = (JObject)r3.Facets["A"];
            //var dic = facetObject.ToObject<IDictionary>();
            //Assert.AreNotEqual(null, dic);
        }

    }
}
