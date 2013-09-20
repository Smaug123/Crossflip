using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Crossflip;

namespace Testing
{
    [TestClass]
    public class PointTest1
    {
        Board level0;
        Point p;

        [TestInitialize()]
        public void initialiser()
        {
            //recreate the equations system
            level0 = new Board("<script>var boardinit =  \"11,02\";var level = 0;");
            p = new Point(0, level0);
        }

        [TestCleanup()]
        public void cleanup()
        {
            level0 = null;
            p = null;
        }

        [TestMethod]
        public void pointToZeroIndexedLocation1()
        {
            p = new Point(0, level0);
            Assert.AreEqual(p.ToZeroIndexedPosition(), 0);
            Assert.AreEqual(p.firstDimension, 0);
            Assert.AreEqual(p.secondDimension, 0);
        }

        [TestMethod]
        public void pointToZeroIndexedLocation2()
        {
            p = new Point(1, level0);
            Assert.AreEqual(p.ToZeroIndexedPosition(), 1);
            Assert.AreEqual(p.firstDimension, 0);
            Assert.AreEqual(p.secondDimension, 1);
        }

        [TestMethod]
        public void pointToZeroIndexedLocation3()
        {
            p = new Point(2, level0);
            Assert.AreEqual(p.ToZeroIndexedPosition(), 2);
            Assert.AreEqual(p.firstDimension, 1);
            Assert.AreEqual(p.secondDimension, 0);
        }

        [TestMethod]
        public void pointToZeroIndexedLocation4()
        {
            p = new Point(3, level0);
            Assert.AreEqual(p.ToZeroIndexedPosition(), 3);
            Assert.AreEqual(p.firstDimension, 1);
            Assert.AreEqual(p.secondDimension, 1);
        }

        [TestMethod]
        public void pointToZeroIndexedLocationFromXy1()
        {
            p = new Point(0,0, level0);
            Assert.AreEqual(p.ToZeroIndexedPosition(), 0);
            Assert.AreEqual(p.firstDimension, 0);
            Assert.AreEqual(p.secondDimension, 0);
        }

        [TestMethod]
        public void pointToZeroIndexedLocationFromXy2()
        {
            p = new Point(0,1, level0);
            Assert.AreEqual(p.ToZeroIndexedPosition(), 1);
            Assert.AreEqual(p.firstDimension, 0);
            Assert.AreEqual(p.secondDimension, 1);
        }

        [TestMethod]
        public void pointToZeroIndexedLocationFromXy3()
        {
            p = new Point(1,0, level0);
            Assert.AreEqual(p.ToZeroIndexedPosition(), 2);
            Assert.AreEqual(p.firstDimension, 1);
            Assert.AreEqual(p.secondDimension, 0);
        }

        [TestMethod]
        public void pointToZeroIndexedLocationFromXy4()
        {
            p = new Point(1,1, level0);
            Assert.AreEqual(p.ToZeroIndexedPosition(), 3);
            Assert.AreEqual(p.firstDimension, 1);
            Assert.AreEqual(p.secondDimension, 1);
        }
    }
}
