using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Win32;

namespace ZynDrives
{
    public class DriveStatus
    {
        public DriveMapping Mapping { get; set; }
        public bool IsMounted { get; set; }
        public long TotalBytes { get; set; }
        public long FreeBytes { get; set; }
        public long UsedBytes { get; set; }

        public string FormattedTotal
        {
            get { return FormatBytes(TotalBytes); }
        }

        public string FormattedFree
        {
            get { return FormatBytes(FreeBytes); }
        }

        public string FormattedUsed
        {
            get { return FormatBytes(UsedBytes); }
        }

        public int PercentUsed
        {
            get
            {
                if (TotalBytes <= 0) return 0;
                return (int)((UsedBytes * 100) / TotalBytes);
            }
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes <= 0) return "0 GB";
            double gb = bytes / (1024.0 * 1024.0 * 1024.0);
            if (gb >= 1000)
            {
                double tb = gb / 1024.0;
                return tb.ToString("0.0") + " TB";
            }
            return gb.ToString("0.0") + " GB";
        }
    }

    public class DriveManager
    {
        private string _rclonePath;
        private readonly Dictionary<string, Process> _runningMounts = new Dictionary<string, Process>(StringComparer.OrdinalIgnoreCase);

        public string RclonePath
        {
            get { return _rclonePath; }
        }

        public DriveManager()
        {
            _rclonePath = FindRclone();
        }

        public static string FindRclone()
        {
            // 1. Check WinGet package path
            string wingetBase = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WinGet", "Packages");
            if (Directory.Exists(wingetBase))
            {
                try
                {
                    string[] matches = Directory.GetFiles(wingetBase, "rclone.exe", SearchOption.AllDirectories);
                    if (matches.Length > 0) return matches[0];
                }
                catch { }
            }

            // 2. Check Program Files
            string pf = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "rclone", "rclone.exe");
            if (File.Exists(pf)) return pf;

            // 3. Check AppData
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "rclone", "rclone.exe");
            if (File.Exists(appData)) return appData;

            // 4. PATH lookup
            string pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
            foreach (string p in pathEnv.Split(';'))
            {
                try
                {
                    string candidate = Path.Combine(p.Trim(), "rclone.exe");
                    if (File.Exists(candidate)) return candidate;
                }
                catch { }
            }

            return "rclone.exe";
        }

        public bool IsDriveMounted(string driveLetter)
        {
            string root = driveLetter.TrimEnd('\\');
            if (!root.EndsWith(":")) root += ":";
            root += "\\";

            foreach (var d in DriveInfo.GetDrives())
            {
                if (string.Equals(d.Name, root, StringComparison.OrdinalIgnoreCase))
                {
                    return d.IsReady;
                }
            }
            return false;
        }

        public DriveStatus GetStatus(DriveMapping mapping)
        {
            var status = new DriveStatus
            {
                Mapping = mapping,
                IsMounted = IsDriveMounted(mapping.DriveLetter)
            };

            if (status.IsMounted)
            {
                try
                {
                    string root = mapping.DriveLetter.TrimEnd('\\');
                    if (!root.EndsWith(":")) root += ":";
                    root += "\\";

                    var d = new DriveInfo(root);
                    if (d.IsReady)
                    {
                        status.TotalBytes = d.TotalSize;
                        status.FreeBytes = d.TotalFreeSpace;
                        status.UsedBytes = d.TotalSize - d.TotalFreeSpace;
                    }
                }
                catch { }
            }

            return status;
        }

        public bool Mount(DriveMapping mapping)
        {
            if (IsDriveMounted(mapping.DriveLetter))
                return true; // Already mounted

            string letter = mapping.DriveLetter.TrimEnd('\\');
            if (!letter.EndsWith(":")) letter += ":";

            string args = string.Format("mount {0} {1} --vfs-cache-mode full --cache-dir \"F:\\.rclone_cache\" --file-perms 0777 --dir-perms 0777 --vfs-cache-max-size 50G --vfs-cache-max-age 24h --vfs-read-ahead 128M --buffer-size 64M --drive-chunk-size {2} --links --volname \"{3}\"",
                mapping.RemoteName, letter, mapping.ChunkSize, mapping.VolumeName);

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = _rclonePath,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                var proc = Process.Start(psi);
                if (proc != null)
                {
                    lock (_runningMounts)
                    {
                        _runningMounts[letter] = proc;
                    }
                }

                // Wait up to 3 seconds for the drive to appear
                for (int i = 0; i < 15; i++)
                {
                    Thread.Sleep(200);
                    if (IsDriveMounted(letter)) return true;
                }

                return IsDriveMounted(letter);
            }
            catch
            {
                return false;
            }
        }

        public bool Unmount(string driveLetter)
        {
            string letter = driveLetter.TrimEnd('\\');
            if (!letter.EndsWith(":")) letter += ":";

            // If we have a tracked process, kill it
            lock (_runningMounts)
            {
                if (_runningMounts.ContainsKey(letter))
                {
                    try
                    {
                        var p = _runningMounts[letter];
                        if (!p.HasExited) p.Kill();
                    }
                    catch { }
                    _runningMounts.Remove(letter);
                }
            }

            // Also send command line unmount or kill matching rclone
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = string.Format("-NoProfile -Command \"Get-WmiObject Win32_Process | Where-Object {{ $_.Name -eq 'rclone.exe' -and $_.CommandLine -like '*{0}*' }} | ForEach-Object {{ Stop-Process -Id $_.ProcessId -Force }}\"", letter),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                var p = Process.Start(psi);
                if (p != null) p.WaitForExit(3000);
            }
            catch { }

            Thread.Sleep(500);
            return !IsDriveMounted(letter);
        }

        public void MountAll(List<DriveMapping> mappings)
        {
            foreach (var m in mappings)
            {
                if (m.AutoMount)
                {
                    Mount(m);
                }
            }
        }

        public void UnmountAll(List<DriveMapping> mappings)
        {
            foreach (var m in mappings)
            {
                Unmount(m.DriveLetter);
            }
        }

        public static bool IsStartupEnabled()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false))
                {
                    if (key != null)
                    {
                        return key.GetValue("ZynDrives") != null;
                    }
                }
            }
            catch { }
            return false;
        }

        public static void SetStartup(bool enable, string exePath)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        if (enable)
                        {
                            key.SetValue("ZynDrives", "\"" + exePath + "\" --mount-all");
                        }
                        else
                        {
                            key.DeleteValue("ZynDrives", false);
                        }
                    }
                }
            }
            catch { }
        }
    }
}
