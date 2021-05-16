using Launcher.Profiler;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class PlantUmlBuilderTests
    {
        [TestMethod]
        public void SplitName_ModuleAvailable()
        {
            var parts = new FunctionInfo(0, "module!namespace.class.function", true, false);
            Assert.AreEqual("function", parts.Function);
            Assert.AreEqual("module", parts.Module);
            Assert.AreEqual("namespace.class", parts.TypeName);
        }

        [TestMethod]
        public void SplitName_ModuleNotAvailable()
        {
            var parts = new FunctionInfo(0, "namespace.class.function", true, false);
            Assert.AreEqual("function", parts.Function);
            Assert.AreEqual("unknown", parts.Module);
            Assert.AreEqual("namespace.class", parts.TypeName);
        }

    }
}