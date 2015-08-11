using System;
using System.Linq;
using System.Threading.Tasks;
using log4net.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Solr.Client.Linq;

namespace Solr.Client.Test
{
    [TestClass]
    public class IntegrationTest : IDisposable
    {
        private readonly SolrRepository _repository;

        public IntegrationTest()
        {
            // load log4net
            BasicConfigurator.Configure();
            // load solr
            var configuration = new DefaultSolrConfiguration("http://localhost:8983/solr/techproducts");
            _repository = new SolrRepository(configuration, new TechProductFieldResolver());
        }
        
        [TestMethod]
        public async Task RemoveDocument()
        {
            // ensure deleted
            await _repository.Remove("test1");
            // verify
            var r1 = await _repository.Search(new SolrQuery<TechProduct>().SearchFor(x => x.Id == "test1"));
            Assert.AreEqual(0, r1.NumFound);
            // add
            var lastModified = DateTime.Now.AddDays(-1);
            await _repository.Add(new TechProduct
            {
                Id = "test1",
                Title = new[]{"UnitTest2"},
                LastModified = lastModified
            });
            // verify
            var r2 = await _repository.Search(new SolrQuery<TechProduct>().SearchFor(x => x.Id == "test1"));
            Assert.AreEqual(1, r2.NumFound);
            var readLastModified = r2.Documents.First().LastModified;
            Assert.IsNotNull(readLastModified);
            // ignore fragments of a second with tostring
            Assert.AreEqual(lastModified.ToString("R"), readLastModified.Value.ToString("R"));
            // delete
            await _repository.Remove("test1");
            // verify
            var r3 = await _repository.Search(new SolrQuery<TechProduct>().SearchFor(x => x.Id == "test1").Take(10));
            Assert.AreEqual(0, r3.NumFound);
        }

        [TestMethod]
        public async Task Filter1()
        {
            var r3 = await _repository.Search(new SolrQuery<TechProduct>().SearchFor("*", "lucene").Filter(x => x.Title.Contains("volapyk")));
            Assert.AreEqual(0, r3.NumFound);
        }

        [TestMethod]
        public async Task Facet1()
        {
            var termsFacetFor = new SolrQuery<TechProduct>().SearchFor("*", "lucene").Take(0).TermsFacetFor(x => x.Category);
            termsFacetFor = termsFacetFor.RangeFacetFor(x => x.Popularity, x => x.Range(5, 10, "1"));
            termsFacetFor = termsFacetFor.RangeFacetFor(x => x.ManufactureDate,
                x =>
                    x.Range(DateTime.Now.AddYears(-20), DateTime.Now, "+1YEAR")
                        .TermsFacetFor(y => y.Category,
                            y => y.RangeFacetFor(z => z.Popularity, z => z.Range(5, 10, "1"))));
            var searchResult = await _repository.Search(termsFacetFor);
            var bucket = searchResult.Raw.Facets["R2"]["buckets"].ToList()[10]["T0"]["buckets"].ToList()[0]["R0"]["buckets"].ToList()[1];
            Assert.AreEqual(6, bucket["val"]);
            Assert.AreEqual(2, bucket["count"]);
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

        public void Dispose()
        {
            _repository.Dispose();
        }
    }
}
