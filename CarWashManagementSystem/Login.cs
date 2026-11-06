using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CarWashManagementSystem
{
    public partial class Login : Form
    {
        SqlCommand cm = new SqlCommand();
        dbConnect dbcon = new dbConnect();
        SqlDataReader dr;
        string title = "Car Wash Management System";
        private readonly Button btnSetupManager = new Button();
        public Login()
        {
            InitializeComponent();
            ConfigureManagerSetupButton();
            ApplyVisualStyle();
            loadCompany();
            RefreshManagerSetupVisibility();
        }

        private void ConfigureManagerSetupButton()
        {
            btnSetupManager.Text = "First time here? Set up the manager account";
            btnSetupManager.Location = new Point(585, 399);
            btnSetupManager.Size = new Size(270, 26);
            btnSetupManager.FlatStyle = FlatStyle.Flat;
            btnSetupManager.FlatAppearance.BorderSize = 0;
            btnSetupManager.BackColor = Color.White;
            btnSetupManager.ForeColor = Color.FromArgb(90, 105, 115);
            btnSetupManager.Font = new Font("Segoe UI", 9F);
            btnSetupManager.Click += btnSetupManager_Click;
            panel1.Controls.Add(btnSetupManager);
        }

        private void RefreshManagerSetupVisibility()
        {
            try
            {
                dbcon.open();
                using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM tbEmployer WHERE role = 'Manager'", dbcon.connect()))
                    btnSetupManager.Visible = Convert.ToInt32(command.ExecuteScalar()) == 0;
            }
            catch
            {
                btnSetupManager.Visible = false;
            }
            finally
            {
                dbcon.close();
            }
        }

        private void ApplyVisualStyle()
        {
            Color navy = Color.FromArgb(26, 43, 64);
            Color teal = Color.FromArgb(20, 145, 160);
            BackColor = Color.FromArgb(239, 244, 247);
            panel1.BackColor = Color.White;
            label1.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            label1.ForeColor = navy;
            label1.Text = "Welcome back";
            label2.ForeColor = Color.FromArgb(64, 78, 91);
            label3.ForeColor = Color.FromArgb(64, 78, 91);
            txtName.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtName.BackColor = Color.FromArgb(248, 250, 252);
            txtPassword.BackColor = Color.FromArgb(248, 250, 252);
            checkBox1.ForeColor = Color.FromArgb(83, 99, 113);
            btnLogin.BackColor = teal;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnCancel.BackColor = Color.FromArgb(231, 237, 241);
            btnCancel.ForeColor = navy;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnRegister.ForeColor = teal;
            btnRegister.FlatAppearance.BorderSize = 0;
            lblCompany.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            lblCompany.ForeColor = navy;
            lblCompany.Location = new Point(460, 428);
            lblCompany.Size = new Size(400, 39);
            lblCompany.TextAlign = ContentAlignment.MiddleCenter;
            lblAddress.ForeColor = Color.FromArgb(104, 119, 131);
            lblAddress.Location = new Point(460, 469);
            lblAddress.Size = new Size(400, 21);
            lblAddress.TextAlign = ContentAlignment.MiddleCenter;
            pictureBox2.Visible = false;
            btnRegister.Text = "New customer? Register your car";
            btnRegister.Location = new Point(585, 365);
            btnRegister.Size = new Size(270, 30);
        }


        public void loadCompany()
        {
            try
            {
                dbcon.open();
                using (cm = new SqlCommand("SELECT TOP 1 name, address FROM tbCompany", dbcon.connect()))
                using (dr = cm.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        lblCompany.Text = dr["name"].ToString();
                        lblAddress.Text = dr["address"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't load the business details. Check the database connection.\n\n" + ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                dbcon.close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            txtName.Clear();
            txtPassword.Clear();
            txtName.Focus();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                dbcon.open();
                string employeeName = null;
                string employeeRole = null;
                using (cm = new SqlCommand("SELECT TOP 1 name, role FROM tbEmployer WHERE name = @name AND password = @password ORDER BY CASE WHEN role = 'Manager' THEN 0 ELSE 1 END, id", dbcon.connect()))
                {
                    cm.Parameters.AddWithValue("@name", txtName.Text.Trim());
                    cm.Parameters.AddWithValue("@password", txtPassword.Text);
                    using (SqlDataReader reader = cm.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            employeeName = reader["name"].ToString();
                            employeeRole = reader["role"] == DBNull.Value ? "Worker" : reader["role"].ToString();
                        }
                    }
                }
                dbcon.close();
                if (employeeName != null)
                {
                    MessageBox.Show("Welcome, " + employeeName + "!", "Access granted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Hide();
                    MainForm main = new MainForm(employeeRole);
                    main.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Invalid username or password", "ACCESS DENIED", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                dbcon.close();
                MessageBox.Show(ex.Message, title);
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            using (CustomerCheckIn checkIn = new CustomerCheckIn())
            {
                if (checkIn.ShowDialog(this) == DialogResult.OK)
                {
                    MessageBox.Show("Thanks, " + checkIn.RegisteredName + ". A cashier can now find your customer record.", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnSetupManager_Click(object sender, EventArgs e)
        {
            using (Registration registration = new Registration())
            {
                if (registration.ShowDialog(this) == DialogResult.OK)
                {
                    txtName.Text = registration.RegisteredName;
                    txtPassword.Clear();
                    txtPassword.Focus();
                    RefreshManagerSetupVisibility();
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                txtPassword.UseSystemPasswordChar = false;
            else
                txtPassword.UseSystemPasswordChar = true;
        }
    }
}
