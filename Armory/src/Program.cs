using System;
using System.Collections.Generic;
using System.Windows.Forms;
using IrisZoomDataApi;
using IrisZoomDataApi.Model.Ndfbin;

namespace Armory {
    static class Program {

        // Where to report bugs?
        public const string CONTACT_STRING = "\nTo report, PM throwaway on forums.eugensystems.com";
        private static DialogResult warningsSuppressed = DialogResult.No;
        
        const string EVERYTHING_NDFBIN = @"pc\ndf\patchable\gfx\everything.ndfbin";
        const string UNITES_DIC = @"pc\localisation\us\localisation\unites.dic";
        const string ICON_PACKAGE = @"pc\texture\pack\commoninterface.ppk";

        const string PACT_ICONS_DIRPREFIX = @"pc\texture\assets\2d\interface\common\unitsicons\pact\";
        const string NATO_ICONS_DIRPREFIX = @"pc\texture\assets\2d\interface\common\unitsicons\otan\";

        private const string APPCAST_DIR = @"https://raw.githubusercontent.com/pvutov/armory/master/appcast.xml";

        [STAThread]
        static void Main() {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(cleanup);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            PathFinder paths = new PathFinder();

            // setup everything.ndfbin reader
            // EdataManager dataManager = new EdataManager(AppDomain.CurrentDomain.BaseDirectory + "NDF_Win.dat");
            EdataManager dataManager = new EdataManager(paths.getNdfPath());
            try {
                dataManager.ParseEdataFile();
            }
            catch (System.IO.IOException) {
                warning("IOException thrown, could not parse " + paths.getNdfPath()
                    + ".\nIf wargame is running, you'll have to close it to use the tool. You can avoid this by copying the files listed in settings.ini and then editing settings.ini to point to the copies.");
                Application.Exit();
                Environment.Exit(0);
            }
            NdfbinManager everything = dataManager.ReadNdfbin(EVERYTHING_NDFBIN);
            
            List<NdfObject> unitInstances = everything.GetClass("TUniteAuSolDescriptor").Instances;

            // setup localisation/unites.dic reader
            EdataManager dataManager2 = new EdataManager(paths.getZzPath());
            try {
                dataManager2.ParseEdataFile();
            }
            catch (System.IO.IOException) {
                warning("IOException thrown, could not parse " + paths.getZzPath()
                    + ".\nIf wargame is running, you'll have to close it to use the tool. You can avoid this by copying the files listed in settings.ini and then editing settings.ini to point to the copies.");
                Application.Exit();
                Environment.Exit(0);
            }
            TradManager dict = dataManager2.ReadDictionary(UNITES_DIC);

            // unit icons
            EdataManager zz4File = new EdataManager(paths.getZz4Path());
            try {
                zz4File.ParseEdataFile();
            }
            catch (System.IO.IOException) {
                warning("IOException thrown, could not parse " + paths.getZz4Path()
                    + ".\nIf wargame is running, you'll have to close it to use the tool. You can avoid this by copying the files listed in settings.ini and then editing settings.ini to point to the copies.");
                Application.Exit();
                Environment.Exit(0);
            }
            EdataManager iconPackage = zz4File.ReadPackage(ICON_PACKAGE);

            UnitDatabase unitDatabase = new UnitDatabase(unitInstances, dict, iconPackage, PACT_ICONS_DIRPREFIX, NATO_ICONS_DIRPREFIX);
            
            Application.Run(new Form1(unitDatabase));
        }

        /// <summary>
        ///  Display a warning string in cases of unexpected behavior.
        /// </summary>
        public static void warning(String warningText) {
            if (warningsSuppressed != DialogResult.Yes) {
                warningsSuppressed = MessageBox.Show(warningText + Program.CONTACT_STRING + "\nSuppress further warnings?", "Warning",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// On process exit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void cleanup(object sender, EventArgs e) {
        }
    }
}
