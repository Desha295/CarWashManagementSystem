using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CarWashManagementSystem
{
    public sealed class Registration : Form
    {
        private readonly TextBox txtName = new TextBox();
        private readonly TextBox txtPhone = new TextBox();
        private readonly TextBox txtAddress = new TextBox();
        private readonly DateTimePicker dtBirthDate = new DateTimePicker();
        private readonly ComboBox cbGender = new ComboBox();
        private readonly TextBox txtPassword = new TextBox();
        private readonly TextBox txtConfirmPassword = new TextBox();
        private readonly Label lblBootstrapInfo = new Label();

        public string RegisteredName { get; private set; }

        public Registration()
        {
            Text = "Create Account | Car Wash Management System";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 610);
            BackColor = Color.FromArgb(245, 248, 250);
            Font = new Font("Segoe UI", 10F);

            Panel header = new Panel { Dock = DockStyle.Top, Height = 110, BackColor = Color.FromArgb(17, 145, 170) };
            Label title = new Label { Text = "Create your account", AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 21F, FontStyle.Bold), Location = new Point(34, 22) };
            Label subtitle = new Label { Text = "Join the Car Wash team to manage daily operations.", AutoSize = true, ForeColor = Color.FromArgb(224, 245, 248), Location = new Point(38, 68) };
            header.Controls.Add(title);
            header.Controls.Add(subtitle);
            Controls.Add(header);

            int labelX = 40, inputX = 205, rowY = 136, rowGap = 52, inputWidth = 270;
            AddField("Full name", txtName, labelX, inputX, rowY, inputWidth);
            AddField("Phone", txtPhone, labelX, inputX, rowY + rowGap, inputWidth);
            AddField("Address", txtAddress, labelX, inputX, rowY + rowGap * 2, inputWidth);

            AddLabel("Date of birth", labelX, rowY + rowGap * 3 + 5);
            dtBirthDate.Location = new Point(inputX, rowY + rowGap * 3);
            dtBirthDate.Size = new Size(inputWidth, 30);
            dtBirthDate.Format = DateTimePickerFormat.Short;
            dtBirthDate.MaxDate = DateTime.Today;
            dtBirthDate.Value = DateTime.Today.AddYears(-18);
            Controls.Add(dtBirthDate);

            AddLabel("Gender", labelX, rowY + rowGap * 4 + 5);
            cbGender.Location = new Point(inputX, rowY + rowGap * 4);
            cbGender.Size = new Size(inputWidth, 30);
            cbGender.DropDownStyle = ComboBoxStyle.DropDownList;
            cbGender.Items.AddRange(new object[] { "Male", "Female" });
            cbGender.SelectedIndex = 0;
            Controls.Add(cbGender);

            AddField("Password", txtPassword, labelX, inputX, rowY + rowGap * 5, inputWidth);
            txtPassword.UseSystemPasswordChar = true;
            AddField("Confirm password", txtConfirmPassword, labelX, inputX, rowY + rowGap * 6, inputWidth);
            txtConfirmPassword.UseSystemPasswordChar = true;

            lblBootstrapInfo.Text = "If there is no Manager yet, the first account becomes Manager. Later sign-ups start as Workers.";
            lblBootstrapInfo.ForeColor = Color.FromArgb(90, 105, 115);
            lblBootstrapInfo.Location = new Point(40, 503);
            lblBootstrapInfo.Size = new Size(435, 38);
            Controls.Add(lblBootstrapInfo);

            Button btnCreate = new Button
            {
                Text = "Create account",
                Location = new Point(205, 552),
                Size = new Size(145, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(17, 145, 170),
                ForeColor = Color.White
            };
            btnCreate.FlatAppearance.BorderSize = 0;
            btnCreate.Click += btnCreate_Click;
            Controls.Add(btnCreate);

            Button btnBack = new Button
            {
                Text = "Back",
                Location = new Point(365, 552),
                Size = new Size(110, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(60, 75, 85)
            };
            btnBack.Click += (sender, e) => Close();
            Controls.Add(btnBack);
            AcceptButton = btnCreate;
            CancelButton = btnBack;
            RefreshBootstrapInfo();
        }

        private void RefreshBootstrapInfo()
        {
            dbConnect db = new dbConnect();
            try
            {
                db.open();
                using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM tbEmployer WHERE role = 'Manager'", db.connect()))
                    lblBootstrapInfo.Visible = Convert.ToInt32(command.ExecuteScalar()) == 0;
            }
            catch
            {
                lblBootstrapInfo.Visible = false;
            }
            finally
            {
                db.close();
            }
        }

        private void AddField(string label, TextBox input, int labelX, int inputX, int y, int width)
        {
            AddLabel(label, labelX, y + 5);
            input.Location = new Point(inputX, y);
            input.Size = new Size(width, 30);
            input.MaxLength = label == "Password" || label == "Confirm password" ? 128 : 100;
            Controls.Add(input);
        }

        private void AddLabel(string text, int x, int y)
        {
            Controls.Add(new Label { Text = text, AutoSize = true, Location = new Point(x, y), ForeColor = Color.FromArgb(48, 62, 72) });
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string address = txtAddress.Text.Trim();

            if (name.Length < 2 || phone.Length == 0 || address.Length == 0)
            {
                MessageBox.Show("Enter your name, phone number, and address.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (GetAge(dtBirthDate.Value) < 18)
            {
                MessageBox.Show("You must be at least 18 years old to create an account.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (txtPassword.Text.Length < 6)
            {
                MessageBox.Show("Choose a password with at least 6 characters.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!String.Equals(txtPassword.Text, txtConfirmPassword.Text, StringComparison.Ordinal))
            {
                MessageBox.Show("The passwords do not match.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPassword.Focus();
                return;
            }

            dbConnect db = new dbConnect();
            SqlTransaction transaction = null;
            string assignedRole = "Worker";
            try
            {
                db.open();
                transaction = db.connect().BeginTransaction(IsolationLevel.Serializable);
                using (SqlCommand check = new SqlCommand("SELECT COUNT(*) FROM tbEmployer WHERE name = @name", db.connect(), transaction))
                {
                    check.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = name;
                    if (Convert.ToInt32(check.ExecuteScalar()) > 0)
                    {
                        MessageBox.Show("An account with this name already exists. Please choose another name.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                using (SqlCommand managerCount = new SqlCommand("SELECT COUNT(*) FROM tbEmployer WITH (UPDLOCK, HOLDLOCK) WHERE role = 'Manager'", db.connect(), transaction))
                    assignedRole = Convert.ToInt32(managerCount.ExecuteScalar()) == 0 ? "Manager" : "Worker";

                using (SqlCommand insert = new SqlCommand("INSERT INTO tbEmployer(name, phone, address, dob, gender, role, salary, password) VALUES(@name, @phone, @address, @dob, @gender, @role, @salary, @password)", db.connect(), transaction))
                {
                    insert.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = name;
                    insert.Parameters.Add("@phone", SqlDbType.NVarChar, 50).Value = phone;
                    insert.Parameters.Add("@address", SqlDbType.NVarChar, 250).Value = address;
                    insert.Parameters.Add("@dob", SqlDbType.DateTime).Value = dtBirthDate.Value.Date;
                    insert.Parameters.Add("@gender", SqlDbType.NVarChar, 20).Value = cbGender.SelectedItem.ToString();
                    insert.Parameters.Add("@role", SqlDbType.NVarChar, 50).Value = assignedRole;
                    insert.Parameters.Add("@salary", SqlDbType.Decimal).Value = 0m;
                    insert.Parameters["@salary"].Precision = 18;
                    insert.Parameters["@salary"].Scale = 2;
                    insert.Parameters.Add("@password", SqlDbType.NVarChar, 128).Value = txtPassword.Text;
                    insert.ExecuteNonQuery();
                }
                transaction.Commit();
                lblBootstrapInfo.Visible = false;

                RegisteredName = name;
                string roleMessage = assignedRole == "Manager"
                    ? "You are the first account, so you have been assigned the Manager role. You can add Supervisors and Workers from Staff Management."
                    : "Your Worker account was created. A Manager can add Supervisor and Manager accounts from Staff Management.";
                MessageBox.Show(roleMessage + "\n\nYou can now sign in.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    try { transaction.Rollback(); } catch { }
                }
                MessageBox.Show("Couldn't create the account. Check the database connection.\n\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (transaction != null) transaction.Dispose();
                db.close();
            }
        }

        private static int GetAge(DateTime dateOfBirth)
        {
            int age = DateTime.Today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
            return age;
        }
    }
}
