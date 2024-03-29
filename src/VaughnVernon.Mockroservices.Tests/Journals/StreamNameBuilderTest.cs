//   Copyright © 2020 Vaughn Vernon. All rights reserved.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using VaughnVernon.Mockroservices.Journals;

namespace VaughnVernon.Mockroservices.Tests.Journals
{
    [TestClass]
    public class StreamNameBuilderTest
    {
        [TestMethod]
        public void TestThatStreamNameBuilderBuildsCorrectStream()
        {
            Assert.AreEqual("person_1234", StreamNameBuilder.BuildStreamNameFor<Person>("1234"));
        }
        
        [TestMethod]
        public void TestThatStreamNameBuilderBuildsCorrectCategoryStream()
        {
            Assert.AreEqual("cat-person", StreamNameBuilder.BuildStreamNameFor<Person>());
        }
    }
}