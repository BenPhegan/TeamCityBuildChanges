using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace TeamCityBuildChanges.Tests
{
    [TestFixture]
    public class ChangeManifestTests
    {
        public void CheckFlatteningOfIssues()
        {
            var manifest = TestHelpers.CreateChangeManifest();
            Assert.AreEqual(5, manifest.FlattenedIssueDetails.Count);
            Assert.AreEqual(1, manifest.FlattenedIssueDetails[0].SubIssues.Count);
            Assert.AreEqual(1, manifest.FlattenedIssueDetails[1].SubIssues.Count);
            Assert.AreEqual(manifest.FlattenedIssueDetails[0].SubIssues[0], manifest.FlattenedIssueDetails[1].SubIssues[0]);
            Assert.AreEqual(null, manifest.FlattenedIssueDetails[2].SubIssues);
        }
    }
}
