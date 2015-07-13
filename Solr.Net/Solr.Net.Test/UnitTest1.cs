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
            new SolrRepository("").Get<UnitTest1>("Test").Execute();
            //Assert.AreEqual(10, count.Count);
        }

        [TestMethod]
        public void TestMethod2()
        {
            new SolrRepository("")
                .Get<UnitTest1>("Test")
                .Filter(x => x.Name == "Te\"st")
                .Filter(x => x.Name.Equals("ASDF") && x.Name == "A")
                .Take(19)
                .Skip(5)
                .Execute();
            //Assert.AreEqual(10, count.Count);
        }
    }
}
