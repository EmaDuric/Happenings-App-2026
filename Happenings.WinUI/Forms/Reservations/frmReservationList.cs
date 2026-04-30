using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
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
                Size = new System.Drawing.Size(250, 27)
            };
            txtSearch.TextChanged += (s, e) => FilterReservations();
            panelTop.Controls.Add(txtSearch);

            var lblStatus = new Label
            {
                Text = "Status:",
                Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular),
                Location = new System.Drawing.Point(270, 15),
                AutoSize = true
            };
            panelTop.Controls.Add(lblStatus);

            cmbStatus = new ComboBox
            {
                Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular),
                Location = new System.Drawing.Point(270, 35),
                Size = new System.Drawing.Size(150, 27),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new object[] { "All", "Pending", "Approved", "Rejected", "Cancelled" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += (s, e) => FilterReservations();
            panelTop.Controls.Add(cmbStatus);

            // Approve/Reject buttons
            var btnApprove = new Button
            {
                Text = "✅ Approve",
                Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Bold),
                Size = new System.Drawing.Size(120, 35),
                Location = new System.Drawing.Point(440, 25),
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
                Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Bold),
                Size = new System.Drawing.Size(120, 35),
                Location = new System.Drawing.Point(570, 25),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
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
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular),
                ColumnHeadersHeight = 40
            };

            dgvReservations.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvReservations.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvReservations.ColumnHeadersDefaultCellStyle.Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Bold);
            dgvReservations.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvReservations.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            dgvReservations.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 249, 249);
            dgvReservations.RowTemplate.Height = 35;
            dgvReservations.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);

            dgvReservations.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", Width = 60 });
            dgvReservations.Columns.Add(new DataGridViewTextBoxColumn { Name = "UserId", HeaderText = "User ID", DataPropertyName = "UserId", Width = 80 });
            dgvReservations.Columns.Add(new DataGridViewTextBoxColumn { Name = "EventId", HeaderText = "Event ID", DataPropertyName = "EventId", Width = 80 });
            dgvReservations.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Qty", DataPropertyName = "Quantity", Width = 60 });
            dgvReservations.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", Width = 120 });
            dgvReservations.Columns.Add(new DataGridViewTextBoxColumn { Name = "ReservedAt", HeaderText = "Reserved At", DataPropertyName = "FormattedDate", Width = 150 });

            this.Controls.Add(dgvReservations);
            this.Controls.Add(panelTop);
        }

        private async void LoadReservationsAsync()
        {
            try
            {
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("http://localhost:5000/api/");
                var response = await httpClient.GetAsync("Reservations");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                allReservations = JsonSerializer.Deserialize<List<ReservationListItem>>(json, options) ?? new List<ReservationListItem>();

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
                    r.UserId.ToString().Contains(searchText) ||
                    r.EventId.ToString().Contains(searchText)).ToList();
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
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("http://localhost:5000/api/");
                var response = await httpClient.PostAsync($"Reservations/{id}/approve", null);
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
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("http://localhost:5000/api/");
                var response = await httpClient.PostAsync($"Reservations/{id}/reject", null);
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
        public int EventId { get; set; }
        public int EventTicketTypeId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ReservedAt { get; set; }
        public string FormattedDate => ReservedAt.ToString("dd.MM.yyyy HH:mm");
    }
}