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
    public partial class Splash : Form
    {
        public Splash()
        {
            InitializeComponent();
            BackColor = Color.FromArgb(26, 43, 64);
            label1.Text = "CAR WASH MANAGEMENT SYSTEM";
            label1.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(205, 218, 228);
            label1.Location = new Point(235, 78);
            label2.Text = "Shine";
            label2.ForeColor = Color.White;
            label2.Location = new Point(250, 108);
            label3.Text = "Wash";
            label3.ForeColor = Color.FromArgb(65, 199, 194);
            label3.Location = new Point(357, 108);
            label4.ForeColor = Color.FromArgb(205, 218, 228);
        }

        int start = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            start += 2;
            progressBar1.Value = start;
            if(progressBar1.Value==100)
            {
                progressBar1.Value = 0;
                timer1.Stop();
                Login login = new Login();
                this.Hide();
                login.ShowDialog();
            }
        }

        private void Splash_Load(object sender, EventArgs e)
        {
            timer1.Start();
        }
    }
}
