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
        

        [STAThread]
        static void Main() {
            // Print all uncaught exceptions.
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(
                delegate (Object sender, UnhandledExceptionEventArgs e) {
                    warning(e.ExceptionObject.ToString());
                }
            );

            // Free resources before exiting.
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(cleanup);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            PathFinder paths = PathFinder.getPathFinder();
            Application.Run(new Form1("default", paths.getVersionsDesc(), paths.autoUpdate));
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
