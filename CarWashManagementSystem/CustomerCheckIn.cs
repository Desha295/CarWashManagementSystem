using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Windows.Forms;

namespace CarWashManagementSystem
{
    public sealed class CustomerCheckIn : Form
    {
        private readonly TextBox txtName = new TextBox();
        private readonly TextBox txtPhone = new TextBox();
        private readonly TextBox txtEmail = new TextBox();
        private readonly TextBox txtCarNumber = new TextBox();
        private readonly TextBox txtCarModel = new TextBox();
        private readonly TextBox txtAddress = new TextBox();
        private readonly ComboBox cbVehicleType = new ComboBox();
        private readonly CheckedListBox clbServices = new CheckedListBox();
        private readonly Label lblStatus = new Label();

        public string RegisteredName { get; private set; }
        public string RegisteredCarNumber { get; private set; }
        public string RegisteredTransactionNumber { get; private set; }
        public int RegisteredCustomerId { get; private set; }
        public int RegisteredVehicleTypeId { get; private set; }

        public CustomerCheckIn()
        {
            Text = "Customer check-in | ShineWash";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(590, 800);
            BackColor = Color.FromArgb(246, 249, 251);
            Font = new Font("Segoe UI", 10F);

            Panel header = new Panel { Dock = DockStyle.Top, Height = 112, BackColor = Color.FromArgb(26, 43, 64) };
            header.Controls.Add(new Label
            {
                Text = "Welcome to ShineWash",
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                Location = new Point(32, 20)
            });
            header.Controls.Add(new Label
            {
                Text = "Register your car and choose the service you want.",
                AutoSize = true,
                ForeColor = Color.FromArgb(210, 224, 234),
                Location = new Point(36, 68)
            });
            Controls.Add(header);

            AddField("Your name", txtName, 148);
            AddField("Phone number", txtPhone, 204);
            AddField("Email (optional)", txtEmail, 260);
            AddField("Car plate number", txtCarNumber, 316);
            AddField("Car make / model", txtCarModel, 372);
            AddField("Address", txtAddress, 428);

            Controls.Add(new Label { Text = "Vehicle type", AutoSize = true, Location = new Point(40, 496), ForeColor = Color.FromArgb(48, 62, 72) });
            cbVehicleType.Location = new Point(220, 490);
            cbVehicleType.Size = new Size(320, 30);
            cbVehicleType.DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(cbVehicleType);

            Label lblServices = new Label
            {
                Text = "Choose your wash services",
                AutoSize = true,
                Location = new Point(40, 542),
                ForeColor = Color.FromArgb(26, 43, 64),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            Controls.Add(lblServices);
            clbServices.Location = new Point(40, 570);
            clbServices.Size = new Size(500, 112);
            clbServices.CheckOnClick = true;
            clbServices.BorderStyle = BorderStyle.FixedSingle;
            clbServices.IntegralHeight = false;
            clbServices.DrawMode = DrawMode.OwnerDrawFixed;
            clbServices.ItemHeight = 34;
            clbServices.BackColor = Color.White;
            clbServices.Font = new Font("Segoe UI", 9.5F);
            clbServices.DrawItem += clbServices_DrawItem;
            Controls.Add(clbServices);

            lblStatus.Location = new Point(40, 692);
            lblStatus.Size = new Size(500, 34);
            lblStatus.ForeColor = Color.FromArgb(90, 105, 115);
            Controls.Add(lblStatus);

            Button btnSave = new Button
            {
                Text = "Save my details",
                Location = new Point(220, 738),
                Size = new Size(155, 40),
                BackColor = Color.FromArgb(20, 145, 160),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += btnSave_Click;
            Controls.Add(btnSave);

            Button btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(390, 738),
                Size = new Size(110, 40),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(48, 62, 72),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (sender, e) => Close();
            Controls.Add(btnCancel);
            AcceptButton = btnSave;
            CancelButton = btnCancel;

            LoadVehicleTypes();
            LoadServices();
        }

        private void LoadServices()
        {
            dbConnect db = new dbConnect();
            try
            {
                db.open();
                using (SqlCommand command = new SqlCommand("SELECT id, name, price FROM tbService ORDER BY name", db.connect()))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clbServices.Items.Add(new ServiceChoice(
                            Convert.ToInt32(reader["id"]),
                            Convert.ToString(reader["name"]),
                            Convert.ToDecimal(reader["price"])));
                    }
                    if (clbServices.Items.Count == 0)
                    {
                        lblStatus.Text = "No wash services are available yet. Please ask staff for help.";
                        clbServices.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Couldn't load wash services: " + ex.Message;
                clbServices.Enabled = false;
            }
            finally
            {
                db.close();
            }
        }

        private void clbServices_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            ServiceChoice service = clbServices.Items[e.Index] as ServiceChoice;
            if (service == null) return;

            Color navy = Color.FromArgb(26, 43, 64);
            Color teal = Color.FromArgb(20, 145, 160);
            bool isChecked = clbServices.GetItemChecked(e.Index);
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color rowColor = isChecked ? Color.FromArgb(230, 246, 247) : Color.White;
            if (isSelected && !isChecked) rowColor = Color.FromArgb(244, 248, 250);

            using (SolidBrush background = new SolidBrush(rowColor))
                e.Graphics.FillRectangle(background, e.Bounds);

            Rectangle checkBounds = new Rectangle(e.Bounds.Left + 12, e.Bounds.Top + 9, 16, 16);
            ControlPaint.DrawCheckBox(e.Graphics, checkBounds,
                isChecked ? ButtonState.Checked : ButtonState.Normal);

            using (SolidBrush nameBrush = new SolidBrush(navy))
            using (SolidBrush priceBrush = new SolidBrush(Color.FromArgb(90, 105, 115)))
            using (StringFormat rightAligned = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center })
            using (StringFormat leftAligned = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
            using (Pen divider = new Pen(Color.FromArgb(232, 238, 241)))
            {
                Rectangle nameBounds = new Rectangle(e.Bounds.Left + 40, e.Bounds.Top, e.Bounds.Width - 160, e.Bounds.Height);
                Rectangle priceBounds = new Rectangle(e.Bounds.Right - 105, e.Bounds.Top, 88, e.Bounds.Height);
                e.Graphics.DrawString(service.Name, clbServices.Font, nameBrush, nameBounds, leftAligned);
                e.Graphics.DrawString(service.Price.ToString("N2"), clbServices.Font, priceBrush, priceBounds, rightAligned);
                e.Graphics.DrawLine(divider, e.Bounds.Left + 10, e.Bounds.Bottom - 1, e.Bounds.Right - 10, e.Bounds.Bottom - 1);
            }

            if (isChecked)
            {
                using (Pen selectedMark = new Pen(teal, 2F))
                    e.Graphics.DrawLine(selectedMark, e.Bounds.Left + 3, e.Bounds.Top + 7, e.Bounds.Left + 3, e.Bounds.Bottom - 7);
            }
            e.DrawFocusRectangle();
        }

        private void AddField(string label, TextBox input, int y)
        {
            Controls.Add(new Label { Text = label, AutoSize = true, Location = new Point(40, y + 5), ForeColor = Color.FromArgb(48, 62, 72) });
            input.Location = new Point(220, y);
            input.Size = new Size(320, 30);
            input.MaxLength = label == "Address" ? 250 : (label.StartsWith("Email", StringComparison.Ordinal) ? 254 : 100);
            input.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(input);
        }

        private void LoadVehicleTypes()
        {
            dbConnect db = new dbConnect();
            try
            {
                db.open();
                using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT id,name FROM tbVehicleType ORDER BY name", db.connect()))
                {
                    DataTable vehicleTypes = new DataTable();
                    adapter.Fill(vehicleTypes);
                    cbVehicleType.DataSource = vehicleTypes;
                    cbVehicleType.DisplayMember = "name";
                    cbVehicleType.ValueMember = "id";
                    if (vehicleTypes.Rows.Count == 0)
                    {
                        lblStatus.Text = "Vehicle types haven't been set up yet. Please ask staff for help.";
                        cbVehicleType.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Couldn't load vehicle types: " + ex.Message;
                cbVehicleType.Enabled = false;
            }
            finally
            {
                db.close();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string carNumber = txtCarNumber.Text.Trim().ToUpperInvariant();
            string carModel = txtCarModel.Text.Trim();
            string address = txtAddress.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phoneDigits = new string(phone.Where(Char.IsDigit).ToArray());

            if (name.Length < 2 || phoneDigits.Length < 7 || phoneDigits.Length > 15 || carNumber.Length == 0 || carModel.Length == 0 || address.Length == 0 || cbVehicleType.SelectedValue == null || clbServices.CheckedItems.Count == 0)
            {
                MessageBox.Show("Enter your name, a valid phone number, car details, address, vehicle type, and select at least one wash service.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (email.Length > 0)
            {
                try
                {
                    MailAddress parsed = new MailAddress(email);
                    if (email.Length > 254 || !String.Equals(parsed.Address, email, StringComparison.OrdinalIgnoreCase))
                        throw new FormatException();
                }
                catch (FormatException)
                {
                    MessageBox.Show("Enter a valid email address, or leave the field empty.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return;
                }
            }

            dbConnect db = new dbConnect();
            SqlTransaction transaction = null;
            try
            {
                db.open();
                transaction = db.connect().BeginTransaction(IsolationLevel.Serializable);
                using (SqlCommand duplicate = new SqlCommand("SELECT TOP 1 name FROM tbCustomer WITH (UPDLOCK, HOLDLOCK) WHERE carno = @carno", db.connect(), transaction))
                {
                    duplicate.Parameters.AddWithValue("@carno", carNumber);
                    object existingName = duplicate.ExecuteScalar();
                    if (existingName != null)
                    {
                        transaction.Rollback();
                        MessageBox.Show("This car is already registered under " + existingName + ". Please go to the cashier to continue.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        RegisteredName = existingName.ToString();
                        RegisteredCarNumber = carNumber;
                        DialogResult = DialogResult.OK;
                        Close();
                        return;
                    }
                }

                int vehicleTypeId = Convert.ToInt32(cbVehicleType.SelectedValue);
                ServiceChoice[] selectedServices = clbServices.CheckedItems.Cast<ServiceChoice>().ToArray();
                using (SqlCommand insert = new SqlCommand("INSERT INTO tbCustomer(vid,name,phone,carno,carmodel,address,points,email) VALUES(@vid,@name,@phone,@carno,@carmodel,@address,0,@email); SELECT CAST(SCOPE_IDENTITY() AS int);", db.connect(), transaction))
                {
                    insert.Parameters.AddWithValue("@vid", vehicleTypeId);
                    insert.Parameters.AddWithValue("@name", name);
                    insert.Parameters.AddWithValue("@phone", phone);
                    insert.Parameters.AddWithValue("@carno", carNumber);
                    insert.Parameters.AddWithValue("@carmodel", carModel);
                    insert.Parameters.AddWithValue("@address", address);
                    insert.Parameters.Add("@email", SqlDbType.NVarChar, 254).Value = String.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email;
                    RegisteredCustomerId = Convert.ToInt32(insert.ExecuteScalar());
                }

                string transactionNumber = DateTime.Now.ToString("yyyyMMdd");
                using (SqlCommand nextNumber = new SqlCommand("SELECT ISNULL(MAX(TRY_CONVERT(int, RIGHT(transno, 4))), 1000) + 1 FROM tbCash WITH (UPDLOCK, HOLDLOCK) WHERE transno LIKE @prefix", db.connect(), transaction))
                {
                    nextNumber.Parameters.Add("@prefix", SqlDbType.NVarChar, 30).Value = transactionNumber + "%";
                    transactionNumber += Convert.ToInt32(nextNumber.ExecuteScalar()).ToString("D4");
                }
                foreach (ServiceChoice service in selectedServices)
                {
                    using (SqlCommand addRequestedService = new SqlCommand(
                        "INSERT INTO tbCash(transno,cid,sid,vid,price,date,status) SELECT @transno,@cid,s.id,@vid,s.price,GETDATE(),@status FROM tbService s WHERE s.id=@sid",
                        db.connect(), transaction))
                    {
                        addRequestedService.Parameters.Add("@transno", SqlDbType.NVarChar, 30).Value = transactionNumber;
                        addRequestedService.Parameters.Add("@cid", SqlDbType.Int).Value = RegisteredCustomerId;
                        addRequestedService.Parameters.Add("@sid", SqlDbType.Int).Value = service.Id;
                        addRequestedService.Parameters.Add("@vid", SqlDbType.Int).Value = vehicleTypeId;
                        addRequestedService.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = "Pending";
                        if (addRequestedService.ExecuteNonQuery() != 1)
                            throw new InvalidOperationException("One of the selected wash services is no longer available.");
                    }
                }

                transaction.Commit();
                RegisteredName = name;
                RegisteredCarNumber = carNumber;
                RegisteredTransactionNumber = transactionNumber;
                RegisteredVehicleTypeId = vehicleTypeId;
                MessageBox.Show("Your details and " + selectedServices.Length + " selected service(s) are saved. Please visit the cashier to review the order and pay.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    try { transaction.Rollback(); } catch { }
                }
                MessageBox.Show("Couldn't save your details. Please ask staff for help.\n\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (transaction != null) transaction.Dispose();
                db.close();
            }
        }

        private sealed class ServiceChoice
        {
            public int Id { get; private set; }
            public string Name { get; private set; }
            public decimal Price { get; private set; }

            public ServiceChoice(int id, string name, decimal price)
            {
                Id = id;
                Name = name;
                Price = price;
            }

            public override string ToString()
            {
                return Name + " — " + Price.ToString("N2");
            }
        }
    }
}
