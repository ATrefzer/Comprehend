using System.IO;

using GraphLibrary.PlantUml;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class PlantUmlBuilderTests
    {
        [TestMethod]
        public void SplitName_ModuleAvailable()
        {
            var plantuml = new PlantUmlBuilder();
            var parts = plantuml.SplitFullName("module!namespace.class.function");
            Assert.AreEqual("function", parts.Function);
            Assert.AreEqual("module", parts.Module);
            Assert.AreEqual("namespace.class", parts.TypeName);
        }

        [TestMethod]
        public void SplitName_ModuleNotAvailable()
        {
            var plantuml = new PlantUmlBuilder();
            var parts = plantuml.SplitFullName("namespace.class.function");
            Assert.AreEqual("function", parts.Function);
            Assert.AreEqual("unknown", parts.Module);
            Assert.AreEqual("namespace.class", parts.TypeName);
        }


        [TestMethod]
        public void WriteOutput()
        {
            var ms = new MemoryStream();

            var plantuml = new PlantUmlBuilder();
            plantuml.AddEdge("m1!ns.cls.func", "m2.ns2.cls2.func2");
            plantuml.AddEdge("m2.ns2.cls2.func2", "m3.ns3.cls3.func3");
            plantuml.AddEdge("m3.ns3.cls3.func3", "m1!ns.cls.func");

            plantuml.WriteOutput("unit_test_output.plantuml");
        }
    }
}