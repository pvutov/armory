using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Updater {
    public partial class Form1 : Form {
        private String armoryDir;
        public Form1(String armoryDir, String patchNotes) {
            InitializeComponent();
            this.armoryDir = armoryDir;
            changeListLabel.Text = patchNotes.Replace(@"\r\n", Environment.NewLine);
        }

        private void cancelButton_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void updateButton_Click(object sender, EventArgs e) {
            String thisFile = Assembly.GetEntryAssembly().Location;
            foreach (String f in Directory.GetFiles(Path.GetDirectoryName(thisFile))) {
                if (f != thisFile) {
                    try {
                        File.Copy(f, Path.Combine(armoryDir, Path.GetFileName(f)), true);
                    }
                    catch (IOException ex) {
                        Program.warning("Could not write to file " + Path.Combine(armoryDir, Path.GetFileName(f))
                            + "\nMaybe it is in use?\n" + ex.ToString());
                    }
                }
            }

            if (File.Exists(Path.Combine(armoryDir, "Armory.exe"))) {
                Process.Start(Path.Combine(armoryDir, "Armory.exe"));
            }
            Application.Exit();
        }
    }
}
