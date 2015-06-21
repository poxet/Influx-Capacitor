using System;
using System.Collections.Generic;
using System.Linq;
using InfluxDB.Net.Collector.Business;
using InfluxDB.Net.Collector.Entities;
using InfluxDB.Net.Collector.Interface;
using Moq;
using NUnit.Framework;

namespace InfluxDB.Net.Collector.Tests.Business.CounterBusinessTests
{
    [TestFixture]
    public class CounterTests
    {
        [Test]
        public void Should_throw_if_no_groups()
        {
            //Arrange
            var counterBusiness = new CounterBusiness();
            var config = Mock.Of<IConfig>();
            Exception exception = null;
            List<PerformanceCounterGroup> result = null;

            //Act
            try
            {
                result = counterBusiness.GetPerformanceCounterGroups(config);
            }
             catch (Exception exp)
             {
                 exception = exp;
             }

            //Assert
            Assert.That(result, Is.Null);
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Is.EqualTo("No groups in config."));
        }

        [Test]
        public void Should_return_a_result()
        {
            //Arrange
            var counterBusiness = new CounterBusiness();
            var config = Mock.Of<IConfig>(x => x.Groups == Mocks.Of<ICounterGroup>(y => y.Name == "A" && y.SecondsInterval == 10 && y.Counters == Mocks.Of<ICounter>(z => z.CategoryName == "Processor" && z.CounterName == "% Processor Time" && z.InstanceName == "_Total").Take(2).ToList()).Take(2).ToList());
            Exception exception = null;
            List<PerformanceCounterGroup> result = null;

            //Act
            try
            {
                result = counterBusiness.GetPerformanceCounterGroups(config);
            }
            catch (Exception exp)
            {
                exception = exp;
            }

            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(exception, Is.Null);
            Assert.That(result.Count, Is.EqualTo(2));
        }
    }
}