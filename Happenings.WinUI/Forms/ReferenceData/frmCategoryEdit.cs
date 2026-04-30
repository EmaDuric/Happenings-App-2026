using Happenings.WinUI.Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Happenings.WinUI.Forms.ReferenceData
{
    public class frmCategoryEdit : Form
    {
        public CategoryViewModel? Category { get; private set; }
        private bool isEditMode = false;

        private Label? lblTitle;
        private TextBox? txtName;
        private TextBox? txtDescription;
        private Button? btnSave;
        private Button? btnCancel;

        public frmCategoryEdit(CategoryViewModel? categoryToEdit = null)
        {
            if (categoryToEdit != null)
            {
                Category = categoryToEdit;
                isEditMode = true;
            }
            else
            {
                Category = new CategoryViewModel { IsActive = true };
            }

            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = isEditMode ? "Edit Category" : "Add New Category";
            this.Size = new Size(480, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            int y = 20, left = 30, labelW = 130, ctrlW = 290;

            lblTitle = new Label
            {
                Text = isEditMode ? "Edit Category" : "Create New Category",
                Font = new Font(new FontFamily("Segoe UI"), 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(left, y),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);
            y += 50;

            AddLabel("Category Name:", left, y);
            txtName = new TextBox
            {
                Location = new Point(left + labelW, y - 5),
                Size = new Size(ctrlW, 27),
                Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular)
            };
            this.Controls.Add(txtName);
            y += 45;

            AddLabel("Description:", left, y);
            txtDescription = new TextBox
            {
                Location = new Point(left + labelW, y - 5),
                Size = new Size(ctrlW, 60),
                Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular),
                Multiline = true
            };
            this.Controls.Add(txtDescription);
            y += 75;

            btnCancel = new Button
            {
                Text = "Cancel",
                Font = new Font(new FontFamily("Segoe UI"), 11, FontStyle.Bold),
                Size = new Size(120, 40),
                Location = new Point(left + labelW + 50, y),
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
                Text = isEditMode ? "Update" : "Create",
                Font = new Font(new FontFamily("Segoe UI"), 11, FontStyle.Bold),
                Size = new Size(120, 40),
                Location = new Point(left + labelW + 180, y),
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

        private void LoadData()
        {
            if (Category == null) return;
            txtName!.Text = Category.Name;
            txtDescription!.Text = Category.Description;
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName?.Text))
            {
                MessageBox.Show("Please enter category name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName?.Focus();
                return;
            }

            if (Category != null)
            {
                Category.Name = txtName!.Text;
                Category.Description = txtDescription!.Text;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}