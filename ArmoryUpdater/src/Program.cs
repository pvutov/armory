using System;
using System.Windows.Forms;

namespace Updater {
    static class Program {
        // Where to report bugs?
        public const string CONTACT_STRING = "\nTo report, PM throwaway on forums.eugensystems.com";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String[] args) {
            String armoryDir = args[0];
            String patchNotes = "";

            for (int i = 1; i < args.Length; i++) {
                patchNotes += args[i] + " ";
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(armoryDir, patchNotes));
        }


        /// <summary>
        ///  Display a warning string in cases of unexpected behavior.
        /// </summary>
        public static void warning(String warningText) {
            MessageBox.Show(warningText + Program.CONTACT_STRING + "\nSuppress further warnings?", "Warning",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        }
    }
}
