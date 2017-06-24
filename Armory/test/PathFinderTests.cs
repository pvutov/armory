using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Armory.test {
    class PathFinderTests {
        private const String Wargame_PATH = @"G:\SteamLibrary\SteamApps\common\Wargame Red Dragon\";

        [TestMethod]
        public void testPathFinder() {
            /**
             * A working settings.ini causes PathFinder to skip a lot of its logic
             * and may thus obscure bugs. I once introduced a bug in the algo
             * for finding NDF_Win.dat and didn't notice it until release.
             * This is a regression test for that case.
             */
            PathFinder p = new PathFinder();
            // needed to test private methods
            PrivateObject pathFinder = new PrivateObject(p);
            pathFinder.Invoke("findWargameDataFiles", new Object[] { Wargame_PATH });
            bool pathsExist = (bool)pathFinder.Invoke("foundPathsExist", new Object[] { "" });
            Assert.IsTrue(pathsExist);
        }
    }
}
