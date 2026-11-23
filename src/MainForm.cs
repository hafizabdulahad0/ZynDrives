using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace ZynDrives
{
    public class MainForm : Form
    {
        private readonly DriveManager _manager;
        private List<DriveMapping> _mappings;
        private readonly System.Windows.Forms.Timer _pollTimer;
        private NotifyIcon _trayIcon;

        // Colors
        private readonly Color ColBg = Color.FromArgb(11, 15, 25);
        private readonly Color ColCard = Color.FromArgb(22, 31, 48);
        private readonly Color ColCardBorder = Color.FromArgb(45, 59, 85);
        private readonly Color ColPrimary = Color.FromArgb(56, 189, 248);
        private readonly Color ColPrimaryDark = Color.FromArgb(14, 165, 233);
        private readonly Color ColGreen = Color.FromArgb(52, 211, 153);
        private readonly Color ColRed = Color.FromArgb(244, 63, 94);
        private readonly Color ColTextMain = Color.FromArgb(248, 250, 252);
        private readonly Color ColTextMuted = Color.FromArgb(148, 163, 184);

        // UI Controls
        private Label _lblTotalStorage;
        private Label _lblActiveDrives;
        private Label _lblFreeStorage;
        private CheckBox _chkAutoStart;
        private FlowLayoutPanel _pnlDriveCards;
        private Button _btnMountAll;
        private Button _btnUnmountAll;
        private Button _btnRefresh;
        private Button _btnAddDrive;

        public MainForm()
        {
            _manager = new DriveManager();
            _mappings = ConfigStore.LoadMappings();

            InitializeComponent();
            SetupTrayIcon();

            _pollTimer = new System.Windows.Forms.Timer();
            _pollTimer.Interval = 3000;
            _pollTimer.Tick += (s, e) => RefreshDriveStatuses();
            _pollTimer.Start();

            RefreshDriveStatuses();
        }

        private void InitializeComponent()
        {
            this.Text = "ZynDrives - Cloud SSD Virtual Disk Manager";
            this.Size = new Size(840, 640);
            this.MinimumSize = new Size(800, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ColBg;
            this.ForeColor = ColTextMain;
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
                if (File.Exists(iconPath)) this.Icon = new Icon(iconPath);
            }
            catch { }

            // Header Panel
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                Padding = new Padding(24, 12, 24, 12),
                BackColor = ColBg
            };

            Label lblTitle = new Label
            {
                Text = "⚡ ZynDrives",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = ColPrimary,
                AutoSize = true,
                Location = new Point(20, 10)
            };

            Label lblSubtitle = new Label
            {
                Text = "Virtual NVMe Cloud Disk Manager • ZynOps Solutions",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = ColTextMuted,
                AutoSize = true,
                Location = new Point(22, 40)
            };

            _chkAutoStart = new CheckBox
            {
                Text = "🚀 Mount on Windows Startup",
                ForeColor = ColTextMain,
                AutoSize = true,
                Location = new Point(560, 22),
                Checked = DriveManager.IsStartupEnabled(),
                Cursor = Cursors.Hand
            };
            _chkAutoStart.CheckedChanged += (s, e) =>
            {
                DriveManager.SetStartup(_chkAutoStart.Checked, Application.ExecutablePath);
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(_chkAutoStart);
            this.Controls.Add(pnlHeader);

            // Metrics / HUD Panel
            Panel pnlHud = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                Padding = new Padding(20, 0, 20, 10),
                BackColor = ColBg
            };

            TableLayoutPanel hudTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            hudTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            hudTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            hudTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            Panel card1 = CreateMetricCard("☁️ TOTAL CLOUD CAPACITY", out _lblTotalStorage, ColPrimary);
            Panel card2 = CreateMetricCard("🟢 ACTIVE MOUNTED DRIVES", out _lblActiveDrives, ColGreen);
            Panel card3 = CreateMetricCard("💾 FREE STORAGE AVAILABLE", out _lblFreeStorage, ColTextMain);

            hudTable.Controls.Add(card1, 0, 0);
            hudTable.Controls.Add(card2, 1, 0);
            hudTable.Controls.Add(card3, 2, 0);
            pnlHud.Controls.Add(hudTable);
            this.Controls.Add(pnlHud);

            // Bottom Action Bar
            Panel pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                Padding = new Padding(20, 14, 20, 14),
                BackColor = ColCard
            };

            _btnMountAll = new Button
            {
                Text = "⚡ Mount All Drives",
                BackColor = ColPrimary,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Size = new Size(160, 40),
                Location = new Point(20, 15),
                Cursor = Cursors.Hand
            };
            _btnMountAll.FlatAppearance.BorderSize = 0;
            _btnMountAll.Click += (s, e) =>
            {
                _btnMountAll.Enabled = false;
                _btnMountAll.Text = "Mounting...";
                Application.DoEvents();
                _manager.MountAll(_mappings);
                RefreshDriveStatuses();
                _btnMountAll.Text = "⚡ Mount All Drives";
                _btnMountAll.Enabled = true;
            };

            _btnUnmountAll = new Button
            {
                Text = "⏹️ Unmount All",
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = ColTextMain,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                Size = new Size(130, 40),
                Location = new Point(190, 15),
                Cursor = Cursors.Hand
            };
            _btnUnmountAll.FlatAppearance.BorderColor = ColCardBorder;
            _btnUnmountAll.Click += (s, e) =>
            {
                _btnUnmountAll.Enabled = false;
                _btnUnmountAll.Text = "Stopping...";
                Application.DoEvents();
                _manager.UnmountAll(_mappings);
                RefreshDriveStatuses();
                _btnUnmountAll.Text = "⏹️ Unmount All";
                _btnUnmountAll.Enabled = true;
            };

            _btnAddDrive = new Button
            {
                Text = "➕ Add Remote Mapping",
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = ColTextMain,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(170, 40),
                Location = new Point(330, 15),
                Cursor = Cursors.Hand
            };
            _btnAddDrive.FlatAppearance.BorderColor = ColCardBorder;
            _btnAddDrive.Click += (s, e) => ShowAddDriveDialog();

            _btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = ColTextMuted,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(690, 15),
                Cursor = Cursors.Hand
            };
            _btnRefresh.FlatAppearance.BorderColor = ColCardBorder;
            _btnRefresh.Click += (s, e) => RefreshDriveStatuses();

            pnlBottom.Controls.Add(_btnMountAll);
            pnlBottom.Controls.Add(_btnUnmountAll);
            pnlBottom.Controls.Add(_btnAddDrive);
            pnlBottom.Controls.Add(_btnRefresh);
            this.Controls.Add(pnlBottom);

            // Center Content: Drive Cards Flow Layout
            _pnlDriveCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20, 10, 20, 10),
                BackColor = ColBg,
                WrapContents = false,
                FlowDirection = FlowDirection.TopDown
            };
            this.Controls.Add(_pnlDriveCards);

            // Make sure docking order is: Header, Hud, Bottom, Fill
            _pnlDriveCards.BringToFront();
        }

        private Panel CreateMetricCard(string title, out Label valLabel, Color valColor)
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColCard,
                Margin = new Padding(6),
                Padding = new Padding(14, 10, 14, 10)
            };

            Label lblT = new Label
            {
                Text = title,
                ForeColor = ColTextMuted,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 18
            };

            valLabel = new Label
            {
                Text = "--",
                ForeColor = valColor,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                Dock = DockStyle.Fill
            };

            card.Controls.Add(valLabel);
            card.Controls.Add(lblT);
            return card;
        }

        private void SetupTrayIcon()
        {
            _trayIcon = new NotifyIcon
            {
                Text = "ZynDrives - Cloud Disk Manager",
                Visible = true
            };

            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
                if (File.Exists(iconPath)) _trayIcon.Icon = new Icon(iconPath);
                else _trayIcon.Icon = SystemIcons.Application;
            }
            catch
            {
                _trayIcon.Icon = SystemIcons.Application;
            }

            var menu = new ContextMenuStrip();
            menu.Items.Add("Open ZynDrives", null, (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("⚡ Mount All Drives", null, (s, e) => { _manager.MountAll(_mappings); RefreshDriveStatuses(); });
            menu.Items.Add("⏹️ Unmount All", null, (s, e) => { _manager.UnmountAll(_mappings); RefreshDriveStatuses(); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Exit ZynDrives", null, (s, e) => { _trayIcon.Visible = false; Application.Exit(); });

            _trayIcon.ContextMenuStrip = menu;
            _trayIcon.DoubleClick += (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; };
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        public void RefreshDriveStatuses()
        {
            long totalCap = 0;
            long totalFree = 0;
            int activeCount = 0;

            _pnlDriveCards.SuspendLayout();
            _pnlDriveCards.Controls.Clear();

            foreach (var m in _mappings)
            {
                var status = _manager.GetStatus(m);
                if (status.IsMounted)
                {
                    activeCount++;
                    totalCap += status.TotalBytes;
                    totalFree += status.FreeBytes;
                }

                Panel driveCard = BuildDriveCard(status);
                _pnlDriveCards.Controls.Add(driveCard);
            }

            _pnlDriveCards.ResumeLayout();

            // Update Metrics HUD
            _lblActiveDrives.Text = string.Format("{0} / {1} Mounted", activeCount, _mappings.Count);
            _lblTotalStorage.Text = FormatBytes(totalCap);
            _lblFreeStorage.Text = FormatBytes(totalFree);
        }

        private Panel BuildDriveCard(DriveStatus s)
        {
            int cardWidth = Math.Max(740, this.ClientSize.Width - 65);
            Panel card = new Panel
            {
                Size = new Size(cardWidth, 90),
                BackColor = ColCard,
                Margin = new Padding(0, 0, 0, 12),
                Padding = new Padding(16, 12, 16, 12)
            };

            // Drive Letter Badge
            Label lblBadge = new Label
            {
                Text = s.Mapping.DriveLetter,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = s.IsMounted ? ColGreen : ColTextMuted,
                Size = new Size(50, 40),
                Location = new Point(14, 22),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Remote Name & Volume info
            Label lblRemote = new Label
            {
                Text = s.Mapping.RemoteName + "  ➔  " + s.Mapping.VolumeName,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ColTextMain,
                AutoSize = true,
                Location = new Point(74, 16)
            };

            // Details subtitle
            string cacheDetail = string.Format("VFS: {0} • Chunk: {1}", s.Mapping.CacheMode, s.Mapping.ChunkSize);
            string usageDetail = s.IsMounted
                ? string.Format("{0} used of {1} ({2}% full) • {3}", s.FormattedUsed, s.FormattedTotal, s.PercentUsed, cacheDetail)
                : string.Format("Drive offline • {0}", cacheDetail);

            Label lblDetails = new Label
            {
                Text = usageDetail,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = ColTextMuted,
                AutoSize = true,
                Location = new Point(76, 40)
            };

            // Status Pill
            Label lblStatus = new Label
            {
                Text = s.IsMounted ? "🟢 MOUNTED" : "⚪ OFFLINE",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = s.IsMounted ? ColGreen : ColTextMuted,
                Size = new Size(100, 26),
                Location = new Point(cardWidth - 270, 28),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Action: Mount / Unmount button
            Button btnToggle = new Button
            {
                Text = s.IsMounted ? "Unmount" : "Mount",
                Size = new Size(80, 32),
                Location = new Point(cardWidth - 170, 25),
                FlatStyle = FlatStyle.Flat,
                BackColor = s.IsMounted ? Color.FromArgb(40, 20, 30) : ColPrimary,
                ForeColor = s.IsMounted ? ColRed : Color.Black,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnToggle.FlatAppearance.BorderSize = 0;
            btnToggle.Click += (snd, ev) =>
            {
                btnToggle.Enabled = false;
                if (s.IsMounted)
                {
                    _manager.Unmount(s.Mapping.DriveLetter);
                }
                else
                {
                    _manager.Mount(s.Mapping);
                }
                RefreshDriveStatuses();
            };

            // Action: Open in Explorer button
            Button btnOpen = new Button
            {
                Text = "📂 Open",
                Size = new Size(70, 32),
                Location = new Point(cardWidth - 80, 25),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = ColTextMain,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                Cursor = Cursors.Hand,
                Enabled = s.IsMounted
            };
            btnOpen.FlatAppearance.BorderColor = ColCardBorder;
            btnOpen.Click += (snd, ev) =>
            {
                try
                {
                    Process.Start("explorer.exe", s.Mapping.DriveLetter + "\\");
                }
                catch { }
            };

            card.Controls.Add(lblBadge);
            card.Controls.Add(lblRemote);
            card.Controls.Add(lblDetails);
            card.Controls.Add(lblStatus);
            card.Controls.Add(btnToggle);
            card.Controls.Add(btnOpen);

            return card;
        }

        private void ShowAddDriveDialog()
        {
            Form dlg = new Form
            {
                Text = "Add Cloud Drive Mapping",
                Size = new Size(420, 300),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = ColBg,
                ForeColor = ColTextMain,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label l1 = new Label { Text = "Remote Name (e.g. ahad_dev:):", Location = new Point(20, 20), AutoSize = true, ForeColor = ColTextMuted };
            TextBox txtRemote = new TextBox { Location = new Point(20, 44), Width = 360, BackColor = ColCard, ForeColor = ColTextMain };

            Label l2 = new Label { Text = "Drive Letter (e.g. M:):", Location = new Point(20, 80), AutoSize = true, ForeColor = ColTextMuted };
            TextBox txtLetter = new TextBox { Location = new Point(20, 104), Width = 100, BackColor = ColCard, ForeColor = ColTextMain, Text = "M:" };

            Label l3 = new Label { Text = "Volume Label (e.g. MyCloudDrive):", Location = new Point(140, 80), AutoSize = true, ForeColor = ColTextMuted };
            TextBox txtVol = new TextBox { Location = new Point(140, 104), Width = 240, BackColor = ColCard, ForeColor = ColTextMain, Text = "CloudSSD" };

            Button btnSave = new Button
            {
                Text = "Save & Mount",
                DialogResult = DialogResult.OK,
                Location = new Point(250, 190),
                Size = new Size(130, 36),
                BackColor = ColPrimary,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };

            dlg.Controls.Add(l1);
            dlg.Controls.Add(txtRemote);
            dlg.Controls.Add(l2);
            dlg.Controls.Add(txtLetter);
            dlg.Controls.Add(l3);
            dlg.Controls.Add(txtVol);
            dlg.Controls.Add(btnSave);

            if (dlg.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(txtRemote.Text))
            {
                var newMapping = new DriveMapping(txtRemote.Text.Trim(), txtLetter.Text.Trim().ToUpper(), txtVol.Text.Trim());
                _mappings.Add(newMapping);
                ConfigStore.SaveMappings(_mappings);
                _manager.Mount(newMapping);
                RefreshDriveStatuses();
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
}
