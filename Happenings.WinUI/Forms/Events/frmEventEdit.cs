using Happenings.WinUI.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Happenings.WinUI.Forms.Events
{
    public class frmEventEdit : Form
    {
        public EventViewModel? Event { get; private set; }
        private bool isEditMode = false;

        private Label? lblTitle;
        private TextBox? txtName;
        private TextBox? txtDescription;
        private DateTimePicker? dtpStartDate;
        private DateTimePicker? dtpStartTime;
        private DateTimePicker? dtpEndDate;
        private DateTimePicker? dtpEndTime;
        private NumericUpDown? nudPrice;
        private NumericUpDown? nudTotalTickets;
        private NumericUpDown? nudAvailableTickets;
        private ComboBox? cmbStatus;
        private Button? btnSave;
        private Button? btnCancel;
        private ComboBox? cmbCategory;
        private ComboBox? cmbVenue;
        private ComboBox? cmbOrganizer;

        private List<CategoryViewModel> _categories = new();
        private List<VenueViewModel> _venues = new();
        private List<OrganizerViewModel> _organizers = new();

        private readonly APIService _apiService;

        public frmEventEdit(EventViewModel? eventToEdit = null)
        {
            _apiService = (APIService)Program.ServiceProvider.GetService(typeof(APIService))!;

            if (eventToEdit != null)
            {
                Event = eventToEdit;
                isEditMode = true;
            }
            else
            {
                Event = new EventViewModel
                {
                    StartDateTime = DateTime.UtcNow.AddDays(7),
                    EndDateTime = DateTime.UtcNow.AddDays(7).AddHours(2),
                    Status = "Active",
                    AvailableTickets = 100,
                    TotalTickets = 100
                };
            }

            InitializeComponent();
            this.Load += async (s, e) => await LoadDropdownDataAsync();
        }

        private void InitializeComponent()
        {
            this.Text = isEditMode ? "Edit Event" : "Add New Event";
            this.Size = new Size(620, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            int y = 20, left = 30, labelW = 150, ctrlW = 400;

            lblTitle = new Label
            {
                Text = isEditMode ? "Edit Event" : "Create New Event",
                Font = new Font(new FontFamily("Segoe UI"), 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(left, y),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);
            y += 50;

            AddLabel("Event Name:", left, y);
            txtName = new TextBox { Location = new Point(left + labelW, y - 5), Size = new Size(ctrlW, 27), Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular) };
            this.Controls.Add(txtName);
            y += 40;

            AddLabel("Description:", left, y);
            txtDescription = new TextBox { Location = new Point(left + labelW, y - 5), Size = new Size(ctrlW, 60), Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular), Multiline = true };
            this.Controls.Add(txtDescription);
            y += 75;

            AddLabel("Start Date & Time:", left, y);
            dtpStartDate = new DateTimePicker { Location = new Point(left + labelW, y - 5), Size = new Size(200, 27), Format = DateTimePickerFormat.Short, Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular) };
            this.Controls.Add(dtpStartDate);
            dtpStartTime = new DateTimePicker { Location = new Point(left + labelW + 210, y - 5), Size = new Size(190, 27), Format = DateTimePickerFormat.Time, ShowUpDown = true, Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular) };
            this.Controls.Add(dtpStartTime);
            y += 40;

            AddLabel("End Date & Time:", left, y);
            dtpEndDate = new DateTimePicker { Location = new Point(left + labelW, y - 5), Size = new Size(200, 27), Format = DateTimePickerFormat.Short, Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular) };
            this.Controls.Add(dtpEndDate);
            dtpEndTime = new DateTimePicker { Location = new Point(left + labelW + 210, y - 5), Size = new Size(190, 27), Format = DateTimePickerFormat.Time, ShowUpDown = true, Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular) };
            this.Controls.Add(dtpEndTime);
            y += 40;

            AddLabel("Category:", left, y);
            cmbCategory = new ComboBox { Location = new Point(left + labelW, y - 5), Size = new Size(ctrlW, 27), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular) };
            cmbCategory.DisplayMember = "Name";
            cmbCategory.ValueMember = "Id";
            this.Controls.Add(cmbCategory);
            y += 40;

            AddLabel("Venue:", left, y);
            cmbVenue = new ComboBox { Location = new Point(left + labelW, y - 5), Size = new Size(ctrlW, 27), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular) };
            cmbVenue.DisplayMember = "Name";
            cmbVenue.ValueMember = "Id";
            this.Controls.Add(cmbVenue);
            y += 40;

            AddLabel("Price ($):", left, y);
            nudPrice = new NumericUpDown { Location = new Point(left + labelW, y - 5), Size = new Size(150, 27), DecimalPlaces = 2, Maximum = 10000, Minimum = 0, Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular) };
            this.Controls.Add(nudPrice);
            y += 40;

            AddLabel("Total Tickets:", left, y);
            nudTotalTickets = new NumericUpDown { Location = new Point(left + labelW, y - 5), Size = new Size(150, 27), Maximum = 100000, Minimum = 1, Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular) };
            this.Controls.Add(nudTotalTickets);
            y += 40;

            AddLabel("Available Tickets:", left, y);
            nudAvailableTickets = new NumericUpDown { Location = new Point(left + labelW, y - 5), Size = new Size(150, 27), Maximum = 100000, Minimum = 0, Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular) };
            this.Controls.Add(nudAvailableTickets);
            y += 40;

            AddLabel("Status:", left, y);
            cmbStatus = new ComboBox { Location = new Point(left + labelW, y - 5), Size = new Size(200, 27), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular) };
            cmbStatus.Items.AddRange(new object[] { "Active", "Completed", "Cancelled" });
            this.Controls.Add(cmbStatus);
            y += 40;

            AddLabel("Organizer:", left, y);
            cmbOrganizer = new ComboBox { Location = new Point(left + labelW, y - 5), Size = new Size(ctrlW, 27), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular) };
            cmbOrganizer.DisplayMember = "Name";
            cmbOrganizer.ValueMember = "Id";
            this.Controls.Add(cmbOrganizer);
            y += 50;

            btnCancel = new Button
            {
                Text = "Cancel",
                Font = new Font(new FontFamily("Segoe UI"), 11, FontStyle.Bold),
                Size = new Size(120, 40),
                Location = new Point(left + labelW + 150, y),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnCancel);

            btnSave = new Button
            {
                Text = isEditMode ? "Update Event" : "Create Event",
                Font = new Font(new FontFamily("Segoe UI"), 11, FontStyle.Bold),
                Size = new Size(150, 40),
                Location = new Point(left + labelW + 280, y),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += btnSave_Click;
            this.Controls.Add(btnSave);

            this.CancelButton = btnCancel;
        }

        private void AddLabel(string text, int x, int y)
        {
            this.Controls.Add(new Label
            {
                Text = text,
                Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(x, y),
                AutoSize = true
            });
        }

        private async System.Threading.Tasks.Task LoadDropdownDataAsync()
        {
            try
            {
                // Load categories
                var categoryDtos = await _apiService.GetCategoriesAsync();
                _categories = categoryDtos.Select(d => new CategoryViewModel { Id = d.Id, Name = d.Name, Description = d.Description ?? "", IsActive = true }).ToList();
                cmbCategory!.DataSource = _categories;
                cmbCategory.DisplayMember = "Name";
                cmbCategory.ValueMember = "Id";

                // Load venues
                var locationDtos = await _apiService.GetLocationsAsync();
                _venues = locationDtos.Select(d => new VenueViewModel { Id = d.Id, Name = d.Name, Address = d.Address, City = d.City, IsActive = true }).ToList();
                cmbVenue!.DataSource = _venues;
                cmbVenue.DisplayMember = "Name";
                cmbVenue.ValueMember = "Id";

                // Load organizers
                var organizerDtos = await _apiService.GetOrganizersAsync();
                _organizers = organizerDtos.Select(d => new OrganizerViewModel { Id = d.Id, Name = d.Name, IsActive = true }).ToList();
                cmbOrganizer!.DataSource = _organizers;
                cmbOrganizer.DisplayMember = "Name";
                cmbOrganizer.ValueMember = "Id";

                // Now populate fields
                LoadEventData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading form data:\n{ex.Message}", "Error");
            }
        }

        private void LoadEventData()
        {
            if (Event == null) return;

            txtName!.Text = Event.Name;
            txtDescription!.Text = Event.Description;

            var startDate = Event.StartDateTime > DateTime.MinValue ? Event.StartDateTime : DateTime.UtcNow.AddDays(7);
            dtpStartDate!.Value = startDate;
            dtpStartTime!.Value = startDate;

            var endDate = (Event.EndDateTime.HasValue && Event.EndDateTime.Value > DateTime.MinValue)
                ? Event.EndDateTime.Value
                : startDate.AddHours(2);
            dtpEndDate!.Value = endDate;
            dtpEndTime!.Value = endDate;

            if (Event.CategoryId > 0)
                cmbCategory!.SelectedValue = Event.CategoryId;
            else if (cmbCategory!.Items.Count > 0)
                cmbCategory.SelectedIndex = 0;

            if (Event.VenueId > 0)
                cmbVenue!.SelectedValue = Event.VenueId;
            else if (cmbVenue!.Items.Count > 0)
                cmbVenue.SelectedIndex = 0;

            nudPrice!.Value = Event.Price;
            nudTotalTickets!.Value = Math.Max(1, Event.TotalTickets);
            nudAvailableTickets!.Value = Math.Max(0, Event.AvailableTickets);

            cmbStatus!.SelectedItem = Event.Status;
            if (cmbStatus.SelectedIndex < 0)
                cmbStatus.SelectedIndex = 0;

            if (Event.OrganizerId > 0)
                cmbOrganizer!.SelectedValue = Event.OrganizerId;
            else if (cmbOrganizer!.Items.Count > 0)
                cmbOrganizer.SelectedIndex = 0;
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName?.Text))
            {
                MessageBox.Show("Please enter event name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName?.Focus();
                return;
            }

            if (cmbCategory?.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a category.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCategory?.Focus();
                return;
            }

            if (cmbVenue?.SelectedValue == null)
            {
                MessageBox.Show("Please select a venue.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbVenue?.Focus();
                return;
            }

            if (cmbOrganizer?.SelectedValue == null)
            {
                MessageBox.Show("Please select an organizer.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbOrganizer?.Focus();
                return;
            }

            if (nudAvailableTickets!.Value > nudTotalTickets!.Value)
            {
                MessageBox.Show("Available tickets cannot exceed total tickets.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nudAvailableTickets?.Focus();
                return;
            }

            if (Event != null)
            {
                Event.Name = txtName!.Text;
                Event.Description = txtDescription!.Text;

                Event.StartDateTime = dtpStartDate!.Value.Date.Add(dtpStartTime!.Value.TimeOfDay);
                Event.EndDateTime = dtpEndDate!.Value.Date.Add(dtpEndTime!.Value.TimeOfDay);

                Event.CategoryId = (int)(cmbCategory!.SelectedValue ?? 0);
                Event.VenueId = (int)(cmbVenue!.SelectedValue ?? 0);
                Event.OrganizerId = (int)(cmbOrganizer!.SelectedValue ?? 0);

                Event.Category = ((CategoryViewModel?)cmbCategory.SelectedItem)?.Name ?? "";
                Event.Venue = ((VenueViewModel?)cmbVenue.SelectedItem)?.Name ?? "";
                Event.Organizer = ((OrganizerViewModel?)cmbOrganizer.SelectedItem)?.Name ?? "";

                Event.Price = nudPrice!.Value;
                Event.TotalTickets = (int)nudTotalTickets!.Value;
                Event.AvailableTickets = (int)nudAvailableTickets!.Value;
                Event.Status = cmbStatus!.SelectedItem?.ToString() ?? "Active";
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}