using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IrisZoomDataApi;
using IrisZoomDataApi.Model.Ndfbin;

namespace Armory.test {
    [TestClass]
    public class UnitDatabaseTests {
        private const String NDF_PATH = @"G:\SteamLibrary\SteamApps\common\Wargame Red Dragon\Data\WARGAME\PC\510061340\NDF_Win.dat";
        private const String ZZ_PATH = @"G:\SteamLibrary\SteamApps\common\Wargame Red Dragon\Data\WARGAME\PC\510060540\510061340\ZZ_Win.dat";
        private const String ZZ4_PATH = @"G:\SteamLibrary\SteamApps\common\Wargame Red Dragon\Data\WARGAME\PC\510060540\510061340\ZZ_4.dat";
        private const String Wargame_PATH = @"G:\SteamLibrary\SteamApps\common\Wargame Red Dragon\";

        const string EVERYTHING_NDFBIN = @"pc\ndf\patchable\gfx\everything.ndfbin";
        const string UNITES_DIC = @"pc\localisation\us\localisation\unites.dic";
        const string ICON_PACKAGE = @"pc\texture\pack\commoninterface.ppk";

        const string PACT_ICONS_DIRPREFIX = @"pc\texture\assets\2d\interface\common\unitsicons\pact\";
        const string NATO_ICONS_DIRPREFIX = @"pc\texture\assets\2d\interface\common\unitsicons\otan\";

        [TestMethod]
        public void testInit() {
            EdataManager dataManager = new EdataManager(NDF_PATH);
            dataManager.ParseEdataFile();
            NdfbinManager everything = dataManager.ReadNdfbin(EVERYTHING_NDFBIN);

            List<NdfObject> unitInstances = everything.GetClass("TUniteAuSolDescriptor").Instances;

            // setup localisation/unites.dic reader
            EdataManager dataManager2 = new EdataManager(ZZ_PATH);
            dataManager2.ParseEdataFile();
            TradManager dict = dataManager2.ReadDictionary(UNITES_DIC);

            // unit icons
            EdataManager zz4File = new EdataManager(ZZ4_PATH);
            zz4File.ParseEdataFile();
            EdataManager iconPackage = zz4File.ReadPackage(ICON_PACKAGE);

            UnitDatabase unitDatabase = new UnitDatabase(unitInstances, dict, iconPackage, PACT_ICONS_DIRPREFIX, NATO_ICONS_DIRPREFIX);

            List<String> countries = unitDatabase.getAllCountries();
            CollectionAssert.AllItemsAreUnique(countries);
            Assert.IsTrue(countries.Count > 0);
        }


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
            pathFinder.Invoke("findWargameDataFiles", new Object[]{ Wargame_PATH });
            bool pathsExist = (bool) pathFinder.Invoke("foundPathsExist", new Object[] { "" });
            Assert.IsTrue(pathsExist);
        }
    }
}
