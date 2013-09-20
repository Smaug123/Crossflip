using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Crossflip;

namespace Testing
{
    [TestClass]
    public class EquationsTestLevel0
    {
        ByteStoredSystemOfEquations level0;

        [TestInitialize()]
        public void initialiser()
        {
            //recreate the equations system
            level0 = new ByteStoredSystemOfEquations(new Board("<script>var boardinit = \"11,02\";var level = 0;"));

        }

        [TestCleanup()]
        public void cleanup()
        {
            level0 = null;
        }

        [TestMethod]
        public void addToTest1()
        {
            level0.addTo(0,1);
            //row 0 is changed...
            Assert.AreEqual(level0.nthRowOfEquations(0)[0], 0);
            Assert.AreEqual(level0.nthRowOfEquations(0)[1], 0);
            Assert.AreEqual(level0.nthRowOfEquations(0)[2], 1);

            Assert.AreEqual(level0.RHS[0], 0);

            //and didn't change the other row
            Assert.AreEqual(level0.nthRowOfEquations(1)[0], 1);
            Assert.AreEqual(level0.nthRowOfEquations(1)[1], 1);
            Assert.AreEqual(level0.nthRowOfEquations(1)[2], 0);

            Assert.AreEqual(level0.RHS[1], 1);
        }

        [TestMethod]
        public void addToTest2()
        {
            level0.addTo(0, 2);
            //row 0 is changed...
            Assert.AreEqual(level0.nthRowOfEquations(0)[0], 0);
            Assert.AreEqual(level0.nthRowOfEquations(0)[1], 1);
            Assert.AreEqual(level0.nthRowOfEquations(0)[2], 0);

            Assert.AreEqual(level0.RHS[2], 0);
            
            //and didn't change the other row
            Assert.AreEqual(level0.nthRowOfEquations(2)[0], 1);
            Assert.AreEqual(level0.nthRowOfEquations(2)[1], 0);
            Assert.AreEqual(level0.nthRowOfEquations(2)[2], 1);

            Assert.AreEqual(level0.RHS[2], 0);
        }

        [TestMethod]
        public void addToTest3()
        {
            level0.addTo(2, 1);
            //row 0 is unchanged...
            Assert.AreEqual(level0.nthRowOfEquations(0)[0], 1);
            Assert.AreEqual(level0.nthRowOfEquations(0)[1], 1);
            Assert.AreEqual(level0.nthRowOfEquations(0)[2], 1);

            Assert.AreEqual(level0.RHS[0], 1);

            //as is 1...
            Assert.AreEqual(level0.nthRowOfEquations(1)[0], 1);
            Assert.AreEqual(level0.nthRowOfEquations(1)[1], 1);
            Assert.AreEqual(level0.nthRowOfEquations(1)[2], 0);

            Assert.AreEqual(level0.RHS[1], 1);

            //but 2 is not
            Assert.AreEqual(level0.nthRowOfEquations(2)[0], 0);
            Assert.AreEqual(level0.nthRowOfEquations(2)[1], 1);
            Assert.AreEqual(level0.nthRowOfEquations(2)[2], 1);

            Assert.AreEqual(level0.RHS[0], 1);
        }

        [TestMethod]
        public void coefficient1()
        {
            Assert.AreEqual(level0.coefficient(0, 0), 1);
            Assert.AreEqual(level0.coefficient(0, 1), 1);
            Assert.AreEqual(level0.coefficient(0, 2), 1);
            Assert.AreEqual(level0.coefficient(1, 0), 1);
            Assert.AreEqual(level0.coefficient(1, 1), 1);
            Assert.AreEqual(level0.coefficient(1, 2), 0);
            Assert.AreEqual(level0.coefficient(2, 0), 1);
            Assert.AreEqual(level0.coefficient(2, 1), 0);
            Assert.AreEqual(level0.coefficient(2, 2), 1);
        }

        [TestMethod]
        public void swapTwoEquations1()
        {
            level0.swapTwoEquations(0, 1);

            Assert.AreEqual(level0.coefficient(0, 0), 1);
            Assert.AreEqual(level0.coefficient(0, 1), 1);
            Assert.AreEqual(level0.coefficient(0, 2), 0);
            Assert.AreEqual(level0.coefficient(1, 0), 1);
            Assert.AreEqual(level0.coefficient(1, 1), 1);
            Assert.AreEqual(level0.coefficient(1, 2), 1);
            Assert.AreEqual(level0.coefficient(2, 0), 1);
            Assert.AreEqual(level0.coefficient(2, 1), 0);
            Assert.AreEqual(level0.coefficient(2, 2), 1);

            Assert.AreEqual(level0.RHS[0], 1);
            Assert.AreEqual(level0.RHS[1], 1);
            Assert.AreEqual(level0.RHS[2], 0);
        }

        [TestMethod]
        public void swapTwoEquations2()
        {
            level0.swapTwoEquations(0, 2);

            Assert.AreEqual(level0.coefficient(0, 0), 1);
            Assert.AreEqual(level0.coefficient(0, 1), 0);
            Assert.AreEqual(level0.coefficient(0, 2), 1);
            Assert.AreEqual(level0.coefficient(1, 0), 1);
            Assert.AreEqual(level0.coefficient(1, 1), 1);
            Assert.AreEqual(level0.coefficient(1, 2), 0);
            Assert.AreEqual(level0.coefficient(2, 0), 1);
            Assert.AreEqual(level0.coefficient(2, 1), 1);
            Assert.AreEqual(level0.coefficient(2, 2), 1);

            Assert.AreEqual(level0.RHS[0], 0);
            Assert.AreEqual(level0.RHS[1], 1);
            Assert.AreEqual(level0.RHS[2], 1);
        }

        [TestMethod]
        public void numberOfCoefficients()
        {
            Assert.AreEqual(level0.numberOfEquations, 3);
        }

    }
}
