using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EclipseAutomation.Tasks
{
    [TestClass]
    public class PhotoSettingsStackFactoryTests
    {
        [TestMethod]
        public void BasicStackTest()
        {
            // PhotoSettings doesn't care, so using garbage values is a more robust test
            PhotoSettings defaultSettings = new PhotoSettings()
            {
                IsoNumber = "iso",
                ShutterSpeed = "speed",
                FNumber = "big",
                ExposureCompensation = "none"
            };
            var factory = new PhotoSettingsStackFactory(defaultSettings);

            var results = factory.GetShutterStack("bob", "fred", "joe");
            Assert.AreEqual(3, results.Count);

            Assert.IsTrue(results.All(r => r.IsoNumber == defaultSettings.IsoNumber), "bad IsoNumber");
            Assert.IsTrue(results.All(r => r.FNumber == defaultSettings.FNumber), "bad FNumber");
            Assert.IsTrue(results.All(r => r.ExposureCompensation == defaultSettings.ExposureCompensation), "bad ExposureCompensation");

            Assert.AreEqual("bob", results[0].ShutterSpeed, "bad shutter for [0]");
            Assert.AreEqual("fred", results[1].ShutterSpeed, "bad shutter for [1]");
            Assert.AreEqual("joe", results[2].ShutterSpeed, "bad shutter for [2]");
        }
    }
}
