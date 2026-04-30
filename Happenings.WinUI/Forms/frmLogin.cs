using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Windows.Forms;

namespace Happenings.WinUI.Forms
{
    public class frmLogin : Form
    {
        private readonly APIService _apiService;

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
            this.Text = "Happenings - Admin Login";
            this.Size = new Size(450, 420);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(243, 156, 18)
            };

            lblTitle = new Label
            {
                Text = "HAPPENINGS",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(100, 20)
            };
            panelHeader.Controls.Add(lblTitle);

            lblUsername = new Label
            {
                Text = "Username:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(50, 120),
                AutoSize = true
            };

            txtUsername = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Location = new Point(50, 145),
                Size = new Size(350, 27),
                Text = "admin@mail.com"
            };

            lblPassword = new Label
            {
                Text = "Password:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(50, 200),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Location = new Point(50, 225),
                Size = new Size(350, 27),
                UseSystemPasswordChar = true,
                Text = "test"
            };

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

            this.Controls.Add(panelHeader);
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
        }

        private async void btnLogin_Click(object? sender, EventArgs e)
        {
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

                var response = await _apiService.LoginAsync(txtUsername.Text, txtPassword.Text);

                if (response != null && !string.IsNullOrEmpty(response.Token))
                {
                    // Provjeri je li korisnik Admin
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(response.Token);
                    var role = jwt.Claims
                        .FirstOrDefault(c => c.Type == "role" || c.Type == System.Security.Claims.ClaimTypes.Role)
                        ?.Value;

                    if (role != "Admin")
                    {
                        MessageBox.Show(
                            "Access denied.\nOnly administrators can access this panel.",
                            "Unauthorized",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }

                    TokenStore.Token = response.Token;
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