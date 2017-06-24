using IrisZoomDataApi;
using IrisZoomDataApi.Model.Ndfbin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Armory {
    /// <summary>
    /// Unit database objects are large, ~400mb each. This singleton pool
    /// manages their creation and in particular avoids the creation of duplicates.
    /// </summary>
    class UnitDatabasePool {
        const string EVERYTHING_NDFBIN = @"pc\ndf\patchable\gfx\everything.ndfbin";
        const string UNITES_DIC = @"pc\localisation\us\localisation\unites.dic";
        const string ICON_PACKAGE = @"pc\texture\pack\commoninterface.ppk";

        const string PACT_ICONS_DIRPREFIX = @"pc\texture\assets\2d\interface\common\unitsicons\pact\";
        const string NATO_ICONS_DIRPREFIX = @"pc\texture\assets\2d\interface\common\unitsicons\otan\";

        private const string APPCAST_DIR = @"https://raw.githubusercontent.com/pvutov/armory/master/appcast.xml";

        private static UnitDatabasePool singleton;
        private UnitDatabasePool() { }

        PathFinder paths;
        TradManager dict;
        EdataManager iconPackage;

        private Dictionary<String, List<Form1>> databaseIndex = new Dictionary<String, List<Form1>>();

        public static UnitDatabasePool getUnitDatabasePool() {
            if (singleton == null) {
                singleton = new UnitDatabasePool();

                singleton.paths = PathFinder.getPathFinder();
                
                readDictionaries();
                readIcons();           
            }

            return singleton;
        }

        public UnitDatabase getUnitDatabase(Form1 caller, String version) {
            // We deregister the requesting form from whatever previous version it was using,
            // register it with the new version and create or find a database for it.


            // deregister
            foreach (List<Form1> item in databaseIndex.Values) {
                item.Remove(caller);
            }

            // Check if a database exists or if we will have to create:
            List<Form1> val = null;
            if (databaseIndex.TryGetValue(version, out val) && val.Any()) {
                UnitDatabase result = val.First().unitDatabase.clone();

                // register
                val.Add(caller);

                return result;
            }
            else {
                // register
                val = new List<Form1>();
                val.Add(caller);
                databaseIndex.Remove(version);
                databaseIndex.Add(version, val);

                // create
                EdataManager dataManager = new EdataManager(singleton.paths.getNdfPath(version));
                try {
                    dataManager.ParseEdataFile();
                }
                catch (System.IO.IOException) {
                    Program.warning("IOException thrown, could not parse " + singleton.paths.getNdfPath()
                        + ".\nIf wargame is running, you'll have to close it to use the tool. You can avoid this by copying the files listed in settings.ini and then editing settings.ini to point to the copies.");
                }

                NdfbinManager everything = dataManager.ReadNdfbin(EVERYTHING_NDFBIN);
                List<NdfObject> unitInstances = everything.GetClass("TUniteAuSolDescriptor").Instances;
                
                UnitDatabase database = new UnitDatabase(unitInstances, dict, iconPackage, PACT_ICONS_DIRPREFIX, NATO_ICONS_DIRPREFIX);
                // Transfer weapon lock
                if (caller.unitDatabase != null && caller.unitDatabase.tryGetLockIndexedWeapon() != null) {
                    database.setCurrentWeapon(caller.unitDatabase.tryGetLockIndexedWeapon());
                    database.lockWeapon();
                }

                return database;
            }
        }

        private void readNDF() {

        }

        private static void readDictionaries() {
            EdataManager dataManager2 = new EdataManager(singleton.paths.getZzPath());
            try {
                dataManager2.ParseEdataFile();
            }
            catch (System.IO.IOException) {
                Program.warning("IOException thrown, could not parse " + singleton.paths.getZzPath()
                    + ".\nIf wargame is running, you'll have to close it to use the tool. You can avoid this by copying the files listed in settings.ini and then editing settings.ini to point to the copies.");
                singleton = null;
            }

            singleton.dict = null;
            try {
                singleton.dict = dataManager2.ReadDictionary(UNITES_DIC);
            }
            catch (Exception) {
                Program.warning("Failed reading ZZ_Win.dat. May have selected an incomplete one - try pointing settings.ini to a complete ZZ_Win.dat file.");
                singleton = null;
            }
        }

        private static void readIcons() {
            EdataManager zz4File = new EdataManager(singleton.paths.getZz4Path());
            try {
                zz4File.ParseEdataFile();
            }
            catch (System.IO.IOException) {
                Program.warning("IOException thrown, could not parse " + singleton.paths.getZz4Path()
                    + ".\nIf wargame is running, you'll have to close it to use the tool. You can avoid this by copying the files listed in settings.ini and then editing settings.ini to point to the copies.");
                singleton = null;
            }

            singleton.iconPackage = null;
            try {
                singleton.iconPackage = zz4File.ReadPackage(ICON_PACKAGE);
            }
            catch (Exception) {
                Program.warning("Failed reading ZZ_4.dat. May have selected an incomplete one - try pointing settings.ini to a complete ZZ_4.dat file.");
                singleton = null;
            }
        }
    }
}
