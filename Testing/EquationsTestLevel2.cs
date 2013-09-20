using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Crossflip;

namespace Testing
{
    [TestClass]
    public class EquationsTestLevel2
    {
        ByteStoredSystemOfEquations level2;

        [TestInitialize()]
        public void initialiser()
        {
            //recreate the equations system
            level2 = new ByteStoredSystemOfEquations(new Board("<script>var boardinit = \"21,10\";var level = 2;"));
        }

        [TestCleanup()]
        public void cleanup()
        {
            level2 = null;
        }

        [TestMethod]
        public void coefficient1()
        {
            Assert.AreEqual(level2.coefficient(0, 0), 1);
            Assert.AreEqual(level2.coefficient(0, 1), 0);
            Assert.AreEqual(level2.coefficient(0, 2), 1);
            Assert.AreEqual(level2.coefficient(1, 0), 0);
            Assert.AreEqual(level2.coefficient(1, 1), 1);
            Assert.AreEqual(level2.coefficient(1, 2), 1);
            Assert.AreEqual(level2.coefficient(2, 0), 1);
            Assert.AreEqual(level2.coefficient(2, 1), 1);
            Assert.AreEqual(level2.coefficient(2, 2), 1);
        }

        [TestMethod]
        public void addToTest1()
        {
            //add 1 to 0
            level2.addTo(0, 1);

            Assert.AreEqual(level2.coefficient(0, 0), 1);
            Assert.AreEqual(level2.coefficient(0, 1), 1);
            Assert.AreEqual(level2.coefficient(0, 2), 0);
            Assert.AreEqual(level2.coefficient(1, 0), 0);
            Assert.AreEqual(level2.coefficient(1, 1), 1);
            Assert.AreEqual(level2.coefficient(1, 2), 1);
            Assert.AreEqual(level2.coefficient(2, 0), 1);
            Assert.AreEqual(level2.coefficient(2, 1), 1);
            Assert.AreEqual(level2.coefficient(2, 2), 1);

            Assert.AreEqual(level2.RHS[0], 0);
            Assert.AreEqual(level2.RHS[1], 1);
            Assert.AreEqual(level2.RHS[2], 0);
        }

        [TestMethod]
        public void addToTest2()
        {
            //add 2 to 0
            level2.addTo(0, 2);

            Assert.AreEqual(level2.coefficient(0, 0), 0);
            Assert.AreEqual(level2.coefficient(0, 1), 1);
            Assert.AreEqual(level2.coefficient(0, 2), 0);
            Assert.AreEqual(level2.coefficient(1, 0), 0);
            Assert.AreEqual(level2.coefficient(1, 1), 1);
            Assert.AreEqual(level2.coefficient(1, 2), 1);
            Assert.AreEqual(level2.coefficient(2, 0), 1);
            Assert.AreEqual(level2.coefficient(2, 1), 1);
            Assert.AreEqual(level2.coefficient(2, 2), 1);

            Assert.AreEqual(level2.RHS[0], 1);
            Assert.AreEqual(level2.RHS[1], 1);
            Assert.AreEqual(level2.RHS[2], 0);
        }

        [TestMethod]
        public void addToTest3()
        {
            //add 0 to 1
            level2.addTo(1, 0);

            Assert.AreEqual(level2.coefficient(0, 0), 1);
            Assert.AreEqual(level2.coefficient(0, 1), 0);
            Assert.AreEqual(level2.coefficient(0, 2), 1);
            Assert.AreEqual(level2.coefficient(1, 0), 1);
            Assert.AreEqual(level2.coefficient(1, 1), 1);
            Assert.AreEqual(level2.coefficient(1, 2), 0);
            Assert.AreEqual(level2.coefficient(2, 0), 1);
            Assert.AreEqual(level2.coefficient(2, 1), 1);
            Assert.AreEqual(level2.coefficient(2, 2), 1);

            Assert.AreEqual(level2.RHS[0], 1);
            Assert.AreEqual(level2.RHS[1], 0);
            Assert.AreEqual(level2.RHS[2], 0);
        }

        [TestMethod]
        public void gaussianEliminate1()
        {
            level2.gaussianEliminate();

            Assert.AreEqual(level2.coefficient(0, 0), 1);
            Assert.AreEqual(level2.coefficient(0, 1), 1);
            Assert.AreEqual(level2.coefficient(0, 2), 1);
            Assert.AreEqual(level2.coefficient(1, 0), 0);
            Assert.AreEqual(level2.coefficient(1, 1), 1);
            Assert.AreEqual(level2.coefficient(1, 2), 0);
            Assert.AreEqual(level2.coefficient(2, 0), 0);
            Assert.AreEqual(level2.coefficient(2, 1), 0);
            Assert.AreEqual(level2.coefficient(2, 2), 1);

            Assert.AreEqual(level2.RHS[0], 0);
            Assert.AreEqual(level2.RHS[1], 1);
            Assert.AreEqual(level2.RHS[2], 0);
        }

    }
}
