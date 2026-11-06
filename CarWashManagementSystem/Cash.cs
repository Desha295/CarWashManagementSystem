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
    public partial class Cash : Form
    {
        SqlCommand cm = new SqlCommand();
        dbConnect dbcon = new dbConnect();
        SqlDataReader dr;
        string title = "Car Wash Management System";
        public int customerId = 0, vehicleTypeId = 0;
        public string carno, carmodel;
        MainForm main;
        private Label lblWorkflow;
        public Cash(MainForm mainForm)
        {
            InitializeComponent();
            ApplyVisualStyle();
            getTransno();
            loadCash();
            main = mainForm;
        }

        private void btnAddCustomer_Click(object sender, EventArgs e)
        {
            customerId = 0;
            vehicleTypeId = 0;
            btnAddService.Enabled = false;
            UpdateWorkflowStatus();
            openChildForm(new CashCustomer(this));
        }

        private void btnAddService_Click(object sender, EventArgs e)
        {
            if (customerId <= 0 || vehicleTypeId <= 0)
            {
                MessageBox.Show("Select a customer and vehicle before adding services.", title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            openChildForm(new CashService(this));
            btnAddCustomer.Enabled = false;
        }

        private void btnCash_Click(object sender, EventArgs e)
        {
            if (dgvCash.Rows.Count == 0)
            {
                MessageBox.Show("Add at least one service before taking payment.", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SettlePayment module = new SettlePayment(this);
            module.txtSale.Text = lblTotal.Text;
            module.ShowDialog();            
            if (main != null) main.loadGrossProfit();
        }

        #region method
        private void ApplyVisualStyle()
        {
            Color navy = Color.FromArgb(26, 43, 64);
            Color teal = Color.FromArgb(20, 145, 160);
            BackColor = Color.FromArgb(246, 249, 251);
            panel1.BackColor = Color.White;
            panel3.BackColor = navy;
            panel4.BackColor = Color.White;
            foreach (Button button in new[] { btnAddCustomer, btnAddService })
            {
                button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                button.ForeColor = navy;
                button.FlatAppearance.MouseOverBackColor = Color.FromArgb(224, 241, 243);
            }
            btnCash.BackColor = teal;
            btnCash.ForeColor = Color.White;
            btnCash.FlatAppearance.BorderSize = 0;
            btnAddCustomer.Text = "1  Select customer";
            btnAddCustomer.Size = new Size(175, 50);
            btnAddService.Text = "2  Add services";
            btnAddService.Location = new Point(185, 3);
            btnAddService.Size = new Size(165, 50);
            btnCash.Text = "Take payment";
            btnCash.Image = null;
            btnCash.Size = new Size(145, 50);
            btnCash.Location = new Point(panel4.Width - btnCash.Width - 12, 6);
            btnCash.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblWorkflow = new Label
            {
                AutoSize = true,
                Location = new Point(370, 21),
                ForeColor = Color.FromArgb(70, 86, 100),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            panel1.Controls.Add(lblWorkflow);
            dgvCash.EnableHeadersVisualStyles = false;
            dgvCash.ColumnHeadersDefaultCellStyle.BackColor = navy;
            dgvCash.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCash.ColumnHeadersDefaultCellStyle.SelectionBackColor = navy;
            dgvCash.DefaultCellStyle.SelectionBackColor = Color.FromArgb(213, 238, 239);
            dgvCash.DefaultCellStyle.SelectionForeColor = navy;
            dgvCash.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(244, 248, 250);
            dgvCash.RowTemplate.Height = 36;
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
            panelCash.Height = 200;
            panelCash.Controls.Add(childForm);
            panelCash.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        // Create a function for transatoin generator depend on date

        public void getTransno()
        {
            try
            {
                string sdate = DateTime.Now.ToString("yyyyMMdd");
                int count;
                customerId = 0;
                vehicleTypeId = 0;
                dbcon.open();
                using (cm = new SqlCommand("SELECT MAX(TRY_CONVERT(int, RIGHT(transno, 4))) FROM tbCash WHERE transno LIKE @prefix", dbcon.connect()))
                {
                    cm.Parameters.AddWithValue("@prefix", sdate + "%");
                    object result = cm.ExecuteScalar();
                    count = result == DBNull.Value ? 1000 : Convert.ToInt32(result);
                    lblTransno.Text = sdate + (count + 1).ToString("D4");
                }
                dbcon.close();
                btnAddCustomer.Enabled = true;
                btnAddService.Enabled = false;
                btnCash.Enabled = false;
                UpdateWorkflowStatus();
            }
            catch (Exception ex)
            {
                dbcon.close();
                MessageBox.Show(ex.Message,title);
            }
        }

        private void dgvCash_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string colName = dgvCash.Columns[e.ColumnIndex].Name;
            if (colName == "Delete") // if you want to delete the record to click the delete icon on the datagridview
            {
                try
                {
                    if (MessageBox.Show("Are you sure you want to cancle this service?", "Cancel Services", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        cm = new SqlCommand("DELETE FROM tbCash WHERE id LIKE'" + dgvCash.Rows[e.RowIndex].Cells[1].Value.ToString() + "'", dbcon.connect());
                        dbcon.open();
                        cm.ExecuteNonQuery();
                        dbcon.close();
                        MessageBox.Show("Service has been successfully Canceled!", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, title);
                }
            }
            loadCash();
        }

        private void Cash_Load(object sender, EventArgs e)
        {
        }

        public void loadCash()
        {
            int i = 0;
            decimal total = 0m;
            dgvCash.Rows.Clear();
            try
            {
                dbcon.open();
                using (cm = new SqlCommand("SELECT ca.id,ca.transno,cu.name,cu.carno,cu.carmodel,v.name,v.class,s.name,ca.price,ca.date FROM tbCash AS ca " +
                    "LEFT JOIN tbCustomer AS cu ON ca.cid = cu.id LEFT JOIN tbService AS s ON ca.sid = s.id LEFT JOIN tbVehicleType AS v ON ca.vid = v.id " +
                    "WHERE ca.status = @status AND ca.transno = @transno ORDER BY ca.id", dbcon.connect()))
                {
                    cm.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = "Pending";
                    cm.Parameters.Add("@transno", SqlDbType.NVarChar, 30).Value = lblTransno.Text;
                    using (dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            i++;
                            decimal linePrice = Convert.ToInt32(dr[6]) * Convert.ToDecimal(dr[8]);
                            dgvCash.Rows.Add(i, dr[0], dr[1], dr[2], dr[3], dr[4], dr[5], dr[6], dr[7], linePrice, dr[9]);
                            total += linePrice;
                            carno = Convert.ToString(dr[3]);
                            carmodel = Convert.ToString(dr[4]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't load the current transaction.\n\n" + ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                dbcon.close();
            }
            lblTotal.Text = total.ToString("N2");
            btnCash.Enabled = dgvCash.Rows.Count > 0;
            UpdateWorkflowStatus();
        }

        public void UpdateWorkflowStatus(string selectedCustomerName = null)
        {
            if (lblWorkflow == null) return;
            if (dgvCash.Rows.Count > 0)
                lblWorkflow.Text = "Step 3: review " + dgvCash.Rows.Count + " service(s), then take payment.";
            else if (customerId > 0)
                lblWorkflow.Text = String.IsNullOrWhiteSpace(selectedCustomerName)
                    ? "Step 2: add services for the selected customer."
                    : "Step 2: add services for " + selectedCustomerName + ".";
            else
                lblWorkflow.Text = "Step 1: select an existing customer or add a new one.";
        }
        #endregion method
    }

}
