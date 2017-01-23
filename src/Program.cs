using System;
using System.Collections.Generic;
using System.Windows.Forms;
using IrisZoomDataApi;
using IrisZoomDataApi.Model.Ndfbin;

namespace Armory {
    static class Program {

        // Where to report bugs?
        public const string CONTACT_STRING = " To report, PM throwaway on forums.eugensystems.com";
        private static DialogResult warningsSuppressed = DialogResult.No;
        
        const string EVERYTHING_NDFBIN = @"pc\ndf\patchable\gfx\everything.ndfbin";
        const string UNITES_DIC = @"pc\localisation\us\localisation\unites.dic";
        const string ICON_PACKAGE = @"pc\texture\pack\commoninterface.ppk";

        const string PACT_ICONS_DIRPREFIX = @"pc\texture\assets\2d\interface\common\unitsicons\pact\";
        const string NATO_ICONS_DIRPREFIX = @"pc\texture\assets\2d\interface\common\unitsicons\otan\";
        
        
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            PathFinder paths = new PathFinder();

            // setup everything.ndfbin reader
            //EdataManager dataManager = new EdataManager(AppDomain.CurrentDomain.BaseDirectory + "NDF_Win.dat");
            EdataManager dataManager = new EdataManager(paths.getNdfPath());
            dataManager.ParseEdataFile();
            NdfbinManager everything = dataManager.ReadNdfbin(EVERYTHING_NDFBIN);
            
            List<NdfObject> unitInstances = everything.GetClass("TUniteAuSolDescriptor").Instances;

            // setup localisation/unites.dic reader
            EdataManager dataManager2 = new EdataManager(paths.getZzPath());
            dataManager2.ParseEdataFile();
            TradManager dict = dataManager2.ReadDictionary(UNITES_DIC);

            // unit icons
            EdataManager zz4File = new EdataManager(paths.getZz4Path());
            zz4File.ParseEdataFile();
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
    }
}
