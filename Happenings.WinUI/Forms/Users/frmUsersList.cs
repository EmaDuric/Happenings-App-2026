using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

            panelTop.Controls.Add(new Label { Text = "Search:", Font = new Font("Segoe UI", 10), Location = new System.Drawing.Point(0, 15), AutoSize = true });

            txtSearch = new TextBox { Font = new Font("Segoe UI", 10), Location = new System.Drawing.Point(0, 35), Size = new System.Drawing.Size(300, 27) };
            txtSearch.TextChanged += (s, e) => FilterUsers();
            panelTop.Controls.Add(txtSearch);

            // Dugme za promoviranje u organizatora
            var btnMakeOrganizer = new Button
            {
                Text = "⭐ Make Organizer",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new System.Drawing.Size(160, 35),
                Location = new System.Drawing.Point(320, 23),
                BackColor = Color.FromArgb(243, 156, 18),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnMakeOrganizer.FlatAppearance.BorderSize = 0;
            btnMakeOrganizer.Click += btnMakeOrganizer_Click;
            panelTop.Controls.Add(btnMakeOrganizer);

            // Dugme za uklanjanje organizatora
            var btnRemoveOrganizer = new Button
            {
                Text = "✖ Remove Organizer",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new System.Drawing.Size(170, 35),
                Location = new System.Drawing.Point(490, 23),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRemoveOrganizer.FlatAppearance.BorderSize = 0;
            btnRemoveOrganizer.Click += btnRemoveOrganizer_Click;
            panelTop.Controls.Add(btnRemoveOrganizer);

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
                AutoGenerateColumns = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10),
                ColumnHeadersHeight = 40
            };

            dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvUsers.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvUsers.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            dgvUsers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 249, 249);
            dgvUsers.RowTemplate.Height = 35;
            dgvUsers.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);

            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", Visible = false });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Username", HeaderText = "Username", DataPropertyName = "Username", FillWeight = 25 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email", DataPropertyName = "Email", FillWeight = 35 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "IsOrganizer", HeaderText = "Organizer", DataPropertyName = "IsOrganizerDisplay", FillWeight = 15 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "CreatedAt", HeaderText = "Registered", DataPropertyName = "FormattedDate", FillWeight = 15 });

            this.Controls.Add(dgvUsers);
            this.Controls.Add(panelTop);
        }

        private HttpClient CreateAuthorizedClient()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:5000/api/");
            if (!string.IsNullOrEmpty(TokenStore.Token))
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", TokenStore.Token);
            return httpClient;
        }

        private async void LoadUsersAsync()
        {
            try
            {
                var response = await CreateAuthorizedClient().GetAsync("Users");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                allUsers = JsonSerializer.Deserialize<List<UserListItem>>(json, options) ?? new();
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

        private async void btnMakeOrganizer_Click(object? sender, EventArgs e)
        {
            if (dgvUsers?.CurrentRow == null)
            {
                MessageBox.Show("Please select a user first.", "Info");
                return;
            }

            var id = (int)dgvUsers.CurrentRow.Cells["Id"].Value;
            var username = dgvUsers.CurrentRow.Cells["Username"].Value?.ToString();
            var isOrg = allUsers.FirstOrDefault(u => u.Id == id)?.IsOrganizer ?? false;

            if (isOrg)
            {
                MessageBox.Show($"{username} is already an organizer.", "Info");
                return;
            }

            // Dialog za unos organizer podataka
            var nameBox = new TextBox { Text = username, Width = 300 };
            var emailBox = new TextBox { Width = 300 };
            var phoneBox = new TextBox { Width = 300 };

            var form = new Form
            {
                Text = $"Make '{username}' an Organizer",
                Size = new Size(400, 280),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            int y = 20;
            form.Controls.Add(new Label { Text = "Organizer Name:", Location = new System.Drawing.Point(20, y), AutoSize = true }); y += 20;
            nameBox.Location = new System.Drawing.Point(20, y); form.Controls.Add(nameBox); y += 40;
            form.Controls.Add(new Label { Text = "Contact Email:", Location = new System.Drawing.Point(20, y), AutoSize = true }); y += 20;
            emailBox.Location = new System.Drawing.Point(20, y); form.Controls.Add(emailBox); y += 40;
            form.Controls.Add(new Label { Text = "Phone Number:", Location = new System.Drawing.Point(20, y), AutoSize = true }); y += 20;
            phoneBox.Location = new System.Drawing.Point(20, y); form.Controls.Add(phoneBox); y += 50;

            var btnOk = new Button { Text = "Confirm", DialogResult = DialogResult.OK, Location = new System.Drawing.Point(20, y), Size = new Size(100, 35), BackColor = Color.FromArgb(243, 156, 18), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnOk.FlatAppearance.BorderSize = 0;
            var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new System.Drawing.Point(130, y), Size = new Size(100, 35) };
            form.Controls.Add(btnOk);
            form.Controls.Add(btnCancel);
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            if (form.ShowDialog() != DialogResult.OK) return;

            try
            {
                var request = new
                {
                    userId = id,
                    name = nameBox.Text.Trim(),
                    contactEmail = emailBox.Text.Trim(),
                    phoneNumber = phoneBox.Text.Trim()
                };

                var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await CreateAuthorizedClient().PostAsync("Organizers", content);
                response.EnsureSuccessStatusCode();

                MessageBox.Show($"{username} is now an organizer!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadUsersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error");
            }
        }

        private async void btnRemoveOrganizer_Click(object? sender, EventArgs e)
        {
            if (dgvUsers?.CurrentRow == null)
            {
                MessageBox.Show("Please select a user first.", "Info");
                return;
            }

            var userId = (int)dgvUsers.CurrentRow.Cells["Id"].Value;
            var username = dgvUsers.CurrentRow.Cells["Username"].Value?.ToString();
            var isOrg = allUsers.FirstOrDefault(u => u.Id == userId)?.IsOrganizer ?? false;

            if (!isOrg)
            {
                MessageBox.Show($"{username} is not an organizer.", "Info");
                return;
            }

            if (MessageBox.Show($"Remove organizer role from '{username}'?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                // Dohvati organizer ID za ovog usera
                var response = await CreateAuthorizedClient().GetAsync("Organizers");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var organizers = JsonSerializer.Deserialize<List<OrganizerListItem>>(json, options) ?? new();
                var organizer = organizers.FirstOrDefault(o => o.UserId == userId);

                if (organizer == null)
                {
                    MessageBox.Show("Organizer profile not found.", "Error");
                    return;
                }

                var deleteResponse = await CreateAuthorizedClient().DeleteAsync($"Organizers/{organizer.Id}");
                deleteResponse.EnsureSuccessStatusCode();

                MessageBox.Show($"Organizer role removed from {username}.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadUsersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error");
            }
        }
    }

    public class UserListItem
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsOrganizer { get; set; }
        public string IsOrganizerDisplay => IsOrganizer ? "✅ Yes" : "No";
        public DateTime CreatedAt { get; set; }
        public string FormattedDate => CreatedAt.ToString("dd.MM.yyyy");
    }

    public class OrganizerListItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Name { get; set; }
    }
}