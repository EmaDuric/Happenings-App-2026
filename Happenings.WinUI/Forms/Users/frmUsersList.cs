using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;

namespace Happenings.WinUI.Forms.Users
{
    public class frmUsersList : UserControl
    {
        private List<UserListItem> allUsers = new();
        private readonly APIService _apiService;

        private Panel? panelTop;
        private TextBox? txtSearch;
        private DataGridView? dgvUsers;

        public frmUsersList()
        {
            _apiService = (APIService)Program.ServiceProvider.GetService(typeof(APIService))!;
            InitializeComponent();
            LoadUsersAsync();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.Padding = new Padding(20);

            panelTop = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.Transparent };

            var lblSearch = new Label
            {
                Text = "Search:",
                Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular),
                Location = new System.Drawing.Point(0, 15),
                AutoSize = true
            };
            panelTop.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular),
                Location = new System.Drawing.Point(0, 35),
                Size = new System.Drawing.Size(300, 27)
            };
            txtSearch.TextChanged += (s, e) => FilterUsers();
            panelTop.Controls.Add(txtSearch);

            dgvUsers = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular),
                ColumnHeadersHeight = 40
            };

            dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvUsers.ColumnHeadersDefaultCellStyle.Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Bold);
            dgvUsers.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvUsers.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            dgvUsers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 249, 249);
            dgvUsers.RowTemplate.Height = 35;
            dgvUsers.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);

            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", Width = 60 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Username", HeaderText = "Username", DataPropertyName = "Username" });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email", DataPropertyName = "Email" });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "CreatedAt", HeaderText = "Registered", DataPropertyName = "FormattedDate", Width = 150 });

            this.Controls.Add(dgvUsers);
            this.Controls.Add(panelTop);
        }

        private async void LoadUsersAsync()
        {
            try
            {
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("http://localhost:5000/api/");
                if (!string.IsNullOrEmpty(TokenStore.Token))
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);
                var response = await httpClient.GetAsync("Users");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var users = JsonSerializer.Deserialize<List<UserListItem>>(json, options) ?? new List<UserListItem>();

                allUsers = users;
                dgvUsers!.DataSource = null;
                dgvUsers.DataSource = allUsers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users:\n{ex.Message}", "Error");
            }
        }

        private void FilterUsers()
        {
            if (string.IsNullOrWhiteSpace(txtSearch?.Text))
            {
                dgvUsers!.DataSource = null;
                dgvUsers.DataSource = allUsers;
                return;
            }

            var searchText = txtSearch.Text.ToLower();
            var filtered = allUsers.Where(u =>
                u.Username.ToLower().Contains(searchText) ||
                u.Email.ToLower().Contains(searchText)).ToList();

            dgvUsers!.DataSource = null;
            dgvUsers.DataSource = filtered;
        }
    }

    public class UserListItem
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string FormattedDate => CreatedAt.ToString("dd.MM.yyyy");
    }
}