using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace CarWashManagementSystem
{
    public sealed class WorkerHome : Form
    {
        private readonly DataGridView grid = new DataGridView();
        private readonly Label lblStatus = new Label();
        private readonly Label lblEmailStatus = new Label();
        private readonly Button btnAdvance = new Button();
        private readonly Button btnRefresh = new Button();
        private readonly Timer refreshTimer = new Timer();
        private readonly dbConnect dbcon = new dbConnect();
        private bool isLoading;

        public WorkerHome()
        {
            Text = "Wash Queue";
            BackColor = Color.FromArgb(246, 249, 251);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.None;

            Panel header = new Panel { Dock = DockStyle.Top, Height = 108, BackColor = Color.FromArgb(26, 43, 64) };
            header.Controls.Add(new Label
            {
                Text = "Wash queue",
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 21F, FontStyle.Bold),
                Location = new Point(28, 18)
            });
            header.Controls.Add(new Label
            {
                Text = "Select an order and move it through washing, ready, and delivered.",
                AutoSize = true,
                ForeColor = Color.FromArgb(210, 224, 234),
                Location = new Point(32, 62)
            });

            grid.Dock = DockStyle.Fill;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ColumnHeadersHeight = 40;
            grid.RowTemplate.Height = 38;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(26, 43, 64);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(26, 43, 64);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(213, 238, 239);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(26, 43, 64);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(244, 248, 250);
            grid.Columns.Add("Transaction", "Order");
            grid.Columns.Add("Customer", "Customer");
            grid.Columns.Add("Car", "Car");
            grid.Columns.Add("VehicleType", "Vehicle type");
            grid.Columns.Add("Services", "Services");
            grid.Columns.Add("Added", "Added");
            grid.Columns.Add("WashStatus", "Wash status");
            grid.Columns.Add("Phone", "Phone");
            grid.Columns["Phone"].Visible = false;
            grid.Columns.Add("Email", "Email");
            grid.Columns["Email"].Visible = false;
            grid.Columns["Transaction"].FillWeight = 90;
            grid.Columns["Customer"].FillWeight = 110;
            grid.Columns["Car"].FillWeight = 145;
            grid.Columns["VehicleType"].FillWeight = 105;
            grid.Columns["Services"].FillWeight = 160;
            grid.Columns["Added"].FillWeight = 75;
            grid.Columns["WashStatus"].FillWeight = 95;
            grid.SelectionChanged += grid_SelectionChanged;

            Panel footer = new Panel { Dock = DockStyle.Bottom, Height = 82, BackColor = Color.White };
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(28, 8);
            lblStatus.ForeColor = Color.FromArgb(90, 105, 115);
            lblEmailStatus.AutoSize = true;
            lblEmailStatus.Location = new Point(28, 43);
            lblEmailStatus.ForeColor = Color.FromArgb(90, 105, 115);

            btnAdvance.Text = "Select an order";
            btnAdvance.Size = new Size(180, 38);
            btnAdvance.Enabled = false;
            btnAdvance.BackColor = Color.FromArgb(20, 145, 160);
            btnAdvance.ForeColor = Color.White;
            btnAdvance.FlatStyle = FlatStyle.Flat;
            btnAdvance.FlatAppearance.BorderSize = 0;
            btnAdvance.Click += btnAdvance_Click;

            btnRefresh.Text = "Refresh";
            btnRefresh.Size = new Size(105, 38);
            btnRefresh.BackColor = Color.FromArgb(231, 237, 241);
            btnRefresh.ForeColor = Color.FromArgb(26, 43, 64);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (sender, e) => LoadQueue();

            footer.Resize += (sender, e) =>
            {
                btnRefresh.Location = new Point(footer.Width - btnRefresh.Width - 24, 12);
                btnAdvance.Location = new Point(footer.Width - btnRefresh.Width - btnAdvance.Width - 36, 12);
            };
            footer.Controls.Add(lblStatus);
            footer.Controls.Add(lblEmailStatus);
            footer.Controls.Add(btnAdvance);
            footer.Controls.Add(btnRefresh);

            Controls.Add(grid);
            Controls.Add(footer);
            Controls.Add(header);
            LoadQueue();

            refreshTimer.Interval = 30000;
            refreshTimer.Tick += (sender, e) => LoadQueue();
            refreshTimer.Start();
            FormClosed += (sender, e) => refreshTimer.Stop();
        }

        private void LoadQueue()
        {
            if (isLoading) return;
            isLoading = true;
            grid.Rows.Clear();
            btnAdvance.Enabled = false;
            btnAdvance.Text = "Select an order";
            DateTime today = DateTime.Today;
            try
            {
                dbcon.open();
                const string sql =
                    "SELECT ca.transno, MAX(cu.name) AS customer, MAX(cu.phone) AS phone, MAX(cu.email) AS email, MAX(cu.carno) AS carno, " +
                    "MAX(cu.carmodel) AS carmodel, MAX(v.name) AS vehicle, " +
                    "STUFF((SELECT ', ' + COALESCE(s2.name, '') FROM tbCash c2 " +
                    "LEFT JOIN tbService s2 ON c2.sid = s2.id WHERE c2.transno = ca.transno FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, '') AS services, " +
                    "MIN(ca.date) AS orderdate, MAX(ca.washstatus) AS washstatus " +
                    "FROM tbCash ca " +
                    "LEFT JOIN tbCustomer cu ON ca.cid = cu.id " +
                    "LEFT JOIN tbVehicleType v ON ca.vid = v.id " +
                    "WHERE ca.status = @paid AND ca.washstatus <> @delivered " +
                    "AND ca.date >= @start AND ca.date < @end " +
                    "GROUP BY ca.transno ORDER BY MIN(ca.date), ca.transno";

                using (SqlCommand command = new SqlCommand(sql, dbcon.connect()))
                {
                    command.Parameters.Add("@paid", SqlDbType.NVarChar, 20).Value = "Sold";
                    command.Parameters.Add("@delivered", SqlDbType.NVarChar, 20).Value = "Delivered";
                    command.Parameters.Add("@start", SqlDbType.DateTime).Value = today;
                    command.Parameters.Add("@end", SqlDbType.DateTime).Value = today.AddDays(1);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string car = Convert.ToString(reader["carno"]);
                            string model = Convert.ToString(reader["carmodel"]);
                            if (!String.IsNullOrWhiteSpace(model)) car += "  |  " + model;
                            grid.Rows.Add(
                                reader["transno"],
                                reader["customer"] == DBNull.Value ? "Walk-in" : reader["customer"],
                                String.IsNullOrWhiteSpace(car) ? "—" : car,
                                reader["vehicle"] == DBNull.Value ? "—" : reader["vehicle"],
                                String.IsNullOrWhiteSpace(Convert.ToString(reader["services"])) ? "—" : reader["services"],
                                Convert.ToDateTime(reader["orderdate"]).ToString("h:mm tt"),
                                reader["washstatus"],
                                reader["phone"] == DBNull.Value ? "" : reader["phone"],
                                reader["email"] == DBNull.Value ? "" : reader["email"]);
                        }
                    }
                }

                lblStatus.Text = grid.Rows.Count == 0
                    ? "No active wash orders today. Paid orders will appear here automatically."
                    : grid.Rows.Count + " active order(s). Select one to update its wash status.";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Couldn't load the wash queue: " + ex.Message;
            }
            finally
            {
                dbcon.close();
                isLoading = false;
            }
        }

        private void grid_SelectionChanged(object sender, EventArgs e)
        {
            if (grid.CurrentRow == null || grid.CurrentRow.IsNewRow)
            {
                btnAdvance.Enabled = false;
                btnAdvance.Text = "Select an order";
                return;
            }

            string status = Convert.ToString(grid.CurrentRow.Cells["WashStatus"].Value);
            btnAdvance.Enabled = true;
            if (status == "Waiting") btnAdvance.Text = "Start washing";
            else if (status == "In wash") btnAdvance.Text = "Mark ready";
            else if (status == "Ready") btnAdvance.Text = "Mark delivered";
            else btnAdvance.Enabled = false;
        }

        private void btnAdvance_Click(object sender, EventArgs e)
        {
            if (grid.CurrentRow == null) return;
            string transactionNumber = Convert.ToString(grid.CurrentRow.Cells["Transaction"].Value);
            string currentStatus = Convert.ToString(grid.CurrentRow.Cells["WashStatus"].Value);
            string nextStatus;
            string timestampColumn;
            string customerName = Convert.ToString(grid.CurrentRow.Cells["Customer"].Value);
            string car = Convert.ToString(grid.CurrentRow.Cells["Car"].Value);
            string phone = Convert.ToString(grid.CurrentRow.Cells["Phone"].Value);
            string email = Convert.ToString(grid.CurrentRow.Cells["Email"].Value);
            string services = Convert.ToString(grid.CurrentRow.Cells["Services"].Value);
            if (String.Equals(car, "—", StringComparison.Ordinal)) car = "";
            if (currentStatus == "Waiting")
            {
                nextStatus = "In wash";
                timestampColumn = "washstartedat";
            }
            else if (currentStatus == "In wash")
            {
                nextStatus = "Ready";
                timestampColumn = "washreadyat";
            }
            else if (currentStatus == "Ready")
            {
                nextStatus = "Delivered";
                timestampColumn = "washdeliveredat";
            }
            else return;

            string sql = "UPDATE tbCash SET washstatus=@next, " + timestampColumn + "=GETDATE() " +
                         "WHERE transno=@transno AND status=@paid AND washstatus=@current";
            try
            {
                dbcon.open();
                using (SqlCommand command = new SqlCommand(sql, dbcon.connect()))
                {
                    command.Parameters.Add("@next", SqlDbType.NVarChar, 20).Value = nextStatus;
                    command.Parameters.Add("@transno", SqlDbType.NVarChar, 30).Value = transactionNumber;
                    command.Parameters.Add("@paid", SqlDbType.NVarChar, 20).Value = "Sold";
                    command.Parameters.Add("@current", SqlDbType.NVarChar, 20).Value = currentStatus;
                    if (command.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException("This order changed in another session. Refresh the queue and try again.");
                }
                if (nextStatus == "Ready")
                {
                    lblStatus.Text = "Order ready. Sending WhatsApp notification...";
                    Task.Run(() => WhatsAppNotifier.SendReadyMessage(phone, customerName, car))
                        .ContinueWith(task =>
                        {
                            if (IsDisposed || !IsHandleCreated) return;
                            BeginInvoke((Action)(() =>
                            {
                                if (task.IsFaulted)
                                    lblStatus.Text = "Order ready, but WhatsApp notification failed: " + task.Exception.GetBaseException().Message;
                                else if (task.Result)
                                    lblStatus.Text = "Order ready. WhatsApp notification sent to " + phone + ".";
                                else
                                    lblStatus.Text = "Order ready. WhatsApp is not configured or the customer has no phone number.";
                            }));
                        });
                    NotifyCustomerByEmail(customerName, email, car, services);
                }
                LoadQueue();
            }
            catch (Exception ex)
            {
                MessageBox.Show("The wash status couldn't be updated.\n\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                dbcon.close();
            }
        }

        private async void NotifyCustomerByEmail(string customerName, string email, string car, string services)
        {
            if (String.IsNullOrWhiteSpace(email))
            {
                lblEmailStatus.Text = "No customer email saved; email notification skipped.";
                return;
            }

            lblEmailStatus.Text = "Sending customer email through Brevo...";
            try
            {
                await BrevoNotifier.SendReadyEmailAsync(customerName, email, car, services);
                if (!IsDisposed) lblEmailStatus.Text = "Brevo accepted the email for " + email + ". Check the inbox and Spam/Junk folders.";
            }
            catch (Exception ex)
            {
                if (!IsDisposed) lblEmailStatus.Text = "Email notification failed: " + ex.Message;
            }
        }
    }
}
