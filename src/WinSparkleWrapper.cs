using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Armory {
    class WinSparkleWrapper {

        [DllImport("WinSparkle.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void win_sparkle_init();
        [DllImport("WinSparkle.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void win_sparkle_cleanup();
        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void win_sparkle_set_appcast_url(String url);
        [DllImport("WinSparkle.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void win_sparkle_set_app_details(String company_name,
                                             String app_name,
                                             String app_version);
        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void win_sparkle_set_registry_path(String path);
        [DllImport("WinSparkle.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void win_sparkle_check_update_with_ui();
    }
}
