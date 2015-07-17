using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Solr.Client.Serialization;
using Solr.Client.WebService;

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
            var configuration = new DefaultSolrConfiguration("http://localhost:8983/solr/test");
            _repository = new DefaultSolrRepository(configuration, new CustomSolrFieldResolver());
        }

        [TestMethod]
        public async Task AddDocument()
        {
            await _repository.Add(new TestDocument1
            {
                Id = "test",
                Title = "UnitTest1"
            });
            await _repository.Add(new TestDocument2
            {
                Id = "test",
                Title = "UnitTest1",
                Test = "Dette er en test-tekst."
            });
        }

        [TestMethod]
        public async Task RemoveDocument()
        {
            // ensure deleted
            await _repository.Remove("test1");
            // verify
            var r1 = await _repository.Get(new SolrQuery<TestDocument1>(x => x.Id == "test1"));
            Assert.AreEqual(0, r1.Response.Documents.Count());
            // add
            await _repository.Add(new TestDocument1
            {
                Id = "test1",
                Title = "UnitTest2",
                CreatedAt = DateTime.Now
            });
            // verify
            var r2 = await _repository.Get(new SolrQuery<TestDocument1>(x => x.Id == "test1"));
            Assert.AreEqual(1, r2.Response.Documents.Count());
            // delete
            await _repository.Remove("test1");
            // verify
            var r3 = await _repository.Get(new SolrQuery<TestDocument1>(x => x.Id == "test1"));
            Assert.AreEqual(0, r3.Response.Documents.Count());
        }

        [TestMethod]
        public async Task TestMethod1()
        {
            var r1 =
                await
                    _repository.Get(new SolrQuery<TestDocument1>("UnitTest1")
                        .Facet("testFacet1", new SolrQueryStatisticsFacet<TestDocument1>("unique(id)"))
                        .Facet("testFacet2", new SolrQueryTermsFacet<TestDocument1>(x => x.Id)
                            .Facet("subFacet1", new SolrQueryStatisticsFacet<TestDocument1>("unique(id)"))));
            Assert.AreEqual(1, r1.Response.Documents.Count());
            var r2 = await _repository.Get(new SolrQuery<TestDocument1>("Hest"));
            Assert.AreEqual(0, r2.Response.Documents.Count());
            var r3 = await _repository.Get(new SolrQuery<TestDocument1>("*", "lucene").Filter(x => x.Title.Contains("UnitTest1")));
            Assert.AreEqual(1, r3.Response.Documents.Count());
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

        public class TestDocument1
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class TestDocument2 : TestDocument1
        {
            public string Test { get; set; }
        }
    }

    public class CustomSolrFieldResolver : DefaultSolrFieldResolver
    {
        public override string GetFieldName(MemberInfo memberInfo)
        {
            if (memberInfo.Name == "Id")
            {
                return "id";
            }
            var memberType = GetMemberType(memberInfo);
            if (memberType == typeof(string))
            {
                return string.Format("{0}_txt_da", memberInfo.Name);
            }
            else if (memberType == typeof(DateTime))
            {
                return string.Format("{0}_dt", memberInfo.Name);
            }
            return memberInfo.Name.ToLowerInvariant();
        }
    }
}
