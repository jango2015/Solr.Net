using Microsoft.VisualStudio.TestTools.UnitTesting;
using Solr.Net.WebService;

namespace Solr.Net.Test
{
    [TestClass]
    public class UnitTest1
    {
        private SolrClient _solrClient;

        public UnitTest1()
        {
            _solrClient = new SolrClient("http://localhost:8983/solr/#/catalog");
        }

        [TestMethod]
        public void AddDocument()
        {
            new SolrRepository(_solrClient).Add(new
            {
                Id = "change.me",
                Title = "UnitTest1"
            });
        }

        [TestMethod]
        public void TestMethod1()
        {
            //new SolrRepository("").Get<UnitTest1>("Test").Execute();
            //Assert.AreEqual(10, count.Count);
        }

        [TestMethod]
        public void TestMethod2()
        {
            //new SolrRepository("")
            //    .Get<UnitTest1>("Test")
            //    .Filter(x => x.Name == "Te\"st")
            //    .Filter(x => x.Name.Equals("ASDF") && x.Name == "A")
            //    .Take(19)
            //    .Skip(5)
            //    .Execute();
            //Assert.AreEqual(10, count.Count);
        }
    }
}
