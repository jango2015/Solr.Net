using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Solr.Client.Serialization;

namespace Solr.Client.Test
{
    [TestClass]
    public class IntegrationTest
    {
        private readonly SolrRepository _repository;

        public IntegrationTest()
        {
            var configuration = new DefaultSolrConfiguration("http://localhost:8983/solr/test")
            {
                FieldResolver = new CustomFieldResolver()
            };
            _repository = new SolrRepository(configuration);
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
        public async Task TestMethod1()
        {
            var r1 = await _repository.Get<TestDocument1>("Test").Execute();
            Assert.AreEqual(1, r1.Response.Documents.Count());
            var r2 = await _repository.Get<TestDocument1>("Hest").Execute();
            Assert.AreEqual(0, r2.Response.Documents.Count());
            var r3 =
                await
                    _repository
                        .Get<TestDocument1>("*")
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

        public class TestDocument1
        {
            public string Id { get; set; }
            public string Title { get; set; }
        }

        public class TestDocument2 : TestDocument1
        {
            public string Test { get; set; }
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
