using Happenings.WinUI.Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Happenings.WinUI.Forms.ReferenceData
{
	public class frmVenueEdit : Form
	{
		public VenueViewModel? Venue { get; private set; }
		private bool isEditMode = false;

		private Label? lblTitle;
		private TextBox? txtName;
		private TextBox? txtAddress;
		private TextBox? txtCity;
		private Button? btnSave;
		private Button? btnCancel;

		public frmVenueEdit(VenueViewModel? venueToEdit = null)
		{
			if (venueToEdit != null)
			{
				Venue = venueToEdit;
				isEditMode = true;
			}
			else
			{
				Venue = new VenueViewModel { IsActive = true };
			}

			InitializeComponent();
			LoadData();
		}

		private void InitializeComponent()
		{
			this.Text = isEditMode ? "Edit Venue" : "Add New Venue";
			this.Size = new Size(500, 340);
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.BackColor = Color.White;

			int y = 20, left = 30, labelW = 120, ctrlW = 310;

			lblTitle = new Label
			{
				Text = isEditMode ? "Edit Venue" : "Create New Venue",
				Font = new Font(new FontFamily("Segoe UI"), 16, FontStyle.Bold),
				ForeColor = Color.FromArgb(44, 62, 80),
				Location = new Point(left, y),
				AutoSize = true
			};
			this.Controls.Add(lblTitle);
			y += 50;

			AddLabel("Venue Name:", left, y);
			txtName = new TextBox
			{
				Location = new Point(left + labelW, y - 5),
				Size = new Size(ctrlW, 27),
				Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular)
			};
			this.Controls.Add(txtName);
			y += 45;

			AddLabel("Address:", left, y);
			txtAddress = new TextBox
			{
				Location = new Point(left + labelW, y - 5),
				Size = new Size(ctrlW, 27),
				Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular)
			};
			this.Controls.Add(txtAddress);
			y += 45;

			AddLabel("City:", left, y);
			txtCity = new TextBox
			{
				Location = new Point(left + labelW, y - 5),
				Size = new Size(ctrlW, 27),
				Font = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Regular)
			};
			this.Controls.Add(txtCity);
			y += 55;

			btnCancel = new Button
			{
				Text = "Cancel",
				Font = new Font(new FontFamily("Segoe UI"), 11, FontStyle.Bold),
				Size = new Size(120, 40),
				Location = new Point(left + labelW + 60, y),
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
				Location = new Point(left + labelW + 190, y),
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
			if (Venue == null) return;
			txtName!.Text = Venue.Name;
			txtAddress!.Text = Venue.Address;
			txtCity!.Text = Venue.City;
		}

		private void btnSave_Click(object? sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(txtName?.Text))
			{
				MessageBox.Show("Please enter venue name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				txtName?.Focus();
				return;
			}
			if (string.IsNullOrWhiteSpace(txtAddress?.Text))
			{
				MessageBox.Show("Please enter address.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				txtAddress?.Focus();
				return;
			}
			if (string.IsNullOrWhiteSpace(txtCity?.Text))
			{
				MessageBox.Show("Please enter city.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				txtCity?.Focus();
				return;
			}

			if (Venue != null)
			{
				Venue.Name = txtName!.Text;
				Venue.Address = txtAddress!.Text;
				Venue.City = txtCity!.Text;
			}

			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}