using System;
using System.Drawing;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;

namespace Happenings.WinUI.Forms
{
    public class frmMain : Form
    {
        private Panel? panelSidebar;
        private Panel? panelHeader;
        private Label? lblAppTitle;
        private Button? btnDashboard;
        private Button? btnEvents;
        private Button? btnUsers;
        private Button? btnReservations;
        private Button? btnReports;
        private Button? btnCategories;
        private Button? btnVenues;
        private Panel? panelTopBar;
        private Label? lblPageTitle;
        private Label? lblUserInfo;
        private Button? btnLogout;
        private Panel? panelContent;

        public frmMain()
        {
            InitializeComponent();
            ShowDashboard();
        }

        private void InitializeComponent()
        {
            this.Text = "Happenings - Admin Panel";
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 245, 245);

            panelSidebar = new Panel { Dock = DockStyle.Left, Width = 250, BackColor = Color.FromArgb(44, 62, 80) };

            panelHeader = new Panel { Height = 80, Dock = DockStyle.Top, BackColor = Color.FromArgb(52, 73, 94) };
            lblAppTitle = new Label
            {
                Text = "HAPPENINGS",
                Font = new Font(new FontFamily("Segoe UI"), 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(243, 156, 18),
                AutoSize = false, Size = new Size(250, 40),
                Location = new Point(20, 15), TextAlign = ContentAlignment.MiddleLeft
            };
            panelHeader.Controls.Add(lblAppTitle);
            panelHeader.Controls.Add(new Label
            {
                Text = "Admin Panel",
                Font = new Font(new FontFamily("Segoe UI"), 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(236, 240, 241),
                AutoSize = false, Size = new Size(250, 20),
                Location = new Point(20, 50), TextAlign = ContentAlignment.MiddleLeft
            });
            panelSidebar.Controls.Add(panelHeader);

            int y = 100;
            btnDashboard = CreateMenuButton("📊 Dashboard", y); btnDashboard.Click += (s, e) => ShowDashboard(); y += 50;
            btnEvents = CreateMenuButton("📅 Events", y); btnEvents.Click += (s, e) => ShowEvents(); y += 50;
            btnUsers = CreateMenuButton("👥 Users", y); btnUsers.Click += (s, e) => ShowUsers(); y += 50;
            btnReservations = CreateMenuButton("🎫 Reservations", y); btnReservations.Click += (s, e) => ShowReservations(); y += 50;
            btnReports = CreateMenuButton("📊 Reports", y); btnReports.Click += (s, e) => ShowReports(); y += 50;

            panelSidebar.Controls.Add(new Panel { Height = 2, Width = 230, Location = new Point(10, y + 10), BackColor = Color.FromArgb(52, 73, 94) });
            y += 40;
            panelSidebar.Controls.Add(new Label
            {
                Text = "REFERENCE DATA",
                Font = new Font(new FontFamily("Segoe UI"), 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(149, 165, 166),
                AutoSize = false, Size = new Size(250, 20),
                Location = new Point(20, y), TextAlign = ContentAlignment.MiddleLeft
            });
            y += 30;
            btnCategories = CreateMenuButton("🏷️ Categories", y); btnCategories.Click += (s, e) => ShowCategories(); y += 50;
            btnVenues = CreateMenuButton("📍 Venues", y); btnVenues.Click += (s, e) => ShowVenues();

            panelTopBar = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
            lblPageTitle = new Label
            {
                Text = "Dashboard",
                Font = new Font(new FontFamily("Segoe UI"), 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(20, 15), AutoSize = true
            };
            panelTopBar.Controls.Add(lblPageTitle);

            lblUserInfo = new Label
            {
                Text = "Admin User",
                Font = new Font(new FontFamily("Segoe UI"), 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(127, 140, 141),
                AutoSize = true, Location = new Point(1100, 20)
            };
            panelTopBar.Controls.Add(lblUserInfo);

            btnLogout = new Button
            {
                Text = "Logout",
                Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Bold),
                Size = new Size(100, 35), Location = new Point(1240, 12),
                BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += btnLogout_Click;
            panelTopBar.Controls.Add(btnLogout);

            panelContent = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(245, 245, 245), Padding = new Padding(20) };

            this.Controls.Add(panelContent);
            this.Controls.Add(panelTopBar);
            this.Controls.Add(panelSidebar);
        }

        private Button CreateMenuButton(string text, int yPosition)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font(new FontFamily("Segoe UI"), 12, FontStyle.Regular),
                Size = new Size(250, 45), Location = new Point(0, yPosition),
                BackColor = Color.Transparent, ForeColor = Color.FromArgb(236, 240, 241),
                FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0), Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(52, 73, 94);
            panelSidebar?.Controls.Add(btn);
            return btn;
        }

        private async void ShowDashboard()
        {
            if (lblPageTitle != null) lblPageTitle.Text = "Dashboard";
            panelContent?.Controls.Clear();

            var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            // Summary cards
            int totalEvents = 0, totalUsers = 0, totalReservations = 0;

            try
            {
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("http://localhost:5000/api/");
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var eventsResp = await httpClient.GetAsync("Events");
                if (eventsResp.IsSuccessStatusCode)
                {
                    var json = await eventsResp.Content.ReadAsStringAsync();
                    var paged = JsonSerializer.Deserialize<PagedResult>(json, options);
                    totalEvents = paged?.TotalCount ?? 0;
                }

                var usersResp = await httpClient.GetAsync("Users");
                if (usersResp.IsSuccessStatusCode)
                {
                    var json = await usersResp.Content.ReadAsStringAsync();
                    var users = JsonSerializer.Deserialize<System.Collections.Generic.List<object>>(json, options);
                    totalUsers = users?.Count ?? 0;
                }

                var resResp = await httpClient.GetAsync("Reservations");
                if (resResp.IsSuccessStatusCode)
                {
                    var json = await resResp.Content.ReadAsStringAsync();
                    var reservations = JsonSerializer.Deserialize<System.Collections.Generic.List<object>>(json, options);
                    totalReservations = reservations?.Count ?? 0;
                }
            }
            catch { }

            // Cards
            int cardY = 20;
            AddDashboardCard(panel, "📅 Total Events", totalEvents.ToString(), Color.FromArgb(52, 152, 219), 20, cardY);
            AddDashboardCard(panel, "👥 Total Users", totalUsers.ToString(), Color.FromArgb(46, 204, 113), 270, cardY);
            AddDashboardCard(panel, "🎫 Reservations", totalReservations.ToString(), Color.FromArgb(155, 89, 182), 520, cardY);

            // Welcome text
            var lblWelcome = new Label
            {
                Text = "Welcome to Happenings Admin Panel!",
                Font = new Font(new FontFamily("Segoe UI"), 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                AutoSize = true,
                Location = new Point(20, cardY + 160)
            };
            panel.Controls.Add(lblWelcome);

            var lblInfo = new Label
            {
                Text = "Use the sidebar to navigate between sections.",
                Font = new Font(new FontFamily("Segoe UI"), 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(127, 140, 141),
                AutoSize = true,
                Location = new Point(20, cardY + 200)
            };
            panel.Controls.Add(lblInfo);

            panelContent?.Controls.Add(panel);
        }

        private void AddDashboardCard(Panel parent, string title, string value, Color color, int x, int y)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(230, 130),
                BackColor = color
            };

            card.Controls.Add(new Label
            {
                Text = title,
                Font = new Font(new FontFamily("Segoe UI"), 11, FontStyle.Regular),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(230, 30),
                Location = new Point(15, 15),
                TextAlign = ContentAlignment.MiddleLeft
            });

            card.Controls.Add(new Label
            {
                Text = value,
                Font = new Font(new FontFamily("Segoe UI"), 36, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(230, 70),
                Location = new Point(15, 45),
                TextAlign = ContentAlignment.MiddleLeft
            });

            parent.Controls.Add(card);
        }

        private void ShowEvents()
        {
            if (lblPageTitle != null) lblPageTitle.Text = "Events Management";
            panelContent?.Controls.Clear();
            panelContent?.Controls.Add(new Events.frmEventsList { Dock = DockStyle.Fill });
        }

        private void ShowUsers()
        {
            if (lblPageTitle != null) lblPageTitle.Text = "Users Management";
            panelContent?.Controls.Clear();
            panelContent?.Controls.Add(new Users.frmUsersList { Dock = DockStyle.Fill });
        }

        private void ShowReservations()
        {
            if (lblPageTitle != null) lblPageTitle.Text = "Reservations";
            panelContent?.Controls.Clear();
            panelContent?.Controls.Add(new Reservations.frmReservationsList { Dock = DockStyle.Fill });
        }

        private void ShowReports()
        {
            if (lblPageTitle != null) lblPageTitle.Text = "Reports";
            panelContent?.Controls.Clear();
            panelContent?.Controls.Add(new Reports.frmReports { Dock = DockStyle.Fill });
        }

        private void ShowCategories()
        {
            if (lblPageTitle != null) lblPageTitle.Text = "Event Categories";
            panelContent?.Controls.Clear();
            panelContent?.Controls.Add(new ReferenceData.frmCategoriesList { Dock = DockStyle.Fill });
        }

        private void ShowVenues()
        {
            if (lblPageTitle != null) lblPageTitle.Text = "Venues";
            panelContent?.Controls.Clear();
            panelContent?.Controls.Add(new ReferenceData.frmVenuesList { Dock = DockStyle.Fill });
        }

        private void btnLogout_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?",
                "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                var frmLogin = new frmLogin();
                frmLogin.Show();
                this.Close();
            }
        }

        private class PagedResult
        {
            public int TotalCount { get; set; }
        }
    }
}