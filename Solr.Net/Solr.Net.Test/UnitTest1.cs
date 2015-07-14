using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Solr.Net.Serialization;
using Solr.Net.WebService;

namespace Solr.Net.Test
{
    [TestClass]
    public class UnitTest1
    {
        private readonly SolrClient _solrClient;

        public UnitTest1()
        {
            _solrClient = new SolrClient("http://localhost:8983/solr/test");
            _solrClient.FieldResolver = new CustomFieldResolver();
        }

        [TestMethod]
        public async Task AddDocument()
        {
            await new SolrRepository(_solrClient).Add(new TestDocument
            {
                Id = "test",
                Title = "UnitTest1"
            });
        }

        [TestMethod]
        public async Task TestMethod1()
        {
            var r1 = await new SolrRepository(_solrClient).Get<TestDocument>("Test").Execute();
            Assert.AreEqual(1, r1.Response.Documents.Count());
            var r2 = await new SolrRepository(_solrClient).Get<TestDocument>("Hest").Execute();
            Assert.AreEqual(0, r2.Response.Documents.Count());
            var r3 =
                await
                    new SolrRepository(_solrClient)
                        .Get<TestDocument>("*")
                        .Filter(x => x.Title.Contains("UnitTest1"))
                        .Execute();
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

        public class TestDocument
        {
            public string Id { get; set; }
            public string Title { get; set; }
        }
    }

    public class CustomFieldResolver : DefaultFieldResolver
    {
        public override string GetFieldName(MemberInfo memberInfo)
        {
            if (memberInfo.Name == "Id")
            {
                return "id";
            }
            var memberType = GetMemberType(memberInfo);
            if (memberType == typeof (string))
            {
                return string.Format("{0}_txt_da", memberInfo.Name);
            }
            return memberInfo.Name.ToLowerInvariant();
        }
    }
}
