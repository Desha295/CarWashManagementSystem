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
    public partial class SettlePayment : Form
    {
        SqlCommand cm = new SqlCommand();
        dbConnect dbcon = new dbConnect();        
        string title = "Car Wash Management System";
        Cash cash;
        public SettlePayment(Cash cashform)
        {
            InitializeComponent();
            cash = cashform;
        }

        private void picClose_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void btn0_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn0.Text;
        }

        private void btnPoint_Click(object sender, EventArgs e)
        {
            txtCash.Text += btnPoint.Text;

        }

        private void btn1_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn1.Text;
        }

        private void btn2_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn2.Text;
        }

        private void btn3_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn3.Text;
        }

        private void btn4_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn4.Text;
        }

        private void btn5_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn5.Text;
        }

        private void btn6_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn6.Text;
        }

        private void btn7_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn7.Text;
        }

        private void btn8_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn8.Text;
        }

        private void btn9_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn9.Text;
        }

        private void btn00_Click(object sender, EventArgs e)
        {
            txtCash.Text += btn00.Text;
        }

        private void btnClean_Click(object sender, EventArgs e)
        {
            txtCash.Clear();
            txtCash.Focus();// cusor focus int textbox of cash
        }

        private void btnEnter_Click(object sender, EventArgs e)
        {
            if (cash.dgvCash.Rows.Count == 0)
            {
                MessageBox.Show("There are no services to pay for.", "Payment", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (cash.customerId <= 0)
            {
                MessageBox.Show("Select a customer before taking payment.", "Payment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal total;
            decimal amountReceived;
            if (!TryReadAmount(txtSale.Text, out total) || !TryReadAmount(txtCash.Text, out amountReceived))
            {
                MessageBox.Show("Enter a valid amount received.", "Payment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCash.Focus();
                return;
            }
            if (amountReceived < total)
            {
                MessageBox.Show("The amount received is less than the total.", "Insufficient payment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal change = amountReceived - total;
            SqlTransaction transaction = null;
            try
            {
                dbcon.open();
                transaction = dbcon.connect().BeginTransaction();
                foreach (DataGridViewRow row in cash.dgvCash.Rows)
                {
                    using (SqlCommand updateSale = new SqlCommand("UPDATE tbCash SET status=@status,price=@price WHERE id=@id AND status=@pending", dbcon.connect(), transaction))
                    {
                        updateSale.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = "Sold";
                        SqlParameter priceParameter = updateSale.Parameters.Add("@price", SqlDbType.Decimal);
                        priceParameter.Precision = 18;
                        priceParameter.Scale = 2;
                        priceParameter.Value = Convert.ToDecimal(row.Cells[9].Value, CultureInfo.CurrentCulture);
                        updateSale.Parameters.Add("@id", SqlDbType.Int).Value = Convert.ToInt32(row.Cells[1].Value);
                        updateSale.Parameters.Add("@pending", SqlDbType.NVarChar, 20).Value = "Pending";
                        if (updateSale.ExecuteNonQuery() != 1)
                            throw new InvalidOperationException("One of the services is no longer pending. Reload the transaction and try again.");
                    }
                }

                using (SqlCommand addPoint = new SqlCommand("UPDATE tbCustomer SET points = ISNULL(points, 0) + 1 WHERE id=@id", dbcon.connect(), transaction))
                {
                    addPoint.Parameters.Add("@id", SqlDbType.Int).Value = cash.customerId;
                    addPoint.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    try { transaction.Rollback(); } catch { }
                }
                MessageBox.Show("Payment wasn't saved.\n\n" + ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                if (transaction != null) transaction.Dispose();
                dbcon.close();
            }

            try
            {
                receipt module = new receipt(cash);
                module.loadReceipt(txtCash.Text, change.ToString("N2"));
                module.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Payment was saved, but the receipt couldn't be displayed.\n\n" + ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            MessageBox.Show("Payment saved successfully.", "Payment", MessageBoxButtons.OK, MessageBoxIcon.Information);
            cash.loadCash();
            Dispose();
            cash.getTransno();
        }

        private void txtCash_TextChanged(object sender, EventArgs e)
        {
            decimal total;
            decimal received;
            txtChange.Text = TryReadAmount(txtSale.Text, out total) && TryReadAmount(txtCash.Text, out received)
                ? (received - total).ToString("N2")
                : "0.00";
        }

        private static bool TryReadAmount(string value, out decimal amount)
        {
            amount = 0m;
            if (String.IsNullOrWhiteSpace(value)) return false;
            string normalized = value.Trim()
                .Replace(CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator, String.Empty)
                .Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".");
            return Decimal.TryParse(normalized, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out amount);
        }

        private void SettlePayment_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.Enter)
            {
                btnEnter.PerformClick();// action click enter button
            }
            else if(e.KeyCode==Keys.Escape)
            {
                this.Dispose();
            }
        }
    }
}
