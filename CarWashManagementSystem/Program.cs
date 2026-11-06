using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CarWashManagementSystem
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                DatabaseInitializer.EnsureWashWorkflowColumns();
                DatabaseInitializer.EnsureCustomerEmailColumn();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("The database could not be prepared for the wash workflow.\n\n" + ex.Message,
                    "Car Wash Management System", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Application.Idle += UiTheme.StyleOpenForms;
            Application.Run(new Splash());
        }
    }
}
