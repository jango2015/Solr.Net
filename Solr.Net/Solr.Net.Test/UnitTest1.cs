using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Solr.Net.Test
{
    [TestClass]
    public class UnitTest1
    {
        public string Name { get; set; }

        [TestMethod]
        public void TestMethod1()
        {
            var count = new SolrRepository("").Get<UnitTest1>().ToList();
            Assert.AreEqual(10, count.Count);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var count = new SolrRepository("").Get<UnitTest1>().Where(x => x.Name == "Test").ToList();
            Assert.AreEqual(0, count.Count);
        }
    }
}
