using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Happenings.WinUI.Forms.Events
{
    public class frmEventEdit : Form
    {
        public EventEditModel? Event { get; private set; }
        private bool isEditMode = false;

        private TextBox? txtName;
        private TextBox? txtDescription;
        private DateTimePicker? dtpEventDate;
        private ComboBox? cmbCategory;
        private ComboBox? cmbVenue;
        private Button? btnSave;
        private Button? btnCancel;

        private List<CategoryDto> _categories = new();
        private List<LocationDto> _venues = new();

        private readonly APIService _apiService;

        public frmEventEdit(EventEditModel? eventToEdit = null)
        {
            _apiService = (APIService)Program.ServiceProvider.GetService(typeof(APIService))!;

            if (eventToEdit != null)
            {
                Event = eventToEdit;
                isEditMode = true;
            }
            else
            {
                Event = new EventEditModel
                {
                    EventDate = DateTime.Now.AddDays(7)
                };
            }

            InitializeComponent();
            this.Load += async (s, e) => await LoadDropdownDataAsync();
        }

        private void InitializeComponent()
        {
            this.Text = isEditMode ? "Edit Event" : "Add New Event";
            this.Size = new Size(560, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            int y = 20, left = 30, labelW = 130, ctrlW = 350;

            AddLabel(isEditMode ? "Edit Event" : "Create New Event", left, y, 16, true);
            y += 50;

            AddLabel("Event Name:", left, y);
            txtName = new TextBox { Location = new Point(left + labelW, y - 5), Size = new Size(ctrlW, 27), Font = new Font("Segoe UI", 10) };
            this.Controls.Add(txtName);
            y += 40;

            AddLabel("Description:", left, y);
            txtDescription = new TextBox { Location = new Point(left + labelW, y - 5), Size = new Size(ctrlW, 60), Font = new Font("Segoe UI", 10), Multiline = true };
            this.Controls.Add(txtDescription);
            y += 75;

            AddLabel("Event Date:", left, y);
            dtpEventDate = new DateTimePicker { Location = new Point(left + labelW, y - 5), Size = new Size(220, 27), Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 10) };
            this.Controls.Add(dtpEventDate);
            y += 40;

            AddLabel("Category:", left, y);
            cmbCategory = new ComboBox { Location = new Point(left + labelW, y - 5), Size = new Size(ctrlW, 27), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cmbCategory.DisplayMember = "Name";
            cmbCategory.ValueMember = "Id";
            this.Controls.Add(cmbCategory);
            y += 40;

            AddLabel("Location:", left, y);
            cmbVenue = new ComboBox { Location = new Point(left + labelW, y - 5), Size = new Size(ctrlW, 27), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cmbVenue.DisplayMember = "Name";
            cmbVenue.ValueMember = "Id";
            this.Controls.Add(cmbVenue);
            y += 55;

            btnCancel = new Button
            {
                Text = "Cancel",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(120, 40),
                Location = new Point(left + labelW + 100, y),
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
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(150, 40),
                Location = new Point(left + labelW + 230, y),
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

        private void AddLabel(string text, int x, int y, int fontSize = 10, bool bold = false)
        {
            this.Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", fontSize, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(x, y),
                AutoSize = true
            });
        }

        private async System.Threading.Tasks.Task LoadDropdownDataAsync()
        {
            try
            {
                var categoryDtos = await _apiService.GetCategoriesAsync();
                _categories = categoryDtos;
                cmbCategory!.DataSource = _categories;
                cmbCategory.DisplayMember = "Name";
                cmbCategory.ValueMember = "Id";

                var locationDtos = await _apiService.GetLocationsAsync();
                _venues = locationDtos;
                cmbVenue!.DataSource = _venues;
                cmbVenue.DisplayMember = "Name";
                cmbVenue.ValueMember = "Id";

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
            dtpEventDate!.Value = Event.EventDate > DateTime.MinValue ? Event.EventDate : DateTime.Now.AddDays(7);

            if (Event.EventCategoryId > 0)
                cmbCategory!.SelectedValue = Event.EventCategoryId;
            else if (cmbCategory!.Items.Count > 0)
                cmbCategory.SelectedIndex = 0;

            if (Event.LocationId > 0)
                cmbVenue!.SelectedValue = Event.LocationId;
            else if (cmbVenue!.Items.Count > 0)
                cmbVenue.SelectedIndex = 0;
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName?.Text))
            {
                MessageBox.Show("Please enter event name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName?.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDescription?.Text))
            {
                MessageBox.Show("Please enter description.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDescription?.Focus();
                return;
            }

            if (cmbCategory?.SelectedValue == null)
            {
                MessageBox.Show("Please select a category.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbVenue?.SelectedValue == null)
            {
                MessageBox.Show("Please select a location.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Event = new EventEditModel
            {
                Id = Event?.Id ?? 0,
                Name = txtName!.Text.Trim(),
                Description = txtDescription!.Text.Trim(),
                EventDate = dtpEventDate!.Value,
                EventCategoryId = (int)(cmbCategory!.SelectedValue ?? 0),
                LocationId = (int)(cmbVenue!.SelectedValue ?? 0),
                CategoryName = ((CategoryDto?)cmbCategory.SelectedItem)?.Name ?? "",
                LocationName = ((LocationDto?)cmbVenue.SelectedItem)?.Name ?? ""
            };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }

    public class EventEditModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int EventCategoryId { get; set; }
        public int LocationId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
    }
}