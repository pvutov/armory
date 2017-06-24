using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Armory {
    /// <summary>
    /// Manages the paths to all files needed.
    /// </summary>
    class PathFinder {
        private const bool DEBUG = false;
        private const String NDF_PATH_DEBUG = @"E:\workspaceC\Armory\wrd_data\510061340\NDF_Win.dat";
        private const String ZZ_PATH_DEBUG = @"G:\SteamLibrary\SteamApps\common\Wargame Red Dragon\Data\WARGAME\PC\510060540\510061340\ZZ_Win.dat";
        private const String ZZ4_PATH_DEBUG = @"G:\SteamLibrary\SteamApps\common\Wargame Red Dragon\Data\WARGAME\PC\510060540\510061340\ZZ_4.dat";

        private String ini_path = AppDomain.CurrentDomain.BaseDirectory + "settings.ini";        
        private String ndf;
        private String zz;
        private String zz4;
        private bool _autoUpdate;
        public bool autoUpdate {
            get { return _autoUpdate; }
        }

        public PathFinder() {
            if (DEBUG) {
                ndf = NDF_PATH_DEBUG;
                zz = ZZ_PATH_DEBUG;
                zz4 = ZZ4_PATH_DEBUG;
            }
            else {
                if (File.Exists(ini_path)) {
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

            string[] lines = null;
            try {
                lines = File.ReadAllLines(ini_path);
            } catch (Exception e) {
                Program.warning("Exception when reading settings.ini: " + e.ToString());
                return false;
            }

            foreach (string line in lines) {
                if (line == "autoupdate:true") {
                    _autoUpdate = true;
                }

                if (line.StartsWith("ndf:")) {
                    ndf = line.Substring("ndf:".Length);

                    if (!ndfRead) {
                        if (File.Exists(ndf)) {
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
                        if (File.Exists(zz)) {
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
                        if (File.Exists(zz4)) {
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
            string wargameDir;
            using (FolderBrowserDialog fbd = new FolderBrowserDialog()) { 
                fbd.Description = error + "Where is your wargame folder?"
                    + " The path will be something like \n"
                    + @"C:\SteamLibrary\SteamApps\common\Wargame Red Dragon";
                res = fbd.ShowDialog();
                wargameDir = fbd.SelectedPath;
            }

            if (res != DialogResult.OK) {
                Application.Exit();
                Environment.Exit(0);
            }
            else {
                // sanity check: was the provided dir correct?
                String exe = Path.Combine(wargameDir, "Wargame3.exe");
                if (!File.Exists(exe)) {
                    askUserForWargameDir(exe + " not found. \n");
                    return;
                }

                // Find newest .dat files
                string searchDir = Path.Combine(wargameDir, "Data", "WARGAME", "PC");
                ndf = findNewest("NDF_Win.dat", searchDir, false);
                // checkSize is a hacky fast fix for the random 8kb zz/zz_4..
                zz = findNewest("ZZ_Win.dat", searchDir, true);
                zz4 = findNewest("ZZ_4.dat", searchDir, true);

                // Save dirs for next time
                if (File.Exists(ndf)) {
                    if (File.Exists(zz)) {
                        if (File.Exists(zz4)) {
                            string[] lines = { "ndf:" + ndf, "zz:" + zz, "zz4:" + zz4,
                                // Don't set _autoUpdate the first run, but enable for the future.
                                "autoupdate:true" };
                            File.WriteAllLines(ini_path, lines);
                            return;
                        }

                        // Files not found, try again..
                        askUserForWargameDir(zz4 + " not found. \n");
                        return;
                    }

                    // Files not found, try again..
                    askUserForWargameDir(zz + " not found. \n");
                    return;
                }

                // Files not found, try again..
                askUserForWargameDir(ndf + " not found. \n");
                return;
            }
        }

        /// <summary>
        /// Heuristic: We do a descending alphabetic depth-first search and assume that the first file encountered is correct.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="searchDir"></param>
        /// <returns></returns>
        private String findNewest(String filename, String searchDir, bool checkSize) {
            // TODO
            string result = null;
            DirectoryInfo di = new DirectoryInfo(searchDir);

            // Order from biggest number, which usually means most recent patch
            var ordered = di.GetDirectories().OrderByDescending(x => x.Name);
            foreach (DirectoryInfo innerDi in ordered) {
                if (tryFindNewestRecursive(filename, innerDi, checkSize, out result)) {
                    return result;
                }
            }

            if (result == null) {
                Program.warning("File not found: " + filename);
            }
            return result;
        }

        private bool tryFindNewestRecursive(String filename, DirectoryInfo di, bool checkSize, out string result) {
            result = null;
            string possibleFilePath = Path.Combine(di.FullName, filename);

            // If a file is found, we're done.
            if (File.Exists(possibleFilePath)) {
                long length = new FileInfo(possibleFilePath).Length;
                if (length > 10000 || !checkSize) {
                    result = possibleFilePath;
                    return true;
                }
            }

            // If a file doesn't exist in this directory, recurse on subdirs:
            var ordered = di.GetDirectories().OrderByDescending(x => x.Name);
            foreach (DirectoryInfo innerDi in ordered) {
                if (tryFindNewestRecursive(filename, innerDi, checkSize, out result)) {
                    return true;
                }
            }

            return false;
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
