using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ZynDrives
{
    public class DriveMapping
    {
        public string RemoteName { get; set; }
        public string DriveLetter { get; set; }
        public string VolumeName { get; set; }
        public string CacheMode { get; set; }
        public string ChunkSize { get; set; }
        public bool AutoMount { get; set; }

        public DriveMapping()
        {
            CacheMode = "full";
            ChunkSize = "128M";
            AutoMount = true;
        }

        public DriveMapping(string remote, string letter, string volName) : this()
        {
            RemoteName = remote;
            DriveLetter = letter;
            VolumeName = volName;
        }
    }

    public static class ConfigStore
    {
        private static readonly string ConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ZynOps", "ZynDrives");
        private static readonly string ConfigFile = Path.Combine(ConfigDir, "mappings.txt");

        public static List<DriveMapping> LoadMappings()
        {
            var list = new List<DriveMapping>();

            try
            {
                if (File.Exists(ConfigFile))
                {
                    string[] lines = File.ReadAllLines(ConfigFile);
                    foreach (string line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                        string[] parts = line.Split('|');
                        if (parts.Length >= 3)
                        {
                            var m = new DriveMapping
                            {
                                RemoteName = parts[0].Trim(),
                                DriveLetter = parts[1].Trim().ToUpper(),
                                VolumeName = parts[2].Trim(),
                                CacheMode = parts.Length > 3 ? parts[3].Trim() : "full",
                                ChunkSize = parts.Length > 4 ? parts[4].Trim() : "128M",
                                AutoMount = parts.Length > 5 ? parts[5].Trim() != "0" : true
                            };
                            list.Add(m);
                        }
                    }
                }
            }
            catch { }

            // If empty, auto-populate with detected defaults
            if (list.Count == 0)
            {
                list.Add(new DriveMapping("ahad_dev:", "J:", "ahad.dev"));
                list.Add(new DriveMapping("hafizahad:", "K:", "hafizahad"));
                list.Add(new DriveMapping("ahadawan:", "L:", "ahadawan"));
                SaveMappings(list);
            }

            return list;
        }

        public static void SaveMappings(List<DriveMapping> mappings)
        {
            try
            {
                if (!Directory.Exists(ConfigDir))
                    Directory.CreateDirectory(ConfigDir);

                var sb = new StringBuilder();
                sb.AppendLine("# ZynDrives Configuration Mapping File");
                sb.AppendLine("# Format: RemoteName|DriveLetter|VolumeName|CacheMode|ChunkSize|AutoMount");
                foreach (var m in mappings)
                {
                    sb.AppendLine(string.Format("{0}|{1}|{2}|{3}|{4}|{5}",
                        m.RemoteName, m.DriveLetter, m.VolumeName, m.CacheMode, m.ChunkSize, m.AutoMount ? "1" : "0"));
                }

                File.WriteAllText(ConfigFile, sb.ToString());
            }
            catch { }
        }
    }
}
