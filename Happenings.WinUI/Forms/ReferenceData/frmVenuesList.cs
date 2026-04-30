using Happenings.WinUI.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Happenings.WinUI.Forms.ReferenceData
{
    public class frmVenuesList : UserControl
    {
        private List<VenueViewModel> venues = new List<VenueViewModel>();
        private readonly APIService _apiService;

        private Panel? panelTop;
        private Button? btnAddVenue;
        private TextBox? txtSearch;
        private DataGridView? dgvVenues;

        public frmVenuesList()
        {
            _apiService = (APIService)Program.ServiceProvider.GetService(typeof(APIService))!;
            InitializeComponent();
            LoadVenuesAsync();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.Padding = new Padding(20);

            panelTop = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.Transparent };

            btnAddVenue = new Button
            {
                Text = "➕ Add New Venue",
                Font = new Font(new FontFamily("Segoe UI"), 11, FontStyle.Bold),
                Size = new Size(180, 40),
                Location = new Point(0, 10),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAddVenue.FlatAppearance.BorderSize = 0;
            btnAddVenue.Click += btnAddVenue_Click;
            panelTop.Controls.Add(btnAddVenue);

            var lblSearch = new Label
            {
                Text = "Search:",
                Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular),
                Location = new Point(200, 15),
                AutoSize = true
            };
            panelTop.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular),
                Location = new Point(200, 35),
                Size = new Size(250, 27)
            };
            txtSearch.TextChanged += (s, e) => FilterVenues();
            panelTop.Controls.Add(txtSearch);

            dgvVenues = new DataGridView
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

            dgvVenues.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvVenues.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvVenues.ColumnHeadersDefaultCellStyle.Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Bold);
            dgvVenues.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvVenues.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            dgvVenues.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 249, 249);
            dgvVenues.RowTemplate.Height = 35;
            dgvVenues.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);

            dgvVenues.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", Visible = false });
            dgvVenues.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Venue Name", DataPropertyName = "Name" });
            dgvVenues.Columns.Add(new DataGridViewTextBoxColumn { Name = "Address", HeaderText = "Address", DataPropertyName = "Address" });
            dgvVenues.Columns.Add(new DataGridViewTextBoxColumn { Name = "City", HeaderText = "City", DataPropertyName = "City", Width = 120 });
            dgvVenues.Columns.Add(new DataGridViewButtonColumn { Name = "Edit", HeaderText = "Edit", Text = "✏️ Edit", UseColumnTextForButtonValue = true, Width = 100 });
            dgvVenues.Columns.Add(new DataGridViewButtonColumn { Name = "Delete", HeaderText = "Delete", Text = "🗑️ Delete", UseColumnTextForButtonValue = true, Width = 100 });

            dgvVenues.CellContentClick += dgvVenues_CellContentClick;

            this.Controls.Add(dgvVenues);
            this.Controls.Add(panelTop);
        }

        private async void LoadVenuesAsync()
        {
            try
            {
                var dtos = await _apiService.GetLocationsAsync();
                venues = dtos.Select(d => new VenueViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Address = d.Address,
                    City = d.City,
                    IsActive = true
                }).ToList();

                dgvVenues!.DataSource = null;
                dgvVenues.DataSource = venues;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading venues:\n{ex.Message}", "Error");
            }
        }

        private void FilterVenues()
        {
            if (string.IsNullOrWhiteSpace(txtSearch?.Text))
            {
                dgvVenues!.DataSource = null;
                dgvVenues.DataSource = venues;
                return;
            }

            var searchText = txtSearch.Text.ToLower();
            var filtered = venues.Where(v =>
                v.Name.ToLower().Contains(searchText) ||
                v.Address.ToLower().Contains(searchText) ||
                v.City.ToLower().Contains(searchText)).ToList();

            dgvVenues!.DataSource = null;
            dgvVenues.DataSource = filtered;
        }

        private async void btnAddVenue_Click(object? sender, EventArgs e)
        {
            var editForm = new frmVenueEdit();
            if (editForm.ShowDialog() == DialogResult.OK && editForm.Venue != null)
            {
                try
                {
                    await _apiService.CreateLocationAsync(new LocationInsertRequest
                    {
                        Name = editForm.Venue.Name,
                        Address = editForm.Venue.Address,
                        City = editForm.Venue.City
                    });
                    LoadVenuesAsync();
                    MessageBox.Show("Venue added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating venue:\n{ex.Message}", "Error");
                }
            }
        }

        private async void dgvVenues_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var venueId = (int)dgvVenues!.Rows[e.RowIndex].Cells["Id"].Value;
            var venue = venues.FirstOrDefault(v => v.Id == venueId);
            if (venue == null) return;

            if (dgvVenues.Columns[e.ColumnIndex].Name == "Edit")
            {
                var editForm = new frmVenueEdit(venue);
                if (editForm.ShowDialog() == DialogResult.OK && editForm.Venue != null)
                {
                    try
                    {
                        await _apiService.UpdateLocationAsync(venueId, new LocationUpdateRequest
                        {
                            Name = editForm.Venue.Name,
                            Address = editForm.Venue.Address,
                            City = editForm.Venue.City
                        });
                        LoadVenuesAsync();
                        MessageBox.Show("Venue updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error updating venue:\n{ex.Message}", "Error");
                    }
                }
            }
            else if (dgvVenues.Columns[e.ColumnIndex].Name == "Delete")
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete '{venue.Name}'?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        await _apiService.DeleteLocationAsync(venueId);
                        LoadVenuesAsync();
                        MessageBox.Show("Venue deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting venue:\n{ex.Message}", "Error");
                    }
                }
            }
        }
    }
}