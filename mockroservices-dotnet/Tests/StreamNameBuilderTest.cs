using Microsoft.VisualStudio.TestTools.UnitTesting;
using VaughnVernon.Mockroservices.VaughnVernon.Mockroservices;

namespace VaughnVernon.Mockroservices.Tests
{
    [TestClass]
    public class StreamNameBuilderTest
    {
        [TestMethod]
        public void TestThatStreamNameBuilderBuildsCorrectStream()
        {
            Assert.AreEqual("Person_1234", StreamNameBuilder.BuildStreamNameFor<Person>("1234"));
        }
    }
}