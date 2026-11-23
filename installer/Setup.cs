using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ZynDrivesSetup
{
    public class SetupForm : Form
    {
        private TextBox _txtInstallDir;
        private CheckBox _chkDesktopShortcut;
        private CheckBox _chkStartup;
        private Button _btnInstall;
        private ProgressBar _progressBar;
        private Label _lblStatus;

        public SetupForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "ZynDrives Setup - Cloud SSD Virtual Disk Manager";
            this.Size = new Size(520, 390);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(11, 15, 25);
            this.ForeColor = Color.White;

            Font fontTitle = new Font("Segoe UI", 13.5f, FontStyle.Bold);
            Font fontSub = new Font("Segoe UI", 9.5f);
            Font fontBold = new Font("Segoe UI", 9.5f, FontStyle.Bold);

            Label lblTitle = new Label();
            lblTitle.Text = "⚡ Install ZynDrives";
            lblTitle.Font = fontTitle;
            lblTitle.ForeColor = Color.FromArgb(56, 189, 248);
            lblTitle.Location = new Point(25, 20);
            lblTitle.Size = new Size(300, 28);
            this.Controls.Add(lblTitle);

            Label lblDesc = new Label();
            lblDesc.Text = "Mount Google Drive & Cloud remotes as native Windows SSD drives (J:, K:, L:).";
            lblDesc.Font = fontSub;
            lblDesc.ForeColor = Color.FromArgb(148, 163, 184);
            lblDesc.Location = new Point(27, 50);
            lblDesc.Size = new Size(460, 35);
            this.Controls.Add(lblDesc);

            Label lblPath = new Label();
            lblPath.Text = "Installation Folder:";
            lblPath.Font = fontBold;
            lblPath.Location = new Point(25, 95);
            lblPath.Size = new Size(200, 20);
            this.Controls.Add(lblPath);

            _txtInstallDir = new TextBox();
            string defDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ZynOps", "ZynDrives");
            _txtInstallDir.Text = defDir;
            _txtInstallDir.Location = new Point(27, 120);
            _txtInstallDir.Size = new Size(365, 26);
            _txtInstallDir.BackColor = Color.FromArgb(22, 31, 48);
            _txtInstallDir.ForeColor = Color.White;
            this.Controls.Add(_txtInstallDir);

            Button btnBrowse = new Button();
            btnBrowse.Text = "Browse...";
            btnBrowse.Location = new Point(400, 118);
            btnBrowse.Size = new Size(80, 28);
            btnBrowse.BackColor = Color.FromArgb(30, 41, 59);
            btnBrowse.ForeColor = Color.White;
            btnBrowse.FlatStyle = FlatStyle.Flat;
            btnBrowse.Click += (s, e) =>
            {
                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    fbd.SelectedPath = _txtInstallDir.Text;
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        _txtInstallDir.Text = fbd.SelectedPath;
                    }
                }
            };
            this.Controls.Add(btnBrowse);

            _chkDesktopShortcut = new CheckBox();
            _chkDesktopShortcut.Text = "Create Desktop Shortcut";
            _chkDesktopShortcut.Checked = true;
            _chkDesktopShortcut.Location = new Point(27, 165);
            _chkDesktopShortcut.Size = new Size(300, 24);
            this.Controls.Add(_chkDesktopShortcut);

            _chkStartup = new CheckBox();
            _chkStartup.Text = "Auto-Mount Cloud Drives on Windows Startup";
            _chkStartup.Checked = true;
            _chkStartup.Location = new Point(27, 195);
            _chkStartup.Size = new Size(350, 24);
            this.Controls.Add(_chkStartup);

            _progressBar = new ProgressBar();
            _progressBar.Location = new Point(27, 235);
            _progressBar.Size = new Size(453, 10);
            this.Controls.Add(_progressBar);

            _lblStatus = new Label();
            _lblStatus.Text = "Ready to install.";
            _lblStatus.Location = new Point(27, 255);
            _lblStatus.Size = new Size(453, 20);
            _lblStatus.ForeColor = Color.FromArgb(148, 163, 184);
            this.Controls.Add(_lblStatus);

            _btnInstall = new Button();
            _btnInstall.Text = "Install ZynDrives";
            _btnInstall.Location = new Point(310, 290);
            _btnInstall.Size = new Size(170, 38);
            _btnInstall.BackColor = Color.FromArgb(14, 165, 233);
            _btnInstall.ForeColor = Color.White;
            _btnInstall.FlatStyle = FlatStyle.Flat;
            _btnInstall.FlatAppearance.BorderSize = 0;
            _btnInstall.Font = fontBold;
            _btnInstall.Click += OnInstallClick;
            this.Controls.Add(_btnInstall);
        }

        private void OnInstallClick(object sender, EventArgs e)
        {
            _btnInstall.Enabled = false;
            _progressBar.Style = ProgressBarStyle.Marquee;
            _lblStatus.Text = "Installing ZynDrives binaries...";

            string targetDir = _txtInstallDir.Text.Trim();
            try
            {
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                string currentDir = AppDomain.CurrentDomain.BaseDirectory;
                string sourceExe = Path.Combine(currentDir, "ZynDrives.exe");
                string destExe = Path.Combine(targetDir, "ZynDrives.exe");

                if (File.Exists(sourceExe))
                {
                    File.Copy(sourceExe, destExe, true);
                }
                else
                {
                    string altSource = Path.Combine(currentDir, @"..\bin\ZynDrives.exe");
                    if (File.Exists(altSource))
                    {
                        File.Copy(altSource, destExe, true);
                    }
                }

                // Copy uninstaller
                string uninstallerExe = Path.Combine(targetDir, "Uninstall.exe");
                string currentSetup = Process.GetCurrentProcess().MainModule.FileName;
                File.Copy(currentSetup, uninstallerExe, true);

                // Desktop shortcut
                if (_chkDesktopShortcut.Checked)
                {
                    CreateShortcut(destExe, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "ZynDrives.lnk"));
                }

                // Start Menu shortcut
                string startMenu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "ZynDrives.lnk");
                CreateShortcut(destExe, startMenu);

                // Auto-Mount Startup
                if (_chkStartup.Checked)
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                    {
                        if (key != null)
                        {
                            key.SetValue("ZynDrives", "\"" + destExe + "\" --mount-all");
                        }
                    }
                }

                // Register Uninstaller
                RegisterUninstaller(targetDir, destExe, uninstallerExe);

                _progressBar.Style = ProgressBarStyle.Continuous;
                _progressBar.Value = 100;
                _lblStatus.Text = "Installation complete!";
                _lblStatus.ForeColor = Color.FromArgb(52, 211, 153);

                DialogResult res = MessageBox.Show("ZynDrives installed successfully!\n\nWould you like to launch ZynDrives now?", "Installation Success", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (res == DialogResult.Yes && File.Exists(destExe))
                {
                    Process.Start(destExe);
                }

                this.Close();
            }
            catch (Exception ex)
            {
                _progressBar.Style = ProgressBarStyle.Continuous;
                _progressBar.Value = 0;
                _lblStatus.Text = "Error: " + ex.Message;
                _lblStatus.ForeColor = Color.FromArgb(244, 63, 94);
                _btnInstall.Enabled = true;
            }
        }

        private void CreateShortcut(string targetPath, string shortcutPath)
        {
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                dynamic shell = Activator.CreateInstance(shellType);
                dynamic shortcut = shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = targetPath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
                shortcut.Description = "ZynDrives - Cloud SSD Virtual Disk Manager";
                shortcut.Save();
            }
            catch { }
        }

        private void RegisterUninstaller(string installDir, string exePath, string uninstallerPath)
        {
            try
            {
                string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\ZynDrives";
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath))
                {
                    if (key != null)
                    {
                        key.SetValue("DisplayName", "ZynDrives (ZynOps Solutions)");
                        key.SetValue("DisplayIcon", exePath);
                        key.SetValue("DisplayVersion", "1.0.0");
                        key.SetValue("Publisher", "ZynOps Solutions");
                        key.SetValue("InstallLocation", installDir);
                        key.SetValue("UninstallString", "\"" + uninstallerPath + "\" /uninstall");
                        key.SetValue("URLInfoAbout", "https://github.com/hafizabdulahad0/ZynDrives");
                    }
                }
            }
            catch { }
        }

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0].Equals("/uninstall", StringComparison.OrdinalIgnoreCase))
            {
                DialogResult dr = MessageBox.Show("Are you sure you want to uninstall ZynDrives?", "Uninstall ZynDrives", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    try
                    {
                        // Remove startup
                        using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                        {
                            if (key != null) key.DeleteValue("ZynDrives", false);
                        }

                        // Remove uninstall key
                        Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\ZynDrives", false);

                        // Remove shortcuts
                        string desktopLnk = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "ZynDrives.lnk");
                        if (File.Exists(desktopLnk)) File.Delete(desktopLnk);

                        string startLnk = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "ZynDrives.lnk");
                        if (File.Exists(startLnk)) File.Delete(startLnk);

                        MessageBox.Show("ZynDrives uninstalled successfully.", "Uninstall Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error uninstalling: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SetupForm());
        }
    }
}
