using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows.Forms;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Happenings.WinUI.Forms.Reports
{
    public class frmReports : UserControl
    {
        private Panel? panelTop;
        private Button? btnSalesReport;
        private Button? btnRevenueReport;
        private Button? btnExportExcel;
        private DataGridView? dgvReport;
        private Label? lblReportTitle;

        private List<EventSalesDto> salesData = new();
        private List<EventRevenueDto> revenueData = new();
        private string currentReport = "";

        public frmReports()
        {
            InitializeComponent();
            LoadSalesReport();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
            this.Padding = new Padding(20);

            panelTop = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = System.Drawing.Color.Transparent };

            var lblTitle = new Label
            {
                Text = "Reports",
                Font = new System.Drawing.Font("Segoe UI", 16, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(0, 0),
                AutoSize = true
            };
            panelTop.Controls.Add(lblTitle);

            btnSalesReport = new Button
            {
                Text = "📊 Sales Report",
                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),
                Size = new System.Drawing.Size(160, 38),
                Location = new System.Drawing.Point(0, 40),
                BackColor = System.Drawing.Color.FromArgb(52, 152, 219),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSalesReport.FlatAppearance.BorderSize = 0;
            btnSalesReport.Click += (s, e) => LoadSalesReport();
            panelTop.Controls.Add(btnSalesReport);

            btnRevenueReport = new Button
            {
                Text = "💰 Revenue Report",
                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),
                Size = new System.Drawing.Size(160, 38),
                Location = new System.Drawing.Point(170, 40),
                BackColor = System.Drawing.Color.FromArgb(46, 204, 113),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRevenueReport.FlatAppearance.BorderSize = 0;
            btnRevenueReport.Click += (s, e) => LoadRevenueReport();
            panelTop.Controls.Add(btnRevenueReport);

            var btnExportPDF = new Button
            {
                Text = "📄 Export PDF",
                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),
                Size = new System.Drawing.Size(140, 38),
                Location = new System.Drawing.Point(340, 40),
                BackColor = System.Drawing.Color.FromArgb(231, 76, 60),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExportPDF.FlatAppearance.BorderSize = 0;
            btnExportPDF.Click += btnExportPDF_Click;
            panelTop.Controls.Add(btnExportPDF);

            btnExportExcel = new Button
            {
                Text = "📊 Export Excel",
                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),
                Size = new System.Drawing.Size(140, 38),
                Location = new System.Drawing.Point(490, 40),
                BackColor = System.Drawing.Color.FromArgb(39, 174, 96),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExportExcel.FlatAppearance.BorderSize = 0;
            btnExportExcel.Click += btnExportExcel_Click;
            panelTop.Controls.Add(btnExportExcel);

            lblReportTitle = new Label
            {
                Text = "",
                Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                ForeColor = System.Drawing.Color.FromArgb(44, 62, 80)
            };

            dgvReport = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Font = new System.Drawing.Font("Segoe UI", 10),
                ColumnHeadersHeight = 40,
                AutoGenerateColumns = false
            };
            dgvReport.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(52, 73, 94);
            dgvReport.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dgvReport.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            dgvReport.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
            dgvReport.RowTemplate.Height = 35;

            this.Controls.Add(dgvReport);
            this.Controls.Add(lblReportTitle);
            this.Controls.Add(panelTop);
        }

        private async void LoadSalesReport()
        {
            try
            {
                currentReport = "sales";
                lblReportTitle!.Text = "Sales Report — Tickets sold per event (Approved reservations)";

                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("http://localhost:5000/api/");
                if (!string.IsNullOrEmpty(TokenStore.Token))
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

                var response = await httpClient.GetAsync("Reports/event-sales");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                salesData = JsonSerializer.Deserialize<List<EventSalesDto>>(json, options) ?? new();

                dgvReport!.Columns.Clear();
                dgvReport.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Event", DataPropertyName = "EventName" });
                dgvReport.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tickets Sold", DataPropertyName = "TicketsSold", Width = 130 });

                dgvReport.DataSource = null;
                dgvReport.DataSource = salesData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sales report:\n{ex.Message}", "Error");
            }
        }

        private async void LoadRevenueReport()
        {
            try
            {
                currentReport = "revenue";
                lblReportTitle!.Text = "Revenue Report — Revenue from completed payments per event";

                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("http://localhost:5000/api/");
                if (!string.IsNullOrEmpty(TokenStore.Token))
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

                var response = await httpClient.GetAsync("Reports/revenue");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                revenueData = JsonSerializer.Deserialize<List<EventRevenueDto>>(json, options) ?? new();

                dgvReport!.Columns.Clear();
                dgvReport.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Event", DataPropertyName = "EventName" });
                dgvReport.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Revenue (KM)", DataPropertyName = "FormattedRevenue", Width = 150 });

                dgvReport.DataSource = null;
                dgvReport.DataSource = revenueData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading revenue report:\n{ex.Message}", "Error");
            }
        }

        private void btnExportPDF_Click(object? sender, EventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF Files|*.pdf",
                    FileName = $"{currentReport}Report_{DateTime.UtcNow:yyyyMMdd}.pdf"
                };

                if (saveDialog.ShowDialog() != DialogResult.OK) return;

                var document = new Document(PageSize.A4);
                PdfWriter.GetInstance(document, new FileStream(saveDialog.FileName, FileMode.Create));
                document.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);

                if (currentReport == "sales")
                {
                    document.Add(new Paragraph("Sales Report", titleFont) { Alignment = Element.ALIGN_CENTER });
                    document.Add(new Paragraph($"Generated: {DateTime.Now:dd.MM.yyyy HH:mm}", bodyFont));
                    document.Add(new Paragraph($"Total events with sales: {salesData.Count}", bodyFont));
                    document.Add(new Paragraph($"Total tickets sold: {salesData.Sum(x => x.TicketsSold)}", bodyFont));
                    document.Add(new Paragraph(" "));

                    var table = new PdfPTable(2) { WidthPercentage = 100 };
                    table.AddCell(new PdfPCell(new Phrase("Event Name", headerFont)) { BackgroundColor = new BaseColor(52, 73, 94), BorderColor = BaseColor.WHITE });
                    table.AddCell(new PdfPCell(new Phrase("Tickets Sold", headerFont)) { BackgroundColor = new BaseColor(52, 73, 94), BorderColor = BaseColor.WHITE });

                    foreach (var item in salesData)
                    {
                        table.AddCell(new Phrase(item.EventName, bodyFont));
                        table.AddCell(new Phrase(item.TicketsSold.ToString(), bodyFont));
                    }
                    document.Add(table);
                }
                else
                {
                    document.Add(new Paragraph("Revenue Report", titleFont) { Alignment = Element.ALIGN_CENTER });
                    document.Add(new Paragraph($"Generated: {DateTime.Now:dd.MM.yyyy HH:mm}", bodyFont));
                    document.Add(new Paragraph($"Total events with revenue: {revenueData.Count}", bodyFont));
                    document.Add(new Paragraph($"Total revenue: {revenueData.Sum(x => x.Revenue):N2} KM", bodyFont));
                    document.Add(new Paragraph(" "));

                    var table = new PdfPTable(2) { WidthPercentage = 100 };
                    table.AddCell(new PdfPCell(new Phrase("Event Name", headerFont)) { BackgroundColor = new BaseColor(52, 73, 94), BorderColor = BaseColor.WHITE });
                    table.AddCell(new PdfPCell(new Phrase("Revenue (KM)", headerFont)) { BackgroundColor = new BaseColor(52, 73, 94), BorderColor = BaseColor.WHITE });

                    foreach (var item in revenueData)
                    {
                        table.AddCell(new Phrase(item.EventName, bodyFont));
                        table.AddCell(new Phrase(item.Revenue.ToString("N2"), bodyFont));
                    }
                    document.Add(table);
                }

                document.Close();
                MessageBox.Show($"PDF exported successfully:\n{saveDialog.FileName}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting PDF:\n{ex.Message}", "Error");
            }
        }

        private void btnExportExcel_Click(object? sender, EventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    FileName = $"{currentReport}Report_{DateTime.UtcNow:yyyyMMdd}.xlsx"
                };

                if (saveDialog.ShowDialog() != DialogResult.OK) return;

                using var workbook = new XLWorkbook();

                if (currentReport == "sales")
                {
                    var ws = workbook.Worksheets.Add("Sales Report");
                    ws.Cell("A1").Value = "Sales Report";
                    ws.Cell("A1").Style.Font.Bold = true;
                    ws.Cell("A1").Style.Font.FontSize = 16;
                    ws.Cell("A3").Value = $"Generated: {DateTime.Now:dd.MM.yyyy HH:mm}";
                    ws.Cell("A4").Value = $"Total tickets sold: {salesData.Sum(x => x.TicketsSold)}";

                    ws.Cell("A6").Value = "Event Name"; ws.Cell("A6").Style.Font.Bold = true;
                    ws.Cell("B6").Value = "Tickets Sold"; ws.Cell("B6").Style.Font.Bold = true;

                    int row = 7;
                    foreach (var item in salesData)
                    {
                        ws.Cell(row, 1).Value = item.EventName;
                        ws.Cell(row, 2).Value = item.TicketsSold;
                        row++;
                    }
                    ws.Columns().AdjustToContents();
                }
                else
                {
                    var ws = workbook.Worksheets.Add("Revenue Report");
                    ws.Cell("A1").Value = "Revenue Report";
                    ws.Cell("A1").Style.Font.Bold = true;
                    ws.Cell("A1").Style.Font.FontSize = 16;
                    ws.Cell("A3").Value = $"Generated: {DateTime.Now:dd.MM.yyyy HH:mm}";
                    ws.Cell("A4").Value = $"Total revenue: {revenueData.Sum(x => x.Revenue):N2} KM";

                    ws.Cell("A6").Value = "Event Name"; ws.Cell("A6").Style.Font.Bold = true;
                    ws.Cell("B6").Value = "Revenue (KM)"; ws.Cell("B6").Style.Font.Bold = true;

                    int row = 7;
                    foreach (var item in revenueData)
                    {
                        ws.Cell(row, 1).Value = item.EventName;
                        ws.Cell(row, 2).Value = item.Revenue;
                        row++;
                    }
                    ws.Columns().AdjustToContents();
                }

                workbook.SaveAs(saveDialog.FileName);
                MessageBox.Show($"Excel exported successfully:\n{saveDialog.FileName}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting Excel:\n{ex.Message}", "Error");
            }
        }
    }

    public class EventSalesDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int TicketsSold { get; set; }
    }

    public class EventRevenueDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public string FormattedRevenue => $"{Revenue:N2} KM";
    }
}