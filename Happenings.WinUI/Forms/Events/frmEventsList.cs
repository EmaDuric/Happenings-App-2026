using Happenings.WinUI.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Text.Json;
using System.Net.Http;

namespace Happenings.WinUI.Forms.Events
{
    public class frmEventsList : UserControl
    {
        private List<EventViewModel> allEvents = new List<EventViewModel>();
        private List<EventViewModel> filteredEvents = new List<EventViewModel>();
        private readonly APIService _apiService;

        private Panel? panelTop;
        private Button? btnAddEvent;
        private TextBox? txtSearch;
        private ComboBox? cmbCategory;
        private ComboBox? cmbStatus;
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

                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("http://localhost:5000/api/");

                var response = await httpClient.GetAsync("Events");

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"API returned {response.StatusCode}", "API Error");
                    allEvents = new List<EventViewModel>();
                    FilterEvents();
                    return;
                }

                var jsonString = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var pagedResult = JsonSerializer.Deserialize<PagedResult<EventDto>>(jsonString, options);
                var apiEvents = pagedResult?.Items;

                if (apiEvents == null || apiEvents.Count == 0)
                {
                    allEvents = new List<EventViewModel>();
                    FilterEvents();
                    return;
                }

                allEvents = apiEvents.Select(e => new EventViewModel
                {
                    Id = e.Id,
                    Name = e.Name ?? "No Name",
                    Description = e.Description ?? "",
                    StartDateTime = e.StartDate > DateTime.MinValue ? e.StartDate : e.EventDate,
                    EndDateTime = e.EndDate,
                    CategoryId = e.EventCategoryId,
                    Category = !string.IsNullOrEmpty(e.CategoryName) ? e.CategoryName
                             : !string.IsNullOrEmpty(e.EventCategoryName) ? e.EventCategoryName
                             : "Unknown",
                    VenueId = e.LocationId,
                    Venue = e.LocationName ?? "Unknown",
                    OrganizerId = e.OrganizerId,
                    Organizer = e.OrganizerName ?? "Unknown",
                    Price = e.Price,
                    TotalTickets = e.TotalTickets,
                    AvailableTickets = e.AvailableTickets,
                    Status = string.IsNullOrEmpty(e.Status) ? "Active" : e.Status
                }).ToList();

                FilterEvents();
            }
            catch (JsonException jex)
            {
                MessageBox.Show($"JSON Deserialization Error:\n{jex.Message}", "JSON Error");
                allEvents = new List<EventViewModel>();
                FilterEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error:\n{ex.Message}\n\nInner:\n{ex.InnerException?.Message}", "Error");
                allEvents = new List<EventViewModel>();
                FilterEvents();
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

            panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.Transparent
            };

            btnAddEvent = new Button
            {
                Text = "➕ Add New Event",
                Font = new Font(new FontFamily("Segoe UI"), 11, FontStyle.Bold),
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
                Size = new Size(200, 27)
            };
            txtSearch.TextChanged += (s, e) => FilterEvents();
            panelTop.Controls.Add(txtSearch);

            var lblCategory = new Label
            {
                Text = "Category:",
                Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular),
                Location = new Point(420, 15),
                AutoSize = true
            };
            panelTop.Controls.Add(lblCategory);

            cmbCategory = new ComboBox
            {
                Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular),
                Location = new Point(420, 35),
                Size = new Size(150, 27),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCategory.Items.AddRange(new object[] { "All", "Music", "Technology", "Sport", "Conference", "Theater" });
            cmbCategory.SelectedIndex = 0;
            cmbCategory.SelectedIndexChanged += (s, e) => FilterEvents();
            panelTop.Controls.Add(cmbCategory);

            var lblStatus = new Label
            {
                Text = "Status:",
                Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular),
                Location = new Point(590, 15),
                AutoSize = true
            };
            panelTop.Controls.Add(lblStatus);

            cmbStatus = new ComboBox
            {
                Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular),
                Location = new Point(590, 35),
                Size = new Size(150, 27),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new object[] { "All", "Active", "Completed", "Cancelled" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += (s, e) => FilterEvents();
            panelTop.Controls.Add(cmbStatus);

            dgvEvents = new DataGridView
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

            dgvEvents.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvEvents.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvEvents.ColumnHeadersDefaultCellStyle.Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Bold);
            dgvEvents.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvEvents.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            dgvEvents.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 249, 249);
            dgvEvents.RowTemplate.Height = 35;
            dgvEvents.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);

            dgvEvents.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", Width = 50, Visible = false });
            dgvEvents.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Event Name", DataPropertyName = "Name" });
            dgvEvents.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Date & Time", DataPropertyName = "FormattedDate", Width = 150 });
            dgvEvents.Columns.Add(new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Category", DataPropertyName = "Category", Width = 120 });
            dgvEvents.Columns.Add(new DataGridViewTextBoxColumn { Name = "Venue", HeaderText = "Venue", DataPropertyName = "Venue", Width = 150 });
            dgvEvents.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "Price", DataPropertyName = "FormattedPrice", Width = 80 });
            dgvEvents.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tickets", HeaderText = "Tickets", DataPropertyName = "TicketInfo", Width = 100 });
            dgvEvents.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", Width = 100 });
            dgvEvents.Columns.Add(new DataGridViewButtonColumn { Name = "Actions", HeaderText = "Actions", Text = "View", UseColumnTextForButtonValue = true, Width = 80 });

            dgvEvents.CellContentClick += dgvEvents_CellContentClick;

            this.Controls.Add(dgvEvents);
            this.Controls.Add(panelTop);
        }

        private void FilterEvents()
        {
            filteredEvents = new List<EventViewModel>(allEvents);

            if (!string.IsNullOrWhiteSpace(txtSearch?.Text))
            {
                var searchText = txtSearch.Text.ToLower();
                filteredEvents = filteredEvents.Where(e =>
                    e.Name.ToLower().Contains(searchText) ||
                    e.Venue.ToLower().Contains(searchText) ||
                    e.Description.ToLower().Contains(searchText)
                ).ToList();
            }

            if (cmbCategory?.SelectedIndex > 0)
            {
                var category = cmbCategory.SelectedItem?.ToString();
                filteredEvents = filteredEvents.Where(e => e.Category == category).ToList();
            }

            if (cmbStatus?.SelectedIndex > 0)
            {
                var status = cmbStatus.SelectedItem?.ToString();
                filteredEvents = filteredEvents.Where(e => e.Status == status).ToList();
            }

            dgvEvents!.DataSource = null;
            dgvEvents.DataSource = filteredEvents;
        }

        private async void btnAddEvent_Click(object? sender, EventArgs e)
        {
            var editForm = new frmEventEdit();
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Cursor = Cursors.WaitCursor;

                    var newEvent = editForm.Event;

                    var request = new EventInsertRequest
                    {
                        Name = newEvent!.Name,
                        Description = newEvent.Description,
                        StartDate = newEvent.StartDateTime,
                        EndDate = newEvent.EndDateTime,
                        EventCategoryId = newEvent.CategoryId,
                        LocationId = newEvent.VenueId,
                        OrganizerId = newEvent.OrganizerId,
                        Price = newEvent.Price,
                        TotalTickets = newEvent.TotalTickets
                    };

                    await _apiService.CreateEventAsync(request);
                    LoadEventsFromAPI();

                    MessageBox.Show("Event added successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating event:\n{ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void dgvEvents_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvEvents!.Columns[e.ColumnIndex].Name == "Actions")
            {
                var eventId = (int)dgvEvents.Rows[e.RowIndex].Cells["Id"].Value;
                ShowEventActions(eventId);
            }
        }

        private async void ShowEventActions(int eventId)
        {
            var selectedEvent = allEvents.FirstOrDefault(e => e.Id == eventId);
            if (selectedEvent == null) return;

            var result = MessageBox.Show(
                $"Event: {selectedEvent.Name}\n\n" +
                $"What would you like to do?\n\n" +
                $"Yes - Edit Event\n" +
                $"No - Delete Event\n" +
                $"Cancel - View Details",
                "Event Actions",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                var editForm = new frmEventEdit(selectedEvent);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Cursor = Cursors.WaitCursor;

                        var updatedEvent = editForm.Event;

                        var request = new EventUpdateRequest
                        {
                            Name = updatedEvent!.Name,
                            Description = updatedEvent.Description,
                            StartDate = updatedEvent.StartDateTime,
                            EndDate = updatedEvent.EndDateTime,
                            EventCategoryId = updatedEvent.CategoryId,
                            LocationId = updatedEvent.VenueId,
                            OrganizerId = updatedEvent.OrganizerId,
                            Price = updatedEvent.Price,
                            TotalTickets = updatedEvent.TotalTickets,
                            AvailableTickets = updatedEvent.AvailableTickets,
                            Status = updatedEvent.Status
                        };

                        await _apiService.UpdateEventAsync(eventId, request);
                        LoadEventsFromAPI();

                        MessageBox.Show("Event updated successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error updating event:\n{ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                }
            }
            else if (result == DialogResult.No)
            {
                var confirmDelete = MessageBox.Show(
                    $"Are you sure you want to delete '{selectedEvent.Name}'?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirmDelete == DialogResult.Yes)
                {
                    try
                    {
                        Cursor = Cursors.WaitCursor;

                        await _apiService.DeleteEventAsync(eventId);
                        allEvents.RemoveAll(e => e.Id == eventId);
                        FilterEvents();

                        MessageBox.Show("Event deleted successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting event:\n{ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                }
            }
            else if (result == DialogResult.Cancel)
            {
                MessageBox.Show(
                    $"Event Details:\n\n" +
                    $"Name: {selectedEvent.Name}\n" +
                    $"Date: {selectedEvent.FormattedDate}\n" +
                    $"Category: {selectedEvent.Category}\n" +
                    $"Venue: {selectedEvent.Venue}\n" +
                    $"Price: {selectedEvent.FormattedPrice}\n" +
                    $"Tickets: {selectedEvent.TicketInfo}\n" +
                    $"Status: {selectedEvent.Status}\n" +
                    $"Organizer: {selectedEvent.Organizer}\n\n" +
                    $"Description:\n{selectedEvent.Description}",
                    "Event Details",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
    }

    public class PagedResult<T>
    {
        public int TotalCount { get; set; }
        public List<T> Items { get; set; } = new();
    }
}