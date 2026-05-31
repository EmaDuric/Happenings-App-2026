using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows.Forms;

namespace Happenings.WinUI.Forms.Reservations
{
    public class frmReservationsList : UserControl
    {
        private List<ReservationListItem> allReservations = new();
        private readonly APIService _apiService;

        private Panel? panelTop;
        private TextBox? txtSearch;
        private ComboBox? cmbStatus;
        private DataGridView? dgvReservations;

        public frmReservationsList()
        {
            _apiService = (APIService)Program.ServiceProvider.GetService(typeof(APIService))!;
            InitializeComponent();
            LoadReservationsAsync();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.Padding = new Padding(20);

            panelTop = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.Transparent };

            panelTop.Controls.Add(new Label { Text = "Search:", Font = new Font("Segoe UI", 10), Location = new System.Drawing.Point(0, 15), AutoSize = true });

            txtSearch = new TextBox { Font = new Font("Segoe UI", 10), Location = new System.Drawing.Point(0, 35), Size = new System.Drawing.Size(250, 27) };
            txtSearch.TextChanged += (s, e) => FilterReservations();
            panelTop.Controls.Add(txtSearch);

            panelTop.Controls.Add(new Label { Text = "Status:", Font = new Font("Segoe UI", 10), Location = new System.Drawing.Point(270, 15), AutoSize = true });

            cmbStatus = new ComboBox { Font = new Font("Segoe UI", 10), Location = new System.Drawing.Point(270, 35), Size = new System.Drawing.Size(150, 27), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "All", "Pending", "Approved", "Rejected", "Cancelled" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += (s, e) => FilterReservations();
            panelTop.Controls.Add(cmbStatus);

            var btnApprove = new Button { Text = "✅ Approve", Font = new Font("Segoe UI", 10, FontStyle.Bold), Size = new System.Drawing.Size(120, 35), Location = new System.Drawing.Point(440, 25), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnApprove.FlatAppearance.BorderSize = 0;
            btnApprove.Click += btnApprove_Click;
            panelTop.Controls.Add(btnApprove);

            var btnReject = new Button { Text = "❌ Reject", Font = new Font("Segoe UI", 10, FontStyle.Bold), Size = new System.Drawing.Size(120, 35), Location = new System.Drawing.Point(570, 25), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnReject.FlatAppearance.BorderSize = 0;
            btnReject.Click += btnReject_Click;
            panelTop.Controls.Add(btnReject);

            dgvReservations = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false, // ← KLJUČNO: ne generiše automatski sve kolone
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10),
                ColumnHeadersHeight = 40
            };

            dgvReservations.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvReservations.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvReservations.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvReservations.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvReservations.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            dgvReservations.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 249, 249);
            dgvReservations.RowTemplate.Height = 35;
            dgvReservations.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);

            // ID sakriven — samo za interne akcije
            dgvReservations.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", Visible = false });
            dgvReservations.Columns.Add(new DataGridViewTextBoxColumn { Name = "UserName", HeaderText = "User", DataPropertyName = "UserName", FillWeight = 15 });
            dgvReservations.Columns.Add(new DataGridViewTextBoxColumn { Name = "EventName", HeaderText = "Event", DataPropertyName = "EventName", FillWeight = 20 });
            dgvReservations.Columns.Add(new DataGridViewTextBoxColumn { Name = "EventDate", HeaderText = "Event Date", DataPropertyName = "FormattedEventDate", FillWeight = 12 });
            dgvReservations.Columns.Add(new DataGridViewTextBoxColumn { Name = "TicketTypeName", HeaderText = "Ticket Type", DataPropertyName = "TicketTypeName", FillWeight = 12 });
            dgvReservations.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Qty", DataPropertyName = "Quantity", FillWeight = 6 });
            dgvReservations.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", FillWeight = 10 });
            dgvReservations.Columns.Add(new DataGridViewTextBoxColumn { Name = "ReservedAt", HeaderText = "Reserved At", DataPropertyName = "FormattedDate", FillWeight = 12 });

            this.Controls.Add(dgvReservations);
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

        private async void LoadReservationsAsync()
        {
            try
            {
                var response = await CreateAuthorizedClient().GetAsync("Reservations");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                allReservations = JsonSerializer.Deserialize<List<ReservationListItem>>(json, options) ?? new();

                dgvReservations!.DataSource = null;
                dgvReservations.DataSource = allReservations;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading reservations:\n{ex.Message}", "Error");
            }
        }

        private void FilterReservations()
        {
            var filtered = new List<ReservationListItem>(allReservations);

            if (!string.IsNullOrWhiteSpace(txtSearch?.Text))
            {
                var searchText = txtSearch.Text.ToLower();
                filtered = filtered.Where(r =>
                    (r.UserName?.ToLower().Contains(searchText) ?? false) ||
                    (r.EventName?.ToLower().Contains(searchText) ?? false)).ToList();
            }

            if (cmbStatus?.SelectedIndex > 0)
            {
                var status = cmbStatus.SelectedItem?.ToString();
                filtered = filtered.Where(r => r.Status == status).ToList();
            }

            dgvReservations!.DataSource = null;
            dgvReservations.DataSource = filtered;
        }

        private async void btnApprove_Click(object? sender, EventArgs e)
        {
            if (dgvReservations?.CurrentRow == null) return;
            var id = (int)dgvReservations.CurrentRow.Cells["Id"].Value;
            try
            {
                var response = await CreateAuthorizedClient().PostAsync($"Reservations/{id}/approve", null);
                response.EnsureSuccessStatusCode();
                MessageBox.Show("Reservation approved!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadReservationsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error approving reservation:\n{ex.Message}", "Error");
            }
        }

        private async void btnReject_Click(object? sender, EventArgs e)
        {
            if (dgvReservations?.CurrentRow == null) return;
            var id = (int)dgvReservations.CurrentRow.Cells["Id"].Value;
            try
            {
                var response = await CreateAuthorizedClient().PostAsync($"Reservations/{id}/reject", null);
                response.EnsureSuccessStatusCode();
                MessageBox.Show("Reservation rejected!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadReservationsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error rejecting reservation:\n{ex.Message}", "Error");
            }
        }
    }

    public class ReservationListItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int EventId { get; set; }
        public string? EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string? TicketTypeName { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ReservedAt { get; set; }
        public string FormattedDate => ReservedAt.ToString("dd.MM.yyyy HH:mm");
        public string FormattedEventDate => EventDate.ToString("dd.MM.yyyy");
    }
}