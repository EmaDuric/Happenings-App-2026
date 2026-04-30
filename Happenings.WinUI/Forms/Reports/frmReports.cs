using Happenings.WinUI.Models;
using LiveCharts;
using WinFormsCharts = LiveCharts.WinForms;
using WpfCharts = LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using SystemDrawing = System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Happenings.WinUI.Forms.Reports
{
    public class frmReports : UserControl
    {
        private Panel? panelFilters;
        private DateTimePicker? dtpStartDate;
        private DateTimePicker? dtpEndDate;
        private ComboBox? cmbCategory;
        private ComboBox? cmbStatus;
        private Button? btnGenerate;
        private Button? btnExportPDF;
        private Button? btnExportExcel;

        private Panel? panelCharts;
        private WinFormsCharts.CartesianChart? chartEventsByCategory;
        private WinFormsCharts.PieChart? chartEventStatus;
        private WinFormsCharts.CartesianChart? chartRevenue;

        private Panel? panelSummary;
        private Label? lblTotalEvents;
        private Label? lblTotalRevenue;
        private Label? lblTotalTicketsSold;

        private List<EventViewModel> allEvents = new List<EventViewModel>();
        private List<EventViewModel> filteredEvents = new List<EventViewModel>();
        private readonly APIService _apiService;

        public frmReports()
        {
            _apiService = (APIService)Program.ServiceProvider.GetService(typeof(APIService))!;
            InitializeComponent();
            LoadDataAsync();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = SystemDrawing.Color.FromArgb(245, 245, 245);
            this.AutoScroll = true;

            panelFilters = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = SystemDrawing.Color.White,
                Padding = new Padding(20)
            };

            var lblTitle = new Label
            {
                Text = "Report Parameters",
                Font = new SystemDrawing.Font(new SystemDrawing.FontFamily("Segoe UI"), 14, SystemDrawing.FontStyle.Bold),
                ForeColor = SystemDrawing.Color.FromArgb(44, 62, 80),
                Location = new SystemDrawing.Point(20, 10),
                AutoSize = true
            };
            panelFilters.Controls.Add(lblTitle);

            var lblStartDate = new Label
            {
                Text = "Start Date:",
                Font = new SystemDrawing.Font(new SystemDrawing.FontFamily("Segoe UI"), 10, SystemDrawing.FontStyle.Regular),
                Location = new SystemDrawing.Point(20, 50),
                AutoSize = true
            };
            panelFilters.Controls.Add(lblStartDate);

            dtpStartDate = new DateTimePicker
            {
                Location = new SystemDrawing.Point(20, 70),
                Size = new SystemDrawing.Size(150, 27),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddMonths(-3)
            };
            panelFilters.Controls.Add(dtpStartDate);

            var lblEndDate = new Label
            {
                Text = "End Date:",
                Font = new SystemDrawing.Font(new SystemDrawing.FontFamily("Segoe UI"), 10, SystemDrawing.FontStyle.Regular),
                Location = new SystemDrawing.Point(190, 50),
                AutoSize = true
            };
            panelFilters.Controls.Add(lblEndDate);

            dtpEndDate = new DateTimePicker
            {
                Location = new SystemDrawing.Point(190, 70),
                Size = new SystemDrawing.Size(150, 27),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddMonths(3)
            };
            panelFilters.Controls.Add(dtpEndDate);

            var lblCategory = new Label
            {
                Text = "Category:",
                Font = new SystemDrawing.Font(new SystemDrawing.FontFamily("Segoe UI"), 10, SystemDrawing.FontStyle.Regular),
                Location = new SystemDrawing.Point(360, 50),
                AutoSize = true
            };
            panelFilters.Controls.Add(lblCategory);

            cmbCategory = new ComboBox
            {
                Location = new SystemDrawing.Point(360, 70),
                Size = new SystemDrawing.Size(150, 27),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCategory.Items.Add("All Categories");
            cmbCategory.SelectedIndex = 0;
            panelFilters.Controls.Add(cmbCategory);

            var lblStatus = new Label
            {
                Text = "Status:",
                Font = new SystemDrawing.Font(new SystemDrawing.FontFamily("Segoe UI"), 10, SystemDrawing.FontStyle.Regular),
                Location = new SystemDrawing.Point(530, 50),
                AutoSize = true
            };
            panelFilters.Controls.Add(lblStatus);

            cmbStatus = new ComboBox
            {
                Location = new SystemDrawing.Point(530, 70),
                Size = new SystemDrawing.Size(150, 27),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new object[] { "All", "Active", "Completed", "Cancelled" });
            cmbStatus.SelectedIndex = 0;
            panelFilters.Controls.Add(cmbStatus);

            btnGenerate = new Button
            {
                Text = "📊 Generate Report",
                Font = new SystemDrawing.Font(new SystemDrawing.FontFamily("Segoe UI"), 11, SystemDrawing.FontStyle.Bold),
                Size = new SystemDrawing.Size(180, 40),
                Location = new SystemDrawing.Point(700, 65),
                BackColor = SystemDrawing.Color.FromArgb(52, 152, 219),
                ForeColor = SystemDrawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.Click += btnGenerate_Click;
            panelFilters.Controls.Add(btnGenerate);

            btnExportPDF = new Button
            {
                Text = "📄 Export PDF",
                Font = new SystemDrawing.Font(new SystemDrawing.FontFamily("Segoe UI"), 10, SystemDrawing.FontStyle.Bold),
                Size = new SystemDrawing.Size(140, 35),
                Location = new SystemDrawing.Point(900, 68),
                BackColor = SystemDrawing.Color.FromArgb(231, 76, 60),
                ForeColor = SystemDrawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExportPDF.FlatAppearance.BorderSize = 0;
            btnExportPDF.Click += btnExportPDF_Click;
            panelFilters.Controls.Add(btnExportPDF);

            btnExportExcel = new Button
            {
                Text = "📊 Export Excel",
                Font = new SystemDrawing.Font(new SystemDrawing.FontFamily("Segoe UI"), 10, SystemDrawing.FontStyle.Bold),
                Size = new SystemDrawing.Size(140, 35),
                Location = new SystemDrawing.Point(1050, 68),
                BackColor = SystemDrawing.Color.FromArgb(46, 204, 113),
                ForeColor = SystemDrawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExportExcel.FlatAppearance.BorderSize = 0;
            btnExportExcel.Click += btnExportExcel_Click;
            panelFilters.Controls.Add(btnExportExcel);

            panelSummary = new Panel
            {
                Location = new SystemDrawing.Point(20, 140),
                Size = new SystemDrawing.Size(1200, 100),
                BackColor = SystemDrawing.Color.White
            };

            lblTotalEvents = CreateSummaryLabel("Total Events: 0", 20, 20, SystemDrawing.Color.FromArgb(52, 152, 219));
            panelSummary.Controls.Add(lblTotalEvents);

            lblTotalRevenue = CreateSummaryLabel("Total Revenue: $0.00", 420, 20, SystemDrawing.Color.FromArgb(46, 204, 113));
            panelSummary.Controls.Add(lblTotalRevenue);

            lblTotalTicketsSold = CreateSummaryLabel("Tickets Sold: 0", 820, 20, SystemDrawing.Color.FromArgb(155, 89, 182));
            panelSummary.Controls.Add(lblTotalTicketsSold);

            panelCharts = new Panel
            {
                Location = new SystemDrawing.Point(20, 260),
                Size = new SystemDrawing.Size(1200, 600),
                BackColor = SystemDrawing.Color.Transparent,
                AutoScroll = true
            };

            chartEventsByCategory = new WinFormsCharts.CartesianChart
            {
                Location = new SystemDrawing.Point(10, 10),
                Size = new SystemDrawing.Size(580, 280),
                BackColor = SystemDrawing.Color.White
            };
            panelCharts.Controls.Add(chartEventsByCategory);

            chartEventStatus = new WinFormsCharts.PieChart
            {
                Location = new SystemDrawing.Point(610, 10),
                Size = new SystemDrawing.Size(580, 280),
                BackColor = SystemDrawing.Color.White,
                LegendLocation = LiveCharts.LegendLocation.Bottom
            };
            panelCharts.Controls.Add(chartEventStatus);

            chartRevenue = new WinFormsCharts.CartesianChart
            {
                Location = new SystemDrawing.Point(10, 310),
                Size = new SystemDrawing.Size(1180, 280),
                BackColor = SystemDrawing.Color.White
            };
            panelCharts.Controls.Add(chartRevenue);

            this.Controls.Add(panelCharts);
            this.Controls.Add(panelSummary);
            this.Controls.Add(panelFilters);
        }

        private Label CreateSummaryLabel(string text, int x, int y, SystemDrawing.Color color)
        {
            var panel = new Panel
            {
                Location = new SystemDrawing.Point(x, y),
                Size = new SystemDrawing.Size(360, 60),
                BackColor = color
            };

            var label = new Label
            {
                Text = text,
                Font = new SystemDrawing.Font(new SystemDrawing.FontFamily("Segoe UI"), 18, SystemDrawing.FontStyle.Bold),
                ForeColor = SystemDrawing.Color.White,
                AutoSize = false,
                Size = new SystemDrawing.Size(360, 60),
                TextAlign = ContentAlignment.MiddleCenter
            };
            panel.Controls.Add(label);
            return label;
        }

        private async void LoadDataAsync()
        {
            try
            {
                // Učitaj evente
                var dtos = await _apiService.GetEventsAsync();
                allEvents = dtos.Select(e => new EventViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
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

                // Učitaj kategorije direktno iz API-ja
                var categoryDtos = await _apiService.GetCategoriesAsync();

                cmbCategory!.Items.Clear();
                cmbCategory.Items.Add("All Categories");
                foreach (var cat in categoryDtos)
                    cmbCategory.Items.Add(cat.Name);
                cmbCategory.SelectedIndex = 0;

                GenerateReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data:\n{ex.Message}", "Error");
            }
        }

        private void btnGenerate_Click(object? sender, EventArgs e)
        {
            GenerateReport();
        }

        private void GenerateReport()
        {
            filteredEvents = allEvents.Where(ev =>
                ev.StartDateTime >= dtpStartDate!.Value.Date &&
                ev.StartDateTime <= dtpEndDate!.Value.Date
            ).ToList();

            if (cmbCategory!.SelectedIndex > 0)
            {
                var selectedCategory = cmbCategory.SelectedItem?.ToString();
                filteredEvents = filteredEvents.Where(e => e.Category == selectedCategory).ToList();
            }

            if (cmbStatus!.SelectedIndex > 0)
            {
                var selectedStatus = cmbStatus.SelectedItem?.ToString();
                filteredEvents = filteredEvents.Where(e => e.Status == selectedStatus).ToList();
            }

            UpdateSummary();
            UpdateCharts();
        }

        private void UpdateSummary()
        {
            var totalEvents = filteredEvents.Count;
            var totalRevenue = filteredEvents.Sum(e => e.Price * (e.TotalTickets - e.AvailableTickets));
            var totalTicketsSold = filteredEvents.Sum(e => e.TotalTickets - e.AvailableTickets);

            lblTotalEvents!.Text = $"Total Events: {totalEvents}";
            lblTotalRevenue!.Text = $"Total Revenue: ${totalRevenue:N2}";
            lblTotalTicketsSold!.Text = $"Tickets Sold: {totalTicketsSold:N0}";
        }

        private void UpdateCharts()
        {
            var categoryData = filteredEvents
                .GroupBy(e => e.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToList();

            chartEventsByCategory!.Series = new SeriesCollection
            {
                new WpfCharts.ColumnSeries
                {
                    Title = "Events",
                    Values = new ChartValues<int>(categoryData.Select(d => d.Count)),
                    Fill = System.Windows.Media.Brushes.CornflowerBlue
                }
            };
            chartEventsByCategory.AxisX.Clear();
            chartEventsByCategory.AxisY.Clear();
            chartEventsByCategory.AxisX.Add(new WpfCharts.Axis { Title = "Category", Labels = categoryData.Select(d => d.Category).ToList() });
            chartEventsByCategory.AxisY.Add(new WpfCharts.Axis { Title = "Number of Events", MinValue = 0 });

            var statusData = filteredEvents
                .GroupBy(e => e.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            chartEventStatus!.Series = new SeriesCollection();
            foreach (var item in statusData)
            {
                chartEventStatus.Series.Add(new WpfCharts.PieSeries
                {
                    Title = item.Status,
                    Values = new ChartValues<int> { item.Count },
                    DataLabels = true
                });
            }

            var revenueData = filteredEvents
                .GroupBy(e => e.StartDateTime.ToString("MMM yyyy"))
                .Select(g => new { Month = g.Key, Revenue = g.Sum(ev => ev.Price * (ev.TotalTickets - ev.AvailableTickets)) })
                .ToList();

            chartRevenue!.Series = new SeriesCollection
            {
                new WpfCharts.LineSeries
                {
                    Title = "Revenue",
                    Values = new ChartValues<decimal>(revenueData.Select(d => d.Revenue)),
                    Fill = System.Windows.Media.Brushes.Transparent,
                    PointGeometry = WpfCharts.DefaultGeometries.Circle
                }
            };
            chartRevenue.AxisX.Clear();
            chartRevenue.AxisY.Clear();
            chartRevenue.AxisX.Add(new WpfCharts.Axis { Title = "Month", Labels = revenueData.Select(d => d.Month).ToList() });
            chartRevenue.AxisY.Add(new WpfCharts.Axis { Title = "Revenue ($)", MinValue = 0, LabelFormatter = value => value.ToString("C") });
        }

        private void btnExportPDF_Click(object? sender, EventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF Files|*.pdf",
                    FileName = $"EventsReport_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportToPDF(saveDialog.FileName);
                    MessageBox.Show($"Report exported successfully to:\n{saveDialog.FileName}", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to PDF:\n{ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExportExcel_Click(object? sender, EventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    FileName = $"EventsReport_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportToExcel(saveDialog.FileName);
                    MessageBox.Show($"Report exported successfully to:\n{saveDialog.FileName}", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to Excel:\n{ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToPDF(string filePath)
        {
            var document = new Document(PageSize.A4.Rotate());
            PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
            document.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var title = new Paragraph("Events Report", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);
            document.Add(new Paragraph(" "));

            var summaryFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
            document.Add(new Paragraph($"Report Period: {dtpStartDate!.Value:MM/dd/yyyy} - {dtpEndDate!.Value:MM/dd/yyyy}", summaryFont));
            document.Add(new Paragraph($"Total Events: {filteredEvents.Count}", summaryFont));
            document.Add(new Paragraph($"Total Revenue: ${filteredEvents.Sum(e => e.Price * (e.TotalTickets - e.AvailableTickets)):N2}", summaryFont));
            document.Add(new Paragraph(" "));

            var table = new PdfPTable(7);
            table.WidthPercentage = 100;
            table.AddCell("Event Name");
            table.AddCell("Category");
            table.AddCell("Date");
            table.AddCell("Venue");
            table.AddCell("Price");
            table.AddCell("Tickets Sold");
            table.AddCell("Revenue");

            foreach (var ev in filteredEvents)
            {
                table.AddCell(ev.Name);
                table.AddCell(ev.Category);
                table.AddCell(ev.FormattedDate);
                table.AddCell(ev.Venue);
                table.AddCell(ev.FormattedPrice);
                table.AddCell((ev.TotalTickets - ev.AvailableTickets).ToString());
                table.AddCell($"${(ev.Price * (ev.TotalTickets - ev.AvailableTickets)):N2}");
            }

            document.Add(table);
            document.Close();
        }

        private void ExportToExcel(string filePath)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Events Report");

            worksheet.Cell("A1").Value = "Events Report";
            worksheet.Cell("A1").Style.Font.Bold = true;
            worksheet.Cell("A1").Style.Font.FontSize = 16;

            worksheet.Cell("A3").Value = $"Report Period: {dtpStartDate!.Value:MM/dd/yyyy} - {dtpEndDate!.Value:MM/dd/yyyy}";
            worksheet.Cell("A4").Value = $"Total Events: {filteredEvents.Count}";
            worksheet.Cell("A5").Value = $"Total Revenue: ${filteredEvents.Sum(e => e.Price * (e.TotalTickets - e.AvailableTickets)):N2}";

            var headers = new[] { "Event Name", "Category", "Date", "Venue", "Price", "Tickets Sold", "Revenue", "Status" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(7, i + 1).Value = headers[i];
                worksheet.Cell(7, i + 1).Style.Font.Bold = true;
            }

            int row = 8;
            foreach (var ev in filteredEvents)
            {
                worksheet.Cell(row, 1).Value = ev.Name;
                worksheet.Cell(row, 2).Value = ev.Category;
                worksheet.Cell(row, 3).Value = ev.FormattedDate;
                worksheet.Cell(row, 4).Value = ev.Venue;
                worksheet.Cell(row, 5).Value = ev.Price;
                worksheet.Cell(row, 6).Value = ev.TotalTickets - ev.AvailableTickets;
                worksheet.Cell(row, 7).Value = ev.Price * (ev.TotalTickets - ev.AvailableTickets);
                worksheet.Cell(row, 8).Value = ev.Status;
                row++;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
        }
    }
}