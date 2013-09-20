using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Crossflip;

namespace Testing
{
    [TestClass]
    public class BoardTestLevel0
    {
        Board level0;

        [TestInitialize()]
        public void initialiser()
        {
            //recreate the equations system
            level0 = new Board("<script>var boardinit =  \"11,02\";var level = 0;");
        }

        [TestCleanup()]
        public void cleanup()
        {
            level0 = null;
        }

        [TestMethod]
        public void boardProperties1()
        {
            Assert.AreEqual(level0.boardLength, 4);
            Assert.AreEqual(level0.firstIndexDimension, 2);
            Assert.AreEqual(level0.secondIndexDimension, 2);
            Assert.AreEqual(level0.numberOfUnselectables, 1);
            Assert.AreEqual(level0.locationsOfUnselectables.Length, 1);
            Assert.AreEqual(level0.locationsOfUnselectables[0], 3);
        }
    }
}
