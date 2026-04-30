using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Happenings.WinUI.Forms
{
    public class frmLogin : Form
    {
        private readonly APIService _apiService;

        // Controls
        private Panel? panelHeader;
        private Label? lblTitle;
        private Label? lblUsername;
        private TextBox? txtUsername;
        private Label? lblPassword;
        private TextBox? txtPassword;
        private Button? btnLogin;

        public frmLogin()
        {
            _apiService = Program.ServiceProvider.GetService<APIService>()!;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Form setup
            this.Text = "Happenings - Admin Login";
            this.Size = new Size(450, 420);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Header Panel
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(243, 156, 18)
            };

            // Title
            lblTitle = new Label
            {
                Text = "HAPPENINGS",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(100, 20)
            };
            panelHeader.Controls.Add(lblTitle);

            // Username Label
            lblUsername = new Label
            {
                Text = "Username:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(50, 120),
                AutoSize = true
            };

            // Username TextBox
            txtUsername = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Location = new Point(50, 145),
                Size = new Size(350, 27),
                Text = "admin" // Default za testiranje
            };

            // Password Label
            lblPassword = new Label
            {
                Text = "Password:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(50, 200),
                AutoSize = true
            };

            // Password TextBox
            txtPassword = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Location = new Point(50, 225),
                Size = new Size(350, 27),
                UseSystemPasswordChar = true,
                Text = "test" // Default za testiranje
            };

            // Login Button
            btnLogin = new Button
            {
                Text = "LOGIN",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(50, 280),
                Size = new Size(350, 45),
                BackColor = Color.FromArgb(243, 156, 18),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += btnLogin_Click;

            // Add controls to form
            this.Controls.Add(panelHeader);
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
        }

        private async void btnLogin_Click(object? sender, EventArgs e)
        {
            // Validacija
            if (string.IsNullOrWhiteSpace(txtUsername?.Text))
            {
                MessageBox.Show("Please enter username.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername?.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword?.Text))
            {
                MessageBox.Show("Please enter password.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword?.Focus();
                return;
            }

            try
            {
                if (btnLogin != null)
                {
                    btnLogin.Enabled = false;
                    btnLogin.Text = "Logging in...";
                }
                Cursor = Cursors.WaitCursor;

                // PRAVI API POZIV
                var response = await _apiService.LoginAsync(txtUsername.Text, txtPassword.Text);

                if (response != null && !string.IsNullOrEmpty(response.Token))
                {
                    // Login successful - otvori Main Dashboard
                    var frmMain = new frmMain();
                    frmMain.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Login failed: Invalid response from server", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login failed:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (btnLogin != null)
                {
                    btnLogin.Enabled = true;
                    btnLogin.Text = "LOGIN";
                }
                Cursor = Cursors.Default;
            }
        }
    }
}