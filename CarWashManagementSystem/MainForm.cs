using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CarWashManagementSystem
{
    public partial class MainForm : Form
    {
        private readonly string currentRole;

        public MainForm(string role)
        {
            InitializeComponent();
            FormClosed += MainForm_FormClosed;
            currentRole = NormalizeRole(role);
            ApplyRoleAccess();
            if (currentRole == "Worker")
                openChildForm(new WorkerHome());
            else if (currentRole == "Cashier")
                openChildForm(new Cash(this));
            else
            {
                loadGrossProfit();
                openChildForm(new Dashboard());
            }

        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            if (!RequireRole("Manager", "Supervisor", "Worker")) return;
            panelSlide.Height = btnDashboard.Height;
            panelSlide.Top = btnDashboard.Top;
            if (currentRole != "Worker") loadGrossProfit();
            openChildForm(currentRole == "Worker" ? (Form)new WorkerHome() : new Dashboard());
        }

        private void btnEmployer_Click(object sender, EventArgs e)
        {
            if (!RequireRole("Manager")) return;
            panelSlide.Height = btnEmployer.Height;
            panelSlide.Top = btnEmployer.Top;
            openChildForm(new Employer());
        }

        private void btnCustomer_Click(object sender, EventArgs e)
        {
            if (!RequireRole("Manager", "Supervisor")) return;
            panelSlide.Height = btnCustomer.Height;
            panelSlide.Top = btnCustomer.Top;
            openChildForm(new Customer());
        }

        private void btnService_Click(object sender, EventArgs e)
        {
            if (!RequireRole("Manager", "Supervisor")) return;
            panelSlide.Height = btnService.Height;
            panelSlide.Top = btnService.Top;
            openChildForm(new Service());
        }

        private void btncash_Click(object sender, EventArgs e)
        {
            if (!RequireRole("Manager", "Supervisor", "Cashier")) return;
            panelSlide.Height = btncash.Height;
            panelSlide.Top = btncash.Top;
            openChildForm(new Cash(this));
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            if (!RequireRole("Manager", "Supervisor")) return;
            panelSlide.Height = btnReport.Height;
            panelSlide.Top = btnReport.Top;
            openChildForm(new Report());
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            if (!RequireRole("Manager")) return;
            panelSlide.Height = btnSetting.Height;
            panelSlide.Top = btnSetting.Top;
            openChildForm(new Setting(this));
      
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            panelSlide.Height = btnLogout.Height;
            panelSlide.Top = btnLogout.Top;
            if (MessageBox.Show("Logout Application?", "Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Hide();
                Login login = new Login();
                login.ShowDialog();
            }
        }

        #region method
        private static string NormalizeRole(string role)
        {
            if (String.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase)) return "Manager";
            if (String.Equals(role, "Supervisor", StringComparison.OrdinalIgnoreCase)) return "Supervisor";
            if (String.Equals(role, "Cashier", StringComparison.OrdinalIgnoreCase)) return "Cashier";
            return "Worker";
        }

        private bool RequireRole(params string[] allowedRoles)
        {
            foreach (string allowedRole in allowedRoles)
                if (currentRole == allowedRole) return true;

            MessageBox.Show("Your account doesn't have permission to open this section.", "Access restricted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private void ApplyRoleAccess()
        {
            Color navy = Color.FromArgb(26, 43, 64);
            Color teal = Color.FromArgb(20, 145, 160);
            Color white = Color.FromArgb(246, 249, 251);
            bool isManager = currentRole == "Manager";
            bool isSupervisor = currentRole == "Supervisor";
            bool isCashier = currentRole == "Cashier";
            bool isWorker = currentRole == "Worker";
            panel2.BackColor = navy;
            panel4.BackColor = navy;
            panel3.BackColor = white;
            panelSlide.BackColor = teal;
            label1.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            label1.ForeColor = Color.White;
            label1.Location = new Point(20, 55);
            pictureBox1.Visible = false;
            foreach (Button button in new[] { btnLogout, btnSetting, btnReport, btncash, btnService, btnCustomer, btnEmployer, btnDashboard })
            {
                button.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
                button.ForeColor = Color.White;
                button.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 62, 88);
            }
            btnEmployer.Visible = isManager;
            btnSetting.Visible = isManager;
            btnDashboard.Visible = isManager || isSupervisor || isWorker;
            btnDashboard.Text = isWorker ? " Work Queue" : " Dashboard";
            btnCustomer.Visible = isManager || isSupervisor;
            btnService.Visible = isManager || isSupervisor;
            btnReport.Visible = isManager || isSupervisor;
            btncash.Visible = isManager || isSupervisor || isCashier;

            if (isWorker || isCashier)
            {
                panel3.Visible = false;
                panelChild.Location = new Point(200, 0);
                panelChild.Size = new Size(ClientSize.Width - 200, ClientSize.Height);
                Text = isWorker ? "ShineWash | Work Queue" : "ShineWash | Cashier";
            }
            else
                Text = "ShineWash | " + currentRole;
        }

        // create a function any form to the panelChild on the mainform

        private Form activeForm = null;
        public void openChildForm(Form childForm)
        {
            if (activeForm != null)
                activeForm.Close();
            activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            panelChild.Controls.Add(childForm);
            panelChild.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        // to extract data for dashboard
        // to show only seven day
        public void loadGrossProfit()
        {
            Report module = new Report();
            lblRevenus.Text = module.extractData("SELECT ISNULL(SUM(price),0) AS total FROM tbCash WHERE date >'" + DateTime.Now.AddDays(-7) + "' AND status LIKE 'Sold' ").ToString("#,##0.00");
            lblCostofGood.Text = module.extractData("SELECT ISNULL(SUM(cost),0) AS Cost FROM tbCostofGood WHERE date > '" + DateTime.Now.AddDays(-7) + "'").ToString("#,##0.00");
            lblGrossProfit.Text = (double.Parse(lblRevenus.Text) - double.Parse(lblCostofGood.Text)).ToString("#,##0.00");

            double revlast7 = module.extractData("SELECT ISNULL(SUM(price),0) AS total FROM tbCash WHERE date BETWEEN '" + DateTime.Now.AddDays(-14) + "' AND '" + DateTime.Now.AddDays(-7) + "' AND status LIKE 'Sold' ");
            double coglast7 = module.extractData("SELECT ISNULL(SUM(cost),0) AS Cost FROM tbCostofGood WHERE date BETWEEN  '" + DateTime.Now.AddDays(-14) + "' AND '" + DateTime.Now.AddDays(-7) + "'");
            double gplast7 = revlast7 - coglast7;


            if (revlast7 > double.Parse(lblRevenus.Text))
                picRevenus.Image = Properties.Resources.down_25px;
            else
                picRevenus.Image = Properties.Resources.up_25px;

            if (gplast7 > double.Parse(lblGrossProfit.Text))
            {
                picGrossProfit.Image = Properties.Resources.down_25px;
                lblGrossProfit.ForeColor = Color.Red;
            }
            else
            {
                picGrossProfit.Image = Properties.Resources.up_25px;
                lblGrossProfit.ForeColor = Color.Green;
            }
        }
        #endregion method

    }
}
