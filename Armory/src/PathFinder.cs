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
        private bool _localCopies = true;
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
                if (File.Exists(ini_path) && tryReadIni()) {
                    // do nothing
                }
                else {
                    // No ini or bad  ini, have to search for wargame files:
                    String error = "";
                    bool notDone = true;
                    do {
                        String wargameDir = askUserForWargameDir(error);

                        // sanity check: is the user-provided dir correct?
                        String exe = Path.Combine(wargameDir, "Wargame3.exe");
                        if (!File.Exists(exe)) {
                            error = "Wargame exe " + exe + " not found. \n";
                            continue;
                        }

                        findWargameDataFiles(wargameDir);

                        // Ugly, but filesExist() will overwrite the error
                        // from the sanity check above, if used directly as 
                        // the loop condition
                        notDone = !filesExist(ref error);
                    } while (notDone);

                    // Make a new ini and save the found file paths
                    writeIni();
                }
            }

            // WRD locks NDF when running, by using a local copy we can 
            // run both programs in parallel.
            if (_localCopies) {
                ndf = makeLocalCopy(ndf);
                zz = makeLocalCopy(zz);
                zz4 = makeLocalCopy(zz4);
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

                if (line == "localCopies:false") {
                    _localCopies = false;
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
        
        private String askUserForWargameDir(string error) {
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

            return wargameDir;
        }

        private void findWargameDataFiles(string wargameDir) {
            // Find newest .dat files
            string searchDir = Path.Combine(wargameDir, "Data", "WARGAME", "PC");
            ndf = findNewest("NDF_Win.dat", searchDir, false);
            // checkSize (the bool arg) is a hacky fast fix for the random 8kb zz/zz_4..
            zz = findNewest("ZZ_Win.dat", searchDir, true);
            zz4 = findNewest("ZZ_4.dat", searchDir, true);
        }

        private bool filesExist(ref String error) {
            if (File.Exists(ndf)) {
                if (File.Exists(zz)) {
                    if (File.Exists(zz4)) {
                        return true;
                    }
                    
                    error = zz4 + " not found. \n";
                    return false;
                }

                error = zz + " not found. \n";
                return false;
            }

            error = ndf + " not found. \n";
            return false;
        }

        // When the paths to wargame data files have been found, it helps
        // to save them and avoid searching in future runs.
        private void writeIni() {
            string[] lines = { "ndf:" + ndf, "zz:" + zz, "zz4:" + zz4,
                // Don't set _autoUpdate the first run, but enable for the future.
                "autoupdate:true",
                // Local copies are enabled by default, but include in the file so SSD users can find and edit it easier
                "localCopies:true" };
            File.WriteAllLines(ini_path, lines);
            return;
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

        private String makeLocalCopy(String source) {
            String copiesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "local_copies");
            Directory.CreateDirectory(copiesFolder);
            String localPath = Path.Combine(copiesFolder, Path.GetFileName(source));

            // acceptably risky heuristic comparison, 
            // skip copying if same length
            if (File.Exists(localPath)) {
                if (new FileInfo(source).Length == new FileInfo(localPath).Length)
                    return source;
            }

            File.Copy(source, localPath, true);
            return localPath;
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
