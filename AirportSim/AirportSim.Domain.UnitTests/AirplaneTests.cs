using AirportSim.Domain.Interfaces;
using AirportSim.Domain.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace AirportSim.Domain.UnitTests
{
    [TestClass]
    public class AirplaneTests
    {

        [TestMethod]
        public void TestMethod1()
        {
            var stations = new List<Mock<IStation>>()
        {
            new Mock<IStation>()
        };
            stations[0].Setup(s => s.LandStations)
                .Returns(new List<IStation>());


            var mocked = new 
            stations.Setup(s=>s)
             var airplane = new Airplane(Guid.NewGuid(),"type1",Objectives.Landing,true);
            airplane.StartLanding()

        }
    }
}
