using Happenings.WinUI.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Happenings.WinUI.Forms.ReferenceData
{
    public class frmCategoriesList : UserControl
    {
        private List<CategoryViewModel> categories = new List<CategoryViewModel>();
        private readonly APIService _apiService;

        private Panel? panelTop;
        private Button? btnAddCategory;
        private TextBox? txtSearch;
        private DataGridView? dgvCategories;

        public frmCategoriesList()
        {
            _apiService = (APIService)Program.ServiceProvider.GetService(typeof(APIService))!;
            InitializeComponent();
            LoadCategoriesAsync();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.Padding = new Padding(20);

            panelTop = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.Transparent };

            btnAddCategory = new Button
            {
                Text = "➕ Add New Category",
                Font = new Font(new FontFamily("Segoe UI"), 11, FontStyle.Bold),
                Size = new Size(200, 40),
                Location = new Point(0, 10),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAddCategory.FlatAppearance.BorderSize = 0;
            btnAddCategory.Click += btnAddCategory_Click;
            panelTop.Controls.Add(btnAddCategory);

            var lblSearch = new Label
            {
                Text = "Search:",
                Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular),
                Location = new Point(220, 15),
                AutoSize = true
            };
            panelTop.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular),
                Location = new Point(220, 35),
                Size = new Size(250, 27)
            };
            txtSearch.TextChanged += (s, e) => FilterCategories();
            panelTop.Controls.Add(txtSearch);

            dgvCategories = new DataGridView
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

            dgvCategories.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvCategories.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCategories.ColumnHeadersDefaultCellStyle.Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Bold);
            dgvCategories.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvCategories.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            dgvCategories.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 249, 249);
            dgvCategories.RowTemplate.Height = 35;
            dgvCategories.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);

            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", Visible = false });
            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Category Name", DataPropertyName = "Name" });
            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description", DataPropertyName = "Description" });
            dgvCategories.Columns.Add(new DataGridViewButtonColumn { Name = "Edit", HeaderText = "Edit", Text = "✏️ Edit", UseColumnTextForButtonValue = true, Width = 100 });
            dgvCategories.Columns.Add(new DataGridViewButtonColumn { Name = "Delete", HeaderText = "Delete", Text = "🗑️ Delete", UseColumnTextForButtonValue = true, Width = 100 });

            dgvCategories.CellContentClick += dgvCategories_CellContentClick;

            this.Controls.Add(dgvCategories);
            this.Controls.Add(panelTop);
        }

        private async void LoadCategoriesAsync()
        {
            try
            {
                var dtos = await _apiService.GetCategoriesAsync();
                categories = dtos.Select(d => new CategoryViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description ?? "",
                    IsActive = true
                }).ToList();

                dgvCategories!.DataSource = null;
                dgvCategories.DataSource = categories;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories:\n{ex.Message}", "Error");
            }
        }

        private void FilterCategories()
        {
            if (string.IsNullOrWhiteSpace(txtSearch?.Text))
            {
                dgvCategories!.DataSource = null;
                dgvCategories.DataSource = categories;
                return;
            }

            var searchText = txtSearch.Text.ToLower();
            var filtered = categories.Where(c =>
                c.Name.ToLower().Contains(searchText) ||
                c.Description.ToLower().Contains(searchText)).ToList();

            dgvCategories!.DataSource = null;
            dgvCategories.DataSource = filtered;
        }

        private async void btnAddCategory_Click(object? sender, EventArgs e)
        {
            var editForm = new frmCategoryEdit();
            if (editForm.ShowDialog() == DialogResult.OK && editForm.Category != null)
            {
                try
                {
                    await _apiService.CreateCategoryAsync(new CategoryInsertRequest
                    {
                        Name = editForm.Category.Name,
                        Description = editForm.Category.Description
                    });
                    LoadCategoriesAsync();
                    MessageBox.Show("Category added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating category:\n{ex.Message}", "Error");
                }
            }
        }

        private async void dgvCategories_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var categoryId = (int)dgvCategories!.Rows[e.RowIndex].Cells["Id"].Value;
            var category = categories.FirstOrDefault(c => c.Id == categoryId);
            if (category == null) return;

            if (dgvCategories.Columns[e.ColumnIndex].Name == "Edit")
            {
                var editForm = new frmCategoryEdit(category);
                if (editForm.ShowDialog() == DialogResult.OK && editForm.Category != null)
                {
                    try
                    {
                        await _apiService.UpdateCategoryAsync(categoryId, new CategoryUpdateRequest
                        {
                            Name = editForm.Category.Name,
                            Description = editForm.Category.Description
                        });
                        LoadCategoriesAsync();
                        MessageBox.Show("Category updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error updating category:\n{ex.Message}", "Error");
                    }
                }
            }
            else if (dgvCategories.Columns[e.ColumnIndex].Name == "Delete")
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete '{category.Name}'?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        await _apiService.DeleteCategoryAsync(categoryId);
                        LoadCategoriesAsync();
                        MessageBox.Show("Category deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting category:\n{ex.Message}", "Error");
                    }
                }
            }
        }
    }
}