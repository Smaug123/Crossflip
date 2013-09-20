using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Crossflip;

namespace Testing
{
    [TestClass]
    public class PointTest2
    {
        Board level3;
        Point p;

        [TestInitialize()]
        public void initialiser()
        {
            //recreate the equations system
            level3 = new Board("<script>var boardinit =  \"001,111\";var level = 0;");
            p = new Point(0, level3);
        }

        [TestCleanup()]
        public void cleanup()
        {
            level3 = null;
            p = null;
        }

        [TestMethod]
        public void pointToZeroIndexedLocation1()
        {
            p = new Point(0, level3);
            Assert.AreEqual(p.ToZeroIndexedPosition(), 0);
            Assert.AreEqual(p.firstDimension, 0);
            Assert.AreEqual(p.secondDimension, 0);
        }

        [TestMethod]
        public void pointToZeroIndexedLocation2()
        {
            p = new Point(1, level3);
            Assert.AreEqual(p.ToZeroIndexedPosition(), 1);
            Assert.AreEqual(p.firstDimension, 0);
            Assert.AreEqual(p.secondDimension, 1);
        }

        [TestMethod]
        public void pointToZeroIndexedLocation3()
        {
            p = new Point(2, level3);
            Assert.AreEqual(p.ToZeroIndexedPosition(), 2);
            Assert.AreEqual(p.firstDimension, 0);
            Assert.AreEqual(p.secondDimension, 2);
        }

        [TestMethod]
        public void pointToZeroIndexedLocation4()
        {
            p = new Point(3, level3);
            Assert.AreEqual(p.ToZeroIndexedPosition(), 3);
            Assert.AreEqual(p.firstDimension, 1);
            Assert.AreEqual(p.secondDimension, 0);
        }

        [TestMethod]
        public void pointToZeroIndexedLocation5()
        {
            p = new Point(4, level3);
            Assert.AreEqual(p.ToZeroIndexedPosition(), 4);
            Assert.AreEqual(p.firstDimension, 1);
            Assert.AreEqual(p.secondDimension, 1);
        }

        [TestMethod]
        public void pointToZeroIndexedLocation6()
        {
            p = new Point(5, level3);
            Assert.AreEqual(p.ToZeroIndexedPosition(), 5);
            Assert.AreEqual(p.firstDimension, 1);
            Assert.AreEqual(p.secondDimension, 2);
        }

        [TestMethod]
        public void pointToZeroIndexedLocationXy1()
        {
            p = new Point(0,0, level3);
            Assert.AreEqual(p.ToZeroIndexedPosition(), 0);
            Assert.AreEqual(p.firstDimension, 0);
            Assert.AreEqual(p.secondDimension, 0);
        }

        [TestMethod]
        public void pointToZeroIndexedLocationXy2()
        {
            p = new Point(0, 1, level3);
            Assert.AreEqual(p.ToZeroIndexedPosition(), 1);
            Assert.AreEqual(p.firstDimension, 0);
            Assert.AreEqual(p.secondDimension, 1);
        }

        [TestMethod]
        public void pointToZeroIndexedLocationXy3()
        {
            p = new Point(0, 2, level3);
            Assert.AreEqual(p.ToZeroIndexedPosition(), 2);
            Assert.AreEqual(p.firstDimension, 0);
            Assert.AreEqual(p.secondDimension, 2);
        }

        [TestMethod]
        public void pointToZeroIndexedLocationXy4()
        {
            p = new Point(1, 0, level3);
            Assert.AreEqual(p.ToZeroIndexedPosition(), 3);
            Assert.AreEqual(p.firstDimension, 1);
            Assert.AreEqual(p.secondDimension, 0);
        }

        [TestMethod]
        public void pointToZeroIndexedLocationXy5()
        {
            p = new Point(1, 1, level3);
            Assert.AreEqual(p.ToZeroIndexedPosition(), 4);
            Assert.AreEqual(p.firstDimension, 1);
            Assert.AreEqual(p.secondDimension, 1);
        }

        [TestMethod]
        public void pointToZeroIndexedLocationXy6()
        {
            p = new Point(1, 2, level3);
            Assert.AreEqual(p.ToZeroIndexedPosition(), 5);
            Assert.AreEqual(p.firstDimension, 1);
            Assert.AreEqual(p.secondDimension, 2);
        }
    }
}
