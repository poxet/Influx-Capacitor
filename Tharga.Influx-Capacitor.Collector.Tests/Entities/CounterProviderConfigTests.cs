using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Tests.Entities
{
    [TestFixture]
    public class CounterProviderConfigTests
    {
        [Test]
        public void Default_Configuration()
        {
            // arrange
            var name = "MyProvider";
            var typeName = "MyCounterProvider";

            // act
            var config = new CounterProviderConfig(name, typeName, null);
            
            // assert
            Assert.AreEqual(name, config.Name); 
            Assert.AreEqual(typeName, config.Type);
        }

        [Test]
        public void Should_Resolve_String()
        {
            // arrange
            var values = new Dictionary<string, string> { { "Test", "Value" }, { "Test2", "Value2" }, { "test3", "Value3" } };
            var config = new CounterProviderConfig("MyProvider", "MyConfig", values);

            // act
            var myvalue = config.GetStringValue("Test");
            var myvalue2 = config.GetStringValue("Missing", "Unknown");
            var myvalue3 = config.GetStringValue("Test3");

            // assert
            Assert.AreEqual("Value", myvalue, "Config should return the value from the corresponding dictionary entry");
            Assert.AreEqual("Unknown", myvalue2, "Config should return default value is key is missing");
            Assert.AreEqual("Value3", myvalue3, "Config should be case insensitive");
        }

        [Test]
        public void Should_Resolve_Int32()
        {
            // arrange
            var values = new Dictionary<string, string> { { "Test", "1" }, { "Test2", "0.1" }, { "Test3", " 001 " } };
            var config = new CounterProviderConfig("MyProvider", "MyConfig", values);

            // act
            var myvalue = config.GetInt32Value("Test");
            try
            {
                config.GetInt32Value("Test2");
                Assert.Fail("Should have thrown ArgumentException : 0.1 is not a valid Int32");
            }
            catch (ArgumentException)
            {
            }

            var myvalue3 = config.GetInt32Value("Test3");

            // assert
            Assert.AreEqual(1, myvalue);
            Assert.AreEqual(1, myvalue3);
        }
    }

    internal class FakeCounterProvider : IPerformanceCounterProvider
    {
        public string Name { get; private set; }

        public void Setup(ICounterProviderConfig config)
        {
        }

        public IPerformanceCounterGroup GetGroup(ICounterGroup @group)
        {
            return null;
        }

        public IEnumerable<string> GetCategoryNames()
        {
            return Enumerable.Empty<string>();
        }

        public IEnumerable<string> GetCounterNames(string categoryName, string machineName)
        {
            return Enumerable.Empty<string>();
        }

        public IEnumerable<string> GetInstances(string category, string counterName, string machineName)
        {
            return Enumerable.Empty<string>();
        }
    }
}