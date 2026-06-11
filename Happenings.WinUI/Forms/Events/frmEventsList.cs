using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;
using Happenings.WinUI.Forms.Events;

namespace Happenings.WinUI.Forms.Events
{
    public class frmEventsList : UserControl
    {
        private List<EventListItem> allEvents = new();
        private List<EventListItem> filteredEvents = new();
        private readonly APIService _apiService;

        private Panel? panelTop;
        private Button? btnAddEvent;
        private TextBox? txtSearch;
        private ComboBox? cmbCategory;
        private DataGridView? dgvEvents;

        public frmEventsList()
        {
            _apiService = (APIService)Program.ServiceProvider.GetService(typeof(APIService))!;
            InitializeComponent();
            LoadEventsFromAPI();
        }

        private async void LoadEventsFromAPI()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                var httpClient = APIService.CreateAuthorizedClient();

                var response = await httpClient.GetAsync("Events");
                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"API returned {response.StatusCode}", "Error");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var pagedResult = JsonSerializer.Deserialize<PagedResult<EventApiDto>>(json, options);

                allEvents = (pagedResult?.Items ?? new()).Select(e => new EventListItem
                {
                    Id = e.Id,
                    Name = e.Name ?? "",
                    Description = e.Description ?? "",
                    EventDate = e.EventDate,
                    CategoryName = !string.IsNullOrEmpty(e.CategoryName) ? e.CategoryName
                                 : e.EventCategoryName ?? "Unknown",
                    EventCategoryId = e.EventCategoryId,
                    LocationName = e.LocationName ?? "Unknown",
                    LocationId = e.LocationId,
                    OrganizerId = e.OrganizerId
                }).ToList();

                FilterEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading events:\n{ex.Message}", "Error");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.Padding = new Padding(20);

            panelTop = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.Transparent };

            btnAddEvent = new Button
            {
                Text = "➕ Add New Event",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(180, 40),
                Location = new Point(0, 10),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAddEvent.FlatAppearance.BorderSize = 0;
            btnAddEvent.Click += btnAddEvent_Click;
            panelTop.Controls.Add(btnAddEvent);

            var lblSearch = new Label { Text = "Search:", Font = new Font("Segoe UI", 10), Location = new Point(200, 15), AutoSize = true };
            panelTop.Controls.Add(lblSearch);

            txtSearch = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(200, 35), Size = new Size(200, 27) };
            txtSearch.TextChanged += (s, e) => FilterEvents();
            panelTop.Controls.Add(txtSearch);

            var lblCategory = new Label { Text = "Category:", Font = new Font("Segoe UI", 10), Location = new Point(420, 15), AutoSize = true };
            panelTop.Controls.Add(lblCategory);

            cmbCategory = new ComboBox { Font = new Font("Segoe UI", 10), Location = new Point(420, 35), Size = new Size(150, 27), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategory.Items.Add("All");
            cmbCategory.SelectedIndex = 0;
            cmbCategory.SelectedIndexChanged += (s, e) => FilterEvents();
            panelTop.Controls.Add(cmbCategory);

            dgvEvents = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10),
                ColumnHeadersHeight = 40,
                AutoGenerateColumns = false
            };
            dgvEvents.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvEvents.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvEvents.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvEvents.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 249, 249);
            dgvEvents.RowTemplate.Height = 35;

            dgvEvents.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", Visible = false });
            dgvEvents.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Event Name", DataPropertyName = "Name" });
            dgvEvents.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Category", DataPropertyName = "CategoryName", Width = 120 });
            dgvEvents.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Location", DataPropertyName = "LocationName", Width = 150 });
            dgvEvents.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Date", DataPropertyName = "FormattedDate", Width = 120 });

            var btnCol = new DataGridViewButtonColumn { Name = "Actions", HeaderText = "Actions", Text = "⚙ Actions", UseColumnTextForButtonValue = true, Width = 100 };
            dgvEvents.Columns.Add(btnCol);
            dgvEvents.CellContentClick += dgvEvents_CellContentClick;

            this.Controls.Add(dgvEvents);
            this.Controls.Add(panelTop);
        }

        private void FilterEvents()
        {
            filteredEvents = new List<EventListItem>(allEvents);

            if (!string.IsNullOrWhiteSpace(txtSearch?.Text))
            {
                var s = txtSearch.Text.ToLower();
                filteredEvents = filteredEvents.Where(e =>
                    e.Name.ToLower().Contains(s) ||
                    e.LocationName.ToLower().Contains(s)).ToList();
            }

            if (cmbCategory?.SelectedIndex > 0)
            {
                var cat = cmbCategory.SelectedItem?.ToString();
                filteredEvents = filteredEvents.Where(e => e.CategoryName == cat).ToList();
            }

            dgvEvents!.DataSource = null;
            dgvEvents.DataSource = filteredEvents;
        }

        private async void btnAddEvent_Click(object? sender, EventArgs e)
        {
            var editForm = new frmEventEdit();
            if (editForm.ShowDialog() != DialogResult.OK) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                var ev = editForm.Event!;
                var request = new EventInsertRequest
                {
                    Name = ev.Name,
                    Description = ev.Description,
                    EventDate = ev.EventDate,
                    EventCategoryId = ev.EventCategoryId,
                    LocationId = ev.LocationId
                };
                await _apiService.CreateEventAsync(request);
                LoadEventsFromAPI();
                MessageBox.Show("Event created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating event:\n{ex.Message}", "Error");
            }
            finally { Cursor = Cursors.Default; }
        }

        private void dgvEvents_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvEvents!.Columns[e.ColumnIndex].Name != "Actions") return;
            var id = (int)dgvEvents.Rows[e.RowIndex].Cells["Id"].Value;
            ShowEventActions(id);
        }

        private async void ShowEventActions(int eventId)
        {
            var selected = allEvents.FirstOrDefault(e => e.Id == eventId);
            if (selected == null) return;

            var result = MessageBox.Show(
                $"Event: {selected.Name}\n\nYes - Edit | No - Delete | Cancel - Close",
                "Event Actions", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                var model = new EventEditModel
                {
                    Id = selected.Id,
                    Name = selected.Name,
                    Description = selected.Description,
                    EventDate = selected.EventDate,
                    EventCategoryId = selected.EventCategoryId,
                    LocationId = selected.LocationId
                };

                var editForm = new frmEventEdit(model);
                if (editForm.ShowDialog() != DialogResult.OK) return;

                try
                {
                    Cursor = Cursors.WaitCursor;
                    var ev = editForm.Event!;
                    var request = new EventUpdateRequest
                    {
                        Name = ev.Name,
                        Description = ev.Description,
                        EventDate = ev.EventDate,
                        EventCategoryId = ev.EventCategoryId,
                        LocationId = ev.LocationId
                    };
                    await _apiService.UpdateEventAsync(eventId, request);
                    LoadEventsFromAPI();
                    MessageBox.Show("Event updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating event:\n{ex.Message}", "Error");
                }
                finally { Cursor = Cursors.Default; }
            }
            else if (result == DialogResult.No)
            {
                if (MessageBox.Show($"Delete '{selected.Name}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        Cursor = Cursors.WaitCursor;
                        await _apiService.DeleteEventAsync(eventId);
                        allEvents.RemoveAll(e => e.Id == eventId);
                        FilterEvents();
                        MessageBox.Show("Event deleted!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting event:\n{ex.Message}", "Error");
                    }
                    finally { Cursor = Cursors.Default; }
                }
            }
        }
    }

    public class EventListItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int EventCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public int OrganizerId { get; set; }
        public string FormattedDate => EventDate.ToString("dd.MM.yyyy");
    }

    public class EventApiDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public int EventCategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? EventCategoryName { get; set; }
        public int LocationId { get; set; }
        public string? LocationName { get; set; }
        public int OrganizerId { get; set; }
        public string? OrganizerName { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class PagedResult<T>
    {
        public int TotalCount { get; set; }
        public List<T> Items { get; set; } = new();
    }
}