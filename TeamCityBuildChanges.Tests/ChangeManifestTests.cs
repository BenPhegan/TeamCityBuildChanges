using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TeamCityBuildChanges.Testing;

namespace TeamCityBuildChanges.Tests
{
    [TestFixture]
    public class ChangeManifestTests
    {
        [Test]
        public void CheckFlatteningOfIssues()
        {
            var manifest = TestHelpers.CreateSimpleChangeManifest();
            var list = manifest.FlattenedIssueDetails;
            

            Assert.AreEqual(5, list.Count);
            //Assert.AreEqual(1, list[0].SubIssues.Count);
            //Assert.AreEqual(1, list[1].SubIssues.Count);
            //Assert.AreEqual(list[0].SubIssues[0], list[1].SubIssues[0]);
            //Assert.AreEqual(null, list[2].SubIssues);
        }

        [Test]
        public void CheckConsolidated()
        {
            var manifest = TestHelpers.CreateSimpleChangeManifest();
            var list = manifest.ConsolidatedIssueDetails;
            
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0].SubIssues.Count, Is.EqualTo(2));
            Assert.That(list[1].SubIssues.Count, Is.EqualTo(1));
        }
    }
}
