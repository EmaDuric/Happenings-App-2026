using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace Happenings.WinUI.Forms.OrganizerRequests
{
    public class frmOrganizerRequests : UserControl
    {
        private List<OrganizerRequestListItem> allRequests = new();
        private DataGridView? dgvRequests;
        private ComboBox? cmbStatus;

        public frmOrganizerRequests()
        {
            InitializeComponent();
            LoadRequestsAsync();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.Padding = new Padding(20);

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.Transparent };

            panelTop.Controls.Add(new Label { Text = "Status:", Font = new Font("Segoe UI", 10), Location = new System.Drawing.Point(0, 15), AutoSize = true });

            cmbStatus = new ComboBox { Font = new Font("Segoe UI", 10), Location = new System.Drawing.Point(0, 35), Size = new System.Drawing.Size(150, 27), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "All", "Pending", "Approved", "Rejected" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += (s, e) => FilterRequests();
            panelTop.Controls.Add(cmbStatus);

            var btnApprove = new Button
            {
                Text = "✅ Approve",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new System.Drawing.Size(120, 35),
                Location = new System.Drawing.Point(170, 23),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnApprove.FlatAppearance.BorderSize = 0;
            btnApprove.Click += btnApprove_Click;
            panelTop.Controls.Add(btnApprove);

            var btnReject = new Button
            {
                Text = "❌ Reject",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new System.Drawing.Size(120, 35),
                Location = new System.Drawing.Point(300, 23),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnReject.FlatAppearance.BorderSize = 0;
            btnReject.Click += btnReject_Click;
            panelTop.Controls.Add(btnReject);

            var btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new System.Drawing.Size(110, 35),
                Location = new System.Drawing.Point(430, 23),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadRequestsAsync();
            panelTop.Controls.Add(btnRefresh);

            dgvRequests = new DataGridView
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

            dgvRequests.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvRequests.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRequests.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvRequests.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 249, 249);
            dgvRequests.RowTemplate.Height = 35;
            dgvRequests.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);

            dgvRequests.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", Visible = false });
            dgvRequests.Columns.Add(new DataGridViewTextBoxColumn { Name = "Username", HeaderText = "Username", DataPropertyName = "Username", FillWeight = 20 });
            dgvRequests.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email", DataPropertyName = "Email", FillWeight = 30 });
            dgvRequests.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "StatusDisplay", FillWeight = 15 });
            dgvRequests.Columns.Add(new DataGridViewTextBoxColumn { Name = "CreatedAt", HeaderText = "Requested At", DataPropertyName = "FormattedDate", FillWeight = 15 });
            dgvRequests.Columns.Add(new DataGridViewTextBoxColumn { Name = "RejectedReason", HeaderText = "Reason", DataPropertyName = "RejectedReason", FillWeight = 20 });

            this.Controls.Add(dgvRequests);
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

        private async void LoadRequestsAsync()
        {
            try
            {
                var response = await CreateAuthorizedClient().GetAsync("OrganizerRequests");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                allRequests = JsonSerializer.Deserialize<List<OrganizerRequestListItem>>(json, options) ?? new();
                FilterRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading requests:\n{ex.Message}", "Error");
            }
        }

        private void FilterRequests()
        {
            var filtered = new List<OrganizerRequestListItem>(allRequests);
            if (cmbStatus?.SelectedIndex > 0)
            {
                var status = cmbStatus.SelectedItem?.ToString();
                filtered = filtered.Where(r => r.Status == status).ToList();
            }
            dgvRequests!.DataSource = null;
            dgvRequests.DataSource = filtered;
        }

        private async void btnApprove_Click(object? sender, EventArgs e)
        {
            if (dgvRequests?.CurrentRow == null) return;
            var id = (int)dgvRequests.CurrentRow.Cells["Id"].Value;
            var username = dgvRequests.CurrentRow.Cells["Username"].Value?.ToString();
            var status = dgvRequests.CurrentRow.Cells["Status"].Value?.ToString();

            if (status != "⏳ Pending")
            {
                MessageBox.Show("Only pending requests can be approved.", "Info");
                return;
            }

            if (MessageBox.Show($"Approve organizer request from '{username}'?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                var response = await CreateAuthorizedClient().PostAsync($"OrganizerRequests/{id}/approve",
                    new StringContent("{}", Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                MessageBox.Show($"{username} is now an organizer!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadRequestsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error");
            }
        }

        private async void btnReject_Click(object? sender, EventArgs e)
        {
            if (dgvRequests?.CurrentRow == null) return;
            var id = (int)dgvRequests.CurrentRow.Cells["Id"].Value;
            var username = dgvRequests.CurrentRow.Cells["Username"].Value?.ToString();
            var status = dgvRequests.CurrentRow.Cells["Status"].Value?.ToString();

            if (status != "⏳ Pending")
            {
                MessageBox.Show("Only pending requests can be rejected.", "Info");
                return;
            }

            var reason = Microsoft.VisualBasic.Interaction.InputBox(
                $"Reason for rejecting '{username}'s request (optional):",
                "Reject Request", "");

            try
            {
                var body = JsonSerializer.Serialize(new { reason });
                var response = await CreateAuthorizedClient().PostAsync($"OrganizerRequests/{id}/reject",
                    new StringContent(body, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                MessageBox.Show("Request rejected.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadRequestsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error");
            }
        }
    }

    public class OrganizerRequestListItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string Status { get; set; } = "Pending";
        public string StatusDisplay => Status switch
        {
            "Pending" => "⏳ Pending",
            "Approved" => "✅ Approved",
            "Rejected" => "❌ Rejected",
            _ => Status
        };
        public DateTime CreatedAt { get; set; }
        public string FormattedDate => CreatedAt.ToString("dd.MM.yyyy HH:mm");
        public string? RejectedReason { get; set; }
    }
}