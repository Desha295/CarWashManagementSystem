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
    public partial class CashCustomer : Form
    {
        SqlCommand cm = new SqlCommand();
        dbConnect dbcon = new dbConnect();
        SqlDataReader dr;
        string title = "Car Wash Management System";
        Cash cash;
        private readonly Button btnNewCustomer = new Button();
        public CashCustomer(Cash cashform)
        {
            InitializeComponent();
            cash = cashform;
            AddSelectionHint();
            ConfigureNewCustomerButton();
            ApplyVisualStyle();
            loadCustomer();
        }

        private void AddSelectionHint()
        {
            Panel hintPanel = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = Color.FromArgb(26, 43, 64) };
            hintPanel.Controls.Add(new Label
            {
                Text = "Click anywhere on a row to select the customer, or register a new car.",
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(12, 7)
            });
            Controls.Add(hintPanel);
            hintPanel.BringToFront();
        }

        private void ConfigureNewCustomerButton()
        {
            btnNewCustomer.Text = "New customer";
            btnNewCustomer.Size = new Size(150, 32);
            btnNewCustomer.Location = new Point(panel1.Width - btnNewCustomer.Width, 0);
            btnNewCustomer.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNewCustomer.FlatStyle = FlatStyle.Flat;
            btnNewCustomer.FlatAppearance.BorderSize = 0;
            btnNewCustomer.BackColor = Color.FromArgb(20, 145, 160);
            btnNewCustomer.ForeColor = Color.White;
            btnNewCustomer.Click += btnNewCustomer_Click;
            panel1.Controls.Add(btnNewCustomer);
            textSearch.Width = Math.Max(200, panel1.Width - 129 - btnNewCustomer.Width);
            panel1.Resize += (sender, e) =>
            {
                btnNewCustomer.Location = new Point(panel1.Width - btnNewCustomer.Width, 0);
                textSearch.Width = Math.Max(200, panel1.Width - 129 - btnNewCustomer.Width);
            };
        }

        private void btnNewCustomer_Click(object sender, EventArgs e)
        {
            using (CustomerCheckIn checkIn = new CustomerCheckIn())
            {
                if (checkIn.ShowDialog() != DialogResult.OK)
                    return;

                textSearch.Clear();
                loadCustomer();
                if (checkIn.RegisteredCustomerId > 0 && !String.IsNullOrWhiteSpace(checkIn.RegisteredTransactionNumber))
                {
                    cash.customerId = checkIn.RegisteredCustomerId;
                    cash.vehicleTypeId = checkIn.RegisteredVehicleTypeId;
                    cash.lblTransno.Text = checkIn.RegisteredTransactionNumber;
                    cash.btnAddCustomer.Enabled = false;
                    cash.btnAddService.Enabled = true;
                    cash.loadCash();
                    cash.UpdateWorkflowStatus(checkIn.RegisteredName);
                    cash.panelCash.Height = 1;
                    Dispose();
                    return;
                }
                foreach (DataGridViewRow row in dgvCustomer.Rows)
                {
                    if (String.Equals(Convert.ToString(row.Cells[5].Value), checkIn.RegisteredCarNumber, StringComparison.OrdinalIgnoreCase))
                    {
                        cash.customerId = Convert.ToInt32(row.Cells[1].Value);
                        cash.vehicleTypeId = Convert.ToInt32(row.Cells[2].Value);
                        LoadPendingCustomerRequest(cash.customerId);
                        cash.btnAddCustomer.Enabled = false;
                        cash.btnAddService.Enabled = true;
                        cash.loadCash();
                        cash.UpdateWorkflowStatus(checkIn.RegisteredName);
                        cash.panelCash.Height = 1;
                        Dispose();
                        return;
                    }
                }
                MessageBox.Show("The customer is registered. Search by name or plate number to select them.", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void textSearch_TextChanged(object sender, EventArgs e)
        {
            loadCustomer();
        }

        private void dgvCustomer_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectedRow = dgvCustomer.Rows[e.RowIndex];
                cash.customerId = Convert.ToInt32(selectedRow.Cells[1].Value);
                cash.vehicleTypeId = Convert.ToInt32(selectedRow.Cells[2].Value);
                LoadPendingCustomerRequest(cash.customerId);
                cash.btnAddCustomer.Enabled = false;
                cash.btnAddService.Enabled = true;
                cash.loadCash();
                cash.UpdateWorkflowStatus(Convert.ToString(selectedRow.Cells[3].Value));
            }
            else return;
            this.Dispose();
            cash.panelCash.Height = 1;
        }

        #region method
        private void LoadPendingCustomerRequest(int customerId)
        {
            if (cash.dgvCash.Rows.Count > 0) return;

            try
            {
                dbcon.open();
                using (SqlCommand command = new SqlCommand(
                    "SELECT TOP 1 transno FROM tbCash WHERE cid=@cid AND status=@status ORDER BY date, transno", dbcon.connect()))
                {
                    command.Parameters.Add("@cid", SqlDbType.Int).Value = customerId;
                    command.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = "Pending";
                    object existingRequest = command.ExecuteScalar();
                    if (existingRequest != null && existingRequest != DBNull.Value)
                        cash.lblTransno.Text = Convert.ToString(existingRequest);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't load the customer's pending service request.\n\n" + ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                dbcon.close();
            }
        }

        // create function for load customer
        public void loadCustomer()
        {
            try
            {
                int i = 0;
                dgvCustomer.Rows.Clear();
                dbcon.open();
                using (cm = new SqlCommand("SELECT * FROM tbCustomer WHERE CONCAT(name,phone,address) LIKE @search ORDER BY name", dbcon.connect()))
                {
                    cm.Parameters.Add("@search", SqlDbType.NVarChar, 250).Value = "%" + textSearch.Text.Trim() + "%";
                    using (dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            i++;
                            dgvCustomer.Rows.Add(i, dr[0], dr[1], dr[2], dr[3], dr[4], dr[5], dr[6], dr[7]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't load customers.\n\n" + ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                dbcon.close();
            }
        }

        private void ApplyVisualStyle()
        {
            Color navy = Color.FromArgb(26, 43, 64);
            dgvCustomer.EnableHeadersVisualStyles = false;
            dgvCustomer.ColumnHeadersDefaultCellStyle.BackColor = navy;
            dgvCustomer.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCustomer.ColumnHeadersDefaultCellStyle.SelectionBackColor = navy;
            dgvCustomer.DefaultCellStyle.SelectionBackColor = Color.FromArgb(213, 238, 239);
            dgvCustomer.DefaultCellStyle.SelectionForeColor = navy;
            dgvCustomer.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(244, 248, 250);
            dgvCustomer.RowTemplate.Height = 36;
            dgvCustomer.Columns["Select"].Width = 80;
            dgvCustomer.Columns["Select"].DefaultCellStyle.BackColor = Color.FromArgb(20, 145, 160);
            dgvCustomer.Columns["Select"].DefaultCellStyle.ForeColor = Color.White;
            dgvCustomer.Columns["Select"].DefaultCellStyle.SelectionBackColor = Color.FromArgb(20, 145, 160);
            panel1.BackColor = Color.FromArgb(26, 43, 64);
            textSearch.BorderStyle = BorderStyle.FixedSingle;
        }
        #endregion method
    }
}
