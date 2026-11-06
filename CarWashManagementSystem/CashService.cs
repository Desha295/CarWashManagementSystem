using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CarWashManagementSystem
{
    public partial class CashService : Form
    {
        SqlCommand cm = new SqlCommand();
        dbConnect dbcon = new dbConnect();
        SqlDataReader dr;
        string title = "Car Wash Management System";
        Cash cash;
        public CashService(Cash cashForm)
        {
            InitializeComponent();
            cash = cashForm;
            ApplyVisualStyle();
            loadService();
        }

        private void textSearch_TextChanged(object sender, EventArgs e)
        {
            loadService();
        }



        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (cash.customerId <= 0 || cash.vehicleTypeId <= 0)
            {
                MessageBox.Show("Select a customer and vehicle before adding services.", title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dgvService.EndEdit();
            var selectedRows = dgvService.Rows.Cast<DataGridViewRow>()
                .Where(row => row.Cells["Select"].Value is bool selected && selected)
                .ToList();
            if (selectedRows.Count == 0)
            {
                MessageBox.Show("Select at least one service.", title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int insertedCount = 0;
            SqlTransaction transaction = null;
            try
            {
                dbcon.open();
                transaction = dbcon.connect().BeginTransaction();
                foreach (DataGridViewRow row in selectedRows)
                {
                    decimal price = Convert.ToDecimal(row.Cells[3].Value, CultureInfo.CurrentCulture);
                    using (SqlCommand command = new SqlCommand(
                        "INSERT INTO tbCash(transno,cid,sid,vid,price,date,status) " +
                        "SELECT @transno,@cid,@sid,@vid,@price,@date,@status " +
                        "WHERE NOT EXISTS (SELECT 1 FROM tbCash WHERE sid=@sid AND transno=@transno)",
                        dbcon.connect(), transaction))
                    {
                        command.Parameters.Add("@transno", SqlDbType.NVarChar, 30).Value = cash.lblTransno.Text;
                        command.Parameters.Add("@cid", SqlDbType.Int).Value = cash.customerId;
                        command.Parameters.Add("@sid", SqlDbType.Int).Value = Convert.ToInt32(row.Cells[1].Value);
                        command.Parameters.Add("@vid", SqlDbType.Int).Value = cash.vehicleTypeId;
                        SqlParameter priceParameter = command.Parameters.Add("@price", SqlDbType.Decimal);
                        priceParameter.Precision = 18;
                        priceParameter.Scale = 2;
                        priceParameter.Value = price;
                        command.Parameters.Add("@date", SqlDbType.DateTime).Value = DateTime.Now;
                        command.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = "Pending";
                        insertedCount += command.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    try { transaction.Rollback(); } catch { }
                }
                MessageBox.Show("The selected services couldn't be added. Check the customer, vehicle type, and service price.\n\n" + ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                if (transaction != null) transaction.Dispose();
                dbcon.close();
            }

            if (insertedCount > 0)
            {
                cash.btnCash.Enabled = true;
                cash.loadCash();
            }
            else
                MessageBox.Show("Those services are already on this transaction.", title, MessageBoxButtons.OK, MessageBoxIcon.Information);

            Dispose();
            cash.panelCash.Height = 1;
        }

        #region method
        public void loadService()
        {
            try
            {
                int i = 0;
                dgvService.Rows.Clear();
                dbcon.open();
                using (cm = new SqlCommand("SELECT id,name,price FROM tbService WHERE name LIKE @search ORDER BY name", dbcon.connect()))
                {
                    cm.Parameters.Add("@search", SqlDbType.NVarChar, 100).Value = "%" + txtSearch.Text.Trim() + "%";
                    using (dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            i++;
                            dgvService.Rows.Add(i, dr["id"], dr["name"], dr["price"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't load services.\n\n" + ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            panel1.BackColor = navy;
            btnSubmit.BackColor = teal;
            btnSubmit.ForeColor = Color.White;
            btnSubmit.Text = "Add selected";
            btnSubmit.Image = null;
            btnSubmit.TextAlign = ContentAlignment.MiddleCenter;
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.BackColor = Color.FromArgb(248, 250, 252);
            dgvService.ColumnHeadersDefaultCellStyle.BackColor = navy;
            dgvService.ColumnHeadersDefaultCellStyle.SelectionBackColor = navy;
            dgvService.DefaultCellStyle.SelectionBackColor = Color.FromArgb(213, 238, 239);
            dgvService.DefaultCellStyle.SelectionForeColor = navy;
            dgvService.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(244, 248, 250);
            dgvService.RowTemplate.Height = 36;
            dgvService.Columns["Column4"].DefaultCellStyle.Format = "N2";
            dgvService.Columns["Select"].Width = 80;
        }
        #endregion method
    }
}
