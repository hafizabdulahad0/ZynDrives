using System;
using System.Threading;
using System.Windows.Forms;

namespace ZynDrives
{
    static class Program
    {
        private const string AppGuid = "Global\\ZynDrives_ZynOps_SingleInstance_Mutex";

        [STAThread]
        static void Main(string[] args)
        {
            // Process command-line flags for automation / startup
            if (args != null && args.Length > 0)
            {
                string cmd = args[0].ToLowerInvariant();
                var manager = new DriveManager();
                var mappings = ConfigStore.LoadMappings();

                if (cmd == "--mount-all" || cmd == "/mountall" || cmd == "-m")
                {
                    manager.MountAll(mappings);
                    return;
                }
                else if (cmd == "--unmount-all" || cmd == "/unmountall" || cmd == "-u")
                {
                    manager.UnmountAll(mappings);
                    return;
                }
                else if (cmd == "--status" || cmd == "/status")
                {
                    foreach (var m in mappings)
                    {
                        var s = manager.GetStatus(m);
                        Console.WriteLine("{0} ({1}) -> {2} [{3}]",
                            m.DriveLetter, m.RemoteName, s.IsMounted ? "MOUNTED" : "OFFLINE", s.FormattedTotal);
                    }
                    return;
                }
            }

            // GUI Launch with Single Instance check
            bool isNewInstance;
            using (Mutex mutex = new Mutex(true, AppGuid, out isNewInstance))
            {
                if (!isNewInstance)
                {
                    MessageBox.Show("ZynDrives is already running. Check your system tray icon.",
                        "ZynDrives Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
        }
    }
}
