using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Armory {
    /// <summary>
    /// Manages the paths to all files needed.
    /// </summary>
    class PathFinder {
        private const bool DEBUG = false;
        private const String NDF_PATH = @"E:\workspaceC\Armory\wrd_data\510061340\NDF_Win.dat";
        private const String ZZ_PATH = @"G:\SteamLibrary\SteamApps\common\Wargame Red Dragon\Data\WARGAME\PC\510060540\510061340\ZZ_Win.dat";
        private const String ZZ4_PATH = @"G:\SteamLibrary\SteamApps\common\Wargame Red Dragon\Data\WARGAME\PC\510060540\510061340\ZZ_4.dat";

        private String ini_path = AppDomain.CurrentDomain.BaseDirectory + "settings.ini";
        private String ndf;
        private String zz;
        private String zz4;

        public PathFinder() {
            //AppDomain.CurrentDomain.BaseDirectory
            if (DEBUG) {
                ndf = NDF_PATH;
                zz = ZZ_PATH;
                zz4 = ZZ4_PATH;
            }
            else {
                if (System.IO.File.Exists(ini_path)) {
                    if (!tryReadIni()) {
                        askUserForWargameDir();
                    }
                    else return;
                }
                else {
                    askUserForWargameDir();
                }
            }
        }

        private bool tryReadIni() {
            bool ndfRead = false;
            bool zzRead = false;
            bool zz4Read = false;

            string[] lines = System.IO.File.ReadAllLines(ini_path);
            foreach (string line in lines) {
                if (line.StartsWith("ndf:")) {
                    ndf = line.Substring("ndf:".Length);

                    if (!ndfRead) {
                        if (System.IO.File.Exists(ndf)) {
                            ndfRead = true;
                        }
                    }
                    // multiple valid paths provided for the same file
                    else {
                        return false;
                    }
                }

                else if (line.StartsWith("zz:")) {
                    zz = line.Substring("zz:".Length);

                    if (!zzRead) {
                        if (System.IO.File.Exists(zz)) {
                            zzRead = true;
                        }
                    }
                    // multiple valid paths provided for the same file
                    else {
                        return false;
                    }
                }

                else if (line.StartsWith("zz4:")) {
                    zz4 = line.Substring("zz4:".Length);

                    if (!zz4Read) {
                        if (System.IO.File.Exists(zz4)) {
                            zz4Read = true;
                        }
                    }
                    // multiple valid paths provided for the same file
                    else {
                        return false;
                    }
                }
            }

            if (ndfRead && zzRead && zz4Read) {
                return true;
            }

            return false;
        }

        private void askUserForWargameDir() {
            askUserForWargameDir("");
        }

        private void askUserForWargameDir(string error) {
            DialogResult res;
            string folderPath;
            using (FolderBrowserDialog fbd = new FolderBrowserDialog()) { 
                fbd.Description = error + "Where is your wargame folder?"
                    + " The path will be something like \n"
                    + @"C:\SteamLibrary\SteamApps\common\Wargame Red Dragon";
                res = fbd.ShowDialog();
                folderPath = fbd.SelectedPath;
            }

            if (res != DialogResult.OK) {
                Application.Exit();
            }
            else {
                ndf = folderPath + @"\Data\WARGAME\PC\510061340\NDF_Win.dat";
                zz = folderPath + @"\Data\WARGAME\PC\510060540\510061340\ZZ_Win.dat";
                zz4 = folderPath + @"\Data\WARGAME\PC\510060540\510061340\ZZ_4.dat";

                //fbd.Dispose();

                // Save dirs for next time
                if (System.IO.File.Exists(ndf)) {
                    if (System.IO.File.Exists(zz)) {
                        if (System.IO.File.Exists(zz4)) {
                            string[] lines = { "ndf:" + ndf, "zz:" + zz, "zz4:" + zz4 };
                            System.IO.File.WriteAllLines(ini_path, lines);
                            return;
                        }
                        
                        Application.Exit();

                        // Files not found, try again..
                        askUserForWargameDir(zz4 + " not found. \n");
                        return;
                    }
                    
                    Application.Exit();

                    // Files not found, try again..
                    askUserForWargameDir(zz + " not found. \n");
                    return;
                }
                
                Application.Restart();

                // Files not found, try again..
                askUserForWargameDir(ndf + " not found. \n");
                return;
            }
        }

        public String getNdfPath() {
            return ndf;
        }
        public String getZzPath() {
            return zz;
        }
        public String getZz4Path() {
            return zz4;
        }
    }
}
